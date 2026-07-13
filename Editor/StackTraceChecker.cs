using System.Linq;

namespace Unity.PlasticSCM.Editor
{
    internal static class StackTraceChecker
    {
        internal static bool IsPlasticStackTrace(string stackTrace)
        {
            if (stackTrace == null)
                return false;

            return PlasticNamespacePrefixes.Any(stackTrace.Contains);
        }

        static readonly string[] PlasticNamespacePrefixes = new[] {
            "Codice.",
            "GluonGui.",
            "PlasticGui."
        };
    }
}
