using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransCore.TransTextureCore.Utils
{
    internal static class DimensionIndexUtility
    {
        public static void ForEach2D(Vector2Int Range, Action<int, int> action)
        {
            int countX = 0;
            int countyY = 0;
            while (true)
            {
                if (!(Range.x > countX))
                {
                    countX = 0;
                    countyY += 1;
                }
                if (!(Range.y > countyY))
                {
                    break;
                }

                action.Invoke(countX, countyY);

                countX += 1;
            }
        }
        public static List<Vector2Int> Range2d(Vector2Int Range)
        {
            var list = new List<Vector2Int>();
            ForEach2D(Range, (x, y) => list.Add(new Vector2Int(x, y)));
            return list;
        }

        public static int TwoDToOneDIndex(Vector2Int TowDIndex, int Size)
        {
            return (TowDIndex.y * Size) + TowDIndex.x;
        }
        public static Vector2Int ConvertIndex2D(int Index1D, int width)
        {
            return new Vector2Int(Index1D % width, Index1D / width);
        }
        public static T[] TowDtoOneD<T>(T[,] SouseArray, Vector2Int Size)
        {
            T[] oneDArray = new T[Size.x * Size.y];
            foreach (var index in Range2d(Size))
            {
                oneDArray[TwoDToOneDIndex(index, Size.x)] = SouseArray[index.x, index.y];
            }
            return oneDArray;
        }
        public static T[,] OneDToTowD<T>(T[] SouseArray, Vector2Int Size)
        {
            T[,] towDArray = new T[Size.x, Size.y];
            int count = -1;
            foreach (var value in SouseArray)
            {
                count += 1;
                var index = ConvertIndex2D(count, Size.x);
                towDArray[index.x, index.y] = value;
            }
            return towDArray;
        }
    }
}