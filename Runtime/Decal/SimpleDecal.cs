#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
#if VRC_BASE
using VRC.SDKBase;
#endif
namespace Rs64.TexTransTool.Decal
{
    [AddComponentMenu("TexTransTool/SimpleDecal")]
    [ExecuteInEditMode]
    public class SimpleDecal : AbstractDecal
#if VRC_BASE
    , IEditorOnly
#endif
    {
        public Vector2 Scale = Vector2.one;
        public float MaxDistans = 1;
        public bool AdvansdMode;
        public bool FixedAspect = true;
        public bool SideChek = true;
        public PolygonCaling PolygonCaling = PolygonCaling.Vartex;
        public virtual void ScaleApply()
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
            if (_IsApply) return;
            if (!IsPossibleCompile) return;

            var DictCompiledTextures = new List<Dictionary<Material, List<Texture2D>>>();

            TargetRenderers.ForEach(i => DictCompiledTextures.Add(DecalUtil.CreatDecalTexture(
                                                i,
                                                DecalTexture,
                                                ComvartSpace,
                                                TargetPropatyName,
                                                TrainagleFilters: GetFiltarings())
                                        ));

            var MatTexDict = ZipAndBlendTextures(DictCompiledTextures, BlendType.AlphaLerp);
            var TextureList = Utils.GeneratTexturesList(Utils.GetMaterials(TargetRenderers), MatTexDict);
            TextureList.ForEach(Tex => Tex.name = "DecalTexture");
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

        [SerializeField] protected bool _IsRealTimePreview = false;
        public bool IsRealTimePreview => _IsRealTimePreview;
        public List<MatPea> PreViewMaterials = new List<MatPea>();

        public void EnableRealTimePreview()
        {
            if (_IsRealTimePreview) return;
            if (!IsPossibleCompile) return;
            _IsRealTimePreview = true;

            PreViewMaterials.Clear();

            foreach (var Rendarer in TargetRenderers)
            {
                var Materials = Rendarer.sharedMaterials;
                for (int i = 0; i < Materials.Length; i += 1)
                {
                    if (Materials[i].shader.name == "Hidden/RealTimeSimpleDecalPreview") continue;
                    if (PreViewMaterials.Any(i2 => i2.Material == Materials[i]))
                    {
                        Materials[i] = PreViewMaterials.Find(i2 => i2.Material == Materials[i]).SecndMaterial;
                    }
                    else
                    {
                        var DistMat = Materials[i];
                        var NewMat = Instantiate<Material>(Materials[i]);
                        NewMat.shader = Shader.Find("Hidden/RealTimeSimpleDecalPreview");
                        Materials[i] = NewMat;
                        PreViewMaterials.Add(new MatPea(DistMat, NewMat));
                    }
                }
                Rendarer.sharedMaterials = Materials;
            }
            AssetSaveHelper.SaveAssets(PreViewMaterials.Select(i => i.SecndMaterial));
        }
        public void DisableRealTimePreview()
        {
            if (!_IsRealTimePreview) return;
            _IsRealTimePreview = false;

            foreach (var Rendarer in TargetRenderers)
            {
                var Materials = Rendarer.sharedMaterials;
                for (int i = 0; i < Materials.Length; i += 1)
                {
                    var DistMat = PreViewMaterials.Find(i2 => i2.SecndMaterial == Materials[i]).Material;
                    if (DistMat != null) Materials[i] = DistMat;

                }
                Rendarer.sharedMaterials = Materials;
            }
            AssetSaveHelper.DeletAssets(PreViewMaterials.Select(i => i.SecndMaterial));
            PreViewMaterials.Clear();

        }

        public void UpdateRealTimePreview()
        {
            var Matrix = transform.worldToLocalMatrix;
            foreach (var MatPea in PreViewMaterials)
            {
                MatPea.SecndMaterial.SetMatrix("_WorldToDecal", Matrix);
                MatPea.SecndMaterial.SetTexture("_DecalTex", DecalTexture);
            }
        }

        private void Update()
        {
            UpdateRealTimePreview();
        }
    }
}



#endif