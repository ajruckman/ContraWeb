using Infrastructure.Controller;
using Superset.Logging;

namespace Web
{
    public static class Common
    {
        internal static readonly Logger           Logger;
        internal static readonly ContraCoreClient ContraCoreClient;

        static Common()
        {
            Logger = new Logger(printSourceInfo: true);
            #if DOCKER
            ContraCoreClient = new ContraCoreClient("contracore");
            #else
            ContraCoreClient = new ContraCoreClient("10.3.0.16");
            #endif
        }
    }
}