using net.rs64.TexTransTool.TextureAtlas;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransTool.Build.AvatarBuildUtils;

namespace net.rs64.TexTransTool.ReferenceResolver.ATResolver
{
    [RequireComponent(typeof(AtlasTexture))]
    [DisallowMultipleComponent]
    public class RootObject : AbstractResolver
    {
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