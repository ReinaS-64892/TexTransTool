using JetBrains.Annotations;

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
    }
}