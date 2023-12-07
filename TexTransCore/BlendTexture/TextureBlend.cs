using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using System;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEditor;

namespace net.rs64.TexTransCore.BlendTexture
{
    [Obsolete("Replaced with BlendTypeKey", true)]
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
        AlphaLerp,
        NotBlend,
    }
    public enum TTTBlendTypeKeyEnum
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
    public static class TextureBlend
    {
        public static Dictionary<string, Shader> BlendShaders;
        [InitializeOnLoadMethod]
        public static void BlendShadersInit()
        {
            var TTTBlendShader = BlendTexShader;
            BlendShaders = new Dictionary<string, Shader>()
            {
                {"Normal",TTTBlendShader},
                {"Mul",TTTBlendShader},
                {"Screen",TTTBlendShader},
                {"Overlay",TTTBlendShader},
                {"HardLight",TTTBlendShader},
                {"SoftLight",TTTBlendShader},
                {"ColorDodge",TTTBlendShader},
                {"ColorBurn",TTTBlendShader},
                {"LinearBurn",TTTBlendShader},
                {"VividLight",TTTBlendShader},
                {"LinearLight",TTTBlendShader},
                {"Divide",TTTBlendShader},
                {"Addition",TTTBlendShader},
                {"Subtract",TTTBlendShader},
                {"Difference",TTTBlendShader},
                {"DarkenOnly",TTTBlendShader},
                {"LightenOnly",TTTBlendShader},
                {"Hue",TTTBlendShader},
                {"Saturation",TTTBlendShader},
                {"Color",TTTBlendShader},
                {"Luminosity",TTTBlendShader},
                {"NotBlend",TTTBlendShader},
            };

            var extensions = InterfaceUtility.GetInterfaceInstance<TexBlendExtension>();
            foreach (var ext in extensions)
            {
                var (Keywords, shader) = ext.GetExtensionBlender();
                foreach (var Keyword in Keywords)
                {
                    if (BlendShaders.ContainsKey(Keyword))
                    {
                        Debug.LogWarning($"TexBlendExtension : {ext.GetType().FullName} {Keyword} is Contained!!!");
                    }
                    else
                    {
                        BlendShaders[Keyword] = shader;
                    }
                }
            }
        }
        public const string BL_KEY_DEFAULT = "Normal";
        public const string BLEND_TEX_SHADER = "Hidden/BlendTexture";
        public static Shader BlendTexShader = Shader.Find(BLEND_TEX_SHADER);
        public const string COLOR_MUL_SHADER = "Hidden/ColorMulShader";
        public static Shader ColorMulShader = Shader.Find(COLOR_MUL_SHADER);
        public const string MASK_SHADER = "Hidden/MaskShader";
        public static Shader MaskShader = Shader.Find(MASK_SHADER);
        public const string UNLIT_COLOR_ALPHA_SHADER = "Hidden/UnlitColorAndAlpha";
        public static Shader UnlitColorAlphaShader = Shader.Find(UNLIT_COLOR_ALPHA_SHADER);
        public const string ALPHA_COPY_SHADER = "Hidden/AlphaCopy";
        public static Shader AlphaCopyShader = Shader.Find(ALPHA_COPY_SHADER);
        public static void BlendBlit(this RenderTexture Base, Texture Add, string blendTypeKey, bool keepAlpha = false)
        {
            using (new RTActiveSaver())
            {
                var material = new Material(BlendShaders[blendTypeKey]);
                var swap = RenderTexture.GetTemporary(Base.descriptor);
                Graphics.CopyTexture(Base, swap);
                material.SetTexture("_DistTex", swap);
                material.EnableKeyword(blendTypeKey);

                Graphics.Blit(Add, Base, material);

                if (keepAlpha)
                {
                    var alphaCopyMat = new Material(AlphaCopyShader);
                    var baseSwap = RenderTexture.GetTemporary(Base.descriptor);

                    alphaCopyMat.SetTexture("_AlphaTex", swap);
                    Graphics.CopyTexture(Base, baseSwap);
                    Graphics.Blit(baseSwap, Base, alphaCopyMat);

                    RenderTexture.ReleaseTemporary(baseSwap);
                    UnityEngine.Object.DestroyImmediate(alphaCopyMat);
                }

                RenderTexture.ReleaseTemporary(swap);
                UnityEngine.Object.DestroyImmediate(material);
            }
        }
        public static void BlendBlit(this RenderTexture Base, IEnumerable<BlendTexturePair> Adds)
        {
            using (new RTActiveSaver())
            {
                var material = new Material(BlendTexShader);
                var temRt = RenderTexture.GetTemporary(Base.descriptor);
                var swap = Base;
                var target = temRt;
                Graphics.Blit(swap, target);

                foreach (var Add in Adds)
                {
                    if (material.shader != BlendShaders[Add.BlendTypeKey]) { material.shader = BlendShaders[Add.BlendTypeKey]; }
                    material.SetTexture("_DistTex", swap);
                    material.shaderKeywords = new string[] { Add.BlendTypeKey };
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
        public static RenderTexture BlendBlit(Texture2D Base, Texture Add, string blendTypeKey, RenderTexture targetRt = null)
        {
            using (new RTActiveSaver())
            {
                if (targetRt == null) { targetRt = new RenderTexture(Base.width, Base.height, 0); }
                Graphics.Blit(Base, targetRt);
                targetRt.BlendBlit(Add, blendTypeKey);
                return targetRt;
            }
        }
        public struct BlendTexturePair
        {
            public Texture Texture;
            public string BlendTypeKey;

            public BlendTexturePair(Texture texture, string blendTypeKey)
            {
                Texture = texture;
                BlendTypeKey = blendTypeKey;
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
                var mat = new Material(ColorMulShader);
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
                var mat = new Material(ColorMulShader);
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
                var mat = new Material(MaskShader);
                mat.SetTexture("_MaskTex", MaskTex);
                Graphics.CopyTexture(renderTexture, tempRt);
                Graphics.Blit(tempRt, renderTexture, mat);
                RenderTexture.ReleaseTemporary(tempRt);
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }
        public static void ColorBlit(RenderTexture mulDecalTexture, Color Color)
        {
            using (new RTActiveSaver())
            {
                var unlitMat = new Material(UnlitColorAlphaShader);
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
