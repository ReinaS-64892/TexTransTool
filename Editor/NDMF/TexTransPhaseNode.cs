#if NDMF_1_5_x
using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.preview;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.NDMF
{
    internal class TexTransPhaseNode : IRenderFilterNode
    {
        //TODO : 決め打ちじゃなくて、もっと調べて正しい状態にしてもいい気がする。
        public RenderAspects Reads => _nodeDomain.UsedLookAt ? RenderAspects.Everything : 0;
        public RenderAspects WhatChanged
        {
            get
            {
                RenderAspects flag = 0;

                if (_nodeDomain.UsedMaterialReplace) flag |= RenderAspects.Material;
                if (_nodeDomain.UsedSetMesh) flag |= RenderAspects.Mesh | RenderAspects.Texture;
                if (_nodeDomain.UsedTextureStack) flag |= RenderAspects.Material | RenderAspects.Texture;

                return flag;
            }
        }

        NodeExecuteDomain _nodeDomain;
        internal IEnumerable<TexTransPhase> TargetPhase;
        public void NodeExecuteAndInit(IEnumerable<TexTransBehavior> flattenTTB, IEnumerable<(Renderer origin, Renderer proxy)> proxyPairs, ComputeContext ctx)
        {
            Profiler.BeginSample("NodeExecuteDomain.ctr");
            _nodeDomain = new NodeExecuteDomain(proxyPairs, ctx, ObjectRegistry.ActiveRegistry);
            Profiler.EndSample();
            Profiler.BeginSample("apply ttb s");
            foreach (var ttb in flattenTTB)
            {
                if (ttb == null) { continue; }
                ctx.Observe(ttb);
                Profiler.BeginSample("apply-" + ttb.name);
                ttb.Apply(_nodeDomain);
                Profiler.EndSample();
            }
            Profiler.EndSample();
            Profiler.BeginSample("DomainFinish");
            _nodeDomain.DomainFinish();
            Profiler.EndSample();
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

        public override string ToString()
        {
            return base.ToString() + string.Join("-", TargetPhase.Select(i => i.ToString()));
        }
    }
}
#endif
