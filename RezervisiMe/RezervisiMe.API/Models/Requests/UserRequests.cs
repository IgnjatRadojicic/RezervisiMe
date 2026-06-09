using System;
using Newtonsoft.Json;
using RezervisiMe.RezervisiMe.API.Infrastructure;

namespace RezervisiMe.RezervisiMe.API.Models.Requests
{
    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        [JsonConverter(typeof(CustomDateConverter))]
        public DateTime DateOfBirth { get; set; }

        public Gender Gender { get; set; }
        public UserRole Role { get; set; }   
    }

    public class UpdateProfileRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        [JsonConverter(typeof(CustomDateConverter))]
        public DateTime DateOfBirth { get; set; }

        public Gender Gender { get; set; }
        public string NewPassword { get; set; }  
    }

    public class UserSearchCriteria
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirthFrom { get; set; }
        public DateTime? DateOfBirthTo { get; set; }
        public UserRole? Role { get; set; }
        public string SortBy { get; set; }    // "name" | "dateOfBirth" | "role"
        public string SortDir { get; set; }   // "asc" | "desc"
    }
}