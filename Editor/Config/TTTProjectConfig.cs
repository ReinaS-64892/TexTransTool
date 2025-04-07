using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [FilePath("ProjectSettings/TexTransTool/TTTProjectConfig.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class TTTProjectConfig : ScriptableSingleton<TTTProjectConfig>
    {
        [SerializeField] TexTransCoreTextureFormat internalRenderTextureFormat = TexTransCoreTextureFormat.Byte;

        public TexTransCoreTextureFormat InternalRenderTextureFormat
        {
            get => internalRenderTextureFormat;
            set
            {
                if (internalRenderTextureFormat == value) { return; }
                internalRenderTextureFormat = value;
                Save();
                s_OnChangeInternalRenderTextureFormat?.Invoke(internalRenderTextureFormat);
            }
        }

        [SerializeField] TexTransCoreEngineBackendEnum ttceBackend = TexTransCoreEngineBackendEnum.Unity;

        public enum TexTransCoreEngineBackendEnum{
            Unity = 0,
            Wgpu = 1,
        }
        public TexTransCoreEngineBackendEnum TexTransCoreEngineBackend
        {
            get => ttceBackend;
            set
            {
                if (ttceBackend == value) { return; }
                ttceBackend = value;
                Save();
            }
        }

        [SerializeField] bool ShowDebugItems = false;

        private void Save()
        {
            Save(true);
        }




        [TexTransCoreEngineForUnity.TexTransInitialize]
        internal static void InitInternalTextureFormat()
        {
            TTRt2.SetRGBAFormat(instance.InternalRenderTextureFormat);
            s_OnChangeInternalRenderTextureFormat -= TTRt2.SetRGBAFormat;
            s_OnChangeInternalRenderTextureFormat += TTRt2.SetRGBAFormat;
        }
        static event Action<TexTransCoreTextureFormat> s_OnChangeInternalRenderTextureFormat;
    }
}
