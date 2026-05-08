using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class CustomerAccount : MongoBaseModel
    {
        [BsonElement("code")]
        [JsonPropertyName("code")]
        public string? code { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? name { get; set; }

        [BsonElement("category")]
        [JsonPropertyName("category")]
        public string? category { get; set; }

        [BsonElement("taxCode")]
        [JsonPropertyName("taxCode")]
        public string? taxCode { get; set; }

        [BsonElement("bankAccount")]
        [JsonPropertyName("bankAccount")]
        public string? bankAccount { get; set; }

        [BsonElement("bankName")]
        [JsonPropertyName("bankName")]
        public string? bankName { get; set; }

        [BsonElement("phone")]
        [JsonPropertyName("phone")]
        public string? phone { get; set; }

        [BsonElement("email")]
        [JsonPropertyName("email")]
        public string? email { get; set; }

        [BsonElement("address")]
        [JsonPropertyName("address")]
        public string? address { get; set; }

        [BsonElement("debtAmount")]
        [JsonPropertyName("debtAmount")]
        public decimal debtAmount { get; set; }

        [BsonElement("creditAmount")]
        [JsonPropertyName("creditAmount")]
        public decimal creditAmount { get; set; }

        [BsonElement("status")]
        [JsonPropertyName("status")]
        public string? status { get; set; }

        [BsonElement("riskLevel")]
        [JsonPropertyName("riskLevel")]
        public string? riskLevel { get; set; }

        [BsonElement("owner")]
        [JsonPropertyName("owner")]
        public string? owner { get; set; }

        [BsonElement("tags")]
        [JsonPropertyName("tags")]
        public IList<string>? tags { get; set; } = new List<string>();

        [BsonElement("lastTransactionAt")]
        [JsonPropertyName("lastTransactionAt")]
        public DateTime? lastTransactionAt { get; set; }

        [BsonElement("debtTransactions")]
        [JsonPropertyName("debtTransactions")]
        public IList<DebtTransactionRecord>? debtTransactions { get; set; } = new List<DebtTransactionRecord>();

        [BsonElement("auditLogs")]
        [JsonPropertyName("auditLogs")]
        public IList<CustomerAuditLog>? auditLogs { get; set; } = new List<CustomerAuditLog>();

        [BsonElement("createdAt")]
        [JsonPropertyName("createdAt")]
        public DateTime createdAt { get; set; } = DateTime.UtcNow;

        [BsonElement("createdBy")]
        [JsonPropertyName("createdBy")]
        public string? createdBy { get; set; }

        [BsonElement("updatedAt")]
        [JsonPropertyName("updatedAt")]
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedBy")]
        [JsonPropertyName("updatedBy")]
        public string? updatedBy { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class DebtTransactionRecord
    {
        [BsonElement("id")]
        [JsonPropertyName("id")]
        public string? id { get; set; }

        [BsonElement("transactionType")]
        [JsonPropertyName("transactionType")]
        public string? transactionType { get; set; }

        [BsonElement("amount")]
        [JsonPropertyName("amount")]
        public decimal amount { get; set; }

        [BsonElement("transactionAt")]
        [JsonPropertyName("transactionAt")]
        public DateTime transactionAt { get; set; }

        [BsonElement("note")]
        [JsonPropertyName("note")]
        public string? note { get; set; }

        [BsonElement("createdAt")]
        [JsonPropertyName("createdAt")]
        public DateTime createdAt { get; set; } = DateTime.UtcNow;

        [BsonElement("createdBy")]
        [JsonPropertyName("createdBy")]
        public string? createdBy { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class CustomerAuditLog
    {
        [BsonElement("id")]
        [JsonPropertyName("id")]
        public string? id { get; set; }

        [BsonElement("action")]
        [JsonPropertyName("action")]
        public string? action { get; set; }

        [BsonElement("field")]
        [JsonPropertyName("field")]
        public string? field { get; set; }

        [BsonElement("oldValue")]
        [JsonPropertyName("oldValue")]
        public string? oldValue { get; set; }

        [BsonElement("newValue")]
        [JsonPropertyName("newValue")]
        public string? newValue { get; set; }

        [BsonElement("changedAt")]
        [JsonPropertyName("changedAt")]
        public DateTime changedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("changedBy")]
        [JsonPropertyName("changedBy")]
        public string? changedBy { get; set; }

        [BsonElement("note")]
        [JsonPropertyName("note")]
        public string? note { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class CustomerDebtTransaction
    {
        [BsonElement("id")]
        [JsonPropertyName("id")]
        public string? id { get; set; }

        [BsonElement("customerId")]
        [JsonPropertyName("customerId")]
        public string? customerId { get; set; }

        [BsonElement("transactionType")]
        [JsonPropertyName("transactionType")]
        public string? transactionType { get; set; }

        [BsonElement("amount")]
        [JsonPropertyName("amount")]
        public decimal amount { get; set; }

        [BsonElement("transactionAt")]
        [JsonPropertyName("transactionAt")]
        public DateTime transactionAt { get; set; }

        [BsonElement("note")]
        [JsonPropertyName("note")]
        public string? note { get; set; }

        [BsonElement("createdAt")]
        [JsonPropertyName("createdAt")]
        public DateTime createdAt { get; set; } = DateTime.UtcNow;

        [BsonElement("createdBy")]
        [JsonPropertyName("createdBy")]
        public string? createdBy { get; set; }

        [BsonElement("updatedAt")]
        [JsonPropertyName("updatedAt")]
        public DateTime? updatedAt { get; set; }

        [BsonElement("updatedBy")]
        [JsonPropertyName("updatedBy")]
        public string? updatedBy { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class CustomerAuditLogEntry
    {
        [BsonElement("id")]
        [JsonPropertyName("id")]
        public string? id { get; set; }

        [BsonElement("customerId")]
        [JsonPropertyName("customerId")]
        public string? customerId { get; set; }

        [BsonElement("transactionId")]
        [JsonPropertyName("transactionId")]
        public string? transactionId { get; set; }

        [BsonElement("action")]
        [JsonPropertyName("action")]
        public string? action { get; set; }

        [BsonElement("field")]
        [JsonPropertyName("field")]
        public string? field { get; set; }

        [BsonElement("oldValue")]
        [JsonPropertyName("oldValue")]
        public string? oldValue { get; set; }

        [BsonElement("newValue")]
        [JsonPropertyName("newValue")]
        public string? newValue { get; set; }

        [BsonElement("changedAt")]
        [JsonPropertyName("changedAt")]
        public DateTime changedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("changedBy")]
        [JsonPropertyName("changedBy")]
        public string? changedBy { get; set; }

        [BsonElement("note")]
        [JsonPropertyName("note")]
        public string? note { get; set; }
    }
}
