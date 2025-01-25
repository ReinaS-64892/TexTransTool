#nullable enable
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using UnityEngine.Profiling;
using System.Runtime.CompilerServices;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class MultiLayerImageCanvas : TexTransRuntimeBehavior, ITTTChildExclusion
    {
        internal const string FoldoutName = "MultiLayerImage";
        internal const string ComponentName = "TTT MultiLayerImageCanvas";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.BeforeUVModification;

        public TextureSelector TextureSelector = new();

        [SerializeField, HideInInspector] public TTTImportedCanvasDescription? tttImportedCanvasDescription;

        internal override void Apply(IDomain domain)
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
            var texTransUnityCoreEngine = domain.GetTexTransCoreEngineForUnity();
            using var result = EvaluateCanvas(texTransUnityCoreEngine, canvasWidth, canvasHeigh);
            Profiler.EndSample();

            // TextureBlend.ToLinear(result);
            var notBlendKey = texTransUnityCoreEngine.QueryBlendKey("NotBlend");

            foreach (var target in nowDomainsTargets) { domain.AddTextureStack(target, result, notBlendKey); }
        }

        internal ITTRenderTexture EvaluateCanvas<TTCE4U>(TTCE4U texTransCoreEngine, int canvasWidth, int canvasHeigh)
        where TTCE4U : ITexTransToolForUnity
        , ITexTransCreateTexture
        , ITexTransLoadTexture
        , ITexTransCopyRenderTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        {
            var canvasCtx = new TexTransCore.MultiLayerImageCanvas.CanvasContext<TTCE4U>(texTransCoreEngine);
            Profiler.BeginSample("ctr and GetRootLayerObjects");
            var canvas = new TexTransCore.MultiLayerImageCanvas.Canvas<TTCE4U>(canvasWidth, canvasHeigh, GetRootLayerObjects<TTCE4U>(texTransCoreEngine));
            Profiler.EndSample();

            return canvasCtx.EvaluateCanvas(canvas);
        }

        internal List<TexTransCore.MultiLayerImageCanvas.LayerObject<TTCE4U>> GetRootLayerObjects<TTCE4U>(TTCE4U engine)
        where TTCE4U : ITexTransToolForUnity
        , ITexTransCreateTexture
        , ITexTransLoadTexture
        , ITexTransCopyRenderTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        {
            var layers = GetChileLayers();
            var list = new List<TexTransCore.MultiLayerImageCanvas.LayerObject<TTCE4U>>(layers.Capacity);
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

        internal override IEnumerable<Renderer> ModificationTargetRenderers(IRendererTargeting rendererTargeting)
        {
            return TextureSelector.ModificationTargetRenderers(rendererTargeting);
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
