using System.Linq;
using Database.ContraCoreDB;

namespace Infrastructure.Model
{
    public static class StatsModel
    {
        public static dynamic LeaseVendorCounts(int min)
        {
            using ContraCoreDBContext contraDB = new ContraCoreDBContext();

            return contraDB.lease_vendor_counts
                           .Where(v => v.c >= min)
                           .Select(v => new
                            {
                                vendor  = v.vendor,
                                c       = v.c!.Value,
                                percent = v.ratio!.Value
                            })
                           .ToList();
        }
    }
}