#if UNITY_EDITOR
using System.Threading.Tasks;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.CompilerServices;

namespace Rs.TexturAtlasCompiler
{
    public static class AtlasMapper
    {
        public static List<TraiangleIndex> ToList(int[] triangleIndexs)
        {
            var TraianglesList = new List<TraiangleIndex>();
            int count = 0;
            while (triangleIndexs.Length > count)
            {
                TraianglesList.Add(new TraiangleIndex(triangleIndexs[count], triangleIndexs[count += 1], triangleIndexs[count += 1]));
                count += 1;
            }
            return TraianglesList;
        }

        public static async Task<AtlasMapData> AtlasMapGeneratAsync(AtlasMapData Map, List<TraiangleIndex> triangles, List<Vector2> TargetUV, List<Vector2> SourceUV, PadingType padingType)
        {
            var TargetTexScaleTargetUV = UVtoTexScale(TargetUV, Map.MapSize);

            ConfiguredTaskAwaitable<(Vector2, float)>[,] UVMappingTask = new ConfiguredTaskAwaitable<(Vector2, float)>[Map.MapSize.x, Map.MapSize.y];

            foreach (var index in Utils.Reange2d(Map.MapSize))
            {
                UVMappingTask[index.x, index.y] = Task.Run<(Vector2, float)>(() => UVMapingCalculat(triangles, TargetTexScaleTargetUV, SourceUV, new Vector2Int(index.x, index.y), padingType, Map.DistansMap[index.x, index.y])).ConfigureAwait(false);
            }

            foreach (var index in Utils.Reange2d(Map.MapSize))
            {
                var Result = await UVMappingTask[index.x, index.y];
                if (Map.DistansMap[index.x, index.y] < Result.Item2)
                {
                    Map.Map[index.x, index.y] = Result.Item1;
                    Map.DistansMap[index.x, index.y] = Result.Item2;
                }
            }

            return Map;
        }

        public static AtlasMapData AtlasMapGenerat(AtlasMapData Map, List<TraiangleIndex> triangles, List<Vector2> TargetUV, List<Vector2> SourceUV, PadingType padingType)
        {
            var TargetTexScaleTargetUV = UVtoTexScale(TargetUV, Map.MapSize);
            foreach (var Index in Utils.Reange2d(Map.MapSize))
            {
                var Result = UVMapingCalculat(triangles, TargetTexScaleTargetUV, SourceUV, new Vector2Int(Index.x, Index.y), padingType, Map.DistansMap[Index.x, Index.y]);
                if (Map.DistansMap[Index.x, Index.y] < Result.Item2)
                {
                    Map.Map[Index.x, Index.y] = Result.Item1;
                    Map.DistansMap[Index.x, Index.y] = Result.Item2;
                }
            }
            return Map;
        }

