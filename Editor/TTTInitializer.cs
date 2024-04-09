using System.Linq;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.MipMap;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.TextureAtlas;
using UnityEditor;
using UnityEngine;
namespace net.rs64.TexTransTool.Utils
{
    internal static class TTTInitializeCaller
    {
        [UnityEditor.InitializeOnLoadMethod]
        static void EditorInitDerayCall()
        {
            UnityEditor.EditorApplication.delayCall += EditorInitDerayCaller;
        }
        static void EditorInitDerayCaller()
        {
            Initialize();
            UnityEditor.EditorApplication.delayCall -= EditorInitDerayCaller;
        }
        [UnityEditor.InitializeOnEnterPlayMode]
        public static void Initialize()
        {
            UnityEditor.EditorApplication.update += TexTransCoreRuntime.Update.Invoke;
            TexTransCoreRuntime.LoadAsset = (guid, type) => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), type);
            TexTransCoreRuntime.LoadAssetsAtType = (type) =>
            {
                return UnityEditor.AssetDatabase.GetAllAssetPaths()
                    .Where(i => AssetDatabase.GetMainAssetTypeAtPath(i) == type)
                    .Select(i => AssetDatabase.LoadAssetAtPath(i, type));
            };
            TexTransInitialize.CallInitialize();
        }
    }
}
