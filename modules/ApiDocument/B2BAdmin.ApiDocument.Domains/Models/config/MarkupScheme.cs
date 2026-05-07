using System.Collections.Generic;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class MarkupScheme : MongoBaseModel
    {
        public bool? isDefault { get; set; } = false;
        public string? Name { get; set; }
        
        public string? Nation { get; set; }

        public IList<ServiceType>? ServiceTypes { get; set; }
    }
    
    [BsonIgnoreExtraElements]
    public class ServiceType : MongoBaseModel
    {
        public string? Name { get; set; }
        
        public string? Code { get; set; }
        
        public double Markup { get; set; } = 0;
    }
}