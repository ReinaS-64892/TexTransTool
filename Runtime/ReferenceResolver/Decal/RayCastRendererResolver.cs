using net.rs64.TexTransTool.Decal;
using UnityEngine;

namespace net.rs64.TexTransTool.ReferenceResolver.ATResolver
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    internal class RayCastRendererResolver : AbstractRayCastRendererResolver
    {
        internal const string ComponentName = "TTT Decal RendererResolver";
        private const string MenuPath = FoldoutName + "/" + ComponentName;
        public SimpleDecal ResolveTarget;

        public override void Resolving(ResolverContext avatar)
        {
            if (ResolveTarget == null) { return; }

            AddToDecal(ResolveTarget, FindRayCast(avatar.AvatarRoot));
        }
    }
}
