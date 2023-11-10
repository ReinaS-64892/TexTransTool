using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Build;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
namespace net.rs64.TexTransTool.ReferenceResolver.TBResolver
{
    [RequireComponent(typeof(TextureBlender))]
    [AddComponentMenu("TexTransTool/Resolver/TTT TextureBlender AbsoluteTextureResolver")]
    public class AbsoluteTextureResolver : AbstractResolver
    {
        public Texture2D Texture;

        public override void Resolving(AvatarBuildUtils.ResolverContext avatar)
        {
            var relativeTexture = MLIResolver.AbsoluteTextureResolver.FindRelativeTexture(avatar.AvatarRoot, Texture);
            if (relativeTexture != null)
            {
                GetComponent<TextureBlender>().TargetTexture = relativeTexture;
            }
        }
    }
}