using System;
using RezervisiMe.RezervisiMe.API.Models;

namespace RezervisiMe.RezervisiMe.API.Models.Requests
{
    public class CreateAccommodationRequest
    {
        public string Name { get; set; }
        public AccommodationType Type { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public decimal PricePerNight { get; set; }
        public int MaxGuests { get; set; }
        public string ImagePath { get; set; }  
    }

    public class UpdateAccommodationRequest
    {
        public string Name { get; set; }
        public AccommodationType Type { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public decimal PricePerNight { get; set; }
        public int MaxGuests { get; set; }
        public string ImagePath { get; set; }  
        public bool IsAvailable { get; set; }
    }

    public class AccommodationSearchCriteria
    {
        public string Name { get; set; }
        public string City { get; set; }
        public AccommodationType? Type { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public bool? IsAvailable { get; set; }   
        public Guid? HostId { get; set; }
        public string SortBy { get; set; }     
        public string SortDir { get; set; }
    }
}