using RezervisiMe.RezervisiMe.API.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RezervisiMe.RezervisiMe.API.Models
{
    public class User : EntityBase
    {
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }

        [JsonConverter(typeof(CustomDateConverter))]
        public DateTime DateOfBirth { get; set; }

        public Gender Gender { get; set; } 
        public UserRole Role { get; set; }

    }
}