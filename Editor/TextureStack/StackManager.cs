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
        List<MergeResult> MergeStacks();
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
        public void AddTextureStack(Texture2D dist, BlendTexturePair setTex)
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