using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.EditorIsland;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Pool;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;
using Debug = UnityEngine.Debug;

namespace net.rs64.TexTransTool
{
    internal class RealTimePreviewManager : ScriptableSingleton<RealTimePreviewManager>
    {
        private Dictionary<AbstractDecal, DecalTargetInstance> RealTimePreviews = new();
        private Dictionary<Material, Dictionary<string, CompositePreviewInstance>> PreviewMaterials = new();
        private Dictionary<Material, Material> PreviewMatSwapDict = new();
        private HashSet<Renderer> PreviewTargetRenderer = new();
        private IIslandCache _islandCacheManager;
        private Stopwatch stopwatch = new();
        long lastUpdateTime = 0;
        public long LastDecalUpdateTime => lastUpdateTime;
        protected RealTimePreviewManager()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= ExitPreview;
            AssemblyReloadEvents.beforeAssemblyReload += ExitPreview;
            EditorSceneManager.sceneClosing -= ExitPreview;
            EditorSceneManager.sceneClosing += ExitPreview;
        }

        public static int ContainsPreviewCount => instance.RealTimePreviews.Count;
        public static bool IsContainsRealTimePreviewDecal => instance.RealTimePreviews.Count > 0;
        public static bool IsContainsRealTimePreviewRenderer => instance.PreviewTargetRenderer.Count > 0;
        public static bool Contains(AbstractDecal abstractDecal) => instance.RealTimePreviews.ContainsKey(abstractDecal);
        public AbstractDecal ForcesDecal;

        private void RegtRenderer(Renderer renderer)
        {
            if (PreviewTargetRenderer.Contains(renderer) || renderer == null) { return; }
            PreviewTargetRenderer.Add(renderer);
            foreach (var MatPair in PreviewMatSwapDict)
            {
                SwapMaterial(renderer, MatPair.Key, MatPair.Value);
            }
        }

