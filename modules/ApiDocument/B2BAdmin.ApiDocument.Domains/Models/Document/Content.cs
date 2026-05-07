using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
   
    [BsonIgnoreExtraElements]
    public class DocumentContent : MongoBaseModel
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

        [BsonElement("content")]
        [JsonPropertyName("content")]
        public string? content { get; set; }

    }
}
