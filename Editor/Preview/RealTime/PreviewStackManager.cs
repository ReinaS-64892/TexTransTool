using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEngine;
using net.rs64.TexTransCoreEngineForUnity.Utils;
using static net.rs64.TexTransCoreEngineForUnity.TextureBlend;

namespace net.rs64.TexTransTool.Preview.RealTime
{
    internal class PreviewStackManager
    {
        Dictionary<Texture2D, RenderTexture> _previewTextureMap = new();
        Dictionary<RenderTexture, PrioritizedDeferredStack> _stackMap = new();
        public event Action<Texture2D, RenderTexture> NewPreviewTexture;
        public void AddTextureStack<BlendTex>(int priority, Texture dist, BlendTex setTex) where BlendTex : IBlendTexturePair
        {
            switch (dist)
            {
                case RenderTexture rt:
                    {
                        if (_stackMap.ContainsKey(rt)) { _stackMap[rt].AddTextureStack(priority, new BlendTexturePair(setTex)); }
                        else { Debug.Log("Invalid RenderTexture"); }
                        break;
                    }
                case Texture2D tex2D:
                    {
                        if (_previewTextureMap.ContainsKey(tex2D)) { _stackMap[_previewTextureMap[tex2D]].AddTextureStack(priority, new BlendTexturePair(setTex)); }
                        else
                        {
                            var newStack = new PrioritizedDeferredStack(tex2D);
                            _previewTextureMap[tex2D] = newStack.StackView;
                            _stackMap[newStack.StackView] = newStack;
                            NewPreviewTexture?.Invoke(tex2D, newStack.StackView);
                            newStack.AddTextureStack(priority, new BlendTexturePair(setTex));
                        }
                        break;
                    }
            }
        }

        public void ReleaseStackOfPriority(int priority) { foreach (var stack in _stackMap.Values) { stack.ReleaseStackOfPriority(priority); } }
        public void ReleaseStackOfTexture(Texture texture)
        {
            switch (texture)
            {
                case Texture2D tex2D:
                    {
                        if (!_previewTextureMap.ContainsKey(tex2D)) { return; }
                        var rt = _previewTextureMap[tex2D];

                        var stack = _stackMap[rt];
                        _stackMap.Remove(rt);
                        _previewTextureMap.Remove(tex2D);
                        stack.ReleaseStack();
                        break;
                    }
                case RenderTexture rt:
                    {
                        if (!_stackMap.ContainsKey(rt)) { return; }

                        var stack = _stackMap[rt];
                        _stackMap.Remove(rt);
                        _previewTextureMap.Remove(_previewTextureMap.First(kv => kv.Value == rt).Key);
                        stack.ReleaseStack();
                        break;
                    }
            }
        }
        public void ReleaseStackAll()
        {
            foreach (var stack in _stackMap.Values) { stack.ReleaseStack(); }
            _previewTextureMap.Clear();
            _stackMap.Clear();
        }

        internal HashSet<RenderTexture> FindAtPriority(int priority) { return _stackMap.Keys.Where(i => _stackMap[i].ContainedPriority(priority)).ToHashSet(); }

        public RenderTexture GetPreviewTexture(Texture2D texture)
        {
            if (texture == null) { return null; }
            if (!_previewTextureMap.ContainsKey(texture)) { return null; }
            return _previewTextureMap[texture];
        }


        public void UpdateNeededStack()
        {
            foreach (var stack in _stackMap.Values)
            {
                if (!stack.UpdateNeeded) { continue; }
                stack.StackViewUpdate();
            }
        }

        internal class PrioritizedDeferredStack
        {
            Texture2D _initialTexture;
            RenderTexture _stackViewTexture;

            public bool UpdateNeeded { get; private set; } = false;
            SortedList<int, List<BlendTexturePair>> _stack = new();

            public RenderTexture StackView => _stackViewTexture;

            public PrioritizedDeferredStack(Texture2D initialTexture)
            {
                _initialTexture = initialTexture;
                _stackViewTexture = TTRt.G(initialTexture.width, initialTexture.height);
                _stackViewTexture.name = $"{initialTexture.name}:PrioritizedDeferredStack-{_stackViewTexture.width}x{_stackViewTexture.height}";

                _stackViewTexture.CopyFilWrap(initialTexture);
                Graphics.Blit(initialTexture, _stackViewTexture);
            }

            public void AddTextureStack(int priority, BlendTexturePair blendTexturePair)
            {
                if (!_stack.ContainsKey(priority)) { _stack.Add(priority, new()); }
                _stack[priority].Add(blendTexturePair);
                UpdateNeeded = true;
            }
            public void ReleaseStackOfPriority(int priority)
            {
                if (!_stack.ContainsKey(priority)) { return; }
                var cs = _stack[priority];

                foreach (var l in cs) { if (l.Texture is RenderTexture rt) { TTRt.R(rt); UpdateNeeded = true; } }
                cs.Clear();
            }
            public void ReleaseStack()
            {
                TTRt.R(_stackViewTexture);
                _stackViewTexture = null;
                foreach (var ptl in _stack)
                    foreach (var l in ptl.Value) { if (l.Texture is RenderTexture rt) { TTRt.R(rt); } }

            }
            public void StackViewUpdate()
            {
                Graphics.Blit(_initialTexture, _stackViewTexture);
                foreach (var ptl in _stack)
                    foreach (var l in ptl.Value)
                    {
                        _stackViewTexture.BlendBlit(l);
                    }
                UpdateNeeded = false;
            }

            public bool ContainedPriority(int priority) => _stack.ContainsKey(priority);

        }
    }
}
