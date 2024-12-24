#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace net.rs64.TexTransTool.Decal
{
    [Serializable]
    public class DecalRendererSelector
    {
        public RendererSelectMode Mode = RendererSelectMode.Auto;

        public bool UseMaterialFilteringForAutoSelect = false;
        public bool IsAutoIncludingDisableRenderers = false;
        public List<Material> AutoSelectFilterMaterials = new();

        public List<Renderer> ManualSelections = new();

        internal bool IsTargetNotSet()
        {
            switch (Mode)
            {
                default:
                case RendererSelectMode.Auto:
                    {
                        if (UseMaterialFilteringForAutoSelect is false) { return false; }
                        return AutoSelectFilterMaterials.Any() is false;
                    }
                case RendererSelectMode.Manual: { return ManualSelections.Any() is false; }
            }
        }


        // 自身のモード、そして、マテリアルによるフィルタリングを行うかなどを加味してないから取り扱いには気を付けることね
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IEnumerable<Renderer> GetAutoMaterialFiltered(IEnumerable<Renderer> domainRenderers, OriginEqual originEqual)
        {
            return MaybeFilterDisableRenderers(originEqual.RendererFilterForMaterial(domainRenderers, AutoSelectFilterMaterials));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<Renderer> MaybeFilterDisableRenderers(IEnumerable<Renderer> r)
        {
            if (IsAutoIncludingDisableRenderers) return r;
            return FilterDisableRenderers(r);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IEnumerable<Renderer> FilterDisableRenderers(IEnumerable<Renderer> r)
        {
            return r.Where(i => i.gameObject.activeInHierarchy && i.enabled);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal HashSet<Material> GetAutoMaterialHashSet(IEnumerable<Renderer> domainRenderers, OriginEqual originEqual)
        {
            return originEqual.GetDomainsMaterialsHashSet(domainRenderers, AutoSelectFilterMaterials);
        }
        // 自身のモードを確認していないため取り扱いには気を付けることね
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IEnumerable<Renderer> GetManualRenderers(IEnumerable<Renderer> domainRenderers, OriginEqual originEqual)
        {
            return originEqual.GetDomainsRenderers(domainRenderers, ManualSelections);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal HashSet<Material>? GetOrNullAutoMaterialHashSet(IEnumerable<Renderer> domainRenderers, OriginEqual originEqual)
        {
            switch (Mode)
            {
                default: { return null; }
                case RendererSelectMode.Auto:
                    {
                        if (UseMaterialFilteringForAutoSelect) { return GetAutoMaterialHashSet(domainRenderers, originEqual); }
                        return null;
                    }

            }
        }
        internal IEnumerable<Renderer> GetSelectedOrIncludingAll(IEnumerable<Renderer> domainRenderers, OriginEqual originEqual, out bool isIncludingAll)
        {
            switch (Mode)
            {
                default:
                case RendererSelectMode.Auto:
                    {
                        if (UseMaterialFilteringForAutoSelect)
                        {
                            isIncludingAll = false;
                            return GetAutoMaterialFiltered(domainRenderers, originEqual);
                        }
                        isIncludingAll = true;
                        return MaybeFilterDisableRenderers(domainRenderers);
                    }
                case RendererSelectMode.Manual:
                    {
                        isIncludingAll = false;
                        return GetManualRenderers(domainRenderers, originEqual);
                    }
            }
        }

    }
}
