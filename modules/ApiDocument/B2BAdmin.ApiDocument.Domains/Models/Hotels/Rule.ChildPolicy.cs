using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Hotels
{
    [BsonIgnoreExtraElements]
    public class RuleChildPolicy : MongoBaseModel
    {
        [BsonElement("typeCharge")]
        [JsonPropertyName("typeCharge")]
        public string? TypeCharge { get; set; }

        [BsonElement("value")]
        [JsonPropertyName("value")]
        public double? Value { get; set; }

        [BsonElement("ageFrom")]
        [JsonPropertyName("ageFrom")]
        public double? AgeFrom { get; set; }

        [BsonElement("ageTo")]
        [JsonPropertyName("ageTo")]
        public double? AgeTo { get; set; }

        [BsonElement("price")]
        [JsonPropertyName("price")]
        public double? Price { get; set; }

        [BsonElement("canEdit")]
        [JsonPropertyName("canEdit")]
        public bool? CanEdit { get; set; }

        /*
        const rules = new mongoose.Schema(
        {
            ls_child_policy: [{
                typeCharge: String,
                value: Number,
                ageFrom: Number,
                ageTo: Number,
                price: Number,
                canEdit: Boolean
            }]
        });
        */
    }
}
