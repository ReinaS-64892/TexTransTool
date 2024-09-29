using System;
using System.Linq;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal static class TTTRuntimeLog
    {
        /*
        ログのレベルに関するメモ

        # info

        - ユーザーの操作が足りていない場合
        - 足りていても状況的に無効だったりと実行できない場合

        ## example

        - TargetNotSet -> ターゲットが設定されていない場合
        - TargetNotFound -> ターゲットが設定されているが、ドメイン内に存在しない場合

        # warn

        - 意図しずらい状態になる場合
        - 今後廃止される機能などの場合

        # error

        - 実行時エラー
        - どうしようもなく失敗した場合

        */
        internal const string LogPrefix = "TTTRuntimeLog:";
        public static void Info(string code, params object[] objects)
        {
            InfoCall?.Invoke(code, objects);
#if TTT_DISPLAY_RUNTIME_LOG
            Debug.Log(LogPrefix + code + ":" + string.Join('-', objects.Select(i => i.ToString())));
#endif
        }
        public static void Warning(string code, params object[] objects)
        {
            WarningCall?.Invoke(code, objects);

#if TTT_DISPLAY_RUNTIME_LOG
            Debug.LogWarning(LogPrefix + code + ":" + string.Join('-', objects.Select(i => i.ToString())));
#endif
        }

        public static void Error(string code, params object[] objects)
        {
            ErrorCall?.Invoke(code, objects);

#if TTT_DISPLAY_RUNTIME_LOG
            Debug.LogError(LogPrefix + code + ":" + string.Join('-', objects.Select(i => i.ToString())));
#endif
        }
        public static void Exception(Exception e, string additionalStackTrace = "")
        {
            ExceptionCall?.Invoke(e, additionalStackTrace);

#if TTT_DISPLAY_RUNTIME_LOG
            Debug.LogException(e);
            Debug.LogError(e.Message + "-AdditionalStackTrace:" + additionalStackTrace);
#endif
        }

        public static Action<string, object[]> InfoCall;
        public static Action<string, object[]> WarningCall;
        public static Action<string, object[]> ErrorCall;
        public static Action<Exception, string> ExceptionCall;

    }
}
