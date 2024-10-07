#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Codice.ThemeImages;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public struct CanvasContext
    {
        private ITTEngine _engine;

        public CanvasContext(ITTEngine engine)
        {
            _engine = engine;
        }

        public ITTRenderTexture EvaluateCanvas(Canvas canvas)
        {
            var canvasTexture = _engine.CreateRenderTexture(canvas.Width, canvas.Height);

            EvaluateForFlattened(canvasTexture, null, ToBelowFlattened(canvas.RootLayers));

            return canvasTexture;

        }

        public readonly void EvaluateForFlattened(ITTRenderTexture canvasTex, EvaluateContext? evalCtx, IEnumerable<PreBlendPairedLayer> flattened)
        {
            foreach (var pairedLayer in flattened)
            {
                if (pairedLayer.AbstractLayer.Visible is false) { continue; }

                switch (pairedLayer.AbstractLayer)
                {
                    case ImageLayer imageLayer:
                        {
                            using (var layerRt = _engine.CreateRenderTexture(canvasTex.Width, canvasTex.Hight))
                            {
                                imageLayer.GetImage(_engine, layerRt);

                                using (var nEvalCtx = EvaluateContext.NestContext(_engine, canvasTex.Width, canvasTex.Hight, evalCtx, imageLayer.AlphaMask, pairedLayer.PreBlends))
                                {
                                    nEvalCtx.AlphaMask.Masking(_engine, layerRt);
                                    if (nEvalCtx.PreBlends is not null && nEvalCtx.PreBlends.Any() is true)
                                        EvaluateForFlattened(layerRt, null, nEvalCtx.PreBlends.Select(l => new PreBlendPairedLayer(l, null)));
                                }
                                BlendForAlphaOperation(canvasTex, imageLayer, layerRt);
                            }
                            break;
                        }
                    case GrabLayer grabLayer:
                        {
                            using (var nEvalCtx = EvaluateContext.NestContext(_engine, canvasTex.Width, canvasTex.Hight, evalCtx, grabLayer.AlphaMask, pairedLayer.PreBlends))
                            {
                                grabLayer.GrabImage(_engine, nEvalCtx, canvasTex);
                            }
                            break;
                        }

                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void BlendForAlphaOperation(ITTRenderTexture canvasTexture, ImageLayer imageLayer, ITTRenderTexture layerRt)
        {
            var alphaOperation = imageLayer.AlphaOperation;
            var blendKey = imageLayer.BlendTypeKey;
            BlendForAlphaOperation(_engine, canvasTexture, layerRt, alphaOperation, blendKey);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlendForAlphaOperation(ITTEngine engine, ITTRenderTexture canvasTexture, ITTRenderTexture layerRt, AlphaOperation alphaOperation, ITTBlendKey blendKey)
        {
            switch (alphaOperation)
            {
                default:
                case AlphaOperation.Normal:
                    {
                        engine.TextureBlend(canvasTexture, layerRt, blendKey);
                        break;
                    }
                case AlphaOperation.Inherit:
                    {
                        using (var alphaTemp = engine.CreateRenderTexture(canvasTexture.Width, canvasTexture.Hight))
                        {
                            engine.CopyAlpha(canvasTexture, alphaTemp);
                            engine.FillAlpha(canvasTexture, 1f);//クリッピングの場合はこうしないと困るが、そうではない場合が必要になるなら、ここの case の数を増やす必要がある。
                            engine.TextureBlend(canvasTexture, layerRt, blendKey);
                            engine.CopyAlpha(alphaTemp, canvasTexture);
                        }
                        break;
                    }
                case AlphaOperation.Layer:
                    {
                        engine.FillAlpha(canvasTexture, 1f);//これが必要かは考えるべき
                        engine.TextureBlend(canvasTexture, layerRt, blendKey);
                        engine.CopyAlpha(layerRt, canvasTexture);

                        break;
                    }
                case AlphaOperation.Intersect:
                    {
                        using (var alphaTemp = engine.CreateRenderTexture(canvasTexture.Width, canvasTexture.Hight))
                        {
                            engine.CopyAlpha(canvasTexture, alphaTemp);
                            engine.FillAlpha(canvasTexture, 1f);//これが必要かは考えるべき
                            engine.TextureBlend(canvasTexture, layerRt, blendKey);

                            engine.MulAlpha(alphaTemp, layerRt);
                            engine.CopyAlpha(alphaTemp, canvasTexture);
                        }
                        break;
                    }
            }
        }

        public static List<PreBlendPairedLayer> ToBelowFlattened(IEnumerable<LayerObject> layerObjects)
        {
            var layerStack = new Stack<PreBlendPairedLayer>();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void PushToStack(LayerObject l, bool forcedNull = false)
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
            public LayerObject AbstractLayer;
            public List<LayerObject>? PreBlends;//これが存在しないということは、それの上のレイヤーが先行合成ができないということ。

            public PreBlendPairedLayer(LayerObject abstractLayer, List<LayerObject>? preBlends)
            {
                AbstractLayer = abstractLayer;
                PreBlends = preBlends;
            }
        }


    }


    public class Canvas
    {
        public int Width;
        public int Height;
        public List<LayerObject> RootLayers;

        public Canvas(int width, int height, List<LayerObject> rootLayers)
        {
            Width = width;
            Height = height;
            RootLayers = rootLayers;
        }
    }
    public class EvaluateContext : IDisposable
    {
        ITTRenderTexture _maskTexture;
        TextureToMask _alphaMask;
        List<LayerObject>? _preBlends;

        public EvaluateContext(ITTRenderTexture nowAlphaMask, List<LayerObject>? preBlends)
        {
            _maskTexture = nowAlphaMask;
            _alphaMask = new(nowAlphaMask);
            _preBlends = preBlends;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EvaluateContext NestContext(ITTEngine engine, int width, int height, EvaluateContext? sourceContext, AlphaMask addAlphaMask, List<LayerObject>? addPreBlends)
        {
            var newMask = engine.CreateRenderTexture(width, height);
            engine.FillAlpha(newMask, 1);

            sourceContext?._alphaMask.Masking(engine, newMask);
            addAlphaMask.Masking(engine, newMask);

            var margePreBlends = sourceContext?._preBlends is not null || addPreBlends is not null ? new List<LayerObject>() : null;
            if (sourceContext?._preBlends is not null && margePreBlends is not null) { margePreBlends.AddRange(sourceContext._preBlends); }
            if (addPreBlends is not null && margePreBlends is not null) { margePreBlends.AddRange(addPreBlends); }

            return new(newMask, margePreBlends);
        }

        public AlphaMask AlphaMask => _alphaMask;
        public IEnumerable<LayerObject>? PreBlends => _preBlends;

        public void Dispose()
        {
            _maskTexture.Dispose();
        }
    }
}
