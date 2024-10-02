#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
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
            _engine.ClearRenderTexture(canvasTexture, Color.Zero);

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
                                _engine.ClearRenderTexture(layerRt, Color.Zero);
                                imageLayer.GetImage(_engine, layerRt);

                                using (var nEvalCtx = EvaluateContext.NestContext(_engine, canvasTex.Width, canvasTex.Hight, evalCtx, imageLayer.AlphaModifier, pairedLayer.PreBlends))
                                {
                                    if (nEvalCtx.PreBlends is not null) EvaluateForFlattened(layerRt, null, nEvalCtx.PreBlends.Select(l => new PreBlendPairedLayer(l, null)));
                                    nEvalCtx.AlphaMask.Masking(_engine, layerRt);
                                }
                                BlendForAlphaOperation(canvasTex, imageLayer, layerRt);
                            }
                            break;
                        }
                    case GrabLayer grabLayer:
                        {
                            using (var nEvalCtx = EvaluateContext.NestContext(_engine, canvasTex.Width, canvasTex.Hight, evalCtx, grabLayer.AlphaModifier, pairedLayer.PreBlends))
                            {
                                grabLayer.GrabImage(_engine, nEvalCtx, canvasTex);
                            }
                            break;
                        }

                }
            }
        }

        public readonly void BlendForAlphaOperation(ITTRenderTexture canvasTexture, ImageLayer imageLayer, ITTRenderTexture layerRt)
        {
            switch (imageLayer.AlphaOperation)
            {
                default:
                case AlphaOperation.Normal:
                    {
                        _engine.TextureBlend(canvasTexture, layerRt, imageLayer.BlendTypeKey);
                        break;
                    }
                case AlphaOperation.Inherit:
                    {
                        using (var alphaTemp = _engine.CreateRenderTexture(canvasTexture.Width, canvasTexture.Hight))
                        {
                            _engine.CopyAlpha(canvasTexture, alphaTemp);
                            _engine.TextureBlend(canvasTexture, layerRt, imageLayer.BlendTypeKey);
                            _engine.CopyAlpha(alphaTemp, canvasTexture);
                        }
                        break;
                    }
                case AlphaOperation.Layer:
                    {
                        _engine.TextureBlend(canvasTexture, layerRt, imageLayer.BlendTypeKey);
                        _engine.CopyAlpha(layerRt, canvasTexture);

                        break;
                    }
                case AlphaOperation.Intersect:
                    {
                        using (var alphaTemp = _engine.CreateRenderTexture(canvasTexture.Width, canvasTexture.Hight))
                        {
                            _engine.CopyAlpha(canvasTexture, alphaTemp);
                            _engine.TextureBlend(canvasTexture, layerRt, imageLayer.BlendTypeKey);

                            _engine.MulAlpha(alphaTemp, layerRt);
                            _engine.CopyAlpha(alphaTemp, canvasTexture);
                        }
                        break;
                    }
            }
        }

        public static List<PreBlendPairedLayer> ToBelowFlattened(IEnumerable<LayerObject> layerObjects)
        {
            var layerStack = new Stack<PreBlendPairedLayer>();

            void PushToStack(LayerObject l, bool forcedNull = false)
            {
                switch (l)
                {
                    case ImageLayer imageLayer:
                        {
                            layerStack.Push(new PreBlendPairedLayer(imageLayer, forcedNull is false ? new() : null));
                            return;
                        }
                    case GrabLayer grabLayer:
                        {
                            layerStack.Push(new PreBlendPairedLayer(grabLayer, null));
                            return;
                        }


                    default: { throw new ArgumentException("これは表示されないはず、何かがおかしい、ここに渡る型なんてないはずだ。"); }
                }
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
