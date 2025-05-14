using System;
using System.Linq;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.IslandSelector;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V6
{
    [Obsolete]
    internal static class TextureSelectorV6
    {
        public static void MigrationTextureSelectorV6ToV7(TexTransMonoBase ttm)
        {
            if (ttm == null) { Debug.LogWarning("マイグレーションターゲットが存在しません。"); return; }


            switch (ttm)
            {
                default: break;

                case ColorDifferenceChanger cdc:
                    {
                        cdc.TargetTexture = MigrateTextureSelector(cdc.TargetTexture);
                        break;
                    }
                case TextureBlender tb:
                    {
                        tb.TargetTexture = MigrateTextureSelector(tb.TargetTexture);
                        break;
                    }
                case TextureConfigurator tc:
                    {
                        tc.TargetTexture = MigrateTextureSelector(tc.TargetTexture);
                        break;
                    }
                case MultiLayerImageCanvas mlic:
                    {
                        mlic.TargetTexture = MigrateTextureSelector(mlic.TargetTexture);
                        break;
                    }
            }


            EditorUtility.SetDirty(ttm);
            MigrationUtility.SetSaveDataVersion(ttm, 7);
        }

        internal static TextureSelector MigrateTextureSelector(TextureSelector targetTexture)
        {
            switch (targetTexture.Mode)
            {
                default: case TextureSelector.SelectMode.Absolute: { break; }
                case TextureSelector.SelectMode.Relative:
                    {
                        try
                        {
                            var mat = targetTexture.RendererAsPath?.sharedMaterials?[targetTexture.SlotAsPath];
                            if (mat == null)
                            {
                                targetTexture.SelectTexture = null;
                                break;
                            }
                            if (mat.HasTexture(targetTexture.PropertyNameAsPath))
                            {
                                targetTexture.SelectTexture = mat.GetTexture(targetTexture.PropertyNameAsPath) as Texture2D;
                            }

                        }
                        catch (Exception e)
                        {
                            targetTexture.SelectTexture = null;
                            Debug.LogException(e);
                        }
                        break;
                    }
            }
            return targetTexture;
        }
    }
}
