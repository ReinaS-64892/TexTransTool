using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Color = UnityEngine.Color;

namespace net.rs64.TexTransTool.Utils
{
    internal static class TextureUtility
    {
        public static void FillColor(RenderTexture rt, Color color)
        {
            var cs = ((TTGeneralComputeOperator)ComputeObjectUtility.UStdHolder.ColorFill).Compute;
            cs.SetFloats("Color", color.r, color.g, color.b, color.a);
            cs.SetTexture(0, "Tex", rt);
            cs.Dispatch(0, Mathf.Max(1, rt.width / 32), Mathf.Max(1, rt.height / 32), 1);
        }
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


        public static Texture2D ResizeTexture(Texture2D source, Vector2Int size, bool forceRegenerateMipMap = false)
        {
            throw new NotImplementedException();
            // Profiler.BeginSample("ResizeTexture");
            // using (new RTActiveSaver())
            // {
            //     var useMip = source.mipmapCount > 1;
            //     if (forceRegenerateMipMap) { useMip = false; }

            //     var rt = TTRt.G(size.x, size.y, true);
            //     if (useMip)
            //     {
            //         Graphics.Blit(source, rt);
            //     }
            //     else
            //     {
            //         var mipRt = TTRt.G(source.width, source.height, false, false, true, true);

            //         Graphics.Blit(source, mipRt);
            //         MipMapUtility.GenerateMips(mipRt, DownScalingAlgorithm.Average);
            //         Graphics.Blit(mipRt, rt);

            //         TTRt.R(mipRt);
            //     }

            //     var resizedTexture = rt.CopyTexture2D(overrideUseMip: useMip);
            //     resizedTexture.name = source.name + "_Resized_" + size.x.ToString();

            //     TTRt.R(rt);
            //     Profiler.EndSample();

            //     return resizedTexture;
            // }
        }


        public static int NormalizePowerOfTwo(int v) => Mathf.IsPowerOfTwo(v) ? v : Mathf.NextPowerOfTwo(v);

        // public static RenderTexture CloneTemp(this RenderTexture renderTexture)
        // {
        //     return TTRt.G(renderTexture, true);
        // }

        internal static void CopyFilWrap2D(this Texture2D tex, Texture2D copySource)
        {
            CopyFilWrap(tex, copySource);
#if UNITY_EDITOR
            tex.alphaIsTransparency = copySource.alphaIsTransparency;
#endif
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

        // public static void ApplyTextureST(Texture source, Vector2 s, Vector2 t, RenderTexture write)
        // {
        //     using (new RTActiveSaver())
        //     {
        //         if (s_TempMat == null) { s_TempMat = new Material(s_stApplyShader); }
        //         s_TempMat.shader = s_stApplyShader;

        //         s_TempMat.SetTexture("_OffSetTex", source);
        //         s_TempMat.SetTextureScale("_OffSetTex", s);
        //         s_TempMat.SetTextureOffset("_OffSetTex", t);

        //         Graphics.Blit(null, write, s_TempMat);
        //     }
        // }
        // public static void ApplyTextureST(this RenderTexture rt, Vector2 s, Vector2 t)
        // {
        //     var tmp = rt.CloneTemp();
        //     if (tmp.useMipMap) { MipMapUtility.GenerateMips(tmp, DownScalingAlgorithm.Average); }
        //     ApplyTextureST(tmp, s, t, rt);
        //     TTRt.R(tmp);
        // }


    }
}
