#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Rs64.TexTransTool.Decal
{
    //[AddComponentMenu("TexTransTool/SimpleDecal")]
    public class SimpleDecal : AbstractDecal
    {
        public Vector2 Scale = Vector2.one;
        public float MaxDistans = 1;
        public bool AdvansdMode;
        public bool FixedAspect = true;
        public bool SideChek = true;
        public PolygonCaling PolygonCaling = PolygonCaling.Vartex;
        public virtual void ScaleAppry()
        {
            if (DecalTexture != null && FixedAspect)
            {
                transform.localScale = new Vector3(Scale.x, Scale.x * ((float)DecalTexture.height / (float)DecalTexture.width), MaxDistans);
            }
            else
            {
                transform.localScale = new Vector3(Scale.x, FixedAspect ? Scale.x : Scale.y, MaxDistans);
            }
        }

        public override List<DecalUtil.Filtaring> GetFiltarings()
        {
            List<DecalUtil.Filtaring> Filters = new List<DecalUtil.Filtaring>();

            Filters.Add((i, i2) => DecalUtil.FarClip(i, i2, 1f, false));
            Filters.Add((i, i2) => DecalUtil.NerClip(i, i2, 0f, true));
            if (SideChek) Filters.Add(DecalUtil.SideChek);
            switch (PolygonCaling)
            {
                default:
                case PolygonCaling.Vartex:
                    {
                        Filters.Add((i, i2) => DecalUtil.OutOfPorigonVartexBase(i, i2, 1, 0, true)); break;
                    }
                case PolygonCaling.Edge:
                    {
                        Filters.Add((i, i2) => DecalUtil.OutOfPorigonEdgeBase(i, i2, 1, 0, true)); break;
                    }
                case PolygonCaling.EdgeAndCenterRay:
                    {
                        Filters.Add((i, i2) => DecalUtil.OutOfPorigonEdgeEdgeAndCenterRayCast(i, i2, 1, 0, true)); break;
                    }
            }

            return Filters;
        }
        public override void Compile()
        {
            if (_IsAppry) return;
            if (!IsPossibleCompile) return;

            var DictCompiledTextures = new List<Dictionary<Material, List<Texture2D>>>();

            TargetRenderers.ForEach(i => DictCompiledTextures.Add(DecalUtil.CreatDecalTexture(
                                                i,
                                                DecalTexture,
                                                ComvartSpace,
                                                TargetPropatyName,
                                                TrainagleFilters: GetFiltarings())
                                        ));

            var MatTexDict = ZipAndBlendTextures(DictCompiledTextures, BlendType.Normal);
            var TextureList = Utils.GeneratTexturesList(Utils.GetMaterials(TargetRenderers), MatTexDict);
            SetContainer(TextureList);
        }

        public void AdvansdModeReset()
        {
            TargetPropatyName = "_MainTex";
            AdvansdMode = false;
            SideChek = true;
            PolygonCaling = PolygonCaling.Vartex;
        }



        [NonSerialized] public Material DisplayDecalMat;
        public Color GizmoColoro = new Color(0, 0, 0, 1);
        [NonSerialized] public Mesh Quad;

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = GizmoColoro;
            var Matrix = transform.localToWorldMatrix;

            Gizmos.matrix = Matrix;

            var CenterPos = Vector3.zero;
            Gizmos.DrawWireCube(CenterPos + new Vector3(0, 0, 0.5f), new Vector3(1, 1, 1));//基準となる四角形

            if (DecalTexture != null)
            {
                if (DisplayDecalMat == null || Quad == null) GizmInstans();
                DisplayDecalMat.SetPass(0);
                Graphics.DrawMeshNow(Quad, Matrix);
            }
            Gizmos.DrawLine(CenterPos, CenterPos + new Vector3(0, 0, MaxDistans / 2));//前方向の表示
        }

        public void GizmInstans()
        {
            DisplayDecalMat = new Material(Shader.Find("Hidden/DisplayDecalTexture"));
            DisplayDecalMat.mainTexture = DecalTexture;
            Quad = AssetDatabase.LoadAllAssetsAtPath("Library/unity default resources").ToList().Find(i => i.name == "Quad") as Mesh;

        }
    }
}



#endif