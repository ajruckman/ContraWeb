using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace Database.ContraCoreDB
{
    public partial class lease_details_by_ip
    {
        public DateTime? time { get; set; }
        public IPAddress ip { get; set; }
        public PhysicalAddress mac { get; set; }
        public string hostname { get; set; }
        public string vendor { get; set; }
    }
}
