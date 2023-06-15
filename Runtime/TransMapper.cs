#if UNITY_EDITOR
using System.Threading.Tasks;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Rs64.TexTransTool
{
    public static class TransMapper
    {
        public const string TransMapperPath = "Packages/rs64.tex-trans-tool/Runtime/ComputeShaders/TransMapper.compute";

        public static (Vector2, float) UVMapingCalculat(List<TraiangleIndex> TrianglesToIndex, List<Vector2> TargetTexScaleTargetUV, List<Vector2> SourceUV, Vector2Int TargetPixsel, PadingType padingType, float DefaultDistans)
        {
            Vector2 Targetpixself = TargetPixsel;// + new Vector2(0.5f, 0.5f);
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
                    SourceUVPosition = FromBCS(SourceUVTriangle, ToBCS(ClossT));
                    Distans = Distansnew;
                }
            }
            return (SourceUVPosition, Distans);
        }
        public static Vector4 ClossTraiangle(List<Vector2> Triangle, Vector2 TargetPoint)
        {
            var w = Vector3.Cross(Triangle[2] - Triangle[1], TargetPoint - Triangle[1]).z;
            var u = Vector3.Cross(Triangle[0] - Triangle[2], TargetPoint - Triangle[2]).z;
            var v = Vector3.Cross(Triangle[1] - Triangle[0], TargetPoint - Triangle[0]).z;
            var wuv = TraiangelArea(Triangle);
            return new Vector4(w, u, v, wuv);
        }
        public static float TraiangelArea(List<Vector2> Triangle)
        {
            return Vector3.Cross(Triangle[1] - Triangle[0], Triangle[2] - Triangle[0]).z;
        }

        public static Vector3 ToBCS(Vector4 ClassT)
        {
            var a = ClassT.x / ClassT.w;
            var b = ClassT.y / ClassT.w;
            var c = ClassT.z / ClassT.w;

            return new Vector3(a, b, c);
        }

        public static Vector2 FromBCS(List<Vector2> Triangle, Vector3 SuorseTBC)
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
        }

        public static Vector2 NeaPointOnLine(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 ab = b - a;
            float Leng = ab.magnitude;
            ab.Normalize();
            float lp = Mathf.Clamp(Vector2.Dot(p - a, ab), 0, Leng);
            return a + (lp * ab);
        }
        public static Vector2 NeaPoint(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 ab = b - a;
            ab.Normalize();
            float lp = Vector2.Dot(p - a, ab);
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

        public static TransMapData TransMapGeneratUseComputeSheder(ComputeShader Shader, TransMapData TransMap, List<TraiangleIndex> TrianglesToIndex, List<Vector2> TargetTexScaleTargetUV, List<Vector2> SourceUV, PadingType padingType = PadingType.EdgeBase)
        {
            if (Shader == null) Shader = AssetDatabase.LoadAssetAtPath<ComputeShader>(TransMapperPath);
            Vector2Int ThredGropSize = TransMap.Map.MapSize / 32;
            int karnelindex = -1;
            switch (padingType)
            {
                case PadingType.EdgeBase:
                    karnelindex = Shader.FindKernel("TransMapGeneratPadingEdgeBase");
                    break;
                case PadingType.VartexBase:
                    karnelindex = Shader.FindKernel("TransMapGeneratPadingVartexBase");
                    break;
            }

            var ResBuffer = new ComputeBuffer((TransMap.Map.Array.Length), 12);
            ResBuffer.SetData(TransMap.Map.Array);
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
            }
            TriBuffer.SetData<Vector2>(TriangleList);
            Shader.SetBuffer(karnelindex, "Traiangles", TriBuffer);

            Shader.SetInt("Size", TransMap.Map.MapSize.x);
            Shader.Dispatch(karnelindex, ThredGropSize.x, ThredGropSize.y, TrianglesToIndex.Count);

            ResBuffer.GetData(TransMap.Map.Array);

            ResBuffer.Release();
            TriBuffer.Release();

            return TransMap;
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

        public static bool IsInCal(float w, float u, float v)
        {
            return ((w > 0) == (u > 0)) && ((u > 0) == (v > 0));
        }

    }

    public enum PadingType
    {
        EdgeBase,
        VartexBase,
    }
    [Obsolete]
    public enum ExecuteClient
    {
        AsyncCPU,
        ComputeSheder,

        [Obsolete]
        CPU,
    }
    [Serializable]
    public struct TraiangleIndex : IEnumerable<int>
    {
        public int zero;
        public int one;
        public int two;

        public TraiangleIndex(int zero, int one, int two)
        {
            this.zero = zero;
            this.one = one;
            this.two = two;
        }

        public int this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: { return zero; }
                    case 1: { return one; }
                    case 2: { return two; }
                    default: throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (i)
                {
                    case 0: { zero = value; break; }
                    case 1: { one = value; break; }
                    case 2: { two = value; break; }
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        public IEnumerator<int> GetEnumerator()
        {
            return ToList().GetEnumerator();
        }

        public int[] ToArray()
        {
            return new int[3] { zero, one, two };
        }
        public List<int> ToList()
        {
            return new List<int> { zero, one, two };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ToArray().GetEnumerator();
        }

        public List<T> GetTraiangle<T>(List<T> List)
        {
            return new List<T> { List[zero], List[one], List[two] };
        }
    }
}
#endif