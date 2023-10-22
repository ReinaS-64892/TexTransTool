#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using net.rs64.TexTransTool.TextureAtlas;
using net.rs64.TexTransTool.Utils;
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
            if (atlasTexture.SaveDataVersion > 2) { Debug.Log(atlasTexture.name + " AtlasTexture : マイグレーション不可能なバージョンです。"); return; }


            var maxTexturePixelCount = 0;
            foreach (var matSelect in atlasTexture.SelectMatList)
            {
                var tex = matSelect.Material.mainTexture;
                if (tex == null) { continue; }
                maxTexturePixelCount = Mathf.Max(maxTexturePixelCount, tex.width * tex.height);
            }


            for (int i = 0; i < atlasTexture.SelectMatList.Count; i += 1)
            {
                var selector = atlasTexture.SelectMatList[i];
                var tex = selector.Material.mainTexture;
                if (tex == null) { continue; }
                var texSize = tex.width * tex.height;
                var defaultOffset = texSize / maxTexturePixelCount;
                var offset = selector.TextureSizeOffSet;

                var additionalTextureSizeOffSet = offset / defaultOffset;


                selector.AdditionalTextureSizeOffSet = additionalTextureSizeOffSet;
                atlasTexture.SelectMatList[i] = selector;
            }




            MigrationUtility.SetSaveDataVersion(atlasTexture, 2);
        }
    }
}
#endif