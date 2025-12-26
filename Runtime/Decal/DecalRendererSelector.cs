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

        public DecalRendererSelector() { }
        public DecalRendererSelector(DecalRendererSelector source)
        {
            Mode = source.Mode;
            UseMaterialFilteringForAutoSelect = source.UseMaterialFilteringForAutoSelect;
            IsAutoIncludingDisableRenderers = source.IsAutoIncludingDisableRenderers;
            AutoSelectFilterMaterials = source.AutoSelectFilterMaterials;
            ManualSelections = source.ManualSelections;
        }
        public static bool ValueEqual(DecalRendererSelector l, DecalRendererSelector r)
        {
            if (l.Mode != r.Mode) { return false; }
            if (l.UseMaterialFilteringForAutoSelect != r.UseMaterialFilteringForAutoSelect) { return false; }
            if (l.IsAutoIncludingDisableRenderers != r.IsAutoIncludingDisableRenderers) { return false; }
            if (l.AutoSelectFilterMaterials.SequenceEqual(r.AutoSelectFilterMaterials) is false) { return false; }
            if (l.ManualSelections.SequenceEqual(r.ManualSelections) is false) { return false; }
            return true;
        }

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

        internal IEnumerable<Renderer> GetSelected(IDomainReferenceViewer domainView)
        {
            switch (Mode)
            {
                default:
                case RendererSelectMode.Auto:
                    {
                        if (UseMaterialFilteringForAutoSelect)
                            return MaybeFilterDisableRenderers(domainView, domainView.RendererFilterForMaterial(AutoSelectFilterMaterials));
                        else
                            return MaybeFilterDisableRenderers(domainView, domainView.EnumerateRenderers());
                    }
                case RendererSelectMode.Manual:
                    {
                        return GetManualRenderers(domainView);
                    }
            }
        }
        IEnumerable<Renderer> MaybeFilterDisableRenderers(IDomainReferenceViewer rendererTargeting, IEnumerable<Renderer> r)
        {
            if (IsAutoIncludingDisableRenderers) return r;
            // TODO : これどうなの? IDomainReferenceViewer を頼りに探るべきでは
            return r.Where(i => rendererTargeting.ObserveToGet(i, r => r.gameObject.activeInHierarchy) && rendererTargeting.ObserveToGet(i, r => r.enabled));
        }
        IEnumerable<Renderer> GetManualRenderers(IDomainReferenceViewer rendererTargeting)
        {
            return rendererTargeting.GetDomainsRenderers(ManualSelections);
        }

        internal HashSet<Material>? GetOrNullAutoMaterialHashSet(IDomainReferenceViewer rendererTargeting)
        {
            switch (Mode)
            {
                default: { return null; }
                case RendererSelectMode.Auto:
                    {
                        if (UseMaterialFilteringForAutoSelect) { return rendererTargeting.GetDomainsMaterialsHashSet(AutoSelectFilterMaterials); }
                        return null;
                    }

            }
        }


    }
}
