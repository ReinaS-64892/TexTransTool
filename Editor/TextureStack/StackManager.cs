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
    internal interface IStackManager
    {
        void AddTextureStack(Texture2D Dist, BlendTexturePair SetTex);
        List<MargeResult> MargeStacks();
    }
    internal class StackManager<Stack> : IStackManager
     where Stack : TextureStack, new()
    {
        [SerializeField] List<Stack> _textureStacks = new List<Stack>();
        ITextureManager _textureManager;
        public StackManager(ITextureManager textureManager)
        {
            _textureManager = textureManager;
        }
        public void AddTextureStack(Texture2D Dist, BlendTexturePair SetTex)
        {
            if (Dist == null) { return; }
            var stack = _textureStacks.Find(i => i.FirstTexture == Dist);
            if (stack == null)
            {
                stack = new Stack();
                stack.init(Dist, _textureManager);
                stack.AddStack(SetTex);
                _textureStacks.Add(stack);
            }
            else
            {
                stack.AddStack(SetTex);
            }

        }

        public List<MargeResult> MargeStacks()
        {
            var margeTex = new List<MargeResult>(_textureStacks.Capacity);
            foreach (var stack in _textureStacks)
            {
                margeTex.Add(new MargeResult(stack.FirstTexture, stack.MergeStack()));
            }
            _textureStacks.Clear();
            return margeTex;
        }

    }
    internal readonly struct MargeResult
    {
        public readonly Texture2D FirstTexture;
        public readonly Texture2D MargeTexture;

        public MargeResult(Texture2D firstTexture, Texture2D margeTexture)
        {
            FirstTexture = firstTexture;
            MargeTexture = margeTexture;
        }
    }

    internal abstract class TextureStack
    {
        public virtual void init(Texture2D firstTexture, ITextureManager textureManager) { FirstTexture = firstTexture; TextureManager = textureManager; }
        public Texture2D FirstTexture;
        public ITextureManager TextureManager;
        public abstract void AddStack(BlendTexturePair blendTexturePair);
        public abstract Texture2D MergeStack();
    }
}
#endif