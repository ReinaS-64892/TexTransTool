using UnityEngine;
using System;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;
using System.Collections.Generic;
using UnityEngine.Serialization;
using net.rs64.TexTransTool.TextureAtlas.IslandRelocator;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransTool.TextureAtlas.IslandFineTuner;
using net.rs64.TexTransCoreEngineForUnity.MipMap;

namespace net.rs64.TexTransTool.TextureAtlas
{
    [Serializable]
    public class AtlasSetting
    {
        [PowerOfTwo] public int AtlasTextureSize = 2048;
        [Range(0f, 0.05f)] public float IslandPadding = 0.01f;

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
        public List<TextureIndividualTuning> TextureIndividualFineTuning;
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

    #region V2SaveData
    [Serializable]
    [Obsolete("V2SaveData", true)]
    public class TextureFineTuningData
    {
        [FormerlySerializedAs("select")] public select Select;
        public enum select
        {
            Resize,
            Compress,
            ReferenceCopy,
            Remove,
            MipMapRemove,
            ColorSpace,
        }

        //Resize
        public int Resize_Size = 512;
        public PropertyName Resize_PropertyNames = PropertyName.DefaultValue;
        public PropertySelect Resize_Select = PropertySelect.NotEqual;
        //Compress
        public FormatQuality Compress_FormatQuality = FormatQuality.High;
        public bool Compress_UseOverride = false;
        public TextureFormat Compress_OverrideTextureFormat = TextureFormat.DXT5;
        [Range(0, 100)] public int Compress_CompressionQuality = 50;
        public PropertyName Compress_PropertyNames = PropertyName.DefaultValue;
        public PropertySelect Compress_Select = PropertySelect.Equal;
        //ReferenceCopy
        public PropertyName ReferenceCopy_SourcePropertyName = PropertyName.DefaultValue;
        public PropertyName ReferenceCopy_TargetPropertyName = PropertyName.DefaultValue;
        //Remove
        public PropertyName Remove_PropertyNames = PropertyName.DefaultValue;
        public PropertySelect Remove_Select = PropertySelect.NotEqual;
        //MipMapRemove
        public PropertyName MipMapRemove_PropertyNames = PropertyName.DefaultValue;
        public PropertySelect MipMapRemove_Select = PropertySelect.Equal;

        //ColorSpace
        public PropertyName ColorSpace_PropertyNames = PropertyName.DefaultValue;
        public PropertySelect ColorSpace_Select = PropertySelect.Equal;
        public bool ColorSpace_Linear = false;

        internal ITextureFineTuning GetFineTuning()
        {
            switch (Select)
            {
                case select.Resize:
                    return new Resize(Resize_Size, Resize_PropertyNames, Resize_Select);
                case select.Compress:
                    return new Compress(Compress_FormatQuality, Compress_UseOverride, Compress_OverrideTextureFormat, Compress_CompressionQuality, Compress_PropertyNames, Compress_Select);
                case select.ReferenceCopy:
                    return new ReferenceCopy(ReferenceCopy_SourcePropertyName, new() { ReferenceCopy_TargetPropertyName });
                case select.Remove:
                    return new Remove(Remove_PropertyNames, Remove_Select);
                case select.MipMapRemove:
                    return new MipMapRemove(MipMapRemove_PropertyNames, MipMapRemove_Select);
                case select.ColorSpace:
                    return new FineTuning.ColorSpace(ColorSpace_PropertyNames, ColorSpace_Select, ColorSpace_Linear);

                default:
                    return null;
            }

        }
    }
    #endregion

}
