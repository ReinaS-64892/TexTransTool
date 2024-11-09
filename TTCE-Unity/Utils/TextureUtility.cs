using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity.MipMap;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Color = UnityEngine.Color;

namespace net.rs64.TexTransCoreEngineForUnity.Utils
{
    internal static class TextureUtility
    {
        public static Texture2D CopyTexture2D(this RenderTexture rt, TextureFormat? overrideFormat = null, bool? overrideUseMip = null)
        {
            var useMip = overrideUseMip ?? rt.useMipMap;
            var format = overrideFormat ?? GraphicsFormatUtility.GetTextureFormat(rt.graphicsFormat);
            var readMapCount = rt.useMipMap && useMip ? rt.mipmapCount : 1;

            Span<AsyncGPUReadbackRequest> asyncGPUReadbackRequests = stackalloc AsyncGPUReadbackRequest[readMapCount];
            for (var i = 0; readMapCount > i; i += 1)
            {
                asyncGPUReadbackRequests[i] = AsyncGPUReadback.Request(rt, i);
            }


            var texture = new Texture2D(rt.width, rt.height, format, useMip, !rt.sRGB);
            texture.name = rt.name + "_CopyTex2D";

            if (rt.useMipMap && useMip)
            {
                for (var layer = 0; readMapCount > layer; layer += 1)
                {
                    asyncGPUReadbackRequests[layer].WaitForCompletion();
                    using (var data = asyncGPUReadbackRequests[layer].GetData<Color32>())
                    {
                        texture.SetPixelData(data, layer);
                    }
                }
                texture.Apply(false);
            }
            else
            {
                asyncGPUReadbackRequests[0].WaitForCompletion();
                using (var data = asyncGPUReadbackRequests[0].GetData<Color32>())
                {
                    texture.SetPixelData(data, 0);
                }
                texture.Apply(true);
            }



            return texture;
        }
        public static void DownloadFromRenderTexture<T>(this RenderTexture rt, Span<T> dataSpan) where T : unmanaged
        {
            var (format, channel) = rt.graphicsFormat.ToTTCTextureFormat();
            if (EnginUtil.GetPixelParByte(format, channel) * rt.width * rt.height != dataSpan.Length) { throw new ArgumentException(); }

            var request = AsyncGPUReadback.Request(rt,0);
            request.WaitForCompletion();
            request.GetData<T>().AsSpan().CopyTo(dataSpan);
        }

        public static void Clear(this RenderTexture rt)
        {
            using (new RTActiveSaver())
            {
                RenderTexture.active = rt;
                GL.Clear(true, true, Color.clear);
            }
        }
        public static void ClearWithColor(this RenderTexture rt, Color color)
        {
            using (new RTActiveSaver())
            {
                RenderTexture.active = rt;
                GL.Clear(true, true, color);
            }
        }


        public static Texture2D ResizeTexture(Texture2D source, Vector2Int size, bool forceRegenerateMipMap = false)
        {
            Profiler.BeginSample("ResizeTexture");
            using (new RTActiveSaver())
            {
                var useMip = source.mipmapCount > 1;
                if (forceRegenerateMipMap) { useMip = false; }

                var rt = TTRt.G(size.x, size.y, true);
                if (useMip)
                {
                    Graphics.Blit(source, rt);
                }
                else
                {
                    var mipRt = TTRt.G(source.width, source.height, false, false, true, true);

                    mipRt.useMipMap = true;
                    mipRt.autoGenerateMips = false;

                    Graphics.Blit(source, mipRt);
                    MipMapUtility.GenerateMips(mipRt, DownScalingAlgorithm.Average);
                    Graphics.Blit(mipRt, rt);

                    TTRt.R(mipRt);
                }

                var resizedTexture = rt.CopyTexture2D(overrideUseMip: useMip);
                resizedTexture.name = source.name + "_Resized_" + size.x.ToString();

                TTRt.R(rt);
                Profiler.EndSample();

                return resizedTexture;
            }
        }


        public static Texture2D CreateColorTex(Color color)
        {
            var mainTex2d = new Texture2D(1, 1);
            mainTex2d.SetPixel(0, 0, color);
            mainTex2d.Apply();
            return mainTex2d;
        }
        public static RenderTexture CreateColorTexForRT(Color color)
        {
            var rt = TTRt.G(1);
            rt.name = $"ColorTex4RT-{rt.width}x{rt.height}";
            TextureBlend.FillColor(rt, color);
            return rt;
        }
        public static Texture2D CreateFillTexture(int size, Color fillColor)
        {
            return CreateFillTexture(new Vector2Int(size, size), fillColor);
        }
        public static Texture2D CreateFillTexture(Vector2Int size, Color fillColor)
        {
            var newTex = new Texture2D(size.x, size.y, TextureFormat.RGBA32, true);
            var na = new NativeArray<Color32>(size.x * size.y, Allocator.Temp);
            na.AsSpan().Fill(fillColor);
            newTex.SetPixelData(na, 0);
            return newTex;
        }

        public static int NormalizePowerOfTwo(int v) => Mathf.IsPowerOfTwo(v) ? v : Mathf.NextPowerOfTwo(v);

        public static RenderTexture CloneTemp(this RenderTexture renderTexture)
        {
            return TTRt.G(renderTexture, true);
        }

        internal static void CopyFilWrap2D(this Texture2D tex, Texture2D copySource)
        {
            CopyFilWrap(tex,copySource);
            tex.alphaIsTransparency = copySource.alphaIsTransparency;
            tex.requestedMipmapLevel = copySource.requestedMipmapLevel;
        }
        internal static void CopyFilWrap(this Texture tex, Texture copySource)
        {
            tex.filterMode = copySource.filterMode;
            tex.anisoLevel = copySource.anisoLevel;
            tex.mipMapBias = copySource.mipMapBias;
            tex.wrapModeU = copySource.wrapModeU;
            tex.wrapModeV = copySource.wrapModeV;
            tex.wrapMode = copySource.wrapMode;
        }

        public const string ST_APPLY_SHADER = "Hidden/TextureSTApply";
        static Shader s_stApplyShader;

        [TexTransInitialize]
        public static void Init() { s_stApplyShader = Shader.Find(ST_APPLY_SHADER); }

        static Material s_TempMat;

        public static void ApplyTextureST(Texture source, Vector2 s, Vector2 t, RenderTexture write)
        {
            using (new RTActiveSaver())
            {
                if (s_TempMat == null) { s_TempMat = new Material(s_stApplyShader); }
                s_TempMat.shader = s_stApplyShader;

                s_TempMat.SetTexture("_OffSetTex", source);
                s_TempMat.SetTextureScale("_OffSetTex", s);
                s_TempMat.SetTextureOffset("_OffSetTex", t);

                Graphics.Blit(null, write, s_TempMat);
            }
        }
        public static void ApplyTextureST(this RenderTexture rt, Vector2 s, Vector2 t)
        {
            var tmp = rt.CloneTemp();
            if (tmp.useMipMap) { MipMapUtility.GenerateMips(tmp, DownScalingAlgorithm.Average); }
            ApplyTextureST(tmp, s, t, rt);
            TTRt.R(tmp);
        }


    }
}
