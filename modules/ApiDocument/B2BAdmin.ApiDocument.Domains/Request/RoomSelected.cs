using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using B2BAdmin.ApiDocument.Domains.Models.Hotels;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class RoomSelected : Option
    {
        [BsonElement("lsMealPlanChildForRule")]
        [JsonPropertyName("lsMealPlanChildForRule")]
        public MealPlanForRule? LsMealPlanChildForRule { get; set; }

        [BsonElement("mealPlanAdultForRule")]
        [JsonPropertyName("mealPlanAdultForRule")]
        public MealPlanForRule? MealPlanAdultForRule { get; set; }

        [BsonElement("note")]
        [JsonPropertyName("note")]
        public string? Note { get; set; }

        [BsonElement("new")]
        [JsonPropertyName("new")]
        public bool? New { get; set; }

        [BsonElement("edit")]
        [JsonPropertyName("edit")]
        public bool? Edit { get; set; }

        [BsonElement("DayCurrent")]
        [JsonPropertyName("DayCurrent")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DayCurrent { get; set; }

        [BsonElement("_periodsBeginDate")]
        [JsonPropertyName("_periodsBeginDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? PeriodsBeginDate { get; set; }

        [BsonElement("_periodsEndDate")]
        [JsonPropertyName("_periodsEndDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? PeriodsEndDate { get; set; }

        [BsonElement("_idPeriods")]
        [JsonPropertyName("_idPeriods")]
        public string? IdPeriods { get; set; }

        [BsonElement("endDate")]
        [JsonPropertyName("endDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? EndDate { get; set; }

        [BsonElement("_isID")]
        [JsonPropertyName("_isID")]
        public string? IsID { get; set; }

        [BsonElement("isNight")]
        [JsonPropertyName("isNight")]
        public int IsNight { get; set; }

        [BsonElement("joinDay")]
        [JsonPropertyName("joinDay")]
        public string? JoinDay { get; set; }

        [BsonElement("numberOfNight")]
        [JsonPropertyName("numberOfNight")]
        public int? NumberOfNight { get; set; }

        [BsonElement("originPrice")]
        [JsonPropertyName("originPrice")]
        public double? OriginPrice { get; set; }

        [BsonElement("price")]
        [JsonPropertyName("price")]
        public double? Price { get; set; }

        [BsonElement("rulePrice")]
        [JsonPropertyName("rulePrice")]
        public double? RulePrice { get; set; }

        [BsonElement("unit")]
        [JsonPropertyName("unit")]
        public int? Unit { get; set; }

        [BsonElement("total")]
        [JsonPropertyName("total")]
        public double? Total { get; set; }

        [BsonElement("totalNoChild")]
        [JsonPropertyName("totalNoChild")]
        public double? TotalNoChild { get; set; }

        [BsonElement("action")]
        [JsonPropertyName("action")]
        public string? Action { get; set; }

        [BsonElement("ruleList")]
        [JsonPropertyName("ruleList")]
        public IList<Rule>? RuleList { get; set; }     

        [BsonElement("nameRule")]
        [JsonPropertyName("nameRule")]
        public string? NameRule { get; set; }

        [BsonElement("nameRoom")]
        [JsonPropertyName("nameRoom")]
        public string? NameRoom { get; set; }

        [BsonElement("runfirst")]
        [JsonPropertyName("runfirst")]
        public bool? Runfirst { get; set; }

        [BsonElement("listChildPrice")]
        [JsonPropertyName("listChildPrice")]
        public IList<ChildPrice>? ListChildPrice { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class MealPlanForRule
    {
        [BsonElement("unit")]
        [JsonPropertyName("unit")]
        public int? Unit { get; set; }

        [BsonElement("price")]
        [JsonPropertyName("price")]
        public double? Price { get; set; }

        [BsonElement("rulePrice")]
        [JsonPropertyName("rulePrice")]
        public double? RulePrice { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class ChildPrice
    {
        [BsonElement("childAge")]
        [JsonPropertyName("childAge")]
        public int? ChildAge { get; set; }

        [BsonElement("price")]
        [JsonPropertyName("price")]
        public double? Price { get; set; }
    }
}
