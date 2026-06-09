using System;
using System.Collections.Concurrent;


namespace RezervisiMe.RezervisiMe.API.Auth
{
    public class TokenService
    {
        private static readonly ConcurrentDictionary<string, TokenService> _store 
             = new ConcurrentDictionary<string, TokenService>();

        private static readonly TimeSpan _ttl = TimeSpan.FromHours(8);

        public class TokenEntry
        {
            public Guid UserId { get; set; }
            public string Username { get; set; }
            public string Role { get; set; }       
            public DateTime ExpiresAt { get; set; }
        }
    }
}