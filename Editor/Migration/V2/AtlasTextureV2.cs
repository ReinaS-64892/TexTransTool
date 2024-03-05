using System;
using System.Linq;
using net.rs64.TexTransTool.TextureAtlas;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V2
{
    [Obsolete]
    internal static class AtlasTextureV2
    {
        public static void MigrationAtlasTextureV2ToV3(AtlasTexture atlasTexture)
        {
            if (atlasTexture == null) { Debug.LogWarning("マイグレーションターゲットが存在しません。"); return; }
            if (atlasTexture is ITexTransToolTag TTTag && TTTag.SaveDataVersion > 3) { Debug.Log(atlasTexture.name + " AtlasTexture : マイグレーション不可能なバージョンです。"); return; }

            atlasTexture.AtlasSetting.IslandPadding = atlasTexture.AtlasSetting.Padding / atlasTexture.AtlasSetting.AtlasTextureSize;
            atlasTexture.AtlasSetting.TextureFineTuning = atlasTexture.AtlasSetting.TextureFineTuningDataList.Select(i => i.GetFineTuning()).ToList();

            EditorUtility.SetDirty(atlasTexture);
            MigrationUtility.SetSaveDataVersion(atlasTexture, 3);
        }
    }
}
