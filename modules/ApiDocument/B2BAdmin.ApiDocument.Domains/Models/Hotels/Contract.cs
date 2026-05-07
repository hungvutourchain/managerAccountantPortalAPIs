using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Hotels
{
    [BsonIgnoreExtraElements]
    public class Contract : MongoBaseModel
    {
        [BsonElement("elseRules")]
        [JsonPropertyName("elseRules")]
        public bool? ElseRules { get; set; }

        [BsonElement("DMK")]
        [JsonPropertyName("DMK")]
        public bool? DMK { get; set; }

        [BsonElement("rangeMarkup")]
        [JsonPropertyName("rangeMarkup")]
        public bool? RangeMarkup { get; set; }

        [BsonElement("idRangeMarkup")]
        [JsonPropertyName("idRangeMarkup")]
        public string? IdRangeMarkup { get; set; }

        [BsonElement("valueDMK")]
        [JsonPropertyName("valueDMK")]
        public decimal? ValueDMK { get; set; }

        [BsonElement("_idContracts")]
        [JsonPropertyName("_idContracts")]
        public string? IdContract { get; set; }

        [BsonElement("isActive")]
        [JsonPropertyName("isActive")]
        public bool? IsActive { get; set; }

        [BsonElement("isPublishClient")]
        [JsonPropertyName("isPublishClient")]
        public bool? IsPublishClient { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [BsonElement("nameFull")]
        [JsonPropertyName("nameFull")]
        public string? NameFull { get; set; }

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

        [BsonElement("currency")]
        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [BsonElement("_currency")]
        [JsonPropertyName("_currency")]
        public string? _Currency { get; set; }

        [BsonElement("document")]
        [JsonPropertyName("document")]
        public string? Document { get; set; }



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
        public decimal? CommissionValue { get; set; }

        [BsonElement("cutoffPaymentRequirement")]
        [JsonPropertyName("cutoffPaymentRequirement")]
        public decimal? CutoffPaymentRequirement { get; set; }

        [BsonElement("CommissionBy")]
        [JsonPropertyName("CommissionBy")]
        public string? CommissionBy { get; set; }

        [BsonElement("rules")]
        [JsonPropertyName("rules")]
        public IList<Rule>? Rules { get; set; }

        [BsonElement("linkrules")]
        [JsonPropertyName("linkrules")]
        public IList<ContractLinkRule>? Linkrules { get; set; }



        [BsonElement("Internaluseonly")]
        [JsonPropertyName("Internaluseonly")]
        public bool? Internaluseonly { get; set; }

        [BsonElement("note_rules")]
        [JsonPropertyName("note_rules")]
        public string? Note_rules { get; set; }

        [BsonElement("market")]
        [JsonPropertyName("market")]
        public IList<string>? Market { get; set; }

        [BsonElement("exceptMarket")]
        [JsonPropertyName("exceptMarket")]
        public IList<string>? ExceptMarket { get; set; }

        [BsonElement("note")]
        [JsonPropertyName("note")]
        public string? Note { get; set; }

        [BsonElement("Promotion")]
        [JsonPropertyName("Promotion")]
        public string? Promotion { get; set; }

        [BsonElement("Refurbishments")]
        [JsonPropertyName("Refurbishments")]
        public string? Refurbishments { get; set; }

        [BsonElement("Description")]
        [JsonPropertyName("Description")]
        public string? Description { get; set; }

        [BsonElement("multiDayContent")]
        [JsonPropertyName("multiDayContent")]
        public bool? MultiDayContent { get; set; }

        [BsonElement("contents")]
        [JsonPropertyName("contents")]
        public IList<ContractContent>? Contents { get; set; }



        [BsonElement("ChlidPolicy")]
        [JsonPropertyName("ChlidPolicy")]
        public IList<ContractChildPolicy>? ChlidPolicy { get; set; }

        [BsonElement("note_chlidPolicy")]
        [JsonPropertyName("note_chlidPolicy")]
        public string? Note_chlidPolicy { get; set; }

        [BsonElement("note_cttrash")]
        [JsonPropertyName("note_cttrash")]
        public string? Note_cttrash { get; set; }

        [BsonElement("cancelation_policy")]
        [JsonPropertyName("cancelation_policy")]
        public IList<CancelationPolicy>? Cancelation_policy { get; set; }

        [BsonElement("deposit_policy")]
        [JsonPropertyName("deposit_policy")]
        public IList<Deposit_Policy> DepositPolicies { get; set; }

        [BsonElement("showRulesCancal")]
        [JsonPropertyName("showRulesCancal")]
        public bool? ShowRulesCancal { get; set; }

        [BsonElement("note_cancelation_policy")]
        [JsonPropertyName("note_cancelation_policy")]
        public string? Note_cancelation_policy { get; set; }
        
        

        [BsonElement("note_deposit_policy")]
        [JsonPropertyName("note_deposit_policy")]
        public string? Note_deposit_policy { get; set; }

        [BsonElement("periods")]
        [JsonPropertyName("periods")]
        public IList<ContractPeriod>? Periods { get; set; }

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




        [BsonElement("groupsize")]
        [JsonPropertyName("groupsize")]
        public decimal? Groupsize { get; set; } // default for 'GIT size from' in contract

        [BsonElement("lsMealPlanChild")]
        [JsonPropertyName("lsMealPlanChild")]
        public IList<Exbed>? LsMealPlanChild { get; set; }

        [BsonElement("lsChildSharing")]
        [JsonPropertyName("lsChildSharing")]
        public IList<ChildSharing>? LsChildSharing { get; set; }

        [BsonElement("reservation")]
        [JsonPropertyName("reservation")]
        public ContractReservation? Reservation { get; set; }
    }

    public class ContractContent : MongoBaseModel
    {
        [BsonElement("numberday")]
        [JsonPropertyName("numberday")]
        public decimal? Numberday { get; set; }

        [BsonElement("content")]
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
