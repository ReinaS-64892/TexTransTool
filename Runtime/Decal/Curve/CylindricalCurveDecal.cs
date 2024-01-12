using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransTool.Decal.Cylindrical;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransCore.TransTextureCore;

namespace net.rs64.TexTransTool.Decal.Curve
{
    [AddComponentMenu("TexTransTool/OtherDecal/Cylindrical/Unfinished/TTT CylindricalCurveDecal")]
    internal class CylindricalCurveDecal : CurveDecal
    {
        public CylindricalCoordinatesSystem CylindricalCoordinatesSystem;
        public bool FilteredBackSide = true;

        public RollMode RollMode = RollMode.WorldUp;


        public BezierCurve BezierCurve => new BezierCurve(Segments, RollMode);

        public override Dictionary<Material, Dictionary<string, RenderTexture>> CompileDecal(ITextureManager textureManager, Dictionary<Material, Dictionary<string, RenderTexture>> decalCompiledRenderTextures = null)
        {
            TextureWrap texWarpRange = TextureWrap.NotWrap;
            if (IsTextureWarp)
            {
                texWarpRange = new TextureWrap(texWarpRange.Mode, TextureWarpRange);
            }

            decalCompiledRenderTextures = decalCompiledRenderTextures == null ? new Dictionary<Material, Dictionary<string, RenderTexture>>() : decalCompiledRenderTextures;


            int count = 0;
            foreach (var quad in BezierCurve.GetQuad(LoopCount, Size, CurveStartOffset))
            {
                var targetDecalTexture = DecalTexture;
                if (UseFirstAndEnd)
                {
                    if (count == 0)
                    {
                        targetDecalTexture = FirstTexture;
                    }
                    else if (count == LoopCount - 1)
                    {
                        targetDecalTexture = EndTexture;
                    }
                }
                RenderTexture mulDecalTexture = GetMultipleDecalTexture(textureManager, targetDecalTexture, Color);

                foreach (var Renderer in TargetRenderers)
                {
                    var CCSSpace = new CCSSpace(CylindricalCoordinatesSystem, quad);
                    var CCSfilter = new CCSFilter(GetFilers());


                    DecalUtility.CreateDecalTexture<CCSSpace, Vector2>(Renderer,
                                                decalCompiledRenderTextures,
                                                mulDecalTexture,
                                                CCSSpace,
                                                CCSfilter,
                                                TargetPropertyName,
                                                textureWarp: texWarpRange,
                                                defaultPadding: Padding,
                                                highQualityPadding: HighQualityPadding
                                                );

                    count += 1;
                }
                RenderTexture.ReleaseTemporary(mulDecalTexture);
            }

            return decalCompiledRenderTextures;
        }
        public List<TriangleFilterUtility.ITriangleFiltering<CCSSpace>> GetFilers()
        {
            var filters = new List<TriangleFilterUtility.ITriangleFiltering<CCSSpace>>
            {
                new CCSFilter.BorderOnPolygonStruct(150),
                new CCSFilter.OutOfPerigonStruct(PolygonCulling.Edge, OutOfRangeOffset, false)
            };

            return filters;
        }



        private void OnDrawGizmosSelected()
        {
            DrawerGizmo();
        }

        private void OnDrawGizmos()
        {
            if (DrawGizmoAlways) DrawerGizmo();
        }

        protected virtual void DrawerGizmo()
        {
            if (!IsPossibleSegments) return;
            Gizmos.color = Color.black;
            var quads = BezierCurve.GetQuad(LoopCount, Size, CurveStartOffset);
            GizmosUtility.DrawGizmoQuad(quads);
            GizmosUtility.DrawGizmoLine(Segments.ConvertAll(i => i.position));
            GizmosUtility.DrawGizmoLine(BezierCurve.GetLine());


            var bej = BezierCurve;
            quads.ForEach(i => i.ForEach(j =>
            {
                var ccsPint = CylindricalCoordinatesSystem.GetCCSPoint(j);
                var pos = CylindricalCoordinatesSystem.GetWorldPoint(new Vector3(ccsPint.x, ccsPint.y, 0));
                Gizmos.DrawLine(j, pos);
            }));

        }
    }
}