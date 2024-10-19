using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.preview;
using net.rs64.TexTransTool.Build;
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
        internal TexTransPhase TargetPhase;
        internal Dictionary<Renderer, Renderer> o2pDict;
        internal Renderer[][] ofRenderers;
        internal Dictionary<TexTransBehavior, (int index, int domainIndex)> behaviorIndex;
        public void NodeExecuteAndInit(IEnumerable<TexTransBehavior> flattenTTB, ComputeContext ctx)
        {
            Profiler.BeginSample("NodeExecuteDomain.ctr");
            _nodeDomain = new NodeExecuteDomain(o2pDict, ctx, ObjectRegistry.ActiveRegistry);
            Profiler.EndSample();
            Profiler.BeginSample("apply ttb s");
            foreach (var domainBy in flattenTTB.GroupBy(t => behaviorIndex[t].domainIndex).OrderBy(t => t.Key))
            {
                IDomain domain = domainBy.Key != (ofRenderers.Length - 1) ? _nodeDomain.GetSubDomain(ofRenderers[domainBy.Key]) : _nodeDomain;
                foreach (var behavior in domainBy.OrderBy(t => behaviorIndex[t].index))
                {
                    if (behavior == null) { continue; }
                    ctx.Observe(behavior);

                    Profiler.BeginSample("apply-" + behavior.name);
                    behavior.Apply(domain);
                    Profiler.EndSample();
                }
                Profiler.BeginSample("MargeStack Or DomainFinish");
                if (domain is NodeExecuteDomain) _nodeDomain.DomainFinish();
                else { (domain as NodeExecuteDomain.NodeSubDomain).MergeStack(); }
                Profiler.EndSample();
            }
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
            return base.ToString() + string.Join("-", TargetPhase.ToString());
        }
    }
}
