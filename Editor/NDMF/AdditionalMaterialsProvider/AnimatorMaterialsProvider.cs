#nullable enable

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.NDMF.AdditionalMaterials
{
    internal class AnimatorMaterialsProvider : IAdditionalMaterialsProvider
    {
        private readonly AnimatorServicesContext _animatorServicesContext;

        public AnimatorMaterialsProvider(BuildContext context)
        {
            _animatorServicesContext = context.Extension<AnimatorServicesContext>();
        }

        public HashSet<Material> GetReferencedMaterials()
        {
            return _animatorServicesContext.AnimationIndex
                .GetPPtrReferencedObjects
                .OfType<Material>()
                .ToHashSet();
        }

        public void ReplaceReferencedMaterials(Dictionary<Material, Material> mapping)
        {
            _animatorServicesContext.AnimationIndex.RewriteObjectCurves(obj => {
                if (obj is Material oldMat && mapping.TryGetValue(oldMat, out var newMat)) {
                    return newMat;
                }
                return obj;
            });
        }
    }
}
