using System;
using System.Collections.Concurrent;

namespace RezervisiMe.RezervisiMe.API.Auth
{

    public static class TokenStore
    {
        private static readonly ConcurrentDictionary<string, TokenEntry> _store
            = new ConcurrentDictionary<string, TokenEntry>();

        private static readonly TimeSpan _ttl = TimeSpan.FromHours(8);

        public class TokenEntry
        {
            public Guid UserId { get; set; }
            public string Username { get; set; }
            public string Role { get; set; }     
            public DateTime ExpiresAt { get; set; }
        }

        public static string Issue(Guid userId, string username, string role)
        {
            var token = Guid.NewGuid().ToString("N");
            _store[token] = new TokenEntry
            {
                UserId = userId,
                Username = username,
                Role = role,
                ExpiresAt = DateTime.UtcNow.Add(_ttl)
            };
            return token;
        }

        public static TokenEntry Validate(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            if (!_store.TryGetValue(token, out var entry)) return null;
            if (entry.ExpiresAt < DateTime.UtcNow)
            {
                _store.TryRemove(token, out _);
                return null;
            }
            return entry;
        }

        public static void Revoke(string token)
        {
            if (string.IsNullOrEmpty(token)) return;
            _store.TryRemove(token, out _);
        }
    }
}