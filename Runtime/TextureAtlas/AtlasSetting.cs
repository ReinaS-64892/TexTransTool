#if UNITY_EDITOR
using UnityEngine;
using System;
using net.rs64.TexTransTool.Island;
using net.rs64.TexTransTool.TextureAtlas.AdvancedSetting;
using UnityEditor;
using System.Collections.Generic;

namespace net.rs64.TexTransTool.TextureAtlas
{
    [Serializable]
    public class AtlasSetting
    {
        public bool IsMergeMaterial;
        public Material MergeReferenceMaterial;
        public PropertyBakeSetting PropertyBakeSetting = PropertyBakeSetting.NotBake;
        public bool ForceSetTexture;
        public Vector2Int AtlasTextureSize = new Vector2Int(2048, 2048);
        public PaddingType PaddingType = PaddingType.EdgeBase;
        public float Padding = 10;
        public IslandSorting.IslandSortingType SortingType = IslandSorting.IslandSortingType.NextFitDecreasingHeightPlusFloorCeiling;
        public List<AdvancedSettingData> advancedSettings;
        public float GetTexScalePadding => Padding / AtlasTextureSize.x;

        public List<IAdvancedSetting> GetAdvancedSettings()
        {
            var IAdvancedSettings = new List<IAdvancedSetting>
            {
                new Initialize(),
                new DefaultCompress()
            };
            foreach (var advancedSetting in advancedSettings)
            {
                IAdvancedSettings.Add(advancedSetting.GetAdvancedSetting());
            }
            IAdvancedSettings.Sort((L, R) => L.Order - R.Order);
            return IAdvancedSettings;
        }

    }
    public enum PropertyBakeSetting
    {
        NotBake,
        Bake,
        BakeAllProperty,
    }
    [Serializable]
    public class AdvancedSettingData
    {
        public AdvancedSettingSelect select;
        public enum AdvancedSettingSelect
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
        public PropertyName ReferenceCopy_SousePropertyName;
        public PropertyName ReferenceCopy_TargetPropertyName;
        //Remove
        public PropertyName Remove_PropertyNames;
        public PropertySelect Remove_Select = PropertySelect.NotEqual;
        //MipMapRemove
        public PropertyName MipMapRemove_PropertyNames;
        public PropertySelect MipMapRemove_Select = PropertySelect.Equal;

        public IAdvancedSetting GetAdvancedSetting()
        {
            switch (select)
            {
                case AdvancedSettingSelect.Resize:
                    return new Resize(Resize_Size, Resize_PropertyNames, Resize_Select);
                case AdvancedSettingSelect.Compress:
                    return new Compress(Compress_FormatQuality, Compress_CompressionQuality, Compress_PropertyNames, Compress_Select);
                case AdvancedSettingSelect.ReferenceCopy:
                    return new ReferenceCopy(ReferenceCopy_SousePropertyName, ReferenceCopy_TargetPropertyName);
                case AdvancedSettingSelect.Remove:
                    return new Remove(Remove_PropertyNames, Remove_Select);
                case AdvancedSettingSelect.MipMapRemove:
                    return new MipMapRemove(MipMapRemove_PropertyNames, MipMapRemove_Select);

                default:
                    return null;
            }

        }
    }

}
#endif
