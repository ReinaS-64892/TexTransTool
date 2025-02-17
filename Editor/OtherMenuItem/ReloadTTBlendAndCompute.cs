using System.Linq;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool;
using net.rs64.TexTransTool.TextureAtlas;
using UnityEditor;

namespace net.rs64.TexTransTool.Editor.OtherMenuItem
{
    public static class ReloadTTBlendAndCompute
    {
        [MenuItem("Tools/" + TexTransBehavior.TTTName + "/Debug/ReloadTTBlendAndCompute")]
        public static void Reload()
        {
            var reloadTargets = AssetDatabase.FindAssets("t:TTBlendUnityObject")
             .Concat(AssetDatabase.FindAssets("t:TTComputeUnityObject"))
             .Concat(AssetDatabase.FindAssets("t:TTComputeOperator"))
             .Concat(AssetDatabase.FindAssets("t:TTComputeDownScalingUnityObject"))
             .Concat(AssetDatabase.FindAssets("t:TTComputeUpScalingUnityObject")).Distinct();

            foreach (var asset in reloadTargets)
                AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(asset));
            ComputeObjectUtility.ComputeObjectsInit();
        }
    }
}
