using Superset.Logging;

namespace Infrastructure
{
    internal static class Common
    {
        internal static readonly Logger Logger = new Logger(printSourceInfo: true, projectRoot: "Infrastructure");
    }
}