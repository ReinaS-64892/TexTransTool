using net.rs64.TexTransTool.TextureAtlas;
using UnityEngine;

namespace net.rs64.TexTransTool.ReferenceResolver.ATResolver
{
    [RequireComponent(typeof(AtlasTexture))]
    [DisallowMultipleComponent]
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    internal class RootObject : AbstractResolver
    {
        internal const string ComponentName = "TTT AtlasTexture RootObjectResolver";
        private const string MenuPath = FoldoutName + "/" + ComponentName;
        [SerializeField] SelectEnum SelectType;

        enum SelectEnum
        {
            AvatarRoot,
            RootFormPath,
        }
        public string RootFormPath;


        public override void Resolving(ResolverContext avatar)
        {
            var atlasTexture = GetComponent<AtlasTexture>();
            switch (SelectType)
            {
                case SelectEnum.AvatarRoot:
                    {
                        atlasTexture.TargetRoot = avatar.AvatarRoot;
                        break;
                    }
                case SelectEnum.RootFormPath:
                    {
                        atlasTexture.TargetRoot = avatar.AvatarRoot.transform.Find(RootFormPath).gameObject;
                        break;
                    }
            }
        }
    }
}
