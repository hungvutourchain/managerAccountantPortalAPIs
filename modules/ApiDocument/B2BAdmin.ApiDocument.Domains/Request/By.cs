using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class By : MongoBaseModel
    {
        [BsonElement("cname")]
        [JsonPropertyName("cname")]
        public string? Cname { get; set; }

        [BsonElement("cemail")]
        [JsonPropertyName("cemail")]
        public string? Cemail { get; set; }
    }
}
