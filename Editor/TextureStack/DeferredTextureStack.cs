using System.Collections.Generic;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using System.Linq;
using UnityEditor;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;

namespace net.rs64.TexTransTool.TextureStack
{
    internal class DeferredTextureStack : AbstractTextureStack
    {
        [SerializeField] List<IBlendTexturePair> StackTextures = new();


        public override void AddStack<BlendTex>(BlendTex blendTexturePair) => StackTextures.Add(blendTexturePair);

        public override Texture2D MergeStack()
        {
            if (!StackTextures.Any()) { return FirstTexture; }
            var renderTexture = RenderTexture.GetTemporary(FirstTexture.width, FirstTexture.height, 0);
            renderTexture.Clear();
            TextureManager.WriteOriginalTexture(FirstTexture, renderTexture);

            renderTexture.BlendBlit(StackTextures);
            foreach (var bTex in StackTextures) { if (bTex.Texture is RenderTexture rt && !AssetDatabase.Contains(rt)) { RenderTexture.ReleaseTemporary(rt); } }

            var resultTex = renderTexture.CopyTexture2D().CopySetting(FirstTexture, false);
            resultTex.name = FirstTexture.name + "_MergedStack";
            TextureManager.DeferInheritTextureCompress(FirstTexture, resultTex);
            RenderTexture.ReleaseTemporary(renderTexture);
            return resultTex;
        }
    }
}
