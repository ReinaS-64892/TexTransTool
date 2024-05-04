using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using net.rs64.TexTransCore.BlendTexture;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace net.rs64.TexTransCore.Utils
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

        public static void Clear(this RenderTexture rt)
        {
            using (new RTActiveSaver())
            {
                RenderTexture.active = rt;
                GL.Clear(true, true, Color.clear);
            }

        }


        public static Texture2D ResizeTexture(Texture2D source, Vector2Int size)
        {
            Profiler.BeginSample("ResizeTexture");
            using (new RTActiveSaver())
            {
                var useMip = source.mipmapCount > 1;
                var rt = RenderTexture.GetTemporary(size.x, size.y); rt.Clear();
                if (useMip)
                {
                    Graphics.Blit(source, rt);
                }
                else
                {
                    var mipRt = RenderTexture.GetTemporary(source.width, source.height);
                    mipRt.Release();
                    var preValue = (mipRt.useMipMap, mipRt.autoGenerateMips);

                    mipRt.useMipMap = true;
                    mipRt.autoGenerateMips = false;

                    Graphics.Blit(source, mipRt);
                    mipRt.GenerateMips();
                    Graphics.Blit(mipRt, rt);

                    mipRt.Release();
                    (mipRt.useMipMap, mipRt.autoGenerateMips) = preValue;
                    RenderTexture.ReleaseTemporary(mipRt);
                }

                var resizedTexture = rt.CopyTexture2D(overrideUseMip: useMip);
                resizedTexture.name = source.name + "_Resized_" + size.x.ToString();

                RenderTexture.ReleaseTemporary(rt);
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
            var rt = RenderTexture.GetTemporary(1, 1, 0);
            TextureBlend.ColorBlit(rt, color);
            return rt;
        }
        public static Texture2D CreateFillTexture(int size, Color fillColor)
        {
            return CreateFillTexture(new Vector2Int(size, size), fillColor);
        }
        public static Texture2D CreateFillTexture(Vector2Int size, Color fillColor)
        {
            var TestTex = new Texture2D(size.x, size.y);
            TestTex.SetPixels(CollectionsUtility.FilledArray(fillColor, size.x * size.y));
            return TestTex;
        }

        public static int NormalizePowerOfTwo(int v) => Mathf.IsPowerOfTwo(v) ? v : Mathf.NextPowerOfTwo(v);

        public static RenderTexture CloneTemp(this RenderTexture renderTexture)
        {
            var newTemp = RenderTexture.GetTemporary(renderTexture.descriptor);
            newTemp.CopyFilWrap(renderTexture);
            Graphics.CopyTexture(renderTexture, newTemp);
            return newTemp;
        }

        internal static void CopyFilWrap(this Texture t, Texture s)
        {
            t.filterMode = s.filterMode;
            t.wrapMode = s.wrapMode;
            t.wrapModeU = s.wrapModeU;
            t.wrapModeV = s.wrapModeV;
            t.wrapModeW = s.wrapModeW;
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
            ApplyTextureST(tmp, s, t, rt);
            RenderTexture.ReleaseTemporary(tmp);
        }


    }
}
