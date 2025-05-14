#nullable enable
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + NACMenuPath)]
    [RequireComponent(typeof(SimpleDecal))]
    public sealed class SimpleDecalExperimentalFeature : TexTransAnnotation
    {
        internal const string Name = "TTT " + nameof(SimpleDecalExperimentalFeature);
        internal const string NACMenuPath = TextureBlender.FoldoutName + "/" + Name;

        public MultiLayerImageCanvas? OverrideDecalTextureWithMultiLayerImageCanvas;
        public bool UseDepth;
        public bool DepthInvert;
        internal bool? GetUseDepthOrInvert => UseDepth ? new bool?(DepthInvert) : null;
    }
}
