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
    }
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
    }
}
