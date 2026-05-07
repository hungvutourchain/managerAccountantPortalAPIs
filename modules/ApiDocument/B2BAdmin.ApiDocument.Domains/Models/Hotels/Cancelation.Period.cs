using System;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Hotels
{
    [BsonIgnoreExtraElements]
    public class CancelationPeriod : MongoBaseModel
    {
        [BsonElement("beginDate")]
        [JsonPropertyName("beginDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? BeginDate { get; set; }

        [BsonElement("endDate")]
        [JsonPropertyName("endDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? EndDate { get; set; }

        [BsonElement("timeZone")]
        [JsonPropertyName("timeZone")]
        public string? TimeZone { get; set; }

        /*
        const cancelation_policy = new mongoose.Schema(
        {
            periods_travel: [{
                beginDate: Date,
                endDate: Date,
            }],
            periods_booking: [{
                beginDate: Date,
                endDate: Date,
            }]
        });
        */
    }
}
