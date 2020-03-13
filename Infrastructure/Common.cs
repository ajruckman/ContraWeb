using Superset.Logging;

namespace Infrastructure
{
    internal static class Common
    {
        internal static readonly Logger Logger;

        static Common()
        {
            Logger = new Logger();
        }
    }
}