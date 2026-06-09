using RezervisiMe.RezervisiMe.API.Infrastructure;
using RezervisiMe.RezervisiMe.API.Models;
using RezervisiMe.RezervisiMe.API.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RezervisiMe.RezervisiMe.API.Repositories
{
    public class AccommodationRepository : IAccommodationRepository
    {
        private readonly JsonFileStore<Accommodation> _store;

        public AccommodationRepository(JsonFileStore<Accommodation> store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public IEnumerable<Accommodation> GetAll() => _store.GetAll();
        public IEnumerable<Accommodation> GetAllIncludingDeleted() => _store.LoadAll();
        public Accommodation GetById(Guid id) => _store.GetById(id);
        public Accommodation Add(Accommodation entity) => _store.Add(entity);
        public bool Update(Accommodation entity) => _store.Update(entity);
        public bool SoftDelete(Guid id) => _store.SoftDelete(id);

        public IEnumerable<Accommodation> GetByHost(Guid hostId)
        {
            return _store.GetAll().Where(a => a.HostId == hostId);
        }

    }
}