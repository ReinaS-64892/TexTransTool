#nullable enable
#if CONTAINS_MA

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using nadena.dev.ndmf;
using nadena.dev.modular_avatar.core;

namespace net.rs64.TexTransTool.NDMF.MA
{
    internal class NegotiateMAContext : IExtensionContext
    {
        public bool IsActive;
        public MaterialSwitchObject[]? MaterialSwitchObjects { get; private set; }
        public HashSet<Material> ReferencedMaterials
        {
            get
            {
                if (IsActive == false || MaterialSwitchObjects == null) new InvalidOperationException();

                return MaterialSwitchObjects
                    .Select(obj => obj.Material)
                    .OfType<Material>()
                    .ToHashSet();
            }
        }
        public void ReplaceReferencedMaterials(Dictionary<Material, Material> mapping)
        {
            if (IsActive == false || MaterialSwitchObjects == null) new InvalidOperationException();

            foreach (var obj in MaterialSwitchObjects!)
            {
                if (obj == null || obj.Material == null) continue;

                if (mapping.TryGetValue(obj.Material, out var newMaterial))
                {
                    obj.Material = newMaterial;
                }
            }
        }

        public void OnActivate(BuildContext context)
        {
            var setters = context.AvatarRootObject.GetComponentsInChildren<ModularAvatarMaterialSetter>(true);
            MaterialSwitchObjects = setters.SelectMany(setter => setter.Objects).OfType<MaterialSwitchObject>().ToArray();
            IsActive = true;
        }

        public void OnDeactivate(BuildContext context)
        {
            MaterialSwitchObjects = null;
            IsActive = false;
        }
    }
}

#endif
