#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransTool.Decal.Cylindrical;

namespace net.rs64.TexTransTool.Decal.Curve
{
    [AddComponentMenu("TexTransTool/Experimental/CylindricalCurveDecal")]
    public class CylindricalCurveDecal : CurveDecal
    {
        public CylindricalCoordinatesSystem CylindricalCoordinatesSystem;
        public bool FilteredBackSide = true;

        public RollMode RollMode = RollMode.WorldUp;


        public BezierCurve BezierCurve => new BezierCurve(Segments, RollMode);

        public override Dictionary<Texture2D, Texture> CompileDecal()
        {


            Vector2? texWarpRange = null;
            if (IsTextureWarp)
            {
                texWarpRange = TextureWarpRange;
            }

            Dictionary<Texture2D, RenderTexture> fastDictCompiledTextures = FastMode ? new Dictionary<Texture2D, RenderTexture>() : null;
            List<Dictionary<Texture2D, List<Texture2D>>> slowDictCompiledTextures = FastMode ? null : new List<Dictionary<Texture2D, List<Texture2D>>>();

            var decalCompiledTextures = new Dictionary<Texture2D, Texture>();
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
                foreach (var Renderer in TargetRenderers)
                {
                    var CCSSpace = new CCSSpace(CylindricalCoordinatesSystem, quad);
                    var CCSfilter = new CCSFilter(GetFilers());

                    if (FastMode)
                    {
                        DecalUtil.CreateDecalTexture(Renderer,
                                                    fastDictCompiledTextures,
                                                    targetDecalTexture,
                                                    CCSSpace,
                                                    CCSfilter,
                                                    TargetPropertyName,
                                                    TextureOutRange: texWarpRange,
                                                    DefaultPadding: Padding
                                                    );
                    }
                    else
                    {
                        slowDictCompiledTextures.Add(DecalUtil.CreateDecalTextureCS(Renderer,
                                                                             targetDecalTexture,
                                                                             CCSSpace,
                                                                             CCSfilter,
                                                                             TargetPropertyName,
                                                                             TextureOutRange: texWarpRange,
                                                                             DefaultPadding: Padding
                                                                            ));
                    }

                    count += 1;
                }
            }

            if (FastMode)
            {
                foreach (var Texture in fastDictCompiledTextures)
                {
                    decalCompiledTextures.Add(Texture.Key, Texture.Value);
                }
            }
            else
            {
                var zipDict = Utils.ZipToDictionaryOnList(slowDictCompiledTextures);
                foreach (var texture in zipDict)
                {
                    var compiledTex = TextureLayerUtil.BlendTextureUseComputeShader(null, texture.Value, BlendType.AlphaLerp);
                    compiledTex.Apply();
                    decalCompiledTextures.Add(texture.Key, compiledTex);
                }
            }

            return decalCompiledTextures;
        }

        public List<TriangleFilterUtils.ITriangleFiltering<CCSSpace>> GetFilers()
        {
            var filters = new List<TriangleFilterUtils.ITriangleFiltering<CCSSpace>>
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
#endif
