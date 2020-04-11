using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace Database.ContraCoreDB
{
    public partial class distinct_lease_clients
    {
        public DateTime? time { get; set; }
        public PhysicalAddress mac { get; set; }
        public string hostname { get; set; }
        public string vendor { get; set; }
    }
}
