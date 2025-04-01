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
    internal class SomeMeshSomeAtlasSubSet
    {
        // 同一 Mesh 、同一マテリアルセットのときに、正しく同一の Mesh が割り当てられているかをテストする
        // Mesh 同士の参照比較でチェックできる。
        [Test]
        public void TestAtlasTexture()
        {
            var guid = "0d296c332091dae19b1d54de5bde3e61";
            var prefabAssets = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            var prefab = UnityEngine.Object.Instantiate(prefabAssets);


            using var tempAssetHolder = new TempAssetHolder();
            AvatarBuildUtils.ProcessAvatar(prefab, tempAssetHolder);

            var renderers = prefab.GetComponentsInChildren<Renderer>();
            Assert.That(renderers.Select(r => r.GetMesh()).Distinct().Count() == 1);

            UnityEngine.Object.DestroyImmediate(prefab);
        }
    }

}
