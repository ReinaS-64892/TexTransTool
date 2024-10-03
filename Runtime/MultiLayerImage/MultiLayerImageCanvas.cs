using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using net.rs64.TexTransUnityCore;
using net.rs64.TexTransUnityCore.BlendTexture;
using net.rs64.TexTransUnityCore.Utils;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransUnityCore.BlendTexture.TextureBlend;
using Color = UnityEngine.Color;
using UnityEngine.Profiling;
using System.Runtime.CompilerServices;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class MultiLayerImageCanvas : TexTransRuntimeBehavior, ITTTChildExclusion
    {
        internal const string FoldoutName = "MultiLayerImage";
        internal const string ComponentName = "TTT MultiLayerImageCanvas";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.BeforeUVModification;

        public TextureSelector TextureSelector;

        [SerializeField, HideInInspector] internal TTTImportedCanvasDescription tttImportedCanvasDescription;

        internal override void Apply([NotNull] IDomain domain)
        {
            var replaceTarget = TextureSelector.GetTexture();
            if (replaceTarget == null) { TTTRuntimeLog.Info("MultiLayerImageCanvas:info:TargetNotSet"); domain.LookAt(this); return; }

            var nowDomainsTargets = RendererUtility.GetAllTexture<Texture>(domain.EnumerateRenderer()).Where(m => domain.OriginEqual(m, replaceTarget));
            if (nowDomainsTargets.Any() is false) { TTTRuntimeLog.Info("MultiLayerImageCanvas:info:TargetNotFound"); return; }

            Profiler.BeginSample("LookAtCallingCanvas");
            LookAtCallingCanvas(domain);
            Profiler.EndSample();

            var canvasWidth = tttImportedCanvasDescription?.Width ?? NormalizePowOfTow(replaceTarget.width);
            var canvasHeigh = tttImportedCanvasDescription?.Height ?? NormalizePowOfTow(replaceTarget.width);
            if (domain.IsPreview()) { canvasWidth = Mathf.Min(1024, canvasWidth); canvasHeigh = Mathf.Min(1024, canvasHeigh); }

            Profiler.BeginSample("EvaluateCanvas");
            var result = EvaluateCanvas(domain.GetTextureManager(), canvasWidth, canvasHeigh);
            Profiler.EndSample();

            foreach (var target in nowDomainsTargets) { domain.AddTextureStack(target, new BlendTexturePair(result, "NotBlend")); }
        }

        internal RenderTexture EvaluateCanvas(ITextureManager textureManager, int canvasWidth, int canvasHeigh)
        {
            var texTransUnityCoreEngine = new TTUnityCoreEngine(textureManager.LoadTexture);
            var canvasCtx = new TexTransCore.MultiLayerImageCanvas.CanvasContext(texTransUnityCoreEngine);
            Profiler.BeginSample("ctr and GetRootLayerObjects");
            var canvas = new TexTransCore.MultiLayerImageCanvas.Canvas(canvasWidth, canvasHeigh, GetRootLayerObjects(textureManager));
            Profiler.EndSample();

            return canvasCtx.EvaluateCanvas(canvas).ToUnity();
        }

        internal List<TexTransCore.MultiLayerImageCanvas.LayerObject> GetRootLayerObjects(ITextureManager textureManager)
        {
            var layers = GetChileLayers();
            var list = new List<TexTransCore.MultiLayerImageCanvas.LayerObject>(layers.Capacity);
            foreach (var l in layers) { list.Add(l.GetLayerObject(textureManager)); }
            return list;
        }

        internal void LookAtCallingCanvas(ILookingObject looker)
        {
            TextureSelector.LookAtCalling(looker);
            looker.LookAt(this);
            looker.LookAtChildeComponents<AbstractLayer>(gameObject);
            foreach (var cl in GetChileLayers()) { cl.LookAtCalling(looker); }
        }

        internal override IEnumerable<Renderer> ModificationTargetRenderers(IEnumerable<Renderer> domainRenderers, OriginEqual replaceTracking)
        {
            return TextureSelector.ModificationTargetRenderers(domainRenderers, replaceTracking);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        List<AbstractLayer> GetChileLayers() { return GetChileLayers(transform); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static List<AbstractLayer> GetChileLayers(Transform transform)
        {
            var tf = transform;
            var chilesCount = tf.childCount;
            var chiles = new List<AbstractLayer>(chilesCount);

            for (var i = chilesCount - 1; 0 <= i; i -= 1)
            {
                var layer = tf.GetChild(i).GetComponent<AbstractLayer>();
                if (layer != null)
                {
                    chiles.Add(layer);
                }
            }

            return chiles;
        }

        internal static int NormalizePowOfTow(int v)
        {
            if (Mathf.IsPowerOfTwo(v)) { return v; }

            var nextV = Mathf.NextPowerOfTwo(v);
            var closetV = Mathf.ClosestPowerOfTwo(v);

            if (Mathf.Abs(nextV - v) > Mathf.Abs(closetV - v)) { return closetV; }
            else { return nextV; }
        }
    }
}
