using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RezervisiMe.RezervisiMe.API.Models;


namespace RezervisiMe.RezervisiMe.API.Infrastructure
{

    public static class SeedBootstrapper
    {
        private static readonly JsonSerializerSettings JsonOpts = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static void Run()
        {
            string marker = HostingEnvironment.MapPath("~/App_Data/.seeded");
            if (File.Exists(marker)) return;

            if (!File.Exists(HostingEnvironment.MapPath("~/App_Data/seed_users.json")) &&
                !File.Exists(HostingEnvironment.MapPath("~/App_Data/seed_accommodations.json")) &&
                !File.Exists(HostingEnvironment.MapPath("~/App_Data/seed_reservations.json")) &&
                !File.Exists(HostingEnvironment.MapPath("~/App_Data/seed_reviews.json")))
                return;

            var usernameToId = SeedUsers();
            var accNameToId = SeedAccommodations(usernameToId);
            SeedReservations(usernameToId, accNameToId);
            SeedReviews(usernameToId, accNameToId);

            File.WriteAllText(marker, DateTime.UtcNow.ToString("o"));
        }

       

        private static Dictionary<string, Guid> SeedUsers()
        {
            var map = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            var path = HostingEnvironment.MapPath("~/App_Data/seed_users.json");
            if (!File.Exists(path)) return map;

            var seeds = JsonConvert.DeserializeObject<List<SeedUser>>(File.ReadAllText(path), JsonOpts);
            var repo = Composition.Users;

            foreach (var s in seeds)
            {
                var existing = repo.GetByUsername(s.Username);
                if (existing != null) { map[s.Username] = existing.Id; continue; }

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    UserName = s.Username,
                    PasswordHash = PasswordHasher.Hash(s.Password),
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Email = s.Email,
                    DateOfBirth = DateTime.ParseExact(s.DateOfBirth, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    Gender = (Gender)Enum.Parse(typeof(Gender), s.Gender),
                    Role = (UserRole)Enum.Parse(typeof(UserRole), s.Role)
                };
                repo.Add(user);
                map[s.Username] = user.Id;
            }
            return map;
        }

   

        private static Dictionary<string, Guid> SeedAccommodations(Dictionary<string, Guid> usernameToId)
        {
            var map = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            var path = HostingEnvironment.MapPath("~/App_Data/seed_accommodations.json");
            if (!File.Exists(path)) return map;

            var seeds = JsonConvert.DeserializeObject<List<SeedAccommodation>>(File.ReadAllText(path), JsonOpts);
            var repo = Composition.Accommodations;

            foreach (var s in seeds)
            {
                Guid hostId;
                if (!usernameToId.TryGetValue(s.HostUsername, out hostId)) continue;

                var existing = repo.GetAll().FirstOrDefault(a =>
                    string.Equals(a.Name, s.Name, StringComparison.OrdinalIgnoreCase));
                if (existing != null) { map[s.Name] = existing.Id; continue; }

                var acc = new Accommodation
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    HostId = hostId,
                    Name = s.Name,
                    Type = (AccommodationType)Enum.Parse(typeof(AccommodationType), s.Type),
                    Description = s.Description,
                    Address = s.Address,
                    City = s.City,
                    PricePerNight = s.PricePerNight,
                    MaxGuests = s.MaxGuests,
                    ImagePath = s.ImagePath,
                    IsAvailable = s.IsAvailable,
                    PostedAt = DateTime.UtcNow.AddDays(-s.PostedAtDaysAgo)
                };
                repo.Add(acc);
                map[s.Name] = acc.Id;
            }
            return map;
        }


        private static void SeedReservations(Dictionary<string, Guid> usernameToId, Dictionary<string, Guid> accNameToId)
        {
            var path = HostingEnvironment.MapPath("~/App_Data/seed_reservations.json");
            if (!File.Exists(path)) return;

            var seeds = JsonConvert.DeserializeObject<List<SeedReservation>>(File.ReadAllText(path), JsonOpts);
            var repo = Composition.Reservations;
            var accRepo = Composition.Accommodations;

            foreach (var s in seeds)
            {
                Guid guestId, accId;
                if (!usernameToId.TryGetValue(s.GuestUsername, out guestId)) continue;
                if (!accNameToId.TryGetValue(s.AccommodationName, out accId)) continue;

                var acc = accRepo.GetById(accId);
                if (acc == null) continue;

                var today = DateTime.UtcNow.Date;
                var checkIn = today.AddDays(s.CheckInDaysOffset);
                var checkOut = today.AddDays(s.CheckOutDaysOffset);
                var nights = Math.Max(1, (int)(checkOut - checkIn).TotalDays);

                var res = new Reservation
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow.AddDays(-Math.Max(1, s.CheckInDaysOffset < 0 ? -s.CheckInDaysOffset + 5 : 5)),
                    GuestId = guestId,
                    AccommodationId = accId,
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    NumberOfGuests = s.NumberOfGuests,
                    TotalPrice = acc.PricePerNight * nights,
                    Status = (ReservationStatus)Enum.Parse(typeof(ReservationStatus), s.Status)
                };
                repo.Add(res);
            }
        }

        private static void SeedReviews(Dictionary<string, Guid> usernameToId, Dictionary<string, Guid> accNameToId)
        {
            var path = HostingEnvironment.MapPath("~/App_Data/seed_reviews.json");
            if (!File.Exists(path)) return;

            var seeds = JsonConvert.DeserializeObject<List<SeedReview>>(File.ReadAllText(path), JsonOpts);
            var repo = Composition.Reviews;

            foreach (var s in seeds)
            {
                Guid reviewerId, accId;
                if (!usernameToId.TryGetValue(s.ReviewerUsername, out reviewerId)) continue;
                if (!accNameToId.TryGetValue(s.AccommodationName, out accId)) continue;

                var review = new Review
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow.AddDays(-s.PostedAtDaysAgo),
                    ReviewerId = reviewerId,
                    AccommodationId = accId,
                    Title = s.Title,
                    Content = s.Content,
                    Rating = s.Rating,
                    Status = (ReviewStatus)Enum.Parse(typeof(ReviewStatus), s.Status)
                };
                repo.Add(review);
            }
        }


        private class SeedUser
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string DateOfBirth { get; set; }
            public string Gender { get; set; }
            public string Role { get; set; }
        }

        private class SeedAccommodation
        {
            public string HostUsername { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public decimal PricePerNight { get; set; }
            public int MaxGuests { get; set; }
            public string ImagePath { get; set; }
            public bool IsAvailable { get; set; }
            public int PostedAtDaysAgo { get; set; }
        }

        private class SeedReservation
        {
            public string GuestUsername { get; set; }
            public string AccommodationName { get; set; }
            public int CheckInDaysOffset { get; set; }
            public int CheckOutDaysOffset { get; set; }
            public int NumberOfGuests { get; set; }
            public string Status { get; set; }
        }

        private class SeedReview
        {
            public string ReviewerUsername { get; set; }
            public string AccommodationName { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public int Rating { get; set; }
            public string Status { get; set; }
            public int PostedAtDaysAgo { get; set; }
        }
    }
}