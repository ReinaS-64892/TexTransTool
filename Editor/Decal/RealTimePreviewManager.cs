using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.Decal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Profiling;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;

namespace net.rs64.TexTransTool
{
    internal class RealTimePreviewManager : ScriptableSingleton<RealTimePreviewManager>
    {
        private Dictionary<AbstractDecal, DecalTargetInstance> RealTimePreviews = new();
        private Dictionary<Material, Dictionary<string, CompositePreviewInstance>> PreviewMaterials = new();
        private Dictionary<Material, Material> PreviewMatSwapDict = new();
        private HashSet<Renderer> PreviewTargetRenderer = new();
        private Stopwatch stopwatch = new();
        [NonSerialized] long intervalTime = 0;
        [NonSerialized] long lastUpdateTime = 0;
        public long LastDecalUpdateTime => lastUpdateTime;
        protected RealTimePreviewManager()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= ExitPreview;
            AssemblyReloadEvents.beforeAssemblyReload += ExitPreview;
            EditorSceneManager.sceneClosing -= ExitPreview;
            EditorSceneManager.sceneClosing += ExitPreview;
            DestroyCall.OnDestroy -= DestroyObserve;
            DestroyCall.OnDestroy += DestroyObserve;
        }
        public static int ContainsPreviewCount => instance.RealTimePreviews.Count;
        public static bool IsContainsRealTimePreviewDecal => instance.RealTimePreviews.Count > 0;
        public static bool IsContainsRealTimePreviewRenderer => instance.PreviewTargetRenderer.Count > 0;
        public static bool Contains(AbstractDecal abstractDecal) => instance.RealTimePreviews.ContainsKey(abstractDecal);
        public HashSet<AbstractDecal> ForcesDecal = new();

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
                    var newTarget = RenderTexture.GetTemporary(blendTexture.RenderTexture.descriptor);
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
                var newTarget = RenderTexture.GetTemporary(blendTexture.RenderTexture.descriptor);
                newTarget.CopyFilWrap(blendTexture.RenderTexture);

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
            lastUpdateTime = 0;
            intervalTime = 0;
            stopwatch.Stop();
            stopwatch.Reset();
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
                intervalTime = 0;
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
                                Rt = RenderTexture.GetTemporary(texture2D.width, texture2D.height, 0);
                                break;
                            }
                        case RenderTexture renderTexture://すでにプレビューが入っている場合でRenderTextureをこぴる感じ
                            {
                                Rt = RenderTexture.GetTemporary(renderTexture.descriptor);
                                break;
                            }
                        default:
                            { continue; }
                    }

                    Rt.CopyFilWrap(tex);

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

                var targetLayer = PreviewMaterials[mat][absDecalData.PropertyName].DecalLayers.Find(RTRemoveComparer);
                PreviewMaterials[mat][absDecalData.PropertyName].DecalLayers.Remove(targetLayer);
                targetLayer.Dispose();


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
            var absDecalData = RealTimePreviews[abstractDecal];

            if (absDecalData.PropertyName != abstractDecal.TargetPropertyName
             || !absDecalData.RendererEqual(abstractDecal.GetRenderers))
            {
                Profiler.BeginSample("Reregister decal");
                UnRegtAbstractDecal(abstractDecal); RegtAbstractDecal(abstractDecal);
                Profiler.EndSample();
            }

            if (absDecalData.BlendTypeKey != abstractDecal.BlendTypeKey)
            { absDecalData.SetBlendTypeKey(abstractDecal.BlendTypeKey); }


            Profiler.BeginSample("Clear and compile decal");
            absDecalData.ClearDecalTarget();
            abstractDecal.CompileDecal(new TextureManager(true), absDecalData.decalTargets);
            Profiler.EndSample();
        }

        public void PreviewForcesDecalUpdate()
        {
            if (ForcesDecal == null) { return; }

            if (!ForcesDecal.Any(Contains)) { return; }

            if (stopwatch.IsRunning)//intervalTimeがあるのにストップウォッチが進んでないケースは強制的にアップデートする
            {
                if (intervalTime > stopwatch.ElapsedMilliseconds) { return; }
                stopwatch.Stop();
            }

            stopwatch.Restart();
            foreach (var decal in ForcesDecal) { UpdateAbstractDecal(decal); }

            Profiler.BeginSample("Update preview texture");
            foreach (var mk in ForcesDecal
                                .Where(i => RealTimePreviews.ContainsKey(i))
                                .SelectMany(i => RealTimePreviews[i].decalTargets.Keys.Select(m => (RealTimePreviews[i].PropertyName, m)))
                                .Distinct()
            )
            {
                UpdatePreviewTexture(mk.m, mk.PropertyName);
            }
            Profiler.EndSample();

            stopwatch.Stop();

            lastUpdateTime = stopwatch.ElapsedMilliseconds;
            intervalTime = lastUpdateTime * lastUpdateTime;
            if (intervalTime > 4096) { intervalTime = 4096; }

            stopwatch.Restart();
        }


        public class BlendRenderTextureClass : IBlendTexturePair, IDisposable
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

            public void Dispose()
            {
                RenderTexture.ReleaseTemporary(RenderTexture);
            }
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

            public bool RendererEqual(List<Renderer> renderers)
            {
                var count = TargetRenderers.Count;
                var nCount = renderers.Count;

                if (count != nCount) { return false; }

                for (var i = 0; count > i; i += 1)
                {
                    if (TargetRenderers[i] != renderers[i]) { return false; }
                }

                return true;
            }

        }

        internal struct CompositePreviewInstance
        {
            public PreviewTexturePair ViewTexture;
            public List<BlendRenderTextureClass> DecalLayers;

            public void CompositeUpdate()
            {
                ViewTexture.ReviewReInit();
                ViewTexture.TargetTexture.BlendBlit(DecalLayers);
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
                    Profiler.BeginSample("ReviewReInit");
                    TargetTexture.Clear();
                    Graphics.Blit(SouseTexture, TargetTexture);
                    Profiler.EndSample();
                }

                public PreviewTexturePair(Texture2D souseTexture, RenderTexture targetTexture)
                {
                    SouseTexture = souseTexture;
                    TargetTexture = targetTexture;
                }
            }
        }


        public void DestroyObserve(TexTransBehavior behavior)
        {
            if (behavior is AbstractDecal abstractDecal && Contains(abstractDecal)) { UnRegtAbstractDecal(abstractDecal); }
        }


    }

}
