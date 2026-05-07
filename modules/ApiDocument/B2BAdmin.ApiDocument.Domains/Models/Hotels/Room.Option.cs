using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Hotels
{
    [BsonIgnoreExtraElements]
    public class RoomOption : MongoBaseModel
    {
        [BsonElement("occA")]
        [JsonPropertyName("occA")]
        public int? OccA { get; set; }

        [BsonElement("occC")]
        [JsonPropertyName("occC")]
        public int? OccC { get; set; }

        [BsonElement("childs")]
        [JsonPropertyName("childs")]
        public IList<RoomChild> Childs { get; set; }

        [BsonElement("extraBed")]
        [JsonPropertyName("extraBed")]
        public bool? ExtraBed { get; set; }

        [BsonElement("noExtraBed")]
        [JsonPropertyName("noExtraBed")]
        public int? NoExtraBed { get; set; }

        /*
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
        */
    }    
}
