using RezervisiMe.RezervisiMe.API.Infrastructure;
using RezervisiMe.RezervisiMe.API.Models;
using RezervisiMe.RezervisiMe.API.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RezervisiMe.RezervisiMe.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly JsonFileStore<User> _store;

        public UserRepository(JsonFileStore<User> store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public IEnumerable<User> GetAll() => _store.GetAll();
        public IEnumerable<User> GetAllIncludingDeleted() => _store.LoadAll();
        public User GetById(Guid id) => _store.GetById(id);
        public User Add(User entity) => _store.Add(entity);
        public bool Update(User entity) => _store.Update(entity);
        public bool SoftDelete(Guid id) => _store.SoftDelete(id);

        public User GetByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            return _store.GetAll().FirstOrDefault(u =>
                string.Equals(u.UserName, username, StringComparison.OrdinalIgnoreCase));
        }

        public bool UsernameExists(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            return _store.LoadAll().Any(u =>
                string.Equals(u.UserName, username, StringComparison.OrdinalIgnoreCase));
        }
    }
}