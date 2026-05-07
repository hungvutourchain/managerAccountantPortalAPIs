using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Hotels
{
    [BsonIgnoreExtraElements]
    public class Exbed : MongoBaseModel
    {
        [BsonElement("from")]
        [JsonPropertyName("from")]
        public double? From { get; set; }

        [BsonElement("to")]
        [JsonPropertyName("to")]
        public double? To { get; set; }

        [BsonElement("price")]
        [JsonPropertyName("price")]
        public double? Price { get; set; }

        [BsonElement("price_usd")]
        [JsonPropertyName("price_usd")]
        public double? PriceUsd { get; set; }

        [BsonElement("hasBreakfast")]
        [JsonPropertyName("hasBreakfast")]
        public bool? HasBreakfast { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class ChildSharing : Exbed
    {
        [BsonElement("note")]
        [JsonPropertyName("note")]
        public string Note { get; set; } = "";
    }
}
