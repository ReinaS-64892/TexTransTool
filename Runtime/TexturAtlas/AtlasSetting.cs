#if UNITY_EDITOR
using UnityEngine;
using System;
using net.rs64.TexTransTool.Island;
using net.rs64.TexTransTool.TextureAtlas.FineSettng;
using UnityEditor;
using System.Collections.Generic;

namespace net.rs64.TexTransTool.TextureAtlas
{
    [Serializable]
    public class AtlasSetting
    {
        public bool IsMargeMaterial;
        public Material MargeRefarensMaterial;
        public PropertyBakeSetting PropertyBakeSetting = PropertyBakeSetting.NotBake;
        public bool ForseSetTexture;
        public Vector2Int AtlasTextureSize = new Vector2Int(2048, 2048);
        public PadingType PadingType = PadingType.EdgeBase;
        public float Pading = 10;
        public IslandSorting.IslandSortingType SortingType = IslandSorting.IslandSortingType.NextFitDecreasingHeightPlusFloorCeilineg;
        public List<FineSettingDeta> fineSettings;
        public float GetTexScailPading => Pading / AtlasTextureSize.x;

        public List<IFineSetting> GetFineSettings()
        {
            var IfineSettings = new List<IFineSetting>
            {
                new Initialize(),
                new DefaultCompless()
            };
            foreach (var fineSetting in fineSettings)
            {
                IfineSettings.Add(fineSetting.GetFineSetting());
            }
            IfineSettings.Sort((L, R) => L.Order - R.Order);
            return IfineSettings;
        }

    }
    public enum PropertyBakeSetting
    {
        NotBake,
        Bake,
        BakeAllProperty,
    }
    [Serializable]
    public class FineSettingDeta
    {
        public FineSettingSelect select;
        public enum FineSettingSelect
        {
            Resize,
            Compless,
            RefarensCopy,
            Remove,
            MipMapRemove,
        }

        //Resize
        public int Resize_Size = 512;
        public PropertyName Resize_PropertyNames;
        public PropertySelect Resize_select = PropertySelect.NotEqual;
        //Compless
        public Compless.FromatQuality Compless_fromatQuality = Compless.FromatQuality.High;
        public TextureCompressionQuality Compless_compressionQuality = TextureCompressionQuality.Best;
        public PropertyName Compless_PropertyNames;
        public PropertySelect Compless_select = PropertySelect.Equal;
        //RefarensCopy
        public PropertyName RefarensCopy_SousePropertyName;
        public PropertyName RefarensCopy_TargetPropertyName;
        //Remove
        public PropertyName Remove_PropertyNames;
        public PropertySelect Remove_select = PropertySelect.NotEqual;
        //MipMapRemove
        public PropertyName MipMapRemove_PropertyNames;
        public PropertySelect MipMapRemove_select = PropertySelect.Equal;

        public IFineSetting GetFineSetting()
        {
            switch (select)
            {
                case FineSettingSelect.Resize:
                    return new Resize(Resize_Size, Resize_PropertyNames, Resize_select);
                case FineSettingSelect.Compless:
                    return new Compless(Compless_fromatQuality, Compless_compressionQuality, Compless_PropertyNames, Compless_select);
                case FineSettingSelect.RefarensCopy:
                    return new RefarensCopy(RefarensCopy_SousePropertyName, RefarensCopy_TargetPropertyName);
                case FineSettingSelect.Remove:
                    return new Remove(Remove_PropertyNames, Remove_select);
                case FineSettingSelect.MipMapRemove:
                    return new MipMapRemove(MipMapRemove_PropertyNames, MipMapRemove_select);

                default:
                    return null;
            }

        }
    }

}
#endif
