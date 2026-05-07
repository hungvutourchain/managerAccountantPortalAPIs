using CloudKit.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class lsNation : MongoBaseModel
    {
        [BsonElement("nation")]
        [JsonPropertyName("code")]
        public string? Nation { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? name { get; set; }

        [BsonElement("currency")]
        [JsonPropertyName("currency")]
        public string? currency { get; set; }

        [BsonElement("symbol")]
        [JsonPropertyName("symbol")]
        public string? symbol { get; set; }
    }
}