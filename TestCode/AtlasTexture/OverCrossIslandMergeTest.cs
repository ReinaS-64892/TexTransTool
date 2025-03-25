using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using net.rs64.TexTransTool.Build;

namespace net.rs64.TexTransTool.TestCode
{
    internal class OverCrossIslandMergeTest
    {
        // MaterialGroupID が同一な Island が大幅に重なっているとき(結合分で増える量より結合分で減る量が大きい場合)、にマージされる必要がある。
        // マージされていない場合にテクスチャサイズが 正方形になるように調整したモデルを用意し、正しく動作しているなら正方形以外の横長になるため、それを確認することで Test する。
        [Test]
        public void TestAtlasTexture()
        {
            var guid = "ee2de6a28b1e8bb45921f0a25f507da1";
            // var guid = "96c9e4fc411af126c81bc29fc8f11e62"; // 確定で失敗する Prefab
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
