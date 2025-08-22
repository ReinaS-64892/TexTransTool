#nullable enable
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using UnityEngine.Profiling;
using System.Runtime.CompilerServices;
using net.rs64.TexTransCore;
using UnityEngine.Serialization;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    /*
        IExternalToolCanBehaveAsLayer に [TexTransToolStablePublicAPI] が実装されたときこのコンポーネントは

        `net.rs64.TexTransTool.MultiLayerImage.MultiLayerImageCanvas, net.rs64.tex-trans-tool.runtime`

        という名前空間とアセンブリ名と名前から変更されなくなり、型名は安全に使用できます。
    */
    public sealed class MultiLayerImageCanvas : TexTransBehavior
    {
        internal const string FoldoutName = "MultiLayerImage";
        internal const string ComponentName = "TTT MultiLayerImageCanvas";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.BeforeUVModification;

        [FormerlySerializedAs("TextureSelector")] public TextureSelector TargetTexture = new();

        [SerializeField, HideInInspector] public TTTImportedCanvasDescription? tttImportedCanvasDescription;

        internal override void Apply(IDomain domain)
        {
            var replaceTarget = domain.ObserveToGet(this, b => b.TargetTexture.SelectTexture);
            if (replaceTarget == null) { TTLog.Info("MultiLayerImageCanvas:info:TargetNotSet"); return; }

            var nowDomainsTargets = domain.GetDomainsTextures(replaceTarget).ToHashSet();
            if (nowDomainsTargets.Any() is false) { TTLog.Info("MultiLayerImageCanvas:info:TargetNotFound"); return; }

            var targetContainsMaterials = domain.GetAllMaterials().Where(m => m.EnumerateReferencedTextures().Any(nowDomainsTargets.Contains)).ToHashSet();

            var canvasWidth = tttImportedCanvasDescription?.Width ?? NormalizePowOfTow(replaceTarget.width);
            var canvasHeigh = tttImportedCanvasDescription?.Height ?? NormalizePowOfTow(replaceTarget.height);
            if (domain.GetCustomContext<DomainPreviewCtx>()?.IsPreview ?? false)
            {
                canvasWidth = Mathf.Min(1024, canvasWidth);
                canvasHeigh = Mathf.Min(1024, canvasHeigh);
            }

            Profiler.BeginSample("EvaluateCanvas");
            var texTransUnityCoreEngine = domain.GetTexTransCoreEngineForUnity();
            using var result = EvaluateCanvas(new(domain, (canvasWidth, canvasHeigh), targetContainsMaterials));
            Profiler.EndSample();

            var notBlendKey = texTransUnityCoreEngine.QueryBlendKey("NotBlend");
            foreach (var target in nowDomainsTargets) { domain.AddTextureStack(target, result, notBlendKey); }
        }

        internal ITTRenderTexture EvaluateCanvas(GenerateLayerObjectContext ctx)
        {
            var canvasCtx = new TexTransCore.MultiLayerImageCanvas.CanvasContext<ITexTransToolForUnity>(ctx.Engine);
            Profiler.BeginSample("GenerateLayerObject");
            using var canvas = new TexTransCore.MultiLayerImageCanvas.Canvas<ITexTransToolForUnity>(ctx.CanvasSize.x, ctx.CanvasSize.y, GenerateLayerObject(ctx));
            Profiler.EndSample();
            return canvasCtx.EvaluateCanvas(canvas);
        }
        internal List<TexTransCore.MultiLayerImageCanvas.LayerObject<ITexTransToolForUnity>> GenerateLayerObject(GenerateLayerObjectContext ctx)
        {
            ctx.Domain.Observe(this);
            ctx.Domain.ObserveToChildeComponents<TexTransMonoBase>(gameObject);
            var layers = GetChileLayers();
            var list = new List<TexTransCore.MultiLayerImageCanvas.LayerObject<ITexTransToolForUnity>>(layers.Capacity);
            foreach (var l in layers) { list.Add(l.GetLayerObject(ctx)); }
            return list;
        }


        internal override IEnumerable<Renderer> TargetRenderers(IDomainReferenceViewer domainView)
        {
            return TextureSelector.TargetRenderers(domainView.ObserveToGet(this, b => b.TargetTexture.SelectTexture), domainView);
        }

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
