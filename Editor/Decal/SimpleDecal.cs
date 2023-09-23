#if UNITY_EDITOR
using System.Diagnostics.SymbolStore;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.Serialization;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransCore.Island;

namespace net.rs64.TexTransTool.Decal
{
    [AddComponentMenu("TexTransTool/TTT SimpleDecal")]
    [ExecuteInEditMode]
    public class SimpleDecal : AbstractSingleDecal<ParallelProjectionSpace>
    {
        public Vector2 Scale = Vector2.one;
        public float MaxDistance = 1;
        public bool FixedAspect = true;
        [FormerlySerializedAs("SideChek")] public bool SideCulling = true;
        [FormerlySerializedAs("PolygonCaling")] public PolygonCulling PolygonCulling = PolygonCulling.Vertex;

        public override ParallelProjectionSpace GetSpaceConverter => new ParallelProjectionSpace(transform.worldToLocalMatrix);
        public override DecalUtility.ITrianglesFilter<ParallelProjectionSpace> GetTriangleFilter
        {
            get
            {
                if (IslandCulling) { return new IslandCullingPPFilter(GetFilter(), GetIslandSelector(), new EditorIsland.EditorIslandCache()); }
                else { return new ParallelProjectionFilter(GetFilter()); }
            }
        }

        public bool IslandCulling = false;
        public Vector2 IslandSelectorPos = new Vector2(0.5f, 0.5f);
        public float IslandSelectorRange = 1;
        public override void ScaleApply()
        {
            ScaleApply(new Vector3(Scale.x, Scale.y, MaxDistance), FixedAspect);
        }
        public List<TriangleFilterUtility.ITriangleFiltering<List<Vector3>>> GetFilter()
        {
            var filters = new List<TriangleFilterUtility.ITriangleFiltering<List<Vector3>>>
            {
                new TriangleFilterUtility.FarStruct(1, false),
                new TriangleFilterUtility.NearStruct(0, true)
            };
            if (SideCulling) filters.Add(new TriangleFilterUtility.SideStruct());
            filters.Add(new TriangleFilterUtility.OutOfPolygonStruct(PolygonCulling, 0, 1, true));

            return filters;
        }

        public List<IslandSelector> GetIslandSelector()
        {
            if (!IslandCulling) return null;
            return new List<IslandSelector>() {
                new IslandSelector(new Ray(transform.localToWorldMatrix.MultiplyPoint3x4(IslandSelectorPos - new Vector2(0.5f, 0.5f)), transform.forward), MaxDistance * IslandSelectorRange)
                };
        }


        [NonSerialized] public Material DisplayDecalMat;
        public Color GizmoColor = new Color(0, 0, 0, 1);
        [NonSerialized] public Mesh Quad;

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = GizmoColor;
            var matrix = transform.localToWorldMatrix;

            Gizmos.matrix = matrix;

            var centerPos = Vector3.zero;
            Gizmos.DrawWireCube(centerPos + new Vector3(0, 0, 0.5f), new Vector3(1, 1, 1));//基準となる四角形

            if (DecalTexture != null)
            {
                if (DisplayDecalMat == null || Quad == null) GizmoInstance();
                DisplayDecalMat.SetPass(0);
                Graphics.DrawMeshNow(Quad, matrix);
            }
            if (IslandCulling)
            {
                Vector3 selectorOrigin = new Vector2(IslandSelectorPos.x - 0.5f, IslandSelectorPos.y - 0.5f);
                var selectorTail = (Vector3.forward * IslandSelectorRange) + selectorOrigin;
                Gizmos.DrawLine(selectorOrigin, selectorTail);
            }
        }

        public void GizmoInstance()
        {
            DisplayDecalMat = new Material(Shader.Find("Hidden/DisplayDecalTexture"));
            DisplayDecalMat.mainTexture = DecalTexture;
            Quad = AssetDatabase.LoadAllAssetsAtPath("Library/unity default resources").ToList().Find(i => i.name == "Quad") as Mesh;

        }

        [SerializeField] protected bool _IsRealTimePreview = false;
        public bool IsRealTimePreview => _IsRealTimePreview;
        [SerializeField] RenderTexture DecalRenderTexture;
        Dictionary<RenderTexture, RenderTexture> _RealTimePreviewDecalTextureCompile;
        Dictionary<Texture2D, RenderTexture> _RealTimePreviewDecalTextureBlend;

        public List<MatPair> PreViewMaterials = new List<MatPair>();

