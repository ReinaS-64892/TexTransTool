#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public struct CanvasContext<TTCE>
    where TTCE : ITexTransCreateTexture
    , ITexTransLoadTexture
    , ITexTransCopyRenderTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    {
        private TTCE _engine;

        public CanvasContext(TTCE engine)
        {
            _engine = engine;
        }

        public ITTRenderTexture EvaluateCanvas(Canvas<TTCE> canvas)
        {
            var canvasTexture = _engine.CreateRenderTexture(canvas.Width, canvas.Height);

            EvaluateForFlattened(canvasTexture, null, ToBelowFlattened(canvas.RootLayers));

            return canvasTexture;

        }

        public readonly void EvaluateForFlattened(ITTRenderTexture canvasTex, EvaluateContext<TTCE>? evalCtx, IEnumerable<PreBlendPairedLayer> flattened)
        {
            foreach (var pairedLayer in flattened)
            {
                if (pairedLayer.AbstractLayer.Visible is false) { continue; }

                switch (pairedLayer.AbstractLayer)
                {
                    case ImageLayer<TTCE> imageLayer:
                        {
                            using (var layerRt = _engine.CreateRenderTexture(canvasTex.Width, canvasTex.Hight))
                            {
                                imageLayer.GetImage(_engine, layerRt);

                                using (var nEvalCtx = EvaluateContext<TTCE>.NestContext(_engine, canvasTex.Width, canvasTex.Hight, evalCtx, imageLayer.AlphaMask, pairedLayer.PreBlends))
                                {
                                    nEvalCtx.AlphaMask.Masking(_engine, layerRt);
                                    if (nEvalCtx.PreBlends is not null && nEvalCtx.PreBlends.Any() is true)
                                        EvaluateForFlattened(layerRt, null, nEvalCtx.PreBlends.Select(l => new PreBlendPairedLayer(l, null)));
                                }
                                BlendForAlphaOperation(canvasTex, imageLayer, layerRt);
                            }
                            break;
                        }
                    case GrabLayer<TTCE> grabLayer:
                        {
                            using (var nEvalCtx = EvaluateContext<TTCE>.NestContext(_engine, canvasTex.Width, canvasTex.Hight, evalCtx, grabLayer.AlphaMask, pairedLayer.PreBlends))
                            {
                                grabLayer.GrabImage(_engine, nEvalCtx, canvasTex);
                            }
                            break;
                        }

                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void BlendForAlphaOperation(ITTRenderTexture canvasTexture, ImageLayer<TTCE> imageLayer, ITTRenderTexture layerRt)
        {
            var alphaOperation = imageLayer.AlphaOperation;
            var blendKey = imageLayer.BlendTypeKey;
            BlendForAlphaOperation(_engine, canvasTexture, layerRt, alphaOperation, blendKey);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlendForAlphaOperation(TTCE engine, ITTRenderTexture canvasTexture, ITTRenderTexture layerRt, AlphaOperation alphaOperation, ITTBlendKey blendKey)
        {
            switch (alphaOperation)
            {
                default:
                case AlphaOperation.Normal:
                    {
                        engine.Blending(canvasTexture, layerRt, blendKey);
                        break;
                    }
                case AlphaOperation.Inherit:
                    {
                        using var alphaTemp = engine.CreateRenderTexture(canvasTexture.Width, canvasTexture.Hight);

                        engine.AlphaCopy(alphaTemp, canvasTexture);
                        engine.AlphaFill(canvasTexture, 1f);//クリッピングの場合はこうしないと困るが、そうではない場合が必要になるなら、ここの case の数を増やす必要がある。

                        engine.Blending(canvasTexture, layerRt, blendKey);

                        engine.AlphaCopy(canvasTexture, alphaTemp);

                        break;
                    }
                case AlphaOperation.Layer:
                    {
                        engine.AlphaFill(canvasTexture, 1f);//これが必要かは考えるべき
                        engine.Blending(canvasTexture, layerRt, blendKey);
                        engine.AlphaCopy(canvasTexture, layerRt);

                        break;
                    }
                case AlphaOperation.Intersect:
                    {
                        using var alphaTemp = engine.CreateRenderTexture(canvasTexture.Width, canvasTexture.Hight);

                        engine.AlphaCopy(alphaTemp, canvasTexture);
                        engine.AlphaFill(canvasTexture, 1f);//これが必要かは考えるべき

                        engine.Blending(canvasTexture, layerRt, blendKey);

                        engine.AlphaMultiplyWithTexture(alphaTemp, layerRt);
                        engine.AlphaCopy(canvasTexture, alphaTemp);

                        break;
                    }
            }
        }

        public static List<PreBlendPairedLayer> ToBelowFlattened(IEnumerable<LayerObject<TTCE>> layerObjects)
        {
            var layerStack = new Stack<PreBlendPairedLayer>();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void PushToStack(LayerObject<TTCE> l, bool forcedNull = false)
            {
                layerStack.Push(new PreBlendPairedLayer(l, forcedNull is false ? new() : null));
            }

            foreach (var layer in layerObjects)
            {
                if (layer.PreBlendToLayerBelow)
                {
                    if (layerStack.TryPeek(out var belowPairedLayer))
                        if (belowPairedLayer.PreBlends is not null)
                        {
                            belowPairedLayer.PreBlends.Add(layer);
                            continue;
                        }

                    PushToStack(layer, true);
                }
                else
                {
                    PushToStack(layer);
                }
            }

            return layerStack.Reverse().ToList();
        }

        public class PreBlendPairedLayer
        {
            public LayerObject<TTCE> AbstractLayer;
            public List<LayerObject<TTCE>>? PreBlends;//これが存在しないということは、それの上のレイヤーが先行合成ができないということ。

            public PreBlendPairedLayer(LayerObject<TTCE> abstractLayer, List<LayerObject<TTCE>>? preBlends)
            {
                AbstractLayer = abstractLayer;
                PreBlends = preBlends;
            }
        }


    }


    public class Canvas<TTCE>
    where TTCE : ITexTransCreateTexture
    , ITexTransLoadTexture
    , ITexTransCopyRenderTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    {
        public int Width;
        public int Height;
        public List<LayerObject<TTCE>> RootLayers;

        public Canvas(int width, int height, List<LayerObject<TTCE>> rootLayers)
        {
            Width = width;
            Height = height;
            RootLayers = rootLayers;
        }
    }
    public class EvaluateContext<TTCE> : IDisposable
    where TTCE : ITexTransCreateTexture
    , ITexTransLoadTexture
    , ITexTransCopyRenderTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    {
        TextureToMask<TTCE> _alphaMask;
        List<LayerObject<TTCE>>? _preBlends;

        public EvaluateContext(ITTRenderTexture nowAlphaMask, List<LayerObject<TTCE>>? preBlends)
        {
            _alphaMask = new(nowAlphaMask);
            _preBlends = preBlends;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EvaluateContext<TTCE> NestContext(TTCE engine, int width, int height, EvaluateContext<TTCE>? sourceContext, AlphaMask<TTCE> addAlphaMask, List<LayerObject<TTCE>>? addPreBlends)
        {
            var newMask = engine.CreateRenderTexture(width, height);
            engine.AlphaFill(newMask, 1);

            sourceContext?._alphaMask.Masking(engine, newMask);
            addAlphaMask.Masking(engine, newMask);

            var margePreBlends = sourceContext?._preBlends is not null || addPreBlends is not null ? new List<LayerObject<TTCE>>() : null;
            if (sourceContext?._preBlends is not null && margePreBlends is not null) { margePreBlends.AddRange(sourceContext._preBlends); }
            if (addPreBlends is not null && margePreBlends is not null) { margePreBlends.AddRange(addPreBlends); }

            return new(newMask, margePreBlends);
        }

        public AlphaMask<TTCE> AlphaMask => _alphaMask;
        public IEnumerable<LayerObject<TTCE>>? PreBlends => _preBlends;

        public void Dispose()
        {
            _alphaMask.Dispose();
        }
    }
}
