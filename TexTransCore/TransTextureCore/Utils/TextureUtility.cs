using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.BlendTexture;
using UnityEngine;

namespace net.rs64.TexTransCore.TransTextureCore.Utils
{
    internal static class TextureUtility
    {
        public static Texture2D CopyTexture2D(this RenderTexture rt, TextureFormat? overrideFormat = null, bool? overrideUseMip = null)
        {

            using (new RTActiveSaver())
            {
                RenderTexture.active = rt;
                var useMip = overrideUseMip.HasValue ? overrideUseMip.Value : rt.useMipMap;
                var texture = overrideFormat.HasValue ? new Texture2D(rt.width, rt.height, overrideFormat.Value, useMip) : new Texture2D(rt.width, rt.height, rt.graphicsFormat, useMip ? UnityEngine.Experimental.Rendering.TextureCreationFlags.MipChain : UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
                texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                texture.Apply();
                texture.name = rt.name + "_CopyTex2D";
                return texture;
            }
        }

        public static void Clear(this RenderTexture rt)
        {
            using (new RTActiveSaver())
            {
                RenderTexture.active = rt;
                GL.Clear(true, true, Color.clear);
            }

        }


        public static Texture2D ResizeTexture(Texture2D souse, Vector2Int size)
        {
            using (new RTActiveSaver())
            {
                var useMip = souse.mipmapCount > 1;
                var rt = RenderTexture.GetTemporary(size.x, size.y);
                if (useMip)
                {
                    Graphics.Blit(souse, rt);
                }
                else
                {
                    var mipRt = RenderTexture.GetTemporary(souse.width, souse.height);
                    mipRt.Release();
                    var preValue = (mipRt.useMipMap, mipRt.autoGenerateMips);

                    mipRt.useMipMap = true;
                    mipRt.autoGenerateMips = false;

                    Graphics.Blit(souse, mipRt);
                    mipRt.GenerateMips();
                    Graphics.Blit(mipRt, rt);

                    mipRt.Release();
                    (mipRt.useMipMap, mipRt.autoGenerateMips) = preValue;
                    RenderTexture.ReleaseTemporary(mipRt);
                }

                var resizedTexture = rt.CopyTexture2D(overrideUseMip: useMip);
                resizedTexture.name = souse.name + "_Resized_" + size.x.ToString();

                RenderTexture.ReleaseTemporary(rt);
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

    }
}