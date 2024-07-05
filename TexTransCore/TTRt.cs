using System;
using System.Collections.Generic;
using net.rs64.TexTransCore.Utils;
using UnityEngine;

namespace net.rs64.TexTransCore
{
    internal static class TTRt
    {
        public static RenderTextureFormat RenderTextureDefaultFormat = RenderTextureFormat.ARGB32;

        public static RenderTexture G(int size) => G(size, size);
        public static RenderTexture G(int size, string rtName)
        {
            var rt = G(size, size);
            rt.name = rtName;
            return rt;
        }
        public static UsingRenderTexture U(out RenderTexture tmpRt, int size)
        {
            tmpRt = G(size);
            return new(tmpRt);
        }
        public static RenderTexture G(int width, int height, bool clearRt = false, bool useDepthAndStencil = false, bool useMipMap = false, bool useRandomRW = false, RenderTextureFormat? rtFormat = null)
        {
            var depth = useDepthAndStencil ? 32 : 0;
            var format = rtFormat ?? RenderTextureDefaultFormat;

            var tmpRt = RenderTexture.GetTemporary(width, height, depth, format);
            s_tempSet.Add(tmpRt);

            if (tmpRt.useMipMap != useMipMap)
            {
                if (tmpRt.IsCreated()) { tmpRt.Release(); }
                tmpRt.useMipMap = useMipMap;
                tmpRt.autoGenerateMips = false;
            }

            if (useRandomRW && tmpRt.enableRandomWrite != useRandomRW)
            {
                if (tmpRt.IsCreated()) { tmpRt.Release(); }
                tmpRt.enableRandomWrite = useRandomRW;
            }

            if (clearRt) tmpRt.Clear();

            return tmpRt;
        }
        public static UsingRenderTexture U(out RenderTexture tmpRt, int width, int height, bool clearRt = false, bool useDepthAndStencil = false, bool useMipMap = false, bool useRandomRW = false, RenderTextureFormat? rtFormat = null)
        {
            tmpRt = G(width, height, clearRt, useDepthAndStencil, useMipMap, useRandomRW, rtFormat);
            return new(tmpRt);
        }
        public static RenderTexture G(RenderTexture rt, bool copyTexture = false)
        {
            var newRt = RenderTexture.GetTemporary(rt.descriptor);
            s_tempSet.Add(newRt);
            newRt.CopyFilWrap(rt);
            if (copyTexture) Graphics.CopyTexture(rt, newRt);
            return newRt;
        }
        public static UsingRenderTexture U(out RenderTexture tmpRt, RenderTexture rt, bool copyTexture = false)
        {
            tmpRt = G(rt, copyTexture); return new(tmpRt);
        }
        public static RenderTexture G(RenderTextureDescriptor renderTextureDescriptor, bool clearRt = false)
        {
            var rt = RenderTexture.GetTemporary(renderTextureDescriptor);
            s_tempSet.Add(rt);
            if (clearRt) rt.Clear();
            return rt;
        }
        public static UsingRenderTexture U(out RenderTexture tmpRt, RenderTextureDescriptor renderTextureDescriptor, bool clearRt = false)
        {
            tmpRt = G(renderTextureDescriptor, clearRt); return new(tmpRt);
        }


        internal static HashSet<RenderTexture> s_tempSet = new();

        public static void R(RenderTexture renderTexture)
        {
            RenderTexture.ReleaseTemporary(renderTexture);
            s_tempSet.Remove(renderTexture);
        }

        public static void ForceLeakedRelease()
        {
            foreach (var rt in s_tempSet)
            {
                Debug.Log("ForceReleased-" + rt.name);
                RenderTexture.ReleaseTemporary(rt);
            }
            s_tempSet.Clear();
        }

#if UNITY_EDITOR
        [TexTransInitialize]
        static void RegisterForceLakedResolve()
        {
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += ForceLeakedRelease;
        }
#endif

        public readonly struct UsingRenderTexture : IDisposable
        {
            readonly RenderTexture _rt;
            public UsingRenderTexture(RenderTexture rt) { _rt = rt; }
            public void Dispose() { R(_rt); }
        }
    }
}
