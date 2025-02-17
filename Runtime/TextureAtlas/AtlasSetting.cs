using UnityEngine;
using System;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;
using System.Collections.Generic;
using UnityEngine.Serialization;
using net.rs64.TexTransTool.TextureAtlas.IslandRelocator;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransTool.TextureAtlas.IslandFineTuner;

namespace net.rs64.TexTransTool.TextureAtlas
{
    [Serializable]
    public class AtlasSetting
    {
        [PowerOfTwo] public int AtlasTextureSize = 2048;
        [Range(0f, 0.05f)] public float IslandPadding = 0.01f;
        [PowerOfTwo] public int HeightDenominator = 1;

        [FormerlySerializedAs("IncludeDisableRenderer")] public bool IncludeDisabledRenderer = false;
        public bool ForceSizePriority = false;
        [SerializeReference] internal List<IIslandFineTuner> IslandFineTuners = new();

        public bool MergeMaterials = false;
        public Material MergeReferenceMaterial = null;
        public PropertyBakeSetting PropertyBakeSetting = PropertyBakeSetting.NotBake;
        public bool ForceSetTexture = false;
        public bool PixelNormalize = true;

        [FormerlySerializedAs("MaterialMargeGroups")] public List<MaterialMergeGroup> MaterialMergeGroups = new();
        public List<(Material, Material)> MaterialReplacedReference = new();

        public AtlasIslandRelocatorObject AtlasIslandRelocator = null;
        public bool WriteOriginalUV = false;
        [Range(1, 7)] public int OriginalUVWriteTargetChannel = 1;
        public Color BackGroundColor = Color.white;
        [FormerlySerializedAs("DownScalingAlgorism")] public DownScalingAlgorithm DownScalingAlgorithm = DownScalingAlgorithm.Average;
        [SerializeReference, SubclassSelector] public List<ITextureFineTuning> TextureFineTuning = new List<ITextureFineTuning> { new Resize() };
        public List<TextureIndividualTuning> TextureIndividualFineTuning = new();
        public bool AutoReferenceCopySetting = false;
        public bool AutoMergeTextureSetting = false;
        public float GetTexScalePadding => IslandPadding * AtlasTextureSize;
        public bool TextureScaleOffsetReset = false;
        public bool BakedPropertyWriteMaxValue = false;
        public List<TextureSelector> UnsetTextures = new();

        #region V3SaveData
        public bool UseUpScaling = false;
        #endregion
        #region V2SaveData
        [Obsolete("V2SaveData", true)][SerializeField] internal List<TextureFineTuningData> TextureFineTuningDataList = new List<TextureFineTuningData> { new TextureFineTuningData() };
        [Obsolete("V2SaveData", true)][SerializeField] internal float Padding;
        #endregion
        #region V1SaveData
        [Obsolete("V1SaveData", true)][SerializeField] internal bool UseIslandCache = true;
        #endregion

    }

    [Serializable]
    public class MaterialMergeGroup
    {
        [FormerlySerializedAs("MargeReferenceMaterial")] public Material MergeReferenceMaterial;
        public List<Material> GroupMaterials;
    }
    public enum PropertyBakeSetting
    {
        NotBake,
        Bake,
        BakeAllProperty,
    }
    [Serializable]
    public class TextureIndividualTuning
    {
        public string TuningTarget;

        [FormerlySerializedAs("OverrideAsMargeTexture")] public bool OverrideReferenceCopy = false;
        public string CopyReferenceSource = PropertyName.DefaultValue;

        public bool OverrideResize = false;
        [PowerOfTwo] public int TextureSize = 512;

        public bool OverrideCompression = false;
        public TextureCompressionData CompressionData = new();

        public bool OverrideMipMapRemove = false;
        public bool UseMipMap = true;

        public bool OverrideColorSpace = false;
        public bool Linear = false;

        public bool OverrideRemove = false;
        public bool IsRemove = false;

        [FormerlySerializedAs("OverrideAsMargeTexture")] public bool OverrideMargeTexture = false;
        public string MargeRootProperty;
    }

}
