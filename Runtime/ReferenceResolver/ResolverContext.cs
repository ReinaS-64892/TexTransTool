using UnityEngine;

namespace net.rs64.TexTransTool.ReferenceResolver
{
    internal class ResolverContext
    {
        public readonly GameObject AvatarRoot;

        public ResolverContext(GameObject avatarGameObject)
        {
            AvatarRoot = avatarGameObject;
        }
    }

}