using UnityEngine;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class MaterialOverrideTransfer : TexTransCallEditorBehavior
    {
        internal const string ComponentName = "TTT MaterialOverrideTransfer";
        internal const string FoldoutName = "Other";
        internal const string MenuPath = FoldoutName + "/" + ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.UnDefined;

        public Material TargetMaterial;
        public Material MaterialVariantSource;
    }
}
