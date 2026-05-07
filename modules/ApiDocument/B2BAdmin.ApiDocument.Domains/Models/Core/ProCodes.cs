using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class ProCodeFilter : ProCodes 
    {
        
        [BsonElement("ref")]
        [JsonPropertyName("ref")]
        public string? Ref { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [BsonElement("code")]
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [BsonElement("agencyTitle")]
        [JsonPropertyName("agencyTitle")]
        public string? AgencyTitle { get; set; }

        [BsonElement("contacts")]
        [JsonPropertyName("contacts")]
        public IList<AgencyContact> Contacts { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class partner_financial_statements : MongoBaseModel
    {
        [BsonElement("nation")]
        [JsonPropertyName("nation")]
        public string? Nation { get; set; }

        [BsonElement("md5code")]
        [JsonPropertyName("md5code")]
        public string? md5code { get; set; }

        [BsonElement("creditLimit")]
        [JsonPropertyName("creditLimit")]
        public decimal? CreditLimit { get; set; }

        [BsonElement("logs")]
        [JsonPropertyName("logs")]
        public List<creditLimitLogs>? logs { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class creditLimitLogs : MongoBaseModel
    {
        [BsonElement("strDate")]
        [JsonPropertyName("strDate")]
        public string? strDate { get; set; }

        [BsonElement("user")]
        [JsonPropertyName("user")]
        public string? user { get; set; }

        [BsonElement("dateUpdate")]
        [JsonPropertyName("dateUpdate")]
        public DateTime? dateUpdate { get; set; }

        [BsonElement("formCreditLimit")]
        [JsonPropertyName("formCreditLimit")]
        public decimal? FormCreditLimit { get; set; }

        [BsonElement("creditLimit")]
        [JsonPropertyName("creditLimit")]
        public decimal? CreditLimit { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class ProCodes : MongoBaseModel
    {
        [BsonElement("nation")]
        [JsonPropertyName("nation")]
        public string? Nation { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("gents_onlines")]
        [JsonPropertyName("gents_onlines")]
        public string? Gents_onlines { get; set; }


        [BsonElement("cname")]
        [JsonPropertyName("cname")]
        public string? Cname { get; set; }

        [BsonElement("productType")]
        [JsonPropertyName("productType")]
        public string? ProductType { get; set; }
        
        // Markup scheme section
        [BsonElement("IsMarkupSchemeApplied")]
        [JsonPropertyName("IsMarkupSchemeApplied")]
        public bool IsMarkupSchemeApplied { get; set; } = false;

        [BsonElement("isMarkupBreakDown")]
        [JsonPropertyName("isMarkupBreakDown")]
        public bool? IsMarkupBreakDown { get; set; } = false;

        [BsonElement("MarkupSchemeId")]
        [JsonPropertyName("MarkupSchemeId")]
        public string? MarkupSchemeId { get; set; }


        [BsonElement("chname")]
        [JsonPropertyName("chname")]
        public string? chname { get; set; }


        [BsonElement("cemail")]
        [JsonPropertyName("cemail")]
        public string? cemail { get; set; }

        [BsonElement("agid")]
        [JsonPropertyName("agid")]
        public string? agid { get; set; }

        [BsonElement("ccode")]
        [JsonPropertyName("ccode")]
        public string? ccode { get; set; }

        [BsonElement("passCode")]
        [JsonPropertyName("passCode")]
        public string? PassCode { get; set; }

        [BsonElement("md5code")]
        [JsonPropertyName("md5code")]
        public string? md5code { get; set; }

        [BsonElement("cmarket")]
        [JsonPropertyName("cmarket")]
        public string? cmarket { get; set; }

        [BsonElement("ccurrency")]
        [JsonPropertyName("ccurrency")]
        public string? ccurrency { get; set; }

        [BsonElement("language")]
        [JsonPropertyName("language")]
        public List<string>? Language { get; set; }

        [BsonElement("agencyType")]
        [JsonPropertyName("agencyType")]
        public string? agencyType { get; set; }

        [BsonElement("limitedOpenItems")]
        [JsonPropertyName("limitedOpenItems")]
        public double? limitedOpenItems { get; set; }

        [BsonElement("active")]
        [JsonPropertyName("active")]
        public bool? active { get; set; }

        [BsonElement("online")]
        [JsonPropertyName("online")]
        public  bool? online { get; set; }

        [BsonElement("private_code")]
        [JsonPropertyName("private_code")]
        public  bool? private_code { get; set; }

        [BsonElement("creditlimit")]
        [JsonPropertyName("creditlimit")]
        public string? creditlimit { get; set; }

        [BsonElement("credittype")]
        [JsonPropertyName("credittype")]
        public string? credittype { get; set; }

        [BsonElement("bankname")]
        [JsonPropertyName("bankname")]
        public string? bankname { get; set; }

        [BsonElement("accountname")]
        [JsonPropertyName("accountname")]
        public string? accountname { get; set; }

        [BsonElement("accountnumber")]
        [JsonPropertyName("accountnumber")]
        public double? accountnumber { get; set; }

        [BsonElement("swift")]
        [JsonPropertyName("swift")]
        public string? swift { get; set; }

        [BsonElement("taxoffice")]
        [JsonPropertyName("taxoffice")]
        public string? taxoffice { get; set; }

        [BsonElement("taxnumber")]
        [JsonPropertyName("taxnumber")]
        public double? taxnumber { get; set; }

        [BsonElement("banktel")]
        [JsonPropertyName("banktel")]
        public string? banktel { get; set; }

        [BsonElement("bankaddess")]
        [JsonPropertyName("bankaddess")]
        public string? bankaddess { get; set; }

        [BsonElement("isoReps")]
        [JsonPropertyName("isoReps")]
        public IList<IsoReps> IsoReps { get; set; }

        [BsonElement("period")]
        [JsonPropertyName("period")]
        public IList<PeriodOnline> Period { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class IsoReps : MongoBaseModel
    {

        [BsonElement("idReps")]
        [JsonPropertyName("idReps")]
        public string? IdReps { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [BsonElement("topupcommision")]
        [JsonPropertyName("topupcommision")]
        public double? Topupcommision { get; set; }

        [BsonElement("topupcommision_land")]
        [JsonPropertyName("topupcommision_land")]
        public double? Topupcommision_land { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class PeriodOnline : MongoBaseModel
    {
        [BsonElement("p_begin")]
        [JsonPropertyName("p_begin")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? p_begin { get; set; }

        [BsonElement("p_end")]
        [JsonPropertyName("p_end")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? p_end { get; set; }

        [BsonElement("p_htpro")]
        [JsonPropertyName("p_htpro")]
        public double? p_htpro { get; set; }

        [BsonElement("extraHtpro")]
        [JsonPropertyName("extraHtpro")]
        public double? ExtraHtpro { get; set; }

        [BsonElement("rangeMarkup")]
        [JsonPropertyName("rangeMarkup")]
        public Boolean? rangeMarkup { get; set; }

        [BsonElement("idRangeMarkup")]
        [JsonPropertyName("idRangeMarkup")]
        public string? idRangeMarkup { get; set; }

        [BsonElement("p_tourpro")]
        [JsonPropertyName("p_tourpro")]
        public double? p_tourpro { get; set; }

        [BsonElement("extraTourpro")]
        [JsonPropertyName("extraTourpro")]
        public double? ExtraTourpro { get; set; }

        [BsonElement("p_airfare")]
        [JsonPropertyName("p_airfare")]
        public double? p_airfare { get; set; }

        [BsonElement("p_buffer")]
        [JsonPropertyName("p_buffer")]
        public double? p_buffer { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class MessengerOnline : MongoBaseModel
    {
        [BsonElement("content")]
        [JsonPropertyName("content")]
        public string? content { get; set; }

        [BsonElement("byUser")]
        [JsonPropertyName("byUser")]
        public string? byUser { get; set; }

        [BsonElement("dated")]
        [JsonPropertyName("dated")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? dated { get; set; }
    }    
}