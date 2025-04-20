using System;
using UnityEditor;
using System.Linq;
using net.rs64.TexTransCore;
using UnityEngine;


#if CONTAINS_NDMF
using nadena.dev.ndmf;
#endif

namespace net.rs64.TexTransTool
{

    internal static class TTTLog
    {
        [InitializeOnLoadMethod]
        static void RegisterCall()
        {
            TTLog.LogCall += Info;
            TTLog.WarningCall += Warning;
            TTLog.ErrorCall += Error;
            TTLog.ExceptionCall += Exception;
        }


        public static void Info(string code, params object[] objects)
        {
#if CONTAINS_NDMF
            ErrorReport.ReportError(NDMFLocalizer, ErrorSeverity.Information, code, objects);
#else
            Debug.Log("[info]" + code + ":" + string.Join('-', objects.Select(i => i.ToString())));
#endif
        }
        public static void Warning(string code, params object[] objects)
        {
#if CONTAINS_NDMF
            ErrorReport.ReportError(NDMFLocalizer, ErrorSeverity.NonFatal, code, objects);
#else
            Debug.LogWarning("[Warning]" + code + ":" + string.Join('-', objects.Select(i => i.ToString())));
#endif
        }
        public static void Error(string code, params object[] objects)
        {
#if CONTAINS_NDMF
            ErrorReport.ReportError(NDMFLocalizer, ErrorSeverity.Error, code, objects);
#else
            Debug.LogError("[Error]" + code + ":" + string.Join('-', objects.Select(i => i.ToString())));
#endif
        }
        public static void Exception(Exception e, string additionalStackTrace = "")
        {
#if CONTAINS_NDMF
            ErrorReport.ReportException(e, additionalStackTrace);
#else
            Debug.LogException(e);
            Debug.LogError(e.Message + "-AdditionalStackTrace:" + additionalStackTrace);
#endif
        }

        public static void ReportingObject(UnityEngine.Object obj, Action action)
        {
#if CONTAINS_NDMF
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

#if CONTAINS_NDMF
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
