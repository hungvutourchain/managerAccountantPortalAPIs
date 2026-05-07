using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class LanguageCode : MongoBaseModel
    {
        [BsonElement("language")]
        [JsonPropertyName("language")]
        public string? language { get; set; }

        [BsonElement("code")]
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [BsonElement("fullName")]
        [JsonPropertyName("fullName")]
        public string? FullName => Code + " - " + language;
    }
}
