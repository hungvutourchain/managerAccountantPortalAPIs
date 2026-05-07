using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Hotels
{
    [BsonIgnoreExtraElements]
    public class RoomImage : MongoBaseModel
    {
        [BsonElement("filename")]
        [JsonPropertyName("filename")]
        public string? Filename { get; set; }

        [BsonElement("path")]
        [JsonPropertyName("path")]
        public string? Path { get; set; }

        [BsonElement("isDefault")]
        [JsonPropertyName("isDefault")]
        public bool? IsDefault { get; set; }

        /*
        const rooms = new mongoose.Schema(
        {
            images: [{
                filename: String,
                path: String,
                isDefault: Boolean
            }],
        });
        */
    }
}
