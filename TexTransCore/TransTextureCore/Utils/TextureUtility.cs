using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace net.rs64.TexTransCore.TransTextureCore.Utils
{
    public static class TextureUtility
    {
        public static Texture2D CopyTexture2D(this RenderTexture Rt, TextureFormat? OverrideFormat = null, bool? OverrideUseMip = null)
        {
            var preRt = RenderTexture.active;
            try
            {
                RenderTexture.active = Rt;
                var useMip = OverrideUseMip.HasValue ? OverrideUseMip.Value : Rt.useMipMap;
                var texture = OverrideFormat.HasValue ? new Texture2D(Rt.width, Rt.height, OverrideFormat.Value, useMip) : new Texture2D(Rt.width, Rt.height, Rt.graphicsFormat, useMip ? UnityEngine.Experimental.Rendering.TextureCreationFlags.MipChain : UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
                texture.ReadPixels(new Rect(0, 0, Rt.width, Rt.height), 0, 0);
                texture.Apply();
                texture.name = Rt.name + "_CopyTex2D";
                return texture;
            }
            finally
            {
                RenderTexture.active = preRt;
            }
        }

        public static void Clear(this RenderTexture Rt)
        {
            var preRt = RenderTexture.active;
            try
            {
                RenderTexture.active = Rt;
                GL.Clear(true, true, Color.clear);
            }
            finally
            {
                RenderTexture.active = preRt;
            }

        }
    }
}