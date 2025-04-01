using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.TextureAtlas;
using net.rs64.TexTransTool.TextureAtlas.IslandSizePriorityTuner;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V6
{
    [Obsolete]
    internal static class AtlasTextureV6
    {
        public static void MigrationAtlasTextureV6ToV7(AtlasTexture atlasTexture)
        {
            if (atlasTexture == null) { Debug.LogWarning("マイグレーションターゲットが存在しません。"); return; }
            if (atlasTexture is ITexTransToolTag TTTag && TTTag.SaveDataVersion > 7) { Debug.Log(atlasTexture.name + " AtlasTexture : マイグレーション不可能なバージョンです。"); return; }


            atlasTexture.AtlasTargetMaterials = atlasTexture.SelectMatList.Select(i => i.Material).ToList();
            atlasTexture.IslandSizePriorityTuner = atlasTexture.SelectMatList.Select(i =>
                new SetFromMaterial()
                {
                    Materials = new() { i.Material },
                    PriorityValue = i.MaterialFineTuningValue
                }
            ).ToList<IIslandSizePriorityTuner>();

#if CONTAINS_LNU
            if (atlasTexture.AtlasSetting.PropertyBakeSetting == PropertyBakeSetting.NotBake)
            {
                if (atlasTexture.MigrationTemporarylilToonMaterialNormalizerReference != null)
                {
                    atlasTexture.MigrationTemporarylilToonMaterialNormalizerReference.enabled = false;
                }
            }
            else
            {
                if (atlasTexture.MigrationTemporarylilToonMaterialNormalizerReference == null)
                {
                    var lnuGameObject = new GameObject("lilToonMaterialNormalizer (generate from AtlasTexture migration)");
                    lnuGameObject.transform.SetParent(atlasTexture.transform.parent, false);
                    lnuGameObject.transform.SetSiblingIndex(atlasTexture.transform.GetSiblingIndex());
                    if (lilToonNDMFUtility.lilToonMaterialNormalizerPublicAPI.TryAddComponent(lnuGameObject, out var lnuMn))
                        atlasTexture.MigrationTemporarylilToonMaterialNormalizerReference = lnuMn as Behaviour;
                }

                if (atlasTexture.MigrationTemporarylilToonMaterialNormalizerReference != null)
                {
                    var lnuMN = atlasTexture.MigrationTemporarylilToonMaterialNormalizerReference;
                    lnuMN.enabled = true;

                    lilToonNDMFUtility.lilToonMaterialNormalizerPublicAPI.TrySetTargetMaterials(lnuMN, atlasTexture.AtlasTargetMaterials);
                    if (atlasTexture.AtlasSetting.PropertyBakeSetting is PropertyBakeSetting.Bake)
                        lilToonNDMFUtility.lilToonMaterialNormalizerPublicAPI.TrySetTargetDetectionOfTextureContains(lnuMN);
                    else
                        lilToonNDMFUtility.lilToonMaterialNormalizerPublicAPI.TrySetTargetDetectionOfAll(lnuMN);
                }
            }
#endif




            EditorUtility.SetDirty(atlasTexture);
            MigrationUtility.SetSaveDataVersion(atlasTexture, 7);
        }
    }
}
