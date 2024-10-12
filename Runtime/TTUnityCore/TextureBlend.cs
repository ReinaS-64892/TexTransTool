using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using System;
using net.rs64.TexTransUnityCore.Utils;
using UnityEngine.Profiling;

namespace net.rs64.TexTransUnityCore
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
    internal enum TTTBlendTypeKeyEnum//これをセーブデータとして使うべきではないから注意、ToStringして使うことを前提として
    {
        Normal,
        Dissolve,
        NotBlend,

        Mul,
        ColorBurn,
        LinearBurn,
        DarkenOnly,
        DarkenColorOnly,

        Screen,
        ColorDodge,
        ColorDodgeGlow,
        Addition,
        AdditionGlow,
        LightenOnly,
        LightenColorOnly,

        Overlay,
        SoftLight,
        HardLight,
        VividLight,
        LinearLight,
        PinLight,
        HardMix,

        Difference,
        Exclusion,
        Subtract,
        Divide,

        Hue,
        Saturation,
        Color,
        Luminosity,

    }
    internal static class TextureBlend
    {
        public static Dictionary<string, TTBlendUnityObject> BlendObjects;

        [TexTransInitialize]
        public static void BlendShadersInit()
        {
            InitImpl();
            TexTransCoreRuntime.NewAssetListen[typeof(TTBlendUnityObject)] = InitImpl;

            static void InitImpl()
            {
                BlendObjects = TexTransCoreRuntime.LoadAssetsAtType(typeof(TTBlendUnityObject)).Cast<TTBlendUnityObject>().ToDictionary(i => i.BlendTypeKey, i => i);
            }

            AlphaCopyCS = TexTransCoreRuntime.LoadAsset(AlphaCopy_GUID, typeof(TTComputeOperator)) as TTComputeOperator;
            AlphaMultiplyCS = TexTransCoreRuntime.LoadAsset(AlphaMultiply_GUID, typeof(TTComputeOperator)) as TTComputeOperator;
            AlphaMultiplyWithTextureCS = TexTransCoreRuntime.LoadAsset(AlphaMultiplyWithTexture_GUID, typeof(TTComputeOperator)) as TTComputeOperator;
            ColorMultiplyCS = TexTransCoreRuntime.LoadAsset(ColorMultiply_GUID, typeof(TTComputeOperator)) as TTComputeOperator;
            FillAlphaCS = TexTransCoreRuntime.LoadAsset(FillAlpha_GUID, typeof(TTComputeOperator)) as TTComputeOperator;
            FillColorCS = TexTransCoreRuntime.LoadAsset(FillColor_GUID, typeof(TTComputeOperator)) as TTComputeOperator;
            GammaToLinearCS = TexTransCoreRuntime.LoadAsset(GammaToLinear_GUID, typeof(TTComputeOperator)) as TTComputeOperator;
            LinearToGammaCS = TexTransCoreRuntime.LoadAsset(LinearToGamma_GUID, typeof(TTComputeOperator)) as TTComputeOperator;
        }
        public const string BL_KEY_DEFAULT = "Normal";

        public static TTComputeOperator AlphaCopyCS;
        public const string AlphaCopy_GUID = "2571b2e5ca219a24483f124443cfe576";
        public static TTComputeOperator AlphaMultiplyCS;
        public const string AlphaMultiply_GUID = "cb7ef584d6da2fd4aafb78060dcc6fcd";
        public static TTComputeOperator AlphaMultiplyWithTextureCS;
        public const string AlphaMultiplyWithTexture_GUID = "1c0fcf97b19737949a3e31c87dd9dc8c";
        public static TTComputeOperator ColorMultiplyCS;
        public const string ColorMultiply_GUID = "d212d4b1212893046a46d0f93379238f";
        public static TTComputeOperator FillAlphaCS;
        public const string FillAlpha_GUID = "179eb14ad0b403749b4beaac35ed4eb3";
        public static TTComputeOperator FillColorCS;
        public const string FillColor_GUID = "11de539c9c2863645af990b76a0aded8";
        public static TTComputeOperator GammaToLinearCS;
        public const string GammaToLinear_GUID = "a8e89c380f851544cac5644c50476b24";
        public static TTComputeOperator LinearToGammaCS;
        public const string LinearToGamma_GUID = "e7375a4fbbaae5949b896db160924d95";



        public static void BlendBlit(this RenderTexture baseRenderTexture, RenderTexture addRenderTexture, TTBlendUnityObject blendUnityObject)
        {
            if (baseRenderTexture.width != addRenderTexture.width) { throw new ArgumentException(); }
            if (baseRenderTexture.height != addRenderTexture.height) { throw new ArgumentException(); }
            if (baseRenderTexture.enableRandomWrite is false || addRenderTexture.enableRandomWrite is false) { throw new ArgumentException(); }

            using (new UsingColoSpace(addRenderTexture, blendUnityObject.IsLinerRequired))
            using (new UsingColoSpace(baseRenderTexture, blendUnityObject.IsLinerRequired))
            {
                var cs = blendUnityObject.Compute;

                cs.SetTexture(0, "AddTex", addRenderTexture);
                cs.SetTexture(0, "DistTex", baseRenderTexture);

                cs.Dispatch(0, Mathf.Max(1, baseRenderTexture.width / 32), Mathf.Max(1, baseRenderTexture.height / 32), 1);
            }
        }
        public static void BlendBlit(this RenderTexture baseRenderTexture, Texture Add, string blendTypeKey)
        {
            using (new RTActiveSaver())
            using (TTRt.U(out var swap, baseRenderTexture.descriptor))
            {
                Graphics.CopyTexture(baseRenderTexture, swap);

                var tempMaterial = MatTemp.GetTempMatShader(BlendObjects[blendTypeKey].Shader);
                tempMaterial.SetTexture("_DistTex", swap);

                Graphics.Blit(Add, baseRenderTexture, tempMaterial);

            }
        }
        public static void BlendBlit<BlendTex>(this RenderTexture baseRenderTexture, BlendTex add)
        where BlendTex : IBlendTexturePair
        { baseRenderTexture.BlendBlit(add.Texture, add.BlendTypeKey); }
        public static void BlendBlit<BlendTex>(this RenderTexture baseRenderTexture, IEnumerable<BlendTex> adds)
        where BlendTex : IBlendTexturePair
        {
            Profiler.BeginSample("BlendBlit");
            using (new RTActiveSaver())
            {
                Profiler.BeginSample("Create RT");
                using (TTRt.U(out var temRt, baseRenderTexture.descriptor))
                {
                    Profiler.EndSample();

                    var swap = baseRenderTexture;
                    var target = temRt;
                    Graphics.Blit(swap, target);

                    foreach (var Add in adds)
                    {
                        if (Add == null || Add.Texture == null || Add.BlendTypeKey == null) { continue; }
                        var tempMaterial = MatTemp.GetTempMatShader(BlendObjects[Add.BlendTypeKey].Shader);
                        tempMaterial.SetTexture("_DistTex", swap);
                        Graphics.Blit(Add.Texture, target, tempMaterial);
                        (swap, target) = (target, swap);
                    }

                    if (swap != baseRenderTexture)
                    {
                        Graphics.Blit(swap, baseRenderTexture);
                    }
                }
            }
            Profiler.EndSample();
        }
        public interface IBlendTexturePair
        {
            public Texture Texture { get; }
            public string BlendTypeKey { get; }

        }
        public struct BlendTexturePair : IBlendTexturePair
        {
            public Texture Texture;
            public string BlendTypeKey;

            public BlendTexturePair(IBlendTexturePair setTex)
            {
                Texture = setTex.Texture;
                BlendTypeKey = setTex.BlendTypeKey;
            }

            public BlendTexturePair(Texture texture, string blendTypeKey)
            {
                Texture = texture;
                BlendTypeKey = blendTypeKey;
            }

            Texture IBlendTexturePair.Texture => Texture;

            string IBlendTexturePair.BlendTypeKey => BlendTypeKey;
        }
        public static RenderTexture CreateMultipliedRenderTexture(Texture rt, Color color)
        {
            var mainTexRt = TTRt.G(rt.width, rt.height);
            mainTexRt.name = $"{rt.name}-CreateMultipliedRenderTexture-whit-TempRt-{mainTexRt.width}x{mainTexRt.height}";
            Graphics.Blit(rt, mainTexRt);
            ColorMultiply(mainTexRt, color);
            return mainTexRt;
        }
        public static void ColorMultiply(RenderTexture rt, Color color)
        {
            var cs = ColorMultiplyCS.Compute;
            cs.SetFloats("Color", color.r, color.g, color.b, color.a);
            cs.SetTexture(0, "Tex", rt);
            cs.Dispatch(0, Mathf.Max(1, rt.width / 32), Mathf.Max(1, rt.height / 32), 1);
        }
        public static void AlphaMultiplyWithTexture(RenderTexture rt, RenderTexture maskTex)
        {
            var cs = AlphaMultiplyWithTextureCS.Compute;

            cs.SetTexture(0, "SourceTex", maskTex);
            cs.SetTexture(0, "TargetTex", rt);

            cs.Dispatch(0, Mathf.Max(1, rt.width / 32), Mathf.Max(1, rt.height / 32), 1);
        }

        public static void FillColor(RenderTexture rt, Color color)
        {
            var cs = FillColorCS.Compute;
            cs.SetFloats("Color", color.r, color.g, color.b, color.a);
            cs.SetTexture(0, "Tex", rt);
            cs.Dispatch(0, Mathf.Max(1, rt.width / 32), Mathf.Max(1, rt.height / 32), 1);
        }
        public static void AlphaFill(RenderTexture rt, float alpha)
        {
            var cs = FillAlphaCS.Compute;
            cs.SetFloat("Alpha", alpha);
            cs.SetTexture(0, "Tex", rt);
            cs.Dispatch(0, Mathf.Max(1, rt.width / 32), Mathf.Max(1, rt.height / 32), 1);
        }
        public static void AlphaOne(RenderTexture rt) { AlphaFill(rt, 1f); }

        public static void AlphaCopy(RenderTexture alphaSource, RenderTexture rt)
        {
            var cs = AlphaCopyCS.Compute;

            cs.SetTexture(0, "SourceTex", alphaSource);
            cs.SetTexture(0, "TargetTex", rt);

            cs.Dispatch(0, Mathf.Max(1, rt.width / 32), Mathf.Max(1, rt.height / 32), 1);
        }

        public static void ToGamma(RenderTexture rt)
        {
            var cs = LinearToGammaCS.Compute;
            cs.SetTexture(0, "Tex", rt);
            cs.Dispatch(0, Mathf.Max(1, rt.width / 32), Mathf.Max(1, rt.height / 32), 1);
        }
        public static void ToLinear(RenderTexture rt)
        {
            var cs = GammaToLinearCS.Compute;
            cs.SetTexture(0, "Tex", rt);
            cs.Dispatch(0, Mathf.Max(1, rt.width / 32), Mathf.Max(1, rt.height / 32), 1);
        }
    }
    public readonly struct UsingColoSpace : IDisposable
    {
        public readonly RenderTexture RenderTexture;
        public readonly bool IsLinear;
        public UsingColoSpace(RenderTexture rt, bool isLinear)
        {
            RenderTexture = rt;
            IsLinear = isLinear;

            if (TTUnityCoreEngine.IsLinerRenderTexture != IsLinear)
            {
                if (IsLinear) { TextureBlend.ToLinear(RenderTexture); }//もともとガンマ空間で、ブレンドがリニアでやりたいとき用
                else { TextureBlend.ToGamma(RenderTexture); }//もともとリニア空間で、ブレンドがガンマで
            }
        }
        public void Dispose()
        {

            if (TTUnityCoreEngine.IsLinerRenderTexture != IsLinear)//TTUnityCoreEngine.IsLinerRenderTexture が変わらないと思い込んでいる。
            {
                if (IsLinear) { TextureBlend.ToGamma(RenderTexture); }//それぞれ元に戻す
                else { TextureBlend.ToLinear(RenderTexture); }
            }
        }
    }


}
