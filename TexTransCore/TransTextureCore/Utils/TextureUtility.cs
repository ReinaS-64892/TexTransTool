using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.BlendTexture;
using UnityEngine;

namespace net.rs64.TexTransCore.TransTextureCore.Utils
{
    internal static class TextureUtility
    {
        public static Texture2D CopyTexture2D(this RenderTexture Rt, TextureFormat? OverrideFormat = null, bool? OverrideUseMip = null)
        {

            using (new RTActiveSaver())
            {
                RenderTexture.active = Rt;
                var useMip = OverrideUseMip.HasValue ? OverrideUseMip.Value : Rt.useMipMap;
                var texture = OverrideFormat.HasValue ? new Texture2D(Rt.width, Rt.height, OverrideFormat.Value, useMip) : new Texture2D(Rt.width, Rt.height, Rt.graphicsFormat, useMip ? UnityEngine.Experimental.Rendering.TextureCreationFlags.MipChain : UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
                texture.ReadPixels(new Rect(0, 0, Rt.width, Rt.height), 0, 0);
                texture.Apply();
                texture.name = Rt.name + "_CopyTex2D";
                return texture;
            }
        }

        public static void Clear(this RenderTexture Rt)
        {
            using (new RTActiveSaver())
            {
                RenderTexture.active = Rt;
                GL.Clear(true, true, Color.clear);
            }

        }


        public static Texture2D ResizeTexture(Texture2D Souse, Vector2Int Size)
        {
            using (new RTActiveSaver())
            {
                var useMip = Souse.mipmapCount > 1;
                var rt = RenderTexture.GetTemporary(Size.x, Size.y);
                if (useMip)
                {
                    Graphics.Blit(Souse, rt);
                }
                else
                {
                    var mipRt = RenderTexture.GetTemporary(Souse.width, Souse.height);
                    mipRt.Release();
                    var preValue = (mipRt.useMipMap, mipRt.autoGenerateMips);

                    mipRt.useMipMap = true;
                    mipRt.autoGenerateMips = false;

                    Graphics.Blit(Souse, mipRt);
                    mipRt.GenerateMips();
                    Graphics.Blit(mipRt, rt);

                    mipRt.Release();
                    (mipRt.useMipMap, mipRt.autoGenerateMips) = preValue;
                    RenderTexture.ReleaseTemporary(mipRt);
                }

                var resizedTexture = rt.CopyTexture2D(OverrideUseMip: useMip);
                resizedTexture.name = Souse.name + "_Resized_" + Size.x.ToString();

                RenderTexture.ReleaseTemporary(rt);
                return resizedTexture;
            }
        }


        public static Texture2D CreateColorTex(Color Color)
        {
            var mainTex2d = new Texture2D(1, 1);
            mainTex2d.SetPixel(0, 0, Color);
            mainTex2d.Apply();
            return mainTex2d;
        }

    }
}