using System;
using System.Linq;

namespace API_v2.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public static string GenerateRandomPassword(int length = 10)
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*";
            
            var random = new Random();
            var password = new char[length];
            password[0] = upper[random.Next(upper.Length)];
            password[1] = lower[random.Next(lower.Length)];
            password[2] = digits[random.Next(digits.Length)];
            password[3] = special[random.Next(special.Length)];
            
            const string allChars = upper + lower + digits + special;
            for (int i = 4; i < length; i++)
            {
                password[i] = allChars[random.Next(allChars.Length)];
            }
            
            return new string(password.OrderBy(x => random.Next()).ToArray());
        }
    }
}
