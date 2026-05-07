using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class TariffPeriods : MongoBaseModel
    {
        [BsonElement("nation")]
        [JsonPropertyName("nation")]
        public string? Nation { get; set; }

        [BsonElement("groupPeriod")]
        [JsonPropertyName("groupPeriod")]
        public string? groupPeriod { get; set; }

        [BsonElement("tariffType")]
        [JsonPropertyName("tariffType")]
        public string? tariffType { get; set; }

        [BsonElement("no")]
        [JsonPropertyName("no")]
        public int? no { get; set; }

        [BsonElement("lsPeriod")]
        [JsonPropertyName("lsPeriod")]
        public List<TrPeriods> lsPeriod { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class TrPeriods : MongoBaseModel
    {
        [BsonElement("date")]
        [JsonPropertyName("date")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? date { get; set; }

        [BsonElement("timeZone")]
        [JsonPropertyName("timeZone")]
        public string? TimeZone { get; set; }

        [BsonElement("begindate")]
        [JsonPropertyName("begindate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? begindate { get; set; }

        [BsonElement("enddate")]
        [JsonPropertyName("enddate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? enddate { get; set; }

        [BsonElement("no")]
        [JsonPropertyName("no")]
        public int? no { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? name { get; set; }
    }
}
