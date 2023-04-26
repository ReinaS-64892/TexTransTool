using System;
using System.Collections.Generic;
using UnityEngine;
namespace Rs.TexturAtlasCompiler
{
    public static class Utils
    {
        public static void ForEach2D(Vector2Int Reange, Action<int, int> action)
        {
            int countx = 0;
            int county = 0;
            while (true)
            {
                if (!(Reange.x > countx))
                {
                    countx = 0;
                    county += 1;
                }
                if (!(Reange.y > county))
                {
                    break;
                }

                action.Invoke(countx, county);

                countx += 1;
            }
        }
        public static List<Vector2Int> Reange2d(Vector2Int Reange)
        {
            var List = new List<Vector2Int>();
            ForEach2D(Reange, (x, y) => List.Add(new Vector2Int(x, y)));
            return List;
        }

        public static int TwoDToOneDIndex(Vector2Int TowDIndex, int Size)
        {
            return (TowDIndex.y * Size) + TowDIndex.x;
        }
    }
}
