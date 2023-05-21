#if UNITY_EDITOR
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using System.Collections;

namespace Rs64.TexTransTool
{
    [Serializable]
    public class TransMapData
    {
        public Vector2[,] Map;
        public float[,] DistansMap;
        public float DefaultPading;
        public Vector2Int MapSize;

        public TransMapData(Vector2[,] map, float[,] distansMap, float defaultPading, Vector2Int mapSize)
        {
            Map = map;
            DistansMap = distansMap;
            DefaultPading = defaultPading;
            MapSize = mapSize;
        }
        public TransMapData(float Pading, Vector2Int mapSize)
        {
            Map = new Vector2[mapSize.x, mapSize.y];
            DistansMap = new float[mapSize.x, mapSize.y];
            DefaultPading = Pading;
            foreach (var index in Utils.Reange2d(mapSize))
            {
                DistansMap[index.x, index.y] = Pading;
            }
            MapSize = mapSize;
        }
        public TransMapData()
        {
        }

        public Vector3[,] GetMapAndDistansMap()
        {
            var MargeDmap = new Vector3[MapSize.x, MapSize.y];
            foreach (var index in Utils.Reange2d(MapSize))
            {
                MargeDmap[index.x, index.y] = new Vector3(Map[index.x, index.y].x, Map[index.x, index.y].y, DistansMap[index.x, index.y]);
            }
            return MargeDmap;
        }
    }
    [Serializable]
    public class TransTargetTexture
    {
        public Texture2D Texture2D;
        public float[,] DistansMap;

        public TransTargetTexture(Texture2D texture2D, float[,] distansMap)
        {
            this.Texture2D = texture2D;
            DistansMap = distansMap;
        }
        public TransTargetTexture(Texture2D texture2D, float DefoultDistans)
        {
            this.Texture2D = texture2D;
            DistansMap = new float[texture2D.width, texture2D.height];
            foreach (var index in Utils.Reange2d(new Vector2Int(texture2D.width, texture2D.height)))
            {
                DistansMap[index.x, index.y] = DefoultDistans;
            }
        }
    }

}
#endif