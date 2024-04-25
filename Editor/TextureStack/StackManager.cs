using System.Collections.Generic;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;

namespace net.rs64.TexTransTool.TextureStack
{
    internal class StackManager<Stack> where Stack : AbstractTextureStack, new()
    {
        [SerializeField] List<Stack> _textureStacks = new List<Stack>();
        ITextureManager _textureManager;
        public StackManager(ITextureManager textureManager)
        {
            _textureManager = textureManager;
        }
        public void AddTextureStack<BlendTex>(Texture2D dist, BlendTex setTex) where BlendTex : IBlendTexturePair
        {
            if (dist == null) { return; }
            var stack = _textureStacks.Find(i => i.FirstTexture == dist);
            if (stack == null)
            {
                stack = new Stack();
                stack.init(dist, _textureManager);
                stack.AddStack(setTex);
                _textureStacks.Add(stack);
            }
            else
            {
                stack.AddStack(setTex);
            }

        }
        public List<MergeResult> MergeStacks()
        {
            var mergeTex = new List<MergeResult>(_textureStacks.Capacity);
            foreach (var stack in _textureStacks)
            {
                mergeTex.Add(new MergeResult(stack.FirstTexture, stack.MergeStack()));
            }
            _textureStacks.Clear();
            return mergeTex;
        }

    }
    internal readonly struct MergeResult
    {
        public readonly Texture2D FirstTexture;
        public readonly Texture2D MergeTexture;

        public MergeResult(Texture2D firstTexture, Texture2D mergeTexture)
        {
            FirstTexture = firstTexture;
            MergeTexture = mergeTexture;
        }
    }

    internal abstract class AbstractTextureStack
    {
        public virtual void init(Texture2D firstTexture, ITextureManager textureManager) { FirstTexture = firstTexture; TextureManager = textureManager; }
        public Texture2D FirstTexture;
        public ITextureManager TextureManager;
        // RenderTextureの場合解放責任はこっちにわたるが、Texture2Dの場合は渡らない
        public abstract void AddStack<BlendTex>(BlendTex blendTexturePair) where BlendTex : IBlendTexturePair;
        public abstract Texture2D MergeStack();
    }
}
