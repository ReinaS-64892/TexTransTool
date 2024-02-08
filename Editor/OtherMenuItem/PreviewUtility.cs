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
        [MenuItem("Tools/TexTransTool/RePreview #r")]
        public static void RePreview()
        {
            PreviewContext.instance.RePreview();

        }
    }
}
