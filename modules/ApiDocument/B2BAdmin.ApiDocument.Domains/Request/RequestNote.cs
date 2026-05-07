using System;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class RequestNote
    {
        [BsonElement("type")]
        [JsonPropertyName("type")]
        public string? Type { get; set; } // RequestNoteType

        [BsonElement("date")]
        [JsonPropertyName("date")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? Date { get; set; }

        [BsonElement("by")]
        [JsonPropertyName("by")]
        public string? By { get; set; }

        [BsonElement("note")]
        [JsonPropertyName("note")]
        public string? Note { get; set; }
    }

    [BsonIgnoreExtraElements]
    public static class RequestNoteType
    {
        public const string? Feedback = "feedback";
        public const string? Cancelled = "cancelled";
    }
}
