using Superset.Logging;

namespace Web
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