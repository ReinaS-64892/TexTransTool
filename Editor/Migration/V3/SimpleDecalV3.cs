using System;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.IslandSelector;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V3
{
    [Obsolete]
    internal static class SimpleDecalV3
    {

        public static void MigrationSimpleDecalV3ToV4(SimpleDecal simpleDecal)
        {
            if (simpleDecal == null) { Debug.LogWarning("マイグレーションターゲットが存在しません。"); return; }
            if (simpleDecal is ITexTransToolTag TTTag && TTTag.SaveDataVersion > 4) { Debug.Log(simpleDecal.name + " SimpleDecal : マイグレーション不可能なバージョンです。"); return; }

            if (simpleDecal.IslandCulling) { MigrateIslandCullingToIslandSelector(simpleDecal); }

            if (simpleDecal.PolygonCulling != TexTransUnityCore.Decal.PolygonCulling.Vertex) { simpleDecal.PolygonOutOfCulling = false; }

            EditorUtility.SetDirty(simpleDecal);
            MigrationUtility.SetSaveDataVersion(simpleDecal, 4);
        }


        [Obsolete]
        public static void MigrateIslandCullingToIslandSelector(SimpleDecal simpleDecal)
        {
            simpleDecal.IslandCulling = false;

            RayCastIslandSelector islandSelector = GenerateIslandSelector(simpleDecal);

            SetIslandSelectorTransform(simpleDecal, islandSelector);

        }

        public static RayCastIslandSelector GenerateIslandSelector(SimpleDecal simpleDecal)
        {
            var islandSelector = simpleDecal.IslandSelector as RayCastIslandSelector;

            if (islandSelector == null)
            {
                var go = new GameObject("RayCastIslandSelector");
                go.transform.SetParent(simpleDecal.transform, false);
                simpleDecal.IslandSelector = islandSelector = go.AddComponent<RayCastIslandSelector>();
            }

            return islandSelector;
        }

        public static void SetIslandSelectorTransform(SimpleDecal simpleDecal, RayCastIslandSelector islandSelector)
        {
            Vector3 selectorOrigin = new Vector2(simpleDecal.IslandSelectorPos.x - 0.5f, simpleDecal.IslandSelectorPos.y - 0.5f);


            var ltwMatrix = simpleDecal.transform.localToWorldMatrix;
            islandSelector.transform.position = ltwMatrix.MultiplyPoint3x4(selectorOrigin);
            islandSelector.IslandSelectorRange = simpleDecal.IslandSelectorRange;
        }
    }
}
