using System;
using System.Collections.Generic;
using System.Linq;
using RezervisiMe.RezervisiMe.API.Infrastructure;
using RezervisiMe.RezervisiMe.API.Models;
using RezervisiMe.RezervisiMe.API.Repositories.Interfaces;

namespace RezervisiMe.RezervisiMe.API.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly JsonFileStore<Reservation> _store;

        public ReservationRepository(JsonFileStore<Reservation> store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public IEnumerable<Reservation> GetAll() => _store.GetAll();
        public IEnumerable<Reservation> GetAllIncludingDeleted() => _store.LoadAll();
        public Reservation GetById(Guid id) => _store.GetById(id);
        public Reservation Add(Reservation entity) => _store.Add(entity);
        public bool Update(Reservation entity) => _store.Update(entity);
        public bool SoftDelete(Guid id) => _store.SoftDelete(id);


        public IEnumerable<Reservation> GetByGuest(Guid guestId)
        {
            return _store.GetAll().Where(g => g.GuestId == guestId);
        }

        public IEnumerable<Reservation> GetByAccommodation(Guid accommodationId)
        {
            return _store.GetAll().Where(g => g.AccommodationId == accommodationId);
        }

        public IEnumerable<Reservation> GetOverlappingApproved(Guid accomodationid, DateTime from, DateTime to)
        {
            return _store.GetAll().Where(r =>
            r.AccommodationId == accomodationid &&
            r.Status == ReservationStatus.ODOBRENA &&
            r.CheckIn < to &&
            r.CheckOut > from);
        }
    }
}