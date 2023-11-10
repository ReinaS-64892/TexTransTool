#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransTool;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlendUtils;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu("TexTransTool/MultiLayer/TTT MultiLayerImageCanvas")]
    public class MultiLayerImageCanvas : TextureTransformer
    {
        public RelativeTextureSelector TextureSelector;

        public override List<Renderer> GetRenderers => new List<Renderer>() { TextureSelector.TargetRenderer };

        public override bool IsPossibleApply => true;

        public override TexTransPhase PhaseDefine => TexTransPhase.BeforeUVModification;

        public Vector2Int TextureSize = new Vector2Int(2048, 2048);

        public override void Apply([NotNull] IDomain domain)
        {
            var Canvas = new RenderTexture(TextureSize.x, TextureSize.y, 0);
            var layerStack = new LayerStack() { CanvasSize = TextureSize };

            var replaceTarget = TextureSelector.GetTexture();
            if (replaceTarget == null) { return; }

            var Layers = transform.GetChildren()
            .Select(I => I.GetComponent<AbstractLayer>())
            .Reverse();
            foreach (var layer in Layers) { layer.EvaluateTexture(layerStack); }


            if (layerStack.Stack.Count == 0) { return; }

            layerStack.Stack[0] = new BlendLayer(layerStack.Stack[0].RefLayer, layerStack.Stack[0].BlendTextures.Texture, BlendType.NotBlend);

            foreach (var layer in layerStack.GetLayers)
            {
                domain.AddTextureStack(replaceTarget, layer);
            }
        }

        public class LayerStack
        {
            public Vector2Int CanvasSize;
            public List<BlendLayer> Stack = new List<BlendLayer>();

            public LayerStack CreateSubStack => new LayerStack() { CanvasSize = CanvasSize };

            public IEnumerable<BlendTextures> GetLayers => Stack.Where(I => I.BlendTextures.Texture != null).Select(I => I.BlendTextures);



            public void AddRtForClipping(AbstractLayer abstractLayer, RenderTexture tex, BlendType blendType)
            {
                var index = Stack.Count;
                index -= 1;
                if (index >= 0)
                {
                    var downLayer = Stack[index];
                    if (downLayer.RefLayer is LayerFolder layerFolder && layerFolder.PassThrough) { index = -1; }
                }

                if (index < 0)
                {
                    Stack.Add(new BlendLayer(abstractLayer, tex, blendType));
                }
                else
                {
                    var refBlendLayer = Stack[index];
                    var ClippingDist = refBlendLayer.BlendTextures.Texture as RenderTexture;
                    ClippingDist.BlendBlit(tex, blendType, true);
                }
            }

            public void AddRenderTexture(AbstractLayer abstractLayer, RenderTexture tex, BlendType blendType)
            {
                Stack.Add(new BlendLayer(abstractLayer, tex, blendType));
            }
        }

        public struct BlendLayer
        {
            public AbstractLayer RefLayer;
            public BlendTextures BlendTextures;

            public BlendLayer(AbstractLayer refLayer, Texture layer, BlendType blendType)
            {
                RefLayer = refLayer;
                BlendTextures = new BlendTextures(layer, blendType);
            }

        }
    }
}
#endif