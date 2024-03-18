using UDebug = UnityEngine.Debug;

namespace YokAI.Debugging
{
    public static class Logger
    {
        public static void LogUnknownProperty(string source)
        {
            UDebug.LogWarning($"[{source}] Unknown Property");
        }

        public static void LogUnknownSymbol(string source)
        {
            UDebug.LogWarning($"[{source}] Unknown Symbol in notation");
        }

        public static void LogInvalidSFEN(string source)
        {
            UDebug.LogWarning($"[{source}] Invalid SFEN");
        }

        public static void LogInvalidMove(string source)
        {
            UDebug.LogWarning($"[{source}] Invalid Move");
        }
    }
}