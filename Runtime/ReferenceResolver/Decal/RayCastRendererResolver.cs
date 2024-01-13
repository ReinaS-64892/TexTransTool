using net.rs64.TexTransTool.Decal;
using UnityEngine;

namespace net.rs64.TexTransTool.ReferenceResolver.ATResolver
{
    [AddComponentMenu("TexTransTool/Resolver/TTT Decal RendererResolver")]
    internal class RayCastRendererResolver : AbstractRayCastRendererResolver
    {
        public AbstractDecal ResolveTarget;

        public override void Resolving(ResolverContext avatar)
        {
            if (ResolveTarget == null) { return; }

            AddToDecal(ResolveTarget, FindRayCast(avatar.AvatarRoot));
        }
    }
}