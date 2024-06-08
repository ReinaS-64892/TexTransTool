using System.Collections.Generic;
using System.Security.Cryptography;
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
        internal abstract void Apply([NotNull] IDomain domain);

        internal abstract IEnumerable<Renderer> ModificationTargetRenderers(IEnumerable<Renderer> domainRenderers, OriginEqual replaceTracking);

        /// <summary>
        /// Enumerates references that depend on the component externally.
        /// </summary>
        internal abstract IEnumerable<UnityEngine.Object> GetDependency(IDomain domain);
        internal abstract int GetDependencyHash(IDomain domain);
    }
}
