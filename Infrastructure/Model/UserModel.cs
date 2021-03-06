﻿using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Database.ContraWebDB;
using Infrastructure.Schema;

namespace Infrastructure.Model
{
    public static class UserModel
    {
        public static List<User> List()
        {
            using ContraWebDBContext contraDB = new ContraWebDBContext();

            return contraDB.user
                           .OrderBy(v => v.username)
                           .Select(v => new User(v))
                           .ToList();
        }

        public static void Submit(User user)
        {
            using ContraWebDBContext contraDB = new ContraWebDBContext();

            contraDB.user.Add(new user
            {
                username = user.Username,
                salt     = user.Salt,
                password = user.Password,
                role     = UserRole.RoleToDatabaseName(user.Role),
                macs     = user.MACs.ToArray()
            });

            contraDB.SaveChanges();
        }

        public static async Task<User?> Find(string username)
        {
            await using ContraWebDBContext contraDB = new ContraWebDBContext();

            user? match = contraDB.user.FirstOrDefault(v => v.username == username);

            return match != null ? new User(match) : null;
        }

        public static void Remove(User user)
        {
            using ContraWebDBContext contraDB = new ContraWebDBContext();

            user match = contraDB.user.Single(v => v.username == user.Username);

            contraDB.user.Remove(match);

            contraDB.SaveChanges();
        }

        public static async Task Update(User user)
        {
            await using ContraWebDBContext contraDB = new ContraWebDBContext();

            user match = contraDB.user.Single(v => v.username == user.Username);

            match.salt     = user.Salt;
            match.password = user.Password;
            match.role     = UserRole.RoleToDatabaseName(user.Role);
            match.macs     = user.MACs.ToArray();

            await contraDB.SaveChangesAsync();
        }

        public static async Task<List<User>> FindByMAC(PhysicalAddress mac)
        {
            await using ContraWebDBContext contraDB = new ContraWebDBContext();

            return contraDB.user.Where(v => v.macs.Contains(mac)).Select(v => new User(v)).ToList();
        }
    }
}