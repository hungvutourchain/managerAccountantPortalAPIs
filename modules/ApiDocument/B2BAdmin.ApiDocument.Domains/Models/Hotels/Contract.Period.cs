using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Hotels
{
    [BsonIgnoreExtraElements]
    public class ContractPeriod : MongoBaseModel
    {
        [BsonElement("_idPeriods")]
        [JsonPropertyName("_idPeriods")]
        public string? IdPeriods { get; set; }

        [BsonElement("_idContracts")]
        [JsonPropertyName("_idContracts")]
        public string? IdContracts { get; set; }

        [BsonElement("isActive")]
        [JsonPropertyName("isActive")]
        public bool? IsActive { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [BsonElement("timeZone")]
        [JsonPropertyName("timeZone")]
        public string? TimeZone { get; set; }

        [BsonElement("beginDate")]
        [JsonPropertyName("beginDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? BeginDate { get; set; }


        [BsonElement("group")]
        [JsonPropertyName("group")]
        public int? Group { get; set; }

        [BsonElement("cutOffDate")]
        [JsonPropertyName("cutOffDate")]
        public double? CutOffDate { get; set; }
        // 

        [BsonElement("endDate")]
        [JsonPropertyName("endDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? EndDate { get; set; }

        [BsonElement("_type")]
        [JsonPropertyName("_type")]
        public string? Type { get; set; }

        [BsonElement("_typenumber")]
        [JsonPropertyName("_typenumber")]
        public int? Typenumber { get; set; }

        [BsonElement("mandatory")]
        [JsonPropertyName("mandatory")]
        public bool? Mandatory { get; set; }

        [BsonElement("note")]
        [JsonPropertyName("note")]
        public string? Note { get; set; }



        [BsonElement("description")]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [BsonElement("options")]
        [JsonPropertyName("options")]
        public IList<Option>? Options { get; set; }

        [BsonElement("userCreate")]
        [JsonPropertyName("userCreate")]
        public string? UserCreate { get; set; }

        [BsonElement("userUpdate")]
        [JsonPropertyName("userUpdate")]
        public string? UserUpdate { get; set; }

        [BsonElement("DateCreate")]
        [JsonPropertyName("DateCreate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DateCreate { get; set; }



        [BsonElement("DateUpdate")]
        [JsonPropertyName("DateUpdate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DateUpdate { get; set; }
    }
}
