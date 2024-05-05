using System.Collections.Generic;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using net.rs64.TexTransCore.Utils;
using System.Linq;
using UnityEditor;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransTool.TextureStack
{
    internal class DeferredTextureStack : AbstractTextureStack
    {
        [SerializeField] List<IBlendTexturePair> StackTextures = new();


        public override void AddStack<BlendTex>(BlendTex blendTexturePair) => StackTextures.Add(blendTexturePair);

        public override Texture2D MergeStack()
        {
            if (!StackTextures.Any()) { return FirstTexture; }
            var renderTexture = TTRt.G(FirstTexture.width, FirstTexture.height, true);
            TextureManager.WriteOriginalTexture(FirstTexture, renderTexture);

            renderTexture.BlendBlit(StackTextures);
            foreach (var bTex in StackTextures) { if (bTex.Texture is RenderTexture rt && !AssetDatabase.Contains(rt)) { TTRt.R(rt); } }

            var resultTex = renderTexture.CopyTexture2D().CopySetting(FirstTexture, false);
            resultTex.name = FirstTexture.name + "_MergedStack";
            TextureManager.DeferInheritTextureCompress(FirstTexture, resultTex);
            TTRt.R(renderTexture);
            return resultTex;
        }
    }
}
