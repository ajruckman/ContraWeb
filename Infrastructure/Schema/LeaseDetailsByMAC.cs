using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using Database.ContraCoreDB;

namespace Infrastructure.Schema
{
    public readonly struct Lease
    {
        public readonly DateTime        Time;
        public readonly PhysicalAddress MAC;
        public readonly IPAddress       IP;
        public readonly string?         Hostname;
        public readonly string?         Vendor;

        // public Lease(DateTime time, PhysicalAddress mac, string hostname, string vendor)
        // {
        // Time = time;
        // MAC  = mac;
        // MAC      = string.Join(':', mac.GetAddressBytes().Select(v => v.ToString("X2")));
        // Hostname = hostname;
        // Vendor   = vendor;
        // }

        public Lease(lease_details_by_ip dbLease)
        {
            Time     = dbLease.time!.Value;
            MAC      = dbLease.mac;
            IP       = dbLease.ip;
            Hostname = dbLease.hostname.Length != 0 ? dbLease.hostname : null;
            Vendor   = dbLease.vendor.Length   != 0 ? dbLease.vendor : null;
        }

        public Lease(lease_details_by_mac dbLease)
        {
            Time     = dbLease.time!.Value;
            MAC      = dbLease.mac;
            IP       = dbLease.ip;
            Hostname = dbLease.hostname.Length != 0 ? dbLease.hostname : null;
            Vendor   = dbLease.vendor.Length   != 0 ? dbLease.vendor : null;
        }
    }
}