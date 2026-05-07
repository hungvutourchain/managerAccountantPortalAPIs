using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using B2BAdmin.ApiDocument.Domains.Models;
using B2BAdmin.ApiDocument.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

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
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] string? riskLevel,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "updatedAt",
            [FromQuery] string sortDirection = "desc")
        {
            await EnsureSeedDataAsync();

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
            await EnsureSeedDataAsync();

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

            payload.updatedAt = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(payload.Id))
            {
                payload.createdAt = DateTime.UtcNow;
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
            await _apiDocumentDbContext.CustomerAccounts.ReplaceOneAsync(filter, payload);

            return Ok(payload);
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

        private async Task EnsureSeedDataAsync()
        {
            var hasData = await _apiDocumentDbContext.CustomerAccounts.Find(Builders<CustomerAccount>.Filter.Empty).Limit(1).AnyAsync();
            if (hasData)
            {
                return;
            }

            var seed = new List<CustomerAccount>
            {
                new CustomerAccount { code = "331-VP", name = "Cong ty TNHH TM XNK Vui Phat", category = "331", taxCode = "0316302293", bankAccount = "281213668", bankName = "ACB", phone = "0938937319", email = "ap@vuiphat.vn", address = "So 41/3 Ap Dong Lan, TP. Ho Chi Minh", debtAmount = 280800000, creditAmount = 0, status = "active", riskLevel = "normal", owner = "Accounting Team", tags = new List<string> { "priority", "supplier" }, lastTransactionAt = DateTime.UtcNow.AddDays(-2) },
                new CustomerAccount { code = "KH-002", name = "Nha cung cap Minh Anh", category = "331", taxCode = "0309981123", bankAccount = "0123456789", bankName = "Vietcombank", phone = "0901234567", email = "lienhe@minhanh.vn", address = "Quan 3, TP. Ho Chi Minh", debtAmount = 120500000, creditAmount = 20000000, status = "active", riskLevel = "warning", owner = "Ms. Trang", tags = new List<string> { "overdue" }, lastTransactionAt = DateTime.UtcNow.AddDays(-12) },
                new CustomerAccount { code = "KH-003", name = "CTCP Logistics Sao Bac", category = "131", taxCode = "0201234567", bankAccount = "789654123", bankName = "BIDV", phone = "0911222333", email = "finance@saobac.vn", address = "Hai Phong", debtAmount = 0, creditAmount = 84000000, status = "active", riskLevel = "normal", owner = "Mr. Long", tags = new List<string> { "new" }, lastTransactionAt = DateTime.UtcNow.AddDays(-1) },
                new CustomerAccount { code = "KH-004", name = "Cong ty Du lich Dat Viet", category = "131", taxCode = "0311112233", bankAccount = "4455667788", bankName = "Techcombank", phone = "0988111000", email = "accounting@datviet.vn", address = "Da Nang", debtAmount = 64000000, creditAmount = 4000000, status = "blocked", riskLevel = "critical", owner = "Credit Control", tags = new List<string> { "high-risk" }, lastTransactionAt = DateTime.UtcNow.AddDays(-45) }
            };

            await _apiDocumentDbContext.CustomerAccounts.InsertManyAsync(seed);
        }
    }
}