        public static (Vector2, float) UVMapingCalculat(List<TraiangleIndex> TrianglesToIndex, List<Vector2> TargetTexScaleTargetUV, List<Vector2> SourceUV, Vector2Int TargetPixsel, PadingType padingType, float DefaultDistans)
        {
            Vector2 Targetpixself = TargetPixsel + new Vector2(0.25f, 0.25f);
            float Distans = DefaultDistans;
            Vector2 SourceUVPosition = default;
            foreach (var TriangleToIndex in TrianglesToIndex)
            {
                var TargetUVTriangle = new List<Vector2> { TargetTexScaleTargetUV[TriangleToIndex[0]], TargetTexScaleTargetUV[TriangleToIndex[1]], TargetTexScaleTargetUV[TriangleToIndex[2]] };
                var ClossT = ClossTraiangle(TargetUVTriangle, Targetpixself);
                float Distansnew;
                switch (padingType)
                {
                    case PadingType.EdgeBase:
                    default:
                        {
                            Distansnew = MinVector(DistansVartBase(TargetUVTriangle, Targetpixself));
                            break;
                        }
                    case PadingType.VartexBase:
                        {
                            Distansnew = MinVector(DistansEdgeBase(TargetUVTriangle, Targetpixself));
                            break;
                        }
                }
                Distansnew *= -1;
                if (Distans < Distansnew)
                {
                    var SourceUVTriangle = new List<Vector2> { SourceUV[TriangleToIndex[0]], SourceUV[TriangleToIndex[1]], SourceUV[TriangleToIndex[2]] };
                    SourceUVPosition = FromTBC(SourceUVTriangle, ToTBC(ClossT));
                }
                /*
                                    Debug.Log("tInoutlog " + IsPointInsideTriangleDebagu(TargetUVTriangle, Targetpixself).x + "/" + IsPointInsideTriangleDebagu(TargetUVTriangle, Targetpixself).y + "/" + IsPointInsideTriangleDebagu(TargetUVTriangle, Targetpixself).z + "/" +
                "sousepos " + FromTriangleBarycentricCoordinateConversion(SourceUVTriangle, ToTriangleBarycentricCoordinateConversion(TargetUVTriangle, TargetPixsel)).ToString() + "/" +
                "traianglepos  " + ToTriangleBarycentricCoordinateConversion(TargetUVTriangle, TargetPixsel).ToString() + "/" +
                "targetpos " + TargetPixsel + "/" +
                "Trainagles" + TargetTexScaleTargetUV[TriangleToIndex[0]] + "/" + TargetTexScaleTargetUV[TriangleToIndex[1]] + "/" + TargetTexScaleTargetUV[TriangleToIndex[2]]

                );
                */
            }
            return (SourceUVPosition, Distans);
        }
        public static Vector4 ClossTraiangle(List<Vector2> Triangle, Vector2 TargetPoint)
        {
            var w = Vector3.Cross(Triangle[2] - Triangle[1], TargetPoint - Triangle[1]).z;
            var u = Vector3.Cross(Triangle[0] - Triangle[2], TargetPoint - Triangle[2]).z;
            var v = Vector3.Cross(Triangle[1] - Triangle[0], TargetPoint - Triangle[0]).z;
            var wuv = Vector3.Cross(Triangle[1] - Triangle[0], Triangle[2] - Triangle[0]).z;
            return new Vector4(w, u, v, wuv);
            /*
                      //Debug.Log(vartext[0].ToString() + "/" + vartext[1].ToString() + "/" + vartext[2].ToString() + "/" + targetpos.ToString() + "/" + (res1 && res2 && res3).ToString() + "/" + (!res1 && !res2 && !res3).ToString());
                        if (w && u && v) return true;
                        //if (!Isinverted && !Isinverted2 && !Isinverted3) return true;
                        return false;
            */
        }

        public static Vector3 ToTBC(Vector4 ClassT)
        {
            /*
                        var a = ((Triangle[1].y - Triangle[2].y) * (TargetPoint.x - Triangle[0].x) + (Triangle[2].x - Triangle[1].x) * (TargetPoint.y - Triangle[2].y))
                                /
                                ((Triangle[1].y - Triangle[2].y) * (Triangle[0].x - Triangle[2].x) + (Triangle[2].x - Triangle[1].x) * (Triangle[0].y - Triangle[2].y));
                        var b = ((Triangle[2].y - Triangle[0].y) * (TargetPoint.x - Triangle[0].x) + (Triangle[0].x - Triangle[2].x) * (TargetPoint.y - Triangle[2].y))
                                /
                                ((Triangle[1].y - Triangle[2].y) * (Triangle[0].x - Triangle[2].x) + (Triangle[2].x - Triangle[1].x) * (Triangle[0].y - Triangle[2].y));
                        var y = 1 - a - b;
            */
            var a = ClassT.x / ClassT.w;
            var b = ClassT.y / ClassT.w;
            var c = ClassT.z / ClassT.w;
            //Debug.Log(new Vector3(res1, res2, res3));
            /*
                        var a = Vector2.Distance(Triangle[0], TargetPoint);
                        var b = Vector2.Distance(Triangle[1], TargetPoint);
                        var y = Vector2.Distance(Triangle[2], TargetPoint);
                        var normaraizescild = 1 / (a + b + y);
                        a = 1 - (a * normaraizescild);
                        b = 1 - (b * normaraizescild);
                        y = 1 - (y * normaraizescild);
            */
            return new Vector3(a, b, c);
        }

        public static Vector2 FromTBC(List<Vector2> Triangle, Vector3 SuorseTBC)
        {
            var ConversionPos = Vector2.zero;
            ConversionPos += Triangle[0] * SuorseTBC.x;
            ConversionPos += Triangle[1] * SuorseTBC.y;
            ConversionPos += Triangle[2] * SuorseTBC.z;
            return ConversionPos;
        }

