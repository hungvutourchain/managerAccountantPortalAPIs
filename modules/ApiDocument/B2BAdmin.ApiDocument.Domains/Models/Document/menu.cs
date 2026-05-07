using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class DocumentMenu : DocumentMenuchildren
    {
        [BsonElement("children")]
        [JsonPropertyName("children")]
        public IList<DocumentMenuchildren>? children { get; set; } = new List<DocumentMenuchildren>();
    }

    [BsonIgnoreExtraElements]
    public class DocumentMenuchildren : MongoBaseModel
    {
        [BsonElement("active")]
        [JsonPropertyName("active")]
        public bool? active { get; set; } = false;

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? name { get; set; }

        [BsonElement("url")]
        [JsonPropertyName("url")]
        public string? url { get; set; }

        [BsonElement("icon")]
        [JsonPropertyName("icon")]
        public string? icon { get; set; }

        [BsonElement("content")]
        [JsonPropertyName("content")]
        public string? content { get; set; }
    }
}
