using System;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Hotels
{
    [BsonIgnoreExtraElements]
    public class period_groups : MongoBaseModel
    {
        [BsonElement("level")]
        [JsonPropertyName("level")]
        public int? level { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [BsonElement("nation")]
        [JsonPropertyName("nation")]
        public string? Nation { get; set; }

        [BsonElement("bufferDaysCancelation")]
        [JsonPropertyName("bufferDaysCancelation")]
        public double? BufferDaysCancelation { get; set; }

    }
}
