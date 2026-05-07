using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Hotels
{
    [BsonIgnoreExtraElements]
    public class RoomChild : MongoBaseModel
    {
        [BsonElement("num")]
        [JsonPropertyName("num")]
        public int? Num { get; set; }

        [BsonElement("from")]
        [JsonPropertyName("from")]
        public double? From { get; set; }

        [BsonElement("to")]
        [JsonPropertyName("to")]
        public double? To { get; set; }

        /*
        childs: [{
            num: Number,
            from: Number,
            to: Number
        }],
        */
    }
}
