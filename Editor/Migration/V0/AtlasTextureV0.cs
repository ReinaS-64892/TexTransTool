#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using net.rs64.TexTransTool.TextureAtlas;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V0
{
    internal static class AtlasTextureV0
    {
        public static void MigrationAtlasTextureV0(AtlasTexture atlasTexture)
        {
            if (atlasTexture == null) { Debug.LogWarning("マイグレーションターゲットが存在しません。"); return; }
            if (atlasTexture.SaveDataVersion != 0) { Debug.LogWarning("マイグレーションのバージョンが違います。"); return; }
            if (atlasTexture.AtlasSettings.Count < 1) { Debug.LogWarning("マイグレーション不可能なアトラステクスチャーです。"); return; }

            var GameObject = atlasTexture.gameObject;

            if (atlasTexture.AtlasSettings.Count == 1)
            {
                CopySetting(atlasTexture, 0, atlasTexture);
            }
            else
            {
                var texTransParentGroup = GameObject.AddComponent<TexTransParentGroup>();

                for (int Count = 0; atlasTexture.AtlasSettings.Count > Count; Count += 1)
                {
                    var newGameObject = new GameObject("Channel " + Count);
                    newGameObject.transform.SetParent(GameObject.transform);

                    var newAtlasTexture = newGameObject.AddComponent<net.rs64.TexTransTool.TextureAtlas.AtlasTexture>();
                    CopySetting(atlasTexture, Count, newAtlasTexture);
                    EditorUtility.SetDirty(newAtlasTexture);
                }

                UnityEngine.Object.DestroyImmediate(atlasTexture);
            }

        }

        private static void CopySetting(AtlasTexture atlasTexture, int atlasSettingIndex, AtlasTexture newAtlasTexture)
        {
            newAtlasTexture.TargetRoot = atlasTexture.TargetRoot;
            newAtlasTexture.AtlasSetting = atlasTexture.AtlasSettings[atlasSettingIndex];
            newAtlasTexture.AtlasSetting.UseIslandCache = atlasTexture.UseIslandCache;
            newAtlasTexture.SelectMatList = atlasTexture.MatSelectors
            .Where(I => I.IsTarget && I.AtlasChannel == atlasSettingIndex)
            .Select(I => new TexTransTool.TextureAtlas.AtlasTexture.MatSelector()
            {
                Material = I.Material,
                TextureSizeOffSet = I.TextureSizeOffSet
            }).ToList();
            EditorUtility.SetDirty(newAtlasTexture);
            if (atlasTexture == newAtlasTexture)
            {
                var sObj = new SerializedObject(newAtlasTexture);
                var saveDataProp = sObj.FindProperty("_saveDataVersion");
                saveDataProp.intValue = 1;
                sObj.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
#endif