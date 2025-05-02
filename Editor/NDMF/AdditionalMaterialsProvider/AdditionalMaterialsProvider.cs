#nullable enable

using System.Collections.Generic;
using UnityEngine;
using nadena.dev.ndmf;

namespace net.rs64.TexTransTool.NDMF.AdditionalMaterials
{
    internal interface IAdditionalMaterialsProvider
    {
        HashSet<Material> GetReferencedMaterials();
        void ReplaceReferencedMaterials(Dictionary<Material, Material> mapping);
    }

    internal class AdditionalMaterialsProvider
    {
        private readonly IAdditionalMaterialsProvider[] _providers;

        public AdditionalMaterialsProvider(BuildContext context)
        {
            _providers = new IAdditionalMaterialsProvider[]
            {
                new AnimatorMaterialsProvider(context),
#if CONTAINS_MA
                new MAMaterialsProvider(context)
#endif
            };
        }

        public HashSet<Material> GetReferencedMaterials()
        {
            var matHash = new HashSet<Material>();
            foreach (var provider in _providers)
            {
                matHash.UnionWith(provider.GetReferencedMaterials());
            }
            return matHash;
        }

        public void ReplaceReferencedMaterials(Dictionary<Material, Material> mapping)
        {
            foreach (var provider in _providers)
            {
                provider.ReplaceReferencedMaterials(mapping);
            }
        }
    }

}