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
            var replaceTarget = TextureSelector.GetTextureWithLookAt(domain, this, GetTextureSelector);
            if (replaceTarget == null) { TTTRuntimeLog.Info("MultiLayerImageCanvas:info:TargetNotSet"); return; }

            var nowDomainsTargets = domain.GetDomainsTextures(replaceTarget);
            if (nowDomainsTargets.Any() is false) { TTTRuntimeLog.Info("MultiLayerImageCanvas:info:TargetNotFound"); return; }

            var canvasWidth = tttImportedCanvasDescription?.Width ?? NormalizePowOfTow(replaceTarget.width);
            var canvasHeigh = tttImportedCanvasDescription?.Height ?? NormalizePowOfTow(replaceTarget.width);
            if (domain.IsPreview()) { canvasWidth = Mathf.Min(1024, canvasWidth); canvasHeigh = Mathf.Min(1024, canvasHeigh); }

            Profiler.BeginSample("EvaluateCanvas");
            var texTransUnityCoreEngine = domain.GetTexTransCoreEngineForUnity();
            using var result = EvaluateCanvas(domain, texTransUnityCoreEngine, canvasWidth, canvasHeigh);
            Profiler.EndSample();

            var notBlendKey = texTransUnityCoreEngine.QueryBlendKey("NotBlend");
            foreach (var target in nowDomainsTargets) { domain.AddTextureStack(target, result, notBlendKey); }
        }

        internal ITTRenderTexture EvaluateCanvas(IDomain domain, ITexTransToolForUnity texTransCoreEngine, int canvasWidth, int canvasHeigh)
        {
            var canvasCtx = new TexTransCore.MultiLayerImageCanvas.CanvasContext<ITexTransToolForUnity>(texTransCoreEngine);
            Profiler.BeginSample("ctr and GetRootLayerObjects");
            using var canvas = new TexTransCore.MultiLayerImageCanvas.Canvas<ITexTransToolForUnity>(canvasWidth, canvasHeigh, GetRootLayerObjects(domain, texTransCoreEngine));
            Profiler.EndSample();
            return canvasCtx.EvaluateCanvas(canvas);
        }
        internal List<TexTransCore.MultiLayerImageCanvas.LayerObject<ITexTransToolForUnity>> GetRootLayerObjects(IDomain domain, ITexTransToolForUnity engine)
        {
            domain.LookAt(this);
            domain.LookAtChildeComponents<TexTransMonoBase>(gameObject);
            var layers = GetChileLayers();
            var list = new List<TexTransCore.MultiLayerImageCanvas.LayerObject<ITexTransToolForUnity>>(layers.Capacity);
            foreach (var l in layers) { list.Add(l.GetLayerObject(domain, engine)); }
            return list;
        }

        internal override IEnumerable<Renderer> ModificationTargetRenderers(IRendererTargeting rendererTargeting)
        {
            return TextureSelector.ModificationTargetRenderers(rendererTargeting, this, GetTextureSelector);
        }
        TextureSelector GetTextureSelector(MultiLayerImageCanvas multiLayerImageCanvas) { return multiLayerImageCanvas.TextureSelector; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        List<IMultiLayerImageCanvasLayer> GetChileLayers() { return GetChileLayers(transform); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static List<IMultiLayerImageCanvasLayer> GetChileLayers(Transform transform)
        {
            var tf = transform;
            var chilesCount = tf.childCount;
            var chiles = new List<IMultiLayerImageCanvasLayer>(chilesCount);

            for (var i = chilesCount - 1; 0 <= i; i -= 1)
            {
                var layer = tf.GetChild(i).GetComponent<IMultiLayerImageCanvasLayer>();
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
