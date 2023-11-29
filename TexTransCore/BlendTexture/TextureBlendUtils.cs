using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using System;
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
        public const string COLOR_MUL_SHADER = "Hidden/ColorMulShader";
        public const string MASK_SHADER = "Hidden/MaskShader";
        public const string UNLIT_COLOR_ALPHA_SHADER = "Hidden/UnlitColorAndAlpha";
        public static void BlendBlit(this RenderTexture Base, Texture Add, BlendType blendType, bool keepAlpha = false)
        {
            using (new RTActiveSaver())
            {
                var material = new Material(Shader.Find(BLEND_TEX_SHADER));
                var swap = RenderTexture.GetTemporary(Base.descriptor);
                Graphics.CopyTexture(Base, swap);
                material.SetTexture("_DistTex", swap);
                material.EnableKeyword(blendType.ToString());
                if (keepAlpha) { material.EnableKeyword("KeepAlpha"); }

                Graphics.Blit(Add, Base, material);
                RenderTexture.ReleaseTemporary(swap);
                UnityEngine.Object.DestroyImmediate(material);
            }
        }
        public static void BlendBlit(this RenderTexture Base, IEnumerable<BlendTexturePair> Adds)
        {
            using (new RTActiveSaver())
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
                UnityEngine.Object.DestroyImmediate(material);
            }
        }
        public static RenderTexture BlendBlit(Texture2D Base, Texture Add, BlendType blendType)
        {
            using (new RTActiveSaver())
            {
                var renderTexture = new RenderTexture(Base.width, Base.height, 32, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB);
                Graphics.Blit(Base, renderTexture);

                var material = new Material(Shader.Find(BLEND_TEX_SHADER));
                material.SetTexture("_DistTex", Base);
                material.EnableKeyword(blendType.ToString());

                Graphics.Blit(Add, renderTexture, material);

                UnityEngine.Object.DestroyImmediate(material);
                return renderTexture;
            }
        }
        public struct BlendTexturePair
        {
            public Texture Texture;
            public BlendType BlendType;

            public BlendTexturePair(Texture texture, BlendType blendType)
            {
                Texture = texture;
                BlendType = blendType;
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
        public static RenderTexture CreateMultipliedRenderTexture(Texture MainTex, Color Color)
        {
            var mainTexRt = new RenderTexture(MainTex.width, MainTex.height, 0);
            MultipleRenderTexture(mainTexRt, MainTex, Color);
            return mainTexRt;
        }
        public static void MultipleRenderTexture(RenderTexture MainTexRt, Texture MainTex, Color Color)
        {
            using (new RTActiveSaver())
            {
                var mat = new Material(Shader.Find(COLOR_MUL_SHADER));
                mat.SetColor("_Color", Color);
                Graphics.Blit(MainTex, MainTexRt, mat);
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }
        public static void MultipleRenderTexture(RenderTexture renderTexture, Color Color)
        {
            using (new RTActiveSaver())
            {
                var tempRt = RenderTexture.GetTemporary(renderTexture.descriptor);
                var mat = new Material(Shader.Find(COLOR_MUL_SHADER));
                mat.SetColor("_Color", Color);
                Graphics.CopyTexture(renderTexture, tempRt);
                Graphics.Blit(tempRt, renderTexture, mat);
                RenderTexture.ReleaseTemporary(tempRt);
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }
        public static void MaskDrawRenderTexture(RenderTexture renderTexture, Texture MaskTex)
        {
            using (new RTActiveSaver())
            {
                var tempRt = RenderTexture.GetTemporary(renderTexture.descriptor);
                var mat = new Material(Shader.Find(MASK_SHADER));
                mat.SetTexture("_MaskTex", MaskTex);
                Graphics.CopyTexture(renderTexture, tempRt);
                Graphics.Blit(tempRt, renderTexture, mat);
                RenderTexture.ReleaseTemporary(tempRt);
                UnityEngine.Object.DestroyImmediate(mat);
            }
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
            using (new RTActiveSaver())
            {
                var unlitMat = new Material(Shader.Find(UNLIT_COLOR_ALPHA_SHADER));
                unlitMat.SetColor("_Color", Color);
                Graphics.Blit(null, mulDecalTexture, unlitMat);
                UnityEngine.Object.DestroyImmediate(unlitMat);
            }
        }
    }

    public struct RTActiveSaver : IDisposable
    {
        readonly RenderTexture PreRT;
        public RTActiveSaver(bool empty = false)
        {
            PreRT = RenderTexture.active;
        }
        public void Dispose()
        {
            RenderTexture.active = PreRT;
        }
    }

}
