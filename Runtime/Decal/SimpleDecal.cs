#if UNITY_EDITOR
using System.Diagnostics.SymbolStore;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
namespace Rs64.TexTransTool.Decal
{
    [AddComponentMenu("TexTransTool/SimpleDecal")]
    [ExecuteInEditMode]
    public class SimpleDecal : AbstractDecal<ParallelProjectionSpase>
    {
        public Vector2 Scale = Vector2.one;
        public float MaxDistans = 1;
        public bool FixedAspect = true;
        public bool SideChek = true;
        public PolygonCaling PolygonCaling = PolygonCaling.Vartex;

        public override ParallelProjectionSpase GetSpaseConverter => new ParallelProjectionSpase(transform.worldToLocalMatrix);
        public override DecalUtil.ITraianglesFilter<ParallelProjectionSpase> GetTraiangleFilter => new ParallelProjectionFilter(GetFilter());

        public bool IslandCulling = false;
        public override void ScaleApply()
        {
            ScaleApply(new Vector3(Scale.x, Scale.y, MaxDistans), FixedAspect);
        }
        public List<TrainagelFilterUtility.ITraiangleFiltaring<List<Vector3>>> GetFilter()
        {
            var Filters = new List<TrainagelFilterUtility.ITraiangleFiltaring<List<Vector3>>>();

            Filters.Add(new TrainagelFilterUtility.FarStruct(1, false));
            Filters.Add(new TrainagelFilterUtility.NearStruct(0, true));
            if (SideChek) Filters.Add(new TrainagelFilterUtility.SideStruct());
            Filters.Add(new TrainagelFilterUtility.OutOfPorigonStruct(PolygonCaling, 0, 1, true));

            return Filters;
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
        Dictionary<RenderTexture, RenderTexture> _RealTimePreviewDecalTextureCompile;
        Dictionary<Texture2D, RenderTexture> _RealTimePreviewDecalTextureBlend;

        public List<MatPea> PreViewMaterials = new List<MatPea>();

        public void EnableRealTimePreview()
        {
            if (_IsRealTimePreview) return;
            if (!IsPossibleCompile) return;
            _IsRealTimePreview = true;

            PreViewMaterials.Clear();

            _RealTimePreviewDecalTextureCompile = new Dictionary<RenderTexture, RenderTexture>();
            _RealTimePreviewDecalTextureBlend = new Dictionary<Texture2D, RenderTexture>();


            foreach (var Rendarer in TargetRenderers)
            {
                var Materials = Rendarer.sharedMaterials;
                for (int i = 0; i < Materials.Length; i += 1)
                {
                    if (!Materials[i].HasProperty(TargetPropatyName)) { continue; }
                    if (Materials[i].GetTexture(TargetPropatyName) is RenderTexture) { continue; }
                    if (PreViewMaterials.Any(i2 => i2.Material == Materials[i]))
                    {
                        Materials[i] = PreViewMaterials.Find(i2 => i2.Material == Materials[i]).SecndMaterial;
                    }
                    else
                    {
                        var DistMat = Materials[i];
                        var NewMat = Instantiate<Material>(Materials[i]);
                        var srostex = NewMat.GetTexture(TargetPropatyName);

                        if (srostex is Texture2D tex2d && tex2d != null)
                        {
                            var NewTexBlend = new RenderTexture(tex2d.width, tex2d.height, 32, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB, -1);
                            var NewTexCompiled = new RenderTexture(tex2d.width, tex2d.height, 32, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB, -1);
                            _RealTimePreviewDecalTextureCompile.Add(NewTexBlend, NewTexCompiled);
                            _RealTimePreviewDecalTextureBlend.Add(tex2d, NewTexBlend);
                            NewMat.SetTexture(TargetPropatyName, NewTexBlend);

                            Materials[i] = NewMat;
                            PreViewMaterials.Add(new MatPea(DistMat, NewMat));
                        }

                    }
                }
                Rendarer.sharedMaterials = Materials;
            }
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
            PreViewMaterials.Clear();
            _RealTimePreviewDecalTextureBlend = null;
            _RealTimePreviewDecalTextureCompile = null;

        }

        public void UpdateRealTimePreview()
        {
            if (!_IsRealTimePreview) return;

            foreach (var rt in _RealTimePreviewDecalTextureCompile)
            {
                rt.Value.Release();
            }

            foreach (var render in TargetRenderers)
            {
                DecalUtil.CreatDecalTexture(render, _RealTimePreviewDecalTextureCompile, DecalTexture, GetSpaseConverter, GetTraiangleFilter, TargetPropatyName, GetOutRengeTexture, DefaultPading);
            }
            foreach (var Stex in _RealTimePreviewDecalTextureBlend.Keys)
            {
                var BlendRT = _RealTimePreviewDecalTextureBlend[Stex];
                var CompoledRT = _RealTimePreviewDecalTextureCompile[BlendRT];
                BlendRT.Release();
                Graphics.Blit(Stex, BlendRT);
                TextureLayerUtil.BlendBlit(BlendRT, CompoledRT, BlendType);
            }



        }

        private void Update()
        {
            UpdateRealTimePreview();
        }
    }

    public class ParallelProjectionSpase : DecalUtil.IConvertSpace
    {
        public Matrix4x4 ParallelProjectionMatrix;
        public List<Vector3> PPSVarts;
        public DecalUtil.MeshDatas MeshData;
        public ParallelProjectionSpase(Matrix4x4 ParallelProjectionMatrix)
        {
            this.ParallelProjectionMatrix = ParallelProjectionMatrix;

        }
        public void Input(DecalUtil.MeshDatas MeshData)
        {
            this.MeshData = MeshData;
            PPSVarts = DecalUtil.ConvartVerticesInMatlix(ParallelProjectionMatrix, MeshData.Varticals, new Vector3(0.5f, 0.5f, 0));
        }

        public List<Vector2> OutPutUV()
        {
            var UV = new List<Vector2>(PPSVarts.Capacity);
            foreach (var Vart in PPSVarts)
            {
                UV.Add(Vart);
            }
            return UV;
        }

    }

    public class ParallelProjectionFilter : DecalUtil.ITraianglesFilter<ParallelProjectionSpase>
    {
        public List<TrainagelFilterUtility.ITraiangleFiltaring<List<Vector3>>> Filters;
        public List<Ray> IslandSelectors;

        public ParallelProjectionFilter(List<TrainagelFilterUtility.ITraiangleFiltaring<List<Vector3>>> Filters)
        {
            this.Filters = Filters;
            IslandSelectors = null;
        }
        public ParallelProjectionFilter(List<DecalUtil.Filtaring<List<Vector3>>> Filters, List<Ray> IslandSelectors)
        {
            this.Filters = Filters;
            this.IslandSelectors = IslandSelectors;
        }

        public List<TraiangleIndex> Filtering(ParallelProjectionSpase Spase, List<TraiangleIndex> Traiangeles)
        {
            if (IslandSelectors != null) Traiangeles = Island.IslandCulling.Culling(IslandSelectors, Spase.MeshData.Varticals, Spase.MeshData.UV, Traiangeles);
            return TrainagelFilterUtility.FiltaringTraiangle<List<Vector3>, TrainagelFilterUtility.ITraiangleFiltaring<List<Vector3>>>(Traiangeles, Spase.PPSVarts, Filters);
        }
    }
}



#endif