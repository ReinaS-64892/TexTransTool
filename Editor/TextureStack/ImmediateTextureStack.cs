using net.rs64.TexTransTool.Utils;
using UnityEngine;
using net.rs64.TexTransCoreEngineForUnity.Utils;
using UnityEditor;
using net.rs64.TexTransCoreEngineForUnity;

namespace net.rs64.TexTransTool.TextureStack
{
    internal class ImmediateTextureStack : AbstractTextureStack
    {
        protected RenderTexture _renderTexture;
        public override void init(Texture2D firstTexture, ITextureManager textureManager)
        {
            base.init(firstTexture, textureManager);


            using (new RTActiveSaver())
            {
                _renderTexture = TTRt.G(FirstTexture.width, FirstTexture.height);
                _renderTexture.name = $"{firstTexture.name}:ImmediateTextureStack-{_renderTexture.width}x{_renderTexture.height}";
                textureManager.WriteOriginalTexture(FirstTexture, _renderTexture);//解像度は維持しないといけないが、VRAM上の圧縮は外さないといけない
            }
        }

        public override void AddStack<BlendTex>(BlendTex blendTexturePair)
        {
            _renderTexture.BlendBlit(blendTexturePair.Texture, blendTexturePair.BlendTypeKey);

            if (blendTexturePair.Texture is RenderTexture rt && !AssetDatabase.Contains(rt))
            { TTRt.R(rt); }
        }

        public override Texture2D MergeStack()
        {
            var resultTex = _renderTexture.CopyTexture2D().CopySetting(FirstTexture, false);
            resultTex.name = FirstTexture.name + "_MergedStack";
            TextureManager.DeferredInheritTextureCompress(FirstTexture, resultTex);


            TTRt.R(_renderTexture);
            return resultTex;
        }
    }
}
