#if UNITY_EDITOR
using System;
using System.Collections.Generic;
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
        public static void MigrationAtlasTextureV0(AtlasTexture atlasTexture, bool DestroyNow)
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
                if (atlasTexture.ObsoleteChannelsRef.Any())
                {
                    for (int Count = 0; atlasTexture.AtlasSettings.Count > Count; Count += 1)
                    {
                        var newAtlasTexture = atlasTexture.ObsoleteChannelsRef[Count];
                        CopySetting(atlasTexture, Count, newAtlasTexture);
                        EditorUtility.SetDirty(newAtlasTexture);
                    }
                }
                else
                {

                    var texTransParentGroup = GameObject.AddComponent<TexTransParentGroup>();
                    atlasTexture.ObsoleteChannelsRef = new List<AtlasTexture>() { };

                    for (int Count = 0; atlasTexture.AtlasSettings.Count > Count; Count += 1)
                    {
                        var newGameObject = new GameObject("Channel " + Count);
                        newGameObject.transform.SetParent(GameObject.transform);

                        var newAtlasTexture = newGameObject.AddComponent<net.rs64.TexTransTool.TextureAtlas.AtlasTexture>();
                        CopySetting(atlasTexture, Count, newAtlasTexture);
                        atlasTexture.ObsoleteChannelsRef.Add(newAtlasTexture);
                        EditorUtility.SetDirty(newAtlasTexture);
                    }

                }

                if (DestroyNow) { UnityEngine.Object.DestroyImmediate(atlasTexture); }
            }
        }

        private static void CopySetting(AtlasTexture atlasTextureSouse, int atlasSettingIndex, AtlasTexture NewAtlasTextureTarget)
        {
            NewAtlasTextureTarget.TargetRoot = atlasTextureSouse.TargetRoot;
            NewAtlasTextureTarget.AtlasSetting = atlasTextureSouse.AtlasSettings[atlasSettingIndex];
            NewAtlasTextureTarget.AtlasSetting.UseIslandCache = atlasTextureSouse.UseIslandCache;
            NewAtlasTextureTarget.SelectMatList = atlasTextureSouse.MatSelectors
            .Where(I => I.IsTarget && I.AtlasChannel == atlasSettingIndex)
            .Select(I => new TexTransTool.TextureAtlas.AtlasTexture.MatSelector()
            {
                Material = I.Material,
                TextureSizeOffSet = I.TextureSizeOffSet
            }).ToList();
            EditorUtility.SetDirty(NewAtlasTextureTarget);
            if (atlasTextureSouse == NewAtlasTextureTarget)
            {
                var sObj = new SerializedObject(NewAtlasTextureTarget);
                var saveDataProp = sObj.FindProperty("_saveDataVersion");
                saveDataProp.intValue = 1;
                sObj.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
#endif