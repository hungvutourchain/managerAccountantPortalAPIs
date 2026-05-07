using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class ExchangeRate : MongoBaseModel
    {
        [BsonElement("nation")]
        [JsonPropertyName("nation")]
        public string? Nation { get; set; }

        [BsonElement("unit")]
        [JsonPropertyName("unit")]
        public double? Unit { get; set; }

        [BsonElement("currency")]
        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [BsonElement("timeZone")]
        [JsonPropertyName("timeZone")]
        public string? TimeZone { get; set; }

        [BsonElement("beginDate")]
        [JsonPropertyName("beginDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? BeginDate { get; set; }

        [BsonElement("endDate")]
        [JsonPropertyName("endDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? EndDate { get; set; }

        [BsonElement("rates")]
        [JsonPropertyName("rates")]
        public IList<ExRates> Rates { get; set; }

        [BsonElement("groupId")]
        [JsonPropertyName("groupId")]
        public string? GroupId { get; set; }

        [BsonElement("groupName")]
        [JsonPropertyName("groupName")]
        public string? GroupName { get; set; }
    }
    public class ExRates : MongoBaseModel
    {
        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [BsonElement("value")]
        [JsonPropertyName("value")]
        public double? Value { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class CurrencySelected : MongoBaseModel
    {
        //

        [BsonElement("securityStamp")]
        [JsonPropertyName("securityStamp")]
        public string? securityStamp { get; set; }

        [BsonElement("selecteds")]
        [JsonPropertyName("selecteds")]
        public List<selecteds>? Selecteds { get; set; }

        [BsonElement("value")]
        [JsonPropertyName("value")]
        public double? Value { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class selecteds : MongoBaseModel
    {
        [BsonElement("currencies")]
        [JsonPropertyName("currencies")]
        public List<string>? currencies { get; set; }

        [BsonElement("nation")]
        [JsonPropertyName("nation")]
        public string? nation { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class dataCurrencies : MongoBaseModel
    {
        [BsonElement("code")]
        [JsonPropertyName("code")]
        public string? code { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? name { get; set; }

        [BsonElement("symbol")]
        [JsonPropertyName("symbol")]
        public string? symbol { get; set; }
    }
}