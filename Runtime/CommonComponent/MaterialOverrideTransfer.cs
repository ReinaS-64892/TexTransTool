using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    public enum MaterialOverrideTransferMode
    {
        Variant,
        Record
    }

    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class MaterialOverrideTransfer : TexTransCallEditorBehavior
    {
        internal const string Name = "TTT MaterialOverrideTransfer";
        internal const string FoldoutName = "Other";
        internal const string MenuPath = FoldoutName + "/" + Name;
        internal override TexTransPhase PhaseDefine => TexTransPhase.UnDefined;

        public Material TargetMaterial;

        public MaterialOverrideTransferMode Mode = MaterialOverrideTransferMode.Variant;

        // MaterialOverrideTransferMode.Variant
        public Material MaterialVariantSource;

        // MaterialOverrideTransferMode.Record
        public Shader OverrideShader;
        public List<MaterialProperty> OverrideProperties = new();

        // EditorにおけるRecording用
        public bool IsRecording = false;
        public Material TempMaterial;
    }
}
