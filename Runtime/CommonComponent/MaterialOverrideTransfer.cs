using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class MaterialOverrideTransfer : TexTransCallEditorBehavior
    {
        internal const string Name = "TTT MaterialOverrideTransfer";
        internal const string FoldoutName = "Other";
        internal const string MenuPath = FoldoutName + "/" + Name;

        internal override List<Renderer> GetRenderers => null;

        internal override bool IsPossibleApply => TargetMaterial != null && MaterialVariantSource != null ;

        internal override TexTransPhase PhaseDefine => TexTransPhase.UnDefined;

        public Material TargetMaterial;
        public Material MaterialVariantSource;

        internal IEnumerable<Object> GetDependency(IDomain domain)
        {
            yield return TargetMaterial;
            yield return MaterialVariantSource;
        }

        internal int GetDependencyHash(IDomain domain)
        {
            var hash = TargetMaterial?.GetInstanceID() ?? 0;
            hash ^= MaterialVariantSource?.GetInstanceID() ?? 0;
            return hash;
        }
    }
}