        public void EnableRealTimePreview()
        {
            if (_IsRealTimePreview) return;
            if (!IsPossibleApply) return;
            _IsRealTimePreview = true;

            PreViewMaterials.Clear();

            _RealTimePreviewDecalTextureCompile = new Dictionary<RenderTexture, RenderTexture>();
            _RealTimePreviewDecalTextureBlend = new Dictionary<Texture2D, RenderTexture>();
            DecalRenderTexture = new RenderTexture(DecalTexture.width, DecalTexture.height, 32, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB, -1);


            foreach (var renderer in TargetRenderers)
            {
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i += 1)
                {
                    if (!materials[i].HasProperty(TargetPropertyName)) { continue; }
                    if (materials[i].GetTexture(TargetPropertyName) is RenderTexture) { continue; }
                    if (PreViewMaterials.Any(i2 => i2.Material == materials[i]))
                    {
                        materials[i] = PreViewMaterials.Find(i2 => i2.Material == materials[i]).SecondMaterial;
                    }
                    else
                    {
                        var distMat = materials[i];
                        var newMat = Instantiate<Material>(materials[i]);
                        var souseTex = newMat.GetTexture(TargetPropertyName);

                        if (souseTex is Texture2D tex2d && tex2d != null)
                        {
                            if (!_RealTimePreviewDecalTextureBlend.ContainsKey(tex2d))
                            {
                                var NewTexBlend = new RenderTexture(tex2d.width, tex2d.height, 32, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB, -1);
                                var NewTexCompiled = new RenderTexture(tex2d.width, tex2d.height, 32, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB, -1);
                                _RealTimePreviewDecalTextureCompile.Add(NewTexBlend, NewTexCompiled);
                                _RealTimePreviewDecalTextureBlend.Add(tex2d, NewTexBlend);
                                newMat.SetTexture(TargetPropertyName, NewTexBlend);
                            }
                            else
                            {
                                newMat.SetTexture(TargetPropertyName, _RealTimePreviewDecalTextureBlend[tex2d]);
                            }
                            materials[i] = newMat;
                            PreViewMaterials.Add(new MatPair(distMat, newMat));
                        }


                    }
                }
                if (_RealTimePreviewDecalTextureCompile.Count == 0)
                {
                    _IsRealTimePreview = false;
                    Debug.LogWarning("SimpleDecal : ほかのSimpleDecalのコンポーネントがリアルタイムプレビューをしているか、Texture2D以外のテクスチャがセットされているマテリアルのためリアルタイムプレビューができませんでした。");
                    return;
                }
                renderer.sharedMaterials = materials;
            }
        }
        public void DisableRealTimePreview()
        {
            if (!_IsRealTimePreview) return;
            _IsRealTimePreview = false;

            foreach (var renderer in TargetRenderers)
            {
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i += 1)
                {
                    var distMat = PreViewMaterials.Find(i2 => i2.SecondMaterial == materials[i]).Material;
                    if (distMat != null) materials[i] = distMat;

                }
                renderer.sharedMaterials = materials;

            }
            PreViewMaterials.Clear();
            _RealTimePreviewDecalTextureBlend = null;
            _RealTimePreviewDecalTextureCompile = null;
            DecalRenderTexture = null;
        }

        public void UpdateRealTimePreview()
        {
            if (!_IsRealTimePreview) return;
            if (_RealTimePreviewDecalTextureCompile == null)
            {
                DisableRealTimePreview();
                EditorUtility.SetDirty(this);
                Debug.Log("SimpleDecal : シーンのリロードやスクリプトのリロードなどでリアルタイムプレビューが継続できなくなったため中断します。");
                return;
            }

            DecalRenderTexture.Release();
            TextureLayerUtil.MultipleRenderTexture(DecalRenderTexture, DecalTexture, Color);

            foreach (var rt in _RealTimePreviewDecalTextureCompile)
            {
                rt.Value.Release();
            }

            foreach (var render in TargetRenderers)
            {
                DecalUtility.CreateDecalTexture(render, _RealTimePreviewDecalTextureCompile, DecalRenderTexture, GetSpaceConverter, GetTriangleFilter, TargetPropertyName, GetTextureWarp, Padding);
            }
            foreach (var sTex in _RealTimePreviewDecalTextureBlend.Keys)
            {
                var blendRT = _RealTimePreviewDecalTextureBlend[sTex];
                var compiledRT = _RealTimePreviewDecalTextureCompile[blendRT];
                blendRT.Release();
                Graphics.Blit(sTex, blendRT);
                TextureLayerUtil.BlendBlit(blendRT, compiledRT, BlendType);
            }



        }

        private void Update()
        {
            UpdateRealTimePreview();
        }
    }
}



#endif
