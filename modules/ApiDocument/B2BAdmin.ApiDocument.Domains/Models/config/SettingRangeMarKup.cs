using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Tours
{
    [BsonIgnoreExtraElements]
    public class SettingRange_MarKup : MongoBaseModel
    {
        [BsonElement("nation")]
        [JsonPropertyName("nation")]
        public string? Nation { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [BsonElement("currency")]
        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [BsonElement("by")]
        [JsonPropertyName("by")]
        public string? By { get; set; }

        [BsonElement("items")]
        [JsonPropertyName("items")]
        public List<SettingRange_MarKup_Vaule>? Items { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class SettingRange_MarKup_Vaule : MongoBaseModel
    {
        [BsonElement("min")]
        [JsonPropertyName("min")]
        public double? Min { get; set; }

        [BsonElement("value")]
        [JsonPropertyName("value")]
        public double? Value { get; set; }

        [BsonElement("markup")]
        [JsonPropertyName("markup")]
        public double? Markup { get; set; }
    }
}