using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class DetailBookRetailSalesSic : BookRetailSalesSic
    {
        [BsonElement("idBooking")]
        [JsonPropertyName("idBooking")]
        public string? idBooking { get; set; }

        [BsonElement("isDelete")]
        [JsonPropertyName("isDelete")]
        public bool? IsDelete { get; set; }

        [BsonElement("statusBooking")]
        [JsonPropertyName("statusBooking")]
        public string? StatusBooking { get; set; }

        [BsonElement("seatInCoachId")]
        [JsonPropertyName("seatInCoachId")]
        public string? SeatInCoachId { get; set; }

        [BsonElement("nation")]
        [JsonPropertyName("nation")]
        public string? nation { get; set; }

        [BsonElement("md5code")]
        [JsonPropertyName("md5code")]
        public string? md5code { get; set; }

        [BsonElement("proCodeId")]
        [JsonPropertyName("proCodeId")]
        public string? proCodeId { get; set; }

        [BsonElement("createdDate")]
        [JsonPropertyName("createdDate")]
        public DateTime? CreatedDate { get; set; }

        [BsonElement("numberPaymentCutOffDate")]
        [JsonPropertyName("numberPaymentCutOffDate")]
        public double? numberPaymentCutOffDate { get; set; }

        [BsonElement("cutOffDateSic")]
        [JsonPropertyName("cutOffDateSic")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? cutOffDateSic { get; set; }

    }
    [BsonIgnoreExtraElements]
    public class BookRetailSalesSic
    {
        [BsonElement("idBookingOrigin")]
        [JsonPropertyName("idBookingOrigin")]
        public string? idBookingOrigin { get; set; }

        [BsonElement("location")]
        [JsonPropertyName("location")]
        public string? location { get; set; }

        [BsonElement("currency")]
        [JsonPropertyName("currency")]
        public string? currency { get; set; }

        [BsonElement("begindate")]
        [JsonPropertyName("begindate")]
        public string? begindate { get; set; }

        [BsonElement("enddate")]
        [JsonPropertyName("enddate")]
        public string? Enddate { get; set; }

        [BsonElement("bookingCode")]
        [JsonPropertyName("bookingCode")]
        public string? bookingCode { get; set; }

        [BsonElement("bookingName")]
        [JsonPropertyName("bookingName")]
        public string? bookingName { get; set; }

        [BsonElement("idPackage")]
        [JsonPropertyName("idPackage")]
        public string? idPackage { get; set; }

        public List<upgradeRoomRate>? upgradeRoomRate { get; set; }
        public List<ChildSalesSic>? children { get; set; }

        public decimal? Amount { get; set; }
        public decimal? priceSingle { get; set; }
        public decimal? priceTotalSingle { get; set; }
        public int? quantitySingle { get; set; }


        public decimal? priceDouble { get; set; }
        public decimal? priceTotalDouble { get; set; }
        public int? quantityDouble { get; set; }

        public decimal? priceTriple { get; set; }
        public decimal? priceTotalTriple { get; set; }
        public int? quantityTriple { get; set; }
        public double? roomLeft { get; set; }

        public decimal? priceTotalChildren { get; set; }
        public int? quantityChildren { get; set; }
        public List<_Hotels>? hotels { get; set; }


    }
    [BsonIgnoreExtraElements]
    public class ChildSalesSic
    {
        public int? quantity { get; set; } = 0;
        public int? age { get; set; } = 0;
        public decimal? price { get; set; } = 0;
        public decimal? priceTotal { get; set; } = 0;
    }
    [BsonIgnoreExtraElements]
    public class _Hotels
    {
        public string? hotelName { get; set; }
        public string? category { get; set; }
        public string? idHotel { get; set; }
        public List<upgradeRoomRate>? upgradeRoomRates { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class upgradeRoomRate
    {
        public decimal? priceDifference { get; set; }
        public double? occupancy { get; set; }
        public string? category { get; set; }
        public double? nights { get; set; }
        public int? quantity { get; set; }
    }
}

