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
    }

}
#endif