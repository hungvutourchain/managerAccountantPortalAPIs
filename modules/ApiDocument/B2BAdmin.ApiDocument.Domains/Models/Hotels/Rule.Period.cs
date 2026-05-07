using System;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Hotels
{
    [BsonIgnoreExtraElements]
    public class RulePeriod : MongoBaseModel
    {
        [BsonElement("perNights")]
        [JsonPropertyName("perNights")]
        public bool? PerNights { get; set; }

        [BsonElement("perStay")]
        [JsonPropertyName("perStay")]
        public bool? PerStay { get; set; }

        [BsonElement("withinperiod")]
        [JsonPropertyName("withinperiod")]
        public bool? Withinperiod { get; set; }

        [BsonElement("timeZone")]
        [JsonPropertyName("timeZone")]
        public string? TimeZone { get; set; }

        [BsonElement("beginDate")]
        [JsonPropertyName("beginDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? BeginDate { get; set; }

        [BsonElement("endDate")]
        [JsonPropertyName("endDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? EndDate { get; set; }

        /*
        const rules = new mongoose.Schema(
        {
            periods: [{
                perNights: Boolean,
                perStay: Boolean,
                withinperiod: Boolean,
                beginDate: Date,
                endDate: Date
            }],
        });
        */
    }
}
