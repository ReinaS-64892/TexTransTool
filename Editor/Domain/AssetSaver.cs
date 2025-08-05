#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace net.rs64.TexTransTool
{
    internal class AssetSaver : IAssetSaver
    {
        [NotNull] public AvatarDomainAsset Asset;
        public const string SaveDirectory = "Assets/TexTransToolGenerates";

        public static void CheckSaveDirectory()
        {
            if (!Directory.Exists(SaveDirectory)) { Directory.CreateDirectory(SaveDirectory); }
        }

        public AssetSaver(Object? container = null)
        {
            if (container == null)
            {
                Asset = ScriptableObject.CreateInstance<AvatarDomainAsset>();
                CheckSaveDirectory();
                AssetDatabase.CreateAsset(Asset, AssetDatabase.GenerateUniqueAssetPath(Path.Combine(SaveDirectory, "AvatarDomainAsset.asset")));
            }
            else
            {
                Asset = ScriptableObject.CreateInstance<AvatarDomainAsset>();
                Asset.OverrideContainer = container;
                Asset.name = "net.rs64.TexTransTool.AssetContainer";
                Asset.AddSubObject(Asset);
            }
        }

        public AssetSaver(string path)
        {
            Asset = ScriptableObject.CreateInstance<AvatarDomainAsset>();
            AssetDatabase.CreateAsset(Asset, path);
        }

        public void SaveAsset(UnityEngine.Object unityObject)
        {
            Asset.AddSubObject(unityObject);
        }
    }
    internal class TempAssetHolder : IAssetSaver, IDisposable
    {
        HashSet<UnityEngine.Object> Transferred = new();
        public TempAssetHolder() { }

        public void SaveAsset(UnityEngine.Object unityObject) { Transferred.Add(unityObject); }
        public void Dispose() { foreach (var obj in Transferred) { UnityEngine.Object.DestroyImmediate(obj); } }
    }
}
