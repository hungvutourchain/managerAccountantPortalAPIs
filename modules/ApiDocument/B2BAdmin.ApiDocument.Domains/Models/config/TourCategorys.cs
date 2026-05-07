using CloudKit.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class TourCategorys : MongoBaseModel
    {
        [BsonElement("nation")]
        [JsonPropertyName("nation")]
        public string? nation { get; set; }

        [BsonElement("code")]
        [JsonPropertyName("code")]
        public string? code { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? name { get; set; }

        [BsonElement("show")]
        [JsonPropertyName("show")]
        public bool? show { get; set; }

        [BsonElement("sort")]
        [JsonPropertyName("sort")]
        public int? sort { get; set; }
    }
}