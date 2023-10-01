#if UNITY_EDITOR
using UnityEngine;
using System;
using net.rs64.TexTransTool.EditorIsland;
using net.rs64.TexTransTool.TextureAtlas.FineSetting;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Serialization;
using net.rs64.TexTransCore.Island;

namespace net.rs64.TexTransTool.TextureAtlas
{
    [Serializable]
    public class AtlasSetting
    {
        public bool MergeMaterials;
        public Material MergeReferenceMaterial;
        public PropertyBakeSetting PropertyBakeSetting = PropertyBakeSetting.NotBake;
        public bool ForceSetTexture;
        public int AtlasTextureSize = 2048;
        public float Padding = 10;
        public bool UseIslandCache = true;
        public IslandSorting.IslandSortingType SortingType = IslandSorting.IslandSortingType.NextFitDecreasingHeightPlusFloorCeiling;
        public List<TextureFineTuningData> TextureFineTuningDataList = new List<TextureFineTuningData> {
            new TextureFineTuningData()
            {
                Select = TextureFineTuningData.select.Resize,
                Resize_Size = 512,
                Resize_PropertyNames = new PropertyName("_MainTex"),
                Resize_Select = PropertySelect.NotEqual,
            }
        };
        public float GetTexScalePadding => Padding / AtlasTextureSize;

        public List<IAddFineTuning> GetTextureFineTuning()
        {
            var IFineSettings = new List<IAddFineTuning>();
            foreach (var fineSetting in TextureFineTuningDataList)
            {
                IFineSettings.Add(fineSetting.GetFineSetting());
            }
            return IFineSettings;
        }

    }
    public enum PropertyBakeSetting
    {
        NotBake,
        Bake,
        BakeAllProperty,
    }
    [Serializable]
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
        }

        //Resize
        public int Resize_Size = 512;
        public PropertyName Resize_PropertyNames;
        public PropertySelect Resize_Select = PropertySelect.NotEqual;
        //Compress
        public Compress.FormatQuality Compress_FormatQuality = Compress.FormatQuality.High;
        public TextureCompressionQuality Compress_CompressionQuality = TextureCompressionQuality.Best;
        public PropertyName Compress_PropertyNames;
        public PropertySelect Compress_Select = PropertySelect.Equal;
        //ReferenceCopy
        public PropertyName ReferenceCopy_SourcePropertyName;
        public PropertyName ReferenceCopy_TargetPropertyName;
        //Remove
        public PropertyName Remove_PropertyNames;
        public PropertySelect Remove_Select = PropertySelect.NotEqual;
        //MipMapRemove
        public PropertyName MipMapRemove_PropertyNames;
        public PropertySelect MipMapRemove_Select = PropertySelect.Equal;

        public IAddFineTuning GetFineSetting()
        {
            switch (Select)
            {
                case select.Resize:
                    return new Resize(Resize_Size, Resize_PropertyNames, Resize_Select);
                case select.Compress:
                    return new Compress(Compress_FormatQuality, Compress_CompressionQuality, Compress_PropertyNames, Compress_Select);
                case select.ReferenceCopy:
                    return new ReferenceCopy(ReferenceCopy_SourcePropertyName, ReferenceCopy_TargetPropertyName);
                case select.Remove:
                    return new Remove(Remove_PropertyNames, Remove_Select);
                case select.MipMapRemove:
                    return new MipMapRemove(MipMapRemove_PropertyNames, MipMapRemove_Select);

                default:
                    return null;
            }

        }
    }

}
#endif
