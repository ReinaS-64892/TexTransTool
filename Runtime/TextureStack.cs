#nullable enable
using System.Collections.Generic;
using net.rs64.TexTransCore;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureStack
{
    internal class ImmediateStackManager
    {
        Dictionary<Texture, ImmediateTextureStack> _textureStacks = new();
        ITexTransToolForUnity _ttce4U;
        public ImmediateStackManager(ITexTransToolForUnity ttce4U)
        {
            _ttce4U = ttce4U;
        }
        public void AddTextureStack(Texture dist, ITTRenderTexture add, ITTBlendKey blendKey)
        {
            if (dist == null) { return; }
            if (_textureStacks.TryGetValue(dist, out var stack) is false)
                stack = _textureStacks[dist] = new(_ttce4U, dist);

            stack.AddStack(add, blendKey);
        }
        public List<MergeResult> MergeStacks()
        {
            var mergeTex = new List<MergeResult>(_textureStacks.Count);
            foreach (var stackKV in _textureStacks)
            {
                mergeTex.Add(new MergeResult(stackKV.Key, stackKV.Value.StackRenderTexture));
            }
            _textureStacks.Clear();
            return mergeTex;
        }
    }
    internal readonly struct MergeResult
    {
        public readonly Texture FirstTexture;
        public readonly ITTRenderTexture MergeTexture;// owned

        public MergeResult(Texture firstTexture, ITTRenderTexture mergeTexture)
        {
            FirstTexture = firstTexture;
            MergeTexture = mergeTexture;
        }
    }

    internal class ImmediateTextureStack
    {
        public readonly Texture BaseTexture;
        public readonly ITexTransToolForUnity TTCEWith4Unity;

        public readonly ITTRenderTexture StackRenderTexture;
        public ImmediateTextureStack(ITexTransToolForUnity ttce4u, Texture baseTexture)
        {
            BaseTexture = baseTexture;


            StackRenderTexture = ttce4u.CreateRenderTexture(BaseTexture.width, BaseTexture.height);
            StackRenderTexture.Name = $"{BaseTexture.name}:ImmediateTextureStack-{BaseTexture.width}x{BaseTexture.height}";

            TTCEWith4Unity = ttce4u;

            ttce4u.WrappingOrUploadToLoad(StackRenderTexture, BaseTexture);
        }

        public void AddStack(ITTRenderTexture addTex, ITTBlendKey blendKey) { TTCEWith4Unity.BlendingWithAnySize(StackRenderTexture, addTex, blendKey); }
    }
}
