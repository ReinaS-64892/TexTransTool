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
        internal IEnumerable<Renderer> GetAutoMaterialFiltered<TObj>(IRendererTargeting rendererTargeting, TObj thisObj, Func<TObj, DecalRendererSelector> getRendererSelector)
        where TObj : UnityEngine.Object
        {
            return MaybeFilterDisableRenderers(
                rendererTargeting
                , rendererTargeting.RendererFilterForMaterial(
                    rendererTargeting.LookAtGet(
                        thisObj
                        , i => getRendererSelector(i).AutoSelectFilterMaterials.ToArray()//参照が同じになってしまうと比較できないから
                        , (l, r) => l.SequenceEqual(r)
                        )
                    )
                , thisObj
                , getRendererSelector
                );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<Renderer> MaybeFilterDisableRenderers<TObj>(IRendererTargeting rendererTargeting, IEnumerable<Renderer> r, TObj thisObj, Func<TObj, DecalRendererSelector> getRendererSelector)
        where TObj : UnityEngine.Object
        {
            if (rendererTargeting.LookAtGet(thisObj, i => getRendererSelector(i).IsAutoIncludingDisableRenderers)) return r;
            return FilterDisableRenderers(rendererTargeting, r);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IEnumerable<Renderer> FilterDisableRenderers(IRendererTargeting rendererTargeting, IEnumerable<Renderer> r)
        {
            return r.Where(i => rendererTargeting.LookAtGet(i, r => r.gameObject.activeInHierarchy) && rendererTargeting.LookAtGet(i, r => r.enabled));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal HashSet<Material> GetAutoMaterialHashSet(IRendererTargeting rendererTargeting)
        {
            return rendererTargeting.GetDomainsMaterialsHashSet(AutoSelectFilterMaterials);
        }
        // 自身のモードを確認していないため取り扱いには気を付けることね
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IEnumerable<Renderer> GetManualRenderers<TObj>(IRendererTargeting rendererTargeting, TObj thisObj, Func<TObj, DecalRendererSelector> getRendererSelector)
        where TObj : UnityEngine.Object
        {
            return rendererTargeting.GetDomainsRenderers(rendererTargeting.LookAtGet(thisObj, i => getRendererSelector(i).ManualSelections.ToArray(), (l, r) => l.SequenceEqual(r)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal HashSet<Material>? GetOrNullAutoMaterialHashSet(IRendererTargeting rendererTargeting)
        {
            switch (Mode)
            {
                default: { return null; }
                case RendererSelectMode.Auto:
                    {
                        if (UseMaterialFilteringForAutoSelect) { return GetAutoMaterialHashSet(rendererTargeting); }
                        return null;
                    }

            }
        }
        internal IEnumerable<Renderer> GetSelectedOrIncludingAll<TObj>(IRendererTargeting rendererTargeting, TObj thisObj, Func<TObj, DecalRendererSelector> getRendererSelector, out bool isIncludingAll)
        where TObj : UnityEngine.Object
        {
            switch (rendererTargeting.LookAtGet(thisObj, i => getRendererSelector(i).Mode))
            {
                default:
                case RendererSelectMode.Auto:
                    {
                        if (rendererTargeting.LookAtGet(thisObj, i => getRendererSelector(i).UseMaterialFilteringForAutoSelect))
                        {
                            isIncludingAll = false;
                            return GetAutoMaterialFiltered(rendererTargeting, thisObj, getRendererSelector);
                        }
                        isIncludingAll = true;
                        return MaybeFilterDisableRenderers(rendererTargeting, rendererTargeting.EnumerateRenderer(), thisObj, getRendererSelector);
                    }
                case RendererSelectMode.Manual:
                    {
                        isIncludingAll = false;
                        return GetManualRenderers(rendererTargeting, thisObj, getRendererSelector);
                    }
            }
        }

    }
}
