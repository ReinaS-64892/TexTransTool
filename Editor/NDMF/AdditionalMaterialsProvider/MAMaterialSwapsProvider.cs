#nullable enable
#if MA_1_13_0_OR_NEWER

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using nadena.dev.ndmf;
using nadena.dev.modular_avatar.core;

namespace net.rs64.TexTransTool.NDMF.AdditionalMaterials
{
    internal class MAMaterialSwapsProvider : IAdditionalMaterialsProvider
    {
        private readonly ModularAvatarMaterialSwap[] _swappers;

        public MAMaterialSwapsProvider(BuildContext context)
        {
            _swappers = context.AvatarRootObject
                .GetComponentsInChildren<ModularAvatarMaterialSwap>(true)
                .ToArray();
        }

        public HashSet<Material> GetReferencedMaterials()
        {
            return _swappers
                .SelectMany(swap => swap.Swaps)
                .SelectMany(swap => new[] { swap.From, swap.To })
                .UOfType<Material>()
                .ToHashSet();
        }

        public void ReplaceReferencedMaterials(Dictionary<Material, Material> mapping)
        {
            for (int i = 0; i < _swappers.Length; i++)
            {
                var swapper = _swappers[i];
                
                for (int j = 0; j < swapper.Swaps.Count; j++)
                {
                    var matSwap = swapper.Swaps[j].Clone();
                    var from = matSwap.From;
                    var to = matSwap.To;
                    
                    bool swapChanged = false;
                    
                    if (from != null && mapping.TryGetValue(from, out var newFrom))
                    {
                        matSwap.From = newFrom;
                        swapChanged = true;
                    }
                    if (to != null && mapping.TryGetValue(to, out var newTo))
                    {
                        matSwap.To = newTo;
                        swapChanged = true;
                    }

                    if (swapChanged)
                    {
                        swapper.Swaps[j] = matSwap;
                    }
                }
            }
        }
    }   
}

#endif
