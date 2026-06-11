using System;
using System.Collections.Generic;
using System.Linq;
using RezervisiMe.RezervisiMe.API.Infrastructure;
using RezervisiMe.RezervisiMe.API.Models;
using RezervisiMe.RezervisiMe.API.Models.Dto;
using RezervisiMe.RezervisiMe.API.Models.Requests;
using RezervisiMe.RezervisiMe.API.Repositories;
using RezervisiMe.RezervisiMe.API.Repositories.Interfaces;

namespace RezervisiMe.RezervisiMe.API.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservations;
        private readonly IAccommodationRepository _accommodations;
        private readonly IUserRepository _users;

        public ReservationService(
            IReservationRepository reservations,
            IAccommodationRepository accommodations,
            IUserRepository users)
        {
            _reservations = reservations ?? throw new ArgumentNullException(nameof(reservations));
            _accommodations = accommodations ?? throw new ArgumentNullException(nameof(accommodations));
            _users = users ?? throw new ArgumentNullException(nameof(users));
        }

        public Result<ReservationDto> Create(Guid guestId, CreateReservationRequest req)
        {
            if (req == null) return Error.Validation("Telo zahteva je obavezno");
            if (req.CheckIn.Date < DateTime.UtcNow.Date)
                return Error.Validation("Datum prijave ne može biti u prošlosti");
            if (req.CheckOut.Date <= req.CheckIn.Date)
                return Error.Validation("Datum odjave mora biti posle datuma prijave");
            if (req.NumberOfGuests <= 0)
                return Error.Validation("Broj gostiju mora biti > 0");

            var acc = _accommodations.GetById(req.AccommodationId);
            if (acc == null) return Error.NotFound("Objekat ne postoji");
            if (!acc.IsAvailable) return Error.Conflict("Objekat nije dostupan za rezervaciju");
            if (req.NumberOfGuests > acc.MaxGuests)
                return Error.Validation(
                    $"Broj gostiju ({req.NumberOfGuests}) prevazilazi kapacitet objekta ({acc.MaxGuests})");

            var overlap = _reservations.GetOverlappingApproved(acc.Id, req.CheckIn, req.CheckOut).Any();
            if (overlap)
                return Error.Conflict("Termin se preklapa sa već odobrenom rezervacijom");

            var nights = (int)(req.CheckOut.Date - req.CheckIn.Date).TotalDays;
            var reservation = new Reservation
            {
                GuestId = guestId,
                AccommodationId = acc.Id,
                CheckIn = req.CheckIn,
                CheckOut = req.CheckOut,
                NumberOfGuests = req.NumberOfGuests,
                TotalPrice = nights * acc.PricePerNight,
                Status = ReservationStatus.KREIRANA
            };
            _reservations.Add(reservation);

            var guest = _users.GetById(guestId);
            return MapToDto(reservation, guest, acc);

        }


        public Result<ReservationDto> Approve(Guid reservationId)
        {
            var r = _reservations.GetById(reservationId);
            if (r == null) return Error.NotFound("Rezervacija ne postoji");
            if (r.Status != ReservationStatus.KREIRANA)
                return Error.Conflict("Samo KREIRANA rezervacija može biti odobrena");

            var overlap = _reservations.GetOverlappingApproved(r.AccommodationId, r.CheckIn, r.CheckOut)
                .Any();
            if (overlap)
                return Error.Conflict("Konflikt sa već odobrenom rezervacijom odbij ovu");

            r.Status = ReservationStatus.ODOBRENA;
            _reservations.Update(r);

            var guest = _users.GetById(r.GuestId);
            var acc = _accommodations.GetById(r.AccommodationId);
            return MapToDto(r, guest, acc);
        }

        public Result<ReservationDto> Reject(Guid reservationId)
        {
            var r = _reservations.GetById(reservationId);
            if (r == null) return Error.NotFound("Rezervacija ne postoji");
            if (r.Status != ReservationStatus.KREIRANA)
                return Error.Conflict("Samo KREIRANA rezervacija može biti odbijena ovde");

            r.Status = ReservationStatus.OTKAZANA;
            _reservations.Update(r);

            var guest = _users.GetById(r.GuestId);
            var acc = _accommodations.GetById(r.AccommodationId);
            return MapToDto(r, guest, acc);
        }

        public Result<ReservationDto> Cancel(Guid reservationId, Guid currentUserId, UserRole currentUserRole)
        {
            var r = _reservations.GetById(reservationId);
            if (r == null) return Error.NotFound("Rezervacija ne postoji");

            if (currentUserRole == UserRole.Gost && r.GuestId != currentUserId)
                return Error.Forbidden("Možeš da otkažeš samo svoju rezervaciju");

            if (r.Status != ReservationStatus.KREIRANA && r.Status != ReservationStatus.ODOBRENA)
                return Error.Conflict("Otkazivanje moguće samo za KREIRANA ili ODOBRENA");

            if ((r.CheckIn - DateTime.UtcNow) < TimeSpan.FromHours(24))
                return Error.Conflict("Otkazivanje moguće najkasnije 24h pre datuma prijave");

            r.Status = ReservationStatus.OTKAZANA;
            _reservations.Update(r);

            var guest = _users.GetById(r.GuestId);
            var acc = _accommodations.GetById(r.AccommodationId);
            return MapToDto(r, guest, acc);
        }

        public Result<ReservationDto> GetById(Guid id)
        {
            var r = _reservations.GetById(id);
            if (r == null) return Error.NotFound("Rezervacija ne postoji");

            var guest = _users.GetById(r.GuestId);
            var acc = _accommodations.GetById(r.AccommodationId);
            return MapToDto(r, guest, acc);
        }

        public void RefreshCompletedStatuses()
        {
            var today = DateTime.UtcNow.Date;
            var past = _reservations.GetAll()
                .Where(r => r.Status == ReservationStatus.ODOBRENA && r.CheckOut.Date < today)
                .ToList();
            foreach (var r in past)
            {
                r.Status = ReservationStatus.ZAVRSENA;
                _reservations.Update(r);
            }
        }


        public IEnumerable<ReservationDto> GetByGuest(Guid guestId, ReservationStatus? statusFilter)
        {
            RefreshCompletedStatuses();

            var q = _reservations.GetByGuest(guestId).AsEnumerable();
            if (statusFilter.HasValue)
                q = q.Where(r => r.Status == statusFilter.Value);

            var list = q.ToList();
            if (list.Count == 0) return new List<ReservationDto>();

            var usersById = _users.GetAllIncludingDeleted().ToDictionary(u => u.Id);
            var accsById = _accommodations.GetAllIncludingDeleted().ToDictionary(a => a.Id);

            return list.Select(r =>
            {
                usersById.TryGetValue(r.GuestId, out var guest);
                accsById.TryGetValue(r.AccommodationId, out var acc);
                return MapToDto(r, guest, acc);
            }).ToList();
        }

        public IEnumerable<ReservationDto> GetAll(ReservationStatus? statusFilter, Guid? accommodationIdFilter)
        {
            RefreshCompletedStatuses();

            var q = _reservations.GetAll().AsEnumerable();
            if (statusFilter.HasValue)
                q = q.Where(r => r.Status == statusFilter.Value);
            if (accommodationIdFilter.HasValue)
                q = q.Where(r => r.AccommodationId == accommodationIdFilter.Value);

            var list = q.ToList();
            if (list.Count == 0) return new List<ReservationDto>();

            var usersById = _users.GetAllIncludingDeleted().ToDictionary(u => u.Id);
            var accsById = _accommodations.GetAllIncludingDeleted().ToDictionary(a => a.Id);

            return list.Select(r =>
            {
                usersById.TryGetValue(r.GuestId, out var guest);
                accsById.TryGetValue(r.AccommodationId, out var acc);
                return MapToDto(r, guest, acc);
            }).ToList();
        }

        private static ReservationDto MapToDto(Reservation r, User guest, Accommodation acc)
        {
            return new ReservationDto
            {
                Id = r.Id,
                GuestId = r.GuestId,
                GuestUsername = guest?.UserName,
                AccommodationId = r.AccommodationId,
                AccommodationName = acc?.Name,
                CheckIn = r.CheckIn,
                CheckOut = r.CheckOut,
                NumberOfGuests = r.NumberOfGuests,
                TotalPrice = r.TotalPrice,
                Status = r.Status
            };
        }
    }
}