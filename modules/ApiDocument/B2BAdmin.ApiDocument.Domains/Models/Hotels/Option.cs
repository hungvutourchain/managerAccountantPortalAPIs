using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Hotels
{
    [BsonIgnoreExtraElements]
    public class Option : MongoBaseModel
    {
        [BsonElement("_idRoom")]
        [JsonPropertyName("_idRoom")]
        public string? IdRoom { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("RoomName")]
        public string? Name { get; set; }

        [BsonElement("textRoom")]
        [JsonPropertyName("OtherRoomName")]
        public string? TextRoom { get; set; }

        [BsonElement("_idOption")]
        [JsonPropertyName("_idOption")]
        public string? IdOption { get; set; }

        [BsonElement("isActive")]
        [JsonPropertyName("isActive")]
        public bool? IsActive { get; set; }

        [BsonElement("chargeType")]
        [JsonPropertyName("chargeType")]
        public string? ChargeType { get; set; }

        [BsonElement("mealPlan")]
        [JsonPropertyName("mealPlan")]
        public string? MealPlan { get; set; }

        [BsonElement("mealPlanAdult")]
        [JsonPropertyName("mealPlanAdult")]
        public double? MealPlanAdult { get; set; }

        //[BsonElement("mealPlanAdultOriginal")]
        //[JsonPropertyName("mealPlanAdultOriginal")]
        //public double? MealPlanAdultOriginal { get; set; }

        [BsonElement("fit")]
        [JsonPropertyName("fit")]
        public double? Fit { get; set; }

        //[BsonElement("fitOriginal")]
        //[JsonPropertyName("fitOriginal")]
        //public double? FitOriginal { get; set; }

        [BsonElement("fit_usd")]
        [JsonPropertyName("fit_usd")]
        public double? Fit_usd { get; set; }

        [BsonElement("perPerson")]
        [JsonPropertyName("perPerson")]
        public double? PerPerson { get; set; }

        [BsonElement("perPerson_usd")]
        [JsonPropertyName("perPerson_usd")]
        public double? PerPerson_usd { get; set; }
        
        [BsonElement("inputPrice")]
        [JsonPropertyName("inputPrice")]
        public double? inputPrice { get; set; }
        
        [BsonElement("pricePolicies")]
        [JsonPropertyName("pricePolicies")]
        public List<PricePolicies>? pricePolicies { get; set; }
        
        [BsonElement("finalPrice")]
        [JsonPropertyName("finalPrice")]
        public double? finalPrice { get; set; }

        [BsonElement("durationType")]
        [JsonPropertyName("durationType")]
        public string? durationType { get; set; }

        [BsonElement("packageDay")]
        [JsonPropertyName("packageDay")]
        public double? packageDay { get; set; }

        [BsonElement("packageNight")]
        [JsonPropertyName("packageNight")]
        public double? packageNight { get; set; }

        [BsonElement("git")]
        [JsonPropertyName("git")]
        public double? Git { get; set; }

        //[BsonElement("gitOriginal")]
        //[JsonPropertyName("gitOriginal")]
        //public double? GitOriginal { get; set; }

        [BsonElement("git_usd")]
        [JsonPropertyName("git_usd")]
        public double? Git_usd { get; set; }

        [BsonElement("additional")]
        [JsonPropertyName("additional")]
        public double? Additional { get; set; }

        [BsonElement("additional_usd")]
        [JsonPropertyName("additional_usd")]
        public double? Additional_usd { get; set; }

        [BsonElement("exbedA")]
        [JsonPropertyName("exbedA")]
        public Exbed? ExbedA { get; set; }

        //[BsonElement("exbedAOriginal")]
        //[JsonPropertyName("exbedAOriginal")]
        //public Exbed? ExbedAOriginal { get; set; }

        [BsonElement("exbedC")]
        [JsonPropertyName("exbedC")]
        public Exbed? ExbedC { get; set; }

        //[BsonElement("exbedCOriginal")]
        //[JsonPropertyName("exbedCOriginal")]
        //public Exbed? ExbedCOriginal { get; set; }

        [BsonElement("MealPlanChild")]
        [JsonPropertyName("MealPlanChild")]
        public Exbed? MealPlanChild { get; set; }

        //[BsonElement("MealPlanChildOriginal")]
        //[JsonPropertyName("MealPlanChildOriginal")]
        //public Exbed? MealPlanChildOriginal { get; set; }

        [BsonElement("lsMealPlanChild")]
        [JsonPropertyName("lsMealPlanChild")]
        public IList<Exbed>? LsMealPlanChild { get; set; }

        //[BsonElement("lsMealPlanChildOriginal")]
        //[JsonPropertyName("lsMealPlanChildOriginal")]
        //public IList<Exbed>? LsMealPlanChildOriginal { get; set; }

        [BsonElement("lsChildSharing")]
        [JsonPropertyName("lsChildSharing")]
        public IList<ChildSharing>? LsChildSharing { get; set; }

        [BsonElement("addOns")]
        [JsonPropertyName("addOns")]
        public IList<AddOns>? AddOns { get; set; }

        [BsonElement("applyPriceBeforeTax")]
        [JsonPropertyName("applyPriceBeforeTax")]
        public bool? applyPriceBeforeTax { get; set; }

        [BsonElement("beforeTax")]
        [JsonPropertyName("beforeTax")]
        public decimal? beforeTax { get; set; }

        [BsonElement("applyCommission")]
        [JsonPropertyName("applyCommission")]
        public bool? ApplyCommission { get; set; }

        [BsonElement("CommissionValue")]
        [JsonPropertyName("CommissionValue")]
        public double? CommissionValue { get; set; }

        [BsonElement("CommissionBy")]
        [JsonPropertyName("CommissionBy")]
        public string? CommissionBy { get; set; }

        [BsonElement("editCommission")]
        [JsonPropertyName("editCommission")]
        public bool? EditCommission { get; set; }

        [BsonElement("exbedACommission")]
        [JsonPropertyName("exbedACommission")]
        public Exbed? ExbedACommission { get; set; }

        [BsonElement("exbedCCommission")]
        [JsonPropertyName("exbedCCommission")]
        public Exbed? ExbedCCommission { get; set; }

        [BsonElement("MealPlanChildCommission")]
        [JsonPropertyName("MealPlanChildCommission")]
        public Exbed? MealPlanChildCommission { get; set; }

        [BsonElement("lsMealPlanChildCommission")]
        [JsonPropertyName("lsMealPlanChildCommission")]
        public IList<Exbed>? LsMealPlanChildCommission { get; set; }

        [BsonElement("otherOption")]
        [JsonPropertyName("otherOption")]
        public List<OtherOption>? OtherOption { get; set; }

        //[BsonElement("otherOptionOriginal")]
        //[JsonPropertyName("otherOptionOriginal")]
        //public List<OtherOption>? OtherOptionOriginal { get; set; }

        [BsonElement("groupsize")]
        [JsonPropertyName("groupsize")]
        public double? Groupsize { get; set; }

        [BsonElement("occ")]
        [JsonPropertyName("occ")]
        public double? Occ { get; set; } = 0;

        [BsonElement("allweek")]
        [JsonPropertyName("allweek")]
        public bool? Allweek { get; set; }

        [BsonElement("mon")]
        [JsonPropertyName("mon")]
        public bool? Mon { get; set; }

        [BsonElement("tue")]
        [JsonPropertyName("tue")]
        public bool? Tue { get; set; }

        [BsonElement("wed")]
        [JsonPropertyName("wed")]
        public bool? Wed { get; set; }

        [BsonElement("thu")]
        [JsonPropertyName("thu")]
        public bool? Thu { get; set; }

        [BsonElement("fri")]
        [JsonPropertyName("fri")]
        public bool? Fri { get; set; }

        [BsonElement("sat")]
        [JsonPropertyName("sat")]
        public bool? Sat { get; set; }

        [BsonElement("sun")]
        [JsonPropertyName("sun")]
        public bool? Sun { get; set; }

        [BsonElement("hasBreakfastFit")]
        [JsonPropertyName("hasBreakfastFit")]
        public bool? HasBreakfastFit { get; set; }

        [BsonElement("userCreate")]
        [JsonPropertyName("userCreate")]
        public string? UserCreate { get; set; }

        [BsonElement("userUpdate")]
        [JsonPropertyName("userUpdate")]
        public string? UserUpdate { get; set; }

        [BsonElement("DateCreate")]
        [JsonPropertyName("DateCreate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DateCreate { get; set; }

        [BsonElement("DateUpdate")]
        [JsonPropertyName("DateUpdate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DateUpdate { get; set; }

        [BsonElement("enddate")]
        [JsonPropertyName("enddate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? EndDate { get; set; }

        [BsonElement("begindate")]
        [JsonPropertyName("begindate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? BeginDate { get; set; }

        [BsonElement("numberOfNight")]
        [JsonPropertyName("numberOfNight")]
        public int NumberOfNight { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class PricePolicies : MongoBaseModel
    {
        [BsonElement("actionName")]
        [JsonPropertyName("actionName")]
        public string? actionName { get; set; }
        
        [BsonElement("action")]
        [JsonPropertyName("action")]
        public string? action { get; set; }
        
        [BsonElement("option")]
        [JsonPropertyName("option")]
        public string? option { get; set; }
        
        [BsonElement("value")]
        [JsonPropertyName("value")]
        public double? value { get; set; }
        
        [BsonElement("price")]
        [JsonPropertyName("price")]
        public double? price { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class OtherOption : MongoBaseModel
    {
        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [BsonElement("by")]
        [JsonPropertyName("by")]
        public string? By { get; set; }

        [BsonElement("value")]
        [JsonPropertyName("value")]
        public Double? value { get; set; }

        [BsonElement("byValue")]
        [JsonPropertyName("byValue")]
        public Double? byValue { get; set; }

        [BsonElement("modified")]
        [JsonPropertyName("modified")]
        public bool? modified { get; set; }

        [BsonElement("occ")]
        [JsonPropertyName("occupancy")]
        public Double? occ { get; set; }

        [BsonElement("from")]
        [JsonPropertyName("from")]
        public double? From { get; set; }

        [BsonElement("to")]
        [JsonPropertyName("to")]
        public double? To { get; set; }

        [BsonElement("priceUnit")]
        [JsonPropertyName("priceUnit")]
        public Double? PriceUnit { get; set; }

        [BsonElement("quality")]
        [JsonPropertyName("quality")]
        public Double? Quality { get; set; }

        [BsonElement("price")]
        [JsonPropertyName("price")]
        public Double? Price { get; set; }

        [BsonElement("sourcePrice")]
        [JsonPropertyName("sourcePrice")]
        public string? SourcePrice { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class AddOns : MongoBaseModel
    {
        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [BsonElement("isChecked")]
        [JsonPropertyName("isChecked")]
        public bool? IsChecked { get; set; }

        [BsonElement("value")]
        [JsonPropertyName("value")]
        public Double? value { get; set; }

        [BsonElement("price")]
        [JsonPropertyName("price")]
        public Double? Price { get; set; }

        [BsonElement("additional")]
        [JsonPropertyName("additional")]
        public Double? additional { get; set; }

        [BsonElement("from")]
        [JsonPropertyName("from")]
        public double? From { get; set; }

        [BsonElement("to")]
        [JsonPropertyName("to")]
        public double? To { get; set; }

        [BsonElement("note")]
        [JsonPropertyName("note")]
        public string? Note { get; set; }
    }
}