        public static (Vector2, Vector2) TriangleToBoundingBox(List<Vector2> Triangle)
        {
            Vector2 min = Triangle[0];
            Vector2 max = Triangle[0];

            min = Vector2.Min(min, Triangle[1]);
            max = Vector2.Max(max, Triangle[1]);

            min = Vector2.Min(min, Triangle[2]);
            max = Vector2.Max(max, Triangle[2]);
            return (min, max);
        }

        public static Vector3 DistansVartBase(List<Vector2> Triangle, Vector2 TargetPoint)
        {
            var w = Vector2.Distance(Triangle[0], TargetPoint);
            var u = Vector2.Distance(Triangle[1], TargetPoint);
            var v = Vector2.Distance(Triangle[2], TargetPoint);
            return new Vector3(w, u, v);
            /*
                      //Debug.Log(vartext[0].ToString() + "/" + vartext[1].ToString() + "/" + vartext[2].ToString() + "/" + targetpos.ToString() + "/" + (res1 && res2 && res3).ToString() + "/" + (!res1 && !res2 && !res3).ToString());
                        if (w && u && v) return true;
                        //if (!Isinverted && !Isinverted2 && !Isinverted3) return true;
                        return false;
            */
        }

        public static Vector2 NeaPointOnLine(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 ab = b - a;
            float Leng = ab.magnitude;
            ab.Normalize();
            float lp = Mathf.Clamp(Vector2.Dot(p - a, ab), 0, Leng);
            return a + (lp * ab);
        }

        public static Vector3 DistansEdgeBase(List<Vector2> Triangle, Vector2 TargetPoint)
        {
            float DistansA = Vector2.Distance(NeaPointOnLine(Triangle[0], Triangle[1], TargetPoint), TargetPoint);
            float DistansB = Vector2.Distance(NeaPointOnLine(Triangle[1], Triangle[2], TargetPoint), TargetPoint);
            float DistansC = Vector2.Distance(NeaPointOnLine(Triangle[2], Triangle[0], TargetPoint), TargetPoint);
            return new Vector3(DistansA, DistansB, DistansC);
        }

        public static float MinVector(Vector3 Vector)

        {
            return Mathf.Min(Vector.x, Mathf.Min(Vector.y, Vector.z));
        }

