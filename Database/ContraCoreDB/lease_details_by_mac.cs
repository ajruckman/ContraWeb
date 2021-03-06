﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace Database.ContraCoreDB
{
    public partial class lease_details_by_mac
    {
        public DateTime? time { get; set; }
        public PhysicalAddress mac { get; set; }
        public IPAddress ip { get; set; }
        public string hostname { get; set; }
        public string vendor { get; set; }
    }
}
