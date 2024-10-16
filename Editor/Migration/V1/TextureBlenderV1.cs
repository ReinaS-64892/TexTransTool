using System;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V1
{
    [Obsolete]
    internal static class TextureBlenderV1
    {

        public static void MigrationV1ToV2(TextureBlender textureBlender)
        {
            if (textureBlender == null) { Debug.LogWarning("マイグレーションターゲットが存在しません。"); return; }
            if (textureBlender is ITexTransToolTag TTTag && TTTag.SaveDataVersion > 2) { Debug.Log(textureBlender.name + " AtlasTexture : マイグレーション不可能なバージョンです。"); return; }

            var convertBlendTypeKey = textureBlender.BlendType == BlendType.AlphaLerp ? TextureBlend.BL_KEY_DEFAULT : textureBlender.BlendType.ToString();
            textureBlender.BlendTypeKey = convertBlendTypeKey;

            EditorUtility.SetDirty(textureBlender);
            MigrationUtility.SetSaveDataVersion(textureBlender, 2);
        }
    }
}
