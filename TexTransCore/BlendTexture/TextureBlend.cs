using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using System;
using net.rs64.TexTransCore.TransTextureCore.Utils;

namespace net.rs64.TexTransCore.BlendTexture
{
    [Obsolete("Replaced with BlendTypeKey", true)]
    internal enum BlendType
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
    internal enum TTTBlendTypeKeyEnum
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
    internal static class TextureBlend
    {
        public static Dictionary<string, Shader> BlendShaders;
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        static void EditorInitDerayCall()
        {
            UnityEditor.EditorApplication.delayCall += EditorInitDerayCaller;
        }
        static void EditorInitDerayCaller()
        {
            BlendShadersInit();
            UnityEditor.EditorApplication.delayCall -= EditorInitDerayCaller;
        }
#endif
        public static void BlendShadersInit()
        {
            BlendTexShader = Shader.Find(BLEND_TEX_SHADER);
            ColorMulShader = Shader.Find(COLOR_MUL_SHADER);
            MaskShader = Shader.Find(MASK_SHADER);
            UnlitColorAlphaShader = Shader.Find(UNLIT_COLOR_ALPHA_SHADER);
            AlphaCopyShader = Shader.Find(ALPHA_COPY_SHADER);

            var tttBlendShader = BlendTexShader;
            BlendShaders = new Dictionary<string, Shader>()
            {
                {"Normal",tttBlendShader},
                {"Mul",tttBlendShader},
                {"Screen",tttBlendShader},
                {"Overlay",tttBlendShader},
                {"HardLight",tttBlendShader},
                {"SoftLight",tttBlendShader},
                {"ColorDodge",tttBlendShader},
                {"ColorBurn",tttBlendShader},
                {"LinearBurn",tttBlendShader},
                {"VividLight",tttBlendShader},
                {"LinearLight",tttBlendShader},
                {"Divide",tttBlendShader},
                {"Addition",tttBlendShader},
                {"Subtract",tttBlendShader},
                {"Difference",tttBlendShader},
                {"DarkenOnly",tttBlendShader},
                {"LightenOnly",tttBlendShader},
                {"Hue",tttBlendShader},
                {"Saturation",tttBlendShader},
                {"Color",tttBlendShader},
                {"Luminosity",tttBlendShader},
                {"NotBlend",tttBlendShader},
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
        public static Shader BlendTexShader;
        public const string COLOR_MUL_SHADER = "Hidden/ColorMulShader";
        public static Shader ColorMulShader;
        public const string MASK_SHADER = "Hidden/MaskShader";
        public static Shader MaskShader;
        public const string UNLIT_COLOR_ALPHA_SHADER = "Hidden/UnlitColorAndAlpha";
        public static Shader UnlitColorAlphaShader;
        public const string ALPHA_COPY_SHADER = "Hidden/AlphaCopy";
        public static Shader AlphaCopyShader;
        public static void BlendBlit(this RenderTexture baseRenderTexture, Texture Add, string blendTypeKey, bool keepAlpha = false)
        {
            using (new RTActiveSaver())
            {
                var material = new Material(BlendShaders[blendTypeKey]);
                var swap = RenderTexture.GetTemporary(baseRenderTexture.descriptor);
                Graphics.CopyTexture(baseRenderTexture, swap);
                material.SetTexture("_DistTex", swap);
                material.EnableKeyword(blendTypeKey);

                Graphics.Blit(Add, baseRenderTexture, material);

                if (keepAlpha)
                {
                    var alphaCopyMat = new Material(AlphaCopyShader);
                    var baseSwap = RenderTexture.GetTemporary(baseRenderTexture.descriptor);

                    alphaCopyMat.SetTexture("_AlphaTex", swap);
                    Graphics.CopyTexture(baseRenderTexture, baseSwap);
                    Graphics.Blit(baseSwap, baseRenderTexture, alphaCopyMat);

                    RenderTexture.ReleaseTemporary(baseSwap);
                    UnityEngine.Object.DestroyImmediate(alphaCopyMat);
                }

                RenderTexture.ReleaseTemporary(swap);
                UnityEngine.Object.DestroyImmediate(material);
            }
        }
        public static void BlendBlit(this RenderTexture baseRenderTexture, IEnumerable<BlendTexturePair> adds)
        {
            using (new RTActiveSaver())
            {
                var material = new Material(BlendTexShader);
                var temRt = RenderTexture.GetTemporary(baseRenderTexture.descriptor);
                var swap = baseRenderTexture;
                var target = temRt;
                Graphics.Blit(swap, target);

                foreach (var Add in adds)
                {
                    if (material.shader != BlendShaders[Add.BlendTypeKey]) { material.shader = BlendShaders[Add.BlendTypeKey]; }
                    material.SetTexture("_DistTex", swap);
                    material.shaderKeywords = new[] { Add.BlendTypeKey };
                    Graphics.Blit(Add.Texture, target, material);
                    (swap, target) = (target, swap);
                }

                if (swap != baseRenderTexture)
                {
                    Graphics.Blit(swap, baseRenderTexture);
                }
                RenderTexture.ReleaseTemporary(temRt);
                UnityEngine.Object.DestroyImmediate(material);
            }
        }
        public static RenderTexture BlendBlit(Texture2D baseRenderTexture, Texture add, string blendTypeKey, RenderTexture targetRt = null)
        {
            using (new RTActiveSaver())
            {
                if (targetRt == null) { targetRt = new RenderTexture(baseRenderTexture.width, baseRenderTexture.height, 0); }
                Graphics.Blit(baseRenderTexture, targetRt);
                targetRt.BlendBlit(add, blendTypeKey);
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

        public static RenderTexture CreateMultipliedRenderTexture(Texture mainTex, Color color)
        {
            var mainTexRt = new RenderTexture(mainTex.width, mainTex.height, 0);
            MultipleRenderTexture(mainTexRt, mainTex, color);
            return mainTexRt;
        }
        public static void MultipleRenderTexture(RenderTexture mainTexRt, Texture mainTex, Color color)
        {
            using (new RTActiveSaver())
            {
                var mat = new Material(ColorMulShader);
                mat.SetColor("_Color", color);
                Graphics.Blit(mainTex, mainTexRt, mat);
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }
        public static void MultipleRenderTexture(RenderTexture renderTexture, Color color)
        {
            using (new RTActiveSaver())
            {
                var tempRt = RenderTexture.GetTemporary(renderTexture.descriptor);
                var mat = new Material(ColorMulShader);
                mat.SetColor("_Color", color);
                Graphics.CopyTexture(renderTexture, tempRt);
                Graphics.Blit(tempRt, renderTexture, mat);
                RenderTexture.ReleaseTemporary(tempRt);
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }
        public static void MaskDrawRenderTexture(RenderTexture renderTexture, Texture maskTex)
        {
            using (new RTActiveSaver())
            {
                var tempRt = RenderTexture.GetTemporary(renderTexture.descriptor);
                var mat = new Material(MaskShader);
                mat.SetTexture("_MaskTex", maskTex);
                Graphics.CopyTexture(renderTexture, tempRt);
                Graphics.Blit(tempRt, renderTexture, mat);
                RenderTexture.ReleaseTemporary(tempRt);
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }
        public static void ColorBlit(RenderTexture mulDecalTexture, Color color)
        {
            using (new RTActiveSaver())
            {
                var unlitMat = new Material(UnlitColorAlphaShader);
                unlitMat.SetColor("_Color", color);
                Graphics.Blit(null, mulDecalTexture, unlitMat);
                UnityEngine.Object.DestroyImmediate(unlitMat);
            }
        }
    }

    internal struct RTActiveSaver : IDisposable
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
