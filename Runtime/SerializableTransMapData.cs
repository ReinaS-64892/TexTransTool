using System;
using UnityEngine;

namespace Rs64.TexTransTool
{
    [Serializable]
    public class SerializableTransMapData
    {
        public Vector2[] OneDMap;
        public float[] OneDDistansMap;
        public float DefaultPading;
        public Vector2Int MapSize;

        public SerializableTransMapData(TransMapData S)
        {
            OneDMap = Utils.TowDtoOneD(S.Map, S.MapSize);
            OneDDistansMap = Utils.TowDtoOneD(S.DistansMap, S.MapSize);
            DefaultPading = S.DefaultPading;
            MapSize = S.MapSize;
        }
        public TransMapData GetTransMapData()
        {
            var TDM = new TransMapData();
            TDM.Map = Utils.OneDToTowD(OneDMap, MapSize);
            TDM.DistansMap = Utils.OneDToTowD(OneDDistansMap, MapSize);
            TDM.DefaultPading = DefaultPading;
            TDM.MapSize = MapSize;
            return TDM;
        }

    }
}