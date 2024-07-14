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

            var maxSizeOffset = 1f;
            for (var i = 0; atlasTexture.SelectMatList.Count > i; i += 1)
            {
                var selector = atlasTexture.SelectMatList[i];
                maxSizeOffset = Mathf.Max(maxSizeOffset, selector.AdditionalTextureSizeOffSet);
            }
            for (var i = 0; atlasTexture.SelectMatList.Count > i; i += 1)
            {
                var selector = atlasTexture.SelectMatList[i];
                selector.MaterialFineTuningValue =  selector.AdditionalTextureSizeOffSet / maxSizeOffset;
                atlasTexture.SelectMatList[i] = selector;
            }

            atlasTexture.AtlasSetting.ForceSizePriority = true;

            EditorUtility.SetDirty(atlasTexture);
            MigrationUtility.SetSaveDataVersion(atlasTexture, 4);
        }
    }
}
