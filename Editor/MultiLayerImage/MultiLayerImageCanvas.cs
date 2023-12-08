#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransTool;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu("TexTransTool/MultiLayer/TTT MultiLayerImageCanvas")]
    internal class MultiLayerImageCanvas : TextureTransformer
    {
        public RelativeTextureSelector TextureSelector;

        public override List<Renderer> GetRenderers => new List<Renderer>() { TextureSelector.TargetRenderer };

        public override bool IsPossibleApply => true;

        public override TexTransPhase PhaseDefine => TexTransPhase.BeforeUVModification;

        public Vector2Int TextureSize = new Vector2Int(2048, 2048);

        public override void Apply([NotNull] IDomain domain)
        {
            using (var CanvasContext = new CanvasContext(TextureSize, false))
            {
                var replaceTarget = TextureSelector.GetTexture();
                if (replaceTarget == null) { return; }

                var Layers = transform.GetChildren()
                .Select(I => I.GetComponent<AbstractLayer>())
                .Reverse();
                foreach (var layer in Layers) { layer.EvaluateTexture(CanvasContext); }


                if (CanvasContext.RootLayerStack.Stack.Count == 0) { return; }

                CanvasContext.RootLayerStack.Stack[0] = new BlendLayer(CanvasContext.RootLayerStack.Stack[0].RefLayer, CanvasContext.RootLayerStack.Stack[0].BlendTextures.Texture, BL_KEY_DEFAULT);

                foreach (var layer in CanvasContext.RootLayerStack.GetLayers)
                {
                    domain.AddTextureStack(replaceTarget, layer);
                }
            }
        }
        public class CanvasContext : IDisposable
        {
            public LayerStack RootLayerStack;
            public TextureManageContext TextureManage;

            public CanvasContext(Vector2Int canvasSize, bool IsRealTimePreview)
            {
                RootLayerStack = new LayerStack(canvasSize);
                TextureManage = new TextureManageContext(IsRealTimePreview);
            }

            public CanvasContext(Vector2Int canvasSize, TextureManageContext textureManage)
            {
                RootLayerStack = new LayerStack(canvasSize);
                TextureManage = textureManage;
            }

            public CanvasContext CreateSubContext => new CanvasContext(RootLayerStack.CanvasSize, TextureManage);

            public void Dispose()
            {
                TextureManage.Dispose();
            }
        }

        public class LayerStack
        {
            public Vector2Int CanvasSize;
            public List<BlendLayer> Stack = new List<BlendLayer>();

            public LayerStack(Vector2Int textureSize)
            {
                CanvasSize = textureSize;
            }

            public IEnumerable<BlendTexturePair> GetLayers => Stack.Where(I => I.BlendTextures.Texture != null).Select(I => I.BlendTextures);



            public void AddRtForClipping(AbstractLayer abstractLayer, RenderTexture tex, string blendTypeKey)
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
                    Stack.Add(new BlendLayer(abstractLayer, tex, blendTypeKey));
                }
                else
                {
                    var refBlendLayer = Stack[index];
                    var ClippingDist = refBlendLayer.BlendTextures.Texture as RenderTexture;
                    ClippingDist.BlendBlit(tex, blendTypeKey, true);
                }
            }

            public void AddRenderTexture(AbstractLayer abstractLayer, RenderTexture tex, string blendTypeKey)
            {
                Stack.Add(new BlendLayer(abstractLayer, tex, blendTypeKey));
            }
        }

        public struct BlendLayer
        {
            public AbstractLayer RefLayer;
            public BlendTexturePair BlendTextures;

            public BlendLayer(AbstractLayer refLayer, Texture layer, string blendTypeKey)
            {
                RefLayer = refLayer;
                BlendTextures = new BlendTexturePair(layer, blendTypeKey);
            }

        }
    }

    public class TextureManageContext : IDisposable
    {
        public readonly bool IsRealTimePreview;
        public HashSet<Texture> DestroyTarget = new HashSet<Texture>();

        public TextureManageContext(bool isRealTimePreview)
        {
            IsRealTimePreview = isRealTimePreview;
        }

        public Texture2D TryGetUnCompress(Texture2D texture2D)
        {
            if (IsRealTimePreview) { return texture2D; }
            var unCompressed = texture2D.TryGetUnCompress();
            if (unCompressed != texture2D || !AssetDatabase.Contains(unCompressed)) { DestroyTarget.Add(unCompressed); }
            return unCompressed;
        }
        public void Dispose()
        {
            foreach (var tex in DestroyTarget)
            {
                UnityEngine.Object.DestroyImmediate(tex);
            }
            DestroyTarget.Clear();
        }


    }
}
#endif