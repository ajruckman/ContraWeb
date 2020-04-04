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
            Logger           = new Logger(printSourceInfo: true);
            ContraCoreClient = new ContraCoreClient("127.0.0.1");
        }
    }
}