using System.Collections.Generic;
using System.Text.Json.Serialization;

using CloudKit.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Hotels
{
    [BsonIgnoreExtraElements]
    public class CancelationPolicy: MongoBaseModel
    {
        [BsonElement("_isId")]
        [JsonPropertyName("_isId")]
        public string? IsId { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [BsonElement("num_Day")]
        [JsonPropertyName("numberOfDay")]
        public double? Num_Day { get; set; }

        [BsonElement("nocharge")]
        [JsonPropertyName("nocharge")]
        public bool? Nocharge { get; set; }

        [BsonElement("value")]
        [JsonPropertyName("value")]
        public double? Value { get; set; }
        //group:Number,
        [BsonElement("group")]
        [JsonPropertyName("group")]
        public int? Group { get; set; }



        [BsonElement("_type")]
        [JsonPropertyName("type")]
        public string? Type { get; set; } // CancelTypeConst

        [BsonElement("options")]
        [JsonPropertyName("options")]
        public string? Options { get; set; }

        [BsonElement("nights")]
        [JsonPropertyName("nights")]
        public double? Nights { get; set; }

        [BsonElement("numberOfRoomsFrom")]
        [JsonPropertyName("numberOfRoomsFrom")]
        public double? numberOfRoomsFrom { get; set; }

        [BsonElement("numberOfRoomsTo")]
        [JsonPropertyName("numberOfRoomsTo")]
        public double? numberOfRoomsTo { get; set; }

        [BsonElement("washDown")]
        [JsonPropertyName("washDown")]
        public double? washDown { get; set; }

        [BsonElement("washDownBefore")]
        [JsonPropertyName("washDownBefore")]
        public double? washDownBefore { get; set; }

        [BsonElement("arrivalDate")]
        [JsonPropertyName("arrivalDate")]
        public double? arrivalDate { get; set; }

        [BsonElement("numberOfBookingNightsFrom")]
        [JsonPropertyName("numberOfBookingNightsFrom")]
        public double? numberOfBookingNightsFrom { get; set; }

        [BsonElement("numberOfBookingNightsTo")]
        [JsonPropertyName("numberOfBookingNightsTo")]
        public double? numberOfBookingNightsTo { get; set; }

        [BsonElement("periods_travel")]
        [JsonPropertyName("periods_travel")]
        public IList<CancelationPeriod> Periods_travel { get; set; }

        [BsonElement("periods_booking")]
        [JsonPropertyName("periods_booking")]
        public IList<CancelationPeriod> Periods_booking { get; set; }

        [BsonElement("lsNights")]
        [JsonPropertyName("lsNights")]
        public IList<Cancelnights> LsNights { get; set; }

        [BsonElement("currency")]
        [JsonPropertyName("currency")]
        public string? Currency { get; set; } // for general

        [BsonElement("note")]
        [JsonPropertyName("note")]
        public string? Note { get; set; }
    }

    public static class CancelTypeConst
    {
        public const string Fixed = "Fixed";
        public const string Percent = "%";
    }

    [BsonIgnoreExtraElements]
    public class Cancelnights : MongoBaseModel
    {
        [BsonElement("from")]
        [JsonPropertyName("from")]
        public double? From { get; set; }

        [BsonElement("to")]
        [JsonPropertyName("to")]
        public double? To { get; set; }

        [BsonElement("value")]
        [JsonPropertyName("value")]
        public double? Value { get; set; }
    }
}
