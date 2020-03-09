using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace Database.ContraDB
{
    public partial class log
    {
        public long id { get; set; }
        public DateTime time { get; set; }
        public IPAddress client { get; set; }
        public string question { get; set; }
        public string question_type { get; set; }
        public string action { get; set; }
        public string[] answers { get; set; }
        public PhysicalAddress client_mac { get; set; }
        public string client_hostname { get; set; }
        public string client_vendor { get; set; }
    }
}
