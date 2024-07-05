using System;
using net.rs64.TexTransTool.TextureAtlas;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V1
{
    [Obsolete]
    internal static class AtlasTextureV1
    {
        public static void MigrationAtlasTextureV1ToV2(AtlasTexture atlasTexture)
        {
            if (atlasTexture == null) { Debug.LogWarning("マイグレーションターゲットが存在しません。"); return; }
            if (atlasTexture is ITexTransToolTag TTTag && TTTag.SaveDataVersion >= 2) { Debug.Log(atlasTexture.name + " AtlasTexture : マイグレーション不可能なバージョンです。"); return; }


            var maxTexturePixelCount = 0;
            foreach (var matSelect in atlasTexture.SelectMatList)
            {
                if (matSelect.Material == null) { continue; }
                var tex = matSelect.Material.mainTexture;
                if (tex == null) { continue; }
                maxTexturePixelCount = Mathf.Max(maxTexturePixelCount, tex.width * tex.height);
            }

            var matList = atlasTexture.SelectMatList;
            for (int i = 0; i < matList.Count; i += 1)
            {
                var selector = matList[i];
                if (selector.Material == null) { continue; }
                var tex = selector.Material.mainTexture;
                if (tex == null) { continue; }
                var texSize = tex.width * tex.height;
                var defaultOffset = (float)texSize / maxTexturePixelCount;
                var offset = selector.TextureSizeOffSet;

                var additionalTextureSizeOffSet = offset / defaultOffset;

                // Debug.Log($"texSize{texSize}/{maxTexturePixelCount}:Offset{offset}:Default{defaultOffset}:result{additionalTextureSizeOffSet}");


                selector.TextureSizeOffSet = additionalTextureSizeOffSet;
                matList[i] = selector;
            }

            atlasTexture.SelectMatList = matList;

            atlasTexture.AtlasSetting.WriteOriginalUV = true;


            EditorUtility.SetDirty(atlasTexture);


            MigrationUtility.SetSaveDataVersion(atlasTexture, 2);
        }
    }
}
