using System;
using Database.ContraCoreDB;

namespace Infrastructure.Schema
{
    public class Blacklist
    {
        public int       ID      { get; set; }
        public string    Pattern { get; set; } = null!;
        public DateTime? Expires { get; set; }
        public int       Class   { get; set; }
        public string    Domain  { get; set; } = null!;
        public string    TLD     { get; set; } = null!;
        public string    SLD     { get; set; } = null!;

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