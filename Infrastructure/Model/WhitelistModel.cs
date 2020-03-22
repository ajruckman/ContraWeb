#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Database.ContraDB;
using Infrastructure.Schema;

namespace Infrastructure.Model
{
    public static class WhitelistModel
    {
        public static List<Whitelist> List()
        {
            using ContraDBContext contraDB = new ContraDBContext();

            return contraDB.whitelist.Select(v => new Whitelist(v)).ToList();
        }

        public static void Create(Whitelist whitelist)
        {
            using ContraDBContext contraDB = new ContraDBContext();

            whitelist dbWhitelist = new whitelist
            {
                id      = whitelist.ID,
                pattern = whitelist.Pattern,
                expires = whitelist.Expires,
                ips     = whitelist.IPs?.ToArray(),
                subnets = whitelist.Subnets?
                                   .Select(v => new ValueTuple<IPAddress, int>(v.Network, v.Cidr))
                                   .ToArray(),
                macs      = whitelist.MACs?.ToArray(),
                vendors   = whitelist.Vendors?.ToArray(),
                hostnames = whitelist.Hostnames?.ToArray()
            };

            contraDB.Add(dbWhitelist);
            contraDB.SaveChanges();

            whitelist.ID = dbWhitelist.id;
        }

        public static Whitelist? Find(int id)
        {
            using ContraDBContext contraDB = new ContraDBContext();

            whitelist match = contraDB.whitelist.SingleOrDefault(v => v.id == id);
            return match != null ? new Whitelist(match) : null;
        }
    }
}