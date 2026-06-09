using Newtonsoft.Json;
using RezervisiMe.RezervisiMe.API.Infrastructure;
using System;


namespace RezervisiMe.RezervisiMe.API.Models.Dto
{
    public class UserDto 
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }

        [JsonConverter(typeof(CustomDateConverter))]
        public DateTime DateOfBirth { get; set; }

        public Gender Gender { get; set; }
        public UserRole Role { get; set; }

    }
}