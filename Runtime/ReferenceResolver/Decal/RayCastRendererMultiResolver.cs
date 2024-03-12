using net.rs64.TexTransTool.Decal;
using UnityEngine;

namespace net.rs64.TexTransTool.ReferenceResolver.ATResolver
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    internal class RayCastRendererMultiResolver : AbstractRayCastRendererResolver
    {
        internal const string ComponentName = "TTT Decal RendererMultiResolver";
        private const string MenuPath = FoldoutName + "/" + ComponentName;
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
