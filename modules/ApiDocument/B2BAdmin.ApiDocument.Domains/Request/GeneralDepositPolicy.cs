using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using B2BAdmin.ApiDocument.Domains.Models.Hotels;
using CloudKit.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    public class GeneralDepositPolicy : MongoBaseModel
    {
        public string? Nation { get; set; }
        
        public IList<Deposit_Policy> DepositPolicies { get; set; }
    }    
}
