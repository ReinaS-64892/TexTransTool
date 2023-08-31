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

namespace net.rs64.TexTransTool
{
    public static class TransMapper
    {
        public const string TransMapperPath = "Packages/net.rs64.tex-trans-tool/Runtime/ComputeShaders/TransMapper.compute";
        [Obsolete]
        public static (Vector2, float) UVMapingCalculat(List<TriangleIndex> TrianglesToIndex, List<Vector2> TargetTexScaleTargetUV, List<Vector2> SourceUV, Vector2Int TargetPixsel, PaddingType paddingType, float DefaultDistans)
        {
            Vector2 Targetpixself = TargetPixsel;// + new Vector2(0.5f, 0.5f);
            float Distans = DefaultDistans;
            Vector2 SourceUVPosition = default;
            foreach (var TriangleToIndex in TrianglesToIndex)
            {
                var TargetUVTriangle = new List<Vector2> { TargetTexScaleTargetUV[TriangleToIndex[0]], TargetTexScaleTargetUV[TriangleToIndex[1]], TargetTexScaleTargetUV[TriangleToIndex[2]] };
                var CloseT =CrossTriangle(TargetUVTriangle, Targetpixself);
                float Distansnew;
                switch (paddingType)
                {
                    case PaddingType.EdgeBase:
                    default:
                        {
                            Distansnew = MinVector(DistansVartBase(TargetUVTriangle, Targetpixself));
                            break;
                        }
                    case PaddingType.VartexBase:
                        {
                            Distansnew = MinVector(DistansEdgeBase(TargetUVTriangle, Targetpixself));
                            break;
                        }
                }
                Distansnew *= -1;
                if (Distans < Distansnew)
                {
                    var SourceUVTriangle = new List<Vector2> { SourceUV[TriangleToIndex[0]], SourceUV[TriangleToIndex[1]], SourceUV[TriangleToIndex[2]] };
                    SourceUVPosition = FromBarycentricCoordinateSystem(SourceUVTriangle, ToBarycentricCoordinateSystem(CloseT));
                    Distans = Distansnew;
                }
            }
            return (SourceUVPosition, Distans);
        }
        public static Vector4 CrossTriangle(IList<Vector2> Triangle, Vector2 TargetPoint)
        {
            var w = Vector3.Cross(Triangle[2] - Triangle[1], TargetPoint - Triangle[1]).z;
            var u = Vector3.Cross(Triangle[0] - Triangle[2], TargetPoint - Triangle[2]).z;
            var v = Vector3.Cross(Triangle[1] - Triangle[0], TargetPoint - Triangle[0]).z;
            var wuv = TriangleArea(Triangle);
            return new Vector4(w, u, v, wuv);
        }
        public static float TriangleArea(IList<Vector2> Triangle)
        {
            return Vector3.Cross(Triangle[1] - Triangle[0], Triangle[2] - Triangle[0]).z;
        }

        public static Vector3 ToBarycentricCoordinateSystem(Vector4 CrossT)
        {
            var a = CrossT.x / CrossT.w;
            var b = CrossT.y / CrossT.w;
            var c = CrossT.z / CrossT.w;

            return new Vector3(a, b, c);
        }

