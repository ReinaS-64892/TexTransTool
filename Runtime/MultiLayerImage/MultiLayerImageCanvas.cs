using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;

namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu("TexTransTool/MultiLayer/TTT MultiLayerImageCanvas")]
    public sealed class MultiLayerImageCanvas : TexTransRuntimeBehavior, ITTTChildExclusion
    {
        public RelativeTextureSelector TextureSelector;

        internal override List<Renderer> GetRenderers => new List<Renderer>() { TextureSelector.TargetRenderer };

        internal override bool IsPossibleApply => TextureSelector.TargetRenderer != null;

        internal override TexTransPhase PhaseDefine => TexTransPhase.BeforeUVModification;

        public bool AdditionalCanvasMode;

        internal override void Apply([NotNull] IDomain domain)
        {
            if (!IsPossibleApply) { throw new TTTNotExecutable(); }
            var replaceTarget = TextureSelector.GetTexture();
            if (replaceTarget == null) { throw new TTTNotExecutable(); }

            var canvasContext = new CanvasContext(replaceTarget.width, domain.GetTextureManager());


            var Layers = transform.GetChildren()
            .Select(I => I.GetComponent<AbstractLayer>())
            .Where(I => I != null)
            .Reverse();
            foreach (var layer in Layers) { layer.EvaluateTexture(canvasContext); }


            if (canvasContext.RootLayerStack.Stack.Count == 0) { return; }

            if (!AdditionalCanvasMode) { domain.AddTextureStack(replaceTarget, new BlendTexturePair(RenderTexture.GetTemporary(2, 2), "NotBlend")); }

            foreach (var layer in canvasContext.RootLayerStack.GetLayers)
            {
                domain.AddTextureStack(replaceTarget, layer);
            }

            domain.GetTextureManager().DestroyTextures();
        }
        internal class CanvasContext
        {
            public int CanvasSize;
            public LayerStack RootLayerStack;
            public ITextureManager TextureManager;

            public CanvasContext(int canvasSize, ITextureManager textureManager)
            {
                CanvasSize = canvasSize;
                RootLayerStack = new();
                TextureManager = textureManager;
            }
            public CanvasContext CreateSubCanvas => new CanvasContext(CanvasSize, TextureManager);
        }

        internal class LayerStack
        {
            public List<BlendLayer> Stack = new List<BlendLayer>();

            public IEnumerable<BlendLayer.BlendRenderTexture> GetLayers => Stack.Where(I => I.BlendTextures.Texture != null).Select(I => I.BlendTextures);



            public void AddRtForClipping(AbstractLayer abstractLayer, RenderTexture tex, string blendTypeKey)
            {
                var index = Stack.Count;
                index -= 1;
                if (index >= 0)
                {
                    var downLayer = Stack[index];
                    if (downLayer.RefLayer is LayerFolder layerFolder && layerFolder.PassThrough) { index = -1; }
                }

                if (index >= 0)
                {
                    var refBlendLayer = Stack[index];
                    var ClippingDist = refBlendLayer.BlendTextures.Texture;
                    if (ClippingDist == null) { RenderTexture.ReleaseTemporary(tex); return; }
                    ClippingDist.BlendBlit(tex, blendTypeKey, true);
                }
                RenderTexture.ReleaseTemporary(tex);
            }

            public void AddRenderTexture(AbstractLayer abstractLayer, RenderTexture tex, string blendTypeKey)
            {
                Stack.Add(new BlendLayer(abstractLayer, tex, blendTypeKey));
            }

            public void ReleaseLayers()
            {
                foreach (var layer in Stack)
                {
                    RenderTexture.ReleaseTemporary(layer.BlendTextures.Texture);
                }
                Stack.Clear();
            }
        }

        internal struct BlendLayer
        {
            public AbstractLayer RefLayer;
            public BlendRenderTexture BlendTextures;

            public BlendLayer(AbstractLayer refLayer, RenderTexture layer, string blendTypeKey)
            {
                RefLayer = refLayer;
                BlendTextures = new BlendRenderTexture(layer, blendTypeKey);
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

        }
    }
}