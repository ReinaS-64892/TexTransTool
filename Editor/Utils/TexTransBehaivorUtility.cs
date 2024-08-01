using System;
using System.Collections.Generic;
using net.rs64.TexTransTool.EditorProcessor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal static class TexTransBehaviorUtility
    {
        public static void Apply(this TexTransBehavior texTransBehavior, IDomain domain)
        {
            switch (texTransBehavior)
            {
                case TexTransRuntimeBehavior texTransRuntime:
                    { texTransRuntime.Apply(domain); break; }
                case TexTransCallEditorBehavior texTransCallEditorBehavior:
                    { EditorProcessorUtility.CallProcessorApply(texTransCallEditorBehavior, domain); break; }
            }
        }
        public static IEnumerable<Renderer> ModificationTargetRenderers(this TexTransBehavior texTransBehavior, IEnumerable<Renderer> domainRenderers, OriginEqual replaceTracking)
        {
            switch (texTransBehavior)
            {
                case TexTransRuntimeBehavior texTransRuntime:
                    { return texTransRuntime.ModificationTargetRenderers(domainRenderers, replaceTracking); }
                case TexTransCallEditorBehavior texTransCallEditorBehavior:
                    { return EditorProcessorUtility.CallProcessorModificationTargetRenderers(texTransCallEditorBehavior, domainRenderers, replaceTracking); }

                default:
                    return Array.Empty<Renderer>();
            }
        }
    }
}
