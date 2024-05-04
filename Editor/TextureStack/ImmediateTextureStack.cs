using net.rs64.TexTransTool.Utils;
using UnityEngine;
using net.rs64.TexTransCore.Utils;
using UnityEditor;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransTool.TextureStack
{
    internal class ImmediateTextureStack : AbstractTextureStack
    {
        RenderTexture renderTexture;
        public override void init(Texture2D firstTexture, ITextureManager textureManager)
        {
            base.init(firstTexture, textureManager);


            using (new RTActiveSaver())
            {
                renderTexture = TTRt.G(FirstTexture.width, FirstTexture.height);
                textureManager.WriteOriginalTexture(FirstTexture, renderTexture);//解像度は維持しないといけないが、VRAM上の圧縮は外さないといけない
            }
        }

        public override void AddStack<BlendTex>(BlendTex blendTexturePair)
        {
            renderTexture.BlendBlit(blendTexturePair.Texture, blendTexturePair.BlendTypeKey);

            if (blendTexturePair.Texture is RenderTexture rt && !AssetDatabase.Contains(rt))
            { TTRt.R(rt); }
        }

        public override Texture2D MergeStack()
        {
            var resultTex = renderTexture.CopyTexture2D().CopySetting(FirstTexture, false);
            resultTex.name = FirstTexture.name + "_MergedStack";
            TextureManager.DeferInheritTextureCompress(FirstTexture, resultTex);


            TTRt.R(renderTexture);
            return resultTex;
        }
    }
}
