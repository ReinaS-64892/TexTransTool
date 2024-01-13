using System.Collections.Generic;
using net.rs64.TexTransTool.TextureAtlas;
using UnityEngine;

namespace net.rs64.TexTransTool.ReferenceResolver.ATResolver
{
    [RequireComponent(typeof(AtlasTexture))]
    [DisallowMultipleComponent]
    [AddComponentMenu("TexTransTool/Resolver/TTT AtlasTexture AbsoluteMaterialResolver")]
    internal class AddAbsoluteMaterials : AbstractResolver
    {
        public List<AtlasTexture.MatSelector> AddSelectors = new List<AtlasTexture.MatSelector>();

        public override void Resolving(ResolverContext avatar)
        {
            var atlasTexture = GetComponent<AtlasTexture>();

            foreach (var add in AddSelectors)
            {
                var index = atlasTexture.SelectMatList.FindIndex(I => I.Material == add.Material);

                if (index != -1)
                {
                    var matSelector = atlasTexture.SelectMatList[index];
                    matSelector.AdditionalTextureSizeOffSet = add.AdditionalTextureSizeOffSet;
                    atlasTexture.SelectMatList[index] = matSelector;
                }
                else
                {
                    atlasTexture.SelectMatList.Add(add);
                }
            }

        }
    }
}