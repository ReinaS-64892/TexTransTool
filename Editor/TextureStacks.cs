#if UNITY_EDITOR
using System.Collections.Generic;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using net.rs64.TexTransCore.TransTextureCore;
using System.Linq;
using UnityEditor;
using System.IO;
using static net.rs64.TexTransCore.BlendTexture.TextureBlendUtils;

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
            var rendererTexture = RenderTexture.GetTemporary(size.x, size.y, 32);
            Graphics.Blit(TryGetUnCompress(FirstTexture, out var outUnCompress) ? outUnCompress : FirstTexture, rendererTexture);

            rendererTexture.BlendBlit(StackTextures);

            rendererTexture.name = FirstTexture.name + "_MergedStack";
            var resultTex = rendererTexture.CopyTexture2D().CopySetting(FirstTexture);
            RenderTexture.ReleaseTemporary(rendererTexture);
            return resultTex;
        }

        public bool TryGetUnCompress(Texture2D firstTexture, out Texture2D unCompress)
        {
            if (!AssetDatabase.Contains(firstTexture)) { unCompress = firstTexture; return false; }
            var path = AssetDatabase.GetAssetPath(firstTexture);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || importer.textureType != TextureImporterType.Default) { unCompress = firstTexture; return false; }
            unCompress = new Texture2D(2, 2);
            unCompress.LoadImage(File.ReadAllBytes(path));
            return true;
        }
    }

}
#endif