        private void SwapMaterialAll(Material material, Material editableMat)
        {
            foreach (var renderer in PreviewTargetRenderer)
            {
                SwapMaterial(renderer, material, editableMat);
            }
        }
        private void SwapMaterial(Renderer renderer, Material souse, Material target)
        {
            using (var serialized = new SerializedObject(renderer))
            {
                foreach (SerializedProperty property in serialized.FindProperty("m_Materials"))
                {
                    if (property.objectReferenceValue is Material material && material == souse)
                    {
                        AnimationMode.AddPropertyModification(
                            EditorCurveBinding.PPtrCurve("", renderer.GetType(), property.propertyPath),
                            new PropertyModification
                            {
                                target = renderer,
                                propertyPath = property.propertyPath,
                                objectReference = souse,
                            },
                            true);
                        property.objectReferenceValue = target;
                    }
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private void RegtPreviewRenderTexture(Material material, string propertyName, BlendRenderTextureClass blendTexture)
        {
            if (PreviewMatSwapDict.ContainsKey(material)) { material = PreviewMatSwapDict[material]; }

            if (PreviewMaterials.ContainsKey(material))
            {
                if (PreviewMaterials[material].ContainsKey(propertyName))
                {
                    PreviewMaterials[material][propertyName].DecalLayers.Add(blendTexture);
                }
                else//既にそのマテリアルでプレビューが存在するが、別のプロパティの場合
                {
                    var newTarget = new RenderTexture(blendTexture.RenderTexture.descriptor);
                    var souseTexture = material.GetTexture(propertyName) as Texture2D;
                    material.SetTexture(propertyName, newTarget);

                    var previewIPair = new CompositePreviewInstance.PreviewTexturePair(souseTexture, newTarget);
                    previewIPair.ReviewReInit();

                    PreviewMaterials[material].Add(propertyName, new(previewIPair, new List<BlendRenderTextureClass>() { blendTexture }));
                }
            }
            else //そのマテリアルにプレビューが存在しない場合　つまりそのマテリアルの初回
            {
                var editableMat = Instantiate(material);

                SwapMaterialAll(material, editableMat);
                PreviewMatSwapDict.Add(material, editableMat);

                var souseTexture = material.GetTexture(propertyName) as Texture2D;
                var newTarget = new RenderTexture(blendTexture.RenderTexture.descriptor);
                editableMat.SetTexture(propertyName, newTarget);

                var previewIPair = new CompositePreviewInstance.PreviewTexturePair(souseTexture, newTarget);
                previewIPair.ReviewReInit();

                PreviewMaterials.Add(editableMat, new() { { propertyName, new(previewIPair, new List<BlendRenderTextureClass>() { blendTexture }) } });
            }
        }


        private void UpdatePreviewTexture(Material material, string propertyName)
        {
            if (!PreviewMaterials.ContainsKey(material)) { return; }
            if (!PreviewMaterials[material].ContainsKey(propertyName)) { return; }

            PreviewMaterials[material][propertyName].CompositeUpdate();

        }
        private void PreviewStart()
        {
            EditorApplication.update -= PreviewForcesDecalUpdate;
            EditorApplication.update += PreviewForcesDecalUpdate;
        }
        public void ExitPreview()
        {
            if (IsContainsRealTimePreviewRenderer)
            {
                AnimationMode.StopAnimationMode();
                RealTimePreviews.Clear();
                PreviewMaterials.Clear();
                PreviewMatSwapDict.Clear();
                PreviewTargetRenderer.Clear();
                PreviewMaterials.Clear();

                EditorApplication.update -= PreviewForcesDecalUpdate;
                stopwatch.Stop();
                stopwatch.Reset();
                lastUpdateTime = 0;
            }
        }
        private void ExitPreview(UnityEngine.SceneManagement.Scene scene, bool removingScene)
        {
            ExitPreview();
        }

        public void RegtAbstractDecal(AbstractDecal abstractDecal)
        {
            if (RealTimePreviews.Count == 0) { AnimationMode.StartAnimationMode(); PreviewStart(); }
            if (RealTimePreviews.ContainsKey(abstractDecal)) { return; }

            var decalTargets = new Dictionary<Material, RenderTexture>();
            var blends = new List<BlendRenderTextureClass>();
            var TargetMats = RendererUtility.GetFilteredMaterials(abstractDecal.TargetRenderers, ListPool<Material>.Get());

            foreach (var mat in TargetMats)
            {
                if (mat.HasProperty(abstractDecal.TargetPropertyName) && mat.GetTexture(abstractDecal.TargetPropertyName) != null)
                {
                    var tex = mat.GetTexture(abstractDecal.TargetPropertyName);
                    RenderTexture Rt = null;
                    switch (tex)
                    {
                        case Texture2D texture2D:
                            {
                                Rt = new RenderTexture(texture2D.width, texture2D.height, 0);
                                break;
                            }
                        case RenderTexture renderTexture:
                            {
                                Rt = new RenderTexture(renderTexture.descriptor);
                                break;
                            }
                        default:
                            { continue; }
                    }

                    var blendTex = new BlendRenderTextureClass(Rt, abstractDecal.BlendTypeKey);
                    blends.Add(blendTex);

                    RegtPreviewRenderTexture(mat, abstractDecal.TargetPropertyName, blendTex);
                    Material editableMat = PreviewMatSwapDict.ContainsKey(mat) ? PreviewMatSwapDict[mat] : mat;

                    decalTargets.Add(editableMat, Rt);
                }
            }

            foreach (var render in abstractDecal.GetRenderers) { RegtRenderer(render); }

            RealTimePreviews.Add(abstractDecal, new(abstractDecal.TargetPropertyName, abstractDecal.BlendTypeKey, abstractDecal.GetRenderers, blends, decalTargets));

            ListPool<Material>.Release(TargetMats);
        }
        public bool IsRealTimePreview(AbstractDecal abstractDecal) => RealTimePreviews.ContainsKey(abstractDecal);
        public void UnRegtAbstractDecal(AbstractDecal abstractDecal)
        {
            if (!IsRealTimePreview(abstractDecal)) { return; }
            var absDecalData = RealTimePreviews[abstractDecal];

            foreach (var decalTarget in absDecalData.decalTargets)
            {
                var mat = decalTarget.Key;
                var rt = decalTarget.Value;

                if (!PreviewMaterials.ContainsKey(mat)) { continue; }

                bool RTRemoveComparer(BlendRenderTextureClass btc) => btc.RenderTexture == rt;

                PreviewMaterials[mat][absDecalData.PropertyName].DecalLayers
                .Remove(PreviewMaterials[mat][absDecalData.PropertyName].DecalLayers.Find(RTRemoveComparer));


                if (PreviewMaterials[mat][absDecalData.PropertyName].DecalLayers.Count == 0)
                {
                    mat.SetTexture(absDecalData.PropertyName, PreviewMaterials[mat][absDecalData.PropertyName].ViewTexture.SouseTexture);
                    PreviewMaterials[mat].Remove(absDecalData.PropertyName);
                }
                else
                {
                    UpdatePreviewTexture(mat, absDecalData.PropertyName);
                }

            }

            RealTimePreviews.Remove(abstractDecal);
            if (RealTimePreviews.Count == 0) { ExitPreview(); }
        }

        public void UpdateAbstractDecal(AbstractDecal abstractDecal)
        {
            if (!RealTimePreviews.ContainsKey(abstractDecal)) { return; }
            _islandCacheManager ??= new EditorIslandCache();
            var absDecalData = RealTimePreviews[abstractDecal];

            if (absDecalData.PropertyName != abstractDecal.TargetPropertyName
             || !absDecalData.TargetRenderers.SequenceEqual(abstractDecal.GetRenderers))
            { UnRegtAbstractDecal(abstractDecal); RegtAbstractDecal(abstractDecal); }

            if (absDecalData.BlendTypeKey != abstractDecal.BlendTypeKey)
            { absDecalData.SetBlendTypeKey(abstractDecal.BlendTypeKey); }


            absDecalData.ClearDecalTarget();
            abstractDecal.CompileDecal(new TextureManager(true), _islandCacheManager, absDecalData.decalTargets);

            foreach (var mat in absDecalData.decalTargets.Keys)
            { UpdatePreviewTexture(mat, absDecalData.PropertyName); }
        }

        public void PreviewForcesDecalUpdate()
        {
            if (ForcesDecal == null) { return; }

            if (!Contains(ForcesDecal)) { return; }
            if ((lastUpdateTime * lastUpdateTime) > stopwatch.ElapsedMilliseconds) { return; }
            stopwatch.Stop();

            stopwatch.Restart();
            UpdateAbstractDecal(ForcesDecal);
            stopwatch.Stop();
            lastUpdateTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
        }


        public class BlendRenderTextureClass : IBlendTexturePair
        {
            public RenderTexture RenderTexture;
            public string BlendTypeKey;

            public BlendRenderTextureClass(RenderTexture renderTexture, string blendTypeKey)
            {
                RenderTexture = renderTexture;
                BlendTypeKey = blendTypeKey;
            }

            public Texture Texture => RenderTexture;
            string IBlendTexturePair.BlendTypeKey => BlendTypeKey;
        }
        internal struct DecalTargetInstance
        {
            public string PropertyName;
            public string BlendTypeKey;
            public List<Renderer> TargetRenderers;
            public List<BlendRenderTextureClass> blendTextureList;
            public Dictionary<Material, RenderTexture> decalTargets;//CompileDealの型合わせ
            public DecalTargetInstance(string propertyName, string blendTypeKey, List<Renderer> targetRenderers, List<BlendRenderTextureClass> blendTextureList, Dictionary<Material, RenderTexture> decalTargets)
            {
                PropertyName = propertyName;
                BlendTypeKey = blendTypeKey;
                TargetRenderers = new(targetRenderers);
                this.blendTextureList = blendTextureList;
                this.decalTargets = decalTargets;
            }

            public void ClearDecalTarget()
            {
                foreach (var target in decalTargets)
                { target.Value.Clear(); }
            }

            public void SetBlendTypeKey(string blendTypeKey)
            {
                BlendTypeKey = blendTypeKey;

                foreach (var blendData in blendTextureList)
                { blendData.BlendTypeKey = BlendTypeKey; }

            }

        }

        internal struct CompositePreviewInstance
        {
            public PreviewTexturePair ViewTexture;
            public List<BlendRenderTextureClass> DecalLayers;

            public void CompositeUpdate()
            {
                ViewTexture.ReviewReInit();
                ViewTexture.TargetTexture.BlendBlit(DecalLayers.Where(IsNotNull));
                static bool IsNotNull(BlendRenderTextureClass btc) => btc.RenderTexture != null;
            }

            public CompositePreviewInstance(PreviewTexturePair viewTex, List<BlendRenderTextureClass> decals)
            {
                ViewTexture = viewTex;
                DecalLayers = decals;
            }
            internal struct PreviewTexturePair
            {
                public Texture2D SouseTexture;
                public RenderTexture TargetTexture;

                public void ReviewReInit()
                {
                    TargetTexture.Clear();
                    Graphics.Blit(SouseTexture, TargetTexture);
                }

                public PreviewTexturePair(Texture2D souseTexture, RenderTexture targetTexture)
                {
                    SouseTexture = souseTexture;
                    TargetTexture = targetTexture;
                }
            }
        }



    }

}
