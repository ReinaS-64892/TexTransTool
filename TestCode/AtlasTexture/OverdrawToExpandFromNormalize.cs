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
    internal class OverdrawToExpandFromNormalize
    {
        // Normalize が発生しうる Mesh の状況下で、マテリアルスロットが少ない場合、SubMesh数を減らす必要はないため維持される必要がある。
        [Test]
        public void TestAtlasTexture()
        {
            var guid = "74316836a1984719ca1c56df23d81097";
            var prefabAssets = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            var prefab = UnityEngine.Object.Instantiate(prefabAssets);

            using var tempAssetHolder = new TempAssetHolder();
            AvatarBuildUtils.ProcessAvatar(prefab, tempAssetHolder);

            var renderers = prefab.GetComponentsInChildren<Renderer>();
            Assert.That(renderers.Select(r => r.GetMesh()).Distinct().First().subMeshCount == 4);

            UnityEngine.Object.DestroyImmediate(prefab);
        }
    }

}
