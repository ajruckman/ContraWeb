
using System.Linq;
using Database.ContraCoreDB;
using Infrastructure.Schema;
using Superset.Logging;

namespace Infrastructure.Model
{
    public static class ConfigModel
    {
        public static Config? Read()
        {
            using ContraCoreDBContext contraDB = new ContraCoreDBContext();

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

        public static bool Update(Config config)
        {
            using ContraCoreDBContext contraDB = new ContraCoreDBContext();

            Config? current = Read();
            if (current != null)
                if (current.ToString() == config.ToString())
                {
                    Common.Logger.Info(
                        "The latest config in the database matches the new config passed to ConfigController.Update(); not storing new config.",
                        new Fields {{"Config", config.ToString()}}
                    );
                    return true;
                }

            Common.Logger.Info(
                "Storing new config in database.",
                new Fields {{"Config", config.ToString()}}
            );

            contraDB.config.Add(new config
            {
                id              = 0,
                sources         = config.Sources.ToArray(),
                search_domains  = config.SearchDomains.ToArray(),
                domain_needed   = config.DomainNeeded,
                spoofed_a       = config.SpoofedA,
                spoofed_aaaa    = config.SpoofedAAAA,
                spoofed_cname   = config.SpoofedCNAME,
                spoofed_default = config.SpoofedDefault
            });

            contraDB.SaveChanges();

            return true;
        }

        public static int BlacklistRuleCount()
        {
            using ContraCoreDBContext contraDB = new ContraCoreDBContext();
            return contraDB.blacklist.Count();
        }
    }
}