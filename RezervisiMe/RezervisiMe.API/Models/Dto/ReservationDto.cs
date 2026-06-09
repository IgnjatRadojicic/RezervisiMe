
using Newtonsoft.Json;
using RezervisiMe.RezervisiMe.API.Infrastructure;
using System;

namespace RezervisiMe.RezervisiMe.API.Models.Dto
{
    public class ReservationDto
    {
        public Guid Id { get; set; }
        public Guid GuestId { get; set; }
        public string GuestUsername { get; set; }
        public Guid AccommodationId { get; set; }
        public string AccommodationName { get; set; }

        [JsonConverter(typeof(CustomDateConverter))]
        public DateTime CheckIn { get; set; }

        [JsonConverter(typeof(CustomDateConverter))]
        public DateTime CheckOut { get; set; }

        public int NumberOfGuests { get; set; }
        public decimal TotalPrice { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.KREIRANA;
    }
}