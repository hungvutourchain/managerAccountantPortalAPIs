using System;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class PriceSeparate : MongoBaseModel
    {
        [BsonElement("forAdult")]
        [JsonPropertyName("forAdult")]
        public bool? ForAdult { get; set; }

        [BsonElement("age")]
        [JsonPropertyName("age")]
        public double? Age { get; set; }

        [BsonElement("title")]
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [BsonElement("quantity")]
        [JsonPropertyName("quantity")]
        public int? Quantity { get; set; }

        [BsonElement("unit")]
        [JsonPropertyName("unit")]
        public double? Unit { get; set; }

        [BsonElement("mealPlanChild")]
        [JsonPropertyName("mealPlanChild")]
        public double? MealPlanChild { get; set; }

        [BsonElement("mealPlanChildQuantity")]
        [JsonPropertyName("mealPlanChildQuantity")]
        public double? MealPlanChildQuantity { get; set; }

        [BsonElement("total")]
        [JsonPropertyName("total")]
        public double? Total { get; set; }
    }
}
