#nullable enable
using System.Linq;
using Database.ContraDB;
using Infrastructure.Schema;

namespace Infrastructure.Controller
{
    public class ConfigController
    {
        public Config? Read()
        {
            using ContraDBContext contraDB = new ContraDBContext();

            if (!contraDB.config.Any())
                return null;
            
            config config = contraDB.config.OrderByDescending(v => v.id).Take(1).Single();

            return new Config
            {
                ID             = config.id,
                Sources        = config.sources.ToList(),
                SearchDomains  = config.search_domains.ToList(),
                DomainNeeded   = config.domain_needed ?? false,
                SpoofedA       = config.spoofed_a,
                SpoofedAAAA    = config.spoofed_aaaa,
                SpoofedCNAME   = config.spoofed_cname,
                SpoofedDefault = config.spoofed_default
            };
        }
        
        public void Update(Config config)
        {
            using ContraDBContext contraDB = new ContraDBContext();

            contraDB.config.Add(new config
            {
                id              = config.ID,
                sources         = config.Sources.ToArray(),
                search_domains  = config.SearchDomains.ToArray(),
                domain_needed   = config.DomainNeeded,
                spoofed_a       = config.SpoofedA,
                spoofed_aaaa    = config.SpoofedAAAA,
                spoofed_cname   = config.SpoofedCNAME,
                spoofed_default = config.SpoofedDefault
            });

            contraDB.SaveChanges();
        }
    }
}