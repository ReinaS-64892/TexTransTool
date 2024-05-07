using System.Linq;
using net.rs64.TexTransCore;
using UnityEditor;
using UnityEngine.Profiling;

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
            Profiler.BeginSample("TTTInitializeCaller:TexTransCoreRuntime");
            UnityEditor.EditorApplication.update += () => { TexTransCoreRuntime.Update.Invoke(); };
            TexTransCoreRuntime.LoadAsset = (guid, type) => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), type);
            TexTransCoreRuntime.LoadAssetsAtType = (type) =>
            {
                return UnityEditor.AssetDatabase.FindAssets($"t:{type.Name}")
                    .Select(i => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(i), type));
            };
            Profiler.EndSample();
            Profiler.BeginSample("TexTransInitialize");
            TexTransInitialize.CallInitialize();
            Profiler.EndSample();
        }
    }
}
