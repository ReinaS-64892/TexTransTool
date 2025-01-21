#nullable enable
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    public abstract class TexTransRuntimeBehavior : TexTransBehavior, ITexTransToolTag
    {
        /// <summary>
        /// Applies this TextureTransformer with that domain
        /// You MUST NOT modify state of this component.
        /// </summary>
        /// <param name="domain">The domain</param>
        internal abstract void Apply(IDomain domain);

        internal abstract IEnumerable<Renderer> ModificationTargetRenderers(IRendererTargeting rendererTargeting);
        internal virtual void AffectingRendererTargeting(IAffectingRendererTargeting rendererTargetingModification)
        {
            // non op
        }
    }
}
