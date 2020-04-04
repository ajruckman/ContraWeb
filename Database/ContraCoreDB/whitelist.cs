using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace Database.ContraCoreDB
{
    public partial class whitelist
    {
        public int id { get; set; }
        public string pattern { get; set; }
        public DateTime? expires { get; set; }
        public IPAddress[] ips { get; set; }
        public ValueTuple<IPAddress, int>[] subnets { get; set; }
        public string[] hostnames { get; set; }
        public PhysicalAddress[] macs { get; set; }
        public string[] vendors { get; set; }
    }
}