        public static AtlasMapData UVMappingTableGeneratorComputeShederUsed(ComputeShader Shader, AtlasMapData Map, List<TraiangleIndex> TrianglesToIndex, List<Vector2> TargetTexScaleTargetUV, List<Vector2> SourceUV, PadingType padingType = PadingType.EdgeBase)
        {
            Vector2Int ThredGropSize = Map.MapSize / 32;
            int karnelindex = -1;
            switch (padingType)
            {
                case PadingType.EdgeBase:
                    karnelindex = Shader.FindKernel("AtlasMapGeneratPadingEdgeBase");
                    break;
                case PadingType.VartexBase:
                    karnelindex = Shader.FindKernel("AtlasMapGeneratPadingVartexBase");
                    break;
            }
            var ResBuffer = new ComputeBuffer((Map.MapSize.x * Map.MapSize.y), 12);
            //Debug.Log(Buffer.count + "/" + (TargetTexturesize.x * TargetTexturesize.y).ToString());


            var array = Utils.Reange2d(Map.MapSize).ConvertAll<Vector3>(i => new Vector3(0, 0, Map.DefaultPading));
            ResBuffer.SetData<Vector3>(array);
            Shader.SetBuffer(karnelindex, "Result", ResBuffer);


            var TriBuffer = new ComputeBuffer((TrianglesToIndex.Count * 6), 8);
            var TriangleList = new List<Vector2>();
            foreach (var TriangleToIndex in TrianglesToIndex)
            {
                TriangleList.Add(TargetTexScaleTargetUV[TriangleToIndex[0]]);
                TriangleList.Add(TargetTexScaleTargetUV[TriangleToIndex[1]]);
                TriangleList.Add(TargetTexScaleTargetUV[TriangleToIndex[2]]);
                TriangleList.Add(SourceUV[TriangleToIndex[0]]);
                TriangleList.Add(SourceUV[TriangleToIndex[1]]);
                TriangleList.Add(SourceUV[TriangleToIndex[2]]);
                /*
                var TargetUVTriangle = new float[3 * 4];
                TargetUVTriangle[0] = TargetTexScaleTargetUV[TriangleToIndex[0]].x;
                TargetUVTriangle[1] = TargetTexScaleTargetUV[TriangleToIndex[0]].y;
                TargetUVTriangle[4] = TargetTexScaleTargetUV[TriangleToIndex[1]].x;
                TargetUVTriangle[5] = TargetTexScaleTargetUV[TriangleToIndex[1]].y;
                TargetUVTriangle[8] = TargetTexScaleTargetUV[TriangleToIndex[2]].x;
                TargetUVTriangle[9] = TargetTexScaleTargetUV[TriangleToIndex[2]].y;
                var SouseUVTraiangle = new float[3 * 4];
                SouseUVTraiangle[0] = SourceUV[TriangleToIndex[0]].x;
                SouseUVTraiangle[1] = SourceUV[TriangleToIndex[0]].y;
                SouseUVTraiangle[4] = SourceUV[TriangleToIndex[1]].x;
                SouseUVTraiangle[5] = SourceUV[TriangleToIndex[1]].y;
                SouseUVTraiangle[8] = SourceUV[TriangleToIndex[2]].x;
                SouseUVTraiangle[9] = SourceUV[TriangleToIndex[2]].y;
*/
                /*
                                                Debug.Log("target " + TargetTexScaleTargetUV[TriangleToIndex[0]].x + "/" + TargetTexScaleTargetUV[TriangleToIndex[0]].y + " " +
                                                TargetTexScaleTargetUV[TriangleToIndex[1]].x + "/" + TargetTexScaleTargetUV[TriangleToIndex[1]].y + " " +
                                                TargetTexScaleTargetUV[TriangleToIndex[2]].x + "/" + TargetTexScaleTargetUV[TriangleToIndex[2]].y
                                                ); Debug.Log("Siouse " + SourceUV[TriangleToIndex[0]].x + "/" + SourceUV[TriangleToIndex[0]].y + " " +
                                                SourceUV[TriangleToIndex[1]].x + "/" + SourceUV[TriangleToIndex[1]].y + " " +
                                                SourceUV[TriangleToIndex[2]].x + "/" + SourceUV[TriangleToIndex[2]].y
                                                );
                */
            }
            TriBuffer.SetData<Vector2>(TriangleList);
            Shader.SetBuffer(karnelindex, "Traiangles", TriBuffer);


            Shader.SetInt("Size", Map.MapSize.x);
            Shader.Dispatch(karnelindex, ThredGropSize.x, ThredGropSize.y, TrianglesToIndex.Count);


            var carray = array.ToArray();
            ResBuffer.GetData(carray);
            foreach (var Index in Utils.Reange2d(Map.MapSize))
            {
                var data = carray[(Index.y * Map.MapSize.x) + Index.x];

                if (data.z > Map.DefaultPading)
                {
                    Map.Map[Index.x, Index.y] = new Vector2(data.x, data.y);
                    Map.DistansMap[Index.x, Index.y] = data.z;
                }
                /*
                                //Debug.Log("NYANYA " + "/" + cout + "/" + ((cout.y * TargetTexturesize.x) + cout.x) + "/" + data.x + "/" + data.y + "/" + data.z);
                                //Debug.Log(data.x + "/" + data.y + "/" + data.z);
                */
            }


            ResBuffer.Release();
            TriBuffer.Release();

            return Map;
        }

        public static List<Vector2> UVtoTexScale(List<Vector2> UV, Vector2Int TexSize)
        {
            return UV.ConvertAll<Vector2>(i => new Vector2(i.x * TexSize.x, i.y * TexSize.y));
        }

        public static (Vector2, Vector2) BoxCal(List<Vector2> Traiangels)
        {
            Vector2 min = new Vector2();
            Vector2 max = new Vector2();
            bool Farst = true;
            foreach (var tri in Traiangels)
            {
                if (Farst)
                {
                    min = tri;
                    max = tri;
                    Farst = false;
                    continue;
                }
                min = Vector2.Min(min, tri);
                max = Vector2.Max(max, tri);
            }
            return (min, max);
        }

    }

    public enum PadingType
    {
        EdgeBase,
        VartexBase,
    }
    public enum ExecuteClient
    {
        CPU,
        AsyncCPU,
        ComputeSheder,
    }


}
#endif