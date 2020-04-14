using System;
using Database.ContraCoreDB;

namespace Infrastructure.Schema
{
    public class Blacklist
    {
        public int       ID      { get; set; }
        public string    Pattern { get; set; }
        public DateTime? Expires { get; set; }
        public int       Class   { get; set; }
        public string    Domain  { get; set; }
        public string    TLD     { get; set; }
        public string    SLD     { get; set; }

        public Blacklist()
        {
            
        }
        
        public Blacklist(blacklist dbBlacklist)
        {
            ID      = dbBlacklist.id;
            Pattern = dbBlacklist.pattern;
            Expires = dbBlacklist.expires;
            Class   = dbBlacklist._class;
            Domain  = dbBlacklist.domain;
            TLD     = dbBlacklist.tld;
            SLD     = dbBlacklist.sld;
        }
    }
}