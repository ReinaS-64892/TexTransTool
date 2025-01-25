#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using net.rs64.TexTransCore;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace net.rs64.TexTransCoreEngineForUnity
{
    internal static class TTRt2
    {
        static TexTransCoreTextureFormat RGAndRFormat = TexTransCoreTextureFormat.Float;
        static TexTransCoreTextureFormat RGBAFormat = TexTransCoreTextureFormat.Byte;
        private static readonly Dictionary<TTRenderTextureDescriptor, List<TempRtState>> s_temporaryDictionary = new();
        private static readonly Dictionary<RenderTexture, TempRtState> s_reverseTempRtState = new();
        private static int s_releaseFrameCount = 0;

        public static void SetRGBAFormat(TexTransCoreTextureFormat format) { RGBAFormat = format; }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RenderTexture Get(int width, int height, TexTransCoreTextureChannel channel = TexTransCoreTextureChannel.RGBA)
        {
            return Get_Impl(new(width, height, channel));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RenderTexture Get_Impl(TTRenderTextureDescriptor renderTextureDescriptor)
        {
            if (s_temporaryDictionary.ContainsKey(renderTextureDescriptor) is false) { s_temporaryDictionary[renderTextureDescriptor] = new List<TempRtState>(); }
            var tmpList = s_temporaryDictionary[renderTextureDescriptor];
            var letIndex = tmpList.FindIndex(i => i.IsUsed is false);

            if (letIndex == -1)
            {
                var newTemp = new TempRtState(renderTextureDescriptor);
                newTemp.IsUsed = true;

                s_reverseTempRtState[newTemp.RenderTexture] = newTemp;
                tmpList.Add(newTemp);

                newTemp.RenderTexture.Clear();
                return newTemp.RenderTexture;
            }
            else
            {
                tmpList[letIndex].IsUsed = true;
                tmpList[letIndex].RenderTexture.Clear();

                return tmpList[letIndex].RenderTexture;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Rel(RenderTexture renderTexture)
        {
            if (s_reverseTempRtState.TryGetValue(renderTexture, out var state) is false) { throw new InvalidOperationException(); }
            state.IsUsed = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsTemp(RenderTexture rt) { return s_reverseTempRtState.ContainsKey(rt); }

        static void ReleaseUpdate()
        {
            s_releaseFrameCount += 1;
            if (s_releaseFrameCount <= 10) { return; }
            s_releaseFrameCount = 0;

            foreach (var rtDeskKV in s_temporaryDictionary)
            {
                var count = rtDeskKV.Value.Count;
                var list = rtDeskKV.Value;
                for (int i = 0; count > i; i += 1)
                {
                    var rtState = list[i];
                    if (rtState.IsUsed) { continue; }

#if TTT_DEBUG_TEMP_RT_TRACE
                    Debug.Log(rtState.name + " Release ");
#endif
                    list.RemoveAt(i);
                    rtState.Dispose();
                    return;
                }
            }
        }

        public static void ForceLeakedRelease()
        {
            foreach (var rtState in s_temporaryDictionary.SelectMany(i => i.Value))
            {
                if (rtState is null) { continue; }


#if TTT_DEBUG_TEMP_RT_TRACE
                Debug.Log("ForceReleased-" + rt.name);
#endif
                rtState.Dispose();
            }
            s_temporaryDictionary.Clear();
            s_reverseTempRtState.Clear();
        }

        [TexTransInitialize]
        static void RegisterForceLakedResolve()
        {
#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += ForceLeakedRelease;
#endif
            TexTransCoreRuntime.Update += ReleaseUpdate;
        }


        static void Clear(this RenderTexture rt)
        {
            var prv = RenderTexture.active;
            try
            {
                RenderTexture.active = rt;
                GL.Clear(true, true, UnityEngine.Color.clear);
            }
            finally
            {
                RenderTexture.active = prv;
            }
        }

        internal static bool Contains(RenderTexture renderTexture) { return s_reverseTempRtState.TryGetValue(renderTexture, out _); }

        private struct TTRenderTextureDescriptor : IEquatable<TTRenderTextureDescriptor>
        {
            public int Width;
            public int Height;
            public TexTransCoreTextureChannel Channel;

            public TTRenderTextureDescriptor(int width, int height, TexTransCoreTextureChannel channel)
            {
                Width = width;
                Height = height;
                Channel = channel;
            }
            public TTRenderTextureDescriptor(RenderTexture renderTexture) : this(renderTexture.descriptor) { }
            public TTRenderTextureDescriptor(RenderTextureDescriptor descriptor)
            {
                Width = descriptor.width;
                Height = descriptor.height;
                Channel = (TexTransCoreTextureChannel)GraphicsFormatUtility.GetComponentCount(descriptor.graphicsFormat);
            }
            public bool Equals(TTRenderTextureDescriptor other)
            {
                if (Width != other.Width) { return false; }
                if (Height != other.Height) { return false; }
                if (Channel != other.Channel) { return false; }
                return true;
            }
            public override bool Equals(object obj) { return obj is TTRenderTextureDescriptor rtd && Equals(rtd); }
            public override int GetHashCode() { return HashCode.Combine(Width, Height, Channel); }
        }
        private class TempRtState : IDisposable
        {
            public bool IsUsed;
            public RenderTexture RenderTexture;

            public TempRtState(TTRenderTextureDescriptor renderTextureDescriptor)
            {
                var format = renderTextureDescriptor.Channel is TexTransCoreTextureChannel.RGBA ? RGBAFormat : RGAndRFormat;
                RenderTexture = new RenderTexture(renderTextureDescriptor.Width, renderTextureDescriptor.Height, 0, format.ToUnityGraphicsFormat(renderTextureDescriptor.Channel));
                RenderTexture.enableRandomWrite = true;
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(RenderTexture);
            }
        }



    }
}
