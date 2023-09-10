#if UNITY_EDITOR
using System.Collections.Generic;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace net.rs64.TexTransTool
{
    public abstract class TextureTransformer : MonoBehaviour, ITexTransToolTag
    {
        public virtual bool ThisEnable => gameObject.activeSelf && enabled;
        public abstract List<Renderer> GetRenderers { get; }
        public abstract bool IsApply { get; set; }
        public abstract bool IsPossibleApply { get; }
        [FormerlySerializedAs("_IsSelfCallApply"), SerializeField] bool _PreviewApply;
        public virtual bool IsPreviewApply { get => _PreviewApply; protected set => _PreviewApply = value; }
        [HideInInspector, SerializeField] int _saveDataVersion = ToolUtils.ThiSaveDataVersion;
        public int SaveDataVersion => _saveDataVersion;
        public abstract void Apply(IDomain Domain = null);
    }
}
#endif
