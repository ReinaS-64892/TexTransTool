#if UNITY_EDITOR
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.TextureAtlas;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransTool.Build.AvatarBuildUtils;

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
#endif