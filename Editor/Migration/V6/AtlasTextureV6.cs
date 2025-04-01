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
                    lnuGameObject.transform.SetParent(atlasTexture.transform.parent != null ? atlasTexture.transform.parent : atlasTexture.transform, false);
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

            if (atlasTexture.AtlasSetting.MergeMaterials)
            {
                var mergeRef = atlasTexture.AtlasSetting.MergeReferenceMaterial;
                if (mergeRef == null) { mergeRef = atlasTexture.AtlasTargetMaterials.FirstOrDefault(); }
                atlasTexture.AllMaterialMergeReference = mergeRef;
            }
            else
            {
                atlasTexture.MergeMaterialGroups = new();
                atlasTexture.AllMaterialMergeReference = null;
            }

            atlasTexture.MergeMaterialGroups = atlasTexture.AtlasSetting.MaterialMergeGroups.Select(i => new AtlasTexture.MaterialMergeGroup()
            {
                Group = i.GroupMaterials,
                Reference = i.MergeReferenceMaterial,
            }).ToList();

            var usedExperimentalOption = atlasTexture.AtlasSetting.AutoMergeTextureSetting
                || atlasTexture.AtlasSetting.AutoReferenceCopySetting
                || atlasTexture.AtlasSetting.UnsetTextures.Any()
                || atlasTexture.AtlasSetting.TextureIndividualFineTuning.Any();
            if(usedExperimentalOption)
            {
                var exp = atlasTexture.GetComponent<AtlasTextureExperimentalFeature>();
               if(exp == null) exp = atlasTexture.gameObject.AddComponent<AtlasTextureExperimentalFeature>();
               exp.AutoMergeTextureSetting = atlasTexture.AtlasSetting.AutoMergeTextureSetting;
               exp.AutoReferenceCopySetting = atlasTexture.AtlasSetting.AutoReferenceCopySetting;
               exp.UnsetTextures = atlasTexture.AtlasSetting.UnsetTextures;
               exp.TextureIndividualFineTuning = atlasTexture.AtlasSetting.TextureIndividualFineTuning;
            }

            EditorUtility.SetDirty(atlasTexture);
            MigrationUtility.SetSaveDataVersion(atlasTexture, 7);
        }
    }
}
