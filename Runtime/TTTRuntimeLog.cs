using System;

namespace net.rs64.TexTransTool
{
    internal static class TTTRuntimeLog
    {
        public static void Info(string code, params object[] objects)
        {
            InfoCall?.Invoke(code, objects);
        }
        public static void Warning(string code, params object[] objects)
        {
            WarningCall?.Invoke(code, objects);
        }

        public static void Error(string code, params object[] objects)
        {
            ErrorCall?.Invoke(code, objects);
        }

        public static Action<string, object[]> InfoCall;
        public static Action<string, object[]> WarningCall;
        public static Action<string, object[]> ErrorCall;

    }
}
