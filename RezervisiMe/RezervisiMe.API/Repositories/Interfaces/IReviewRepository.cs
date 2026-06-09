using RezervisiMe.RezervisiMe.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RezervisiMe.RezervisiMe.API.Repositories.Interfaces
{
    public interface IReviewRepository : IRepository<Review>
    {
        IEnumerable<Review> GetByAccommodation(Guid accommodationId);
        IEnumerable<Review> GetByReviewer(Guid reviewerId);
    }
}