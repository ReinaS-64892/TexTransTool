using net.rs64.TexTransTool.Decal;
using UnityEngine;

namespace net.rs64.TexTransTool.ReferenceResolver.ATResolver
{
    [AddComponentMenu("TexTransTool/Resolver/TTT Decal RendererMultiResolver")]
    internal class RayCastRendererMultiResolver : AbstractRayCastRendererResolver
    {
        public GameObject ResolveTargetRoot;

        public override void Resolving(ResolverContext avatar)
        {
            if (ResolveTargetRoot == null) { return; }

            var hits = FindRayCast(avatar.AvatarRoot);

            foreach (var abstractDecal in ResolveTargetRoot.GetComponentsInChildren<AbstractDecal>())
            {
                AddToDecal(abstractDecal, hits);
            }
        }
    }
}