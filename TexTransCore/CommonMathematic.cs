#nullable enable
using System.Runtime.InteropServices;

namespace net.rs64.TexTransCore
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Vector2
    {
        [FieldOffset(0)]
        public float X;
        [FieldOffset(4)]
        public float Y;
        public float[] ToArray() { return new float[] { X, Y }; }
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct Vector3
    {
        [FieldOffset(0)]
        public float X;
        [FieldOffset(4)]
        public float Y;
        [FieldOffset(8)]
        public float Z;
        public float[] ToArray() { return new float[] { X, Y, Z }; }
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct Vector4
    {
        [FieldOffset(0)]
        public float X;
        [FieldOffset(4)]
        public float Y;
        [FieldOffset(8)]
        public float Z;
        [FieldOffset(12)]
        public float W;

        public Vector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        public float[] ToArray() { return new float[] { X, Y, Z, W }; }
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct ColorWOAlpha
    {
        [FieldOffset(0)]
        public float R;
        [FieldOffset(4)]
        public float G;
        [FieldOffset(8)]
        public float B;
        public float[] ToArray() { return new float[] { R, G, B }; }
    }
    /// <summary>
    /// ガンマ色空間の色を表現する
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Color
    {
        [FieldOffset(0)]
        public float R;
        [FieldOffset(4)]
        public float G;
        [FieldOffset(8)]
        public float B;
        [FieldOffset(12)]
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
}
