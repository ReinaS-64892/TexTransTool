#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.Preview.RealTime
{
    internal class PreviewStackManager
    {
        Dictionary<Texture2D, ITTRenderTexture> _previewTextureMap = new();
        Dictionary<RenderTexture, PrioritizedDeferredStack> _stackMap = new();

        TTCEUnityWithTTT4Unity _ttce4U;
        Action<Texture2D, ITTRenderTexture> _newPreviewTexture;

        public PreviewStackManager(TTCEUnityWithTTT4Unity ttce4U, Action<Texture2D, ITTRenderTexture> newPreviewTextureRegister)
        {
            _ttce4U = ttce4U;
            _newPreviewTexture = newPreviewTextureRegister;
        }
        public void AddTextureStack(int priority, Texture dist, ITTRenderTexture addTex, ITTBlendKey blendKey)
        {
            switch (dist)
            {
                case RenderTexture rt:
                    {
                        if (_stackMap.ContainsKey(rt)) { _stackMap[rt].AddTextureStack(priority, addTex, blendKey); }
                        else { Debug.Log("Invalid RenderTexture"); }
                        break;
                    }
                case Texture2D tex2D:
                    {
                        if (_previewTextureMap.ContainsKey(tex2D)) { _stackMap[_ttce4U.GetReferenceRenderTexture(_previewTextureMap[tex2D])].AddTextureStack(priority, addTex, blendKey); }
                        else
                        {
                            var newStack = new PrioritizedDeferredStack(_ttce4U, tex2D);
                            _previewTextureMap[tex2D] = newStack.StackView;
                            _stackMap[_ttce4U.GetReferenceRenderTexture(newStack.StackView)] = newStack;
                            _newPreviewTexture?.Invoke(tex2D, newStack.StackView);
                            newStack.AddTextureStack(priority, addTex, blendKey);
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

                        _stackMap.Remove(_ttce4U.GetReferenceRenderTexture(rt), out var stack);
                        stack.Dispose();

                        _previewTextureMap.Remove(tex2D);
                        break;
                    }
                case RenderTexture rt:
                    {
                        if (!_stackMap.ContainsKey(rt)) { return; }

                        _stackMap.Remove(rt, out var stack);
                        stack.Dispose();

                        _previewTextureMap.Remove(_previewTextureMap.First(kv => _ttce4U.GetReferenceRenderTexture(kv.Value) == rt).Key);
                        break;
                    }
            }
        }
        public void ReleaseStackAll()
        {
            foreach (var stack in _stackMap.Values) { stack.Dispose(); }
            _stackMap.Clear();
            _previewTextureMap.Clear();
        }

        internal HashSet<RenderTexture> FindAtPriority(int priority) { return _stackMap.Keys.Where(i => _stackMap[i].ContainedPriority(priority)).ToHashSet(); }

        public RenderTexture? GetPreviewTexture(Texture2D? texture)
        {
            if (texture == null) { return null; }
            if (!_previewTextureMap.ContainsKey(texture)) { return null; }

            return _ttce4U.GetReferenceRenderTexture(_previewTextureMap[texture]);
        }


        public void UpdateNeededStack()
        {
            foreach (var stack in _stackMap.Values)
            {
                if (!stack.UpdateNeeded) { continue; }
                stack.StackViewUpdate();
            }
        }

        internal class PrioritizedDeferredStack : IDisposable
        {
            ITTRenderTexture _initialTexture;
            ITTRenderTexture _stackViewTexture;

            TTCEUnityWithTTT4Unity _ttce4U;

            public bool UpdateNeeded { get; private set; } = false;
            SortedList<int, List<(ITTRenderTexture addTex, ITTBlendKey blendKey)>> _stack = new();

            public ITTRenderTexture StackView => _stackViewTexture;

            public PrioritizedDeferredStack(TTCEUnityWithTTT4Unity ttce4U, Texture2D initialTexture)
            {
                _ttce4U = ttce4U;

                using var initialDiskTex = _ttce4U.Wrapping(initialTexture);
                _initialTexture = _ttce4U.CreateRenderTexture(initialDiskTex.Width, initialDiskTex.Hight);
                _ttce4U.LoadTexture(_initialTexture, initialDiskTex);

                _stackViewTexture = (_ttce4U as ITexTransToolForUnity).CloneRenderTexture(_initialTexture);
                _stackViewTexture.Name = $"{initialTexture.name}:PrioritizedDeferredStack-{_stackViewTexture.Width}x{_stackViewTexture.Hight}";

                _ttce4U.GetReferenceRenderTexture(_stackViewTexture).CopyFilWrap(initialTexture);
                _ttce4U.GetReferenceRenderTexture(_initialTexture).CopyFilWrap(initialTexture);
            }

            public void AddTextureStack(int priority, ITTRenderTexture addTex, ITTBlendKey blendKey)
            {
                if (!_stack.ContainsKey(priority)) { _stack.Add(priority, new()); }

                _stack[priority].Add(((_ttce4U as ITexTransToolForUnity).CloneRenderTexture(addTex), blendKey));// addTex は基本的に借用。だから所有権を増やす必要がある。
                UpdateNeeded = true;
            }
            public void ReleaseStackOfPriority(int priority)
            {
                if (!_stack.ContainsKey(priority)) { return; }
                var cs = _stack[priority];

                foreach (var l in cs) { l.addTex.Dispose(); }
                cs.Clear();
            }
            public void StackViewUpdate()
            {
                _ttce4U.CopyRenderTexture(_stackViewTexture, _initialTexture);
                foreach (var ptl in _stack)
                    foreach (var l in ptl.Value)
                    {
                        _ttce4U.BlendingWithAnySize(_stackViewTexture, l.addTex, l.blendKey);
                    }
                UpdateNeeded = false;
            }

            public bool ContainedPriority(int priority) => _stack.ContainsKey(priority);

            public void Dispose()
            {
                _stackViewTexture?.Dispose();
                _stackViewTexture = null!;
                foreach (var ptl in _stack) foreach (var l in ptl.Value) { l.addTex.Dispose(); }
                _stack.Clear();
            }
        }
    }
}
