#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using static Rs64.TexTransTool.Decal.DecalUtil;
#if VRC_BASE
using VRC.SDKBase;
#endif

namespace Rs64.TexTransTool.Decal.Curve.Cylindrical
{
    [AddComponentMenu("TexTransTool/Experimental/CylindricalCurveDecal")]
    public class CylindricalCurveDecal : CurveDecal
#if VRC_BASE
    , IEditorOnly
#endif
    {
        public CylindricalCoordinatesSystem CylindricalCoordinatesSystem;
        public bool FiltedBackSide = true;

        public BezierCurve BezierCurve => new BezierCurve(Segments);

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
            var Quads = BezierCurve.GetQuad(LoopCount, Size);
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
            if (_IsAppry) return;
            if (!IsPossibleCompile) return;

            Vector2? TexRenage = null;
            if (IsTextureStreach) TexRenage = TextureStreathRenge;

            var DictCompiledTextures = new List<Dictionary<Material, List<Texture2D>>>();
            int Count = 0;
            foreach (var Quad in BezierCurve.GetQuad(LoopCount, Size))
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
                    DictCompiledTextures.Add(DecalUtil.CreatDecalTexture(Renderer, TargetDecalTexture,
                    I => ComvartSpace(Quad, I),
                    TargetPropatyName,
                    TextureOutRenge: TexRenage,
                    TrainagleFilters: GetFiltarings(),
                    DefoaltPading: DefaultPading,
                    TexWrapMode: TexWrapMode.Stretch
                    ));
                }
                Count += 1;
            }

            var MatTexDict = ZipAndBlendTextures(DictCompiledTextures);
            var TextureList = Utils.GeneratTexturesList(Utils.GetMaterials(TargetRenderers), MatTexDict);
            SetContainer(TextureList);
        }

        private List<Vector3> ComvartSpace(List<Vector3> Quad, List<Vector3> Vartexs)
        {
            var LoaclPases = CylindricalCoordinatesSystem.VartexsConvertCCS(Quad, Vartexs, true);
            var Normalaized = DecalUtil.QuadNormaliz(LoaclPases.Item1.ConvertAll(i => (Vector2)i), LoaclPases.Item2.ConvertAll(i => (Vector2)i));
            return Utils.ZipListVector3(Normalaized, LoaclPases.Item2.ConvertAll(i => i.z));
        }

        public override List<DecalUtil.Filtaring> GetFiltarings()
        {
            List<DecalUtil.Filtaring> Filters = new List<DecalUtil.Filtaring>();

            if (FiltedBackSide)
            {
                Filters.Add((i, i2) => DecalUtil.SideChek(i, i2, true));
                Filters.Add((i, i2) => DecalUtil.OutOfPorigonEdgeBase(i, i2, 1 + OutOfRangeOffset, 0 - OutOfRangeOffset, true));
            }
            else
            {
                Filters.Add((i, i2) => DecalUtil.OutOfPorigonVartexBase(i, i2, 1 + OutOfRangeOffset, 0 - OutOfRangeOffset, true));
            }

            return Filters;
        }
    }
}
#endif