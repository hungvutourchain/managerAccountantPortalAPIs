using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using B2BAdmin.ApiDocument.Domains.Models.Hotels;
using CloudKit.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class infohotels_view_loadtour : MongoBaseModel
    {
        [BsonElement("isActive")]
        [JsonPropertyName("isActive")]
        public bool? IsActive { get; set; }

        [BsonElement("nation")]
        [JsonPropertyName("nation")]
        public string? Nation { get; set; }

        [BsonElement("overView")]
        [JsonPropertyName("overView")]
        public string? overView { get; set; }

        [BsonElement("address")]
        [JsonPropertyName("address")]
        public string? address { get; set; }

        [BsonElement("rating")]
        [JsonPropertyName("rating")]
        public double? rating { get; set; }

        [BsonElement("tel")]
        [JsonPropertyName("tel")]
        public string? tel { get; set; }

        [BsonElement("url")]
        [JsonPropertyName("url")]
        public string? url { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class infohotels_view_contract_period_option_ope : MongoBaseModel
    {
        [BsonElement("nation")]
        [JsonPropertyName("nation")]
        public string? Nation { get; set; }

        [BsonElement("rating")]
        [JsonPropertyName("rating")]
        public double? Rating { get; set; }

        [BsonElement("Category")]
        [JsonPropertyName("Category")]
        public string? Category { get; set; }

        [BsonElement("isActive")]
        [JsonPropertyName("isActive")]
        public bool? IsActive { get; set; }

        [BsonElement("locationIds")]
        [JsonPropertyName("locationIds")]
        public string? locationIds { get; set; }

        [BsonElement("hotelName")]
        [JsonPropertyName("hotelName")]
        public string? HotelName { get; set; }

        [BsonElement("address")]
        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [BsonElement("tel")]
        [JsonPropertyName("tel")]
        public string? Tel { get; set; }

        [BsonElement("email")]
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [BsonElement("url")]
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [BsonElement("_idService")]
        [JsonPropertyName("_idService")]
        public string? IdService { get; set; }

        [BsonElement("rooms")]
        [JsonPropertyName("rooms")]
        public IList<Room>? Rooms { get; set; }

        [BsonElement("_idPeriods")]
        [JsonPropertyName("_idPeriods")]
        public string? periodId { get; set; }

        [BsonElement("_idContracts")]
        [JsonPropertyName("_idContracts")]
        public string? contractId { get; set; }

        [BsonElement("timeZone")]
        [JsonPropertyName("timeZone")]
        public string? TimeZone { get; set; }

        [BsonElement("currency")]
        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [BsonElement("ct_name")]
        [JsonPropertyName("ct_name")]
        public string? ct_name { get; set; }

        [BsonElement("ct_beginDate")]
        [JsonPropertyName("ct_beginDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? ct_beginDate { get; set; }

        [BsonElement("ct_endDate")]
        [JsonPropertyName("ct_endDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? ct_endDate { get; set; }

        [BsonElement("ct_isActive")]
        [JsonPropertyName("ct_isActive")]
        public bool? ct_isActive { get; set; }

        [BsonElement("ct_market")]
        [JsonPropertyName("ct_market")]
        public IList<string>? ct_Market { get; set; }

        [BsonElement("ct_exceptMarket")]
        [JsonPropertyName("ct_exceptMarket")]
        public IList<string>? ct_ExceptMarket { get; set; }

        [BsonElement("per_name")]
        [JsonPropertyName("per_name")]
        public string? per_name { get; set; }

        [BsonElement("per_isActive")]
        [JsonPropertyName("per_isActive")]
        public bool? per_isActive { get; set; }

        [BsonElement("per_beginDate")]
        [JsonPropertyName("per_beginDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? per_beginDate { get; set; }

        [BsonElement("per_endDate")]
        [JsonPropertyName("per_endDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? per_endDate { get; set; }

        [BsonElement("per_type")]
        [JsonPropertyName("per_type")]
        public string? per_type { get; set; }

        [BsonElement("per_cutOffDate")]
        [JsonPropertyName("per_cutOffDate")]
        public double? per_cutOffDate { get; set; }

        [BsonElement("per_note")]
        [JsonPropertyName("per_note")]
        public string? per_note { get; set; }

        [BsonElement("option")]
        [JsonPropertyName("option")]
        public Option? option { get; set; }


        // add new 
        [BsonElement("rangeMarkup")]
        [JsonPropertyName("rangeMarkup")]
        public bool? RangeMarkup { get; set; }

        [BsonElement("idRangeMarkup")]
        [JsonPropertyName("idRangeMarkup")]
        public string? IdRangeMarkup { get; set; }

        [BsonElement("DMK")]
        [JsonPropertyName("DMK")]
        public bool? DMK { get; set; }

        [BsonElement("valueDMK")]
        [JsonPropertyName("valueDMK")]
        public decimal? ValueDMK { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class hotelRoom : Option
    {


        [BsonElement("nation")]
        [JsonPropertyName("nation")]
        public string? Nation { get; set; }

        [BsonElement("rating")]
        [JsonPropertyName("rating")]
        public double? Rating { get; set; }

        [BsonElement("Category")]
        [JsonPropertyName("Category")]
        public string? Category { get; set; }

        [BsonElement("isActive")]
        [JsonPropertyName("isActive")]
        public bool? IsActive { get; set; }

        [BsonElement("locationIds")]
        [JsonPropertyName("locationIds")]
        public string? locationIds { get; set; }

        [BsonElement("hotelName")]
        [JsonPropertyName("accommodationName")]
        public string? HotelName { get; set; }

        [BsonElement("address")]
        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [BsonElement("tel")]
        [JsonPropertyName("tel")]
        public string? Tel { get; set; }

        [BsonElement("email")]
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [BsonElement("url")]
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [BsonElement("_idService")]
        [JsonPropertyName("_idService")]
        public string? IdService { get; set; }

        //[BsonElement("rooms")]
        //[JsonPropertyName("rooms")]
        //public IList<Room>? Rooms { get; set; }

        [BsonElement("_idPeriods")]
        [JsonPropertyName("_idPeriods")]
        public string? periodId { get; set; }

        [BsonElement("_idContracts")]
        [JsonPropertyName("_idContracts")]
        public string? contractId { get; set; }

        [BsonElement("timeZone")]
        [JsonPropertyName("timeZone")]
        public string? TimeZone { get; set; }

        [BsonElement("currency")]
        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [BsonElement("ct_name")]
        [JsonPropertyName("contractName")]
        public string? ct_name { get; set; }

        [BsonElement("ct_beginDate")]
        [JsonPropertyName("contractbeginDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? ct_beginDate { get; set; }

        [BsonElement("ct_endDate")]
        [JsonPropertyName("contractendDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? ct_endDate { get; set; }

        [BsonElement("ct_isActive")]
        [JsonPropertyName("contractIsActive")]
        public bool? ct_isActive { get; set; }

        [BsonElement("ct_market")]
        [JsonPropertyName("contractMarket")]
        public IList<string>? ct_Market { get; set; }

        [BsonElement("ct_exceptMarket")]
        [JsonPropertyName("contractExceptMarket")]
        public IList<string>? ct_ExceptMarket { get; set; }

        [BsonElement("per_name")]
        [JsonPropertyName("periodName")]
        public string? per_name { get; set; }

        [BsonElement("per_isActive")]
        [JsonPropertyName("periodIsActive")]
        public bool? per_isActive { get; set; }

        [BsonElement("per_beginDate")]
        [JsonPropertyName("periodBeginDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? per_beginDate { get; set; }

        [BsonElement("per_endDate")]
        [JsonPropertyName("periodEndDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? per_endDate { get; set; }

        [BsonElement("per_type")]
        [JsonPropertyName("periodType")]
        public string? per_type { get; set; }

        [BsonElement("per_cutOffDate")]
        [JsonPropertyName("periodCutOffDate")]
        public double? per_cutOffDate { get; set; }

        [BsonElement("per_note")]
        [JsonPropertyName("periodNote")]
        public string? per_note { get; set; }

        //[BsonElement("option")]
        //[JsonPropertyName("option")]
        //public Option? option { get; set; }


        // add new 
        //[BsonElement("rangeMarkup")]
        //[JsonPropertyName("rangeMarkup")]
        //public bool? RangeMarkup { get; set; }

        //[BsonElement("idRangeMarkup")]
        //[JsonPropertyName("idRangeMarkup")]
        //public string? IdRangeMarkup { get; set; }

        //[BsonElement("DMK")]
        //[JsonPropertyName("DMK")]
        //public bool? DMK { get; set; }

        //[BsonElement("valueDMK")]
        //[JsonPropertyName("valueDMK")]
        //public decimal? ValueDMK { get; set; }
    }
}