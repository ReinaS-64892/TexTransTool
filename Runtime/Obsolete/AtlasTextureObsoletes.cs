using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace net.rs64.TexTransTool.TextureAtlas
{
    public sealed partial class AtlasTexture
    {

        #region V6SaveData
        [Obsolete("V6SaveData", true)][SerializeField][FormerlySerializedAs("TargetRoot")] internal GameObject LimitCandidateMaterials;

        [Obsolete("V6SaveData", true)][SerializeField] internal List<MatSelector> SelectMatList = new List<MatSelector>();
        [Serializable]
        [Obsolete("V6SaveData", true)]
        internal struct MatSelector
        {
            public Material Material;
            public float MaterialFineTuningValue;
        }
        [Obsolete("V6SaveData", true)][SerializeField] internal Behaviour MigrationTemporarylilToonMaterialNormalizerReference = null;
        #endregion

    }
    public partial class AtlasSetting
    {

        #region V6SaveData
        [Obsolete("V6SaveData", true)][SerializeField] internal List<TextureSelector> UnsetTextures = new();
        [Obsolete("V6SaveData", true)][SerializeField] internal List<TextureIndividualTuning> TextureIndividualFineTuning = new();
        [Obsolete("V6SaveData", true)][SerializeField] internal bool AutoReferenceCopySetting = false;
        [Obsolete("V6SaveData", true)][SerializeField] internal bool AutoMergeTextureSetting = false;

        [Obsolete("V6SaveData", true)][SerializeReference] internal List<IslandFineTuner.IIslandFineTuner> IslandFineTuners = new();
        [Obsolete("V6SaveData", true)][SerializeField] internal PropertyBakeSetting PropertyBakeSetting = PropertyBakeSetting.NotBake;

        [Range(0, 7)][Obsolete("V6SaveData", true)][SerializeField] internal UVCopy MigrationTemporaryUVCopyReference = null;
        [Obsolete("V6SaveData", true)][SerializeField] internal bool WriteOriginalUV = false;
        [Range(0, 7)][Obsolete("V6SaveData", true)][SerializeField] internal int OriginalUVWriteTargetChannel = 1;
        [Obsolete("V6SaveData", true)][SerializeField] internal bool TextureScaleOffsetReset = false;
        [Obsolete("V6SaveData", true)][SerializeField] internal bool BakedPropertyWriteMaxValue = false;
        [Obsolete("V6SaveData", true)][SerializeField][PowerOfTwo] internal int HeightDenominator = 1;
        [Obsolete("V6SaveData", true)][SerializeField] internal bool MergeMaterials = false;
        [Obsolete("V6SaveData", true)][SerializeField] internal Material MergeReferenceMaterial = null;
        [Obsolete("V6SaveData", true)][SerializeField][FormerlySerializedAs("MaterialMargeGroups")] internal List<MaterialMergeGroup> MaterialMergeGroups = new();

        [Serializable]
        [Obsolete("V6SaveData", true)]
        public class MaterialMergeGroup
        {
            [FormerlySerializedAs("MargeReferenceMaterial")] public Material MergeReferenceMaterial;
            public List<Material> GroupMaterials;
        }
        #endregion
    }

}
