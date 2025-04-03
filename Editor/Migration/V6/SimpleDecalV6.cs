using System;
using System.Linq;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.IslandSelector;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V6
{
    [Obsolete]
    internal static class SimpleDecalV6
    {
        public static void MigrationSimpleDecalV6ToV7(SimpleDecal simpleDecal)
        {
            if (simpleDecal == null) { Debug.LogWarning("マイグレーションターゲットが存在しません。"); return; }

            var usedExperimentalOption = simpleDecal.OverrideDecalTextureWithMultiLayerImageCanvas != null || simpleDecal.UseDepth;

            if (simpleDecal.MigrationTemporaryExperimentalFeature == null || usedExperimentalOption)
                simpleDecal.MigrationTemporaryExperimentalFeature = simpleDecal.gameObject.AddComponent<SimpleDecalExperimentalFeature>();
            if (simpleDecal.MigrationTemporaryExperimentalFeature != null)
            {
                var exp = simpleDecal.MigrationTemporaryExperimentalFeature;

                exp.UseDepth = simpleDecal.UseDepth;
                exp.DepthInvert = simpleDecal.DepthInvert;

                exp.OverrideDecalTextureWithMultiLayerImageCanvas = simpleDecal.OverrideDecalTextureWithMultiLayerImageCanvas;
                EditorUtility.SetDirty(exp);
            }

            EditorUtility.SetDirty(simpleDecal);
            MigrationUtility.SetSaveDataVersion(simpleDecal, 7);
        }
    }
}
