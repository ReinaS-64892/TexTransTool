using System;
using System.Linq;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.IslandSelector;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V5
{
    [Obsolete]
    internal static class SingleGradationDecalV5
    {
        public static void MigrationSingleGradationDecalV5ToV6(SingleGradationDecal singleGradationDecal)
        {
            if (singleGradationDecal == null) { Debug.LogWarning("マイグレーションターゲットが存在しません。"); return; }

            singleGradationDecal.RendererSelector.Mode = RendererSelectMode.Auto;
            singleGradationDecal.RendererSelector.UseMaterialFilteringForAutoSelect = true;
            singleGradationDecal.RendererSelector.IsAutoIncludingDisableRenderers = true;
            singleGradationDecal.RendererSelector.AutoSelectFilterMaterials = singleGradationDecal.TargetMaterials.ToList();

            EditorUtility.SetDirty(singleGradationDecal);
            MigrationUtility.SetSaveDataVersion(singleGradationDecal, 6);
        }
    }
}
