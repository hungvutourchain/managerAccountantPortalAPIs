using CloudKit.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class GentOnlineObject : MongoBaseModel
    {
        [BsonElement("nation")]
        [JsonPropertyName("nation")]
        public string? Nation { get; set; }

        [BsonElement("ref")]
        [JsonPropertyName("ref")]
        public string? Ref { get; set; }

        [BsonElement("isPrivate")]
        [JsonPropertyName("isPrivate")]
        public bool? IsPrivate { get; set; }

        [BsonElement("agencyTitle")]
        [JsonPropertyName("agencyTitle")]
        public string? AgencyTitle { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [BsonElement("code")]
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [BsonElement("agid")]
        [JsonPropertyName("agid")]
        public string? Agid { get; set; }


        [BsonElement("email")]
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [BsonElement("country")]
        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [BsonElement("address")]
        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [BsonElement("adtel")]
        [JsonPropertyName("adtel")]
        public string? Adtel { get; set; }

        [BsonElement("booker_email")]
        [JsonPropertyName("booker_email")]
        public string? Booker_email { get; set; }

        [BsonElement("booker_url")]
        [JsonPropertyName("booker_url")]
        public string? Booker_url { get; set; }


        [BsonElement("agencygroup")]
        [JsonPropertyName("agencygroup")]
        public string? Agencygroup { get; set; }


        [BsonElement("aconsortia")]
        [JsonPropertyName("aconsortia")]
        public string? Aconsortia { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("procodes")]
        [JsonPropertyName("procodes")]
        public string? Procodes { get; set; }

        [BsonElement("isActive")]
        [JsonPropertyName("isActive")]
        public bool? IsActive { get; set; }

        [BsonElement("contacts")]
        [JsonPropertyName("contacts")]
        public IList<AgencyContact> Contacts { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class AgencyContact : MongoBaseModel
    {
        [BsonElement("code")]
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [BsonElement("active")]
        [JsonPropertyName("active")]
        public bool? Active { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [BsonElement("title")]
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [BsonElement("tel")]
        [JsonPropertyName("tel")]
        public string? Tel { get; set; }

        [BsonElement("email")]
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [BsonElement("url")]
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}