using net.rs64.TexTransTool;
using net.rs64.TexTransTool.TextureAtlas;
using UnityEditor;


namespace net.rs64.TexTransTool.Editor.OtherMenuItem
{public static class ReloadAtlasShaderSupporter
{
    [MenuItem("Tools/" + TexTransBehavior.TTTName + "/Debug/ReloadAtlasShaderSupporter")]
    public static void Reload() { AtlasShaderSupportUtils.Initialize(); }
}
}
