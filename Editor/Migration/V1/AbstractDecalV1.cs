using System;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransTool.Decal;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V1
{
    [Obsolete]
    internal static class AbstractDecalV1
    {

        public static void MigrationAbstractDecalV1ToV2(SimpleDecal abstractDecal)
        {
            if (abstractDecal == null) { Debug.LogWarning("マイグレーションターゲットが存在しません。"); return; }
            if (abstractDecal is ITexTransToolTag TTTag && TTTag.SaveDataVersion >= 2) { Debug.Log(abstractDecal.name + " AtlasTexture : マイグレーション不可能なバージョンです。"); return; }

            var convertBlendTypeKey = abstractDecal.BlendType == TexTransCore.BlendTexture.BlendType.AlphaLerp ? TextureBlend.BL_KEY_DEFAULT : abstractDecal.BlendType.ToString();
            abstractDecal.BlendTypeKey = convertBlendTypeKey;

            EditorUtility.SetDirty(abstractDecal);
            MigrationUtility.SetSaveDataVersion(abstractDecal, 2);
        }
    }
}
