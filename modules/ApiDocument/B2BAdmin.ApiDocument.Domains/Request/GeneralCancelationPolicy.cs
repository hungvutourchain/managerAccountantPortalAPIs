using System.Collections.Generic;
using System.Text.Json.Serialization;
using B2BAdmin.ApiDocument.Domains.Models.Hotels;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    public class GeneralCancelationPolicy : MongoBaseModel
    {
        [BsonElement("nation")]
        [JsonPropertyName("nation")]
        public string? Nation { get; set; }

        [BsonElement("cancelationPolicies")]
        [JsonPropertyName("cancelationPolicies")]
        public IList<CancelationPolicy> CancelationPolicies { get; set; }
    }    
}
