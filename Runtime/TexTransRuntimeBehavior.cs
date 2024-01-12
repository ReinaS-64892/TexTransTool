using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using net.rs64.TexTransTool.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal abstract class TexTransRuntimeBehavior : TexTransBehavior, ITexTransToolTag
    {
        /// <summary>
        /// Applies this TextureTransformer with that domain
        /// You MUST NOT modify state of this component.
        /// </summary>
        /// <param name="domain">The domain</param>
        public abstract void Apply([NotNull] IDomain domain);
    }
}