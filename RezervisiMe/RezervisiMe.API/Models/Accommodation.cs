using RezervisiMe.RezervisiMe.API.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RezervisiMe.RezervisiMe.API.Models
{
    public class Accommodation : EntityBase
    {
        public Guid HostId { get; set; }
        public string Name { get; set; }
        public AccommodationType Type { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public decimal PricePerNight { get; set; }
        public int MaxGuests { get; set; }

        public string ImagePath { get; set; }

        [JsonConverter(typeof(CustomDateConverter))]
        public DateTime PostedAt { get; set; } = DateTime.UtcNow;

        public bool IsAvailable { get; set; } = true;
    }
}