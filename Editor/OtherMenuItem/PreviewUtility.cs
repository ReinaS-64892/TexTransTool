using net.rs64.TexTransTool.Preview;
using net.rs64.TexTransTool.Preview.RealTime;
using UnityEditor;

namespace net.rs64.TexTransTool.Editor.OtherMenuItem
{
    internal class PreviewUtility
    {
        [MenuItem("Tools/TexTransTool/Exit Previews")]
        public static void ExitPreviews()
        {
            if (RealTimePreviewContext.instance.IsPreview()) { RealTimePreviewContext.instance.ExitRealTimePreview(); return; }
            if (OneTimePreviewContext.IsPreviewContains) { OneTimePreviewContext.instance.ExitPreview(); }
        }
        [MenuItem("Tools/TexTransTool/RePreview")]
        public static void RePreview()
        {
            OneTimePreviewContext.instance.RePreview();

        }

        public static bool IsPreviewContains => RealTimePreviewContext.instance.IsPreview() || OneTimePreviewContext.IsPreviewContains;
    }
}
