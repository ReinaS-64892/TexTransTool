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
    internal class SomeMeshDifferentAtlasSubSet
    {
        // 同一 Mesh だが、マテリアルの違いにより AtlasSubSet が異なる場合に、ターゲットになっていないものが正しく UV の操作が行われていないことをテストする。
        // ターゲットではない Mesh の UV 座標一覧を持っておき、処理後に比較することで確認する。
        [Test]
        public void TestAtlasTexture()
        {
            var guid = "9e69fbe6b7fb9a231907409747b62c2c";
            var prefabAssets = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            var prefab = UnityEngine.Object.Instantiate(prefabAssets);

            var renderer = prefab.GetComponentsInChildren<Renderer>().First(i => i.sharedMaterials.Distinct().Count() == 2);


            var mesh = renderer.GetMesh(); var uv = mesh.uv;
            var notMoveUVPositions = mesh.GetTriangles(0).Select(i => uv[i]).ToArray();

            using var tempAssetHolder = new TempAssetHolder();
            AvatarBuildUtils.ProcessAvatar(prefab, tempAssetHolder);

            var atlasedMesh = renderer.GetMesh();
            var atlasedUV = atlasedMesh.uv;
            var atlasedNotUVPositions = atlasedMesh.GetTriangles(0).Select(i => atlasedUV[i]).ToArray();

            Assert.That(notMoveUVPositions.Length == notMoveUVPositions.Length);
            for (var i = 0; notMoveUVPositions.Length > i; i += 1)
            {
                Assert.That(notMoveUVPositions[i] == notMoveUVPositions[i]);
            }

            UnityEngine.Object.DestroyImmediate(prefab);
        }
    }

}
