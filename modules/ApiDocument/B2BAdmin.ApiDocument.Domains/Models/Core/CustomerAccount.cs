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

        [BsonElement("createdAt")]
        [JsonPropertyName("createdAt")]
        public DateTime createdAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        [JsonPropertyName("updatedAt")]
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
    }
}
