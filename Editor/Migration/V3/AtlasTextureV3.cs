using System;
using System.Linq;
using net.rs64.TexTransTool.TextureAtlas;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V3
{
    [Obsolete]
    internal static class AtlasTextureV3
    {
        public static void MigrationAtlasTextureV3ToV4(AtlasTexture atlasTexture)
        {
            if (atlasTexture == null) { Debug.LogWarning("マイグレーションターゲットが存在しません。"); return; }
            if (atlasTexture is ITexTransToolTag TTTag && TTTag.SaveDataVersion > 4) { Debug.Log(atlasTexture.name + " AtlasTexture : マイグレーション不可能なバージョンです。"); return; }

            atlasTexture.AtlasSetting.ForceSizePriority = true;

            EditorUtility.SetDirty(atlasTexture);
            MigrationUtility.SetSaveDataVersion(atlasTexture, 4);
        }
    }
}
