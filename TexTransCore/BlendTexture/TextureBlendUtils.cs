using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using System;
using net.rs64.TexTransCore.TransTextureCore.TransCompute;
using net.rs64.TexTransCore.TransTextureCore.Utils;

namespace net.rs64.TexTransCore.BlendTexture
{
    public enum BlendType
    {
        Normal,
        Mul,
        Screen,
        Overlay,
        HardLight,
        SoftLight,
        ColorDodge,
        ColorBurn,
        LinearBurn,
        VividLight,
        LinearLight,
        Divide,
        Addition,
        Subtract,
        Difference,
        DarkenOnly,
        LightenOnly,
        Hue,
        Saturation,
        Color,
        Luminosity,
        NotBlend,
    }
    public static class TextureBlendUtils
    {
        public const string BLEND_TEX_SHADER = "Hidden/BlendTexture";
        public const string COLOR_MUL_SHADER = "Hidden/BlendTexture";
        public const string MASK_SHADER = "Hidden/MaskShader";
        public const string UNLIT_COLOR_ALPHA_SHADER = "Hidden/UnlitColorAndAlpha";
        public static void BlendBlit(this RenderTexture Base, Texture Add, BlendType blendType, bool keepAlpha = false)
        {
            var material = new Material(Shader.Find(BLEND_TEX_SHADER));
            var swap = RenderTexture.GetTemporary(Base.descriptor);
            Graphics.CopyTexture(Base, swap);
            material.SetTexture("_DistTex", swap);
            material.EnableKeyword(blendType.ToString());
            if (keepAlpha) { material.EnableKeyword("KeepAlpha"); }

            Graphics.Blit(Add, Base, material);
            RenderTexture.ReleaseTemporary(swap);
        }
        public static void BlendBlit(this RenderTexture Base, IEnumerable<BlendTextures> Adds)
        {
            var material = new Material(Shader.Find(BLEND_TEX_SHADER));
            var temRt = RenderTexture.GetTemporary(Base.descriptor);
            var swap = Base;
            var target = temRt;
            Graphics.Blit(swap, target);

            foreach (var Add in Adds)
            {
                material.SetTexture("_DistTex", swap);
                material.shaderKeywords = new string[] { Add.BlendType.ToString() };
                Graphics.Blit(Add.Texture, target, material);
                (swap, target) = (target, swap);
            }

            if (swap != Base)
            {
                Graphics.Blit(swap, Base);
            }
            RenderTexture.ReleaseTemporary(temRt);
        }
        public static RenderTexture BlendBlit(Texture2D Base, Texture Add, BlendType blendType)
        {
            var renderTexture = new RenderTexture(Base.width, Base.height, 32, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB);
            Graphics.Blit(Base, renderTexture);

            var material = new Material(Shader.Find(BLEND_TEX_SHADER));
            material.SetTexture("_DistTex", Base);
            material.EnableKeyword(blendType.ToString());

            Graphics.Blit(Add, renderTexture, material);

            return renderTexture;
        }
        public struct BlendTextures
        {
            public Texture Texture;
            public BlendType BlendType;

            public BlendTextures(Texture texture, BlendType blendType)
            {
                Texture = texture;
                BlendType = blendType;
            }
        }
        public static Texture2D ResizeTexture(Texture2D Souse, Vector2Int Size)
        {
            var resizedTexture = new Texture2D(Size.x, Size.y, Souse.graphicsFormat, Souse.mipmapCount > 1 ? UnityEngine.Experimental.Rendering.TextureCreationFlags.MipChain : UnityEngine.Experimental.Rendering.TextureCreationFlags.None);

            var pixels = new Color[Size.x * Size.y];

            foreach (var Index in Enumerable.Range(0, pixels.Length))
            {
                pixels[Index] = GetColorOnTexture(Souse, Index, Size);
            }

            resizedTexture.SetPixels(pixels);
            resizedTexture.Apply();
            resizedTexture.name = Souse.name + "_Resized_" + Size.x.ToString();

            return resizedTexture;
        }
        public static Color GetColorOnTexture(Texture2D Texture, int Index, Vector2Int SouseSize)
        {
            var pos = DimensionIndexUtility.ConvertIndex2D(Index, SouseSize.x);
            return Texture.GetPixelBilinear(pos.x / (float)SouseSize.x, pos.y / (float)SouseSize.y);
        }

        public static RenderTexture CreateMultipliedRenderTexture(Texture MainTex, Color Color)
        {
            var mainTexRt = new RenderTexture(MainTex.width, MainTex.height, 0);
            MultipleRenderTexture(mainTexRt, MainTex, Color);
            return mainTexRt;
        }
        public static void MultipleRenderTexture(RenderTexture MainTexRt, Texture MainTex, Color Color)
        {
            var mat = new Material(Shader.Find(COLOR_MUL_SHADER));
            mat.SetColor("_Color", Color);
            Graphics.Blit(MainTex, MainTexRt, mat);
        }
        public static void MultipleRenderTexture(RenderTexture renderTexture, Color Color)
        {
            var tempRt = RenderTexture.GetTemporary(renderTexture.descriptor);
            var mat = new Material(Shader.Find(COLOR_MUL_SHADER));
            mat.SetColor("_Color", Color);
            Graphics.CopyTexture(renderTexture, tempRt);
            Graphics.Blit(tempRt, renderTexture, mat);
            RenderTexture.ReleaseTemporary(tempRt);
        }
        public static void MaskDrawRenderTexture(RenderTexture renderTexture, Texture MaskTex)
        {
            var tempRt = RenderTexture.GetTemporary(renderTexture.descriptor);
            var mat = new Material(Shader.Find(MASK_SHADER));
            mat.SetTexture("_MaskTex", MaskTex);
            Graphics.CopyTexture(renderTexture, tempRt);
            Graphics.Blit(tempRt, renderTexture, mat);
            RenderTexture.ReleaseTemporary(tempRt);
        }


        public static Texture2D CreateColorTex(Color Color)
        {
            var mainTex2d = new Texture2D(1, 1);
            mainTex2d.SetPixel(0, 0, Color);
            mainTex2d.Apply();
            return mainTex2d;
        }

        public static void ColorBlit(RenderTexture mulDecalTexture, Color Color)
        {
            var unlitMat = new Material(Shader.Find(UNLIT_COLOR_ALPHA_SHADER));
            unlitMat.SetColor("_Color", Color);
            Graphics.Blit(null, mulDecalTexture, unlitMat);
        }
    }

}
