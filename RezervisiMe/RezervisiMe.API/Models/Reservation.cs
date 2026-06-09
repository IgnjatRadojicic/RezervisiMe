using RezervisiMe.RezervisiMe.API.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace RezervisiMe.RezervisiMe.API.Models
{
    public class Reservation : EntityBase
    {
        public Guid GuestId { get; set; }
        public Guid AccommodationId { get; set; }

        [JsonConverter(typeof(CustomDateConverter))]
        public DateTime CheckIn { get; set; }

        [JsonConverter(typeof(CustomDateConverter))]
        public DateTime CheckOut { get; set; }
        
        public int NumberOfGuests { get; set; }
        public decimal TotalPrice { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.KREIRANA;
    }
}