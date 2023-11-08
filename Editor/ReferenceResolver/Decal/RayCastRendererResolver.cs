#if UNITY_EDITOR
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.TextureAtlas;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransTool.Build.AvatarBuildUtils;

namespace net.rs64.TexTransTool.ReferenceResolver.ATResolver
{
    [AddComponentMenu("TexTransTool/Resolver/TTT Decal RendererResolver")]
    public class RayCastRendererResolver : AbstractRayCastRendererResolver
    {
        public AbstractDecal ResolveTarget;

        public override void Resolving(ResolverContext avatar)
        {
            if (ResolveTarget == null) { return; }

            AddToDecal(ResolveTarget, FindRayCast(avatar.AvatarRoot));
        }
    }
}
#endif