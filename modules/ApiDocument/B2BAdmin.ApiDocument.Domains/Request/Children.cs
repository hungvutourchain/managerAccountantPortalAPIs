using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class Children : MongoBaseModel
    {
        [BsonElement("No")]
        [JsonPropertyName("No")]
        public int? No { get; set; }

        [BsonElement("age")]
        [JsonPropertyName("age")]
        public double? Age { get; set; }

        [BsonElement("price")]
        [JsonPropertyName("price")]
        public double? Price { get; set; }

        /*
            No: Number,
            age: Number,
            price: Number,
        */
    }
}
