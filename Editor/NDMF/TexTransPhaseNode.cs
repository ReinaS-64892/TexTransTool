using System;
using System.Collections.Generic;
using nadena.dev.ndmf.preview;
using nadena.dev.ndmf.rq;
using nadena.dev.ndmf.rq.unity.editor;
using UnityEngine;

namespace net.rs64.TexTransTool.NDMF
{
    internal class TexTransPhaseNode : IRenderFilterNode
    {
        //TODO : 決め打ちじゃなくて、もっと調べて正しい状態にしてもいい気がする。
        public RenderAspects Reads => RenderAspects.Everything;
        public RenderAspects WhatChanged => RenderAspects.Material | RenderAspects.Mesh | RenderAspects.Texture;

        NodeExecuteDomain _nodeDomain;

        public void NodeExecuteAndInit(IEnumerable<TexTransBehavior> flattenTTB, IEnumerable<(Renderer origin, Renderer proxy)> proxyPairs, ComputeContext ctx)
        {
            _nodeDomain = new NodeExecuteDomain(proxyPairs, ctx);
            foreach (var ttb in flattenTTB)
            {
                if (ttb == null) { continue; }
                ctx.Observe(ttb);
                ttb.Apply(_nodeDomain);
            }
            _nodeDomain.DomainFinish();
        }
        public void OnFrame(Renderer original, Renderer proxy)
        {
            _nodeDomain.DomainRecaller(original, proxy);
        }

        void IDisposable.Dispose()
        {
            _nodeDomain.Dispose();
            _nodeDomain = null;
        }
    }
}
