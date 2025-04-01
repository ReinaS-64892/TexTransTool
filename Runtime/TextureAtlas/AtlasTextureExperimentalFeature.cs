using System.Collections.Generic;
using net.rs64.TexTransTool.TextureAtlas;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + NACMenuPath)]
    [RequireComponent(typeof(AtlasTexture))]
    public sealed class AtlasTextureExperimentalFeature : TexTransAnnotation
    {
        internal const string Name = "TTT " + nameof(AtlasTextureExperimentalFeature);
        internal const string NACMenuPath = TextureBlender.FoldoutName + "/" + Name;

        public List<TextureSelector> UnsetTextures = new();

        public List<TextureIndividualTuning> TextureIndividualFineTuning = new();


        public bool AutoTextureSizeSetting = false;
        public bool AutoReferenceCopySetting = false;
        public bool AutoMergeTextureSetting = false;
    }
}
