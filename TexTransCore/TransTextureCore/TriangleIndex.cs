using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace net.rs64.TexTransCore.TransTextureCore
{
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