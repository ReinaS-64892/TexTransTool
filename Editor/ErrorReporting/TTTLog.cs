using System;
using UnityEngine;
using System.Runtime.CompilerServices;
using nadena.dev.ndmf.localization;



#if NDMF
using nadena.dev.ndmf;
#endif

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
                                Debug.LogException(e);
                                throw e;
                        }
#endif
                }

#if NDMF
                private static nadena.dev.ndmf.localization.Localizer NDMFLocalizer = new nadena.dev.ndmf.localization.Localizer("en-US",
                () =>
                {
                        return new() {
                        // TODO : このありえないほど治安の悪いKeyへのフォールバック回避をどうするか考えないといけないというか、
                        //そもそもこのツールのローカライズ対応をどうするべきかをそろそろ考えるべきだ...
                        ("en-US", (str) => str + "\u0020"),
                        ("ja-JP", (str) => {
                                var lcStr = Localize.GetLocalizeJP(str);
                                if(lcStr == str){ lcStr += "\u0020";}
                                return lcStr;
                        })
                        };
                });
#endif
        }



}
