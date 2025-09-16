using System;
using System.Security.Cryptography;

namespace Tourism_Website.Security
{
    public static class PasswordHashing
    {
        private const int SaltSize = 16;            // 128-bit
        private const int KeySize = 32;            // 256-bit derived key
        private const int DefaultIterations = 100000;
        private const string AlgoName = "SHA1";     // .NET Framework PBKDF2 = HMAC-SHA1

        public static string Hash(string password, int iterations = DefaultIterations)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            var salt = new byte[SaltSize];
            using (var rng = new RNGCryptoServiceProvider()) { rng.GetBytes(salt); }

            byte[] key;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                key = pbkdf2.GetBytes(KeySize);
            }

            return $"PBKDF2${AlgoName}${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
        }

        public static bool Verify(string password, string stored, out bool needsUpgrade)
        {
            needsUpgrade = false;

            if (string.IsNullOrEmpty(stored))
                return false;

            // Legacy plain-text support (optional): if not our format, compare directly
            if (!stored.StartsWith("PBKDF2$", StringComparison.Ordinal))
                return string.Equals(password, stored);

            var parts = stored.Split('$');
            if (parts.Length != 5 || parts[0] != "PBKDF2") return false;

            var algo = parts[1];
            if (!int.TryParse(parts[2], out var iterations)) return false;
            var salt = Convert.FromBase64String(parts[3]);
            var expected = Convert.FromBase64String(parts[4]);

            // On .NET Framework we only support SHA1 here
            if (!algo.Equals("SHA1", StringComparison.OrdinalIgnoreCase)) return false;

            byte[] actual;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                actual = pbkdf2.GetBytes(expected.Length);
            }

            var ok = ConstantTimeEquals(expected, actual);

            // Suggest upgrading if your iteration count is lower than today’s default
            if (ok && iterations < DefaultIterations) needsUpgrade = true;

            return ok;
        }

        public static bool IsFormattedHash(string s) =>
            s != null && s.StartsWith("PBKDF2$", StringComparison.Ordinal);

        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
