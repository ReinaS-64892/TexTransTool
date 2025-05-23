using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using net.rs64.TexTransTool.Build;
using System.Linq;

namespace net.rs64.TexTransTool.TestCode
{
    internal class SmallerMaterialSlotThanSubMesh
    {
        // サブメッシュの数よりもマテリアルスロットが小さい場合にも正しく処理できなければならない。
        [Test]
        public void TestAtlasTexture()
        {
            var guid = "cb122fa6652168323917132700a8f481";
            var prefabAssets = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            var prefab = UnityEngine.Object.Instantiate(prefabAssets);

            Assert.DoesNotThrow(() =>
            {
                using var tempAssetHolder = new TempAssetHolder();
                AvatarBuildUtils.ProcessAvatar(prefab, tempAssetHolder);

                var renderer = prefab.GetComponentInChildren<Renderer>();
                var atlasedTexture = renderer.sharedMaterial.mainTexture;

                UnityEngine.Object.DestroyImmediate(prefab);
            });
        }
    }

}
