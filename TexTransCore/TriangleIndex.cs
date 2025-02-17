#nullable enable
using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.InteropServices;
using System.Numerics;

namespace net.rs64.TexTransCore
{
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct TriangleIndex : IEnumerable<int>, IEquatable<TriangleIndex>
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
            yield return zero;
            yield return one;
            yield return two;
        }

        public int[] ToArray()
        {
            return new int[3] { zero, one, two };
        }
        public List<int> ToList(List<int>? output = null)
        {
            if (output == null) { return new List<int>(3) { zero, one, two }; }
            else
            {
                output.Clear();
                output.Add(zero);
                output.Add(one);
                output.Add(two);
                return output;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ToArray().GetEnumerator();
        }

        public List<T> GetTriangle<T>(List<T> vert, List<T>? output = null)
        {
            if (output == null) { return new List<T> { vert[zero], vert[one], vert[two] }; }
            else
            {
                output.Clear();
                output.Add(vert[zero]);
                output.Add(vert[one]);
                output.Add(vert[two]);
                return output;
            }
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

        public static IEnumerable<int> SelectMany(IEnumerable<TriangleIndex> ints) { foreach (var tri in ints) { for (var i = 0; 3 > i; i += 1) { yield return tri[i]; } } }
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct Triangle : IEnumerable<Vector3>
    {
        [FieldOffset(0)] public Vector3 zero;
        [FieldOffset(12)] public Vector3 one;
        [FieldOffset(24)] public Vector3 two;

        public Triangle(TriangleIndex TriIndex, Span<Vector3> vector3s)
        {
            zero = vector3s[TriIndex.zero];
            one = vector3s[TriIndex.one];
            two = vector3s[TriIndex.two];
        }

        public Triangle(Vector3 vector31, Vector3 vector32, Vector3 vector33)
        {
            zero = vector31;
            one = vector32;
            two = vector33;
        }

        public TTVector4 Cross(Vector3 TargetPoint)
        {
            var u = Vector3.Cross(two - one, TargetPoint - one).Z;
            var v = Vector3.Cross(zero - two, TargetPoint - two).Z;
            var w = Vector3.Cross(one - zero, TargetPoint - zero).Z;
            var uvw = Vector3.Cross(one - zero, two - zero).Z;
            return new TTVector4(u, v, w, uvw);
        }

        public Vector3 FromBCS(Vector3 SourceTBC)
        {
            var conversionPos = new Vector3(0, 0, 0);
            conversionPos += zero * SourceTBC.X;
            conversionPos += one * SourceTBC.Y;
            conversionPos += two * SourceTBC.Z;
            return conversionPos;
        }

        public IEnumerator<Vector3> GetEnumerator()
        {
            yield return zero;
            yield return one;
            yield return two;
        }

        public Vector3[] ToArray()
        {
            return new Vector3[3] { zero, one, two };
        }
        public List<Vector3> ToList(List<Vector3>? output = null)
        {
            if (output == null) { return new List<Vector3>(3) { zero, one, two }; }
            else
            {
                output.Clear();
                output.Add(zero);
                output.Add(one);
                output.Add(two);
                return output;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ToArray().GetEnumerator();
        }


        public Vector3 this[int i]
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
    }
}
