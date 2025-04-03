#nullable enable
using UnityEngine;
using System.Collections.Generic;
using System;
using net.rs64.TexTransTool.Utils;
using System.Linq;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class UVCopy : TexTransRuntimeBehavior
    {
        internal const string ComponentName = "TTT " + nameof(UVCopy);
        internal const string MenuPath = TextureBlender.FoldoutName + "/" + ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.UVModification;

        public List<Mesh> TargetMeshes = new();

        public UVChannel CopySource = UVChannel.UV0;
        public UVChannel CopyTarget = UVChannel.UV1;


        internal override void Apply(IDomain domain)
        {
            domain.LookAt(this);
            var targets = ModificationTargetRenderers(domain);

            if (targets.Any() is false) { return; }

            var distMeshes = targets.Select(r => domain.GetMesh(r)).Distinct().Where(m => m != null).Cast<Mesh>();
            var replaceDict = new Dictionary<Mesh, Mesh>();

            var copySource = Math.Clamp((int)CopySource, 0, 7);
            var copyTarget = Math.Clamp((int)CopyTarget, 0, 7);

            UnityEngine.Rendering.VertexAttribute distVertexAttribute = UnityEngine.Rendering.VertexAttribute.TexCoord0 + copySource;
            List<Vector2>? uvDim2 = null;
            List<Vector3>? uvDim3 = null;
            List<Vector4>? uvDim4 = null;
            foreach (var dMesh in distMeshes)
            {
                domain.LookAt(dMesh);
                var copedMesh = replaceDict[dMesh] = UnityEngine.Object.Instantiate(dMesh);
                copedMesh.name = dMesh.name + "(Clone from TTT UVCopy)";

                switch (copedMesh.GetVertexAttributeDimension(distVertexAttribute))
                {

                    default:
                    case 2:
                        {
                            uvDim2 ??= new();
                            copedMesh.GetUVs(copySource, uvDim2);
                            copedMesh.SetUVs(copyTarget, uvDim2);
                            break;
                        }
                    case 3:
                        {
                            uvDim3 ??= new();
                            copedMesh.GetUVs(copySource, uvDim3);
                            copedMesh.SetUVs(copyTarget, uvDim3);
                            break;
                        }
                    case 4:
                        {
                            uvDim4 ??= new();
                            copedMesh.GetUVs(copySource, uvDim4);
                            copedMesh.SetUVs(copyTarget, uvDim4);
                            break;
                        }
                }
            }

            foreach (var r in targets)
                domain.SetMesh(r, replaceDict[domain.GetMesh(r)!]);

            domain.TransferAssets(replaceDict.Values);
            domain.RegisterReplaces(replaceDict);
        }

        internal override IEnumerable<Renderer> ModificationTargetRenderers(IRendererTargeting rendererTargeting)
        {
            var targetMeshes = TargetMeshes.Where(i => i != null).ToArray();
            return rendererTargeting.EnumerateRenderer()
                 .Where(r => rendererTargeting.GetMesh(r) != null)
                 .Where(r =>
                 {
                     var mesh = rendererTargeting.GetMesh(r);
                     return TargetMeshes.Any(tm => rendererTargeting.OriginEqual(tm, mesh));
                 });
        }
    }
}
