#if UNITY_EDITOR
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
        public Texture2D ReplaceTarget;
        public List<Renderer> PreviewRenderer;

        public override List<Renderer> GetRenderers => PreviewRenderer;

        public override bool IsPossibleApply => true;

        public override TexTransPhase PhaseDefine => TexTransPhase.BeforeUVModification;

        public Vector2Int TextureSize = new Vector2Int(2048, 2048);

        public override void Apply([NotNull] IDomain domain)
        {
            var Canvas = new RenderTexture(TextureSize.x, TextureSize.y, 0);
            var layerStack = new LayerStack() { CanvasSize = TextureSize };

            var Layers = transform.GetChildren()
            .Select(I => I.GetComponent<AbstractLayer>())
            .Reverse();
            foreach (var layer in Layers) { layer.EvaluateTexture(layerStack); }


            if (layerStack.Stack.Count == 0) { return; }

            layerStack.Stack[0] = new BlendLayer(layerStack.Stack[0].RefLayer, layerStack.Stack[0].BlendTextures.Texture, BlendType.NotBlend);

            foreach (var layer in layerStack.GetLayers)
            {
                domain.AddTextureStack(ReplaceTarget, layer);
            }
        }

        public class LayerStack
        {
            public Vector2Int CanvasSize;
            public List<BlendLayer> Stack = new List<BlendLayer>();

            public LayerStack CreateSubStack => new LayerStack() { CanvasSize = CanvasSize };

            public IEnumerable<BlendTextures> GetLayers => Stack.Where(I => I.BlendTextures.Texture != null).Select(I => I.BlendTextures);
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