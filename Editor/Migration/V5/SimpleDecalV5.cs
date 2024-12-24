using System;
using System.Linq;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.IslandSelector;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V5
{
    [Obsolete]
    internal static class SimpleDecalV5
    {
        public static void MigrationSimpleDecalV5ToV6(SimpleDecal simpleDecal)
        {
            if (simpleDecal == null) { Debug.LogWarning("マイグレーションターゲットが存在しません。"); return; }

            simpleDecal.RendererSelector.Mode = RendererSelectMode.Manual;
            simpleDecal.RendererSelector.ManualSelections = simpleDecal.TargetRenderers.ToList();

            EditorUtility.SetDirty(simpleDecal);
            MigrationUtility.SetSaveDataVersion(simpleDecal, 6);
        }
    }
}
