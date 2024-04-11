using System;
using UnityEditor;
using UnityEngine;
using System.Linq;





#if NDMF_1_3_x
using nadena.dev.ndmf;
#endif

namespace net.rs64.TexTransTool
{

    internal static class TTTLog
    {
        [InitializeOnLoadMethod]
        static void RegisterCall()
        {
            TTTRuntimeLog.InfoCall += Info;
            TTTRuntimeLog.WarningCall += Warning;
            TTTRuntimeLog.ErrorCall += Error;
        }


        public static void Info(string code, params object[] objects)
        {
#if NDMF_1_3_x
            ErrorReport.ReportError(NDMFLocalizer, ErrorSeverity.Information, code, objects);
#else
            Debug.Log(TTTRuntimeLog.LogPrefix + code + ":" + string.Join('-', objects.Select(i => i.ToString())));
#endif
        }
        public static void Warning(string code, params object[] objects)
        {
#if NDMF_1_3_x
            ErrorReport.ReportError(NDMFLocalizer, ErrorSeverity.NonFatal, code, objects);
#else
            Debug.LogWarning(TTTRuntimeLog.LogPrefix + code + ":" + string.Join('-', objects.Select(i => i.ToString())));
#endif
        }
        public static void Error(string code, params object[] objects)
        {
#if NDMF_1_3_x
            ErrorReport.ReportError(NDMFLocalizer, ErrorSeverity.Error, code, objects);
#else
            Debug.LogError(TTTRuntimeLog.LogPrefix + code + ":" + string.Join('-', objects.Select(i => i.ToString())));
#endif
        }
        public static void Exception(Exception e, string additionalStackTrace = "")
        {
#if NDMF_1_3_x
            ErrorReport.ReportException(e, additionalStackTrace);
#else
            Debug.LogException(e);
            Debug.LogError(e.Message + "-AdditionalStackTrace:" + additionalStackTrace);
#endif
        }

        public static void ReportingObject(UnityEngine.Object obj, Action action)
        {
#if NDMF_1_3_x
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

#if NDMF_1_3_x
        private static nadena.dev.ndmf.localization.Localizer _ndmfLocalizer;
        public static nadena.dev.ndmf.localization.Localizer NDMFLocalizer
        {
            get
            {
                if (_ndmfLocalizer == null)
                {
                    Localize.LoadLocalize();
                    _ndmfLocalizer = new nadena.dev.ndmf.localization.Localizer("en-US", () => Localize.LocalizationAssets.Values.ToList());
                }
                return _ndmfLocalizer;
            }
        }
#endif
    }



}
