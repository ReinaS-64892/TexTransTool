#nullable enable
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
        public static IEnumerable<Renderer> ModificationTargetRenderers(this TexTransBehavior texTransBehavior, IRendererTargeting rendererTargeting)
        {
            switch (texTransBehavior)
            {
                case TexTransRuntimeBehavior texTransRuntime:
                    { return texTransRuntime.ModificationTargetRenderers(rendererTargeting); }
                case TexTransCallEditorBehavior texTransCallEditorBehavior:
                    { return EditorProcessorUtility.CallProcessorModificationTargetRenderers(texTransCallEditorBehavior, rendererTargeting); }

                default:
                    return Array.Empty<Renderer>();
            }
        }
        public static bool AffectingRendererTargeting(this IRendererTargetingAffecter texTransBehavior, IAffectingRendererTargeting affectingRendererTargeting)
        {
            switch (texTransBehavior)
            {
                case IRendererTargetingAffecterWithRuntime texTransRuntime:
                    { texTransRuntime.AffectingRendererTargeting(affectingRendererTargeting); return true; }
                case TexTransCallEditorBehavior texTransCallEditorBehavior:
                    { return EditorProcessorUtility.CallProcessorAffectingRendererTargeting(texTransCallEditorBehavior, affectingRendererTargeting); }
                default:
                    return false;
            }
        }
    }
}
