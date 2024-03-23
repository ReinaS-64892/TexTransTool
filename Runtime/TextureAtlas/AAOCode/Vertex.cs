using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.AAOCode
{
    internal class Vertex
    {
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public Vector4 Tangent { get; set; } = new Vector4(1, 0, 0, 1);
        public Vector4 TexCoord0 { get; set; }
        public Vector4 TexCoord1 { get; set; }
        public Vector4 TexCoord2 { get; set; }
        public Vector4 TexCoord3 { get; set; }
        public Vector4 TexCoord4 { get; set; }
        public Vector4 TexCoord5 { get; set; }
        public Vector4 TexCoord6 { get; set; }
        public Vector4 TexCoord7 { get; set; }

        public Color32 Color { get; set; } = new Color32(0xff, 0xff, 0xff, 0xff);

        // SkinnedMesh related
        public List<(Bone bone, float weight)> BoneWeights = new List<(Bone, float)>();

        // Each frame must sorted increasingly
        public readonly Dictionary<string, BlendShapeFrame[]> BlendShapes =
            new Dictionary<string, BlendShapeFrame[]>();

        public readonly struct BlendShapeFrame
        {
            public readonly float Weight;
            public readonly Vector3 Position;
            public readonly Vector3 Normal;
            public readonly Vector3 Tangent;

            public BlendShapeFrame(float weight, Vector3 position, Vector3 normal, Vector3 tangent)
            {
                Position = position;
                Normal = normal;
                Tangent = tangent;
                Weight = weight;
            }

            public void Deconstruct(out float weight, out Vector3 position, out Vector3 normal, out Vector3 tangent)
            {
                weight = Weight;
                position = Position;
                normal = Normal;
                tangent = Tangent;
            }
        }

        public Vector4 GetTexCoord(int index)
        {
            switch (index)
            {
                // @formatter off
                case 0: return TexCoord0;
                case 1: return TexCoord1;
                case 2: return TexCoord2;
                case 3: return TexCoord3;
                case 4: return TexCoord4;
                case 5: return TexCoord5;
                case 6: return TexCoord6;
                case 7: return TexCoord7;
                default: throw new IndexOutOfRangeException("TexCoord index");
                    // @formatter on
            }
        }

        public void SetTexCoord(int index, Vector4 value)
        {
            switch (index)
            {
                // @formatter off
                case 0: TexCoord0 = value; break;
                case 1: TexCoord1 = value; break;
                case 2: TexCoord2 = value; break;
                case 3: TexCoord3 = value; break;
                case 4: TexCoord4 = value; break;
                case 5: TexCoord5 = value; break;
                case 6: TexCoord6 = value; break;
                case 7: TexCoord7 = value; break;
                default: throw new IndexOutOfRangeException("TexCoord index");
                    // @formatter on
            }
        }

        public bool TryGetBlendShape(string name, float weight, out Vector3 position, out Vector3 normal,
            out Vector3 tangent, bool getDefined = false)
        {
            if (!BlendShapes.TryGetValue(name, out var frames))
            {
                position = default;
                normal = default;
                tangent = default;
                return false;
            }

            if (frames.Length == 0)
            {
                position = default;
                normal = default;
                tangent = default;
                return false;
            }

            if (!getDefined && Mathf.Abs(weight) <= 0.0001f && ZeroForWeightZero())
            {
                position = Vector3.zero;
                normal = Vector3.zero;
                tangent = Vector3.zero;
                return true;
            }

            bool ZeroForWeightZero()
            {
                if (frames.Length == 1) return true;
                var first = frames.First();
                var end = frames.Last();

                // both weight are same sign, zero for 0 weight
                if (first.Weight <= 0 && end.Weight <= 0) return true;
                if (first.Weight >= 0 && end.Weight >= 0) return true;

                return false;
            }

            if (frames.Length == 1)
            {
                // simplest and likely
                var frame = frames[0];
                var ratio = weight / frame.Weight;
                position = frame.Position * ratio;
                normal = frame.Normal * ratio;
                tangent = frame.Tangent * ratio;
                return true;
            }
            else
            {
                // multi frame
                var (lessFrame, greaterFrame) = FindFrame();
                var ratio = InverseLerpUnclamped(lessFrame.Weight, greaterFrame.Weight, weight);

                position = Vector3.LerpUnclamped(lessFrame.Position, greaterFrame.Position, ratio);
                normal = Vector3.LerpUnclamped(lessFrame.Normal, greaterFrame.Normal, ratio);
                tangent = Vector3.LerpUnclamped(lessFrame.Tangent, greaterFrame.Tangent, ratio);
                return true;
            }

            (BlendShapeFrame, BlendShapeFrame) FindFrame()
            {
                var firstFrame = frames[0];
                var lastFrame = frames.Last();

                if (firstFrame.Weight > 0 && weight < firstFrame.Weight)
                {
                    // if all weights are positive and the weight is less than first weight: lerp 0..first
                    return (default, firstFrame);
                }

                if (lastFrame.Weight < 0 && weight > lastFrame.Weight)
                {
                    // if all weights are negative and the weight is more than last weight: lerp last..0
                    return (lastFrame, default);
                }

                // otherwise, lerp between two surrounding frames OR nearest two frames

                for (var i = 1; i < frames.Length; i++)
                {
                    if (weight <= frames[i].Weight)
                        return (frames[i - 1], frames[i]);
                }

                return (frames[frames.Length - 2], frames[frames.Length - 1]);
            }

            float InverseLerpUnclamped(float a, float b, float value) => (value - a) / (b - a);
        }

        public Vertex()
        {
        }

        private Vertex(Vertex vertex)
        {
            Position = vertex.Position;
            Normal = vertex.Normal;
            Tangent = vertex.Tangent;
            TexCoord0 = vertex.TexCoord0;
            TexCoord1 = vertex.TexCoord1;
            TexCoord2 = vertex.TexCoord2;
            TexCoord3 = vertex.TexCoord3;
            TexCoord4 = vertex.TexCoord4;
            TexCoord5 = vertex.TexCoord5;
            TexCoord6 = vertex.TexCoord6;
            TexCoord7 = vertex.TexCoord7;
            Color = vertex.Color;
            BoneWeights = vertex.BoneWeights.ToList();
            BlendShapes = new Dictionary<string, BlendShapeFrame[]>(vertex.BlendShapes);
        }

        public Vertex Clone() => new Vertex(this);

    }

    public class Bone
    {
        public Matrix4x4 Bindpose;
        public Transform Transform;

        public Bone(Matrix4x4 bindPose) : this(bindPose, null) { }
        public Bone(Matrix4x4 bindPose, Transform transform) => (Bindpose, Transform) = (bindPose, transform);
    }

    public enum TexCoordStatus
    {
        NotDefined = 0,
        Vector2 = 1,
        Vector3 = 2,
        Vector4 = 3,
    }
}
