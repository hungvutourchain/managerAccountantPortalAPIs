using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class LogsconnectionLogs : UserAccess
    {
        
        
        [BsonElement("connectDate")]
        [JsonPropertyName("connectDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? ConnectDate { get; set; }

        [BsonElement("connectStrDate")]
        [JsonPropertyName("connectStrDate")]
        public string? ConnectStrDate { get; set; }

        [BsonElement("apiName")]
        [JsonPropertyName("apiName")]
        public string? ApiName { get; set; }

        [BsonElement("slugId")]
        [JsonPropertyName("slugId")]
        public string? SlugId { get; set; }
    }
}
