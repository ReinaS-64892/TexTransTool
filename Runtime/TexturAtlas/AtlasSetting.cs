#if UNITY_EDITOR
using UnityEngine;
using System;
using Rs64.TexTransTool.Island;
using Rs64.TexTransTool.TexturAtlas.FineSettng;
using UnityEditor;
using System.Collections.Generic;

namespace Rs64.TexTransTool.TexturAtlas
{
    [Serializable]
    public class AtlasSetting
    {
        public bool IsMargeMaterial;
        public Material MargeRefarensMaterial;
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
            IfineSettings.Sort((L,R) => L.Order - R.Order);
            return IfineSettings;
        }

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
        public string Resize_PropatyNames = "_MainTex";
        public PropatySelect Resize_select = PropatySelect.NotEqual;
        //Compless
        public Compless.FromatQuality Compless_fromatQuality = Compless.FromatQuality.High;
        public TextureCompressionQuality Compless_compressionQuality = TextureCompressionQuality.Best;
        public string Compless_PropatyNames = "_MainTex";
        public PropatySelect Compless_select = PropatySelect.Equal;
        //RefarensCopy
        public string RefarensCopy_SousePropatyName = "_MainTex";
        public string RefarensCopy_TargetPropatyName = "_OutlineTex";
        //Remove
        public string Remove_PropatyNames = "_MainTex";
        public PropatySelect Remove_select = PropatySelect.NotEqual;
        //MipMapRemove
        public string MipMapRemove_PropatyNames = "_OutlineWidthMask";
        public PropatySelect MipMapRemove_select = PropatySelect.Equal;

        public IFineSetting GetFineSetting()
        {
            switch (select)
            {
                case FineSettingSelect.Resize:
                    return new Resize(Resize_Size, Resize_PropatyNames, Resize_select);
                case FineSettingSelect.Compless:
                    return new Compless(Compless_fromatQuality, Compless_compressionQuality, Compless_PropatyNames, Compless_select);
                case FineSettingSelect.RefarensCopy:
                    return new RefarensCopy(RefarensCopy_SousePropatyName, RefarensCopy_TargetPropatyName);
                case FineSettingSelect.Remove:
                    return new Remove(Remove_PropatyNames, Remove_select);
                case FineSettingSelect.MipMapRemove:
                    return new MipMapRemove(MipMapRemove_PropatyNames, MipMapRemove_select);

                default:
                    return null;
            }

        }
    }

}
#endif