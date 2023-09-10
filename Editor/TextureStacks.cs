#if UNITY_EDITOR
using System.Collections.Generic;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransTool.TextureLayerUtil;
using net.rs64.TexTransCore.TransTextureCore;
using System.Linq;

namespace net.rs64.TexTransTool
{
    public class TextureStacks
    {
        [SerializeField] List<TextureStack> _textureStacks = new List<TextureStack>();
        public void AddTextureStack(Texture2D Dist, BlendTextures SetTex)
        {
            var stack = _textureStacks.Find(i => i.FirstTexture == Dist);
            if (stack == null)
            {
                stack = new TextureStack { FirstTexture = Dist };
                stack.Stack = SetTex;
                _textureStacks.Add(stack);
            }
            else
            {
                stack.Stack = SetTex;
            }

        }

        public List<MargeResult> MargeStacks()
        {
            var margeTex = new List<MargeResult>(_textureStacks.Capacity);
            foreach (var stack in _textureStacks)
            {
                margeTex.Add(new MargeResult(stack.FirstTexture, stack.MergeStack()));
            }
            return margeTex;
        }

        public struct MargeResult
        {
            public Texture2D FirstTexture;
            public Texture2D MargeTexture;

            public MargeResult(Texture2D firstTexture, Texture2D margeTexture)
            {
                FirstTexture = firstTexture;
                MargeTexture = margeTexture;
            }

        }
    }
    public class TextureStack
    {
        public Texture2D FirstTexture;
        [SerializeField] List<BlendTextures> StackTextures = new List<BlendTextures>();

        public BlendTextures Stack
        {
            set => StackTextures.Add(value);
        }

        public Texture2D MergeStack()
        {
            if (!StackTextures.Any()) { return FirstTexture; }
            var size = FirstTexture.NativeSize();
            var rendererTexture = new RenderTexture(size.x, size.y, 32, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB);
            Graphics.Blit(FirstTexture, rendererTexture);

            rendererTexture.BlendBlit(StackTextures);

            rendererTexture.name = FirstTexture.name + "_MergedStack";
            return rendererTexture.CopyTexture2D().CopySetting(FirstTexture);
        }

    }

}
#endif