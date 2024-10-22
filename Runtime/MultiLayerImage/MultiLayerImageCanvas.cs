using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransCoreEngineForUnity.Utils;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using Color = UnityEngine.Color;
using UnityEngine.Profiling;
using System.Runtime.CompilerServices;
using net.rs64.TexTransCore;
using static net.rs64.TexTransCoreEngineForUnity.TextureBlend;

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

        [SerializeField, HideInInspector] public TTTImportedCanvasDescription tttImportedCanvasDescription;

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
            var texTransUnityCoreEngine = new TTCE4UnityWithTTT4Unity(domain.IsPreview(), domain.GetTextureManager());
            var result = EvaluateCanvas(texTransUnityCoreEngine, canvasWidth, canvasHeigh).Unwrap();
            Profiler.EndSample();

            foreach (var target in nowDomainsTargets) { domain.AddTextureStack(target, new BlendTexturePair(result, "NotBlend")); }
        }

        internal ITTRenderTexture EvaluateCanvas<TTT4U>(TTT4U texTransCoreEngine, int canvasWidth, int canvasHeigh)
        where TTT4U : ITexTransToolForUnity
        , ITexTransGetTexture
        , ITexTransLoadTexture
        , ITexTransRenderTextureOperator
        , ITexTransRenderTextureReScaler
        , ITexTranBlending
        {
            var canvasCtx = new TexTransCore.MultiLayerImageCanvas.CanvasContext<TTT4U>(texTransCoreEngine);
            Profiler.BeginSample("ctr and GetRootLayerObjects");
            var canvas = new TexTransCore.MultiLayerImageCanvas.Canvas<TTT4U>(canvasWidth, canvasHeigh, GetRootLayerObjects<TTT4U>(texTransCoreEngine));
            Profiler.EndSample();

            return canvasCtx.EvaluateCanvas(canvas);
        }

        internal List<TexTransCore.MultiLayerImageCanvas.LayerObject<TTT4U>> GetRootLayerObjects<TTT4U>(TTT4U engine)
        where TTT4U : ITexTransToolForUnity
        , ITexTransGetTexture
        , ITexTransLoadTexture
        , ITexTransRenderTextureOperator
        , ITexTransRenderTextureReScaler
        , ITexTranBlending
        {
            var layers = GetChileLayers();
            var list = new List<TexTransCore.MultiLayerImageCanvas.LayerObject<TTT4U>>(layers.Capacity);
            foreach (var l in layers) { list.Add(l.GetLayerObject(engine)); }
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
