#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using Rs64.TexTransTool.Decal.Cylindrical;
#if VRC_BASE
using VRC.SDKBase;
#endif

namespace Rs64.TexTransTool.Decal.Curve
{
    [AddComponentMenu("TexTransTool/Experimental/CylindricalCurveDecal")]
    public class CylindricalCurveDecal : CurveDecal<CCSSpace>
#if VRC_BASE
    , IEditorOnly
#endif
    {
        public CylindricalCoordinatesSystem CylindricalCoordinatesSystem;
        public bool FiltedBackSide = true;

        public RoolMode RoolMode = RoolMode.WorldUp;


        public BezierCurve BezierCurve => new BezierCurve(Segments, RoolMode);

        public override CCSSpace GetSpaseConverter => throw new System.NotImplementedException();
        public override DecalUtil.ITraiangleFilter<CCSSpace> GetTraiangleFilter => throw new System.NotImplementedException();


        public override Dictionary<Texture2D, Texture> CompileDecal()
        {


            Vector2? TexRenage = null;
            if (IsTextureStreach)
            {
                TexRenage = TextureStreathRenge;
            }

            Dictionary<Texture2D, RenderTexture> FastDictCompiledTextures = FastMode ? new Dictionary<Texture2D, RenderTexture>() : null;
            List<Dictionary<Texture2D, List<Texture2D>>> SlowDictCompiledTextures = FastMode ? null : new List<Dictionary<Texture2D, List<Texture2D>>>();

            var DecalCompiledTextures = new Dictionary<Texture2D, Texture>();
            int Count = 0;
            foreach (var Quad in BezierCurve.GetQuad(LoopCount, Size, CurveStartOffset))
            {
                var TargetDecalTexture = DecalTexture;
                if (UseFirstAndEnd)
                {
                    if (Count == 0)
                    {
                        TargetDecalTexture = FirstTexture;
                    }
                    else if (Count == LoopCount - 1)
                    {
                        TargetDecalTexture = EndTexture;
                    }
                }
                foreach (var Renderer in TargetRenderers)
                {
                    var CCSspase = new CCSSpace(CylindricalCoordinatesSystem, Quad);
                    var CCSfilter = new CCSFilter(CCSFilter.DefaultFilter(OutOfRangeOffset));

                    if (FastMode)
                    {
                        DecalUtil.CreatDecalTexture(Renderer,
                                                    FastDictCompiledTextures,
                                                    TargetDecalTexture,
                                                    CCSspase,
                                                    CCSfilter,
                                                    TargetPropatyName,
                                                    TextureOutRenge: TexRenage,
                                                    DefoaltPading: DefaultPading
                                                    );
                    }
                    else
                    {
                        SlowDictCompiledTextures.Add(DecalUtil.CreatDecalTextureCS(Renderer,
                                                                             TargetDecalTexture,
                                                                             CCSspase,
                                                                             CCSfilter,
                                                                             TargetPropatyName,
                                                                             TextureOutRenge: TexRenage,
                                                                             DefoaltPading: DefaultPading
                                                                            ));
                    }

                    Count += 1;
                }
            }

            if (FastMode)
            {
                foreach (var Texture in FastDictCompiledTextures)
                {
                    DecalCompiledTextures.Add(Texture.Key, Texture.Value);
                }
            }
            else
            {
                var zipd = Utils.ZipToDictionaryOnList(SlowDictCompiledTextures);
                foreach (var Texture in zipd)
                {
                    DecalCompiledTextures.Add(Texture.Key, TextureLayerUtil.BlendTextureUseComputeSheder(null, Texture.Value, BlendType.AlphaLerp));
                }
            }

            return DecalCompiledTextures;
        }




        private void OnDrawGizmosSelected()
        {
            DrowGizmo();
        }

        private void OnDrawGizmos()
        {
            if (DorwGizmoAwiys) DrowGizmo();
        }

        protected virtual void DrowGizmo()
        {
            if (!IsPossibleSegments) return;
            Gizmos.color = Color.black;
            var Quads = BezierCurve.GetQuad(LoopCount, Size, CurveStartOffset);
            GizmosUtility.DrowGizmoQuad(Quads);
            GizmosUtility.DrowGimzLine(Segments.ConvertAll(i => i.position));
            GizmosUtility.DrowGimzLine(BezierCurve.GetLine());


            var bej = BezierCurve;
            Quads.ForEach(i => i.ForEach(j =>
            {
                var ccsp = CylindricalCoordinatesSystem.GetCCSPoint(j);
                var pos = CylindricalCoordinatesSystem.GetWorldPoint(new Vector3(ccsp.x, ccsp.y, 0));
                Gizmos.DrawLine(j, pos);
            }));

        }
    }
}
#endif