using UnityEditor;

namespace net.rs64.TexTransTool.Editor.OtherMenuItem
{
    internal class PreviewUtility
    {
        [MenuItem("Tools/TexTransTool/Exit Previews")]
        public static void ExitPreviews()
        {
            if (RealTimePreviewManager.IsContainsRealTimePreviewDecal) { RealTimePreviewManager.instance.ExitPreview(); return; }
            if (PreviewContext.IsPreviewContains) { PreviewContext.instance.ExitPreview(); }
        }
        [MenuItem("Tools/TexTransTool/RePreview")]
        public static void RePreview()
        {
            PreviewContext.instance.RePreview();

        }

        public static bool IsPreviewContains => RealTimePreviewManager.IsContainsRealTimePreviewDecal || PreviewContext.IsPreviewContains;
    }
}
