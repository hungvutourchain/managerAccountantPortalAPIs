using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Hotels
{
    [BsonIgnoreExtraElements]
    public class HotelLogs : MongoBaseModel
    {
        [BsonElement("hotel")]
        [JsonPropertyName("hotel")]
        public Hotel? hotel { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class Hotel : MongoBaseModel
    {
        [BsonElement("nation")]
        [JsonPropertyName("nation")]
        public string? Nation { get; set; }

        [BsonElement("httype")]
        [JsonPropertyName("httype")]
        public IList<string>? Httype { get; set; }

        [BsonElement("accommodationStyles")]
        [JsonPropertyName("accommodationStyles")]
        public IList<string>? AccommodationStyles { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [BsonElement("MatchScore")]
        [JsonPropertyName("MatchScore")]
        public int? MatchScore { get; set; }

        //MatchScore

        [BsonElement("brandName")]
        [JsonPropertyName("brandName")]
        public string? BrandName { get; set; }

        [BsonElement("rating")]
        [JsonPropertyName("rating")]
        public double? Rating { get; set; }

        [BsonElement("location")]
        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [BsonElement("idCountry")]
        [JsonPropertyName("idCountry")]
        public string? IdCountry { get; set; }

        [BsonElement("province")]
        [JsonPropertyName("province")]
        public string? province { get; set; }

        [BsonElement("companyId")]
        [JsonPropertyName("companyId")]
        public string? companyId { get; set; }

        [BsonElement("courseId")]
        [JsonPropertyName("courseId")]
        public string? courseId { get; set; }

        [BsonElement("departmentId")]
        [JsonPropertyName("departmentId")]
        public string? departmentId { get; set; }

        [BsonElement("Category")]
        [JsonPropertyName("Category")]
        public string? Category { get; set; }

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

        [BsonElement("supplier")]
        [JsonPropertyName("supplier")]
        public string? Supplier { get; set; } // supplier _id

        [BsonElement("supplierID")]
        [JsonPropertyName("supplierID")]
        public string? SupplierID { get; set; } // supplier code

        [BsonElement("_idService")]
        [JsonPropertyName("_idService")]
        public string? IdService { get; set; } // service _id

        [BsonElement("serviceID")]
        [JsonPropertyName("serviceID")]
        public string? ServiceID { get; set; } // service _id



        [BsonElement("isActive")]
        [JsonPropertyName("isActive")]
        public bool? IsActive { get; set; }

        [BsonElement("description")]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [BsonElement("overView")]
        [JsonPropertyName("overView")]
        public string? OverView { get; set; }

        [BsonElement("thumbnailImage")]
        [JsonPropertyName("thumbnailImage")]
        public string? thumbnailImage { get; set; }

        [BsonElement("rooms")]
        [JsonPropertyName("rooms")]
        public IList<Room>? Rooms { get; set; }

        [BsonElement("contracts")]
        [JsonPropertyName("contracts")]
        public IList<Contract>? Contracts { get; set; }



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

        [BsonElement("City")]
        [JsonPropertyName("City")]
        public string? City { get; set; }

        [BsonElement("locationIds")]
        [JsonPropertyName("locationIds")]
        public string? locationIds { get; set; }

        [BsonElement("cityName")]
        [JsonPropertyName("cityName")]
        public string? CityName { get; set; }

        [BsonElement("GroupName")]
        [JsonPropertyName("GroupName")]
        public string? GroupName { get; set; }

        [BsonElement("Area")]
        [JsonPropertyName("Area")]
        public string? Area { get; set; }
    }
}
