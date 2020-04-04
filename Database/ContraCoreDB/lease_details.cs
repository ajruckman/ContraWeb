using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace Database.ContraCoreDB
{
    public partial class lease_details
    {
        public DateTime? time { get; set; }
        public string op { get; set; }
        public PhysicalAddress mac { get; set; }
        public IPAddress ip { get; set; }
        public string hostname { get; set; }
        public string vendor { get; set; }
    }
}
