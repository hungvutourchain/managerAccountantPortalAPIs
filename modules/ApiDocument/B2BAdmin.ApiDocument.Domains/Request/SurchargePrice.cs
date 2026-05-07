using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class SurchargePrice : MongoBaseModel
    {
        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [BsonElement("IsItem")]
        [JsonPropertyName("IsItem")]
        public string? IsItem { get; set; }

        [BsonElement("tariffPeriodName")]
        [JsonPropertyName("tariffPeriodName")]
        public string? TariffPeriodName { get; set; }

        [BsonElement("priceSeparate")]
        [JsonPropertyName("priceSeparate")]
        public IList<PriceSeparate>? PriceSeparates { get; set; }

        [BsonElement("adultQuantity")]
        [JsonPropertyName("adultQuantity")]
        public double? AdultQuantity { get; set; }

        [BsonElement("childQuantity")]
        [JsonPropertyName("childQuantity")]
        public double? ChildQuantity { get; set; }

        [BsonElement("total")]
        [JsonPropertyName("total")]
        public double? Total { get; set; }

        [BsonElement("totalNoChild")]
        [JsonPropertyName("totalNoChild")]
        public double? TotalNoChild { get; set; }
    }
}
