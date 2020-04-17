using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Database.ContraCoreDB;
using Infrastructure.Schema;

namespace Infrastructure.Model
{
    public static class WhitelistModel
    {
        public static List<Whitelist> List()
        {
            using ContraCoreDBContext contraDB = new ContraCoreDBContext();

            return contraDB.whitelist
                           .OrderByDescending(v => v.id)
                           .Select(v => new Whitelist(v))
                           .ToList();
        }

        public static IEnumerable<Whitelist> List(string clientIP)
        {
            using ContraCoreDBContext contraDB = new ContraCoreDBContext();

            return contraDB.whitelist
                           .Where(v => v.ips        != null)
                           .Where(v => v.ips.Length == 1)
                           .OrderByDescending(v => v.id)
                           .ToList()
                           .Where(v => v.ips[0].ToString() == clientIP)
                           .Select(v => new Whitelist(v))
                           .ToList();
        }

        public static void Submit(Whitelist whitelist)
        {
            using ContraCoreDBContext contraDB = new ContraCoreDBContext();

            whitelist dbWhitelist = new whitelist
            {
                pattern = whitelist.Pattern,
                expires = whitelist.Expires,
                ips     = whitelist.IPs?.ToArray(),
                subnets = whitelist.Subnets?
                                   .Select(v => new ValueTuple<IPAddress, int>(v.Network, v.Cidr))
                                   .ToArray(),
                hostnames = whitelist.Hostnames?.ToArray(),
                macs      = whitelist.MACs?.ToArray(),
                vendors   = whitelist.Vendors?.ToArray()
            };

            contraDB.Add(dbWhitelist);
            contraDB.SaveChanges();

            whitelist.ID = dbWhitelist.id;
        }

        public static void Update(Whitelist whitelist)
        {
            using ContraCoreDBContext contraDB = new ContraCoreDBContext();

            whitelist? match = contraDB.whitelist.SingleOrDefault(v => v.id == whitelist.ID);
            if (match == null)
                throw new ArgumentException($"Could not find whitelist rule with ID '{whitelist.ID}'");

            match.pattern = whitelist.Pattern;
            match.expires = whitelist.Expires;
            match.ips     = whitelist.IPs?.ToArray();
            match.subnets = whitelist.Subnets?
                                     .Select(v => new ValueTuple<IPAddress, int>(v.Network, v.Cidr))
                                     .ToArray();
            match.hostnames = whitelist.Hostnames?.ToArray();
            match.macs      = whitelist.MACs?.ToArray();
            match.vendors   = whitelist.Vendors?.ToArray();

            contraDB.Update(match);
            contraDB.SaveChanges();
        }

        public static Whitelist? Find(int id)
        {
            using ContraCoreDBContext contraDB = new ContraCoreDBContext();

            whitelist match = contraDB.whitelist.SingleOrDefault(v => v.id == id);
            return match != null ? new Whitelist(match) : null;
        }

        public static void Remove(Whitelist row)
        {
            using ContraCoreDBContext contraDB = new ContraCoreDBContext();

            var match = contraDB.whitelist.SingleOrDefault(v => v.id == row.ID);

            contraDB.whitelist.Remove(match);
            contraDB.SaveChanges();
        }
    }
}