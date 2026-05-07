using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class AgentVaultFavourite : MongoBaseModel// data from Tours, Hotel
    {
        [BsonElement("md5code")]
        [JsonPropertyName("md5code")]
        public string Md5code { get; set; }

        [BsonElement("tourId")]
        [JsonPropertyName("tourId")]
        public string? TourId { get; set; }

        [BsonElement("nameTour")]
        [JsonPropertyName("nameTour")]
        public string? nameTour { get; set; }

        [BsonElement("IsItem")]
        [JsonPropertyName("IsItem")]
        public string? IsItem { get; set; }

        [BsonElement("type")]
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [BsonElement("isForTourChain")]
        [JsonPropertyName("isForTourChain")]
        public bool? IsForTourChain { get; set; }

        [BsonElement("parentActivityId")]
        [JsonPropertyName("parentActivityId")]
        public string? ParentActivityId { get; set; }
    }
}
