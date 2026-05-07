using CloudKit.Domain;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    public class LocationFormatType : EntityBase<string>
    {

        public byte? MaxLevel { get; set; }

        public string? Level1Name { get; set; }

        public string? Level2Name { get; set; }

        public string? Level3Name { get; set; }

        public string? Level4Name { get; set; }
    }
}