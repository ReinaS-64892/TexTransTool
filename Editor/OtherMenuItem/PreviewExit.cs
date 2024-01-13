using UnityEditor;

namespace net.rs64.TexTransTool.Editor.OtherMenuItem
{
    internal class PreviewExit
    {
        [MenuItem("Tools/TexTransTool/Exit Previews")]
        public static void ExitPreviews()
        {
            if (RealTimePreviewManager.instance.ContainsPreview) { RealTimePreviewManager.instance.ExitPreview(); return; }
            if (PreviewContext.IsPreviewContains) { PreviewContext.instance.ExitPreview(); }
        }

    }
}
