using System;
using net.rs64.TexTransTool.Preview;
using net.rs64.TexTransTool.Preview.RealTime;
using UnityEditor;

namespace net.rs64.TexTransTool.Editor.OtherMenuItem
{
    internal class PreviewUtility
    {
        static Action s_rePreviewActin;
        [MenuItem("Tools/TexTransTool/Exit Previews")]
        public static void ExitPreviews()
        {
            if (RealTimePreviewContext.instance.IsPreview()) { RealTimePreviewContext.instance.ExitRealTimePreview(); return; }
            if (OneTimePreviewContext.IsPreviewContains) { OneTimePreviewContext.instance.ExitPreview(); }
        }
        [MenuItem("Tools/TexTransTool/RePreview")]
        public static void RePreview()
        {
            ExitPreviews();
            s_rePreviewActin?.Invoke();
        }

        public static bool IsPreviewContains => RealTimePreviewContext.instance.IsPreview() || OneTimePreviewContext.IsPreviewContains;
        [TexTransUnityCore.TexTransInitialize]
        public static void Init()
        {
            OneTimePreviewContext.instance.OnPreviewEnter += ttb => { s_rePreviewActin = () => { if (ttb is TexTransBehavior texTransBehavior) { OneTimePreviewContext.instance.ApplyTexTransBehavior(texTransBehavior); } }; };
            RealTimePreviewContext.instance.OnPreviewEnter += dr => { s_rePreviewActin = () => { RealTimePreviewContext.instance.EnterRealtimePreview(dr); }; };
        }
    }
}
