#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace net.rs64.TexTransCore
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ColorWOAlpha
    {
        public float R;
        public float G;
        public float B;
        public float[] ToArray() { return new float[] { R, G, B }; }
    }
    /// <summary>
    /// ガンマ色空間の色を表現する
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Color
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public Color(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static Color Zero => new Color(0, 0, 0, 0);

        public float[] ToArray() { return new float[] { R, G, B, A }; }

        public override string ToString()
        {
            return base.ToString() + $"{R}-{G}-{B}-{A}";
        }
    }
    /// <summary>
    /// System.Numeric の Vector4 は W が先頭にあるためそれを回避するための存在
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TTVector4
    {
        public float X;
        public float Y;
        public float Z;
        public float W;
        public TTVector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        public override string ToString()
        {
            return $"{X}-{Y}-{Z}-{W}";
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z, W);
        }
        public override bool Equals(object obj)
        {
            var o = (TTVector4)obj;
            if (X != o.X) { return false; }
            if (Y != o.Y) { return false; }
            if (Z != o.Z) { return false; }
            if (W != o.W) { return false; }
            return true;
        }
    }


    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    sealed class Range : System.Attribute
    {
        public float Min;
        public float Max;
        public Range(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

    public static class TTMath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NormalizeOf4Multiple(int v)
        {
            return v % 4 is not 0 ? v + (4 - v % 4) : v;
        }
    }
}
