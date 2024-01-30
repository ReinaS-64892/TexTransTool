using net.rs64.TexTransTool.Utils;
using UnityEngine;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEditor;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;
using net.rs64.TexTransCore.BlendTexture;

namespace net.rs64.TexTransTool.TextureStack
{
    internal class ImmediateTextureStack : TextureStack
    {
        RenderTexture renderTexture;
        public override void init(Texture2D firstTexture, ITextureManager textureManager)
        {
            base.init(firstTexture, textureManager);


            using (new RTActiveSaver())
            {
                renderTexture = RenderTexture.GetTemporary(FirstTexture.width, FirstTexture.height, 0);
                textureManager.WriteOriginalTexture(FirstTexture, renderTexture);//解像度は維持しないといけないが、VRAM上の圧縮は外さないといけない
            }
        }

        public override void AddStack<BlendTex>(BlendTex blendTexturePair)
        {
            renderTexture.BlendBlit(blendTexturePair.Texture, blendTexturePair.BlendTypeKey);

            if (blendTexturePair.Texture is RenderTexture rt && !AssetDatabase.Contains(rt))
            { RenderTexture.ReleaseTemporary(rt); }
        }

        public override Texture2D MergeStack()
        {
            var resultTex = renderTexture.CopyTexture2D().CopySetting(FirstTexture, false);
            resultTex.name = FirstTexture.name + "_MergedStack";
            TextureManager.ReplaceTextureCompressDelegation(FirstTexture, resultTex);


            RenderTexture.ReleaseTemporary(renderTexture);
            return resultTex;
        }
    }
}
