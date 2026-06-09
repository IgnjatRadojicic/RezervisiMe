
using System;
using System.Security.Cryptography;

namespace RezervisiMe.RezervisiMe.API.Infrastructure
{
    public static class PasswordHasher
    {
        private const int Iterations = 100_000;
        private const int SaltBytes = 16;
        private const int HashBytes = 32;

        public static string Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password ne sme biti prazan", nameof(password));

            var salt = new byte[SaltBytes];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(salt);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                var hash = pbkdf2.GetBytes(HashBytes);
                return string.Format("{0}.{1}.{2}",
                    Iterations,
                    Convert.ToBase64String(salt),
                    Convert.ToBase64String(hash));
            }
        }

        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(stored))
                return false;

            var parts = stored.Split('.');
            if (parts.Length != 3) return false;
            if (!int.TryParse(parts[0], out var iterations)) return false;

            byte[] salt, hash;
            try
            {
                salt = Convert.FromBase64String(parts[1]);
                hash = Convert.FromBase64String(parts[2]);
            }
            catch (FormatException) { return false; }

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                var computed = pbkdf2.GetBytes(hash.Length);
                return ConstantTimeEquals(hash, computed);
            }
        }

        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            var diff = 0;
            for (var i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}