using RezervisiMe.RezervisiMe.API.Infrastructure;
using RezervisiMe.RezervisiMe.API.Models;
using RezervisiMe.RezervisiMe.API.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RezervisiMe.RezervisiMe.API.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly JsonFileStore<Review> _store;

        public ReviewRepository(JsonFileStore<Review> store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public IEnumerable<Review> GetAll() => _store.GetAll();
        public IEnumerable<Review> GetAllIncludingDeleted() => _store.LoadAll();
        public Review GetById(Guid id) => _store.GetById(id);
        public Review Add(Review entity) => _store.Add(entity);
        public bool Update(Review entity) => _store.Update(entity);
        public bool SoftDelete(Guid id) => _store.SoftDelete(id);

        public IEnumerable<Review> GetByAccommodation(Guid accommodationId)
        {
            return _store.GetAll().Where(r => r.AccommodationId == accommodationId);
        }

        public IEnumerable<Review> GetByReviewer(Guid reviewerId)
        {
            return _store.GetAll().Where(r => r.ReviewerId == reviewerId);
        }
    }
}