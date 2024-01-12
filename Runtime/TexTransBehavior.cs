using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    internal abstract class TexTransBehavior : MonoBehaviour, ITexTransToolTag
    {
        public virtual bool ThisEnable => gameObject.activeSelf;
        public abstract List<Renderer> GetRenderers { get; }
        public abstract bool IsPossibleApply { get; }
        public abstract TexTransPhase PhaseDefine { get; }

        //v0.3.x == 0
        //v0.4.x == 1
        //v0.5.x == 2
        public const int TTTDataVersion = 2;

        [HideInInspector, SerializeField] int _saveDataVersion = TTTDataVersion;
        public int SaveDataVersion => _saveDataVersion;

        protected virtual void OnDestroy()
        {
            DestroyCall.DestroyThis(this);
            // if (PreviewContext.IsPreviewing(this)) { PreviewContext.instance.ExitPreview(); }
        }
    }

    internal static class DestroyCall
    {
        public static Action<TexTransBehavior> OnDestroy;
        public static void DestroyThis(TexTransBehavior destroy) => OnDestroy?.Invoke(destroy);

    }

    internal enum TexTransPhase
    {
        UnDefined,
        BeforeUVModification,
        UVModification,
        AfterUVModification,
    }
}