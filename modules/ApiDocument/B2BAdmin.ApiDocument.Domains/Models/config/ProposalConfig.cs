using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudKit.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    [BsonIgnoreExtraElements]
    public class ProposalConfig : MongoBaseModel
    {
        [BsonElement("showDate")]
        [JsonPropertyName("showDate")]
        public bool? showDate { get; set; }

        [BsonElement("nation")]
        [JsonPropertyName("nation")]
        public string? Nation { get; set; }

        [BsonElement("bookmark")]
        [JsonPropertyName("bookmark")]
        public bool? Bookmark { get; set; }

        [BsonElement("bookmarkProposal")]
        [JsonPropertyName("bookmarkProposal")]
        public bool? BookmarkProposal { get; set; }

        [BsonElement("finalProposal")]
        [JsonPropertyName("finalProposal")]
        public bool? FinalProposal { get; set; }

        [BsonElement("finalProposalBookmark")]
        [JsonPropertyName("finalProposalBookmark")]
        public bool? FinalProposalBookmark { get; set; }

        [BsonElement("isProposalForTA")]
        [JsonPropertyName("isProposalForTA")]
        public bool? IsProposalForTA { get; set; }

        [BsonElement("textNote")]
        [JsonPropertyName("textNote")]
        public string? TextNote { get; set; }

        [BsonElement("quotationSectionFooter")]
        [JsonPropertyName("quotationSectionFooter")]
        public bool? quotationSectionFooter { get; set; }

        [BsonElement("templateForGuide")]
        [JsonPropertyName("templateForGuide")]
        public bool? templateForGuide { get; set; }

        [BsonElement("idLanguage")]
        [JsonPropertyName("idLanguage")]
        public string? idLanguage { get; set; }

        [BsonElement("thumbnail")]
        [JsonPropertyName("thumbnail")]
        public string? thumbnail { get; set; }

        [BsonElement("Note")]
        [JsonPropertyName("Note")]
        public string? Note { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string? name { get; set; }

        [BsonElement("code")]
        [JsonPropertyName("code")]
        public string? code { get; set; }

        [BsonElement("Day")]
        [JsonPropertyName("Day")]
        public string? Day { get; set; }


        [BsonElement("keyBreakfast")]
        [JsonPropertyName("keyBreakfast")]
        public string? KeyBreakfast { get; set; }

        [BsonElement("Inclusive")]
        [JsonPropertyName("Inclusive")]
        public string? Inclusive { get; set; }


        [BsonElement("Exclusive")]
        [JsonPropertyName("Exclusive")]
        public string? Exclusive { get; set; }

        [BsonElement("dateFormat")]
        [JsonPropertyName("dateFormat")]
        public string? dateFormat { get; set; }

        [BsonElement("dateFormatExport")]
        [JsonPropertyName("dateFormatExport")]
        public string? dateFormatExport { get; set; }

        [BsonElement("travelPeriodStart")]
        [JsonPropertyName("travelPeriodStart")]
        public string? travelPeriodStart { get; set; }

        [BsonElement("travelPeriodEnd")]
        [JsonPropertyName("travelPeriodEnd")]
        public string? travelPeriodEnd { get; set; }

        [BsonElement("dateTimeFormat")]
        [JsonPropertyName("dateTimeFormat")]
        public string? dateTimeFormat { get; set; }

        [BsonElement("TitelTransfer")]
        [JsonPropertyName("TitelTransfer")]
        public string? TitelTransfer { get; set; }

        [BsonElement("From")]
        [JsonPropertyName("From")]
        public string? From { get; set; }

        [BsonElement("To")]
        [JsonPropertyName("To")]
        public string? To { get; set; }

        [BsonElement("PickupTime")]
        [JsonPropertyName("PickupTime")]
        public string? PickupTime { get; set; }

        [BsonElement("PickupPoint")]
        [JsonPropertyName("PickupPoint")]
        public string? PickupPoint { get; set; }

        [BsonElement("DropOffPoint")]
        [JsonPropertyName("DropOffPoint")]
        public string? DropOffPoint { get; set; }

        [BsonElement("Duration")]
        [JsonPropertyName("Duration")]
        public string? Duration { get; set; }

        [BsonElement("VehicleType")]
        [JsonPropertyName("VehicleType")]
        public string? VehicleType { get; set; }

        [BsonElement("Airlines")]
        [JsonPropertyName("Airlines")]
        public string? Airlines { get; set; }

        [BsonElement("Airfare")]
        [JsonPropertyName("Airfare")]
        public string? Airfare { get; set; }


        [BsonElement("Visatype")]
        [JsonPropertyName("Visatype")]
        public string? Visatype { get; set; }

        [BsonElement("Cost")]
        [JsonPropertyName("Cost")]
        public string? Cost { get; set; }

        [BsonElement("txtVisaSupplement")]
        [JsonPropertyName("txtVisaSupplement")]
        public string? TxtVisaSupplement { get; set; }

        [BsonElement("txtAirfareSupplement")]
        [JsonPropertyName("txtAirfareSupplement")]
        public string? txtAirfareSupplement { get; set; }

        [BsonElement("txtTotalServicePrice")]
        [JsonPropertyName("txtTotalServicePrice")]
        public string? txtTotalServicePrice { get; set; }

        [BsonElement("txtHotelNameaddress")]
        [JsonPropertyName("txtHotelNameaddress")]
        public string? txtHotelNameaddress { get; set; }

        [BsonElement("txtCheckIn")]
        [JsonPropertyName("txtCheckIn")]
        public string? txtCheckIn { get; set; }

        [BsonElement("txtCheckOut")]
        [JsonPropertyName("txtCheckOut")]
        public string? txtCheckOut { get; set; }

        [BsonElement("txtHotelInformation")]
        [JsonPropertyName("txtHotelInformation")]
        public string? txtHotelInformation { get; set; }

        [BsonElement("txtNo")]
        [JsonPropertyName("txtNo")]
        public string? txtNo { get; set; }

        [BsonElement("txtGender")]
        [JsonPropertyName("txtGender")]
        public string? txtGender { get; set; }

        [BsonElement("txtPassportnumber")]
        [JsonPropertyName("txtPassportnumber")]
        public string? txtPassportnumber { get; set; }

        [BsonElement("PassengerName")]
        [JsonPropertyName("PassengerName")]
        public string? PassengerName { get; set; }

        [BsonElement("Transfer")]
        [JsonPropertyName("Transfer")]
        public string? Transfer { get; set; }

        [BsonElement("Price")]
        [JsonPropertyName("Price")]
        public string? Price { get; set; }

        [BsonElement("Meals")]
        [JsonPropertyName("Meals")]
        public string? Meals { get; set; }

        [BsonElement("listDayTour")]
        [JsonPropertyName("listDayTour")]
        public bool? listDayTour { get; set; }

        [BsonElement("outLineItineraryTable")]
        [JsonPropertyName("outLineItineraryTable")]
        public bool? OutLineItineraryTable { get; set; }

        [BsonElement("onlyLocation")]
        [JsonPropertyName("onlyLocation")]
        public bool? onlyLocation { get; set; }
        
        [BsonElement("Flight")]
        [JsonPropertyName("Flight")]
        public string? Flight { get; set; }

        [BsonElement("FlightInfo")]
        [JsonPropertyName("FlightInfo")]
        public string? FlightInfo { get; set; }

        [BsonElement("Departs")]
        [JsonPropertyName("Departs")]
        public string? Departs { get; set; }

        [BsonElement("Arrives")]
        [JsonPropertyName("Arrives")]
        public string? Arrives { get; set; }

        [BsonElement("Luggageallowance")]
        [JsonPropertyName("Luggageallowance")]
        public string? Luggageallowance { get; set; }

        [BsonElement("Unit")]
        [JsonPropertyName("Unit")]
        public string? Unit { get; set; }

        [BsonElement("flightnumber")]
        [JsonPropertyName("flightnumber")]
        public string? flightnumber { get; set; }

        [BsonElement("TrainInfor")]
        [JsonPropertyName("TrainInfor")]
        public string? TrainInfor { get; set; }

        [BsonElement("flightSummary")]
        [JsonPropertyName("flightSummary")]
        public string? flightSummary { get; set; }

        [BsonElement("Route")]
        [JsonPropertyName("Route")]
        public string? Route { get; set; }

        [BsonElement("Date")]
        [JsonPropertyName("Date")]
        public string? Date { get; set; }

        [BsonElement("Dates")]
        [JsonPropertyName("Dates")]
        public string? Dates { get; set; }

        [BsonElement("DateBegin")]
        [JsonPropertyName("DateBegin")]
        public string? DateBegin { get; set; }

        [BsonElement("DateEnd")]
        [JsonPropertyName("DateEnd")]
        public string? DateEnd { get; set; }

        [BsonElement("Destination")]
        [JsonPropertyName("Destination")]
        public string? Destination { get; set; }

        [BsonElement("Accomodation")]
        [JsonPropertyName("Accomodation")]
        public string? Accomodation { get; set; }

        [BsonElement("Time")]
        [JsonPropertyName("Time")]
        public string? Time { get; set; }

        [BsonElement("Departure")]
        [JsonPropertyName("Departure")]
        public string? Departure { get; set; }

        [BsonElement("arrival")]
        [JsonPropertyName("arrival")]
        public string? arrival { get; set; }

        [BsonElement("Class")]
        [JsonPropertyName("Class")]
        public string? Class { get; set; }

        [BsonElement("BookedBy")]
        [JsonPropertyName("BookedBy")]
        public string? BookedBy { get; set; }

        [BsonElement("Status")]
        [JsonPropertyName("Status")]
        public string? Status { get; set; }

        [BsonElement("tilteHotels")]
        [JsonPropertyName("tilteHotels")]
        public string? tilteHotels { get; set; }
        
        
     

        [BsonElement("City")]
        [JsonPropertyName("City")]
        public string? City { get; set; }

        [BsonElement("Hotel")]
        [JsonPropertyName("Hotel")]
        public string? Hotel { get; set; }

        [BsonElement("RoomCategory")]
        [JsonPropertyName("RoomCategory")]
        public string? RoomCategory { get; set; }

        [BsonElement("Address")]
        [JsonPropertyName("Address")]
        public string? Address { get; set; }

        [BsonElement("Nights")]
        [JsonPropertyName("Nights")]
        public string? Nights { get; set; }

        [BsonElement("DateArrDep")]
        [JsonPropertyName("DateArrDep")]
        public string? DateArrDep { get; set; }

        [BsonElement("Accommodation")]
        [JsonPropertyName("Accommodation")]
        public string? Accommodation { get; set; }

        [BsonElement("ClientInformation")]
        [JsonPropertyName("ClientInformation")]
        public string? ClientInformation { get; set; }

        [BsonElement("FullName")]
        [JsonPropertyName("FullName")]
        public string? FullName { get; set; }

        [BsonElement("Surname")]
        [JsonPropertyName("Surname")]
        public string? Surname { get; set; }

        [BsonElement("paxName")]
        [JsonPropertyName("paxName")]
        public string? paxName { get; set; }

        [BsonElement("sex")]
        [JsonPropertyName("sex")]
        public string? Sex { get; set; }

        [BsonElement("txtRoom")]
        [JsonPropertyName("txtRoom")]
        public string? txtRoom { get; set; }

        [BsonElement("DOB")]
        [JsonPropertyName("DOB")]
        public string? DOB { get; set; }

        [BsonElement("PASSPORT")]
        [JsonPropertyName("PASSPORT")]
        public string? PASSPORT { get; set; }

        [BsonElement("PASSPORTEXP")]
        [JsonPropertyName("PASSPORTEXP")]
        public string? PASSPORTEXP { get; set; }

        [BsonElement("txtlanguage")]
        [JsonPropertyName("txtlanguage")]
        public string? txtlanguage { get; set; }

        [BsonElement("txtGuideTitle")]
        [JsonPropertyName("txtGuideTitle")]
        public string? txtGuideTitle { get; set; }

        [BsonElement("txtGuideName")]
        [JsonPropertyName("txtGuideName")]
        public string? txtGuideName { get; set; }

        [BsonElement("txtGuidePhone")]
        [JsonPropertyName("txtGuidePhone")]
        public string? txtGuidePhone { get; set; }

        [BsonElement("Requirement")]
        [JsonPropertyName("Requirement")]
        public string? Requirement { get; set; }

        [BsonElement("TravelWith")]
        [JsonPropertyName("TravelWith")]
        public string? TravelWith { get; set; }

        [BsonElement("NATIONALITY")]
        [JsonPropertyName("NATIONALITY")]
        public string? NATIONALITY { get; set; }

        [BsonElement("Commissionable")]
        [JsonPropertyName("Commissionable")]
        public string? Commissionable { get; set; }

        [BsonElement("perperson")]
        [JsonPropertyName("perperson")]
        public string? perperson { get; set; }

        [BsonElement("LANDTOURCOST")]
        [JsonPropertyName("LANDTOURCOST")]
        public string? LANDTOURCOST { get; set; }

        [BsonElement("TOTALLANDING")]
        [JsonPropertyName("TOTALLANDING")]
        public string? TOTALLANDING { get; set; }

        [BsonElement("TOTALACCOMMODATION")]
        [JsonPropertyName("TOTALACCOMMODATION")]
        public string? TOTALACCOMMODATION { get; set; }

        [BsonElement("TOTALGROUNDCHILD")]
        [JsonPropertyName("TOTALGROUNDCHILD")]
        public string? TOTALGROUNDCHILD { get; set; }

        [BsonElement("SERVICENAME")]
        [JsonPropertyName("SERVICENAME")]
        public string? SERVICENAME { get; set; }

        [BsonElement("ServiceType")]
        [JsonPropertyName("ServiceType")]
        public string? ServiceType { get; set; }

        [BsonElement("GrandTotal")]
        [JsonPropertyName("GrandTotal")]
        public string? GrandTotal { get; set; }

        [BsonElement("Pax")]
        [JsonPropertyName("Pax")]
        public string? Pax { get; set; }

        [BsonElement("IsName")]
        [JsonPropertyName("IsName")]
        public string? IsName { get; set; }

        [BsonElement("Termsconditions")]
        [JsonPropertyName("Termsconditions")]
        public string? Termsconditions { get; set; }

        [BsonElement("SGLSuppl")]
        [JsonPropertyName("SGLSuppl")]
        public string? SGLSuppl { get; set; }

        [BsonElement("Option")]
        [JsonPropertyName("Option")]
        public string? Option { get; set; }

        [BsonElement("TourQuote")]
        [JsonPropertyName("TourQuote")]
        public string? TourQuote { get; set; }

        [BsonElement("detailedItinerary")]
        [JsonPropertyName("detailedItinerary")]
        public string? DetailedItinerary { get; set; }

        [BsonElement("detailedItineraryAlign")]
        [JsonPropertyName("detailedItineraryAlign")]
        public string? DetailedItineraryAlign { get; set; }

        [BsonElement("endOfOurServices")]
        [JsonPropertyName("endOfOurServices")]
        public string? EndOfOurServices { get; set; }

        [BsonElement("endOfOurServicesAlign")]
        [JsonPropertyName("endOfOurServicesAlign")]
        public string? EndOfOurServicesAlign { get; set; }

        [BsonElement("endOfOurServicesImage")]
        [JsonPropertyName("endOfOurServicesImage")]
        public string? endOfOurServicesImage { get; set; }

        [BsonElement("headerImage")]
        [JsonPropertyName("headerImage")]
        public string? headerImage { get; set; }

        [BsonElement("itineraryInDetails")]
        [JsonPropertyName("itineraryInDetails")]
        public string? itineraryInDetails { get; set; }

        [BsonElement("yourQuotation")]
        [JsonPropertyName("yourQuotation")]
        public string? yourQuotation { get; set; }

        [BsonElement("yourHotels")]
        [JsonPropertyName("yourHotels")]
        public string? yourHotels { get; set; }

        [BsonElement("dailyTemplate")]
        [JsonPropertyName("dailyTemplate")]
        public string? dailyTemplate { get; set; }

        [BsonElement("alignmentHSpace")]
        [JsonPropertyName("alignmentHSpace")]
        public string? alignmentHSpace { get; set; }

        [BsonElement("alignmentVSpace")]
        [JsonPropertyName("alignmentVSpace")]
        public string? alignmentVSpace { get; set; }

        [BsonElement("TableOfContents")]
        [JsonPropertyName("TableOfContents")]
        public string? TableOfContents { get; set; }

        [BsonElement("numberOfSharingRoom")]
        [JsonPropertyName("numberOfSharingRoom")]
        public string? numberOfSharingRoom { get; set; }

        [BsonElement("numberOfSingleRoom")]
        [JsonPropertyName("numberOfSingleRoom")]
        public string? numberOfSingleRoom { get; set; }

        [BsonElement("managementFee")]
        [JsonPropertyName("managementFee")]
        public string? managementFee { get; set; }

        [BsonElement("totalTourLeader")]
        [JsonPropertyName("totalTourLeader")]
        public string? totalTourLeader { get; set; }

        [BsonElement("totalLandingService")]
        [JsonPropertyName("totalLandingService")]
        public string? totalLandingService { get; set; }

        [BsonElement("totalTextAccommodation")]
        [JsonPropertyName("totalTextAccommodation")]
        public string? totalTextAccommodation { get; set; }

        [BsonElement("totalService")]
        [JsonPropertyName("totalService")]
        public string? totalService { get; set; }

        [BsonElement("YourTravelRoutes")]
        [JsonPropertyName("YourTravelRoutes")]
        public string? YourTravelRoutes { get; set; }

        [BsonElement("ProposalInformation")]
        [JsonPropertyName("ProposalInformation")]
        public string? ProposalInformation { get; set; }

        [BsonElement("DestinationOverview")]
        [JsonPropertyName("DestinationOverview")]
        public string? DestinationOverview { get; set; }
        
        [BsonElement("coverImageOverview")]
        [JsonPropertyName("coverImageOverview")]
        public string? coverImageOverview { get; set; }
        
        [BsonElement("includeExcludeViews")]
        [JsonPropertyName("includeExcludeViews")]
        public string? includeExcludeViews { get; set; }
        

        [BsonElement("DailyTitle")]
        [JsonPropertyName("DailyTitle")]
        public string? DailyTitle { get; set; }

        [BsonElement("wordTemplate")]
        [JsonPropertyName("wordTemplate")]
        public string? wordTemplate { get; set; }

        [BsonElement("wordFooter")]
        [JsonPropertyName("wordFooter")]
        public string? wordFooter { get; set; }

        [BsonElement("font")]
        [JsonPropertyName("font")]
        public string? font { get; set; }

        [BsonElement("fontWeb")]
        [JsonPropertyName("fontWeb")]
        public string? fontWeb { get; set; }

        [BsonElement("fontSize")]
        [JsonPropertyName("fontSize")]
        public double? FontSize { get; set; }

        [BsonElement("fontColor")]
        [JsonPropertyName("fontColor")]
        public string? fontColor { get; set; }

        [BsonElement("customFontColor")]
        [JsonPropertyName("customFontColor")]
        public bool? customFontColor { get; set; }

        [BsonElement("numberLengthText")]
        [JsonPropertyName("numberLengthText")]
        public double NumberLengthText { get; set; }

        [BsonElement("fontType")]
        [JsonPropertyName("fontType")]
        public string? fontType { get; set; }

        [BsonElement("icons")]
        [JsonPropertyName("icons")]
        public List<ProposalConfigIcons>? icons { get; set; }

        [BsonElement("listValues")]
        [JsonPropertyName("listValues")]
        public List<ProposalConfiglistValues>? listValues { get; set; }

        [BsonElement("listImgCover")]
        [JsonPropertyName("listImgCover")]
        public List<string>? listImgCover { get; set; }

        [BsonElement("vlMenu")]
        [JsonPropertyName("vlMenu")]
        public List<ProposalConfigVlMenu>? vlMenu { get; set; }


        [BsonElement("priceCardSimply")]
        [JsonPropertyName("priceCardSimply")]
        public bool? priceCardSimply { get; set; }

        [BsonElement("accommodationOption")]
        [JsonPropertyName("accommodationOption")]
        public string? accommodationOption { get; set; }

        [BsonElement("optionalServices")]
        [JsonPropertyName("optionalServices")]
        public bool? optionalServices { get; set; }

        [BsonElement("accommodationSummary")]
        [JsonPropertyName("accommodationSummary")]
        public bool? accommodationSummary { get; set; }

        [BsonElement("accommodationUpgrade")]
        [JsonPropertyName("accommodationUpgrade")]
        public bool? accommodationUpgrade { get; set; }

        [BsonElement("flightJourney")]
        [JsonPropertyName("flightJourney")]
        public bool? flightJourney { get; set; }

        [BsonElement("depositPolicy")]
        [JsonPropertyName("depositPolicy")]
        public bool? depositPolicy { get; set; }

        [BsonElement("landServicePricePerPerson")]
        [JsonPropertyName("landServicePricePerPerson")]
        public bool? landServicePricePerPerson { get; set; }

        [BsonElement("landServiceTotalPrice")]
        [JsonPropertyName("landServiceTotalPrice")]
        public bool? landServiceTotalPrice { get; set; }

        [BsonElement("accommodationPricePerPerson")]
        [JsonPropertyName("accommodationPricePerPerson")]
        public bool? accommodationPricePerPerson { get; set; }

        [BsonElement("AccommodationTotalPrice")]
        [JsonPropertyName("AccommodationTotalPrice")]
        public bool? AccommodationTotalPrice { get; set; }

        [BsonElement("packagePricePerPerson")]
        [JsonPropertyName("packagePricePerPerson")]
        public bool? packagePricePerPerson { get; set; }

        [BsonElement("totalPackageCost")]
        [JsonPropertyName("totalPackageCost")]
        public bool? totalPackageCost { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class ProposalConfigIcons : MongoBaseModel
    {
        [BsonElement("thumbnail")]
        [JsonPropertyName("thumbnail")]
        public string? thumbnail { get; set; }

        [BsonElement("CodeIcon")]
        [JsonPropertyName("CodeIcon")]
        public string? CodeIcon { get; set; }

        [BsonElement("text")]
        [JsonPropertyName("text")]
        public string? text { get; set; }

        [BsonElement("width")]
        [JsonPropertyName("width")]
        public int? width { get; set; }

        [BsonElement("height")]
        [JsonPropertyName("height")]
        public int? height { get; set; }

    }
    [BsonIgnoreExtraElements]
    public class ProposalConfiglistValues : MongoBaseModel
    {
        [BsonElement("isActive")]
        [JsonPropertyName("isActive")]
        public bool? isActive { get; set; }

        [BsonElement("types")]
        [JsonPropertyName("types")]
        public string? types { get; set; }

        [BsonElement("itemId")]
        [JsonPropertyName("itemId")]
        public string? itemId { get; set; }

        [BsonElement("header")]
        [JsonPropertyName("header")]
        public string? header { get; set; }

        [BsonElement("content")]
        [JsonPropertyName("content")]
        public string? content { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class ProposalConfigVlMenu : MongoBaseModel
    {
        [BsonElement("text")]
        [JsonPropertyName("text")]
        public string? text { get; set; }

        [BsonElement("key")]
        [JsonPropertyName("key")]
        public string? key { get; set; }

        [BsonElement("value")]
        [JsonPropertyName("value")]
        public string? value { get; set; }

        [BsonElement("show")]
        [JsonPropertyName("show")]
        public bool? show { get; set; }

        [BsonElement("items")]
        [JsonPropertyName("items")]
        public List<ProposalConfigVlMenItems>? items { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class ProposalConfigVlMenItems : MongoBaseModel
    {
        [BsonElement("text")]
        [JsonPropertyName("text")]
        public string? text { get; set; }

        [BsonElement("key")]
        [JsonPropertyName("key")]
        public string? key { get; set; }


        [BsonElement("value")]
        [JsonPropertyName("value")]
        public string? value { get; set; }

        [BsonElement("show")]
        [JsonPropertyName("show")]
        public bool? show { get; set; }

        [BsonElement("footer")]
        [JsonPropertyName("footer")]
        public bool? footer { get; set; }

        [BsonElement("showTextNumberDay")]
        [JsonPropertyName("showTextNumberDay")]
        public bool? showTextNumberDay { get; set; }

        [BsonElement("toLowerCase")]
        [JsonPropertyName("toLowerCase")]
        public bool? toLowerCase { get; set; }

        [BsonElement("showModuleName")]
        [JsonPropertyName("showModuleName")]
        public bool? showModuleName { get; set; }

        [BsonElement("toUpperCase")]
        [JsonPropertyName("toUpperCase")]
        public bool? toUpperCase { get; set; }

        [BsonElement("showPickupDropOffTime")]
        [JsonPropertyName("showPickupDropOffTime")]
        public bool? showPickupDropOffTime { get; set; }

        [BsonElement("pageBreakDaily")]
        [JsonPropertyName("pageBreakDaily")]
        public bool? pageBreakDaily { get; set; }
    }
}
