using System.Collections.Generic;
using System.Linq;
using Database.ContraDB;
using FlareSelect;
using Superset.Common;

namespace Infrastructure.Model
{
    public static class OUIModel
    {
        private static IEnumerable<IOption> _cache;

        public static IEnumerable<IOption> Options()
        {
            using ContraDBContext contraDB = new ContraDBContext();
            return _cache ??=
                contraDB.oui_vendors
                        .Select(v => new Option
                         {
                             ID           = v.vendor,
                             Text         = v.vendor,
                             SelectedText = v.vendor
                         }).ToList();
        }
    }
}