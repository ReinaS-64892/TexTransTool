#nullable enable
namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public abstract class ImageLayer : LayerObject
    {
        public AlphaOperation AlphaOperation;
        public ITTBlendKey BlendTypeKey;
        public ImageLayer(bool visible, AlphaMask alphaModifier, AlphaOperation alphaOperation, bool preBlendToLayerBelow, ITTBlendKey blendTypeKey) : base(visible, preBlendToLayerBelow, alphaModifier)
        {
            AlphaOperation = alphaOperation;
            BlendTypeKey = blendTypeKey;
        }

        /// <summary>
        /// ここで渡される writeTarget はクリアされてい無ければならない。
        /// さぁ何かを書きこむのだ！
        /// 一枚のラスター画像を生成するのだ！
        /// </summary>
        public abstract void GetImage(ITTEngine engine, ITTRenderTexture writeTarget);

    }

    public enum AlphaOperation
    {
        Normal = 0,
        Inherit = 1,
        Layer = 2,
        Intersect = 3,
    }
}
