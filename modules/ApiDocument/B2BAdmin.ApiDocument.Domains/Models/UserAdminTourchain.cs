using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    [BsonDiscriminator(Required = false)]
    public class UserAdminTourchain : UserAdmin
    {
        [BsonElement("IdAgency")]
        [JsonPropertyName("IdAgency")]
        public string? IdAgency { get; set; }

        [BsonElement("currencyRounding")]
        [JsonPropertyName("currencyRounding")]
        public string? CurrencyRounding { get; set; }

        [BsonElement("nation")]
        [JsonPropertyName("nation")]
        public string? nation { get; set; }
    }
    public class UserAccess
    {
        [BsonElement("userID")]
        [JsonPropertyName("userID")]
        public string? userID { get; set; }

        [BsonElement("nation")]
        [JsonPropertyName("nation")]
        public string? nation { get; set; }

        [BsonElement("IdAgency")]
        [JsonPropertyName("IdAgency")]
        public string? IdAgency { get; set; }

        [BsonElement("IsPass")]
        [JsonPropertyName("IsPass")]
        public bool? IsPass { get; set; }

        [BsonElement("currencyRounding")]
        [JsonPropertyName("currencyRounding")]
        public string? CurrencyRounding { get; set; }
    }


    [BsonIgnoreExtraElements]
    [BsonDiscriminator(Required = false)]
    public class UserAdmin : MongoBaseModel
    {
       

        [BsonElement("Username")]
        [JsonPropertyName("Username")]
        public string? Username { get; set; }

        [BsonElement("Password")]
        [JsonPropertyName("Password")]
        public string? Password { get; set; }

        [BsonElement("FullName")]
        [JsonPropertyName("FullName")]
        public string? FullName { get; set; }

        [BsonElement("Email")]
        [JsonPropertyName("Email")]
        public string? Email { get; set; }
    }
}
