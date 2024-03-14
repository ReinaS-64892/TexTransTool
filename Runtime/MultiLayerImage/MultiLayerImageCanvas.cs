using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class MultiLayerImageCanvas : TexTransRuntimeBehavior, ITTTChildExclusion
    {
        internal const string FoldoutName = "MultiLayerImage";
        internal const string ComponentName = "TTT MultiLayerImageCanvas";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        internal override List<Renderer> GetRenderers => new List<Renderer>() { TextureSelector.RendererAsPath };
        internal override bool IsPossibleApply => TextureSelector.GetTexture() != null;
        internal override TexTransPhase PhaseDefine => TexTransPhase.BeforeUVModification;

        public TextureSelector TextureSelector;

        [SerializeField, HideInInspector] internal TTTImportedCanvasDescription tttImportedCanvasDescription;

        internal override void Apply([NotNull] IDomain domain)
        {
            if (!IsPossibleApply) { throw new TTTNotExecutable(); }
            var replaceTarget = TextureSelector.GetTexture(domain);


            var Layers = transform.GetChildren()
            .Select(I => I.GetComponent<AbstractLayer>())
            .Where(I => I != null)
            .Reverse();

            var canvasSize = tttImportedCanvasDescription?.Width ?? NormalizePowOfTow(replaceTarget.width);
            if (domain.IsPreview()) { canvasSize = Mathf.Min(1024, canvasSize); }

            var canvasContext = new CanvasContext(canvasSize, domain.GetTextureManager());
            foreach (var layer in Layers) { layer.EvaluateTexture(canvasContext); }
            var result = canvasContext.LayerCanvas.FinalizeCanvas();
            domain.AddTextureStack(replaceTarget, new BlendTexturePair(result, "NotBlend"));

        }
        internal static int NormalizePowOfTow(int v)
        {
            if (Mathf.IsPowerOfTwo(v)) { return v; }

            var nextV = Mathf.NextPowerOfTwo(v);
            var closetV = Mathf.ClosestPowerOfTwo(v);

            if (Mathf.Abs(nextV - v) > Mathf.Abs(closetV - v)) { return closetV; }
            else { return nextV; }
        }

        internal class CanvasContext
        {
            public ITextureManager TextureManager;
            public int CanvasSize;

            public LayerCanvas LayerCanvas;

            public CanvasContext(int canvasSize, ITextureManager textureManager)
            {
                CanvasSize = canvasSize;
                LayerCanvas = new LayerCanvas(RenderTexture.GetTemporary(canvasSize, canvasSize));
                TextureManager = textureManager;
            }
            public CanvasContext CreateSubCanvas => new CanvasContext(CanvasSize, TextureManager);
        }

        internal class LayerCanvas
        {
            RenderTexture _canvas;
            Stack<LayerScope> _layerScopes;

            LayerScope? NowScope => _layerScopes.Count == 0 ? null : _layerScopes.Peek();



            //これが Null な場合は下のレイヤーが非表示にされていて、クリッピングでの追加は無効化される。
            //非表示レイヤーでもクリッピングを無効化する場合は NotClipping を入れること
            IClippingTarget _nowClippingTarget = DisallowClippingLayer.Default;


            public LayerCanvas(RenderTexture renderTexture)
            {
                _canvas = renderTexture; _canvas.Clear();
                _layerScopes = new();
            }
            public void AddLayer(BlendRenderTexture blendLayer, LayerAlphaMod layerAlphaMod, bool thisClipping)
            {
                if (NowScope.HasValue) { NowScope.Value.AddLayer(blendLayer, layerAlphaMod, thisClipping); }
                else
                {
                    if (thisClipping)
                    {
                        if (_nowClippingTarget != null)
                        {
                            AlphaModApply(blendLayer, layerAlphaMod);
                            layerAlphaMod.Dispose();

                            _nowClippingTarget.DrawOnClipping(blendLayer.ToEval());
                        }
                        else { RenderTexture.ReleaseTemporary(blendLayer.Texture); layerAlphaMod.Dispose(); return; }
                    }
                    else
                    {
                        AlphaModApply(blendLayer, layerAlphaMod);
                        layerAlphaMod.Dispose();

                        Composite(blendLayer);
                    }
                }
            }

            //GrabCanvasModifiedAction の実行タイミングは即時ではない可能性がある。
            public void GrabCanvas(Action<RenderTexture, RenderTexture> GrabCanvasModifiedAction, LayerAlphaMod layerAlphaMod, string blendTypeKey, bool GrabForClipping)//左がGrabSouse 右がWriteTarget
            {
                if (NowScope.HasValue) { NowScope.Value.GrabCanvas(GrabCanvasModifiedAction, layerAlphaMod, blendTypeKey, GrabForClipping); }
                else
                {
                    var evalGrab = new EvalGrabLayer(GrabCanvasModifiedAction, layerAlphaMod, blendTypeKey);
                    if (GrabForClipping)
                    {
                        if (_nowClippingTarget == null) { return; }
                        _nowClippingTarget.DrawOnClipping(evalGrab);
                    }
                    else
                    {
                        CompositeClippingTarget();
                        _nowClippingTarget = new DisallowClippingLayer(evalGrab);
                    }
                }
            }
            public void AddHiddenLayer(bool thisClipping, bool disallowClipping)
            {
                if (NowScope.HasValue)
                {
                    NowScope.Value.AddHiddenLayer(thisClipping, disallowClipping);
                }
                else
                {
                    if (thisClipping) { return; }
                    CompositeClippingTarget();
                    if (!disallowClipping) { _nowClippingTarget = null; }
                    else { _nowClippingTarget = DisallowClippingLayer.Default; }
                }
            }
            internal static void AlphaModApply(BlendRenderTexture newLayer, LayerAlphaMod alphaMod)
            {
                if (newLayer.Texture != null)
                {
                    if (alphaMod.Mask != null)
                    {
                        MaskDrawRenderTexture(newLayer.Texture, alphaMod.Mask);
                    }
                    if (!Mathf.Approximately(alphaMod.Opacity, 1))
                    {
                        MultipleRenderTexture(newLayer.Texture, new Color(1, 1, 1, alphaMod.Opacity));
                    }
                }
            }

            void Composite(BlendRenderTexture newLayer)
            {
                CompositeClippingTarget();

                _nowClippingTarget = new ClippingLayer(newLayer.ToEval());

                // if (!disallowClipping) { _nowClippingTarget = new ClippingLayer(newLayer.ToEval()); }
                // else { _nowClippingTarget = new DisallowClippingLayer(newLayer.ToEval()); }
            }

            void CompositeClippingTarget()//使った後はちゃんと _nowClippingTarget に次のやつを入れるようにね
            {
                if (_nowClippingTarget == null) { return; }
                foreach (var evalBlending in _nowClippingTarget.ExtractLayers())
                {
                    evalBlending.EvalDrawCanvas(_canvas);
                    evalBlending.Dispose();
                }
                _nowClippingTarget = null;
            }



            public RenderTexture FinalizeCanvas() { CompositeClippingTarget(); return _canvas; }

            internal static void DrawOnClipping(BlendRenderTexture drawTargetLayer, IEvaluateBlending clippingLayer)
            {
                var targetRt = drawTargetLayer.Texture;
                var swap = RenderTexture.GetTemporary(targetRt.descriptor);
                Graphics.CopyTexture(targetRt, swap);

                TextureBlend.AlphaOne(targetRt);
                clippingLayer.EvalDrawCanvas(targetRt);
                TextureBlend.AlphaCopy(swap, targetRt);

                RenderTexture.ReleaseTemporary(swap);
            }

            public LayerScopeUsingStruct UsingLayerScope(LayerAlphaMod layerAlphaMod) { EnterLayerScope(layerAlphaMod); return new(this); }
            private void EnterLayerScope(LayerAlphaMod layerAlphaMod)
            {
                layerAlphaMod.Mask ??= GetTempRtMask();
                if (NowScope.HasValue) { LayerAlphaAnd(ref layerAlphaMod, NowScope.Value.AlphaMod); }
                _layerScopes.Push(new LayerScope(layerAlphaMod));
            }

            private RenderTexture GetTempRtMask()
            {
                var rt = RenderTexture.GetTemporary(_canvas.descriptor);
                TextureBlend.ColorBlit(rt, Color.white);
                return rt;
            }

            private void ExitLayerScope()
            {
                var clippingTarget = _layerScopes.Pop().ExitScope();

                if (NowScope.HasValue) { NowScope.Value.AddStack(clippingTarget); }
                else
                {
                    CompositeClippingTarget();
                    _nowClippingTarget = clippingTarget;
                }
            }
            internal static void LayerAlphaAnd(ref LayerAlphaMod target, LayerAlphaMod and)
            {
                if (target.Mask == null) { target.Mask = RenderTexture.GetTemporary(and.Mask.descriptor); TextureBlend.ColorBlit(target.Mask, Color.white); }
                if (and.Mask != null) { TextureBlend.MaskDrawRenderTexture(target.Mask, and.Mask); }

                target.Opacity *= and.Opacity;
            }
            public struct LayerScopeUsingStruct : IDisposable
            {
                private LayerCanvas layerCanvas;
                public LayerScopeUsingStruct(LayerCanvas layerCanvas) { this.layerCanvas = layerCanvas; }
                public void Dispose() { layerCanvas.ExitLayerScope(); }
            }

            private struct LayerScope
            {
                LayerAlphaMod _alphaMod;
                public LayerAlphaMod AlphaMod => _alphaMod;
                Stack<IClippingTarget> _layerStack;
                IClippingTarget _nowClippingTarget { get { if (_layerStack.TryPeek(out var result)) { return result; } else { return null; } } }


                public LayerScope(LayerAlphaMod alphaMod)
                {
                    _alphaMod = alphaMod;
                    _layerStack = new();
                }

                internal void AddStack(IClippingTarget clippingTarget) => _layerStack.Push(clippingTarget);


                public void AddLayer(BlendRenderTexture blendLayer, LayerAlphaMod layerAlphaMod, bool thisClipping)
                {
                    if (thisClipping)
                    {
                        if (_nowClippingTarget != null)
                        {
                            //正常なクリッピング
                            LayerAlphaAnd(ref layerAlphaMod, _alphaMod);
                            AlphaModApply(blendLayer, layerAlphaMod);
                            layerAlphaMod.Dispose();

                            _nowClippingTarget.DrawOnClipping(blendLayer.ToEval());

                            return;
                        }
                        else { RenderTexture.ReleaseTemporary(blendLayer.Texture); layerAlphaMod.Dispose(); return; }
                    }
                    else
                    {
                        LayerAlphaAnd(ref layerAlphaMod, _alphaMod);
                        AlphaModApply(blendLayer, layerAlphaMod);
                        layerAlphaMod.Dispose();

                        _layerStack.Push(new ClippingLayer(blendLayer.ToEval()));
                        // if (!disallowClipping) { _layerStack.Push(new ClippingLayer(blendLayer.ToEval())); }
                        // else { _layerStack.Push(new DisallowClippingLayer(blendLayer.ToEval())); }

                        return;
                    }
                }

                public void AddHiddenLayer(bool thisClipping, bool disallowClipping)
                {
                    if (thisClipping) { return; }
                    if (!disallowClipping) { _layerStack.Push(null); }
                    else { _layerStack.Push(DisallowClippingLayer.Default); }
                }

                public void GrabCanvas(Action<RenderTexture, RenderTexture> GrabCanvasModifiedAction, LayerAlphaMod layerAlphaMod, string blendTypeKey, bool GrabForClipping)//左がGrabSouse 右がWriteTarget
                {
                    LayerAlphaAnd(ref layerAlphaMod, _alphaMod);

                    var evalGrab = new EvalGrabLayer(GrabCanvasModifiedAction, layerAlphaMod, blendTypeKey);

                    if (GrabForClipping)
                    {
                        if (_nowClippingTarget == null) { return; }
                        _nowClippingTarget.DrawOnClipping(evalGrab);
                    }
                    else { _layerStack.Push(new DisallowClippingLayer(evalGrab)); }
                }

                public ClippingGroup ExitScope()
                {
                    _alphaMod.Dispose();
                    return new ClippingGroup(_layerStack.Reverse().Where(NullFilter).SelectMany(Extractor));

                    static IEnumerable<IEvaluateBlending> Extractor(IClippingTarget i) => i.ExtractLayers();
                    static bool NullFilter(IClippingTarget i) => i != null;
                }
            }
        }

        internal struct LayerAlphaMod : IDisposable
        {
            public RenderTexture Mask;
            public float Opacity;
            public LayerAlphaMod(RenderTexture mask, float opacity)//Temp以外入れないで
            {
                Mask = mask;
                Opacity = opacity;
            }

            public static LayerAlphaMod NonMasked => new(null, 1);

            public void Dispose()
            {
                RenderTexture.ReleaseTemporary(Mask);
                Mask = null;
            }
        }
        interface IClippingTarget//クリッピングの対象を単体と複数で抽象化するための物
        {
            void DrawOnClipping(IEvaluateBlending blendLayer);//RenderTextureの解放責任は奪うよ
            IEnumerable<IEvaluateBlending> ExtractLayers();
        }
        internal struct ClippingGroup : IClippingTarget
        {
            IEnumerable<IEvaluateBlending> _layers;

            internal ClippingGroup(IEnumerable<IEvaluateBlending> layers) { _layers = layers; }
            public void DrawOnClipping(IEvaluateBlending blendLayer)
            {
                using (blendLayer)
                {
                    foreach (var layer in _layers)
                    {
                        if (layer is EvalBlendLayer evalBlendLayer)
                        {
                            LayerCanvas.DrawOnClipping(evalBlendLayer.BlendRenderTexture, blendLayer);
                        }
                    }
                }
            }
            public IEnumerable<IEvaluateBlending> ExtractLayers() => _layers;

        }
        internal struct ClippingLayer : IClippingTarget
        {
            IEvaluateBlending _layer;

            internal ClippingLayer(IEvaluateBlending layer) { _layer = layer; }
            public void DrawOnClipping(IEvaluateBlending blendLayer)
            {
                using (blendLayer)
                {
                    if (_layer is EvalBlendLayer evalBlendLayer)
                    {
                        LayerCanvas.DrawOnClipping(evalBlendLayer.BlendRenderTexture, blendLayer);
                    }
                }
            }
            public IEnumerable<IEvaluateBlending> ExtractLayers() { yield return _layer; }
        }


        internal struct DisallowClippingLayer : IClippingTarget
        {
            List<IEvaluateBlending> _layers;
            public DisallowClippingLayer(IEvaluateBlending layer) { _layers = new() { layer }; }
            public void DrawOnClipping(IEvaluateBlending blendLayer) { _layers.Add(blendLayer); }
            public IEnumerable<IEvaluateBlending> ExtractLayers() => _layers;

            public static DisallowClippingLayer Default => new() { _layers = new() };
        }

        internal interface IEvaluateBlending : IDisposable
        {
            void EvalDrawCanvas(RenderTexture canvas);
        }
        internal struct EvalBlendLayer : IEvaluateBlending //RenderTextureの解放責任は奪う
        {
            BlendRenderTexture _blendRenderTexture;
            internal BlendRenderTexture BlendRenderTexture => _blendRenderTexture;

            public EvalBlendLayer(BlendRenderTexture blendRenderTexture)
            {
                _blendRenderTexture = blendRenderTexture;
            }


            public void EvalDrawCanvas(RenderTexture canvas)
            {
                if (_blendRenderTexture.Texture == null) { return; }
                canvas.BlendBlit(_blendRenderTexture);
            }
            public void Dispose()
            {
                RenderTexture.ReleaseTemporary(_blendRenderTexture.Texture);
                _blendRenderTexture.Texture = null;
            }
        }

        internal struct EvalGrabLayer : IEvaluateBlending
        {
            Action<RenderTexture, RenderTexture> _grabCanvasModifiedAction;
            LayerAlphaMod _layerAlphaMod;
            string _blendTypeKey;
            public EvalGrabLayer(Action<RenderTexture, RenderTexture> grabCanvasModifiedAction, LayerAlphaMod layerAlphaMod, string blendTypeKey)
            {
                _grabCanvasModifiedAction = grabCanvasModifiedAction;
                _layerAlphaMod = layerAlphaMod;
                _blendTypeKey = blendTypeKey;
            }

            public void EvalDrawCanvas(RenderTexture canvas)
            {
                if (_grabCanvasModifiedAction == null) { return; }
                GrabImpl(canvas, _grabCanvasModifiedAction, _layerAlphaMod, _blendTypeKey);
            }
            public void Dispose()
            {
                _grabCanvasModifiedAction = null;
                _layerAlphaMod.Dispose();
                _blendTypeKey = null;
            }


            internal static void GrabImpl(RenderTexture target, Action<RenderTexture, RenderTexture> GrabCanvasModifiedAction, LayerAlphaMod layerAlphaMod, string blendTypeKey)
            {
                var grabRt = RenderTexture.GetTemporary(target.descriptor);
                var writeRt = RenderTexture.GetTemporary(target.descriptor); writeRt.Clear();

                Graphics.CopyTexture(target, grabRt);
                TextureBlend.AlphaOne(grabRt);

                GrabCanvasModifiedAction.Invoke(grabRt, writeRt);

                var blendGrabbedLayer = new BlendRenderTexture(writeRt, blendTypeKey);
                LayerCanvas.AlphaModApply(blendGrabbedLayer, layerAlphaMod);

                var alphaBackup = RenderTexture.GetTemporary(target.descriptor);
                Graphics.CopyTexture(target, alphaBackup);

                AlphaOne(target);
                target.BlendBlit(blendGrabbedLayer);
                AlphaCopy(alphaBackup, target);

                RenderTexture.ReleaseTemporary(alphaBackup);
                RenderTexture.ReleaseTemporary(grabRt);
                RenderTexture.ReleaseTemporary(writeRt);
            }

        }


        internal struct BlendRenderTexture : IBlendTexturePair
        {
            public RenderTexture Texture;
            public string BlendTypeKey;

            public BlendRenderTexture(RenderTexture texture, string blendTypeKey)
            {
                Texture = texture;
                BlendTypeKey = blendTypeKey;
            }

            Texture IBlendTexturePair.Texture => Texture;

            string IBlendTexturePair.BlendTypeKey => BlendTypeKey;


            internal EvalBlendLayer ToEval() => new EvalBlendLayer(this);
        }

    }
}
