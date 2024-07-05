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

        [FormerlySerializedAs("IncludeDisableRenderer")] public bool IncludeDisabledRenderer = false;
        public bool ForceSizePriority;
        [SerializeReference] internal List<IIslandFineTuner> IslandFineTuners;

        public bool MergeMaterials;
        public Material MergeReferenceMaterial;
        public PropertyBakeSetting PropertyBakeSetting = PropertyBakeSetting.NotBake;
        public bool ForceSetTexture;

        [FormerlySerializedAs("MaterialMargeGroups")] public List<MaterialMergeGroup> MaterialMergeGroups;

        public AtlasIslandRelocatorObject AtlasIslandRelocator;
        public bool WriteOriginalUV = false;
        public bool PixelNormalize = false;

        [SerializeReference,SubclassSelector] public List<ITextureFineTuning> TextureFineTuning = new List<ITextureFineTuning> { new Resize() };
        public float GetTexScalePadding => IslandPadding * AtlasTextureSize;

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
                    return new ReferenceCopy(ReferenceCopy_SourcePropertyName, ReferenceCopy_TargetPropertyName);
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
