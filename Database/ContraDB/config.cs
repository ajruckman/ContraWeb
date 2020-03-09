using System;
using System.Collections.Generic;

namespace Database.ContraDB
{
    public partial class config
    {
        public int id { get; set; }
        public string[] sources { get; set; }
        public string[] search_domains { get; set; }
        public bool? domain_needed { get; set; }
        public string spoofed_a { get; set; }
        public string spoofed_aaaa { get; set; }
        public string spoofed_cname { get; set; }
        public string spoofed_default { get; set; }
    }
}
