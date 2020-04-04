
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using Database.ContraCoreDB;

namespace Infrastructure.Schema
{
    public class Whitelist
    {
        public int                    ID        { get; set; }
        public string                 Pattern   { get; set; } = "";
        public DateTime?              Expires   { get; set; }
        public List<IPAddress>?       IPs       { get; set; }
        public List<IPNetwork>?       Subnets   { get; set; }
        public List<string>?          Hostnames { get; set; }
        public List<PhysicalAddress>? MACs      { get; set; }
        public List<string>?          Vendors   { get; set; }
        
        public dynamic RemoveButton { get; set; }

        public Whitelist() { }

        public Whitelist(whitelist whitelist)
        {
            ID        = whitelist.id;
            Pattern   = whitelist.pattern;
            Expires   = whitelist.expires;
            IPs       = whitelist.ips?.ToList();
            Subnets   = whitelist.subnets?.Select(v => IPNetwork.Parse(v.Item1.ToString(), (byte) v.Item2)).ToList();
            MACs      = whitelist.macs?.ToList();
            Vendors   = whitelist.vendors?.ToList();
            Hostnames = whitelist.hostnames?.ToList();
        }
    }
}