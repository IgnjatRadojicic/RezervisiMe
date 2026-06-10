using RezervisiMe.RezervisiMe.API.Infrastructure;
using RezervisiMe.RezervisiMe.API.Models;
using RezervisiMe.RezervisiMe.API.Models.Dto;
using RezervisiMe.RezervisiMe.API.Models.Requests;
using RezervisiMe.RezervisiMe.API.Repositories;
using RezervisiMe.RezervisiMe.API.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

namespace RezervisiMe.RezervisiMe.API.Services
{
    public class AccommodationService : IAccommodationService
    {
        private readonly IAccommodationRepository _accommodations;
        private readonly IReservationRepository _reservations;
        private readonly IUserRepository _users;
        private readonly IReviewRepository _reviews;

        public AccommodationService(
            IAccommodationRepository accommodations,
            IReservationRepository reservations,
            IUserRepository users,
            IReviewRepository reviews)
        {
            _accommodations = accommodations ?? throw new ArgumentNullException(nameof(accommodations));
            _reservations = reservations ?? throw new ArgumentNullException(nameof(reservations));
            _users = users ?? throw new ArgumentNullException(nameof(users));
            _reviews = reviews ?? throw new ArgumentNullException(nameof(reviews));
        }

        public Result<AccommodationDto> Create(Guid hostId, CreateAccommodationRequest req)
        {
            if (req == null) return Error.Validation("Telo zahteva je obavezno");
            if (string.IsNullOrWhiteSpace(req.Name)) return Error.Validation("Naziv je obavezan");
            if (string.IsNullOrWhiteSpace(req.Address)) return Error.Validation("Adresa je obavezna");
            if (string.IsNullOrWhiteSpace(req.City)) return Error.Validation("Grad je obavezan");
            if (req.PricePerNight <= 0) return Error.Validation("Cena po noći mora biti > 0");
            if (req.MaxGuests <= 0) return Error.Validation("Maks. broj gostiju mora biti > 0");
            if (string.IsNullOrWhiteSpace(req.ImagePath))
                return Error.Validation("Slika je obavezna pri kreiranju objekta");

            var host = _users.GetById(hostId);
            if (host == null) return Error.NotFound("Domaćin ne postoji");
            if (host.Role != UserRole.Domacin)
                return Error.Forbidden("Samo domaćini mogu kreirati objekte");

            var acc = new Accommodation
            {
                HostId = hostId,
                Name = req.Name.Trim(),
                Type = req.Type,
                Description = req.Description ?? "",
                Address = req.Address.Trim(),
                City = req.City.Trim(),
                PricePerNight = req.PricePerNight,
                MaxGuests = req.MaxGuests,
                ImagePath = req.ImagePath,
                PostedAt = DateTime.UtcNow,
                IsAvailable = true
            };


            _accommodations.Add(acc);
            return MapToDto(acc, host, null);
        }

        public Result<AccommodationDto> Update(Guid id, UpdateAccommodationRequest req,
            Guid currentUserId, UserRole currentUserRole)
        {
            
            if (req == null) return Error.Validation("Telo zahteva je obavezno");
            var acc = _accommodations.GetById(id);
            if (acc == null) return Error.NotFound("Objekat ne postoji");

            if (currentUserRole == UserRole.Domacin)
            {
                if (acc.HostId != currentUserId)
                    return Error.Forbidden("Možeš da menjaš samo svoje objekte");
                if (!acc.IsAvailable)
                    return Error.Forbidden("Nedostupan objekat se ne može menjati");
            }

            if (string.IsNullOrWhiteSpace(req.Name)) return Error.Validation("Naziv je obavezan");
            if (string.IsNullOrWhiteSpace(req.Address)) return Error.Validation("Adresa je obavezna");
            if (string.IsNullOrWhiteSpace(req.City)) return Error.Validation("Grad je obavezan");
            if (req.PricePerNight <= 0) return Error.Validation("Cena po noći mora biti > 0");
            if (req.MaxGuests <= 0) return Error.Validation("Maks. broj gostiju mora biti > 0");

            acc.Name = req.Name.Trim();
            acc.Type = req.Type;
            acc.Description = req.Description ?? "";
            acc.Address = req.Address.Trim();
            acc.City = req.City.Trim();
            acc.PricePerNight = req.PricePerNight;
            acc.MaxGuests = req.MaxGuests;
            acc.IsAvailable = req.IsAvailable;

            
            if (!string.IsNullOrWhiteSpace(req.ImagePath))
                acc.ImagePath = req.ImagePath;

            _accommodations.Update(acc);

            var host = _users.GetById(acc.HostId);
            var approved = _reviews.GetByAccommodation(acc.Id)
                .Where(r => r.Status == ReviewStatus.ODOBRENA)
                .ToList();
            return MapToDto(acc, host, approved);
        }

