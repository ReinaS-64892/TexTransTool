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
        public static void AffectingRendererTargeting(this TexTransBehavior texTransBehavior, IAffectingRendererTargeting affectingRendererTargeting)
        {
            switch (texTransBehavior)
            {
                case TexTransRuntimeBehavior texTransRuntime:
                    { texTransRuntime.AffectingRendererTargeting(affectingRendererTargeting); return; }
                case TexTransCallEditorBehavior texTransCallEditorBehavior:
                    { EditorProcessorUtility.CallProcessorAffectingRendererTargeting(texTransCallEditorBehavior, affectingRendererTargeting); return; }

                default:
                    return;
            }
        }
    }
}
