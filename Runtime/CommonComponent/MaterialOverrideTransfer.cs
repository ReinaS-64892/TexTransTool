using UnityEngine;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class MaterialOverrideTransfer : TexTransCallEditorBehavior, IRendererTargetingAffecter
    {
        internal const string ComponentName = "TTT MaterialOverrideTransfer";
        internal const string MenuPath = TextureBlender.FoldoutName + "/" + ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.MaterialModification;

        [AffectVRAM] public Material TargetMaterial;
        [AffectVRAM] public Material MaterialVariantSource;
    }
}
