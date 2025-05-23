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
    internal class PolygonLessSubMesh
    {
        // サブメッシュにポリゴンを持たないケースがあっても例外を吐かず、回避しなければならない。
        [Test]
        public void TestAtlasTexture()
        {
            var guid = "ad578bf23091e87b5be125710d1b9382";
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
