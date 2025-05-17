using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using net.rs64.TexTransTool.Build;

namespace net.rs64.TexTransTool.TestCode
{
    internal class DisableRendererHaveTargetMaterial
    {
        // ターゲットとなるマテリアルを持つレンダラーが無効な時に混じったりして失敗しないことを確認すること
        [Test]
        public void TestAtlasTexture()
        {
            var guid = "83fc80aefe3c03e779bb196d653c42b0";
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
