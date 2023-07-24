#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rs64.TexTransTool.Island;
using Rs64.TexTransTool.ShaderSupport;
using UnityEditor;
using UnityEngine;
namespace Rs64.TexTransTool.TexturAtlas
{
    public static class TexturAtlasCompiler
    {

        public static void OffSetApply<T>(this TagIslandPool<T> IslandPool, float Offset)
        {
            foreach (var islandI in IslandPool)
            {
                var island = islandI.island;
                island.Size *= Offset;
            }
        }

        public static void GenereatMovedIlands<T>(IslandSortingType SortingType, TagIslandPool<T> IslandPool)
        {
            switch (SortingType)
            {
                case IslandSortingType.EvenlySpaced:
                    {
                        IslandUtils.IslandPoolEvenlySpaced(IslandPool);
                        break;
                    }
                case IslandSortingType.NextFitDecreasingHeight:
                    {
                        IslandUtils.IslandPoolNextFitDecreasingHeight(IslandPool);
                        break;
                    }
                case IslandSortingType.NextFitDecreasingHeightPlusFloorCeilineg:
                    {
                        IslandUtils.IslandPoolNextFitDecreasingHeightPlusFloorCeilineg(IslandPool);
                        break;
                    }

                default: throw new ArgumentException();
            }
        }

    }

    public enum IslandSortingType
    {
        EvenlySpaced,
        NextFitDecreasingHeight,
        NextFitDecreasingHeightPlusFloorCeilineg,
    }
    [Serializable]
    public class PropAndAtlasTex
    {
        public string PropertyName;
        public TransTargetTexture AtlasTexture;

        public PropAndAtlasTex(TransTargetTexture texture2D, string propertyName)
        {
            AtlasTexture = texture2D;
            PropertyName = propertyName;
        }
        public PropAndAtlasTex(string propertyName, TransTargetTexture texture2D)
        {
            AtlasTexture = texture2D;
            PropertyName = propertyName;
        }

        public static explicit operator PropAndTexture(PropAndAtlasTex s)
        {
            return new PropAndTexture(s.PropertyName, s.AtlasTexture.Texture2D);
        }
    }
}
#endif