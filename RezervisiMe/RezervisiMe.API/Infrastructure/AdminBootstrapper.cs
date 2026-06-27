using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Newtonsoft.Json;
using RezervisiMe.RezervisiMe.API.Models;

namespace RezervisiMe.RezervisiMe.API.Infrastructure
{

    public static class AdminBootstrapper
    {
        private class AdminSeed
        {
            public string Username { get; set; }
            public string Password { get; set; }     
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string DateOfBirth { get; set; }  
            public string Gender { get; set; }       
        }

        public static void SeedAdminsFromFile()
        {
            var path = HostingEnvironment.MapPath("~/App_Data/admins.json");
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) return;

            List<AdminSeed> seeds;
            try { seeds = JsonConvert.DeserializeObject<List<AdminSeed>>(json); }
            catch (JsonException) { return; }

            if (seeds == null || seeds.Count == 0) return;

            var repo = Composition.Users;
            var existing = repo.GetAllIncludingDeleted().ToList();

            foreach (var s in seeds)
            {
                if (existing.Any(u => string.Equals(u.UserName, s.Username,
                        StringComparison.OrdinalIgnoreCase))) continue;

                var user = new User
                {
                    UserName = s.Username,
                    PasswordHash = PasswordHasher.Hash(s.Password),
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Email = s.Email,
                    DateOfBirth = DateTime.ParseExact(s.DateOfBirth, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture),
                    Gender = (Gender)Enum.Parse(typeof(Gender), s.Gender, true),
                    Role = UserRole.Administrator
                };
                repo.Add(user);
            }
        }
    }
}