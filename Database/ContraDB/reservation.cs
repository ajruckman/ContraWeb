using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace Database.ContraDB
{
    public partial class reservation
    {
        public int id { get; set; }
        public DateTime time { get; set; }
        public bool? active { get; set; }
        public PhysicalAddress mac { get; set; }
        public IPAddress ip { get; set; }
        public string creator { get; set; }
        public string comment { get; set; }
    }
}
