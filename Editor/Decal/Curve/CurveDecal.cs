#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Pool;

namespace net.rs64.TexTransTool.Decal.Curve
{
    internal abstract class CurveDecal : AbstractDecal
    {
        public float Size = 0.5f;
        public uint LoopCount = 1;
        public float OutOfRangeOffset = 0f;
        public bool IsTextureWarp = true;
        public Vector2 TextureWarpRange = new Vector2(0, 0.05f);
        public List<CurveSegment> Segments = new List<CurveSegment>();
        public bool DrawGizmoAlways = false;
        public bool UseFirstAndEnd = false;
        public Texture2D DecalTexture;
        public Texture2D FirstTexture;
        public Texture2D EndTexture;
        public float CurveStartOffset;

        public bool IsPossibleSegments
        {
            get
            {
                if (Segments.Count <= 1 || Segments.Any(i => i == null)) { return false; }
                var segmentPosHash = HashSetPool<Vector3>.Get();
                foreach (var seg in Segments)
                {
                    if (!segmentPosHash.Add(seg.position))
                    {
                        HashSetPool<Vector3>.Release(segmentPosHash);
                        return false;
                    }
                }
                HashSetPool<Vector3>.Release(segmentPosHash);
                return true;
            }
        }
        public override bool IsPossibleApply => TargetRenderers.Any(i => i != null) && IsPossibleSegments;




    }
}
#endif
