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

    [AddComponentMenu("TexTransTool/MultiLayer/TTT MultiLayerImageCanvas")]
    public sealed class MultiLayerImageCanvas : TexTransRuntimeBehavior, ITTTChildExclusion
    {
        internal override List<Renderer> GetRenderers => new List<Renderer>() { TextureSelector.TargetRenderer };
        internal override bool IsPossibleApply => TextureSelector.GetTexture() != null;
        internal override TexTransPhase PhaseDefine => TexTransPhase.BeforeUVModification;

        public RelativeTextureSelector TextureSelector;

        [SerializeField, HideInInspector] internal TTTImportedCanvasDescription tttImportedCanvasDescription;

        internal override void Apply([NotNull] IDomain domain)
        {
            if (!IsPossibleApply) { throw new TTTNotExecutable(); }
            var replaceTarget = TextureSelector.GetTexture();


            var Layers = transform.GetChildren()
            .Select(I => I.GetComponent<AbstractLayer>())
            .Where(I => I != null)
            .Reverse();

            var canvasContext = new CanvasContext(tttImportedCanvasDescription?.Width ?? replaceTarget.width, domain.GetTextureManager());
            foreach (var layer in Layers) { layer.EvaluateTexture(canvasContext); }
            var result = canvasContext.LayerCanvas.FinalizeCanvas();
            domain.AddTextureStack(replaceTarget, new BlendTexturePair(result, "NotBlend"));


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
            RenderTexture Canvas;
            (BlendLayer layer, LayerAlphaMod alphaMod) _BeforeLayer;
            (BlendLayer layer, LayerAlphaMod alphaMod) BeforeLayer => _BeforeLayer;

            RenderTexture AwaitingReleaseTempRt;

            Stack<LayerAlphaMod> AlphaModStack;

            public LayerCanvas(RenderTexture renderTexture)
            {
                Canvas = renderTexture;
                Canvas.Clear();
                AlphaModStack = new();
            }


            public void AddLayer(BlendLayer blendLayer)
            {
                if (blendLayer.ThisClipping)
                {
                    if (blendLayer.NotVisible)//無効化されてる場合クリッピングレイヤーは消失する
                    {
                        RenderTexture.ReleaseTemporary(blendLayer.BlendTexture.Texture);
                        return;
                    }
                    if (BeforeLayer.layer.DisallowClipping)//クリッピング不可能な対象なので通常の合成にフォールバック
                    {
                        blendLayer.ThisClipping = false;
                        Composite(blendLayer);
                        return;
                    }
                    if (BeforeLayer.layer.NotVisible)//クリッピング対象が無効化されてる場合クリッピングレイヤーは消失する
                    {
                        RenderTexture.ReleaseTemporary(blendLayer.BlendTexture.Texture);
                        return;
                    }

                    //正常なクリッピング
                    var rTex = blendLayer.BlendTexture.Texture;
                    if (NowAlphaMod.Mask != null)
                    {
                        MaskDrawRenderTexture(rTex, NowAlphaMod.Mask);
                    }
                    if (!Mathf.Approximately(NowAlphaMod.Opacity, 1))
                    {
                        MultipleRenderTexture(rTex, new Color(1, 1, 1, NowAlphaMod.Opacity));
                    }

                    BeforeLayer.layer.BlendTexture.Texture.BlendBlit(blendLayer.BlendTexture, true);
                    RenderTexture.ReleaseTemporary(blendLayer.BlendTexture.Texture);

                }
                else
                {
                    Composite(blendLayer);
                    return;
                }
            }

            public RenderTexture GrabCanvas(bool GrabForClipping)
            {
                if (GrabForClipping)
                {
                    if (BeforeLayer.layer.NotVisible)
                    {
                        if (BeforeLayer.layer.DisallowClipping)
                        {//次のレイヤーのクリッピングを無効化し、キャンバスを渡す通常動作へのフォールバック
                            Composite(new(false, true, false, null, null));
                            return Canvas;
                        }
                        else
                        {//無効化、消失
                            return null;
                        }
                    }
                    else
                    {
                        if (BeforeLayer.layer.DisallowClipping)
                        {//次のレイヤーのクリッピングを無効化し、キャンバスを渡す通常動作へのフォールバック
                            Composite(new(false, true, false, null, null));
                            return Canvas;
                        }
                        else
                        {//クリッピングを正常にできる通常動作
                            return BeforeLayer.layer.BlendTexture.Texture;
                        }
                    }
                }
                else
                {//次のレイヤーのクリッピングを無効化し、キャンバスを渡す通常動作
                    Composite(new(false, true, false, null, null));
                    return Canvas;
                }
            }

            private void Composite(BlendLayer newLayer)
            {
                if (BeforeLayer.layer.BlendTexture.Texture != null && BeforeLayer.layer.BlendTexture.BlendTypeKey != null)
                {
                    var rTex = BeforeLayer.layer.BlendTexture.Texture;

                    if (BeforeLayer.alphaMod.Mask != null)
                    {
                        MaskDrawRenderTexture(rTex, BeforeLayer.alphaMod.Mask);

                        if (AwaitingReleaseTempRt != null)
                        {
                            RenderTexture.ReleaseTemporary(AwaitingReleaseTempRt); AwaitingReleaseTempRt = null;
                        }
                    }
                    if (!Mathf.Approximately(BeforeLayer.alphaMod.Opacity, 1))
                    {
                        MultipleRenderTexture(rTex, new Color(1, 1, 1, BeforeLayer.alphaMod.Opacity));
                    }

                    Canvas.BlendBlit(BeforeLayer.layer.BlendTexture);
                    RenderTexture.ReleaseTemporary(rTex);
                }
                _BeforeLayer = (newLayer, NowAlphaMod);
            }
            public RenderTexture FinalizeCanvas()
            {
                Composite(new(true, false, false, null, null));
                return Canvas;
            }

            public LayerAlphaMod NowAlphaMod => AlphaModStack.Count == 0 ? LayerAlphaMod.NonMasked : AlphaModStack.Peek();

            public AlphaModScopeStruct AlphaModScope(LayerAlphaMod layerAlphaMod)
            {
                EnterAlphaModScope(layerAlphaMod);
                return new(this);
            }
            private void EnterAlphaModScope(LayerAlphaMod layerAlphaMod)
            {
                if (layerAlphaMod.Mask != null)
                {
                    TextureBlend.MaskDrawRenderTexture(layerAlphaMod.Mask, NowAlphaMod.Mask ?? (Texture)Texture2D.whiteTexture);
                }
                layerAlphaMod.Opacity *= NowAlphaMod.Opacity;
                AlphaModStack.Push(layerAlphaMod);

            }

            private void EndAlphaModScope()
            {
                var discardMod = AlphaModStack.Pop();
                if (AwaitingReleaseTempRt == null) { AwaitingReleaseTempRt = discardMod.Mask; }
                else { RenderTexture.ReleaseTemporary(discardMod.Mask); }
            }

            public struct AlphaModScopeStruct : IDisposable
            {
                private LayerCanvas layerCanvas;

                public AlphaModScopeStruct(LayerCanvas layerCanvas)
                {
                    this.layerCanvas = layerCanvas;
                }

                public void Dispose()
                {
                    layerCanvas.EndAlphaModScope();
                }
            }


        }

        internal struct BlendLayer
        {

            public bool NotVisible;
            public bool DisallowClipping;
            public bool ThisClipping;
            public BlendRenderTexture BlendTexture;

            public BlendLayer(bool notVisible, bool disallowClipping, bool thisClipping, RenderTexture layer, string blendTypeKey)
            {
                NotVisible = notVisible;
                DisallowClipping = disallowClipping;
                ThisClipping = thisClipping;
                BlendTexture = new BlendRenderTexture(layer, blendTypeKey);
            }

            public struct BlendRenderTexture : IBlendTexturePair
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
            }

            public static BlendLayer Null(bool disallowClipping, bool clipping) => new(true, disallowClipping, clipping, null, null);

        }
        internal struct LayerAlphaMod
        {
            public RenderTexture Mask;
            public float Opacity;
            public LayerAlphaMod(RenderTexture mask, float opacity)
            {
                Mask = mask;
                Opacity = opacity;
            }

            public static LayerAlphaMod NonMasked => new(null, 1);

            public override bool Equals(object obj)
            {
                return obj is LayerAlphaMod other &&
                       EqualityComparer<RenderTexture>.Default.Equals(Mask, other.Mask) &&
                       Opacity == other.Opacity;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Mask, Opacity);
            }

            public void Deconstruct(out RenderTexture mask, out float opacity)
            {
                mask = Mask;
                opacity = Opacity;
            }

            public static implicit operator (RenderTexture Mask, float Opacity)(LayerAlphaMod value)
            {
                return (value.Mask, value.Opacity);
            }

            public static implicit operator LayerAlphaMod((RenderTexture Mask, float Opacity) value)
            {
                return new LayerAlphaMod(value.Mask, value.Opacity);
            }
        }
    }

}
