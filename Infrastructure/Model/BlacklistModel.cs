using System.Collections.Generic;
using System.Linq;
using Database.ContraCoreDB;
using Infrastructure.Schema;

namespace Infrastructure.Model
{
    public static class BlacklistModel
    {
        public static void Submit(Blacklist newRule)
        {
            using ContraCoreDBContext contraDB = new ContraCoreDBContext();

            contraDB.blacklist.Add(new blacklist
            {
                pattern = newRule.Pattern,
                expires = newRule.Expires,
                _class  = 0,
            });

            contraDB.SaveChanges();
        }

        public static IEnumerable<Blacklist> List()
        {
            using ContraCoreDBContext contraDB = new ContraCoreDBContext();

            return contraDB.blacklist.Select(v => new Blacklist(v)).ToList();
        }

        public static void Remove(Blacklist row)
        {
            using ContraCoreDBContext contraDB = new ContraCoreDBContext();

            var match = contraDB.blacklist.SingleOrDefault(v => v.id == row.ID);

            contraDB.blacklist.Remove(match);
            contraDB.SaveChanges();
        }
    }
}