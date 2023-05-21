using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using static Rs64.TexTransTool.Decal.DecalUtil;

namespace Rs64.TexTransTool.Decal.Curve.Cylindrical
{
    public class CylindricalCurveDecal : CurveDecal
    {
        public CylindricalCoordinatesSystem CylindricalCoordinatesSystem;

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

        public override void Appry(MaterialDomain avatarMaterialDomain = null)
        {
            if (!IsPossibleAppry) return;
            if (_IsAppry) return;
            _IsAppry = true;
            if (avatarMaterialDomain == null) avatarMaterialDomain = new MaterialDomain(TargetRenderers);

            var MatAndTexs = Container.DecalCompiledTextures;
            var GeneretaMatAndTex = new List<MatAndTex>();
            foreach (var MatAndTex in MatAndTexs)
            {
                var Mat = MatAndTex.Material;
                var Tex = MatAndTex.Texture;
                if (Mat.GetTexture(TargetPropatyName) is Texture2D OldTex)
                {
                    var Newtex = TextureLayerUtil.BlendTextureUseComputeSheder(null, OldTex, Tex, BlendType);
                    var SavedTex = AssetSaveHelper.SaveAsset(Newtex);

                    var NewMat = Instantiate<Material>(Mat);
                    NewMat.SetTexture(TargetPropatyName, SavedTex);

                    GeneretaMatAndTex.Add(new MatAndTex(NewMat, SavedTex));
                }
            }

            Container.DecaleBlendTexteres = GeneretaMatAndTex;
            Container.GenereatMaterials = GeneretaMatAndTex.ConvertAll(i => i.Material);

            avatarMaterialDomain.SetMaterials(Container.DistMaterials, Container.GenereatMaterials);
            _IsAppry = true;
        }

        public override void Compile()
        {
            if (_IsAppry) return;
            if (!IsPossibleCompile) return;

            Vector2? TexRenage = null;
            if (IsTextureStreach) TexRenage = TextureStreathRenge;

            var DictCompiledTextures = new List<Dictionary<Material, List<Texture2D>>>();
            foreach (var Quad in BezierCurve.GetQuad(LoopCount, Size))
            {
                foreach (var Renderer in TargetRenderers)
                {
                    DictCompiledTextures.Add(DecalUtil.CreatDecalTexture(Renderer, DecalTexture,
                    I => ComvartSpace(Quad, I),
                    TargetPropatyName,
                    TextureOutRenge: TexRenage,
                    TrainagleFilters: GetFiltarings(),
                    DefoaltPading: DefaultPading
                    ));
                }
            }

            var DictCompiledTexture = Utils.ZipToDictionaryOnList(DictCompiledTextures);
            var MatAndTexs = new List<MatAndTex>();
            foreach (var kvp in DictCompiledTexture)
            {
                var Mat = kvp.Key;
                var Texs = kvp.Value;
                var Tex = TextureLayerUtil.BlendTexturesUseComputeSheder(null, Texs, BlendType);
                MatAndTexs.Add(new MatAndTex(Mat, Tex));
            }
            if (Container == null) { Container = ScriptableObject.CreateInstance<DecalDataContainer>(); AssetSaveHelper.SaveAsset(Container); }
            Container.DecalCompiledTextures = MatAndTexs;
            Container.DistMaterials = MatAndTexs.ConvertAll<Material>(i => i.Material);
        }

        public override void Revart(MaterialDomain avatarMaterialDomain = null)
        {
            if (!_IsAppry) return;
            _IsAppry = false;
            if (avatarMaterialDomain == null) avatarMaterialDomain = new MaterialDomain(TargetRenderers);

            avatarMaterialDomain.SetMaterials(Container.GenereatMaterials, Container.DistMaterials);
        }

        private List<Vector3> ComvartSpace(List<Vector3> Quad, List<Vector3> Vartexs)
        {
            var LoaclPases = CylindricalCoordinatesSystem.VartexsConvertCCS(Quad, Vartexs, true);
            var Normalaized = DecalUtil.QuadNormaliz(LoaclPases.Item1.ConvertAll(i => (Vector2)i), LoaclPases.Item2.ConvertAll(i => (Vector2)i));
            return Utils.ZipListVector3(Normalaized, LoaclPases.Item2.ConvertAll(i => i.z));
        }

        public List<DecalUtil.Filtaring> GetFiltarings()
        {
            List<DecalUtil.Filtaring> Filters = new List<DecalUtil.Filtaring>();

            Filters.Add((i, i2) => DecalUtil.OutOfPorigonVartexBase(i, i2, 1 + OutOfRangeOffset, 0 - OutOfRangeOffset, false));
            Filters.Add((i, i2) => DecalUtil.OutOfPorigonEdgeBase(i, i2, 1, 0, true));


            return Filters;
        }


    }
}