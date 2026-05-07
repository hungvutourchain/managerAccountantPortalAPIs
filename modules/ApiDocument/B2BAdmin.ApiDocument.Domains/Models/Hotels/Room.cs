using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Hotels
{
    [BsonIgnoreExtraElements]
    public class Room : MongoBaseModel
    {
        [BsonElement("_idRoom")]
        [JsonPropertyName("_idRoom")]
        public string? IdRoom { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        

        [BsonElement("isLeadRoom")]
        [JsonPropertyName("isLeadRoom")]
        public bool IsLeadRoom { get; set; }

        [BsonElement("quantity")]
        [JsonPropertyName("quantity")]
        public double? Quantity { get; set; }

        [BsonElement("options")]
        [JsonPropertyName("options")]
        public IList<RoomOption>? Options { get; set; }

        [BsonElement("images")]
        [JsonPropertyName("images")]
        public IList<RoomImage>? Images { get; set; }
        
        [BsonElement("roomOccupancyTemplates")]
        [JsonPropertyName("roomOccupancyTemplates")]
        public RoomOccupancyTemplate? RoomOccupancyTemplate { get; set; }

        /*
        const rooms = new mongoose.Schema(
        {
            _idRoom: String,
            name: String,
            options: [{
                occA: Number,
                occC: Number,
                childs: [{
                    num: Number,
                    from: Number,
                    to: Number
                }],
                extraBed: Boolean,
                noExtraBed: Number
            }],
            images: [{
                filename: String,
                path: String,
                isDefault: Boolean
            }],
            roomOccupancyTemplates: {
                name: String
            }
        });
        */
    }    
    public class RoomOccupancyTemplate
    {
        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
