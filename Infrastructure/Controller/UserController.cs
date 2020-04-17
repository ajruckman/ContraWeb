using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Infrastructure.Model;
using Infrastructure.Schema;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Infrastructure.Controller
{
    public static class UserController
    {
        public static User Create(string username, string password, UserRole.Roles role, List<PhysicalAddress> macs)
        {
            (string salt, string hash) = Hash(password);

            User user = new User(username, salt, hash, role, macs);

            UserModel.Submit(user);

            return user;
        }

        public static (string salt, string hash) Hash(string password)
        {
            // https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing

            byte[]                      salt = new byte[128 / 8];
            using RandomNumberGenerator rng  = RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            return (Convert.ToBase64String(salt), Hash(salt, password));
        }

        private static string Hash(byte[] salt, string password)
        {
            // https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA1,
                10000,
                256 / 8
            ));

            return hashed;
        }

        public static async Task<User?> LogIn(string username, string password)
        {
            User? user = await UserModel.Find(username);
            if (user == null)
                return null;

            if (user.Password == Hash(Convert.FromBase64String(user.Salt), password))
                return user;

            return null;
        }

        // https://stackoverflow.com/a/4616745/9911189
        private const string AllowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";

        private static string NewToken()
        {
            Random rd    = new Random();
            char[] chars = new char[32];

            for (var i = 0; i < 32; i++)
                chars[i] = AllowedChars[rd.Next(0, AllowedChars.Length)];

            return new string(chars);
        }

        public static async Task<UserSession?> FindSession(string token)
        {
            return await UserSessionModel.Find(token);
        }

        public static async Task<bool> HasActiveSession(User user)
        {
            UserSession? match = await UserSessionModel.Find(user);
            if (match == null)
                return false;

            if (DateTime.Now.Subtract(match.RefreshedAt) > TimeSpan.FromDays(7))
                return false;

            return true;
        }

        public static async Task<string> CreateUserSession(User user)
        {
            string token = NewToken();
            await UserSessionModel.Delete(user);
            await UserSessionModel.Create(user, token);
            return token;
        }

        public static async Task DeleteUserSession(User user)
        {
            await UserSessionModel.Delete(user);
        }

        public static async Task RefreshSession(User user)
        {
            await UserSessionModel.Refresh(user);
            // string token = NewToken();

            // await UserSessionModel.Delete(user);
            // await UserSessionModel.Create(user, token);
            // return token;
        }

        public static async Task UpdatePassword(User user, string password)
        {
            (string salt, string hash) = Hash(password);
            user.Salt                  = salt;
            user.Password              = hash;
            await UserModel.Update(user);
        }
    }
}