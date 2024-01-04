using System;
using UnityEngine;
using System.Runtime.CompilerServices;


#if NDMF
using nadena.dev.ndmf;
#endif

[assembly: InternalsVisibleTo("net.rs64.tex-trans-tool")]
[assembly: InternalsVisibleTo("net.rs64.tex-trans-tool.Inspector")]

namespace net.rs64.TexTransTool
{

        internal static class TTTLog
        {
                public static void Info(string code, params object[] objects)
                {
#if NDMF
                        ErrorReport.ReportError(NDMFLocalizer, ErrorSeverity.Information, code, objects);
#else
            Debug.Log(code);
#endif
                }
                public static void Warning(string code, params object[] objects)
                {
#if NDMF
                        ErrorReport.ReportError(NDMFLocalizer, ErrorSeverity.NonFatal, code, objects);
#else
            Debug.LogWarning(code);
#endif
                }
                public static void Fatal(string code, params object[] objects)
                {
#if NDMF
                        ErrorReport.ReportError(NDMFLocalizer, ErrorSeverity.Error, code, objects);
#else
            Debug.LogError(code);
#endif
                }
                public static void Exception(Exception e, string additionalStackTrace = "")
                {
#if NDMF
                        ErrorReport.ReportException(e, additionalStackTrace);
#else
            Debug.LogException(e);
#endif
                }

                public static void ReportingObject(UnityEngine.Object obj, Action action)
                {
#if NDMF
                        ErrorReport.WithContextObject(obj, action);
#else
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                LogException(e);
                throw e;
            }
#endif
                }

#if NDMF
                private static nadena.dev.ndmf.localization.Localizer NDMFLocalizer =>
                        new nadena.dev.ndmf.localization.Localizer("ja-JP", () => { return new() { ("ja-JP", Localize.GetLocalize) }; });
#endif
        }


        [System.Serializable]
        public class TTTException : System.Exception
        {
                public TTTException() { }
                public TTTException(string message) : base(message) { }
                public TTTException(string message, System.Exception inner) : base(message, inner) { }
                protected TTTException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
}
