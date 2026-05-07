using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class siteTemplates : MongoBaseModel
    {
        [BsonElement("defaultCountry")]
        [JsonPropertyName("defaultCountry")]
        public string? defaultCountry { get; set; }

        [BsonElement("currencyRounding")]
        [JsonPropertyName("currencyRounding")]
        public string? CurrencyRounding { get; set; }

        [BsonElement("domain")]
        [JsonPropertyName("domain")]
        public string? domain { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? name { get; set; }

        [BsonElement("info")]
        [JsonPropertyName("info")]
        public string? info { get; set; }

        [BsonElement("IdTemplate")]
        [JsonPropertyName("IdTemplate")]
        public string? IdTemplate { get; set; }

        [BsonElement("isActive")]
        [JsonPropertyName("isActive")]
        public bool? isActive { get; set; }

        [BsonElement("header")]
        [JsonPropertyName("header")]
        public string? header { get; set; }

        [BsonElement("urlIconLogoHome")]
        [JsonPropertyName("urlIconLogoHome")]
        public string? urlIconLogoHome { get; set; }

        [BsonElement("imageLogo")]
        [JsonPropertyName("imageLogo")]
        public string? imageLogo { get; set; }

        [BsonElement("linkAdmin")]
        [JsonPropertyName("linkAdmin")]
        public string? linkAdmin { get; set; }

        [BsonElement("nameCompany")]
        [JsonPropertyName("nameCompany")]
        public string? nameCompany { get; set; }

        [BsonElement("heightHeaderLogo")]
        [JsonPropertyName("heightHeaderLogo")]
        public double? heightHeaderLogo { get; set; }

        [BsonElement("heightHeader")]
        [JsonPropertyName("heightHeader")]
        public double? heightHeader { get; set; }

        [BsonElement("iconLogoHome")]
        [JsonPropertyName("iconLogoHome")]
        public string? iconLogoHome { get; set; }

        [BsonElement("textIconLogoHome")]
        [JsonPropertyName("textIconLogoHome")]
        public string? textIconLogoHome { get; set; }

        [BsonElement("homeIconShow")]
        [JsonPropertyName("homeIconShow")]
        public bool? homeIconShow { get; set; }

        [BsonElement("colorBody")]
        [JsonPropertyName("colorBody")]
        public string? colorBody { get; set; }

        [BsonElement("backgroundBody")]
        [JsonPropertyName("backgroundBody")]
        public string? backgroundBody { get; set; }

        [BsonElement("active")]
        [JsonPropertyName("active")]
        public valueActive? active { get; set; }

        [BsonElement("deActive")]
        [JsonPropertyName("deActive")]
        public valueActive? deActive { get; set; }

        [BsonElement("colorProfile")]
        [JsonPropertyName("colorProfile")]
        public string? colorProfile { get; set; }

        [BsonElement("textColorBody")]
        [JsonPropertyName("textColorBody")]
        public string? textColorBody { get; set; }


        [BsonElement("tourShow")]
        [JsonPropertyName("tourShow")]
        public bool? tourShow { get; set; }

        [BsonElement("tourText")]
        [JsonPropertyName("tourText")]
        public string? tourText { get; set; }

        [BsonElement("tourUrl")]
        [JsonPropertyName("tourUrl")]
        public string? tourUrl { get; set; }

        [BsonElement("hotelsShow")]
        [JsonPropertyName("hotelsShow")]
        public bool? hotelsShow { get; set; }

        [BsonElement("hotelsText")]
        [JsonPropertyName("hotelsText")]
        public string? hotelsText { get; set; }

        [BsonElement("hotelsUrl")]
        [JsonPropertyName("hotelsUrl")]
        public string? hotelsUrl { get; set; }

        [BsonElement("requestShow")]
        [JsonPropertyName("requestShow")]
        public bool? requestShow { get; set; }

        [BsonElement("requesText")]
        [JsonPropertyName("requesText")]
        public string? requesText { get; set; }

        [BsonElement("requestUrl")]
        [JsonPropertyName("requestUrl")]
        public string? requestUrl { get; set; }


        [BsonElement("seatInCoachShow")]
        [JsonPropertyName("seatInCoachShow")]
        public bool? seatInCoachShow { get; set; }

        [BsonElement("seatInCoachText")]
        [JsonPropertyName("seatInCoachText")]
        public string? seatInCoachText { get; set; }

        [BsonElement("seatInCoachUrl")]
        [JsonPropertyName("seatInCoachUrl")]
        public string? seatInCoachUrl { get; set; }

        [BsonElement("sliders")]
        [JsonPropertyName("sliders")]
        public List<valuesliders>? sliders { get; set; }

        [BsonElement("headerTitleShow")]
        [JsonPropertyName("headerTitleShow")]
        public bool? headerTitleShow { get; set; }

        [BsonElement("headerTitleText")]
        [JsonPropertyName("headerTitleText")]
        public string? headerTitleText { get; set; }

        [BsonElement("colorHeaderTitle")]
        [JsonPropertyName("colorHeaderTitle")]
        public string? colorHeaderTitle { get; set; }

        [BsonElement("colorbackgroundContent")]
        [JsonPropertyName("colorbackgroundContent")]
        public string? colorbackgroundContent { get; set; }

        [BsonElement("defaultFontWeight")]
        [JsonPropertyName("defaultFontWeight")]
        public string? defaultFontWeight { get; set; }

        [BsonElement("fontName")]
        [JsonPropertyName("fontName")]
        public string? FontName { get; set; }

        [BsonElement("defaultFontUrl")]
        [JsonPropertyName("defaultFontUrl")]
        public string? defaultFontUrl { get; set; }

        [BsonElement("showUsefulInfo")]
        [JsonPropertyName("showUsefulInfo")]
        public bool? ShowUsefulInfo { get; set; }

        [BsonElement("usefulInfoText")]
        [JsonPropertyName("usefulInfoText")]
        public string? UsefulInfoText { get; set; }

        [BsonElement("usefulInfoList")]
        [JsonPropertyName("usefulInfoList")]
        public List<UsefulInformation>? UsefulInfoList { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class valueActive : MongoBaseModel
    {
        [BsonElement("color")]
        [JsonPropertyName("color")]
        public string? color { get; set; }

        [BsonElement("background")]
        [JsonPropertyName("background")]
        public string? background { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class UsefulInformation : MongoBaseModel
    {
        [BsonElement("languageId")]
        [JsonPropertyName("languageId")]
        public string? LanguageId { get; set; }

        [BsonElement("content")]
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [BsonElement("pageConfig")]
        [JsonPropertyName("pageConfig")]
        public valueActive? PageConfig { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class valuesliders : MongoBaseModel
    {
        [BsonElement("src")]
        [JsonPropertyName("src")]
        public string? src { get; set; }

        [BsonElement("sort")]
        [JsonPropertyName("sort")]
        public double? sort { get; set; }
    }
}