        public static Vector2 FromBarycentricCoordinateSystem(IList<Vector2> Triangle, Vector3 SourceTBC)
        {
            var ConversionPos = Vector2.zero;
            ConversionPos += Triangle[0] * SourceTBC.x;
            ConversionPos += Triangle[1] * SourceTBC.y;
            ConversionPos += Triangle[2] * SourceTBC.z;
            return ConversionPos;
        }
        public static Vector3 FromBarycentricCoordinateSystem(IList<Vector3> Triangle, Vector3 SourceTBC)
        {
            var ConversionPos = Vector3.zero;
            ConversionPos += Triangle[0] * SourceTBC.x;
            ConversionPos += Triangle[1] * SourceTBC.y;
            ConversionPos += Triangle[2] * SourceTBC.z;
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

        public static Vector2 NearPointOnLine(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 ab = b - a;
            float Leng = ab.magnitude;
            ab.Normalize();
            float lp = Mathf.Clamp(Vector2.Dot(p - a, ab), 0, Leng);
            return a + (lp * ab);
        }
        public static Vector2 NearPoint(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 ab = b - a;
            ab.Normalize();
            float lp = Vector2.Dot(p - a, ab);
            return a + (lp * ab);
        }

        public static Vector3 DistansEdgeBase(List<Vector2> Triangle, Vector2 TargetPoint)
        {
            float DistansA = Vector2.Distance(NearPointOnLine(Triangle[0], Triangle[1], TargetPoint), TargetPoint);
            float DistansB = Vector2.Distance(NearPointOnLine(Triangle[1], Triangle[2], TargetPoint), TargetPoint);
            float DistansC = Vector2.Distance(NearPointOnLine(Triangle[2], Triangle[0], TargetPoint), TargetPoint);
            return new Vector3(DistansA, DistansB, DistansC);
        }

        public static float MinVector(Vector3 Vector)

        {
            return Mathf.Min(Vector.x, Mathf.Min(Vector.y, Vector.z));
        }

        public static TransMapData TransMapGenerateUseComputeShader(ComputeShader Shader, TransMapData TransMap, IReadOnlyList<TriangleIndex> TrianglesToIndex, IReadOnlyList<Vector2> TargetTexScaleTargetUV, IReadOnlyList<Vector2> SourceUV, PaddingType paddingType = PaddingType.EdgeBase)
        {
            if (Shader == null) Shader = AssetDatabase.LoadAssetAtPath<ComputeShader>(TransMapperPath);
            Vector2Int ThredGropSize = TransMap.Map.MapSize / 32;
            int karnelindex = -1;
            switch (paddingType)
            {
                case PaddingType.EdgeBase:
                    karnelindex = Shader.FindKernel("TransMapGeneratPaddingEdgeBase");
                    break;
                case PaddingType.VartexBase:
                    karnelindex = Shader.FindKernel("TransMapGeneratPaddingVartexBase");
                    break;
            }

            var ResBuffer = new ComputeBuffer((TransMap.Map.Array.Length), 12);
            ResBuffer.SetData(TransMap.Map.Array);
            Shader.SetBuffer(karnelindex, "Result", ResBuffer);

            var TriBufferSize = TrianglesToIndex.Count * 6;
            var TriBuffer = new ComputeBuffer(TriBufferSize, 8);
            var TriangleList = new List<Vector2>(TriBufferSize);
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
            Shader.SetBuffer(karnelindex, "Triangles", TriBuffer);
            Shader.SetInt("Size", TransMap.Map.MapSize.x);

            Shader.Dispatch(karnelindex, ThredGropSize.x, ThredGropSize.y, TrianglesToIndex.Count);

            ResBuffer.GetData(TransMap.Map.Array);

            ResBuffer.Release();
            TriBuffer.Release();

            return TransMap;
        }
        public static void UVtoTexScale(List<Vector2> UV, Vector2Int TexSize)
        {
            for (int i = 0; i < UV.Count; i++)
            {
                UV[i] = new Vector2(UV[i].x * TexSize.x, UV[i].y * TexSize.y);
            }
        }

        public static (Vector2, Vector2) BoxCal(List<Vector2> Triangles)
        {
            Vector2 min = new Vector2();
            Vector2 max = new Vector2();
            bool Farst = true;
            foreach (var tri in Triangles)
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

    public enum PaddingType
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
    public struct TriangleIndex : IEnumerable<int>
    {
        public int zero;
        public int one;
        public int two;

        public TriangleIndex(int zero, int one, int two)
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

        public List<T> GetTriangle<T>(List<T> List)
        {
            return new List<T> { List[zero], List[one], List[two] };
        }

        public override bool Equals(object obj)
        {
            return obj is TriangleIndex index &&
                   zero == index.zero &&
                   one == index.one &&
                   two == index.two;
        }

        public override int GetHashCode()
        {
            int hashCode = 662952323;
            hashCode = hashCode * -1521134295 + zero.GetHashCode();
            hashCode = hashCode * -1521134295 + one.GetHashCode();
            hashCode = hashCode * -1521134295 + two.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(TriangleIndex left, TriangleIndex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TriangleIndex left, TriangleIndex right)
        {
            return !(left == right);
        }
    }
}
#endif
