#if UNITY_EDITOR
using System.Collections.Generic;
using JetBrains.Annotations;
using net.rs64.TexTransTool.Build;
using net.rs64.TexTransTool.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [DisallowMultipleComponent]
    internal abstract class TextureTransformer : MonoBehaviour, ITexTransToolTag
    {
        public virtual bool ThisEnable => gameObject.activeSelf && enabled;
        public abstract List<Renderer> GetRenderers { get; }
        public abstract bool IsPossibleApply { get; }
        public abstract TexTransPhase PhaseDefine { get; }
        [HideInInspector, SerializeField] int _saveDataVersion = ToolUtils.ThiSaveDataVersion;
        public int SaveDataVersion => _saveDataVersion;

        /// <summary>
        /// Applies this TextureTransformer with that domain
        /// You MUST NOT modify state of this component.
        /// </summary>
        /// <param name="domain">The domain</param>
        public abstract void Apply([NotNull] IDomain domain);
    }

    internal enum TexTransPhase
    {
        UnDefined,
        BeforeUVModification,
        UVModification,
        AfterUVModification,
    }
}
#endif
