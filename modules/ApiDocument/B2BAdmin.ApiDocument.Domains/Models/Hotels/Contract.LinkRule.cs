using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Hotels
{
    [BsonIgnoreExtraElements]
    public class ContractLinkRule : MongoBaseModel
    {
        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [BsonElement("optionRule")]
        [JsonPropertyName("optionRule")]
        public bool? OptionRule { get; set; }

        [BsonElement("_idGroup")]
        [JsonPropertyName("_idGroup")]
        public string? IdGroup { get; set; }

        [BsonElement("rules")]
        [JsonPropertyName("rules")]
        public IList<Rule>? Rules { get; set; }

        /*
        contracts: [{
            linkrules: [{
                name: String,
                _idGroup: String,
                rules: [rules]
            }],
        }],
        */
    }
}
