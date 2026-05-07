using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Hotels
{
    [BsonIgnoreExtraElements]
    public class Rule : MongoBaseModel
    {
        [BsonElement("_isId")]
        [JsonPropertyName("_isId")]
        public string? IsId { get; set; }

        [BsonElement("_rulesID")]
        [JsonPropertyName("_rulesID")]
        public string? RulesID { get; set; }

        [BsonElement("ruleName")]
        [JsonPropertyName("ruleName")]
        public string? RuleName { get; set; }

        [BsonElement("type_Rule")]
        [JsonPropertyName("type_Rule")]
        public int? Type_Rule { get; set; }

        [BsonElement("numberDay")]
        [JsonPropertyName("numberDay")]
        public int? NumberDay { get; set; }

        [BsonElement("Date_before")]
        [JsonPropertyName("Date_before")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? Date_before { get; set; }

        [BsonElement("travelingdate")]
        [JsonPropertyName("travelingdate")]
        public bool? Travelingdate { get; set; }

        [BsonElement("bookingdate")]
        [JsonPropertyName("bookingdate")]
        public bool? Bookingdate { get; set; }

        [BsonElement("timeZone")]
        [JsonPropertyName("timeZone")]
        public string? TimeZone { get; set; }

        [BsonElement("bookingBeginDate")]
        [JsonPropertyName("bookingBeginDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? bookingBeginDate { get; set; }

        [BsonElement("bookingEndDate")]
        [JsonPropertyName("bookingEndDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? bookingEndDate { get; set; }

        /*
        const rules = new mongoose.Schema(
        {
            _isId: String,
            _rulesID: String,
            ruleName: String,
            type_Rule: Number,
            numberDay: Number,
            Date_before: Date,
            travelingdate: Boolean,
            bookingdate: Boolean,
            ...
        });
        */

        [BsonElement("note")]
        [JsonPropertyName("note")]
        public string? Note { get; set; }

        [BsonElement("accumulative")]
        [JsonPropertyName("accumulative")]
        public bool? Accumulative { get; set; }

        [BsonElement("Repeat")]
        [JsonPropertyName("Repeat")]
        public double? Repeat { get; set; }

        [BsonElement("fixed")]
        [JsonPropertyName("fixed")]
        public bool? Fixed { get; set; }

        [BsonElement("for_room")]
        [JsonPropertyName("for_room")]
        public double? for_room { get; set; }

        [BsonElement("for_occ")]
        [JsonPropertyName("for_occ")]
        public int? For_occ { get; set; }

        [BsonElement("ls_Room")]
        [JsonPropertyName("ls_Room")]
        public IList<Room>? Ls_Room { get; set; }

        [BsonElement("setAllRoom")]
        [JsonPropertyName("setAllRoom")]
        public bool? SetAllRoom { get; set; }

        [BsonElement("normal")]
        [JsonPropertyName("normal")]
        public bool? normal { get; set; }

        [BsonElement("promotion")]
        [JsonPropertyName("promotion")]
        public bool? promotion { get; set; }

        [BsonElement("surcharge")]
        [JsonPropertyName("surcharge")]
        public bool? surcharge { get; set; }



        /*
        const rules = new mongoose.Schema(
        {
            ...
            note: String,
            accumulative: Boolean,
            fixed: Boolean,
            for_room: Number,
            for_occ: Number,
            ls_Room: [rooms],
            setAllRoom: Boolean,
            ...
        });
        */

        [BsonElement("ls_option_by_rooms")]
        [JsonPropertyName("ls_option_by_rooms")]
        public IList<RuleOptionByRoom>? Ls_option_by_rooms { get; set; }

        /*
        const rules = new mongoose.Schema(
        {
            ...
            ls_option_by_rooms: [
                {
                    value: Number,
                    occ: Number,
                    _idRoom: String,
                    name: String,
                    exbedA: Boolean,
                    exbedC: Boolean,
                    mealPlan: Boolean,
                    ls_child_policy: [
                        {
                            typeCharge: String,
                            value: Number,
                            ageFrom: Number,
                            ageTo: Number,
                            price: Number,
                            canEdit: Boolean
                        }
                    ]
                }
            ],
            ...
        });
        */

        [BsonElement("periods")]
        [JsonPropertyName("periods")]
        public IList<RulePeriod>? Periods { get; set; }

        [BsonElement("ls_child_policy")]
        [JsonPropertyName("ls_child_policy")]
        public IList<RuleChildPolicy>? Ls_child_policy { get; set; }

        /*
        const rules = new mongoose.Schema(
        {
            ...
            periods: [{
                perNights: Boolean,
                perStay: Boolean,
                withinperiod: Boolean,
                beginDate: Date,
                endDate: Date
            }],
            ls_child_policy: [
                {
                    typeCharge: String,
                    value: Number,
                    ageFrom: Number,
                    ageTo: Number,
                    price: Number,
                    canEdit: Boolean
                }
            ]
        });
        */
    }
}
