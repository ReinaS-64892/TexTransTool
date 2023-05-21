using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Rs64.TexTransTool.Decal.Curve
{
    public abstract class CurveDecal : AbstractDecal
    {
        public float Size = 0.5f;
        public uint LoopCount = 1;
        public float OutOfRangeOffset = 0f;
        public bool IsTextureStreach = true;
        public Vector2 TextureStreathRenge = new Vector2(0, 0.05f);
        public List<CurevSegment> Segments = new List<CurevSegment>();
        public bool DorwGizmoAwiys = false;

        public bool IsPossibleSegments => Segments.Count > 1 && !Segments.Any(i => i == null);
        public override bool IsPossibleCompile => base.IsPossibleCompile && IsPossibleSegments;




    }
}