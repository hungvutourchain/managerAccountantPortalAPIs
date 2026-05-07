using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using B2BAdmin.ApiDocument.Domains.Models;
using B2BAdmin.ApiDocument.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace ApiPlugin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerManagementController : ControllerBase
    {
        private readonly ApiDocumentDbContext _apiDocumentDbContext;

        public CustomerManagementController(ApiDocumentDbContext apiDocumentDbContext)
        {
            _apiDocumentDbContext = apiDocumentDbContext;
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

            var finalFilter = filters.Count > 0 ? filterBuilder.And(filters) : filterBuilder.Empty;

            var sortDefinition = BuildSort(sortBy, sortDirection);
            var totalItems = await _apiDocumentDbContext.CustomerAccounts.CountDocumentsAsync(finalFilter);

            var items = await _apiDocumentDbContext.CustomerAccounts
                .Find(finalFilter)
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
                items
            });
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummaryAsync()
        {
            var data = await _apiDocumentDbContext.CustomerAccounts.Find(Builders<CustomerAccount>.Filter.Empty).ToListAsync();

            var totalCustomers = data.Count;
            var activeCustomers = data.Count(x => string.Equals(x.status, "active", StringComparison.OrdinalIgnoreCase));
            var warningCustomers = data.Count(x => string.Equals(x.riskLevel, "warning", StringComparison.OrdinalIgnoreCase));
            var blockedCustomers = data.Count(x => string.Equals(x.status, "blocked", StringComparison.OrdinalIgnoreCase));
            var totalDebt = data.Sum(x => x.debtAmount);
            var totalCredit = data.Sum(x => x.creditAmount);

            return Ok(new
            {
                totalCustomers,
                activeCustomers,
                warningCustomers,
                blockedCustomers,
                totalDebt,
                totalCredit
            });
        }

        [HttpPost("customers")]
        public async Task<IActionResult> UpsertCustomerAsync([FromBody] CustomerAccount payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.name))
            {
                return BadRequest("Invalid payload");
            }

            var now = DateTime.UtcNow;
            var actor = GetCurrentActor();
            payload.updatedAt = now;
            payload.updatedBy = actor;

            if (string.IsNullOrWhiteSpace(payload.Id))
            {
                payload.createdAt = now;
                payload.createdBy = actor;
                payload.debtTransactions ??= new List<DebtTransactionRecord>();
                payload.auditLogs ??= new List<CustomerAuditLog>();
                AddAuditLog(payload, "create", "record", null, BuildRecordSnapshot(payload), actor, "Customer created");
                await _apiDocumentDbContext.CustomerAccounts.InsertOneAsync(payload);
                return Ok(payload);
            }

            var filter = Builders<CustomerAccount>.Filter.Eq(x => x.Id, payload.Id);
            var existing = await _apiDocumentDbContext.CustomerAccounts.Find(filter).FirstOrDefaultAsync();

            if (existing == null)
            {
                return NotFound();
            }

            payload.createdAt = existing.createdAt;
            payload.createdBy = existing.createdBy;
            payload.debtTransactions = existing.debtTransactions ?? new List<DebtTransactionRecord>();
            payload.auditLogs = existing.auditLogs ?? new List<CustomerAuditLog>();

            var changes = BuildCustomerChanges(existing, payload);
            foreach (var change in changes)
            {
                AddAuditLog(payload, "update", change.field, change.oldValue, change.newValue, actor, null);
            }

            if (changes.Count == 0)
            {
                AddAuditLog(payload, "touch", "record", null, null, actor, "No field value changed");
            }

            await _apiDocumentDbContext.CustomerAccounts.ReplaceOneAsync(filter, payload);

            return Ok(payload);
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

            if (customer == null)
            {
                return NotFound();
            }

            var logs = (customer.auditLogs ?? new List<CustomerAuditLog>())
                .OrderByDescending(x => x.changedAt)
                .ToList();

            var totalItems = logs.Count;
            var items = logs
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

        [HttpDelete("customers/{id}")]
        public async Task<IActionResult> DeleteCustomerAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Invalid id");
            }

            var filter = Builders<CustomerAccount>.Filter.Eq(x => x.Id, id);
            var deleteResult = await _apiDocumentDbContext.CustomerAccounts.DeleteOneAsync(filter);

            if (deleteResult.DeletedCount == 0)
            {
                return NotFound();
            }

            return Ok(new { success = true });
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

            if (customer.debtTransactions == null)
            {
                customer.debtTransactions = new List<DebtTransactionRecord>();
            }

            if (customer.auditLogs == null)
            {
                customer.auditLogs = new List<CustomerAuditLog>();
            }

            var actor = GetCurrentActor();
            var oldDebtAmount = customer.debtAmount;
            var oldCreditAmount = customer.creditAmount;
            var oldLastTransactionAt = customer.lastTransactionAt;
            var oldTransactionCount = customer.debtTransactions.Count;

            var transactionAt = payload.transactionAt ?? DateTime.UtcNow;
            var transaction = new DebtTransactionRecord
            {
                id = Guid.NewGuid().ToString("N"),
                transactionType = transactionType,
                amount = payload.amount,
                transactionAt = transactionAt,
                note = payload.note,
                createdAt = DateTime.UtcNow,
                createdBy = actor,
            };

            customer.debtTransactions.Add(transaction);
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

            AddAuditLog(customer, "transaction-create", $"debtTransaction.{transaction.id}.transactionType", null, transaction.transactionType, actor, payload.note);
            AddAuditLog(customer, "transaction-create", $"debtTransaction.{transaction.id}.amount", null, transaction.amount.ToString(), actor, payload.note);
            AddAuditLog(customer, "transaction-create", $"debtTransaction.{transaction.id}.transactionAt", null, FormatDateTime(transaction.transactionAt), actor, payload.note);
            AddAuditLog(customer, "transaction-create", $"debtTransaction.{transaction.id}.note", null, NormalizeText(transaction.note), actor, payload.note);
            AddAuditLog(customer, "transaction", "debtTransactions.count", oldTransactionCount.ToString(), customer.debtTransactions.Count.ToString(), actor, payload.note);
            if (oldDebtAmount != customer.debtAmount)
            {
                AddAuditLog(customer, "transaction", "debtAmount", oldDebtAmount.ToString(), customer.debtAmount.ToString(), actor, payload.note);
            }

            if (oldCreditAmount != customer.creditAmount)
            {
                AddAuditLog(customer, "transaction", "creditAmount", oldCreditAmount.ToString(), customer.creditAmount.ToString(), actor, payload.note);
            }

            AddAuditLog(customer, "transaction", "lastTransactionAt", FormatDateTime(oldLastTransactionAt), FormatDateTime(customer.lastTransactionAt), actor, payload.note);

            await _apiDocumentDbContext.CustomerAccounts.ReplaceOneAsync(filter, customer);

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

            var filter = Builders<CustomerAccount>.Filter.ElemMatch(x => x.debtTransactions, t => t.id == transactionId);
            var customer = await _apiDocumentDbContext.CustomerAccounts.Find(filter).FirstOrDefaultAsync();

            if (customer == null)
            {
                return NotFound("Transaction not found");
            }

            customer.debtTransactions ??= new List<DebtTransactionRecord>();
            customer.auditLogs ??= new List<CustomerAuditLog>();

            var transaction = customer.debtTransactions.FirstOrDefault(x => string.Equals(x.id, transactionId, StringComparison.Ordinal));
            if (transaction == null)
            {
                return NotFound("Transaction not found");
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

            customer.lastTransactionAt = customer.debtTransactions.Count == 0
                ? (DateTime?)null
                : customer.debtTransactions.Max(x => x.transactionAt);
            customer.updatedAt = DateTime.UtcNow;
            customer.updatedBy = actor;

            AddAuditLog(customer, "transaction-update", $"debtTransaction.{transactionId}.transactionType", NormalizeText(oldType), NormalizeText(transaction.transactionType), actor, payload.note);
            AddAuditLog(customer, "transaction-update", $"debtTransaction.{transactionId}.amount", oldAmount.ToString(), transaction.amount.ToString(), actor, payload.note);
            AddAuditLog(customer, "transaction-update", $"debtTransaction.{transactionId}.transactionAt", FormatDateTime(oldTransactionAt), FormatDateTime(transaction.transactionAt), actor, payload.note);
            AddAuditLog(customer, "transaction-update", $"debtTransaction.{transactionId}.note", NormalizeText(oldNote), NormalizeText(transaction.note), actor, payload.note);

            if (oldDebtAmount != customer.debtAmount)
            {
                AddAuditLog(customer, "transaction", "debtAmount", oldDebtAmount.ToString(), customer.debtAmount.ToString(), actor, payload.note);
            }

            if (oldCreditAmount != customer.creditAmount)
            {
                AddAuditLog(customer, "transaction", "creditAmount", oldCreditAmount.ToString(), customer.creditAmount.ToString(), actor, payload.note);
            }

            AddAuditLog(customer, "transaction", "lastTransactionAt", FormatDateTime(oldLastTransactionAt), FormatDateTime(customer.lastTransactionAt), actor, payload.note);

            var customerFilter = Builders<CustomerAccount>.Filter.Eq(x => x.Id, customer.Id);
            await _apiDocumentDbContext.CustomerAccounts.ReplaceOneAsync(customerFilter, customer);

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

            var filter = Builders<CustomerAccount>.Filter.ElemMatch(x => x.debtTransactions, t => t.id == transactionId);
            var customer = await _apiDocumentDbContext.CustomerAccounts.Find(filter).FirstOrDefaultAsync();
            if (customer == null)
            {
                return NotFound("Transaction not found");
            }

            var fieldPrefix = $"debtTransaction.{transactionId}.";
            var logs = (customer.auditLogs ?? new List<CustomerAuditLog>())
                .Where(x => !string.IsNullOrWhiteSpace(x.field) && x.field.StartsWith(fieldPrefix, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.changedAt)
                .ToList();

            var totalItems = logs.Count;
            var items = logs
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

            var filterBuilder = Builders<CustomerAccount>.Filter;
            FilterDefinition<CustomerAccount> filter = filterBuilder.Empty;

            if (!string.IsNullOrWhiteSpace(customerId))
            {
                filter = filterBuilder.Eq(x => x.Id, customerId);
            }

            var customers = await _apiDocumentDbContext.CustomerAccounts
                .Find(filter)
                .ToListAsync();

            var transactions = customers
                .SelectMany(customer => (customer.debtTransactions ?? new List<DebtTransactionRecord>())
                    .Select(t => new DebtTransactionListItem
                    {
                        id = t.id,
                        customerId = customer.Id,
                        customerCode = customer.code,
                        customerName = customer.name,
                        transactionType = t.transactionType,
                        amount = t.amount,
                        transactionAt = t.transactionAt,
                        note = t.note,
                        createdAt = t.createdAt,
                        createdBy = t.createdBy,
                    }))
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
        public async Task<IActionResult> ExportDebtCustomerExcelAsync(string customerId)
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

            var fileBytes = BuildDebtCustomerExcelFile(customer);
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

        private async Task<List<CustomerAccount>> GetFilteredDebtSourceAsync(string search, string status, string riskLevel)
        {
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

            if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
            {
                filters.Add(filterBuilder.Eq(x => x.status, status));
            }

            if (!string.IsNullOrWhiteSpace(riskLevel) && !string.Equals(riskLevel, "all", StringComparison.OrdinalIgnoreCase))
            {
                filters.Add(filterBuilder.Eq(x => x.riskLevel, riskLevel));
            }

            var finalFilter = filters.Count > 0 ? filterBuilder.And(filters) : filterBuilder.Empty;

            return await _apiDocumentDbContext.CustomerAccounts
                .Find(finalFilter)
                .ToListAsync();
        }

        private byte[] BuildDebtCustomerExcelFile(CustomerAccount customer)
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

            var transactions = (customer.debtTransactions ?? new List<DebtTransactionRecord>())
                .OrderBy(x => x.transactionAt)
                .ToList();
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
            worksheet.Cells[8, 2].Value = "CÔNG TY TNHH TM XNK VUI PHAT";
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

        private string BuildPeriodRangeText(List<DebtTransactionRecord> transactions)
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

        private void AddAuditLog(
            CustomerAccount customer,
            string action,
            string field,
            string oldValue,
            string newValue,
            string changedBy,
            string note)
        {
            customer.auditLogs ??= new List<CustomerAuditLog>();
            customer.auditLogs.Add(new CustomerAuditLog
            {
                id = Guid.NewGuid().ToString("N"),
                action = action,
                field = field,
                oldValue = oldValue,
                newValue = newValue,
                changedAt = DateTime.UtcNow,
                changedBy = changedBy,
                note = note,
            });
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

    }
}
