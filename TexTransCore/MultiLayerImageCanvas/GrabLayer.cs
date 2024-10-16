#nullable enable
namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public abstract class GrabLayer : LayerObject
    {
        public GrabLayer(bool visible, AlphaMask alphaMask, bool preBlendToLayerBelow) : base(visible, alphaMask, preBlendToLayerBelow)
        {

        }

        /// <summary>
        /// grabTexture はキャンバスそのもの、読み込んでだり描いたり自由だ。
        /// PassThoughtもできるし一度画像をコピーし色調補正でもかけた後に合成するもできる。
        /// ただそのために、 評価するためのコンテキストが渡ってくるよ...使わないのもできる。
        /// </summary>
        public abstract void GrabImage(ITexTransCoreEngine engine, EvaluateContext evaluateContext, ITTRenderTexture grabTexture);
    }
}
