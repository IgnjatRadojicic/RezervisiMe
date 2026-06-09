using RezervisiMe.RezervisiMe.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RezervisiMe.RezervisiMe.API.Repositories.Interfaces
{
    public class IReservationRepository
    {
        public interface IReservationRepository : IRepository<Reservation>
        {
            IEnumerable<Reservation> GetByGuest(Guid guestId);
            IEnumerable<Reservation> GetByAccommodation(Guid accommodationId);
            IEnumerable<Reservation> GetOverLappingApproved(Guid accommodationId, DateTime from, DateTime to);
        }
    }
}