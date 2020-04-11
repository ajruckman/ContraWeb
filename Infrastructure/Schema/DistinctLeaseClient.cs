using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace Infrastructure.Schema
{
    public readonly struct DistinctLeaseClient
    {
        public readonly DateTime Time;
        public readonly string   MAC;
        public readonly string   Hostname;
        public readonly string   Vendor;

        public DistinctLeaseClient(DateTime time, PhysicalAddress mac, string hostname, string vendor)
        {
            Time     = time;
            MAC      = string.Join(':', mac.GetAddressBytes().Select(v => v.ToString("X2")));
            Hostname = hostname;
            Vendor   = vendor;
        }
    }
}