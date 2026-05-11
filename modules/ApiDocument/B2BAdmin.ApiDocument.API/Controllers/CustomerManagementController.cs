using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using B2BAdmin.ApiDocument.Domains.Models;
using B2BAdmin.ApiDocument.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Globalization;

namespace B2BAdmin.ApiDocument.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerManagementController : ControllerBase
    {
        private readonly ApiDocumentDbContext _apiDocumentDbContext;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public CustomerManagementController(ApiDocumentDbContext apiDocumentDbContext, IHostingEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _apiDocumentDbContext = apiDocumentDbContext;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomersAsync(
            [FromQuery] string search = null,
            [FromQuery] string status = null,
            [FromQuery] string riskLevel = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "updatedAt",
            [FromQuery] string sortDirection = "desc")
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

            var filterBuilder = Builders<CustomerAccount>.Filter;
            var filters = new List<FilterDefinition<CustomerAccount>>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                filters.Add(filterBuilder.Or(
                    filterBuilder.Regex(x => x.name, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
                    filterBuilder.Regex(x => x.code, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
                    filterBuilder.Regex(x => x.taxCode, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
                    filterBuilder.Regex(x => x.phone, new MongoDB.Bson.BsonRegularExpression(keyword, "i"))
                ));
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "all")
            {
                filters.Add(filterBuilder.Eq(x => x.status, status));
            }

            if (!string.IsNullOrWhiteSpace(riskLevel) && riskLevel != "all")
            {
                filters.Add(filterBuilder.Eq(x => x.riskLevel, riskLevel));
            }

            // Exclude both current and legacy soft-delete flags.
            filters.Add(filterBuilder.Eq(x => x.isDeleted, false));
            filters.Add(filterBuilder.Ne("isDelete", true));

            var finalFilter = filters.Count > 0 ? filterBuilder.And(filters) : filterBuilder.Empty;

            var sortDefinition = BuildSort(sortBy, sortDirection);
            var totalItems = await _apiDocumentDbContext.CustomerAccounts.CountDocumentsAsync(finalFilter);

            var items = await _apiDocumentDbContext.CustomerAccounts
                .Find(finalFilter)
                .Sort(sortDefinition)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .Project(x => new CustomerAccount
                {
                    Id = x.Id,
                    code = x.code,
                    name = x.name,
                    category = x.category,
                    taxCode = x.taxCode,
                    bankAccount = x.bankAccount,
                    bankName = x.bankName,
                    phone = x.phone,
                    email = x.email,
                    address = x.address,
                    debtAmount = x.debtAmount,
                    creditAmount = x.creditAmount,
                    status = x.status,
                    riskLevel = x.riskLevel,
                    owner = x.owner,
                    tags = x.tags,
                    lastTransactionAt = x.lastTransactionAt,
                    updatedAt = x.updatedAt,
                    createdAt = x.createdAt,
                    isDeleted = x.isDeleted,
                })
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalItems,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                items
            });
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummaryAsync()
        {
            var filterBuilder = Builders<CustomerAccount>.Filter;
            var baseFilter = filterBuilder.And(
                filterBuilder.Eq(x => x.isDeleted, false),
                filterBuilder.Ne("isDelete", true));

            // Run all counts in parallel instead of loading every document to RAM
            var totalTask   = _apiDocumentDbContext.CustomerAccounts.CountDocumentsAsync(baseFilter);
            var activeTask  = _apiDocumentDbContext.CustomerAccounts.CountDocumentsAsync(
                filterBuilder.And(baseFilter, filterBuilder.Eq(x => x.status, "active")));
            var warningTask = _apiDocumentDbContext.CustomerAccounts.CountDocumentsAsync(
                filterBuilder.And(baseFilter, filterBuilder.Eq(x => x.riskLevel, "warning")));
            var blockedTask = _apiDocumentDbContext.CustomerAccounts.CountDocumentsAsync(
                filterBuilder.And(baseFilter, filterBuilder.Eq(x => x.status, "blocked")));

            var sumsTask = _apiDocumentDbContext.CustomerAccounts
                .Aggregate()
                .Match(baseFilter)
                .AppendStage<BsonDocument>(new BsonDocument("$group", new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "totalDebt",   new BsonDocument("$sum", "$debtAmount") },
                    { "totalCredit", new BsonDocument("$sum", "$creditAmount") },
                }))
                .FirstOrDefaultAsync();

            await Task.WhenAll(totalTask, activeTask, warningTask, blockedTask, sumsTask);

            var sums = sumsTask.Result;
            decimal ParseBsonDecimal(BsonDocument doc, string field)
            {
                if (doc == null || !doc.TryGetValue(field, out var v) || v.IsBsonNull) return 0m;
                return v.BsonType == BsonType.Decimal128 ? (decimal)v.AsDecimal128 : Convert.ToDecimal(v.ToDouble());
            }

            return Ok(new
            {
                totalCustomers = totalTask.Result,
                activeCustomers = activeTask.Result,
                warningCustomers = warningTask.Result,
                blockedCustomers = blockedTask.Result,
                totalDebt  = ParseBsonDecimal(sums, "totalDebt"),
                totalCredit = ParseBsonDecimal(sums, "totalCredit"),
            });
        }

        [HttpGet("reports/customer-debt-summary")]
        public async Task<IActionResult> GetCustomerDebtReportSummaryAsync(
            [FromQuery] string status = null,
            [FromQuery] string riskLevel = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            // ---- Build filter (mirrors customer-debt-details pattern) ----
            var filterBuilder = Builders<CustomerAccount>.Filter;
            var filters = new List<FilterDefinition<CustomerAccount>>
            {
                filterBuilder.Eq(x => x.isDeleted, false),
                filterBuilder.Ne("isDelete", true),
            };

            if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
                filters.Add(filterBuilder.Eq(x => x.status, status));

            if (!string.IsNullOrWhiteSpace(riskLevel) && !string.Equals(riskLevel, "all", StringComparison.OrdinalIgnoreCase))
                filters.Add(filterBuilder.Eq(x => x.riskLevel, riskLevel));

            var normalizedFromDate = fromDate?.Date;
            var normalizedToDate = toDate?.Date.AddDays(1).AddTicks(-1);

            if (normalizedFromDate.HasValue || normalizedToDate.HasValue)
            {
                var txFilterBuilder = Builders<CustomerDebtTransaction>.Filter;
                var txFilters = new List<FilterDefinition<CustomerDebtTransaction>>
                {
                    txFilterBuilder.Eq(x => x.isDeleted, false),
                };
                if (normalizedFromDate.HasValue)
                    txFilters.Add(txFilterBuilder.Gte(x => x.transactionAt, normalizedFromDate.Value));
                if (normalizedToDate.HasValue)
                    txFilters.Add(txFilterBuilder.Lte(x => x.transactionAt, normalizedToDate.Value));

                var matchingIds = await _apiDocumentDbContext.CustomerDebtTransactions
                    .Distinct<string>("customerId", txFilterBuilder.And(txFilters))
                    .ToListAsync();

                var distinctIds = matchingIds
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                if (distinctIds.Count == 0)
                {
                    return Ok(new
                    {
                        generatedAt = DateTime.UtcNow,
                        filters = new
                        {
                            status = string.IsNullOrWhiteSpace(status) ? "all" : status.Trim(),
                            riskLevel = string.IsNullOrWhiteSpace(riskLevel) ? "all" : riskLevel.Trim(),
                            fromDate = fromDate?.Date.ToString("yyyy-MM-dd"),
                            toDate = toDate?.Date.ToString("yyyy-MM-dd"),
                        },
                        customerSummary = new
                        {
                            totalCustomers = 0, activeCustomers = 0, warningCustomers = 0,
                            blockedCustomers = 0, totalDebt = 0m, totalCredit = 0m,
                        },
                        debtOverview = new
                        {
                            totalReceivable = 0m, totalPayable = 0m, netExposure = 0m,
                            highRiskExposure = 0m, customerCount = 0, debtorCount = 0, creditorCount = 0,
                            agingBuckets = Array.Empty<object>(), topDebtors = Array.Empty<object>(),
                        },
                    });
                }

                filters.Add(filterBuilder.In(x => x.Id, distinctIds));
            }

            var finalFilter = filterBuilder.And(filters);

            // ---- MongoDB aggregation: $addFields → $facet (single server-side pass) ----
            var nowMs = new BsonDateTime(DateTime.UtcNow);

            // Compute netBalance and agingDays on each document
            var addFieldsStage = new BsonDocument("$addFields", new BsonDocument
            {
                {
                    "netBalance", new BsonDocument("$subtract",
                        new BsonArray { "$debtAmount", "$creditAmount" })
                },
                {
                    "agingDays", new BsonDocument("$cond", new BsonDocument
                    {
                        { "if",   new BsonDocument("$not", new BsonArray { "$lastTransactionAt" }) },
                        { "then", 999 },
                        { "else", new BsonDocument("$toInt",
                            new BsonDocument("$divide", new BsonArray
                            {
                                new BsonDocument("$subtract", new BsonArray { nowMs, "$lastTransactionAt" }),
                                86400000.0
                            })) },
                    })
                },
            });

            // Aging bucket expression: matches C# GetAgingBucket()
            var agingBucketExpr = new BsonDocument("$switch", new BsonDocument
            {
                {
                    "branches", new BsonArray
                    {
                        new BsonDocument { { "case", new BsonDocument("$lte", new BsonArray { "$agingDays", 30  }) }, { "then", "0-30"  } },
                        new BsonDocument { { "case", new BsonDocument("$lte", new BsonArray { "$agingDays", 60  }) }, { "then", "31-60" } },
                        new BsonDocument { { "case", new BsonDocument("$lte", new BsonArray { "$agingDays", 90  }) }, { "then", "61-90" } },
                    }
                },
                { "default", ">90" },
            });

            var facetStage = new BsonDocument("$facet", new BsonDocument
            {
                {
                    "overview", new BsonArray
                    {
                        new BsonDocument("$group", new BsonDocument
                        {
                            { "_id", BsonNull.Value },
                            { "totalCustomers",  new BsonDocument("$sum", 1) },
                            { "activeCustomers",  new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray { new BsonDocument("$eq",  new BsonArray { "$status",    "active"   }), 1, 0 })) },
                            { "warningCustomers", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray { new BsonDocument("$eq",  new BsonArray { "$riskLevel", "warning"  }), 1, 0 })) },
                            { "blockedCustomers", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray { new BsonDocument("$eq",  new BsonArray { "$status",    "blocked"  }), 1, 0 })) },
                            { "totalDebt",        new BsonDocument("$sum", "$debtAmount") },
                            { "totalCredit",      new BsonDocument("$sum", "$creditAmount") },
                            { "totalReceivable",  new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray { new BsonDocument("$gt", new BsonArray { "$netBalance", 0 }), "$netBalance", 0 })) },
                            { "totalPayable",     new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray { new BsonDocument("$lt", new BsonArray { "$netBalance", 0 }), new BsonDocument("$abs", "$netBalance"), 0 })) },
                            {
                                "highRiskExposure", new BsonDocument("$sum",
                                    new BsonDocument("$cond", new BsonArray
                                    {
                                        new BsonDocument("$and", new BsonArray
                                        {
                                            new BsonDocument("$gt", new BsonArray { "$netBalance", 0 }),
                                            new BsonDocument("$in", new BsonArray { "$riskLevel", new BsonArray { "warning", "critical" } }),
                                        }),
                                        "$netBalance",
                                        0,
                                    }))
                            },
                            { "debtorCount",   new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray { new BsonDocument("$gt", new BsonArray { "$netBalance", 0 }), 1, 0 })) },
                            { "creditorCount", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray { new BsonDocument("$lt", new BsonArray { "$netBalance", 0 }), 1, 0 })) },
                        }),
                    }
                },
                {
                    "agingBuckets", new BsonArray
                    {
                        new BsonDocument("$match",     new BsonDocument("netBalance", new BsonDocument("$gt", 0))),
                        new BsonDocument("$addFields", new BsonDocument("agingBucket", agingBucketExpr)),
                        new BsonDocument("$group",     new BsonDocument
                        {
                            { "_id",       "$agingBucket" },
                            { "amount",    new BsonDocument("$sum", "$netBalance") },
                            { "customers", new BsonDocument("$sum", 1) },
                        }),
                        new BsonDocument("$project", new BsonDocument
                        {
                            { "_id", 0 }, { "bucket", "$_id" }, { "amount", 1 }, { "customers", 1 },
                        }),
                    }
                },
                {
                    "topDebtors", new BsonArray
                    {
                        new BsonDocument("$match",   new BsonDocument("netBalance", new BsonDocument("$gt", 0))),
                        new BsonDocument("$sort",    new BsonDocument { { "netBalance", -1 }, { "agingDays", -1 } }),
                        new BsonDocument("$limit",   5),
                        new BsonDocument("$project", new BsonDocument
                        {
                            { "id", "$_id" }, { "_id", 0 },
                            { "code", 1 }, { "name", 1 }, { "status", 1 }, { "riskLevel", 1 },
                            { "agingDays", 1 }, { "lastTransactionAt", 1 }, { "netBalance", 1 },
                        }),
                    }
                },
            });

            var aggResult = await _apiDocumentDbContext.CustomerAccounts
                .Aggregate()
                .Match(finalFilter)
                .AppendStage<BsonDocument>(addFieldsStage)
                .AppendStage<BsonDocument>(facetStage)
                .FirstOrDefaultAsync();

            // ---- Parse BsonDocument result ----
            static int GetInt(BsonDocument doc, string field)
            {
                if (doc == null || !doc.TryGetValue(field, out var v) || v.IsBsonNull) return 0;
                return v.ToInt32();
            }
            static decimal GetDecimal(BsonDocument doc, string field)
            {
                if (doc == null || !doc.TryGetValue(field, out var v) || v.IsBsonNull) return 0m;
                return v.BsonType == BsonType.Decimal128 ? (decimal)v.AsDecimal128 : Convert.ToDecimal(v.ToDouble());
            }

            var overviewArr = aggResult?["overview"]?.AsBsonArray;
            var ov = overviewArr?.Count > 0 ? overviewArr[0].AsBsonDocument : null;

            var totalCustomers  = GetInt(ov, "totalCustomers");
            var activeCustomers = GetInt(ov, "activeCustomers");
            var warningCustomers = GetInt(ov, "warningCustomers");
            var blockedCustomers = GetInt(ov, "blockedCustomers");
            var totalDebt        = GetDecimal(ov, "totalDebt");
            var totalCredit      = GetDecimal(ov, "totalCredit");
            var totalReceivable  = GetDecimal(ov, "totalReceivable");
            var totalPayable     = GetDecimal(ov, "totalPayable");
            var highRiskExposure = GetDecimal(ov, "highRiskExposure");
            var debtorCount      = GetInt(ov, "debtorCount");
            var creditorCount    = GetInt(ov, "creditorCount");
            var netExposure      = totalReceivable - totalPayable;

            var agingBucketsArr = aggResult?["agingBuckets"]?.AsBsonArray ?? new BsonArray();
            var agingBuckets = agingBucketsArr.Select(b =>
            {
                var bd = b.AsBsonDocument;
                return (object)new
                {
                    bucket    = bd.GetValue("bucket", BsonNull.Value).IsBsonNull ? null : bd["bucket"].AsString,
                    amount    = GetDecimal(bd, "amount"),
                    customers = GetInt(bd, "customers"),
                };
            }).ToList();

            var topDebtorsArr = aggResult?["topDebtors"]?.AsBsonArray ?? new BsonArray();
            var topDebtors = topDebtorsArr.Select(b =>
            {
                var bd = b.AsBsonDocument;
                return (object)new
                {
                    id        = bd.GetValue("id",        BsonNull.Value).IsBsonNull ? null : bd["id"].AsString,
                    code      = bd.GetValue("code",      BsonNull.Value).IsBsonNull ? null : bd["code"].AsString,
                    name      = bd.GetValue("name",      BsonNull.Value).IsBsonNull ? null : bd["name"].AsString,
                    status    = bd.GetValue("status",    BsonNull.Value).IsBsonNull ? null : bd["status"].AsString,
                    riskLevel = bd.GetValue("riskLevel", BsonNull.Value).IsBsonNull ? null : bd["riskLevel"].AsString,
                    agingDays = GetInt(bd, "agingDays"),
                    lastTransactionAt = bd.TryGetValue("lastTransactionAt", out var lat) && !lat.IsBsonNull
                        ? (DateTime?)lat.ToUniversalTime() : null,
                    netBalance = GetDecimal(bd, "netBalance"),
                };
            }).ToList();

            return Ok(new
            {
                generatedAt = DateTime.UtcNow,
                filters = new
                {
                    status    = string.IsNullOrWhiteSpace(status)    ? "all" : status.Trim(),
                    riskLevel = string.IsNullOrWhiteSpace(riskLevel) ? "all" : riskLevel.Trim(),
                    fromDate  = fromDate?.Date.ToString("yyyy-MM-dd"),
                    toDate    = toDate?.Date.ToString("yyyy-MM-dd"),
                },
                customerSummary = new
                {
                    totalCustomers, activeCustomers, warningCustomers, blockedCustomers,
                    totalDebt, totalCredit,
                },
                debtOverview = new
                {
                    totalReceivable, totalPayable, netExposure, highRiskExposure,
                    customerCount = totalCustomers, debtorCount, creditorCount,
                    agingBuckets, topDebtors,
                },
            });
        }

        [HttpGet("reports/customer-debt-details")]
        public async Task<IActionResult> GetCustomerDebtReportDetailsAsync(
            [FromQuery] string search = null,
            [FromQuery] string status = null,
            [FromQuery] string riskLevel = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "netBalance",
            [FromQuery] string sortDirection = "desc")
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

            var filterBuilder = Builders<CustomerAccount>.Filter;
            var filters = new List<FilterDefinition<CustomerAccount>>
            {
                filterBuilder.Eq(x => x.isDeleted, false),
                filterBuilder.Ne("isDelete", true),
            };

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = Regex.Escape(search.Trim());
                filters.Add(filterBuilder.Or(
                    filterBuilder.Regex(x => x.name, new BsonRegularExpression(keyword, "i")),
                    filterBuilder.Regex(x => x.code, new BsonRegularExpression(keyword, "i")),
                    filterBuilder.Regex(x => x.taxCode, new BsonRegularExpression(keyword, "i")),
                    filterBuilder.Regex(x => x.phone, new BsonRegularExpression(keyword, "i"))
                ));
            }

            if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
            {
                filters.Add(filterBuilder.Eq(x => x.status, status));
            }

            if (!string.IsNullOrWhiteSpace(riskLevel) && !string.Equals(riskLevel, "all", StringComparison.OrdinalIgnoreCase))
            {
                filters.Add(filterBuilder.Eq(x => x.riskLevel, riskLevel));
            }

            var normalizedFromDate = fromDate?.Date;
            var normalizedToDate = toDate?.Date.AddDays(1).AddTicks(-1);
            if (normalizedFromDate.HasValue || normalizedToDate.HasValue)
            {
                var transactionFilterBuilder = Builders<CustomerDebtTransaction>.Filter;
                var transactionFilters = new List<FilterDefinition<CustomerDebtTransaction>>
                {
                    transactionFilterBuilder.Eq(x => x.isDeleted, false)
                };

                if (normalizedFromDate.HasValue)
                {
                    transactionFilters.Add(transactionFilterBuilder.Gte(x => x.transactionAt, normalizedFromDate.Value));
                }

                if (normalizedToDate.HasValue)
                {
                    transactionFilters.Add(transactionFilterBuilder.Lte(x => x.transactionAt, normalizedToDate.Value));
                }

                var matchingCustomerIds = await _apiDocumentDbContext.CustomerDebtTransactions
                    .Distinct<string>("customerId", transactionFilterBuilder.And(transactionFilters))
                    .ToListAsync();

                var distinctCustomerIds = matchingCustomerIds
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                if (distinctCustomerIds.Count == 0)
                {
                    return Ok(new
                    {
                        page,
                        pageSize,
                        totalItems = 0,
                        totalPages = 0,
                        items = new List<DebtListItemDto>()
                    });
                }

                filters.Add(filterBuilder.In(x => x.Id, distinctCustomerIds));
            }

            var finalFilter = filterBuilder.And(filters);
            var totalItems = await _apiDocumentDbContext.CustomerAccounts.CountDocumentsAsync(finalFilter);

            if (totalItems == 0)
            {
                return Ok(new
                {
                    page,
                    pageSize,
                    totalItems = 0,
                    totalPages = 0,
                    items = new List<DebtListItemDto>()
                });
            }

            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            var normalizedSortBy = (sortBy ?? string.Empty).Trim().ToLowerInvariant();

            var aggregate = _apiDocumentDbContext.CustomerAccounts
                .Aggregate()
                .Match(finalFilter)
                .Project(x => new DebtListItemDto
                {
                    id = x.Id,
                    code = x.code,
                    name = x.name,
                    phone = x.phone,
                    status = x.status,
                    riskLevel = x.riskLevel,
                    debtAmount = x.debtAmount,
                    creditAmount = x.creditAmount,
                    netBalance = x.debtAmount - x.creditAmount,
                    agingDays = 0,
                    agingBucket = null,
                    lastTransactionAt = x.lastTransactionAt,
                    updatedAt = x.updatedAt,
                });

            aggregate = normalizedSortBy switch
            {
                "name" => isDesc ? aggregate.SortByDescending(x => x.name) : aggregate.SortBy(x => x.name),
                "status" => isDesc ? aggregate.SortByDescending(x => x.status) : aggregate.SortBy(x => x.status),
                "risklevel" => isDesc ? aggregate.SortByDescending(x => x.riskLevel) : aggregate.SortBy(x => x.riskLevel),
                "lasttransactionat" => isDesc ? aggregate.SortByDescending(x => x.lastTransactionAt) : aggregate.SortBy(x => x.lastTransactionAt),
                "agingdays" => isDesc ? aggregate.SortBy(x => x.lastTransactionAt) : aggregate.SortByDescending(x => x.lastTransactionAt),
                _ => isDesc ? aggregate.SortByDescending(x => x.netBalance) : aggregate.SortBy(x => x.netBalance),
            };

            var items = await aggregate
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            foreach (var item in items)
            {
                item.agingDays = GetAgingDays(item.lastTransactionAt);
                item.agingBucket = GetAgingBucket(item.agingDays);
            }

            return Ok(new
            {
                page,
                pageSize,
                totalItems,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                items
            });
        }

        [HttpGet("reports/customer-debt-export")]
        public async Task<IActionResult> ExportCustomerDebtReportAsync(
            [FromQuery] string search = null,
            [FromQuery] string status = null,
            [FromQuery] string riskLevel = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string sortBy = "netBalance",
            [FromQuery] string sortDirection = "desc")
        {
            var filteredData = await GetFilteredDebtSourceAsync(search, status, riskLevel, fromDate, toDate);

            var projected = filteredData
                .Select(x =>
                {
                    var agingDays = GetAgingDays(x.lastTransactionAt);
                    var netBalance = GetNetBalance(x);

                    return new DebtListItemDto
                    {
                        id = x.Id,
                        code = x.code,
                        name = x.name,
                        phone = x.phone,
                        status = x.status,
                        riskLevel = x.riskLevel,
                        debtAmount = x.debtAmount,
                        creditAmount = x.creditAmount,
                        netBalance = netBalance,
                        agingDays = agingDays,
                        agingBucket = GetAgingBucket(agingDays),
                        lastTransactionAt = x.lastTransactionAt,
                        updatedAt = x.updatedAt,
                    };
                })
                .ToList();

            var sorted = SortDebtItems(projected, sortBy, sortDirection);
            var fileBytes = BuildCustomerDebtReportExcelFile(sorted, status, riskLevel, fromDate, toDate, search);
            var exportNow = ConvertToDisplayTime(DateTime.UtcNow);
            var fileName = $"customer-debt-report-{exportNow:yyyyMMddHHmmss}.xlsx";
            var actor = GetCurrentActor();
            var exportHistoryId = Guid.NewGuid().ToString("N");
            var exportDirectory = EnsureCustomerDebtReportExportHistoryDirectory(exportNow, out var relativeDirectory);
            var storedFileName = BuildStoredFileName(fileName, exportHistoryId);
            var absolutePath = Path.Combine(exportDirectory, storedFileName);
            await System.IO.File.WriteAllBytesAsync(absolutePath, fileBytes);

            var exportHistory = new CustomerDebtReportExportHistory
            {
                id = exportHistoryId,
                fileName = fileName,
                storedFileName = storedFileName,
                relativePath = BuildCustomerDebtReportExportHistoryRelativePath(relativeDirectory, storedFileName),
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                size = fileBytes.LongLength,
                status = NormalizeText(status),
                riskLevel = NormalizeText(riskLevel),
                search = NormalizeText(search),
                sortBy = NormalizeText(sortBy),
                sortDirection = NormalizeText(sortDirection),
                fromDate = fromDate,
                toDate = toDate,
                recordCount = sorted.Count,
                exportedAt = DateTime.UtcNow,
                exportedBy = actor,
            };
            await _apiDocumentDbContext.CustomerDebtReportExportHistories.InsertOneAsync(exportHistory);

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("reports/customer-debt-export-history")]
        public async Task<IActionResult> GetCustomerDebtReportExportHistoryAsync(
            [FromQuery] string search = null,
            [FromQuery] string exportedBy = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sortBy = "exportedAt",
            [FromQuery] string sortDirection = "desc")
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 200);

            var historyFilterBuilder = Builders<CustomerDebtReportExportHistory>.Filter;
            var historyFilters = new List<FilterDefinition<CustomerDebtReportExportHistory>>
            {
                historyFilterBuilder.Eq(x => x.isDeleted, false),
            };

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = Regex.Escape(search.Trim());
                historyFilters.Add(historyFilterBuilder.Or(
                    historyFilterBuilder.Regex(x => x.fileName, new BsonRegularExpression(keyword, "i")),
                    historyFilterBuilder.Regex(x => x.search, new BsonRegularExpression(keyword, "i")),
                    historyFilterBuilder.Regex(x => x.status, new BsonRegularExpression(keyword, "i")),
                    historyFilterBuilder.Regex(x => x.riskLevel, new BsonRegularExpression(keyword, "i"))));
            }

            if (!string.IsNullOrWhiteSpace(exportedBy))
            {
                historyFilters.Add(historyFilterBuilder.Regex(x => x.exportedBy, new BsonRegularExpression(Regex.Escape(exportedBy.Trim()), "i")));
            }

            if (fromDate.HasValue)
            {
                historyFilters.Add(historyFilterBuilder.Gte(x => x.exportedAt, fromDate.Value.Date));
            }

            if (toDate.HasValue)
            {
                historyFilters.Add(historyFilterBuilder.Lt(x => x.exportedAt, toDate.Value.Date.AddDays(1)));
            }

            var historyFilter = historyFilterBuilder.And(historyFilters);
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            SortDefinition<CustomerDebtReportExportHistory> sortDefinition;
            switch ((sortBy ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "filename":
                    sortDefinition = isDesc
                        ? Builders<CustomerDebtReportExportHistory>.Sort.Descending(x => x.fileName)
                        : Builders<CustomerDebtReportExportHistory>.Sort.Ascending(x => x.fileName);
                    break;
                case "size":
                    sortDefinition = isDesc
                        ? Builders<CustomerDebtReportExportHistory>.Sort.Descending(x => x.size)
                        : Builders<CustomerDebtReportExportHistory>.Sort.Ascending(x => x.size);
                    break;
                case "exportedby":
                    sortDefinition = isDesc
                        ? Builders<CustomerDebtReportExportHistory>.Sort.Descending(x => x.exportedBy)
                        : Builders<CustomerDebtReportExportHistory>.Sort.Ascending(x => x.exportedBy);
                    break;
                default:
                    sortDefinition = isDesc
                        ? Builders<CustomerDebtReportExportHistory>.Sort.Descending(x => x.exportedAt)
                        : Builders<CustomerDebtReportExportHistory>.Sort.Ascending(x => x.exportedAt);
                    break;
            }

            var totalItems = (int)await _apiDocumentDbContext.CustomerDebtReportExportHistories.CountDocumentsAsync(historyFilter);
            var items = await _apiDocumentDbContext.CustomerDebtReportExportHistories
                .Find(historyFilter)
                .Sort(sortDefinition)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalItems,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                items,
            });
        }

        [HttpGet("reports/customer-debt-export-history/{historyId}/download")]
        public async Task<IActionResult> DownloadCustomerDebtReportExportHistoryAsync(string historyId)
        {
            if (string.IsNullOrWhiteSpace(historyId))
            {
                return BadRequest("Invalid historyId");
            }

            var history = await _apiDocumentDbContext.CustomerDebtReportExportHistories
                .Find(x => x.id == historyId && !x.isDeleted)
                .FirstOrDefaultAsync();
            if (history == null)
            {
                return NotFound("Report export history not found");
            }

            var filePath = ResolveCustomerDebtReportExportHistoryAbsolutePath(history);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Report export file not found");
            }

            return PhysicalFile(
                filePath,
                string.IsNullOrWhiteSpace(history.contentType)
                    ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                    : history.contentType,
                string.IsNullOrWhiteSpace(history.fileName) ? "customer-debt-report.xlsx" : history.fileName);
        }

        [HttpGet("account-types")]
        public async Task<IActionResult> GetAccountTypesAsync(
            [FromQuery] string search = null,
            [FromQuery] bool includeAll = false,
            [FromQuery] int maxItems = 200)
        {
            maxItems = maxItems < 1 ? 200 : Math.Min(maxItems, 500);

            var filterBuilder = Builders<BsonDocument>.Filter;
            var filters = new List<FilterDefinition<BsonDocument>>
            {
                filterBuilder.Ne("isDelete", true),
                filterBuilder.Ne("isDeleted", true)
            };

            if (!includeAll)
            {
                filters.Add(filterBuilder.Regex("accountType", new BsonRegularExpression("^(131|331)", "i")));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                var keywordRegex = new BsonRegularExpression(keyword, "i");
                filters.Add(filterBuilder.Or(
                    filterBuilder.Regex("accountType", keywordRegex),
                    filterBuilder.Regex("accountName", keywordRegex),
                    filterBuilder.Regex("accountNameLocal", keywordRegex)
                ));
            }

            var finalFilter = filters.Count == 0 ? filterBuilder.Empty : filterBuilder.And(filters);

            var docs = await _apiDocumentDbContext.AccountTypes
                .Find(finalFilter)
                .Limit(maxItems)
                .ToListAsync();

            var items = docs
                .Select(doc =>
                {
                    var accountType = GetBsonString(doc, "accountType");
                    var accountName = GetBsonString(doc, "accountName");
                    var accountNameLocal = GetBsonString(doc, "accountNameLocal");
                    var balanceSide = GetBsonString(doc, "balanceSide");
                    var displayName = !string.IsNullOrWhiteSpace(accountNameLocal) ? accountNameLocal : accountName;

                    return new
                    {
                        value = accountType,
                        label = string.IsNullOrWhiteSpace(displayName) ? accountType : $"{accountType} - {displayName}",
                        accountType,
                        accountName,
                        accountNameLocal,
                        balanceSide,
                    };
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.value))
                .OrderBy(x => x.accountType, StringComparer.Ordinal)
                .ToList();

            return Ok(items);
        }

        [HttpGet("account-types/manage")]
        public async Task<IActionResult> GetAccountTypeConfigsAsync(
            [FromQuery] string search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sortBy = "accountType",
            [FromQuery] string sortDirection = "asc")
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 200);

            var filterBuilder = Builders<BsonDocument>.Filter;
            var filters = new List<FilterDefinition<BsonDocument>>
            {
                filterBuilder.Ne("isDelete", true),
                filterBuilder.Ne("isDeleted", true)
            };

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                var keywordRegex = new BsonRegularExpression(keyword, "i");
                filters.Add(filterBuilder.Or(
                    filterBuilder.Regex("accountType", keywordRegex),
                    filterBuilder.Regex("accountName", keywordRegex),
                    filterBuilder.Regex("accountNameLocal", keywordRegex)
                ));
            }

            var finalFilter = filters.Count == 0 ? filterBuilder.Empty : filterBuilder.And(filters);
            var totalItems = (int)await _apiDocumentDbContext.AccountTypes.CountDocumentsAsync(finalFilter);

            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            var sort = (sortBy ?? "accountType").Trim().ToLowerInvariant() switch
            {
                "accountname" => isDesc
                    ? Builders<BsonDocument>.Sort.Descending("accountName")
                    : Builders<BsonDocument>.Sort.Ascending("accountName"),
                "updatedat" => isDesc
                    ? Builders<BsonDocument>.Sort.Descending("updatedAt")
                    : Builders<BsonDocument>.Sort.Ascending("updatedAt"),
                _ => isDesc
                    ? Builders<BsonDocument>.Sort.Descending("accountType")
                    : Builders<BsonDocument>.Sort.Ascending("accountType")
            };

            var docs = await _apiDocumentDbContext.AccountTypes
                .Find(finalFilter)
                .Sort(sort)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            var items = docs.Select(doc => new
            {
                id = GetBsonId(doc),
                accountType = GetBsonString(doc, "accountType"),
                accountName = GetBsonString(doc, "accountName"),
                accountNameLocal = GetBsonString(doc, "accountNameLocal"),
                balanceSide = GetBsonString(doc, "balanceSide"),
                updatedAt = GetBsonDateTime(doc, "updatedAt")
            }).ToList();

            return Ok(new
            {
                page,
                pageSize,
                totalItems,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                items
            });
        }

        [HttpPost("account-types")]
        public async Task<IActionResult> UpsertAccountTypeConfigAsync([FromBody] AccountTypeConfigUpsertRequest payload)
        {
            if (payload == null)
            {
                return BadRequest("Invalid payload");
            }

            var accountType = NormalizeAccountType(payload.accountType);
            if (string.IsNullOrWhiteSpace(accountType))
            {
                return BadRequest("accountType is required");
            }

            var accountName = (payload.accountName ?? string.Empty).Trim();
            var accountNameLocal = (payload.accountNameLocal ?? string.Empty).Trim();
            var balanceSide = NormalizeBalanceSide(payload.balanceSide);
            var now = DateTime.UtcNow;

            var requestedId = (payload.id ?? string.Empty).Trim();
            var updateFilter = BuildBsonIdFilter(requestedId);
            var existing = updateFilter == null
                ? null
                : await _apiDocumentDbContext.AccountTypes.Find(updateFilter).FirstOrDefaultAsync();

            var duplicateFilter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("accountType", accountType),
                Builders<BsonDocument>.Filter.Ne("isDelete", true),
                Builders<BsonDocument>.Filter.Ne("isDeleted", true)
            );

            var duplicate = await _apiDocumentDbContext.AccountTypes.Find(duplicateFilter).FirstOrDefaultAsync();
            if (duplicate != null)
            {
                var duplicateId = GetBsonId(duplicate);
                if (string.IsNullOrWhiteSpace(requestedId) || !string.Equals(duplicateId, requestedId, StringComparison.Ordinal))
                {
                    return Conflict(new { message = "accountType already exists" });
                }
            }

            if (existing != null)
            {
                existing["accountType"] = accountType;
                existing["accountName"] = accountName;
                existing["accountNameLocal"] = accountNameLocal;
                existing["balanceSide"] = balanceSide;
                existing["updatedAt"] = now;

                if (!existing.Contains("createdAt"))
                {
                    existing["createdAt"] = now;
                }

                existing["isDelete"] = false;
                existing["isDeleted"] = false;

                await _apiDocumentDbContext.AccountTypes.ReplaceOneAsync(updateFilter, existing);
                return Ok(new { success = true, id = GetBsonId(existing) });
            }

            var document = new BsonDocument
            {
                ["_id"] = ObjectId.GenerateNewId(),
                ["accountType"] = accountType,
                ["accountName"] = accountName,
                ["accountNameLocal"] = accountNameLocal,
                ["balanceSide"] = balanceSide,
                ["isDelete"] = false,
                ["isDeleted"] = false,
                ["createdAt"] = now,
                ["updatedAt"] = now
            };

            await _apiDocumentDbContext.AccountTypes.InsertOneAsync(document);
            return Ok(new { success = true, id = GetBsonId(document) });
        }

        [HttpDelete("account-types/{id}")]
        public async Task<IActionResult> DeleteAccountTypeConfigAsync(string id)
        {
            var filter = BuildBsonIdFilter(id);
            if (filter == null)
            {
                return BadRequest("Invalid id");
            }

            var existing = await _apiDocumentDbContext.AccountTypes.Find(filter).FirstOrDefaultAsync();
            if (existing == null)
            {
                return NotFound();
            }

            var now = DateTime.UtcNow;
            existing["isDelete"] = true;
            existing["isDeleted"] = true;
            existing["deletedAt"] = now;
            existing["updatedAt"] = now;

            await _apiDocumentDbContext.AccountTypes.ReplaceOneAsync(filter, existing);
            return Ok(new { success = true });
        }

        [HttpPost("customers")]
        public async Task<IActionResult> UpsertCustomerAsync([FromBody] CustomerAccount payload)
        {
            try
            {
                if (payload == null || string.IsNullOrWhiteSpace(payload.name))
                {
                    return BadRequest("Invalid payload");
                }
                var now = DateTime.UtcNow;
                var actor = GetCurrentActor();
                payload.updatedAt = now;
                payload.updatedBy = actor;

                // Check if this is a new record (no valid Id provided, or auto-generated ObjectId without matching customer)
                bool isNewRecord = string.IsNullOrWhiteSpace(payload.Id);
                
                // If ID exists, check if it's a valid existing customer
                if (!isNewRecord)
                {
                    var filter = Builders<CustomerAccount>.Filter.Eq(x => x.Id, payload.Id);
                    var existing = await _apiDocumentDbContext.CustomerAccounts.Find(filter).FirstOrDefaultAsync();
                    
                    if (existing != null)
                    {
                        // Existing customer - proceed with update logic
                        payload.createdAt = existing.createdAt;
                        payload.createdBy = existing.createdBy;

                        var changes = BuildCustomerChanges(existing, payload);

                        await _apiDocumentDbContext.CustomerAccounts.ReplaceOneAsync(filter, payload);

                        if (changes.Count > 0)
                        {
                            var logs = changes.Select(change => CreateAuditLogEntry(
                                payload.Id,
                                null,
                                "update",
                                change.field,
                                change.oldValue,
                                change.newValue,
                                actor,
                                null));
                            await AppendCustomerAuditLogsAsync(logs);
                        }
                        else
                        {
                            await AppendCustomerAuditLogAsync(CreateAuditLogEntry(
                                payload.Id,
                                null,
                                "touch",
                                "record",
                                null,
                                null,
                                actor,
                                "No field value changed"));
                        }

                        return Ok(payload);
                    }
                    else
                    {
                        // ID provided but customer not found - treat as new record
                        isNewRecord = true;
                    }
                }

                // Create new customer
                if (isNewRecord)
                {
                    payload.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
                    payload.createdAt = now;
                    payload.createdBy = actor;
                    await _apiDocumentDbContext.CustomerAccounts.InsertOneAsync(payload);

                    await AppendCustomerAuditLogAsync(CreateAuditLogEntry(
                        payload.Id,
                        null,
                        "create",
                        "record",
                        null,
                        BuildRecordSnapshot(payload),
                        actor,
                        "Customer created"));

                    return Ok(payload);
                }

                return BadRequest("Invalid request");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in UpsertCustomerAsync: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message, trace = ex.StackTrace });
            }
        }

        [HttpGet("customers/{id}/audit-logs")]
        public async Task<IActionResult> GetCustomerAuditLogsAsync(
            string id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Invalid id");
            }

            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 50 : Math.Min(pageSize, 200);

            var filter = Builders<CustomerAccount>.Filter.Eq(x => x.Id, id);
            var customer = await _apiDocumentDbContext.CustomerAccounts.Find(filter).FirstOrDefaultAsync();

            if (customer == null || customer.isDeleted)
            {
                return NotFound();
            }

            await MigrateLegacyDataForCustomerAsync(customer, true);

                var logsFilter = Builders<CustomerAuditLogEntry>.Filter.And(
                    Builders<CustomerAuditLogEntry>.Filter.Eq(x => x.customerId, id),
                    Builders<CustomerAuditLogEntry>.Filter.Eq(x => x.isDeleted, false));
                var totalItems = (int)await _apiDocumentDbContext.CustomerAuditLogs.CountDocumentsAsync(logsFilter);
            var items = await _apiDocumentDbContext.CustomerAuditLogs
                .Find(logsFilter)
                .SortByDescending(x => x.changedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalItems,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                items
            });
        }

        [HttpDelete("customers/{id}")]
        public async Task<IActionResult> DeleteCustomerAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Invalid id");
            }

            var filter = Builders<CustomerAccount>.Filter.Eq(x => x.Id, id);
            var customer = await _apiDocumentDbContext.CustomerAccounts.Find(filter).FirstOrDefaultAsync();

            if (customer == null)
            {
                return NotFound();
            }

            var actor = GetCurrentActor();
            var now = DateTime.UtcNow;

            // Soft delete customer
            customer.isDeleted = true;
            customer.deletedAt = now;
            customer.deletedBy = actor;
            customer.updatedAt = now;
            customer.updatedBy = actor;

            await _apiDocumentDbContext.CustomerAccounts.ReplaceOneAsync(filter, customer);

            // Create audit log for deletion
            await AppendCustomerAuditLogAsync(CreateAuditLogEntry(
                customer.Id,
                null,
                "delete",
                "record",
                "active",
                "deleted",
                actor,
                "Customer deleted"));

            // Soft delete related transactions
            var transactionFilter = Builders<CustomerDebtTransaction>.Filter.And(
                Builders<CustomerDebtTransaction>.Filter.Eq(x => x.customerId, id),
                Builders<CustomerDebtTransaction>.Filter.Eq(x => x.isDeleted, false));

            var transactionsToDelete = await _apiDocumentDbContext.CustomerDebtTransactions
                .Find(transactionFilter)
                .ToListAsync();

            foreach (var transaction in transactionsToDelete)
            {
                transaction.isDeleted = true;
                transaction.deletedAt = now;
                transaction.deletedBy = actor;
                transaction.updatedAt = now;
                transaction.updatedBy = actor;

                await _apiDocumentDbContext.CustomerDebtTransactions.ReplaceOneAsync(
                    Builders<CustomerDebtTransaction>.Filter.Eq(x => x.id, transaction.id),
                    transaction);
            }

            // Soft delete related audit logs
            var auditFilter = Builders<CustomerAuditLogEntry>.Filter.And(
                Builders<CustomerAuditLogEntry>.Filter.Eq(x => x.customerId, id),
                Builders<CustomerAuditLogEntry>.Filter.Eq(x => x.isDeleted, false));

            var auditsToDelete = await _apiDocumentDbContext.CustomerAuditLogs
                .Find(auditFilter)
                .ToListAsync();

            foreach (var audit in auditsToDelete)
            {
                audit.isDeleted = true;
                audit.deletedAt = now;
                audit.deletedBy = actor;

                await _apiDocumentDbContext.CustomerAuditLogs.ReplaceOneAsync(
                    Builders<CustomerAuditLogEntry>.Filter.Eq(x => x.id, audit.id),
                    audit);
            }

            return Ok(new { success = true, message = "Customer marked as deleted" });
        }

        [HttpPost("maintenance/migrate-legacy-customer-data")]
        public async Task<IActionResult> MigrateLegacyCustomerDataAsync([FromQuery] int batchSize = 500)
        {
            batchSize = batchSize < 1 ? 500 : Math.Min(batchSize, 5000);

            var filterBuilder = Builders<CustomerAccount>.Filter;
            var migrationFilter = filterBuilder.Or(
                filterBuilder.SizeGt(x => x.debtTransactions, 0),
                filterBuilder.SizeGt(x => x.auditLogs, 0));

            var customers = await _apiDocumentDbContext.CustomerAccounts
                .Find(migrationFilter)
                .Limit(batchSize)
                .ToListAsync();

            var scanned = 0;
            var migratedTransactions = 0;
            var migratedAuditLogs = 0;
            var clearedCustomers = 0;

            foreach (var customer in customers)
            {
                scanned++;
                var result = await MigrateLegacyDataForCustomerAsync(customer, true);
                migratedTransactions += result.migratedTransactions;
                migratedAuditLogs += result.migratedAuditLogs;
                if (result.clearedLegacy)
                {
                    clearedCustomers++;
                }
            }

            return Ok(new
            {
                scanned,
                migratedTransactions,
                migratedAuditLogs,
                clearedCustomers,
                hasMore = scanned >= batchSize
            });
        }

        [HttpGet("debt/overview")]
        public async Task<IActionResult> GetDebtOverviewAsync(
            [FromQuery] string status = null,
            [FromQuery] string riskLevel = null)
        {
            var filteredData = await GetFilteredDebtSourceAsync(null, status, riskLevel);

            var positiveBalances = filteredData
                .Select(x => GetNetBalance(x))
                .Where(x => x > 0)
                .ToList();

            var negativeBalances = filteredData
                .Select(x => GetNetBalance(x))
                .Where(x => x < 0)
                .Select(Math.Abs)
                .ToList();

            var totalReceivable = positiveBalances.Sum();
            var totalPayable = negativeBalances.Sum();
            var netExposure = totalReceivable - totalPayable;

            var highRiskExposure = filteredData
                .Where(x => string.Equals(x.riskLevel, "warning", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(x.riskLevel, "critical", StringComparison.OrdinalIgnoreCase))
                .Select(x => GetNetBalance(x))
                .Where(x => x > 0)
                .Sum();

            var agingBuckets = filteredData
                .Where(x => GetNetBalance(x) > 0)
                .Select(x => new { net = GetNetBalance(x), agingBucket = GetAgingBucket(GetAgingDays(x.lastTransactionAt)) })
                .GroupBy(x => x.agingBucket)
                .Select(x => new
                {
                    bucket = x.Key,
                    amount = x.Sum(y => y.net),
                    customers = x.Count()
                })
                .ToList();

            var topDebtors = filteredData
                .Select(x => new
                {
                    id = x.Id,
                    code = x.code,
                    name = x.name,
                    status = x.status,
                    riskLevel = x.riskLevel,
                    agingDays = GetAgingDays(x.lastTransactionAt),
                    lastTransactionAt = x.lastTransactionAt,
                    netBalance = GetNetBalance(x)
                })
                .Where(x => x.netBalance > 0)
                .OrderByDescending(x => x.netBalance)
                .ThenByDescending(x => x.agingDays)
                .Take(5)
                .ToList();

            return Ok(new
            {
                totalReceivable,
                totalPayable,
                netExposure,
                highRiskExposure,
                customerCount = filteredData.Count,
                debtorCount = filteredData.Count(x => GetNetBalance(x) > 0),
                creditorCount = filteredData.Count(x => GetNetBalance(x) < 0),
                agingBuckets,
                topDebtors
            });
        }

        [HttpGet("debt/list")]
        public async Task<IActionResult> GetDebtListAsync(
            [FromQuery] string search = null,
            [FromQuery] string status = null,
            [FromQuery] string riskLevel = null,
            [FromQuery] string balanceType = "all",
            [FromQuery] string agingBucket = "all",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "netBalance",
            [FromQuery] string sortDirection = "desc")
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

            var filteredData = await GetFilteredDebtSourceAsync(search, status, riskLevel);

            var projected = filteredData
                .Select(x =>
                {
                    var agingDays = GetAgingDays(x.lastTransactionAt);
                    var netBalance = GetNetBalance(x);

                    return new DebtListItemDto
                    {
                        id = x.Id,
                        code = x.code,
                        name = x.name,
                        phone = x.phone,
                        status = x.status,
                        riskLevel = x.riskLevel,
                        debtAmount = x.debtAmount,
                        creditAmount = x.creditAmount,
                        netBalance = netBalance,
                        agingDays = agingDays,
                        agingBucket = GetAgingBucket(agingDays),
                        lastTransactionAt = x.lastTransactionAt,
                        updatedAt = x.updatedAt
                    };
                })
                .ToList();

            if (!string.IsNullOrWhiteSpace(balanceType) && !string.Equals(balanceType, "all", StringComparison.OrdinalIgnoreCase))
            {
                projected = balanceType.ToLowerInvariant() switch
                {
                    "debt" => projected.Where(x => x.netBalance > 0).ToList(),
                    "credit" => projected.Where(x => x.netBalance < 0).ToList(),
                    "zero" => projected.Where(x => x.netBalance == 0).ToList(),
                    _ => projected
                };
            }

            if (!string.IsNullOrWhiteSpace(agingBucket) && !string.Equals(agingBucket, "all", StringComparison.OrdinalIgnoreCase))
            {
                projected = projected.Where(x => string.Equals(x.agingBucket, agingBucket, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var sorted = SortDebtItems(projected, sortBy, sortDirection);
            var totalItems = sorted.Count;
            var items = sorted
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                page,
                pageSize,
                totalItems,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                items
            });
        }

        [HttpPost("debt/transactions")]
        public async Task<IActionResult> AddDebtTransactionAsync([FromBody] CreateDebtTransactionRequest payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.customerId))
            {
                return BadRequest("Invalid payload");
            }

            if (payload.amount <= 0)
            {
                return BadRequest("Amount must be greater than 0");
            }

            var transactionType = (payload.transactionType ?? string.Empty).Trim().ToLowerInvariant();
            if (transactionType != "debt" && transactionType != "credit")
            {
                return BadRequest("transactionType must be debt or credit");
            }

            var filter = Builders<CustomerAccount>.Filter.Eq(x => x.Id, payload.customerId);
            var customer = await _apiDocumentDbContext.CustomerAccounts.Find(filter).FirstOrDefaultAsync();

            if (customer == null)
            {
                return NotFound("Customer not found");
            }

            await MigrateLegacyDataForCustomerAsync(customer, true);

            var actor = GetCurrentActor();
            var oldDebtAmount = customer.debtAmount;
            var oldCreditAmount = customer.creditAmount;
            var oldLastTransactionAt = customer.lastTransactionAt;
            var transactionCountFilter = Builders<CustomerDebtTransaction>.Filter.Eq(x => x.customerId, customer.Id);
            var oldTransactionCount = (int)await _apiDocumentDbContext.CustomerDebtTransactions.CountDocumentsAsync(transactionCountFilter);

            var transactionAt = payload.transactionAt ?? DateTime.UtcNow;
            var transaction = new CustomerDebtTransaction
            {
                id = Guid.NewGuid().ToString("N"),
                customerId = customer.Id,
                transactionType = transactionType,
                amount = payload.amount,
                transactionAt = transactionAt,
                note = payload.note,
                createdAt = DateTime.UtcNow,
                createdBy = actor,
            };

            await _apiDocumentDbContext.CustomerDebtTransactions.InsertOneAsync(transaction);

            if (transactionType == "debt")
            {
                customer.debtAmount += payload.amount;
            }
            else
            {
                customer.creditAmount += payload.amount;
            }

            customer.lastTransactionAt = transactionAt;
            customer.updatedAt = DateTime.UtcNow;
            customer.updatedBy = actor;

            await _apiDocumentDbContext.CustomerAccounts.ReplaceOneAsync(filter, customer);

            var logs = new List<CustomerAuditLogEntry>
            {
                CreateAuditLogEntry(customer.Id, transaction.id, "transaction-create", $"debtTransaction.{transaction.id}.transactionType", null, transaction.transactionType, actor, payload.note),
                CreateAuditLogEntry(customer.Id, transaction.id, "transaction-create", $"debtTransaction.{transaction.id}.amount", null, transaction.amount.ToString(), actor, payload.note),
                CreateAuditLogEntry(customer.Id, transaction.id, "transaction-create", $"debtTransaction.{transaction.id}.transactionAt", null, FormatDateTime(transaction.transactionAt), actor, payload.note),
                CreateAuditLogEntry(customer.Id, transaction.id, "transaction-create", $"debtTransaction.{transaction.id}.note", null, NormalizeText(transaction.note), actor, payload.note),
                CreateAuditLogEntry(customer.Id, null, "transaction", "debtTransactions.count", oldTransactionCount.ToString(), (oldTransactionCount + 1).ToString(), actor, payload.note),
            };

            if (oldDebtAmount != customer.debtAmount)
            {
                logs.Add(CreateAuditLogEntry(customer.Id, null, "transaction", "debtAmount", oldDebtAmount.ToString(), customer.debtAmount.ToString(), actor, payload.note));
            }

            if (oldCreditAmount != customer.creditAmount)
            {
                logs.Add(CreateAuditLogEntry(customer.Id, null, "transaction", "creditAmount", oldCreditAmount.ToString(), customer.creditAmount.ToString(), actor, payload.note));
            }

            logs.Add(CreateAuditLogEntry(customer.Id, null, "transaction", "lastTransactionAt", FormatDateTime(oldLastTransactionAt), FormatDateTime(customer.lastTransactionAt), actor, payload.note));
            await AppendCustomerAuditLogsAsync(logs);

            return Ok(new
            {
                success = true,
                customerId = customer.Id,
                debtAmount = customer.debtAmount,
                creditAmount = customer.creditAmount,
                netBalance = GetNetBalance(customer),
                transaction = new
                {
                    id = transaction.id,
                    transactionType = transaction.transactionType,
                    amount = transaction.amount,
                    transactionAt = transaction.transactionAt,
                    note = transaction.note,
                    createdAt = transaction.createdAt,
                    createdBy = transaction.createdBy,
                }
            });
        }

        [HttpPut("debt/transactions/{transactionId}")]
        public async Task<IActionResult> UpdateDebtTransactionAsync(string transactionId, [FromBody] UpdateDebtTransactionRequest payload)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
            {
                return BadRequest("Invalid transactionId");
            }

            if (payload == null || payload.amount <= 0)
            {
                return BadRequest("Invalid payload");
            }

            var transactionType = (payload.transactionType ?? string.Empty).Trim().ToLowerInvariant();
            if (transactionType != "debt" && transactionType != "credit")
            {
                return BadRequest("transactionType must be debt or credit");
            }

            var transactionFilter = Builders<CustomerDebtTransaction>.Filter.Eq(x => x.id, transactionId);
            var transaction = await _apiDocumentDbContext.CustomerDebtTransactions.Find(transactionFilter).FirstOrDefaultAsync();
            if (transaction == null || transaction.isDeleted)
            {
                var legacyCustomerFilter = Builders<CustomerAccount>.Filter.ElemMatch(x => x.debtTransactions, t => t.id == transactionId);
                var legacyCustomer = await _apiDocumentDbContext.CustomerAccounts.Find(legacyCustomerFilter).FirstOrDefaultAsync();
                if (legacyCustomer == null)
                {
                    return NotFound("Transaction not found");
                }

                await MigrateLegacyDataForCustomerAsync(legacyCustomer, true);
                transaction = await _apiDocumentDbContext.CustomerDebtTransactions.Find(transactionFilter).FirstOrDefaultAsync();
                if (transaction == null || transaction.isDeleted)
                {
                    return NotFound("Transaction not found");
                }
            }

            var customerFilter = Builders<CustomerAccount>.Filter.Eq(x => x.Id, transaction.customerId);
            var customer = await _apiDocumentDbContext.CustomerAccounts.Find(customerFilter).FirstOrDefaultAsync();
            if (customer == null)
            {
                return NotFound("Customer not found");
            }

            var actor = GetCurrentActor();
            var oldType = transaction.transactionType;
            var oldAmount = transaction.amount;
            var oldTransactionAt = transaction.transactionAt;
            var oldNote = transaction.note;
            var oldDebtAmount = customer.debtAmount;
            var oldCreditAmount = customer.creditAmount;
            var oldLastTransactionAt = customer.lastTransactionAt;

            var nextTransactionAt = payload.transactionAt ?? transaction.transactionAt;

            if (string.Equals(oldType, "debt", StringComparison.OrdinalIgnoreCase))
            {
                customer.debtAmount -= oldAmount;
            }
            else
            {
                customer.creditAmount -= oldAmount;
            }

            if (string.Equals(transactionType, "debt", StringComparison.OrdinalIgnoreCase))
            {
                customer.debtAmount += payload.amount;
            }
            else
            {
                customer.creditAmount += payload.amount;
            }

            transaction.transactionType = transactionType;
            transaction.amount = payload.amount;
            transaction.transactionAt = nextTransactionAt;
            transaction.note = payload.note;

            var otherTransactionDates = await _apiDocumentDbContext.CustomerDebtTransactions
                .Find(x => x.customerId == customer.Id && x.id != transactionId)
                .Project(x => x.transactionAt)
                .ToListAsync();
            customer.lastTransactionAt = otherTransactionDates.Count == 0
                ? nextTransactionAt
                : new[] { nextTransactionAt, otherTransactionDates.Max() }.Max();
            customer.updatedAt = DateTime.UtcNow;
            customer.updatedBy = actor;

            transaction.updatedAt = DateTime.UtcNow;
            transaction.updatedBy = actor;

            await _apiDocumentDbContext.CustomerDebtTransactions.ReplaceOneAsync(transactionFilter, transaction);
            await _apiDocumentDbContext.CustomerAccounts.ReplaceOneAsync(customerFilter, customer);

            var logs = new List<CustomerAuditLogEntry>
            {
                CreateAuditLogEntry(customer.Id, transactionId, "transaction-update", $"debtTransaction.{transactionId}.transactionType", NormalizeText(oldType), NormalizeText(transaction.transactionType), actor, payload.note),
                CreateAuditLogEntry(customer.Id, transactionId, "transaction-update", $"debtTransaction.{transactionId}.amount", oldAmount.ToString(), transaction.amount.ToString(), actor, payload.note),
                CreateAuditLogEntry(customer.Id, transactionId, "transaction-update", $"debtTransaction.{transactionId}.transactionAt", FormatDateTime(oldTransactionAt), FormatDateTime(transaction.transactionAt), actor, payload.note),
                CreateAuditLogEntry(customer.Id, transactionId, "transaction-update", $"debtTransaction.{transactionId}.note", NormalizeText(oldNote), NormalizeText(transaction.note), actor, payload.note),
            };

            if (oldDebtAmount != customer.debtAmount)
            {
                logs.Add(CreateAuditLogEntry(customer.Id, null, "transaction", "debtAmount", oldDebtAmount.ToString(), customer.debtAmount.ToString(), actor, payload.note));
            }

            if (oldCreditAmount != customer.creditAmount)
            {
                logs.Add(CreateAuditLogEntry(customer.Id, null, "transaction", "creditAmount", oldCreditAmount.ToString(), customer.creditAmount.ToString(), actor, payload.note));
            }

            logs.Add(CreateAuditLogEntry(customer.Id, null, "transaction", "lastTransactionAt", FormatDateTime(oldLastTransactionAt), FormatDateTime(customer.lastTransactionAt), actor, payload.note));
            await AppendCustomerAuditLogsAsync(logs);

            return Ok(new
            {
                success = true,
                customerId = customer.Id,
                debtAmount = customer.debtAmount,
                creditAmount = customer.creditAmount,
                netBalance = GetNetBalance(customer),
                transaction
            });
        }

        [HttpGet("debt/transactions/{transactionId}/audit-logs")]
        public async Task<IActionResult> GetDebtTransactionAuditLogsAsync(
            string transactionId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
            {
                return BadRequest("Invalid transactionId");
            }

            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 50 : Math.Min(pageSize, 200);

            var transactionFilter = Builders<CustomerDebtTransaction>.Filter.Eq(x => x.id, transactionId);
            var transaction = await _apiDocumentDbContext.CustomerDebtTransactions.Find(transactionFilter).FirstOrDefaultAsync();
            if (transaction == null)
            {
                var legacyCustomerFilter = Builders<CustomerAccount>.Filter.ElemMatch(x => x.debtTransactions, t => t.id == transactionId);
                var legacyCustomer = await _apiDocumentDbContext.CustomerAccounts.Find(legacyCustomerFilter).FirstOrDefaultAsync();
                if (legacyCustomer == null)
                {
                    return NotFound("Transaction not found");
                }

                await MigrateLegacyDataForCustomerAsync(legacyCustomer, true);
                transaction = await _apiDocumentDbContext.CustomerDebtTransactions.Find(transactionFilter).FirstOrDefaultAsync();
                if (transaction == null)
                {
                    return NotFound("Transaction not found");
                }
            }

            var fieldPrefixPattern = $"^debtTransaction\\.{transactionId}\\.";
            var logsFilterBuilder = Builders<CustomerAuditLogEntry>.Filter;
            var logsFilter = logsFilterBuilder.And(
                logsFilterBuilder.Eq(x => x.customerId, transaction.customerId),
                logsFilterBuilder.Or(
                    logsFilterBuilder.Eq(x => x.transactionId, transactionId),
                    logsFilterBuilder.Regex(x => x.field, new MongoDB.Bson.BsonRegularExpression(fieldPrefixPattern, "i"))),
                logsFilterBuilder.Eq(x => x.isDeleted, false));

            var totalItems = (int)await _apiDocumentDbContext.CustomerAuditLogs.CountDocumentsAsync(logsFilter);
            var items = await _apiDocumentDbContext.CustomerAuditLogs
                .Find(logsFilter)
                .SortByDescending(x => x.changedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalItems,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                items
            });
        }

        [HttpGet("debt/transactions")]
        public async Task<IActionResult> GetDebtTransactionsAsync(
            [FromQuery] string customerId = null,
            [FromQuery] string search = null,
            [FromQuery] string transactionType = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sortBy = "transactionAt",
            [FromQuery] string sortDirection = "desc")
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 200);

            if (!string.IsNullOrWhiteSpace(customerId))
            {
                var customerExists = await _apiDocumentDbContext.CustomerAccounts
                    .Find(x => x.Id == customerId && !x.isDeleted)
                    .AnyAsync();

                if (!customerExists)
                {
                    return Ok(new
                    {
                        page,
                        pageSize,
                        totalItems = 0,
                        totalPages = 0,
                        items = new List<DebtTransactionListItem>()
                    });
                }
            }

            var transactionFilterBuilder = Builders<CustomerDebtTransaction>.Filter;
            var transactionFilter = !string.IsNullOrWhiteSpace(customerId)
                ? transactionFilterBuilder.Eq(x => x.customerId, customerId)
                : transactionFilterBuilder.Empty;

            var transactionDocs = await _apiDocumentDbContext.CustomerDebtTransactions
                .Find(transactionFilterBuilder.And(transactionFilter, transactionFilterBuilder.Eq(x => x.isDeleted, false)))
                .ToListAsync();

            var transactionCustomerIds = transactionDocs
                .Select(x => x.customerId)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .ToList();
            var customers = transactionCustomerIds.Count == 0
                ? new List<CustomerAccount>()
                : await _apiDocumentDbContext.CustomerAccounts
                    .Find(x => transactionCustomerIds.Contains(x.Id) && !x.isDeleted)
                    .ToListAsync();
            var customerLookup = customers
                .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .ToDictionary(x => x.Id, x => x);

            var transactionIdSet = new HashSet<string>(
                transactionDocs
                    .Where(x => !string.IsNullOrWhiteSpace(x.id))
                    .Select(x => x.id),
                StringComparer.Ordinal);
            var legacyCustomerFilter = !string.IsNullOrWhiteSpace(customerId)
                ? Builders<CustomerAccount>.Filter.Eq(x => x.Id, customerId)
                : Builders<CustomerAccount>.Filter.SizeGt(x => x.debtTransactions, 0);
            var legacyCustomers = await _apiDocumentDbContext.CustomerAccounts
                .Find(legacyCustomerFilter)
                .Project(x => new CustomerAccount
                {
                    Id = x.Id,
                    code = x.code,
                    name = x.name,
                    debtTransactions = x.debtTransactions
                })
                .ToListAsync();

            var legacyTransactionDocs = legacyCustomers
                .Where(c => c.debtTransactions != null)
                .SelectMany(c => c.debtTransactions
                    .Where(t => !string.IsNullOrWhiteSpace(t.id) && !transactionIdSet.Contains(t.id))
                    .Select(t => new CustomerDebtTransaction
                    {
                        id = t.id,
                        customerId = c.Id,
                        transactionType = t.transactionType,
                        amount = t.amount,
                        transactionAt = t.transactionAt,
                        note = t.note,
                        createdAt = t.createdAt,
                        createdBy = t.createdBy,
                    }))
                .ToList();

            transactionDocs.AddRange(legacyTransactionDocs);

            foreach (var legacyCustomer in legacyCustomers)
            {
                if (string.IsNullOrWhiteSpace(legacyCustomer.Id) || customerLookup.ContainsKey(legacyCustomer.Id))
                {
                    continue;
                }

                customerLookup[legacyCustomer.Id] = legacyCustomer;
            }

            var transactions = transactionDocs
                .Select(t =>
                {
                    customerLookup.TryGetValue(t.customerId ?? string.Empty, out var customer);
                    return new DebtTransactionListItem
                    {
                        id = t.id,
                        customerId = t.customerId,
                        customerCode = customer?.code,
                        customerName = customer?.name,
                        transactionType = t.transactionType,
                        amount = t.amount,
                        transactionAt = t.transactionAt,
                        note = t.note,
                        createdAt = t.createdAt,
                        createdBy = t.createdBy,
                    };
                })
                .ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                transactions = transactions
                    .Where(x =>
                        ContainsText(x.customerCode, keyword)
                        || ContainsText(x.customerName, keyword)
                        || ContainsText(x.note, keyword)
                        || ContainsText(x.createdBy, keyword))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(transactionType) && !string.Equals(transactionType, "all", StringComparison.OrdinalIgnoreCase))
            {
                transactions = transactions
                    .Where(x => string.Equals(x.transactionType, transactionType, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            transactions = SortDebtTransactions(transactions, sortBy, sortDirection);

            var totalItems = transactions.Count;
            var items = transactions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                page,
                pageSize,
                totalItems,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                items
            });
        }

        [HttpGet("debt/customers/{customerId}/export-excel")]
        public async Task<IActionResult> ExportDebtCustomerExcelAsync(
            string customerId,
            [FromQuery] string transactionIds = null)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                return BadRequest("Invalid customerId");
            }

            var filter = Builders<CustomerAccount>.Filter.Eq(x => x.Id, customerId);
            var customer = await _apiDocumentDbContext.CustomerAccounts.Find(filter).FirstOrDefaultAsync();
            if (customer == null)
            {
                return NotFound("Customer not found");
            }

            await MigrateLegacyDataForCustomerAsync(customer, true);

            List<CustomerDebtTransaction> transactions;
            var requestedIds = (transactionIds ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (requestedIds.Count > 0)
            {
                var allTransactions = await _apiDocumentDbContext.CustomerDebtTransactions
                    .Find(x => x.customerId == customerId)
                    .SortBy(x => x.transactionAt)
                    .ToListAsync();
                transactions = allTransactions
                    .Where(t => requestedIds.Contains(t.id ?? string.Empty))
                    .ToList();
            }
            else
            {
                transactions = await _apiDocumentDbContext.CustomerDebtTransactions
                    .Find(x => x.customerId == customerId)
                    .SortBy(x => x.transactionAt)
                    .ToListAsync();
            }

            var fileBytes = BuildDebtCustomerExcelFile(customer, transactions);
            var safeCode = string.IsNullOrWhiteSpace(customer.code)
                ? "khach-hang"
                : string.Join("-", customer.code.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
            var exportNow = ConvertToDisplayTime(DateTime.UtcNow);
            var fileName = $"so-chi-tiet-cong-no-{safeCode}-{exportNow:yyyyMMddHHmmss}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        private SortDefinition<CustomerAccount> BuildSort(string sortBy, string sortDirection)
        {
            var sortBuilder = Builders<CustomerAccount>.Sort;
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return sortBy?.ToLowerInvariant() switch
            {
                "name" => isDesc ? sortBuilder.Descending(x => x.name) : sortBuilder.Ascending(x => x.name),
                "debtamount" => isDesc ? sortBuilder.Descending(x => x.debtAmount) : sortBuilder.Ascending(x => x.debtAmount),
                "creditamount" => isDesc ? sortBuilder.Descending(x => x.creditAmount) : sortBuilder.Ascending(x => x.creditAmount),
                _ => isDesc ? sortBuilder.Descending(x => x.updatedAt) : sortBuilder.Ascending(x => x.updatedAt)
            };
        }

        private async Task<List<CustomerAccount>> GetFilteredDebtSourceAsync(string search, string status, string riskLevel, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var filterBuilder = Builders<CustomerAccount>.Filter;
            var filters = new List<FilterDefinition<CustomerAccount>>();

            filters.Add(filterBuilder.Eq(x => x.isDeleted, false));
            filters.Add(filterBuilder.Ne("isDelete", true));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                filters.Add(filterBuilder.Or(
                    filterBuilder.Regex(x => x.name, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
                    filterBuilder.Regex(x => x.code, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
                    filterBuilder.Regex(x => x.taxCode, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
                    filterBuilder.Regex(x => x.phone, new MongoDB.Bson.BsonRegularExpression(keyword, "i"))
                ));
            }

            if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
            {
                filters.Add(filterBuilder.Eq(x => x.status, status));
            }

            if (!string.IsNullOrWhiteSpace(riskLevel) && !string.Equals(riskLevel, "all", StringComparison.OrdinalIgnoreCase))
            {
                filters.Add(filterBuilder.Eq(x => x.riskLevel, riskLevel));
            }

            var normalizedFromDate = fromDate?.Date;
            var normalizedToDate = toDate?.Date.AddDays(1).AddTicks(-1);

            if (normalizedFromDate.HasValue || normalizedToDate.HasValue)
            {
                var transactionFilterBuilder = Builders<CustomerDebtTransaction>.Filter;
                var transactionFilters = new List<FilterDefinition<CustomerDebtTransaction>>
                {
                    transactionFilterBuilder.Eq(x => x.isDeleted, false)
                };

                if (normalizedFromDate.HasValue)
                {
                    transactionFilters.Add(transactionFilterBuilder.Gte(x => x.transactionAt, normalizedFromDate.Value));
                }

                if (normalizedToDate.HasValue)
                {
                    transactionFilters.Add(transactionFilterBuilder.Lte(x => x.transactionAt, normalizedToDate.Value));
                }

                var matchingCustomerIds = await _apiDocumentDbContext.CustomerDebtTransactions
                    .Find(transactionFilterBuilder.And(transactionFilters))
                    .Project(x => x.customerId)
                    .ToListAsync();

                var distinctCustomerIds = matchingCustomerIds
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                if (distinctCustomerIds.Count == 0)
                {
                    return new List<CustomerAccount>();
                }

                filters.Add(filterBuilder.In(x => x.Id, distinctCustomerIds));
            }

            var finalFilter = filters.Count > 0 ? filterBuilder.And(filters) : filterBuilder.Empty;

            return await _apiDocumentDbContext.CustomerAccounts
                .Find(finalFilter)
                .ToListAsync();
        }

        private byte[] BuildCustomerDebtReportExcelFile(
            List<DebtListItemDto> items,
            string status,
            string riskLevel,
            DateTime? fromDate,
            DateTime? toDate,
            string search)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var summarySheet = package.Workbook.Worksheets.Add("ReportSummary");
            var detailSheet = package.Workbook.Worksheets.Add("CustomerDetails");

            var totalDebt = items.Sum(x => x.debtAmount);
            var totalCredit = items.Sum(x => x.creditAmount);
            var totalNet = items.Sum(x => x.netBalance);
            var topExposure = items.Count == 0 ? 0 : items.Max(x => x.netBalance);
            var displayNow = ConvertToDisplayTime(DateTime.UtcNow);

            summarySheet.Cells.Style.Font.Name = "Times New Roman";
            summarySheet.Cells[1, 1, 1, 4].Merge = true;
            summarySheet.Cells[1, 1].Value = "CUSTOMER DEBT REPORT / BAO CAO CONG NO KHACH HANG";
            summarySheet.Cells[1, 1].Style.Font.Bold = true;
            summarySheet.Cells[1, 1].Style.Font.Size = 16;
            summarySheet.Cells[2, 1, 2, 4].Merge = true;
            summarySheet.Cells[2, 1].Value = $"Generated at / Tao luc: {displayNow:dd/MM/yyyy HH:mm}";

            summarySheet.Cells[4, 1].Value = "Status";
            summarySheet.Cells[4, 2].Value = string.IsNullOrWhiteSpace(status) ? "all" : status;
            summarySheet.Cells[5, 1].Value = "Risk level";
            summarySheet.Cells[5, 2].Value = string.IsNullOrWhiteSpace(riskLevel) ? "all" : riskLevel;
            summarySheet.Cells[6, 1].Value = "From date";
            summarySheet.Cells[6, 2].Value = fromDate.HasValue ? ConvertToDisplayTime(fromDate.Value).ToString("dd/MM/yyyy") : "-";
            summarySheet.Cells[7, 1].Value = "To date";
            summarySheet.Cells[7, 2].Value = toDate.HasValue ? ConvertToDisplayTime(toDate.Value).ToString("dd/MM/yyyy") : "-";
            summarySheet.Cells[8, 1].Value = "Search";
            summarySheet.Cells[8, 2].Value = string.IsNullOrWhiteSpace(search) ? "-" : search.Trim();

            summarySheet.Cells[10, 1].Value = "Customer count";
            summarySheet.Cells[10, 2].Value = items.Count;
            summarySheet.Cells[11, 1].Value = "Total debt";
            summarySheet.Cells[11, 2].Value = totalDebt;
            summarySheet.Cells[12, 1].Value = "Total credit";
            summarySheet.Cells[12, 2].Value = totalCredit;
            summarySheet.Cells[13, 1].Value = "Net exposure";
            summarySheet.Cells[13, 2].Value = totalNet;
            summarySheet.Cells[14, 1].Value = "Top exposure";
            summarySheet.Cells[14, 2].Value = topExposure;
            summarySheet.Column(1).Width = 18;
            summarySheet.Column(2).Width = 28;
            summarySheet.Column(3).Width = 18;
            summarySheet.Column(4).Width = 18;
            summarySheet.Cells[11, 2, 14, 2].Style.Numberformat.Format = "#,##0";

            detailSheet.Cells.Style.Font.Name = "Times New Roman";
            detailSheet.Cells[1, 1, 1, 9].Merge = true;
            detailSheet.Cells[1, 1].Value = "CUSTOMER DEBT DETAILS / CHI TIET BAO CAO CONG NO";
            detailSheet.Cells[1, 1].Style.Font.Bold = true;
            detailSheet.Cells[1, 1].Style.Font.Size = 15;

            var headers = new[] { "Code", "Customer", "Phone", "Status", "Risk", "Debt", "Credit", "Net", "Last Txn" };
            for (var index = 0; index < headers.Length; index++)
            {
                detailSheet.Cells[3, index + 1].Value = headers[index];
                detailSheet.Cells[3, index + 1].Style.Font.Bold = true;
            }

            var row = 4;
            foreach (var item in items)
            {
                detailSheet.Cells[row, 1].Value = item.code;
                detailSheet.Cells[row, 2].Value = item.name;
                detailSheet.Cells[row, 3].Value = item.phone;
                detailSheet.Cells[row, 4].Value = item.status;
                detailSheet.Cells[row, 5].Value = item.riskLevel;
                detailSheet.Cells[row, 6].Value = item.debtAmount;
                detailSheet.Cells[row, 7].Value = item.creditAmount;
                detailSheet.Cells[row, 8].Value = item.netBalance;
                detailSheet.Cells[row, 9].Value = item.lastTransactionAt.HasValue ? ConvertToDisplayTime(item.lastTransactionAt.Value).ToString("dd/MM/yyyy") : string.Empty;
                row++;
            }

            detailSheet.Column(1).Width = 14;
            detailSheet.Column(2).Width = 34;
            detailSheet.Column(3).Width = 18;
            detailSheet.Column(4).Width = 14;
            detailSheet.Column(5).Width = 14;
            detailSheet.Column(6).Width = 16;
            detailSheet.Column(7).Width = 16;
            detailSheet.Column(8).Width = 16;
            detailSheet.Column(9).Width = 16;
            if (row > 4)
            {
                detailSheet.Cells[4, 6, row - 1, 8].Style.Numberformat.Format = "#,##0";
            }

            return package.GetAsByteArray();
        }

        private string EnsureCustomerDebtReportExportHistoryDirectory(DateTime exportedAt, out string relativeDirectory)
        {
            relativeDirectory = exportedAt.ToString("ddMMMyyyy", CultureInfo.InvariantCulture);
            var path = Path.Combine(GetCustomerDebtReportExportHistoryRootPath(), relativeDirectory.Replace("/", Path.DirectorySeparatorChar.ToString()));
            Directory.CreateDirectory(path);
            return path;
        }

        private string BuildCustomerDebtReportExportHistoryRelativePath(string relativeDirectory, string storedFileName)
        {
            return Path.Combine(relativeDirectory ?? string.Empty, storedFileName ?? string.Empty).Replace("\\", "/");
        }

        private string ResolveCustomerDebtReportExportHistoryAbsolutePath(CustomerDebtReportExportHistory history)
        {
            if (history == null)
            {
                return Path.Combine(GetCustomerDebtReportExportHistoryRootPath(), string.Empty);
            }

            if (!string.IsNullOrWhiteSpace(history.relativePath))
            {
                if (Path.IsPathRooted(history.relativePath))
                {
                    return history.relativePath;
                }

                var normalizedRelativePath = history.relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()).TrimStart(Path.DirectorySeparatorChar);
                return Path.Combine(GetCustomerDebtReportExportHistoryRootPath(), normalizedRelativePath);
            }

            return Path.Combine(GetCustomerDebtReportExportHistoryRootPath(), history.storedFileName ?? GetSafeOriginalFileName(history.fileName));
        }

        private string GetCustomerDebtReportExportHistoryRootPath()
        {
            var configuredPath = _configuration["AttachmentStorage:DebtExportHistoryRootPath"];
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                var trimmed = configuredPath.Trim();
                var basePath = Path.IsPathRooted(trimmed)
                    ? trimmed
                    : Path.GetFullPath(trimmed, _webHostEnvironment.ContentRootPath);
                return Path.Combine(basePath, "ReportHistory");
            }

            return Path.Combine(_webHostEnvironment.ContentRootPath, "AppData", "DebtTransactionFiles", "ExportHistory", "ReportHistory");
        }

        private string GetSafeOriginalFileName(string fileName)
        {
            var source = string.IsNullOrWhiteSpace(fileName) ? "upload.bin" : fileName.Trim();
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(source.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray()).Trim();
            return string.IsNullOrWhiteSpace(sanitized) ? "upload.bin" : sanitized;
        }

        private string BuildStoredFileName(string originalFileName, string uniqueId)
        {
            var safeOriginalName = GetSafeOriginalFileName(originalFileName);
            var extension = SanitizeFileExtension(Path.GetExtension(safeOriginalName) ?? string.Empty);
            var baseName = Path.GetFileNameWithoutExtension(safeOriginalName) ?? "file";
            var normalized = baseName.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();
            foreach (var ch in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(ch);
                }
            }

            var asciiBaseName = builder.ToString().Normalize(NormalizationForm.FormC);
            var slug = Regex.Replace(asciiBaseName, "[^A-Za-z0-9]+", "-").Trim('-').ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(slug))
            {
                slug = "file";
            }

            return string.IsNullOrWhiteSpace(extension)
                ? $"{slug}-{uniqueId}"
                : $"{slug}-{uniqueId}{extension}";
        }

        private string SanitizeFileExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                return string.Empty;
            }

            var lowered = extension.Trim().ToLowerInvariant();
            if (!lowered.StartsWith('.'))
            {
                lowered = $".{lowered}";
            }

            var cleaned = Regex.Replace(lowered, "[^a-z0-9.]", string.Empty);
            return cleaned == "." ? string.Empty : cleaned;
        }

        private byte[] BuildDebtCustomerExcelFile(CustomerAccount customer, List<CustomerDebtTransaction> transactions)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("SoChiTietCongNo");
            worksheet.View.ShowGridLines = false;
            worksheet.Cells.Style.Font.Name = "Times New Roman";
            worksheet.Cells.Style.Font.Size = 11;

            worksheet.Column(1).Width = 8;
            worksheet.Column(2).Width = 12;
            worksheet.Column(3).Width = 42;
            worksheet.Column(4).Width = 8;
            worksheet.Column(5).Width = 15;
            worksheet.Column(6).Width = 15;
            worksheet.Column(7).Width = 26;

            transactions ??= new List<CustomerDebtTransaction>();
            var ledgerAccountCode = ResolveLedgerAccountCode(customer);
            var ledgerTitle = ledgerAccountCode == "131"
                ? "SỔ CHI TIẾT CÔNG NỢ PHẢI THU"
                : "SỔ CHI TIẾT CÔNG NỢ PHẢI TRẢ";

            var totalDebit = transactions
                .Where(x => string.Equals(x.transactionType, "debt", StringComparison.OrdinalIgnoreCase))
                .Sum(x => x.amount);
            var totalCredit = transactions
                .Where(x => string.Equals(x.transactionType, "credit", StringComparison.OrdinalIgnoreCase))
                .Sum(x => x.amount);
            var closingBalance = totalDebit - totalCredit;

            worksheet.Cells[1, 1].Value = "Tên đơn vị:";
            worksheet.Cells[1, 2, 1, 7].Merge = true;
            worksheet.Cells[1, 2].Value = customer.name ?? string.Empty;

            worksheet.Cells[2, 1].Value = "Địa chỉ:";
            worksheet.Cells[2, 2, 2, 7].Merge = true;
            worksheet.Cells[2, 2].Value = customer.address ?? string.Empty;

            worksheet.Cells[3, 1].Value = "MST:";
            worksheet.Cells[3, 2, 3, 7].Merge = true;
            worksheet.Cells[3, 2].Value = customer.taxCode ?? string.Empty;

            worksheet.Cells[6, 1, 6, 7].Merge = true;
            worksheet.Cells[6, 1].Value = ledgerTitle;
            worksheet.Cells[6, 1].Style.Font.Bold = true;
            worksheet.Cells[6, 1].Style.Font.Size = 18;
            worksheet.Cells[6, 1].Style.Font.Color.SetColor(Color.Red);
            worksheet.Cells[6, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells[7, 1, 7, 7].Merge = true;
            worksheet.Cells[7, 1].Value = BuildPeriodRangeText(transactions);
            worksheet.Cells[7, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells[8, 2, 8, 6].Merge = true;
            worksheet.Cells[8, 2].Value = customer.name ?? string.Empty;
            worksheet.Cells[8, 2].Style.Font.Bold = true;
            worksheet.Cells[8, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells[9, 2, 9, 6].Merge = true;
            worksheet.Cells[9, 2].Value = customer.code ?? string.Empty;
            worksheet.Cells[9, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[9, 2].Style.Font.Italic = true;

            worksheet.Cells[10, 1, 11, 1].Merge = true;
            worksheet.Cells[10, 2, 11, 2].Merge = true;
            worksheet.Cells[10, 3, 11, 3].Merge = true;
            worksheet.Cells[10, 4, 11, 4].Merge = true;
            worksheet.Cells[10, 5, 10, 6].Merge = true;
            worksheet.Cells[10, 7, 11, 7].Merge = true;

            worksheet.Cells[10, 1].Value = "Số hiệu";
            worksheet.Cells[10, 2].Value = "Ngày tháng";
            worksheet.Cells[10, 3].Value = "DIỄN GIẢI";
            worksheet.Cells[10, 4].Value = "Số hiệu TK";
            worksheet.Cells[10, 5].Value = "Số tiền";
            worksheet.Cells[10, 7].Value = "Ghi chú";
            worksheet.Cells[11, 5].Value = "Nợ";
            worksheet.Cells[11, 6].Value = "Có";

            using (var headerRange = worksheet.Cells[10, 1, 11, 7])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                headerRange.Style.WrapText = true;
                headerRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            var startRow = 12;
            worksheet.Cells[startRow, 1, startRow, 4].Merge = true;
            worksheet.Cells[startRow, 1].Value = "Số dư đầu tháng";
            worksheet.Cells[startRow, 1].Style.Font.Bold = true;
            worksheet.Cells[startRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[startRow, 5].Value = "-";
            worksheet.Cells[startRow, 6].Value = "-";

            var detailRow = startRow + 1;
            var serial = 0;
            foreach (var transaction in transactions)
            {
                serial++;
                var displayTransactionAt = ConvertToDisplayTime(transaction.transactionAt);
                worksheet.Cells[detailRow, 1].Value = serial;
                worksheet.Cells[detailRow, 2].Value = displayTransactionAt;
                worksheet.Cells[detailRow, 2].Style.Numberformat.Format = "d/M/yy";
                worksheet.Cells[detailRow, 3].Value = string.IsNullOrWhiteSpace(transaction.note)
                    ? (string.Equals(transaction.transactionType, "credit", StringComparison.OrdinalIgnoreCase)
                        ? (ledgerAccountCode == "131" ? "THU TIEN CONG NO" : "THANH TOAN CONG NO")
                        : (ledgerAccountCode == "131" ? "PHAT SINH CONG NO PHAI THU" : "PHAT SINH CONG NO PHAI TRA"))
                    : transaction.note;
                worksheet.Cells[detailRow, 4].Value = ledgerAccountCode;
                worksheet.Cells[detailRow, 5].Value = string.Equals(transaction.transactionType, "debt", StringComparison.OrdinalIgnoreCase) ? transaction.amount : 0;
                worksheet.Cells[detailRow, 6].Value = string.Equals(transaction.transactionType, "credit", StringComparison.OrdinalIgnoreCase) ? transaction.amount : 0;
                worksheet.Cells[detailRow, 7].Value = transaction.note ?? string.Empty;
                detailRow++;
            }

            var minimumLedgerRows = 8;
            while ((detailRow - (startRow + 1)) < minimumLedgerRows)
            {
                worksheet.Cells[detailRow, 1].Value = string.Empty;
                worksheet.Cells[detailRow, 2].Value = string.Empty;
                worksheet.Cells[detailRow, 3].Value = string.Empty;
                worksheet.Cells[detailRow, 4].Value = string.Empty;
                worksheet.Cells[detailRow, 5].Value = string.Empty;
                worksheet.Cells[detailRow, 6].Value = string.Empty;
                worksheet.Cells[detailRow, 7].Value = string.Empty;
                detailRow++;
            }

            var totalRow = detailRow;
            worksheet.Cells[totalRow, 1, totalRow, 4].Merge = true;
            worksheet.Cells[totalRow, 1].Value = "Tổng số phát sinh";
            worksheet.Cells[totalRow, 1].Style.Font.Bold = true;
            worksheet.Cells[totalRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[totalRow, 5].Value = totalDebit;
            worksheet.Cells[totalRow, 6].Value = totalCredit;

            var closingRow = totalRow + 1;
            worksheet.Cells[closingRow, 1, closingRow, 4].Merge = true;
            worksheet.Cells[closingRow, 1].Value = "Số dư cuối tháng";
            worksheet.Cells[closingRow, 1].Style.Font.Bold = true;
            worksheet.Cells[closingRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[closingRow, 5].Value = closingBalance >= 0 ? closingBalance : 0;
            worksheet.Cells[closingRow, 6].Value = closingBalance < 0 ? Math.Abs(closingBalance) : 0;

            using (var bodyRange = worksheet.Cells[12, 1, closingRow, 7])
            {
                bodyRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                bodyRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                bodyRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                bodyRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            using (var amountRange = worksheet.Cells[12, 5, closingRow, 6])
            {
                amountRange.Style.Numberformat.Format = "#,##0";
                amountRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                amountRange.Style.Font.Color.SetColor(Color.DarkRed);
            }

            using (var noteRange = worksheet.Cells[12, 1, closingRow, 7])
            {
                noteRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            var footerNow = ConvertToDisplayTime(DateTime.UtcNow);
            var footerRow = closingRow + 2;
            worksheet.Cells[footerRow, 5, footerRow, 7].Merge = true;
            worksheet.Cells[footerRow, 5].Value = $"HCM, Ngày {footerNow:dd} tháng {footerNow:MM} năm {footerNow:yyyy}";
            worksheet.Cells[footerRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[footerRow, 5].Style.Font.Italic = true;

            var signTitleRow = footerRow + 2;
            worksheet.Cells[signTitleRow, 2, signTitleRow, 3].Merge = true;
            worksheet.Cells[signTitleRow, 2].Value = "Kế toán trưởng";
            worksheet.Cells[signTitleRow, 2].Style.Font.Bold = true;
            worksheet.Cells[signTitleRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells[signTitleRow, 5, signTitleRow, 6].Merge = true;
            worksheet.Cells[signTitleRow, 5].Value = "Giám Đốc";
            worksheet.Cells[signTitleRow, 5].Style.Font.Bold = true;
            worksheet.Cells[signTitleRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var signNoteRow = signTitleRow + 1;
            worksheet.Cells[signNoteRow, 2, signNoteRow, 3].Merge = true;
            worksheet.Cells[signNoteRow, 2].Value = "(Ký, họ tên)";
            worksheet.Cells[signNoteRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells[signNoteRow, 5, signNoteRow, 6].Merge = true;
            worksheet.Cells[signNoteRow, 5].Value = "(Ký, họ tên)";
            worksheet.Cells[signNoteRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            return package.GetAsByteArray();
        }

        private string BuildPeriodRangeText(List<CustomerDebtTransaction> transactions)
        {
            if (transactions == null || transactions.Count == 0)
            {
                return "Từ ngày --/--/---- đến --/--/----";
            }

            var fromDate = ConvertToDisplayTime(transactions.Min(x => x.transactionAt));
            var toDate = ConvertToDisplayTime(transactions.Max(x => x.transactionAt));
            return $"Từ ngày {fromDate:dd/MM/yyyy} đến {toDate:dd/MM/yyyy}";
        }

        private string ResolveLedgerAccountCode(CustomerAccount customer)
        {
            var rawCategory = (customer.category ?? string.Empty).Trim();
            if (rawCategory.Contains("131", StringComparison.OrdinalIgnoreCase))
            {
                return "131";
            }

            if (rawCategory.Contains("331", StringComparison.OrdinalIgnoreCase))
            {
                return "331";
            }

            return customer.debtAmount >= customer.creditAmount ? "131" : "331";
        }

        private DateTime ConvertToDisplayTime(DateTime value)
        {
            var timezone = GetDisplayTimeZone();
            var normalized = value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };

            return TimeZoneInfo.ConvertTimeFromUtc(normalized, timezone);
        }

        private TimeZoneInfo GetDisplayTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
            catch
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                }
                catch
                {
                    return TimeZoneInfo.CreateCustomTimeZone("UTC+7", TimeSpan.FromHours(7), "UTC+7", "UTC+7");
                }
            }
        }

        private decimal GetNetBalance(CustomerAccount item)
        {
            return item.debtAmount - item.creditAmount;
        }

        private string GetCurrentActor()
        {
            var user = HttpContext?.Items?["UserAdmin"] as UserAdmin;
            if (user == null)
            {
                return "system";
            }

            if (!string.IsNullOrWhiteSpace(user.Username))
            {
                return user.Username;
            }

            if (!string.IsNullOrWhiteSpace(user.FullName))
            {
                return user.FullName;
            }

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                return user.Email;
            }

            return user.Id ?? "system";
        }

        private CustomerAuditLogEntry CreateAuditLogEntry(
            string customerId,
            string transactionId,
            string action,
            string field,
            string oldValue,
            string newValue,
            string changedBy,
            string note)
        {
            return new CustomerAuditLogEntry
            {
                id = Guid.NewGuid().ToString("N"),
                customerId = customerId,
                transactionId = transactionId,
                action = action,
                field = field,
                oldValue = oldValue,
                newValue = newValue,
                changedAt = DateTime.UtcNow,
                changedBy = changedBy,
                note = note,
            };
        }

        private async Task AppendCustomerAuditLogAsync(CustomerAuditLogEntry entry)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.customerId))
            {
                return;
            }

            await _apiDocumentDbContext.CustomerAuditLogs.InsertOneAsync(entry);
        }

        private async Task AppendCustomerAuditLogsAsync(IEnumerable<CustomerAuditLogEntry> entries)
        {
            var materialized = entries
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.customerId))
                .ToList();

            if (materialized.Count == 0)
            {
                return;
            }

            await _apiDocumentDbContext.CustomerAuditLogs.InsertManyAsync(materialized);
        }

        private async Task<(int migratedTransactions, int migratedAuditLogs, bool clearedLegacy)> MigrateLegacyDataForCustomerAsync(CustomerAccount customer, bool clearLegacy)
        {
            if (customer == null || string.IsNullOrWhiteSpace(customer.Id))
            {
                return (0, 0, false);
            }

            var legacyTransactions = (customer.debtTransactions ?? new List<DebtTransactionRecord>())
                .Where(x => !string.IsNullOrWhiteSpace(x.id))
                .ToList();
            var legacyAuditLogs = (customer.auditLogs ?? new List<CustomerAuditLog>())
                .Where(x => !string.IsNullOrWhiteSpace(x.id))
                .ToList();

            var migratedTransactions = 0;
            var migratedAuditLogs = 0;

            if (legacyTransactions.Count > 0)
            {
                var existingTransactionIds = await _apiDocumentDbContext.CustomerDebtTransactions
                    .Find(x => x.customerId == customer.Id)
                    .Project(x => x.id)
                    .ToListAsync();
                var existingTransactionIdSet = new HashSet<string>(
                    existingTransactionIds.Where(x => !string.IsNullOrWhiteSpace(x)),
                    StringComparer.Ordinal);

                var insertTransactions = legacyTransactions
                    .Where(x => !existingTransactionIdSet.Contains(x.id))
                    .Select(x => new CustomerDebtTransaction
                    {
                        id = x.id,
                        customerId = customer.Id,
                        transactionType = x.transactionType,
                        amount = x.amount,
                        transactionAt = x.transactionAt,
                        note = x.note,
                        createdAt = x.createdAt,
                        createdBy = x.createdBy,
                    })
                    .ToList();

                if (insertTransactions.Count > 0)
                {
                    await _apiDocumentDbContext.CustomerDebtTransactions.InsertManyAsync(insertTransactions);
                    migratedTransactions = insertTransactions.Count;
                }
            }

            if (legacyAuditLogs.Count > 0)
            {
                var existingAuditIds = await _apiDocumentDbContext.CustomerAuditLogs
                    .Find(x => x.customerId == customer.Id)
                    .Project(x => x.id)
                    .ToListAsync();
                var existingAuditIdSet = new HashSet<string>(
                    existingAuditIds.Where(x => !string.IsNullOrWhiteSpace(x)),
                    StringComparer.Ordinal);

                var insertAuditLogs = legacyAuditLogs
                    .Where(x => !existingAuditIdSet.Contains(x.id))
                    .Select(x => new CustomerAuditLogEntry
                    {
                        id = x.id,
                        customerId = customer.Id,
                        transactionId = ExtractTransactionIdFromField(x.field),
                        action = x.action,
                        field = x.field,
                        oldValue = x.oldValue,
                        newValue = x.newValue,
                        changedAt = x.changedAt,
                        changedBy = x.changedBy,
                        note = x.note,
                    })
                    .ToList();

                if (insertAuditLogs.Count > 0)
                {
                    await _apiDocumentDbContext.CustomerAuditLogs.InsertManyAsync(insertAuditLogs);
                    migratedAuditLogs = insertAuditLogs.Count;
                }
            }

            var hasLegacyData = legacyTransactions.Count > 0 || legacyAuditLogs.Count > 0;
            if (!clearLegacy || !hasLegacyData)
            {
                return (migratedTransactions, migratedAuditLogs, false);
            }

            customer.debtTransactions = new List<DebtTransactionRecord>();
            customer.auditLogs = new List<CustomerAuditLog>();
            var customerFilter = Builders<CustomerAccount>.Filter.Eq(x => x.Id, customer.Id);
            await _apiDocumentDbContext.CustomerAccounts.ReplaceOneAsync(customerFilter, customer);

            return (migratedTransactions, migratedAuditLogs, true);
        }

        private string ExtractTransactionIdFromField(string field)
        {
            if (string.IsNullOrWhiteSpace(field))
            {
                return null;
            }

            var prefix = "debtTransaction.";
            if (!field.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var remaining = field.Substring(prefix.Length);
            var separatorIndex = remaining.IndexOf('.');
            if (separatorIndex <= 0)
            {
                return null;
            }

            return remaining.Substring(0, separatorIndex);
        }

        private List<CustomerFieldChange> BuildCustomerChanges(CustomerAccount oldData, CustomerAccount newData)
        {
            var changes = new List<CustomerFieldChange>();
            AddChangeIfDiff(changes, "code", oldData.code, newData.code);
            AddChangeIfDiff(changes, "name", oldData.name, newData.name);
            AddChangeIfDiff(changes, "category", oldData.category, newData.category);
            AddChangeIfDiff(changes, "taxCode", oldData.taxCode, newData.taxCode);
            AddChangeIfDiff(changes, "bankAccount", oldData.bankAccount, newData.bankAccount);
            AddChangeIfDiff(changes, "bankName", oldData.bankName, newData.bankName);
            AddChangeIfDiff(changes, "phone", oldData.phone, newData.phone);
            AddChangeIfDiff(changes, "email", oldData.email, newData.email);
            AddChangeIfDiff(changes, "address", oldData.address, newData.address);
            AddChangeIfDiff(changes, "status", oldData.status, newData.status);
            AddChangeIfDiff(changes, "riskLevel", oldData.riskLevel, newData.riskLevel);
            AddChangeIfDiff(changes, "owner", oldData.owner, newData.owner);
            AddChangeIfDiff(changes, "debtAmount", oldData.debtAmount.ToString(), newData.debtAmount.ToString());
            AddChangeIfDiff(changes, "creditAmount", oldData.creditAmount.ToString(), newData.creditAmount.ToString());
            AddChangeIfDiff(changes, "lastTransactionAt", FormatDateTime(oldData.lastTransactionAt), FormatDateTime(newData.lastTransactionAt));
            AddChangeIfDiff(changes, "tags", JoinTags(oldData.tags), JoinTags(newData.tags));
            return changes;
        }

        private void AddChangeIfDiff(List<CustomerFieldChange> changes, string field, string oldValue, string newValue)
        {
            var oldNorm = NormalizeText(oldValue);
            var newNorm = NormalizeText(newValue);
            if (string.Equals(oldNorm, newNorm, StringComparison.Ordinal))
            {
                return;
            }

            changes.Add(new CustomerFieldChange
            {
                field = field,
                oldValue = oldNorm,
                newValue = newNorm,
            });
        }

        private string NormalizeText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }

        private string JoinTags(IList<string> tags)
        {
            if (tags == null || tags.Count == 0)
            {
                return null;
            }

            return string.Join(",", tags.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()));
        }

        private string BuildRecordSnapshot(CustomerAccount data)
        {
            return $"{data.code}|{data.name}|{data.status}|{data.riskLevel}";
        }

        private string FormatDateTime(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("o") : null;
        }

        private int GetAgingDays(DateTime? lastTransactionAt)
        {
            if (!lastTransactionAt.HasValue)
            {
                return 999;
            }

            var delta = DateTime.UtcNow.Date - lastTransactionAt.Value.Date;
            return delta.Days < 0 ? 0 : delta.Days;
        }

        private string GetAgingBucket(int agingDays)
        {
            if (agingDays <= 30)
            {
                return "0-30";
            }

            if (agingDays <= 60)
            {
                return "31-60";
            }

            if (agingDays <= 90)
            {
                return "61-90";
            }

            return ">90";
        }

        private List<DebtListItemDto> SortDebtItems(List<DebtListItemDto> items, string sortBy, string sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return sortBy?.ToLowerInvariant() switch
            {
                "name" => isDesc ? items.OrderByDescending(x => x.name).ToList() : items.OrderBy(x => x.name).ToList(),
                "status" => isDesc ? items.OrderByDescending(x => x.status).ToList() : items.OrderBy(x => x.status).ToList(),
                "risklevel" => isDesc ? items.OrderByDescending(x => x.riskLevel).ToList() : items.OrderBy(x => x.riskLevel).ToList(),
                "agingdays" => isDesc ? items.OrderByDescending(x => x.agingDays).ToList() : items.OrderBy(x => x.agingDays).ToList(),
                "lasttransactionat" => isDesc ? items.OrderByDescending(x => x.lastTransactionAt).ToList() : items.OrderBy(x => x.lastTransactionAt).ToList(),
                _ => isDesc ? items.OrderByDescending(x => x.netBalance).ToList() : items.OrderBy(x => x.netBalance).ToList(),
            };
        }

        private List<DebtTransactionListItem> SortDebtTransactions(List<DebtTransactionListItem> items, string sortBy, string sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return sortBy?.ToLowerInvariant() switch
            {
                "customername" => isDesc ? items.OrderByDescending(x => x.customerName).ToList() : items.OrderBy(x => x.customerName).ToList(),
                "transactiontype" => isDesc ? items.OrderByDescending(x => x.transactionType).ToList() : items.OrderBy(x => x.transactionType).ToList(),
                "amount" => isDesc ? items.OrderByDescending(x => x.amount).ToList() : items.OrderBy(x => x.amount).ToList(),
                "createdby" => isDesc ? items.OrderByDescending(x => x.createdBy).ToList() : items.OrderBy(x => x.createdBy).ToList(),
                _ => isDesc ? items.OrderByDescending(x => x.transactionAt).ToList() : items.OrderBy(x => x.transactionAt).ToList(),
            };
        }

        private bool ContainsText(string source, string keyword)
        {
            return !string.IsNullOrWhiteSpace(source)
                && !string.IsNullOrWhiteSpace(keyword)
                && source.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string GetBsonString(BsonDocument source, string key)
        {
            if (source == null || string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            if (!source.TryGetValue(key, out var value) || value == null || value.IsBsonNull)
            {
                return string.Empty;
            }

            return value.ToString().Trim();
        }

        private static string GetBsonId(BsonDocument source)
        {
            if (source == null || !source.TryGetValue("_id", out var value) || value == null || value.IsBsonNull)
            {
                return string.Empty;
            }

            return value.BsonType == BsonType.ObjectId
                ? value.AsObjectId.ToString()
                : value.ToString().Trim();
        }

        private static DateTime? GetBsonDateTime(BsonDocument source, string key)
        {
            if (source == null || string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            if (!source.TryGetValue(key, out var value) || value == null || value.IsBsonNull)
            {
                return null;
            }

            if (value.BsonType == BsonType.DateTime)
            {
                return value.ToUniversalTime();
            }

            var text = value.ToString();
            if (DateTime.TryParse(text, out var parsed))
            {
                return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            }

            return null;
        }

        private static FilterDefinition<BsonDocument> BuildBsonIdFilter(string id)
        {
            var normalized = (id ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return null;
            }

            if (ObjectId.TryParse(normalized, out var objectId))
            {
                return Builders<BsonDocument>.Filter.Or(
                    Builders<BsonDocument>.Filter.Eq("_id", objectId),
                    Builders<BsonDocument>.Filter.Eq("_id", normalized));
            }

            return Builders<BsonDocument>.Filter.Eq("_id", normalized);
        }

        private static string NormalizeAccountType(string value)
        {
            var trimmed = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return string.Empty;
            }

            return string.Concat(trimmed.Where(ch => !char.IsWhiteSpace(ch)));
        }

        private class DebtListItemDto
        {
            public string id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public string phone { get; set; }
            public string status { get; set; }
            public string riskLevel { get; set; }
            public decimal debtAmount { get; set; }
            public decimal creditAmount { get; set; }
            public decimal netBalance { get; set; }
            public int agingDays { get; set; }
            public string agingBucket { get; set; }
            public DateTime? lastTransactionAt { get; set; }
            public DateTime updatedAt { get; set; }
        }

        public class CreateDebtTransactionRequest
        {
            public string customerId { get; set; }
            public string transactionType { get; set; }
            public decimal amount { get; set; }
            public DateTime? transactionAt { get; set; }
            public string note { get; set; }
        }

        public class UpdateDebtTransactionRequest
        {
            public string transactionType { get; set; }
            public decimal amount { get; set; }
            public DateTime? transactionAt { get; set; }
            public string note { get; set; }
        }

        private class DebtTransactionListItem
        {
            public string id { get; set; }
            public string customerId { get; set; }
            public string customerCode { get; set; }
            public string customerName { get; set; }
            public string transactionType { get; set; }
            public decimal amount { get; set; }
            public DateTime transactionAt { get; set; }
            public string note { get; set; }
            public DateTime createdAt { get; set; }
            public string createdBy { get; set; }
        }

        private class CustomerFieldChange
        {
            public string field { get; set; }
            public string oldValue { get; set; }
            public string newValue { get; set; }
        }

        private string NormalizeBalanceSide(string value)
        {
            var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            return normalized switch
            {
                "debit" => "debit",
                "credit" => "credit",
                _ => string.Empty,
            };
        }

        public class AccountTypeConfigUpsertRequest
        {
            public string id { get; set; }
            public string accountType { get; set; }
            public string accountName { get; set; }
            public string accountNameLocal { get; set; }
            public string balanceSide { get; set; }
        }

    }
}
