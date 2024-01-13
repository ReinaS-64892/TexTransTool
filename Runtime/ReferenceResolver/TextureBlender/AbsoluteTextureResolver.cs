using UnityEngine;

namespace net.rs64.TexTransTool.ReferenceResolver.TBResolver
{
    [RequireComponent(typeof(TextureBlender))]
    [AddComponentMenu("TexTransTool/Resolver/TTT TextureBlender AbsoluteTextureResolver")]
    internal class AbsoluteTextureResolver : AbstractResolver
    {
        public Texture2D Texture;

        public override void Resolving(ResolverContext avatar)
        {
            var relativeTexture = MLIResolver.AbsoluteTextureResolver.FindRelativeTexture(avatar.AvatarRoot, Texture);
            if (relativeTexture != null)
            {
                GetComponent<TextureBlender>().TargetTexture = relativeTexture;
            }
        }
    }
}