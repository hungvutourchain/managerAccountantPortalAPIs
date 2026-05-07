using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Hotels
{
    [BsonIgnoreExtraElements]
    public class RuleOptionByRoom : MongoBaseModel
    {
        [BsonElement("value")]
        [JsonPropertyName("value")]
        public double? Value { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [BsonElement("occ")]
        [JsonPropertyName("occ")]
        public int? Occ { get; set; }

        [BsonElement("_idRoom")]
        [JsonPropertyName("_idRoom")]
        public string? IdRoom { get; set; }

        [BsonElement("exbedA")]
        [JsonPropertyName("exbedA")]
        public bool? ExbedA { get; set; }

        [BsonElement("exbedC")]
        [JsonPropertyName("exbedC")]
        public bool? ExbedC { get; set; }

        [BsonElement("mealPlan")]
        [JsonPropertyName("mealPlan")]
        public bool? MealPlan { get; set; }

        [BsonElement("ls_child_policy")]
        [JsonPropertyName("ls_child_policy")]
        public IList<RuleChildPolicy>? Ls_child_policy { get; set; }

        /*
        const rules = new mongoose.Schema(
        {
            ls_option_by_rooms: [{
                value: Number,
                occ: Number,
                _idRoom: String,
                name: String,
                exbedA: Boolean,
                exbedC: Boolean,
                mealPlan: Boolean,
                ls_child_policy: [{
                    typeCharge: String,
                    value: Number,
                    ageFrom: Number,
                    ageTo: Number,
                    price: Number,
                    canEdit: Boolean
                }]
            }],
        });
        */
    }
}
