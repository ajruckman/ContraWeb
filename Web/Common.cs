using Infrastructure.Controller;
using Superset.Logging;

namespace Web
{
    internal static class Common
    {
        internal static readonly Logger           Logger;
        internal static readonly ContraCoreClient ContraCoreClient;

        static Common()
        {
            Logger           = new Logger();
            ContraCoreClient = new ContraCoreClient("10.3.0.16");
        }
    }
}