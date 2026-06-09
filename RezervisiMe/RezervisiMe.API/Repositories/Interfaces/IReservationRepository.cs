using System;
using System.Collections.Generic;
using RezervisiMe.RezervisiMe.API.Models;

namespace RezervisiMe.RezervisiMe.API.Repositories.Interfaces
{
    public interface IReservationRepository : IRepository<Reservation>
    {
        IEnumerable<Reservation> GetByGuest(Guid guestId);
        IEnumerable<Reservation> GetByAccommodation(Guid accommodationId);
        IEnumerable<Reservation> GetOverlappingApproved(Guid accommodationId, DateTime from, DateTime to);
    }


}