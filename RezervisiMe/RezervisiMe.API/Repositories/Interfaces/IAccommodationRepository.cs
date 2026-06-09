using RezervisiMe.RezervisiMe.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RezervisiMe.RezervisiMe.API.Repositories.Interfaces
{
    public interface IAccommodationRepository : IRepository<Accommodation>
    {
        IEnumerable<Accommodation> GetByHost(Guid hostId);
    }
}