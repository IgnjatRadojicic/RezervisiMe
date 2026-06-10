using System;
using Newtonsoft.Json;
using RezervisiMe.RezervisiMe.API.Infrastructure;

namespace RezervisiMe.RezervisiMe.API.Models.Requests
{
    public class CreateReservationRequest
    {
        public Guid AccommodationId { get; set; }

        [JsonConverter(typeof(CustomDateConverter))]
        public DateTime CheckIn { get; set; }

        [JsonConverter(typeof(CustomDateConverter))]
        public DateTime CheckOut { get; set; }

        public int NumberOfGuests { get; set; }
    }
}