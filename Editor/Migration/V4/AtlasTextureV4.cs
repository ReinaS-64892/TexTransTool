using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.TextureAtlas;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V4
{
    [Obsolete]
    internal static class AtlasTextureV4
    {
        public static void MigrationAtlasTextureV4ToV5(AtlasTexture atlasTexture)
        {
            if (atlasTexture == null) { Debug.LogWarning("マイグレーションターゲットが存在しません。"); return; }
            if (atlasTexture is ITexTransToolTag TTTag && TTTag.SaveDataVersion > 5) { Debug.Log(atlasTexture.name + " AtlasTexture : マイグレーション不可能なバージョンです。"); return; }

            foreach (var tf in atlasTexture.AtlasSetting.TextureFineTuning)
            {
                switch (tf)
                {
                    default: { break; }
                    case TextureAtlas.FineTuning.ColorSpace tfs:
                        { tfs.PropertyNameList = ConvertList(tfs.PropertyNames); break; }
                    case TextureAtlas.FineTuning.Compress tfs:
                        { tfs.PropertyNameList = ConvertList(tfs.PropertyNames); break; }
                    case TextureAtlas.FineTuning.MipMapRemove tfs:
                        { tfs.PropertyNameList = ConvertList(tfs.PropertyNames); break; }
                    case TextureAtlas.FineTuning.Remove tfs:
                        { tfs.PropertyNameList = ConvertList(tfs.PropertyNames); break; }
                    case TextureAtlas.FineTuning.Resize tfs:
                        { tfs.PropertyNameList = ConvertList(tfs.PropertyNames); break; }
                    case TextureAtlas.FineTuning.ReferenceCopy rc:
                        { rc.TargetPropertyNameList = new() { rc.TargetPropertyName }; break; }
                }
            }

            EditorUtility.SetDirty(atlasTexture);
            MigrationUtility.SetSaveDataVersion(atlasTexture, 5);
        }

        static List<PropertyName> ConvertList(PropertyName propertyName)
        {
            if (propertyName.UseCustomProperty) { return propertyName.ToString().Split(' ').Select(i => new PropertyName(i)).ToList(); }
            return new() { propertyName };
        }
    }
}
