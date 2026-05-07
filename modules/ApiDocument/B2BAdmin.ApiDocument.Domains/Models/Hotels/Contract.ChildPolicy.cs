using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Hotels
{
    [BsonIgnoreExtraElements]
    public class ContractChildPolicy : MongoBaseModel
    {
        [BsonElement("_isId")]
        [JsonPropertyName("_isId")]
        public string? IsId { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [BsonElement("no")]
        [JsonPropertyName("no")]
        public int? No { get; set; }

        [BsonElement("age_from")]
        [JsonPropertyName("age_from")]
        public double? Age_from { get; set; }

        [BsonElement("age_to")]
        [JsonPropertyName("age_to")]
        public double? Age_to { get; set; }



        [BsonElement("_isType")]
        [JsonPropertyName("_isType")]
        public string? IsType { get; set; }

        [BsonElement("_iswith")]
        [JsonPropertyName("_iswith")]
        public string? Iswith { get; set; }

        [BsonElement("value")]
        [JsonPropertyName("value")]
        public double? Value { get; set; }

        [BsonElement("ls_Room")]
        [JsonPropertyName("ls_Room")]
        public IList<Room>? Ls_Room { get; set; }
    }
}
