using System.Collections.Generic;
using System.Linq;
using Database.ContraCoreDB;
using FS3;
using Superset.Common;

namespace Infrastructure.Model
{
    public static class OUIModel
    {
        private static IEnumerable<IOption<string>> _cache;

        public static IEnumerable<IOption<string>> Options()
        {
            using ContraCoreDBContext contraDB = new ContraCoreDBContext();
            if (_cache == null)
                _cache = contraDB.oui_vendors
                                 .Take(100)
                                 .Select(v => new Option<string>
                                  {
                                      ID           = v.vendor,
                                      OptionText   = v.vendor,
                                      SelectedText = v.vendor
                                  }).ToList();

            return _cache;
        }

        public static int OUICount()
        {
            using ContraCoreDBContext contraDB = new ContraCoreDBContext();
            return contraDB.oui.Count();
        }
    }
}