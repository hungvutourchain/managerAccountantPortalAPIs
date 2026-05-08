using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using B2BAdmin.ApiDocument.API.Services;
using B2BAdmin.ApiDocument.Domains.Models;
using B2BAdmin.ApiDocument.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace B2BAdmin.ApiDocument.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionManagementController : ControllerBase
    {
        private readonly ApiDocumentDbContext _apiDocumentDbContext;
        private readonly IDebtAiService _debtAiService;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public TransactionManagementController(ApiDocumentDbContext apiDocumentDbContext, IDebtAiService debtAiService, IHostingEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _apiDocumentDbContext = apiDocumentDbContext;
            _debtAiService = debtAiService;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        [HttpPost("transactions")]
        public async Task<IActionResult> AddDebtTransactionAsync([FromBody] CreateDebtTransactionRequest payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.customerId))
            {
                return BadRequest("Invalid payload");
            }

            if (string.IsNullOrWhiteSpace(payload.contractCode))
            {
                return BadRequest("contractCode is required");
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

            if (customer == null || customer.isDeleted)
            {
                return NotFound("Customer not found");
            }

            await MigrateLegacyDataForCustomerAsync(customer, true);

            var actor = GetCurrentActor();
            var oldDebtAmount = customer.debtAmount;
            var oldCreditAmount = customer.creditAmount;
            var oldLastTransactionAt = customer.lastTransactionAt;
            var transactionCountFilter = Builders<CustomerDebtTransaction>.Filter.And(
                Builders<CustomerDebtTransaction>.Filter.Eq(x => x.customerId, customer.Id),
                Builders<CustomerDebtTransaction>.Filter.Eq(x => x.isDeleted, false));
            var oldTransactionCount = (int)await _apiDocumentDbContext.CustomerDebtTransactions.CountDocumentsAsync(transactionCountFilter);

            var transactionAt = payload.transactionAt ?? DateTime.UtcNow;
            var transaction = new CustomerDebtTransaction
            {
                id = Guid.NewGuid().ToString("N"),
                customerId = customer.Id,
                transactionType = transactionType,
                amount = payload.amount,
                transactionAt = transactionAt,
                contractCode = NormalizeText(payload.contractCode),
                note = payload.note,
                createdAt = DateTime.UtcNow,
                createdBy = actor,
                attachments = new List<DebtTransactionAttachment>(),
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
                CreateAuditLogEntry(customer.Id, transaction.id, "transaction-create", $"debtTransaction.{transaction.id}.contractCode", null, NormalizeText(transaction.contractCode), actor, payload.note),
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
                    contractCode = transaction.contractCode,
                    note = transaction.note,
                    attachments = transaction.attachments ?? new List<DebtTransactionAttachment>(),
                    createdAt = transaction.createdAt,
                    createdBy = transaction.createdBy,
                }
            });
        }

        [HttpPut("transactions/{transactionId}")]
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

            if (string.IsNullOrWhiteSpace(payload.contractCode))
            {
                return BadRequest("contractCode is required");
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
                if (legacyCustomer == null || legacyCustomer.isDeleted)
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

            var sourceCustomerId = transaction.customerId;
            var nextCustomerId = string.IsNullOrWhiteSpace(payload.customerId)
                ? sourceCustomerId
                : payload.customerId.Trim();
            if (string.IsNullOrWhiteSpace(nextCustomerId))
            {
                return BadRequest("customerId is required");
            }

            var sourceCustomerFilter = Builders<CustomerAccount>.Filter.Eq(x => x.Id, sourceCustomerId);
            var sourceCustomer = await _apiDocumentDbContext.CustomerAccounts.Find(sourceCustomerFilter).FirstOrDefaultAsync();
            if (sourceCustomer == null || sourceCustomer.isDeleted)
            {
                return NotFound("Customer not found");
            }

            var customerChanged = !string.Equals(sourceCustomerId, nextCustomerId, StringComparison.Ordinal);
            var targetCustomer = sourceCustomer;
            FilterDefinition<CustomerAccount> targetCustomerFilter = sourceCustomerFilter;
            if (customerChanged)
            {
                targetCustomerFilter = Builders<CustomerAccount>.Filter.Eq(x => x.Id, nextCustomerId);
                targetCustomer = await _apiDocumentDbContext.CustomerAccounts.Find(targetCustomerFilter).FirstOrDefaultAsync();
                if (targetCustomer == null || targetCustomer.isDeleted)
                {
                    return NotFound("Target customer not found");
                }
            }

            var actor = GetCurrentActor();
            var oldType = transaction.transactionType;
            var oldAmount = transaction.amount;
            var oldTransactionAt = transaction.transactionAt;
            var oldContractCode = transaction.contractCode;
            var oldNote = transaction.note;
            var oldCustomerId = sourceCustomerId;

            var oldSourceDebtAmount = sourceCustomer.debtAmount;
            var oldSourceCreditAmount = sourceCustomer.creditAmount;
            var oldSourceLastTransactionAt = sourceCustomer.lastTransactionAt;

            var oldTargetDebtAmount = targetCustomer.debtAmount;
            var oldTargetCreditAmount = targetCustomer.creditAmount;
            var oldTargetLastTransactionAt = targetCustomer.lastTransactionAt;

            var nextTransactionAt = payload.transactionAt ?? transaction.transactionAt;

            if (string.Equals(oldType, "debt", StringComparison.OrdinalIgnoreCase))
            {
                sourceCustomer.debtAmount -= oldAmount;
            }
            else
            {
                sourceCustomer.creditAmount -= oldAmount;
            }

            if (string.Equals(transactionType, "debt", StringComparison.OrdinalIgnoreCase))
            {
                targetCustomer.debtAmount += payload.amount;
            }
            else
            {
                targetCustomer.creditAmount += payload.amount;
            }

            transaction.customerId = targetCustomer.Id;
            transaction.transactionType = transactionType;
            transaction.amount = payload.amount;
            transaction.transactionAt = nextTransactionAt;
            transaction.contractCode = NormalizeText(payload.contractCode);
            transaction.note = payload.note;

            var sourceOtherTransactionDates = await _apiDocumentDbContext.CustomerDebtTransactions
                .Find(x => x.customerId == sourceCustomer.Id && x.id != transactionId && !x.isDeleted)
                .Project(x => x.transactionAt)
                .ToListAsync();

            if (customerChanged)
            {
                sourceCustomer.lastTransactionAt = sourceOtherTransactionDates.Count == 0
                    ? default
                    : sourceOtherTransactionDates.Max();
            }
            else
            {
                sourceCustomer.lastTransactionAt = sourceOtherTransactionDates.Count == 0
                    ? nextTransactionAt
                    : new[] { nextTransactionAt, sourceOtherTransactionDates.Max() }.Max();
            }

            var targetOtherTransactionDates = customerChanged
                ? await _apiDocumentDbContext.CustomerDebtTransactions
                    .Find(x => x.customerId == targetCustomer.Id && x.id != transactionId && !x.isDeleted)
                    .Project(x => x.transactionAt)
                    .ToListAsync()
                : sourceOtherTransactionDates;

            targetCustomer.lastTransactionAt = targetOtherTransactionDates.Count == 0
                ? nextTransactionAt
                : new[] { nextTransactionAt, targetOtherTransactionDates.Max() }.Max();

            sourceCustomer.updatedAt = DateTime.UtcNow;
            sourceCustomer.updatedBy = actor;

            if (customerChanged)
            {
                targetCustomer.updatedAt = DateTime.UtcNow;
                targetCustomer.updatedBy = actor;
            }

            transaction.updatedAt = DateTime.UtcNow;
            transaction.updatedBy = actor;

            await _apiDocumentDbContext.CustomerDebtTransactions.ReplaceOneAsync(transactionFilter, transaction);
            await _apiDocumentDbContext.CustomerAccounts.ReplaceOneAsync(sourceCustomerFilter, sourceCustomer);
            if (customerChanged)
            {
                await _apiDocumentDbContext.CustomerAccounts.ReplaceOneAsync(targetCustomerFilter, targetCustomer);
            }

            var logs = new List<CustomerAuditLogEntry>
            {
                CreateAuditLogEntry(targetCustomer.Id, transactionId, "transaction-update", $"debtTransaction.{transactionId}.transactionType", NormalizeText(oldType), NormalizeText(transaction.transactionType), actor, payload.note),
                CreateAuditLogEntry(targetCustomer.Id, transactionId, "transaction-update", $"debtTransaction.{transactionId}.amount", oldAmount.ToString(), transaction.amount.ToString(), actor, payload.note),
                CreateAuditLogEntry(targetCustomer.Id, transactionId, "transaction-update", $"debtTransaction.{transactionId}.transactionAt", FormatDateTime(oldTransactionAt), FormatDateTime(transaction.transactionAt), actor, payload.note),
                CreateAuditLogEntry(targetCustomer.Id, transactionId, "transaction-update", $"debtTransaction.{transactionId}.contractCode", NormalizeText(oldContractCode), NormalizeText(transaction.contractCode), actor, payload.note),
                CreateAuditLogEntry(targetCustomer.Id, transactionId, "transaction-update", $"debtTransaction.{transactionId}.note", NormalizeText(oldNote), NormalizeText(transaction.note), actor, payload.note),
            };

            if (!string.Equals(oldCustomerId, transaction.customerId, StringComparison.Ordinal))
            {
                logs.Add(CreateAuditLogEntry(targetCustomer.Id, transactionId, "transaction-update", $"debtTransaction.{transactionId}.customerId", NormalizeText(oldCustomerId), NormalizeText(transaction.customerId), actor, payload.note));
            }

            if (oldSourceDebtAmount != sourceCustomer.debtAmount)
            {
                logs.Add(CreateAuditLogEntry(sourceCustomer.Id, null, "transaction", "debtAmount", oldSourceDebtAmount.ToString(), sourceCustomer.debtAmount.ToString(), actor, payload.note));
            }

            if (oldSourceCreditAmount != sourceCustomer.creditAmount)
            {
                logs.Add(CreateAuditLogEntry(sourceCustomer.Id, null, "transaction", "creditAmount", oldSourceCreditAmount.ToString(), sourceCustomer.creditAmount.ToString(), actor, payload.note));
            }

            logs.Add(CreateAuditLogEntry(sourceCustomer.Id, null, "transaction", "lastTransactionAt", FormatDateTime(oldSourceLastTransactionAt), FormatDateTime(sourceCustomer.lastTransactionAt), actor, payload.note));

            if (customerChanged)
            {
                if (oldTargetDebtAmount != targetCustomer.debtAmount)
                {
                    logs.Add(CreateAuditLogEntry(targetCustomer.Id, null, "transaction", "debtAmount", oldTargetDebtAmount.ToString(), targetCustomer.debtAmount.ToString(), actor, payload.note));
                }

                if (oldTargetCreditAmount != targetCustomer.creditAmount)
                {
                    logs.Add(CreateAuditLogEntry(targetCustomer.Id, null, "transaction", "creditAmount", oldTargetCreditAmount.ToString(), targetCustomer.creditAmount.ToString(), actor, payload.note));
                }

                logs.Add(CreateAuditLogEntry(targetCustomer.Id, null, "transaction", "lastTransactionAt", FormatDateTime(oldTargetLastTransactionAt), FormatDateTime(targetCustomer.lastTransactionAt), actor, payload.note));
            }

            await AppendCustomerAuditLogsAsync(logs);

            return Ok(new
            {
                success = true,
                customerId = targetCustomer.Id,
                debtAmount = targetCustomer.debtAmount,
                creditAmount = targetCustomer.creditAmount,
                netBalance = GetNetBalance(targetCustomer),
                transaction
            });
        }

        [HttpPost("transactions/{transactionId}/attachments")]
        public async Task<IActionResult> UploadDebtTransactionAttachmentsAsync(string transactionId, [FromForm] List<IFormFile> files)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
            {
                return BadRequest("Invalid transactionId");
            }

            var uploadFiles = (files ?? new List<IFormFile>())
                .Where(x => x != null && x.Length > 0)
                .ToList();
            if (uploadFiles.Count == 0)
            {
                return BadRequest("No files uploaded");
            }

            var transactionFilter = Builders<CustomerDebtTransaction>.Filter.Eq(x => x.id, transactionId);
            var transaction = await _apiDocumentDbContext.CustomerDebtTransactions.Find(transactionFilter).FirstOrDefaultAsync();
            if (transaction == null || transaction.isDeleted)
            {
                return NotFound("Transaction not found");
            }

            transaction.attachments ??= new List<DebtTransactionAttachment>();
            var actor = GetCurrentActor();
            var attachmentDirectory = EnsureTransactionAttachmentDirectory(transactionId);
            var addedAttachments = new List<DebtTransactionAttachment>();

            foreach (var file in uploadFiles)
            {
                var attachmentId = Guid.NewGuid().ToString("N");
                var safeOriginalName = GetSafeOriginalFileName(file.FileName);
                var extension = Path.GetExtension(safeOriginalName);
                var storedFileName = string.IsNullOrWhiteSpace(extension)
                    ? attachmentId
                    : $"{attachmentId}{extension}";
                var absolutePath = Path.Combine(attachmentDirectory, storedFileName);

                await using (var stream = new FileStream(absolutePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await file.CopyToAsync(stream);
                }

                var attachment = new DebtTransactionAttachment
                {
                    id = attachmentId,
                    fileName = safeOriginalName,
                    storedFileName = storedFileName,
                    relativePath = BuildTransactionAttachmentRelativePath(transactionId, storedFileName),
                    contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                    size = file.Length,
                    uploadedAt = DateTime.UtcNow,
                    uploadedBy = actor,
                };

                transaction.attachments.Add(attachment);
                addedAttachments.Add(attachment);
            }

            transaction.updatedAt = DateTime.UtcNow;
            transaction.updatedBy = actor;
            await _apiDocumentDbContext.CustomerDebtTransactions.ReplaceOneAsync(transactionFilter, transaction);

            await AppendCustomerAuditLogsAsync(addedAttachments.Select(attachment =>
                CreateAuditLogEntry(
                    transaction.customerId,
                    transaction.id,
                    "transaction-update",
                    $"debtTransaction.{transaction.id}.attachment.{attachment.id}",
                    null,
                    attachment.fileName,
                    actor,
                    $"Uploaded file: {attachment.fileName}")));

            return Ok(new
            {
                success = true,
                attachments = transaction.attachments
            });
        }

        [HttpDelete("transactions/{transactionId}/attachments/{attachmentId}")]
        public async Task<IActionResult> DeleteDebtTransactionAttachmentAsync(string transactionId, string attachmentId)
        {
            if (string.IsNullOrWhiteSpace(transactionId) || string.IsNullOrWhiteSpace(attachmentId))
            {
                return BadRequest("Invalid request");
            }

            var transactionFilter = Builders<CustomerDebtTransaction>.Filter.Eq(x => x.id, transactionId);
            var transaction = await _apiDocumentDbContext.CustomerDebtTransactions.Find(transactionFilter).FirstOrDefaultAsync();
            if (transaction == null || transaction.isDeleted)
            {
                return NotFound("Transaction not found");
            }

            transaction.attachments ??= new List<DebtTransactionAttachment>();
            var attachment = transaction.attachments.FirstOrDefault(x => string.Equals(x.id, attachmentId, StringComparison.Ordinal));
            if (attachment == null)
            {
                return NotFound("Attachment not found");
            }

            var filePath = ResolveAttachmentAbsolutePath(transactionId, attachment);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            transaction.attachments = transaction.attachments
                .Where(x => !string.Equals(x.id, attachmentId, StringComparison.Ordinal))
                .ToList();
            transaction.updatedAt = DateTime.UtcNow;
            transaction.updatedBy = GetCurrentActor();

            await _apiDocumentDbContext.CustomerDebtTransactions.ReplaceOneAsync(transactionFilter, transaction);
            await AppendCustomerAuditLogsAsync(new[]
            {
                CreateAuditLogEntry(
                    transaction.customerId,
                    transaction.id,
                    "transaction-update",
                    $"debtTransaction.{transaction.id}.attachment.{attachment.id}",
                    attachment.fileName,
                    null,
                    transaction.updatedBy,
                    $"Deleted file: {attachment.fileName}")
            });

            return Ok(new
            {
                success = true,
                attachments = transaction.attachments
            });
        }

        [HttpGet("transactions/{transactionId}/attachments/{attachmentId}/download")]
        public async Task<IActionResult> DownloadDebtTransactionAttachmentAsync(string transactionId, string attachmentId)
        {
            if (string.IsNullOrWhiteSpace(transactionId) || string.IsNullOrWhiteSpace(attachmentId))
            {
                return BadRequest("Invalid request");
            }

            var transaction = await _apiDocumentDbContext.CustomerDebtTransactions
                .Find(x => x.id == transactionId && !x.isDeleted)
                .FirstOrDefaultAsync();
            if (transaction == null)
            {
                return NotFound("Transaction not found");
            }

            var attachment = (transaction.attachments ?? new List<DebtTransactionAttachment>())
                .FirstOrDefault(x => string.Equals(x.id, attachmentId, StringComparison.Ordinal));
            if (attachment == null)
            {
                return NotFound("Attachment not found");
            }

            var filePath = ResolveAttachmentAbsolutePath(transactionId, attachment);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Attachment file not found");
            }

            return PhysicalFile(
                filePath,
                string.IsNullOrWhiteSpace(attachment.contentType) ? "application/octet-stream" : attachment.contentType,
                attachment.fileName ?? "download.bin");
        }

        [HttpGet("transactions/{transactionId}/audit-logs")]
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
            if (transaction == null || transaction.isDeleted)
            {
                var legacyCustomerFilter = Builders<CustomerAccount>.Filter.ElemMatch(x => x.debtTransactions, t => t.id == transactionId);
                var legacyCustomer = await _apiDocumentDbContext.CustomerAccounts.Find(legacyCustomerFilter).FirstOrDefaultAsync();
                if (legacyCustomer == null || legacyCustomer.isDeleted)
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

        [HttpGet("transactions")]
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
                ? Builders<CustomerAccount>.Filter.And(
                    Builders<CustomerAccount>.Filter.Eq(x => x.Id, customerId),
                    Builders<CustomerAccount>.Filter.Eq(x => x.isDeleted, false))
                : Builders<CustomerAccount>.Filter.And(
                    Builders<CustomerAccount>.Filter.SizeGt(x => x.debtTransactions, 0),
                    Builders<CustomerAccount>.Filter.Eq(x => x.isDeleted, false));
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
                        contractCode = t.contractCode,
                        note = t.note,
                        attachments = (t.attachments ?? new List<DebtTransactionAttachment>()).ToList(),
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
                        || ContainsText(x.contractCode, keyword)
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

        [HttpPost("debt-ai/query")]
        public async Task<IActionResult> QueryDebtAiAsync([FromBody] DebtAiQueryRequest request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new { message = "prompt is required" });
            }

            try
            {
                var result = await _debtAiService.AnalyzeAsync(request, cancellationToken);
                return Ok(new
                {
                    provider = result.Provider,
                    model = result.Model,
                    summary = result.Summary,
                    findings = result.Findings,
                    recommendations = result.Recommendations,
                    relatedCustomers = result.RelatedCustomers,
                    scopeNotes = result.ScopeNotes,
                    suggestedQuestions = result.SuggestedQuestions,
                    answer = result.Answer,
                    rawText = result.RawText,
                    generatedAt = result.GeneratedAt,
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("customers/{customerId}/export-excel")]
        public async Task<IActionResult> ExportDebtCustomerExcelAsync(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                return BadRequest("Invalid customerId");
            }

            var filter = Builders<CustomerAccount>.Filter.Eq(x => x.Id, customerId);
            var customer = await _apiDocumentDbContext.CustomerAccounts.Find(filter).FirstOrDefaultAsync();
            if (customer == null || customer.isDeleted)
            {
                return NotFound("Customer not found");
            }

            await MigrateLegacyDataForCustomerAsync(customer, true);

            var transactions = await _apiDocumentDbContext.CustomerDebtTransactions
                .Find(x => x.customerId == customerId && !x.isDeleted)
                .SortBy(x => x.transactionAt)
                .ToListAsync();

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
                        contractCode = x.contractCode,
                        note = x.note,
                        attachments = x.attachments ?? new List<DebtTransactionAttachment>(),
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

        private string NormalizeText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }

        private string FormatDateTime(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("o") : null;
        }

        private string EnsureTransactionAttachmentDirectory(string transactionId)
        {
            var path = Path.Combine(GetDebtTransactionAttachmentRootPath(), transactionId);
            Directory.CreateDirectory(path);
            return path;
        }

        private string BuildTransactionAttachmentRelativePath(string transactionId, string storedFileName)
        {
            return Path.Combine(transactionId, storedFileName).Replace("\\", "/");
        }

        private string ResolveAttachmentAbsolutePath(string transactionId, DebtTransactionAttachment attachment)
        {
            if (attachment == null)
            {
                return Path.Combine(EnsureTransactionAttachmentDirectory(transactionId), string.Empty);
            }

            if (!string.IsNullOrWhiteSpace(attachment?.relativePath))
            {
                if (Path.IsPathRooted(attachment.relativePath))
                {
                    return attachment.relativePath;
                }

                var normalizedRelativePath = attachment.relativePath
                    .Replace("/", Path.DirectorySeparatorChar.ToString())
                    .TrimStart(Path.DirectorySeparatorChar);
                var legacyPrefix = $"AppData{Path.DirectorySeparatorChar}DebtTransactionFiles{Path.DirectorySeparatorChar}";
                if (normalizedRelativePath.StartsWith(legacyPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    return Path.Combine(_webHostEnvironment.ContentRootPath, normalizedRelativePath);
                }

                return Path.Combine(GetDebtTransactionAttachmentRootPath(), normalizedRelativePath);
            }

            return Path.Combine(EnsureTransactionAttachmentDirectory(transactionId), attachment?.storedFileName ?? string.Empty);
        }

        private string GetDebtTransactionAttachmentRootPath()
        {
            var configuredPath = _configuration["AttachmentStorage:DebtTransactionRootPath"];
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                var trimmed = configuredPath.Trim();
                return Path.IsPathRooted(trimmed)
                    ? trimmed
                    : Path.GetFullPath(trimmed, _webHostEnvironment.ContentRootPath);
            }

            return Path.Combine(_webHostEnvironment.ContentRootPath, "AppData", "DebtTransactionFiles");
        }

        private string GetSafeOriginalFileName(string fileName)
        {
            var source = string.IsNullOrWhiteSpace(fileName) ? "upload.bin" : fileName.Trim();
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(source.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray()).Trim();
            return string.IsNullOrWhiteSpace(sanitized) ? "upload.bin" : sanitized;
        }

        private bool ContainsText(string source, string keyword)
        {
            return !string.IsNullOrWhiteSpace(source)
                && !string.IsNullOrWhiteSpace(keyword)
                && source.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
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

        public class CreateDebtTransactionRequest
        {
            public string customerId { get; set; }
            public string transactionType { get; set; }
            public decimal amount { get; set; }
            public DateTime? transactionAt { get; set; }
            public string contractCode { get; set; }
            public string note { get; set; }
        }

        public class UpdateDebtTransactionRequest
        {
            public string customerId { get; set; }
            public string transactionType { get; set; }
            public decimal amount { get; set; }
            public DateTime? transactionAt { get; set; }
            public string contractCode { get; set; }
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
            public string contractCode { get; set; }
            public string note { get; set; }
            public List<DebtTransactionAttachment> attachments { get; set; }
            public DateTime createdAt { get; set; }
            public string createdBy { get; set; }
        }
    }
}