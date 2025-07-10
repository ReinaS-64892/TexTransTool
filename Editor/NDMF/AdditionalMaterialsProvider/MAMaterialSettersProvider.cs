#nullable enable
#if MA_1_10_0_OR_NEWER

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using nadena.dev.ndmf;
using nadena.dev.modular_avatar.core;

namespace net.rs64.TexTransTool.NDMF.AdditionalMaterials
{
    internal class MAMaterialSettersProvider : IAdditionalMaterialsProvider
    {
        private readonly MaterialSwitchObject?[] _materialSwitchObjects;

        public MAMaterialSettersProvider(BuildContext context)
        {
            var setters = context.AvatarRootObject
                .GetComponentsInChildren<ModularAvatarMaterialSetter>(true);
            _materialSwitchObjects = setters
                .SelectMany(setter => setter.Objects)
                .OfType<MaterialSwitchObject>()
                .ToArray();
        }

        public HashSet<Material> GetReferencedMaterials()
        {
            return _materialSwitchObjects
                .Select(obj => obj?.Material)
                .UOfType<Material>()
                .ToHashSet();
        }

        public void ReplaceReferencedMaterials(Dictionary<Material, Material> mapping)
        {
            foreach (var obj in _materialSwitchObjects!)
            {
                if (obj == null || obj.Material == null) continue;

                if (mapping.TryGetValue(obj.Material, out var newMaterial))
                {
                    obj.Material = newMaterial;
                }
            }
        }
    }   
}

#endif
