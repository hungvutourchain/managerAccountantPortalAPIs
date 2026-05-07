using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models.Hotels
{
    [BsonIgnoreExtraElements]
    public class ContractReservation
    {
        [BsonElement("CheckinHours")]
        [JsonPropertyName("CheckinHours")]
        public int? CheckinHours { get; set; }

        [BsonElement("checkout")]
        [JsonPropertyName("checkout")]
        public bool? Checkout { get; set; }

        [BsonElement("CheckinMinutes")]
        [JsonPropertyName("CheckinMinutes")]
        public int? CheckinMinutes { get; set; }

        [BsonElement("CheckoutHours")]
        [JsonPropertyName("CheckoutHours")]
        public int? CheckoutHours { get; set; }

        [BsonElement("CheckoutMinutes")]
        [JsonPropertyName("CheckoutMinutes")]
        public int? CheckoutMinutes { get; set; }

        [BsonElement("note")]
        [JsonPropertyName("note")]
        public string? Note { get; set; }



        [BsonElement("latecheckouts")]
        [JsonPropertyName("latecheckouts")]
        public IList<LateCheckout>? Latecheckouts { get; set; }

        [BsonElement("latecheckins")]
        [JsonPropertyName("latecheckins")]
        public IList<LateCheckout>? Latecheckins { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class LateCheckout : MongoBaseModel
    {
        [BsonElement("id")]
        [JsonPropertyName("id")]
        public string? SubId { get; set; }

        [BsonElement("LateCheckoutHours")]
        [JsonPropertyName("LateCheckoutHours")]
        public int? LateCheckoutHours { get; set; }

        [BsonElement("LateCheckoutMinutes")]
        [JsonPropertyName("LateCheckoutMinutes")]
        public int? LateCheckoutMinutes { get; set; }

        [BsonElement("LateCheckoutHoursTo")]
        [JsonPropertyName("LateCheckoutHoursTo")]
        public int? LateCheckoutHoursTo { get; set; }

        [BsonElement("LateCheckoutMinutesTo")]
        [JsonPropertyName("LateCheckoutMinutesTo")]
        public int? LateCheckoutMinutesTo { get; set; }

        [BsonElement("setAllRoom")]
        [JsonPropertyName("setAllRoom")]
        public bool? SetAllRoom { get; set; }

        [BsonElement("ls_Room")]
        [JsonPropertyName("ls_Room")]
        public IList<Room>? Ls_Room { get; set; }

        [BsonElement("typeCharge")]
        [JsonPropertyName("typeCharge")]
        public string? TypeCharge { get; set; } // 'Supplement', 'Deduct', 'Fixed'

        [BsonElement("typeBy")]
        [JsonPropertyName("typeBy")]
        public string? TypeBy { get; set; } // 'By %', 'By Amount'

        [BsonElement("value")]
        [JsonPropertyName("value")]
        public double? Value { get; set; }
    }
}
