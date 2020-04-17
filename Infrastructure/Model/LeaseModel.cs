using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Database.ContraCoreDB;
using Infrastructure.Schema;

namespace Infrastructure.Model
{
    public static class LeaseModel
    {
        public static List<Lease> LeaseDetailsByMAC(TimeSpan since)
        {
            using ContraCoreDBContext contraDB = new ContraCoreDBContext();

            // TODO: Don't use client evaluation
            return contraDB.lease_details_by_mac
                           .ToList()
                           .Where(v => v.time!.Value > DateTime.Now.Subtract(since))
                           .Select(v => new Lease(v))
                           .ToList();
        }

        public static List<Lease> FindByIP(IPAddress ip)
        {
            using ContraCoreDBContext contraDB = new ContraCoreDBContext();

            return contraDB.lease_details_by_ip
                           .Where(v => Equals(v.ip, ip))
                           .Select(v => new Lease(v))
                           .ToList();
        }
    }
}