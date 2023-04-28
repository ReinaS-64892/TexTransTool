#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Rs.TexturAtlasCompiler.Decal
{
    public static class DecalUtil
    {
        public static List<Vector3> ConvartDecalMatlix(Matrix4x4 matrix, float DecalSize, List<Vector3> Vertices)
        {
            var ConvertVertices = new List<Vector3>();
            foreach (var Vertice in Vertices)
            {
                var Pos = matrix.MultiplyPoint3x4(Vertice) + new Vector3(DecalSize / 2, DecalSize / 2, 0);
                Pos.x /= DecalSize;
                Pos.y /= DecalSize;
                ConvertVertices.Add(Pos);
            }
            return ConvertVertices;
        }

        public static List<TraiangleIndex> FiltaringTraiangle(
            List<TraiangleIndex> Target, List<Vector3> Vartex, float MaxDistans,
            float StartDistans = 0, float MinRange = 0, float MaxRange = 1
        )
        {
            var FiltalingTraingles = new List<TraiangleIndex>();
            foreach (var Traiangle in Target)
            {
                if (Vartex[Traiangle[0]].z < StartDistans || Vartex[Traiangle[1]].z < StartDistans || Vartex[Traiangle[2]].z < StartDistans)
                {
                    continue;
                }
                if (Vartex[Traiangle[0]].z > MaxDistans && Vartex[Traiangle[1]].z > MaxDistans && Vartex[Traiangle[2]].z > MaxDistans)
                {
                    continue;
                }

                var ba = Vartex[Traiangle[1]] - Vartex[Traiangle[0]];
                var ac = Vartex[Traiangle[0]] - Vartex[Traiangle[2]];
                var TraiangleSide = Vector3.Cross(ba, ac).z;
                if (TraiangleSide < 0)
                {
                    continue;
                }


                bool OutOfPrygon = false;
                foreach (var VIndex in Traiangle)
                {
                    var Tvartex = Vartex[VIndex];
                    //Debug.Log(Tvartex);
                    if (Tvartex.x < MaxRange && Tvartex.x > MinRange && Tvartex.y < MaxRange && Tvartex.y > MinRange) OutOfPrygon = true;
                }
                if (!OutOfPrygon) continue;


                FiltalingTraingles.Add(Traiangle);
            }
            return FiltalingTraingles;
        }
    }
}
#endif