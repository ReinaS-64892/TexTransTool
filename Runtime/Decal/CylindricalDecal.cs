#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using Rs64.TexTransTool.Decal.Cylindrical;

namespace Rs64.TexTransTool.Decal
{
    [AddComponentMenu("TexTransTool/CylindricalDecal")]
    public class CylindricalDecal : AbstractDecal
    {
        public CylindricalCoordinatesSystem cylindricalCoordinatesSystem;
        public bool FixedAspect = true;
        public Vector2 Scale = Vector2.one;
        public bool SideChek = true;
        public float OutOfRangeOffset = 1f;
        public float FarCulling = 1f;
        public float NierCullingOffSet = 1f;
        public override void Compile()
        {
            if (_IsApply) return;
            if (!IsPossibleCompile) return;

            var DictCompiledTextures = new List<Dictionary<Material, List<Texture2D>>>();


            var PPSSpase = new CCSSpace(cylindricalCoordinatesSystem, GetQuad());
            var PPSFilter = new CCSFilter(GetFilters());


            TargetRenderers.ForEach(i => DictCompiledTextures.Add(DecalUtil.CreatDecalTexture(
                                                i,
                                                DecalTexture,
                                                PPSSpase,
                                                PPSFilter,
                                                TargetPropatyName
                                                )
                                        ));

            var MatTexDict = ZipAndBlendTextures(DictCompiledTextures);
            var TextureList = Utils.GeneratTexturesList(Utils.GetMaterials(TargetRenderers), MatTexDict);
            TextureList.ForEach(Tex => { if (Tex != null) Tex.name = "DecalTexture"; });
            Container.DecalCompiledTextures = TextureList;

            Container.IsPossibleApply = true;
        }

        private List<DecalUtil.Filtaring<CCSSpace>> GetFilters()
        {
            var Filters = new List<DecalUtil.Filtaring<CCSSpace>>();
            Filters.Add((i, i2) => CylindricalCoordinatesSystem.BorderOnPorygon(i, i2.CCSvarts));
            Filters.Add((i, i2) => DecalUtil.OutOfPorigonEdgeBase(i, i2.QuadNormalizedVarts, 1 + OutOfRangeOffset, 0 - OutOfRangeOffset, false));

            var ThisCCSZ = cylindricalCoordinatesSystem.GetCCSPoint(transform.position).z;

            Filters.Add((i, i2) => DecalUtil.FarClip(i, i2.QuadNormalizedVarts, NierCullingOffSet + ThisCCSZ, false));
            Filters.Add((i, i2) => DecalUtil.NerClip(i, i2.QuadNormalizedVarts, Mathf.Max(ThisCCSZ - FarCulling, 0f), false));


            if (SideChek) { Filters.Add((i, i2) => DecalUtil.SideChek(i, i2.QuadNormalizedVarts)); }

            return Filters;
        }

        public static readonly Vector3[] LocalQuad = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0),
        };

        public List<Vector3> GetQuad()
        {
            var Matrix = transform.localToWorldMatrix;
            var WorldSpaseQuad = new List<Vector3>(4);
            foreach (var i in LocalQuad)
            {
                WorldSpaseQuad.Add(Matrix.MultiplyPoint(i));
            }
            return WorldSpaseQuad;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.black;
            var Matrix = Matrix4x4.identity;
            Gizmos.matrix = Matrix;

            var CenterPos = Vector3.zero;

            var Quad = GetQuad();

            foreach (var FromPoint in Quad)
            {
                var CCSPoint = cylindricalCoordinatesSystem.GetCCSPoint(FromPoint);
                CCSPoint.z = Mathf.Max(CCSPoint.z - FarCulling, 0f);
                var OffSetToPoint = cylindricalCoordinatesSystem.GetWorldPoint(CCSPoint);

                var CCSFromPoint = cylindricalCoordinatesSystem.GetCCSPoint(FromPoint);
                CCSFromPoint.z += NierCullingOffSet;
                var OffSetFromPoint = cylindricalCoordinatesSystem.GetWorldPoint(CCSFromPoint);

                Gizmos.DrawLine(OffSetFromPoint, OffSetToPoint);
            }

            for (int Count = 0; 4 > Count; Count += 1)
            {
                (var From, var To) = GetEdge(Quad, Count);
                Gizmos.DrawLine(From, To);
            }


        }
        public static (Vector3, Vector3) GetEdge(IReadOnlyList<Vector3> Quad, int Count)
        {
            switch (Count)
            {
                default:
                case 0:
                    {
                        return (Quad[0], Quad[1]);
                    }
                case 1:
                    {
                        return (Quad[0], Quad[2]);
                    }
                case 2:
                    {
                        return (Quad[2], Quad[3]);
                    }
                case 3:
                    {
                        return (Quad[1], Quad[3]);
                    }
            }
        }

        public override void ScaleApply()
        {
            ScaleApply(new Vector3(Scale.x, Scale.y, 1), FixedAspect);
        }
    }
}
#endif
