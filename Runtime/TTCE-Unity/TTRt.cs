using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using net.rs64.TexTransCoreEngineForUnity.Utils;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace net.rs64.TexTransCoreEngineForUnity
{
    internal static class TTRt
    {
        private static readonly Dictionary<TTRenderTextureDescriptor, List<TempRtState>> s_temporaryDictionary = new();
        private static readonly Dictionary<RenderTexture, TempRtState> s_reverseTempRtState = new();
        private static int s_releaseFrameCount = 0;
        private struct TTRenderTextureDescriptor : IEquatable<TTRenderTextureDescriptor>
        {
            public int Width;
            public int Height;
            public bool UseDepthAndStencil;
            public bool UseMipMap;

            public TTRenderTextureDescriptor(int width, int height, bool useDepthAndStencil, bool useMipMap)
            {
                Width = width;
                Height = height;
                UseDepthAndStencil = useDepthAndStencil;
                UseMipMap = useMipMap;
            }
            public TTRenderTextureDescriptor(RenderTexture renderTexture)
            {
                Width = renderTexture.width;
                Height = renderTexture.height;
                UseDepthAndStencil = renderTexture.depth != 0;
                UseMipMap = renderTexture.useMipMap;
            }
            public TTRenderTextureDescriptor(RenderTextureDescriptor descriptor)
            {
                Width = descriptor.width;
                Height = descriptor.height;
                UseDepthAndStencil = descriptor.depthStencilFormat != UnityEngine.Experimental.Rendering.GraphicsFormat.None;
                UseMipMap = descriptor.useMipMap;
            }
            public bool Equals(TTRenderTextureDescriptor other)
            {
                if (Width != other.Width) { return false; }
                if (Height != other.Height) { return false; }
                if (UseDepthAndStencil != other.UseDepthAndStencil) { return false; }
                if (UseMipMap != other.UseMipMap) { return false; }
                return true;
            }
            public override bool Equals(object obj) { return obj is TTRenderTextureDescriptor rtd && Equals(rtd); }
            public override int GetHashCode() { return HashCode.Combine(Width, Height, UseDepthAndStencil, UseMipMap); }
        }
        private class TempRtState : IDisposable
        {
            public bool IsUsed;
            public RenderTexture RenderTexture;

            public TempRtState(TTRenderTextureDescriptor renderTextureDescriptor)
            {
                var readWrite = TTCoreEngineForUnity.IsLinerRenderTexture ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB;
                var format = RenderTextureFormat.ARGB32;

                RenderTexture = new RenderTexture(
                    renderTextureDescriptor.Width,
                    renderTextureDescriptor.Height,
                    renderTextureDescriptor.UseDepthAndStencil ? 32 : 0,
                    format, readWrite);

                RenderTexture.enableRandomWrite = true;
                RenderTexture.useMipMap = renderTextureDescriptor.UseMipMap;
                RenderTexture.autoGenerateMips = false;
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(RenderTexture);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static RenderTexture Get_Impl(TTRenderTextureDescriptor renderTextureDescriptor)
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
        static void Rel_Impl(RenderTexture renderTexture)
        {
            if (s_reverseTempRtState.TryGetValue(renderTexture, out var state) is false) { throw new InvalidOperationException(); }
            state.IsUsed = false;
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RenderTexture G(int size) => G(size, size);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RenderTexture G(int size, string rtName)
        {
            var rt = G(size, size);
            rt.name = rtName;
            return rt;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UsingRenderTexture U(out RenderTexture tmpRt, int size)
        {
            tmpRt = G(size);
            return new(tmpRt);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RenderTexture Get(int width, int height, bool useDepthAndStencil = false, bool useMipMap = false)
        {
            var tmpRt = Get_Impl(new(width, height, useDepthAndStencil, useMipMap));

            s_tempSet.Add(tmpRt);

#if TTT_DEBUG_TEMP_RT_TRACE
            s_tempHashSet.Add(tmpRt.GetHashCode());
            Debug.Log("GetRt " + tmpRt.GetHashCode() + " - let " + s_tempSet.Count);
            s_totalGetCount += 1;
#endif

            return tmpRt;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RenderTexture G(int width, int height, bool clearRt = false, bool useDepthAndStencil = false, bool useMipMap = false, bool useRandomRW = false, RenderTextureFormat? rtFormat = null)
        {
            return Get(width, height, useDepthAndStencil, useMipMap);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UsingRenderTexture U(out RenderTexture tmpRt, int width, int height, bool clearRt = false, bool useDepthAndStencil = false, bool useMipMap = false, bool useRandomRW = false, RenderTextureFormat? rtFormat = null)
        {
            tmpRt = G(width, height, clearRt, useDepthAndStencil, useMipMap, useRandomRW, rtFormat);
            return new(tmpRt);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RenderTexture G(RenderTexture rt, bool copyTexture = false)
        {
            var newRt = Get_Impl(new(rt));

            s_tempSet.Add(newRt);
#if TTT_DEBUG_TEMP_RT_TRACE
            s_tempHashSet.Add(newRt.GetHashCode());
            Debug.Log("GetRt " + newRt.GetHashCode() + " - let " + s_tempSet.Count);
            s_totalGetCount += 1;
#endif

            newRt.CopyFilWrap(rt);
            if (copyTexture) Graphics.CopyTexture(rt, newRt);
            return newRt;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UsingRenderTexture U(out RenderTexture tmpRt, RenderTexture rt, bool copyTexture = false)
        {
            tmpRt = G(rt, copyTexture); return new(tmpRt);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RenderTexture G(RenderTextureDescriptor renderTextureDescriptor, bool clearRt = false)
        {
            var rt = Get_Impl(new(renderTextureDescriptor));
            s_tempSet.Add(rt);
#if TTT_DEBUG_TEMP_RT_TRACE
            s_tempHashSet.Add(rt.GetHashCode());
            Debug.Log("GetRt " + rt.GetHashCode() + " - let " + s_tempSet.Count);
            s_totalGetCount += 1;
#endif
            return rt;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UsingRenderTexture U(out RenderTexture tmpRt, RenderTextureDescriptor renderTextureDescriptor, bool clearRt = false)
        {
            tmpRt = G(renderTextureDescriptor); return new(tmpRt);
        }


        internal static HashSet<RenderTexture> s_tempSet = new();
#if TTT_DEBUG_TEMP_RT_TRACE
        internal static int s_totalGetCount = 0;
        internal static HashSet<int> s_tempHashSet = new();
        internal static int s_tempSomeTimeMaxCount = 0;
#endif

        public static bool IsTemp(RenderTexture renderTexture) => s_tempSet.Contains(renderTexture);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Rel(RenderTexture renderTexture)
        {
            s_tempSet.Remove(renderTexture);
#if TTT_DEBUG_TEMP_RT_TRACE
            Debug.Log("RelRt " + renderTexture.GetHashCode() + " - let " + s_tempSet.Count);
            s_tempSomeTimeMaxCount = Math.Max(s_tempSomeTimeMaxCount, s_tempSet.Count);
#endif
            Rel_Impl(renderTexture);

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void R(RenderTexture renderTexture)
        {
            Rel(renderTexture);
        }

        public static void ForceLeakedRelease()
        {
            foreach (var rt in s_tempSet)
            {
#if TTT_DEBUG_TEMP_RT_TRACE
                Debug.Log("ForceReleased-" + rt.name);
#endif
                Rel_Impl(rt);
            }
            s_tempSet.Clear();
#if TTT_DEBUG_TEMP_RT_TRACE
            Debug.Log("MaxSomeTimeTempRtCount" + s_tempSomeTimeMaxCount);
            s_tempSomeTimeMaxCount = 0;
            Debug.Log("ConfirmedTempRtCount" + s_tempHashSet.Count);
            s_tempHashSet.Clear();
            Debug.Log("TotalGetCount " + s_totalGetCount);
            s_totalGetCount = 0;
#endif
        }

        [TexTransInitialize]
        static void RegisterForceLakedResolve()
        {
#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += ForceLeakedRelease;
#endif
            TexTransCoreRuntime.Update += ReleaseUpdate;
        }

        public readonly struct UsingRenderTexture : IDisposable
        {
            readonly RenderTexture _rt;
            public UsingRenderTexture(RenderTexture rt) { _rt = rt; }
            public void Dispose() { R(_rt); }
        }
    }
}
