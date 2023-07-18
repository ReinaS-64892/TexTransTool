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
    public class CylindricalCurveDecal : CurveDecal
#if VRC_BASE
    , IEditorOnly
#endif
    {
        public CylindricalCoordinatesSystem CylindricalCoordinatesSystem;
        public bool FiltedBackSide = true;

        public RoolMode RoolMode = RoolMode.WorldUp;


        public BezierCurve BezierCurve => new BezierCurve(Segments, RoolMode);

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

        public override void Compile()
        {
            if (_IsApply) return;
            if (!IsPossibleCompile) return;

            Vector2? TexRenage = null;
            TexWrapMode texWrapMode = TexWrapMode.NotWrap;
            if (IsTextureStreach)
            {
                TexRenage = TextureStreathRenge;
                texWrapMode = TexWrapMode.Stretch;
            }

            var DictCompiledTextures = new List<Dictionary<Material, List<Texture2D>>>();
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
                    DictCompiledTextures.Add(DecalUtil.CreatDecalTexture(Renderer,
                                                                         TargetDecalTexture,
                                                                         CCSspase,
                                                                         CCSfilter,
                                                                         TargetPropatyName,
                                                                         TextureOutRenge: TexRenage,
                                                                         DefoaltPading: DefaultPading,
                                                                         TexWrapMode: texWrapMode));
                }
                Count += 1;
            }

            var MatTexDict = ZipAndBlendTextures(DictCompiledTextures);
            var TextureList = Utils.GeneratTexturesList(Utils.GetMaterials(TargetRenderers), MatTexDict);
            Container.DecalCompiledTextures = TextureList;
            Container.IsPossibleApply = true;
        }
    }
}
#endif