#if UNITY_EDITOR
using System.Collections.Generic;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using System.Linq;
using UnityEditor;
using System.IO;
using static net.rs64.TexTransCore.BlendTexture.TextureBlendUtils;
using net.rs64.TexTransCore.BlendTexture;

namespace net.rs64.TexTransTool.TextureStack
{
    public class ImmediateTextureStack : TextureStack
    {
        RenderTexture renderTexture;
        public override void init(Texture2D firstTexture, ITextureManager textureManager)
        {
            base.init(firstTexture, textureManager);


            using (new RTActiveSaver())
            {
                renderTexture = new RenderTexture(FirstTexture.width, FirstTexture.height, 0);
                Graphics.Blit(TextureManager.GetOriginalTexture2D(FirstTexture), renderTexture);
            }
        }

        public override void AddStack(BlendTexturePair blendTexturePair)
        {
            renderTexture.BlendBlit(blendTexturePair.Texture, blendTexturePair.BlendType);

            if (blendTexturePair.Texture is RenderTexture rt && !AssetDatabase.Contains(rt))
            {
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }

        public override Texture2D MergeStack()
        {
            renderTexture.name = FirstTexture.name + "_MergedStack";
            var resultTex = renderTexture.CopyTexture2D().CopySetting(FirstTexture, false);
            TextureManager.ReplaceTextureCompressDelegation(FirstTexture, resultTex);


            UnityEngine.Object.DestroyImmediate(renderTexture);
            return resultTex;
        }
    }
}
#endif