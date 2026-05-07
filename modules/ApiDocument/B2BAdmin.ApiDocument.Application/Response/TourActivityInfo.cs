using System;
using System.Collections.Generic;

namespace B2BAdmin.ApiDocument.Application.Response
{

    public class TourActivityInfo
    {
        public BasicInfo? BasicInfo { get; set; }
        public ScheduleRs? Schedule { get; set; }
    }

    public class BasicInfo
    {
        public string? TourActivityID { get; set; }
        public TPA_Extensions? TPA_Extensions { get; set; }
    }

    public class TPA_Extensions
    {
        public Provider? Provider { get; set; }
        public string? FullName { get; set; }
        public string? tourCode { get; set; }
        public string? idPackage { get; set; }
        public int? ChildrenAge { get; set; }
        public decimal? InfantsAge { get; set; }
        public string? Category { get; set; }
        public MealPlan? MealPlan { get; set; }
        public Pricing? Pricing { get; set; }
        public PickupDropoff? PickupDropoff { get; set; }
    }

    public class Provider
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
    }

    public class Category
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
    }

    public class MealPlan
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
    }

    public class PickupDropoff
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? PickupInd { get; set; }
        public Country? Country { get; set; }
        public City? City { get; set; }
    }

    public class Country
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
    }

    public class City
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
    }

    public class ScheduleRs
    {
        public Summary? Summary { get; set; }
        public List<Detail>? Details { get; set; }
    }

    public class Summary
    {
        public string? Duration { get; set; }
        public string? Description { get; set; }
    }

    public class Detail
    {
        public DateTime? Start { get; set; }
        public TPA_Extensions? TPA_Extensions { get; set; }
    }

    public class Pricing
    {
        public decimal? baseChildrenPrice { get; set; }
        public decimal? sglSuppl { get; set; }
        public decimal? tripleDeduction { get; set; }
        public decimal? baseInfantPrice { get; set; }
        public double? childAgeMin { get; set; }
        public double? childAgeMax { get; set; }
        public string? CurrencyCode { get; set; }
        public decimal? Amount { get; set; }
        public List<ParticipantCount>? ParticipantCounts { get; set; }
        public bool? Confirmable { get; set; }
    }
    public class ParticipantCount
    {
        public int? Quantity { get; set; }
        public decimal? Amount { get; set; }
        public int? Age { get; set; }
        public string? QualifierInfo { get; set; } // Adult, Child, Infant
        public int? RoomIndex { get; set; }
    }
}
