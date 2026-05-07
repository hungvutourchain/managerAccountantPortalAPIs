using CloudKit.Domain;
using Mapster;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    public class Country : LocationModel<LocationLevel1>
    {
        public string? Currency { get; set; }
        public string? Symbol { get; set; }
        public string? FormatTypeId {get;set;}
        public bool? isActive { get; set; }
        public int? Maxlv { get; set; }
        public IList<ActiveCountryFor>? ActiveCountryFor { get; set; }
    }
    public class ActiveCountryFor
    {
        public string? nation { get; set; }
        public bool? isActive { get; set; }
    }
    public class LocationLevel1 : LocationModel<LocationLevel2>{
        public LocationLevel1()
        {
            lv = 1;
        }
    }
    public class LocationLevel2 : LocationModel<LocationLevel3>{
        public LocationLevel2()
        {
            lv = 2;
        }
    }
    public class LocationLevel3 : LocationModel<LocationLevel4>{
        public LocationLevel3()
        {
            lv = 3;
        }
    }
    public class LocationLevel4 : LocationModel {
        public LocationLevel4()
        {
            lv = 4;
        }
    }
    public class LocationModelAdd : LocationModel {}

    // --------> LocationModel
    public class LocationModel : EntityBase<string>
    {
        public LocationModel():base()
        {
            Id = ObjectId.GenerateNewId().ToString();
        }
        public string? CountryId { get; set; }
        public string? FullParentId { get; set; }
        public string? Nation { get; set; }
        public string? FullName { get; set; }
        public string? ParentName { get; set; }
        public string? ShortName { get; set; }        
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? Tags { get; set; }
        public bool? IsMaster { get; set; }
        public int? Zoom { get; set; }
        public string ? strCount { get; set; }
        public string? Content { get; set; }
        public int? lv { get; set; }
        public IList<LocationModel>? Items { get; set; }
        public virtual void AddSubLocation(string fullParentId, object location)
        {
            
        }
        public virtual void RemoveSubLocation(string fullId)
        {

        }

        public virtual void UpdateSubLocation(string fullId, object location)
        {

        }
    }

    public class LocationModel<TSub> : LocationModel 
        where TSub : LocationModel
    {
        public override void AddSubLocation(string fullParentId, object location)
        {
            if ((string.IsNullOrEmpty(fullParentId) && string.IsNullOrEmpty(FullParentId))
               || ((string.IsNullOrEmpty(FullParentId) ? Id : (FullParentId + ":" + Id)) == fullParentId)
                    )
            {
                if (Items == null) Items = new List<LocationModel>();
                Items.Add(location.Adapt<TSub>());
                return;
            }
            if (this is LocationLevel4 | Items == null || Items.Count == 0)
                return;
            foreach (var item in Items)
            {
                item.AddSubLocation(fullParentId, location);
            }
        }
        public override void RemoveSubLocation(string fullId)
        {
            var itemRemoving = Items?.FirstOrDefault(x => x.FullParentId + ":" + x.Id == fullId);
            if (itemRemoving != null)
            {
                Items.Remove(itemRemoving);
                return;
            }

            if (this is LocationLevel4 || Items == null || Items.Count == 0)
                return;
            foreach (var item in Items)
            {
                item.RemoveSubLocation(fullId);
            }
        }
        public override void UpdateSubLocation(string fullId, dynamic location)
        {
            var itemRemoving = Items?.FirstOrDefault(x => (string.IsNullOrWhiteSpace(x.FullParentId) ? "" : x.FullParentId + ":") + x.Id == fullId);
            if (itemRemoving != null)
            {
                itemRemoving.Nation = location.Nation;
                itemRemoving.FullName = location.FullName;
                itemRemoving.ShortName = location.ShortName;               
                itemRemoving.Latitude = location.Latitude;
                itemRemoving.Longitude = location.Nation;
                itemRemoving.Zoom = location.Zoom;
                itemRemoving.Content = location.Content;
                itemRemoving.CountryId = location.CountryId;
                itemRemoving.Zoom = location.Zoom;

                return;
            }

            if (this is LocationLevel4 || Items == null || Items.Count == 0)
                return;
            foreach (var item in Items)
            {
                item.UpdateSubLocation(fullId, location);
            }
        }
    }
}