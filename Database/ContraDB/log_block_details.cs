using System;
using System.Collections.Generic;
using System.Net;

namespace Database.ContraDB
{
    public partial class log_block_details
    {
        public IPAddress client { get; set; }
        public string hostname { get; set; }
        public string vendor { get; set; }
        public string question { get; set; }
        public long? c { get; set; }
    }
}
