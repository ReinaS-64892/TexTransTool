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
    [Obsolete]
    internal static class AtlasTextureV0
    {
        public static void MigrationAtlasTextureV0ToV1(AtlasTexture atlasTexture, bool DestroyNow)
        {
            if (atlasTexture == null) { Debug.LogWarning("マイグレーションターゲットが存在しません。"); return; }
            if (atlasTexture.SaveDataVersion != 0) { Debug.LogWarning("マイグレーションのバージョンが違います。"); return; }
            if (atlasTexture.AtlasSettings.Count < 1) { Debug.LogWarning("マイグレーション不可能なアトラステクスチャーです。"); return; }

            var GameObject = atlasTexture.gameObject;

            if (atlasTexture.AtlasSettings.Count == 1)
            {
                MigrateSettingV0ToV1(atlasTexture, 0, atlasTexture);
            }
            else
            {
                if (atlasTexture.ObsoleteChannelsRef.Any())
                {
                    for (int Count = 0; atlasTexture.AtlasSettings.Count > Count; Count += 1)
                    {
                        var newAtlasTexture = atlasTexture.ObsoleteChannelsRef[Count];
                        MigrateSettingV0ToV1(atlasTexture, Count, newAtlasTexture);
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
                        newGameObject.transform.parent = GameObject.transform;

                        var newAtlasTexture = newGameObject.AddComponent<net.rs64.TexTransTool.TextureAtlas.AtlasTexture>();
                        MigrateSettingV0ToV1(atlasTexture, Count, newAtlasTexture);
                        atlasTexture.ObsoleteChannelsRef.Add(newAtlasTexture);
                        EditorUtility.SetDirty(newAtlasTexture);
                    }

                }

                if (DestroyNow) { UnityEngine.Object.DestroyImmediate(atlasTexture); }
            }
        }

        private static void MigrateSettingV0ToV1(AtlasTexture atlasTextureSouse, int atlasSettingIndex, AtlasTexture NewAtlasTextureTarget)
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
                MigrationUtility.SetSaveDataVersion(NewAtlasTextureTarget, 1);
            }
        }
    }
}
#endif