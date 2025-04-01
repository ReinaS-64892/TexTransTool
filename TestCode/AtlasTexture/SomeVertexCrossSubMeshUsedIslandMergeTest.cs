using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using net.rs64.TexTransTool.Build;

namespace net.rs64.TexTransTool.TestCode
{
    internal class SomeVertexCrossSubMeshUsedIslandMergeTest
    {
        // サブメッシュが違うけど MaterialGroupID が同一な SubMesh が複数あるときに、同一UV座標を持つ Island 同士をマージする必要があるためそれが正しく行われているかを見る
        // マージされていない場合にテクスチャサイズが 正方形になるように調整したモデルを用意し、正しく動作しているなら正方形以外の横長になるため、それを確認することで Test する。
        [Test]
        public void TestAtlasTexture()
        {
            var guid = "d0665c13aaf415ebc9c38d9e32ffb65f";
            // var guid = "d6c79a8df205835659799c0c12355359"; // 確定で失敗する Prefab
            var prefabAssets = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            var prefab = UnityEngine.Object.Instantiate(prefabAssets);

            using var tempAssetHolder = new TempAssetHolder();
            AvatarBuildUtils.ProcessAvatar(prefab, tempAssetHolder);

            var renderer = prefab.GetComponentInChildren<Renderer>();
            var atlasedTexture = renderer.sharedMaterial.mainTexture;

            Assert.That(atlasedTexture.height < atlasedTexture.width);
            UnityEngine.Object.DestroyImmediate(prefab);
        }
    }

}
