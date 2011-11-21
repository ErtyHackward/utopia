using System.Diagnostics;

namespace Utopia.Shared.Structs.Helpers
{
    public static class TraceHelper
    {
        public static void Write(string template, params object[] args)
        {
            Trace.WriteLine(string.Format(template, args));
        }
    }
}
