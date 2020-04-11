using System;
using System.Collections.Generic;
using System.Linq;
using Database.ContraCoreDB;
using Infrastructure.Schema;

namespace Infrastructure.Model
{
    public static class StatsModel
    {
        public static List<DistinctLeaseClient> DistinctLeaseClients(TimeSpan timeSpan)
        {
            using ContraCoreDBContext contraDB = new ContraCoreDBContext();

            // TODO: Don't use client evaluation
            return contraDB.distinct_lease_clients
                           .ToList()
                           .Where(v => v.time!.Value > DateTime.Now.Subtract(timeSpan))
                           .Select(v => new DistinctLeaseClient(v.time!.Value, v.mac, v.hostname, v.vendor))
                           .ToList();
        }

        public static Dictionary<string, long> LeaseVendorCounts()
        {
            using ContraCoreDBContext contraDB = new ContraCoreDBContext();

            return contraDB.lease_vendor_counts
                           .ToDictionary(
                                k => k?.vendor ?? "<unmatched>",
                                v => v.c!.Value
                            );
        }
    }
}