using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Runtime.InteropServices;

namespace net.rs64.TexTransCore.TransTextureCore
{
    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    internal struct TriangleIndex : IEnumerable<int>, IEquatable<TriangleIndex>
    {
        [FieldOffset(0)] public int zero;
        [FieldOffset(4)] public int one;
        [FieldOffset(8)] public int two;

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
            return obj is TriangleIndex other && Equals(other);
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

        public bool Equals(TriangleIndex other)
        {
            return zero == other.zero && one == other.one && two == other.two;
        }
    }
    [StructLayout(LayoutKind.Explicit)]
    internal struct Triangle : IEnumerable<Vector3>
    {
        [FieldOffset(0)] public Vector3 zero;
        [FieldOffset(12)] public Vector3 one;
        [FieldOffset(24)] public Vector3 two;

        public Triangle(TriangleIndex TriIndex, List<Vector3> vector3s)
        {
            zero = vector3s[TriIndex.zero];
            one = vector3s[TriIndex.one];
            two = vector3s[TriIndex.two];
        }
        public Triangle(TriangleIndex TriIndex, Vector3[] vector3s)
        {
            zero = vector3s[TriIndex.zero];
            one = vector3s[TriIndex.one];
            two = vector3s[TriIndex.two];
        }

        public Vector4 Cross(Vector3 TargetPoint)
        {
            var w = Vector3.Cross(two - one, TargetPoint - one).z;
            var u = Vector3.Cross(zero - two, TargetPoint - two).z;
            var v = Vector3.Cross(one - zero, TargetPoint - zero).z;
            var wuv = Vector3.Cross(one - zero, two - zero).z;
            return new Vector4(w, u, v, wuv);
        }

        public Vector3 FromBCS(Vector3 SourceTBC)
        {
            var ConversionPos = Vector3.zero;
            ConversionPos += zero * SourceTBC.x;
            ConversionPos += one * SourceTBC.y;
            ConversionPos += two * SourceTBC.z;
            return ConversionPos;
        }

        public IEnumerator<Vector3> GetEnumerator()
        {
            return ToList().GetEnumerator();
        }

        public Vector3[] ToArray()
        {
            return new Vector3[3] { zero, one, two };
        }
        public List<Vector3> ToList()
        {
            return new List<Vector3>(3) { zero, one, two };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ToArray().GetEnumerator();
        }
    }
}