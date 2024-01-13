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

        //実行不可はExceptionに

        public static Action<string, object[]> InfoCall;
        public static Action<string, object[]> WarningCall;

    }
}