        public Result Delete(Guid id, Guid currentUserId, UserRole currentUserRole)
        {
            var acc = _accommodations.GetById(id);
            if (acc == null) return Error.NotFound("Objekat ne postoji");

            if (currentUserRole == UserRole.Domacin)
            {
                if (acc.HostId != currentUserId)
                    return Error.Forbidden("Možeš da brišeš samo svoje objekte");
                if (!acc.IsAvailable)
                    return Error.Forbidden("Nedostupan objekat se ne može obrisati");
            }

            var hasActive = _reservations.GetByAccommodation(id)
                .Any(r => r.Status == ReservationStatus.KREIRANA || r.Status == ReservationStatus.ODOBRENA);
            if (hasActive)
                return Error.Conflict("Objekat ima aktivne rezervacije, ne može se obrisati");

            _accommodations.SoftDelete(id);
            return Result.Success();
        }

        public Result<AccommodationDto> GetById(Guid id)
        {
            var acc = _accommodations.GetById(id);
            if (acc == null) return Error.NotFound("Objekat ne postoji");

            var host = _users.GetById(acc.HostId);
            var approved = _reviews.GetByAccommodation(id)
                .Where(r => r.Status == ReviewStatus.ODOBRENA)
                .ToList();

            return MapToDto(acc, host, approved);
        }

        public IEnumerable<AccommodationDto> Search(AccommodationSearchCriteria c)
        {
            c = c ?? new AccommodationSearchCriteria();

            var q = _accommodations.GetAll().AsEnumerable();

            if (c.HostId.HasValue)
                q = q.Where(a => a.HostId == c.HostId.Value);

            if (!string.IsNullOrWhiteSpace(c.Name)) 
                q = q.Where(a => !string.IsNullOrEmpty(a.Name)
                && a.Name.IndexOf(c.Name, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrWhiteSpace(c.City))
                q = q.Where(a => !string.IsNullOrEmpty(a.City)
                    && a.City.IndexOf(c.City, StringComparison.OrdinalIgnoreCase) >= 0);

            if (c.Type.HasValue)
                q = q.Where(a => a.Type == c.Type.Value);

            if (c.PriceMin.HasValue)
                q = q.Where(a => a.PricePerNight >= c.PriceMin.Value);

            if (c.PriceMax.HasValue) 
                q = q.Where(a => a.PricePerNight <= c.PriceMax.Value);

            if (c.IsAvailable.HasValue) 
                q = q.Where(a =>  a.IsAvailable == c.IsAvailable.Value);

            var desc = string.Equals(c.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

            switch((c.SortBy ?? "").ToLowerInvariant())
            {
                case "price":
                    q = desc ? q.OrderByDescending(a => a.PricePerNight)
                             : q.OrderBy(a => a.PricePerNight);
                    break;
                case "postedat":
                    q = desc ? q.OrderByDescending(a => a.PostedAt)
                             : q.OrderBy(a => a.PostedAt);
                    break;
                case "name":
                default:
                    q = desc ? q.OrderByDescending(a => a.Name)
                             : q.OrderBy(a => a.Name);
                    break;
            }
            var filtered = q.ToList();
            if (filtered.Count == 0) return new List<AccommodationDto>();

            var usersById = _users.GetAllIncludingDeleted()
                .ToDictionary(u => u.Id);

            var approvedByAcc = _reviews.GetAll()
                .Where(r => r.Status == ReviewStatus.ODOBRENA)
                .GroupBy(r => r.AccommodationId)
                .ToDictionary(g => g.Key, g => g.ToList());

            return filtered.Select(a =>
            {
                usersById.TryGetValue(a.HostId, out var host);
                approvedByAcc.TryGetValue(a.Id, out var approvedReviews);
                return MapToDto(a, host, approvedReviews);
            }).ToList();
        }

        private static AccommodationDto MapToDto(Accommodation a, User host, List<Review> approved)
        {
            approved = approved ?? new List<Review>();
            return new AccommodationDto
            {
                Id = a.Id,
                HostId = a.HostId,
                HostUsername = host?.UserName,
                Name = a.Name,
                Type = a.Type,
                Description = a.Description,
                Address = a.Address,
                City = a.City,
                PricePerNight = a.PricePerNight,
                MaxGuests = a.MaxGuests,
                ImagePath = a.ImagePath,
                PostedAt = a.PostedAt,
                IsAvailable = a.IsAvailable,
                AverageRating = approved.Count > 0 ? (double?)approved.Average(r => r.Rating) : null,
                ApprovedReviewsCount = approved.Count
            };
        }
    }
}