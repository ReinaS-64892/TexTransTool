#nullable enable
#if CONTAINS_TTCE_WGPU

using net.rs64.TexTransCore;
using UnityEditor;

namespace net.rs64.TexTransTool
{
    public static class TTCEWgpuDeviceWithTTT4UnityHolder
    {
        static TTCEWgpuDeviceWithTTT4Unity? s_wgpuDevice;

        public static TTCEWgpuDeviceWithTTT4Unity Device()
        {
            if (s_wgpuDevice is not null) { return s_wgpuDevice; }
            s_wgpuDevice = new(format: TTTProjectConfig.instance.InternalRenderTextureFormat);
            return s_wgpuDevice;
        }
        [InitializeOnLoadMethod]
        static void RegisterReleaseCall()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= ReleaseWgpuDevice;
            AssemblyReloadEvents.beforeAssemblyReload += ReleaseWgpuDevice;
            EditorApplication.quitting -= ReleaseWgpuDevice;
            EditorApplication.quitting += ReleaseWgpuDevice;
        }
        static void ReleaseWgpuDevice()
        {
            s_wgpuDevice?.Dispose();
            s_wgpuDevice = null;
        }
    }
}
#endif
