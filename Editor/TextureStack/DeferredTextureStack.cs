#if UNITY_EDITOR
using System.Collections.Generic;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using System.Linq;
using UnityEditor;
using System.IO;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;

namespace net.rs64.TexTransTool.TextureStack
{
    public class DeferredTextureStack : TextureStack
    {
        [SerializeField] List<BlendTexturePair> StackTextures = new List<BlendTexturePair>();

        public override void AddStack(BlendTexturePair blendTexturePair) => StackTextures.Add(blendTexturePair);

        public override Texture2D MergeStack()
        {
            if (!StackTextures.Any()) { return FirstTexture; }
            var renderTexture = RenderTexture.GetTemporary(FirstTexture.width, FirstTexture.height, 0);
            renderTexture.Clear();
            Graphics.Blit(TextureManager.GetOriginalTexture2D(FirstTexture), renderTexture);

            renderTexture.BlendBlit(StackTextures);
            foreach (var bTex in StackTextures) { if (bTex.Texture is RenderTexture rt && !AssetDatabase.Contains(rt)) { UnityEngine.Object.DestroyImmediate(rt); } }

            renderTexture.name = FirstTexture.name + "_MergedStack";
            var resultTex = renderTexture.CopyTexture2D().CopySetting(FirstTexture, false);
            TextureManager.ReplaceTextureCompressDelegation(FirstTexture, resultTex);
            RenderTexture.ReleaseTemporary(renderTexture);
            return resultTex;
        }
    }
}
#endif