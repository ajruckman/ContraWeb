using System;
using System.Collections.Generic;

namespace Database.ContraCoreDB
{
    public partial class blacklist
    {
        public int id { get; set; }
        public string pattern { get; set; }
        public DateTime? expires { get; set; }
        public int _class { get; set; }
        public string domain { get; set; }
        public string tld { get; set; }
        public string sld { get; set; }
    }
}
