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
        AlphaLerp,
        NotBlend,
    }
    public static class TextureBlendUtils
    {

        public static void BlendBlit(this RenderTexture Base, Texture Add, BlendType blendType)
        {
            var material = new Material(Shader.Find("Hidden/BlendTexture"));
            var swap = RenderTexture.GetTemporary(Base.descriptor);
            Graphics.CopyTexture(Base, swap);
            material.SetTexture("_DistTex", swap);
            material.EnableKeyword(blendType.ToString());

            Graphics.Blit(Add, Base, material);
            RenderTexture.ReleaseTemporary(swap);
        }
        public static void BlendBlit(this RenderTexture Base, IEnumerable<BlendTextures> Adds)
        {
            var material = new Material(Shader.Find("Hidden/BlendTexture"));
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

            var material = new Material(Shader.Find("Hidden/BlendTexture"));
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
        [Obsolete]
        public static async Task<Texture2D> BlendTexture(Texture2D Base, Texture2D Add, BlendType blendType)
        {
            if (Base.width != Add.width && Base.height != Add.height) throw new System.ArgumentException("Textureの解像度が同一ではありません。。");

            var BasePixels = Base.GetPixels();
            var AddPixels = Add.GetPixels();
            var ResultTextures = new Texture2D(Base.width, Base.height);
            var ResultPixels = new Color[BasePixels.Length];

            var PileTasks = new ConfiguredTaskAwaitable<Color>[BasePixels.Length];

            var indexEnumerator = Enumerable.Range(0, BasePixels.Length);
            switch (blendType)
            {
                default:
                case BlendType.Normal:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorNormal(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Mul:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorMul(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Screen:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorScreen(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Overlay:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorOverlay(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.HardLight:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorHardLight(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.SoftLight:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorSoftLight(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.ColorDodge:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorColorDodge(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.ColorBurn:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorColorBurn(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.LinearBurn:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorLinearBurn(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.VividLight:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorVividLight(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.LinearLight:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorLinearLight(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Divide:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorDivide(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Addition:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorAddition(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Subtract:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorSubtract(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Difference:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorDifference(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.DarkenOnly:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorDarkenOnly(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.LightenOnly:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorLightenOnly(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Hue:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorHue(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Saturation:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColoSaturation(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Color:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorColor(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Luminosity:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorLuminosity(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.AlphaLerp:
                    {
                        foreach (var Index in indexEnumerator)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorAlphaLerp(BasePixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }



            }

            foreach (var Index in indexEnumerator)
            {
                ResultPixels[Index] = await PileTasks[Index];
            }

            ResultTextures.SetPixels(ResultPixels);

            return ResultTextures;
        }
        public static TwoDimensionalMap<Color> BlendTextureUseComputeShader(ComputeShader CS, TwoDimensionalMap<Color> Base, TwoDimensionalMap<Color> Add, BlendType PileType)
        {
            return BlendTextureUseComputeShader(CS, new List<TwoDimensionalMap<Color>> { Base, Add }, PileType);
        }
        public static TwoDimensionalMap<Color> BlendTextureUseComputeShader(ComputeShader CS, List<TwoDimensionalMap<Color>> Textures, BlendType PileType)
        {
            if (!Textures.Any()) throw new System.ArgumentException("対象が存在しません");
            var FirstTex = Textures[0];
            var Size = FirstTex.MapSize;
            if (Textures.Any(i => i.MapSize != Size)) throw new System.ArgumentException("Textureの解像度が同一ではありません。");
            if (CS == null) throw new System.ArgumentException("ComputeShaderが存在しません。");


            var BasePixels = FirstTex.Array.Clone() as Color[];
            int KernelId = CS.FindKernel(PileType.ToString());

            CS.SetInt("Size", Size.x);

            var BaseTexCB = new ComputeBuffer(BasePixels.Length, 16);
            var AddTexCB = new ComputeBuffer(BasePixels.Length, 16);
            BaseTexCB.SetData(BasePixels);
            CS.SetBuffer(KernelId, "BaseTex", BaseTexCB);

            foreach (var tex in Textures.Skip(1))
            {
                var AddTex = tex;
                var AddPixels = AddTex.Array;
                AddTexCB.SetData(AddPixels);
                CS.SetBuffer(KernelId, "AddTex", AddTexCB);

                CS.Dispatch(KernelId, Size.x / 32, Size.y / 32, 1);
            }

            BaseTexCB.GetData(BasePixels);

            BaseTexCB.Release();
            AddTexCB.Release();

            return new TwoDimensionalMap<Color>(BasePixels, Size);
        }
        static (float, float) FinalAlphaAndReversCal(float Base, float Add)
        {
            float AddRevAlpha = 1 - Add;
            float Alpha = Add + (Base * AddRevAlpha);
            return (Alpha, AddRevAlpha);
        }
        public static Color BlendColorNormal(Color Base, Color Add)
        {
            float FinalAlpha, AddRevAlpha; (FinalAlpha, AddRevAlpha) = FinalAlphaAndReversCal(Base.a, Add.a);
            var ResultColor = (Add * Add.a) + ((Base * Base.a) * AddRevAlpha);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorMul(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            var MulColor = Base * Add;
            var ResultColor = Color.Lerp(Base, MulColor, Add.a);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorScreen(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            Color OneColor = new Color(1, 1, 1, 1);
            var ColorScreenColor = OneColor - (OneColor - Base) * (OneColor - Add);
            var ResultColor = Color.Lerp(Base, ColorScreenColor, Add.a);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorOverlay(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            Color ResultColor;
            if (Base.a < 0.5)
            {
                ResultColor = 2 * BlendColorMul(Base, Add);
            }
            else
            {
                ResultColor = BlendColorScreen(Base, Add);
            }
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorHardLight(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            Color BlendColor;
            if (Add.b < 0.5)
            {
                BlendColor = 2 * BlendColorMul(Base, Add);
            }
            else
            {
                BlendColor = BlendColorScreen(Base, Add);
            }
            var ResultColor = Color.Lerp(Base, BlendColor, Add.a);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorSoftLight(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            Color BlendColor = (Color.white - 2 * Add) * (Base * Base) + 2 * BlendColorMul(Base, Add);
            var ResultColor = Color.Lerp(Base, BlendColor, Add.a);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorColorDodge(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            Color BlendColor = Color.white;
            BlendColor.r = Mathf.Clamp01(Base.r / (1 - Add.r));
            BlendColor.g = Mathf.Clamp01(Base.g / (1 - Add.g));
            BlendColor.b = Mathf.Clamp01(Base.b / (1 - Add.b));
            NaNCheck(ref BlendColor);
            var ResultColor = Color.Lerp(Base, BlendColor, Add.a);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorColorBurn(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            Color BlendColor = Color.white;
            BlendColor.r = 1 - Mathf.Clamp01((1 - Base.r) / Add.r);
            BlendColor.g = 1 - Mathf.Clamp01((1 - Base.g) / Add.g);
            BlendColor.b = 1 - Mathf.Clamp01((1 - Base.b) / Add.b);
            NaNCheck(ref BlendColor);
            var ResultColor = Color.Lerp(Base, BlendColor, Add.a);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorLinearBurn(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            Color BlendColor = Base + Add - Color.white;
            var ResultColor = Color.Lerp(Base, BlendColor, Add.a);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorVividLight(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            Color BlendColor = new Color(
                Add.r < 0.5 ? 1 - (1 - Base.r) / (2 * Add.r) : Base.r / (1 - 2 * (Add.r - 0.5f)),
                Add.g < 0.5 ? 1 - (1 - Base.g) / (2 * Add.g) : Base.g / (1 - 2 * (Add.g - 0.5f)),
                Add.b < 0.5 ? 1 - (1 - Base.b) / (2 * Add.b) : Base.b / (1 - 2 * (Add.b - 0.5f)),
                1f
            );
            NaNCheck(ref BlendColor);
            InfinityCheck(ref BlendColor);
            var ResultColor = Color.Lerp(Base, BlendColor, Add.a);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorLinearLight(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            Color BlendColor = Base + (2 * Add) - Color.white;
            var ResultColor = Color.Lerp(Base, BlendColor, Add.a);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorDivide(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            Color BlendColor = new Color(
                 Base.r / Add.r,
                  Base.g / Add.g,
                   Base.b / Add.b,
                   1f
            );
            NaNCheck(ref BlendColor);
            InfinityCheck(ref BlendColor);
            var ResultColor = Color.Lerp(Base, BlendColor, Add.a);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorAddition(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            Color BlendColor = Base + Add;
            ColorClamp01(ref BlendColor);
            var ResultColor = Color.Lerp(Base, BlendColor, Add.a);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorSubtract(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            Color BlendColor = Base - Add;
            ColorClamp01(ref BlendColor);
            var ResultColor = Color.Lerp(Base, BlendColor, Add.a);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorDifference(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            Color BlendColor = new Color(
                Mathf.Abs(Base.r - Add.r),
                Mathf.Abs(Base.g - Add.g),
                Mathf.Abs(Base.b - Add.b),
                1f
            );
            var ResultColor = Color.Lerp(Base, BlendColor, Add.a);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorDarkenOnly(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            Color BlendColor = new Color(
                Mathf.Min(Base.r, Add.r),
                Mathf.Min(Base.g, Add.g),
                Mathf.Min(Base.b, Add.b),
                1f
            );
            var ResultColor = Color.Lerp(Base, BlendColor, Add.a);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorLightenOnly(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            Color BlendColor = new Color(
                Mathf.Max(Base.r, Add.r),
                Mathf.Max(Base.g, Add.g),
                Mathf.Max(Base.b, Add.b),
                1f
            );
            var ResultColor = Color.Lerp(Base, BlendColor, Add.a);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorHue(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            Color.RGBToHSV(Base, out var BaseH, out var BaseS, out var BaseV);
            Color.RGBToHSV(Add, out var AddH, out var AddS, out var AddV);
            Color BlendColor = Color.HSVToRGB(AddH, BaseS, BaseV);
            var ResultColor = Color.Lerp(Base, BlendColor, Add.a);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColoSaturation(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            Color.RGBToHSV(Base, out var BaseH, out var BaseS, out var BaseV);
            Color.RGBToHSV(Add, out var AddH, out var AddS, out var AddV);
            Color BlendColor = Color.HSVToRGB(BaseH, AddS, BaseV);
            var ResultColor = Color.Lerp(Base, BlendColor, Add.a);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorColor(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            Color.RGBToHSV(Base, out var BaseH, out var BaseS, out var BaseV);
            Color.RGBToHSV(Add, out var AddH, out var AddS, out var AddV);
            Color BlendColor = Color.HSVToRGB(AddH, AddS, BaseV);
            var ResultColor = Color.Lerp(Base, BlendColor, Add.a);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorLuminosity(Color Base, Color Add)
        {
            float FinalAlpha = FinalAlphaAndReversCal(Base.a, Add.a).Item1;
            Color.RGBToHSV(Base, out var BaseH, out var BaseS, out var BaseV);
            Color.RGBToHSV(Add, out var AddH, out var AddS, out var AddV);
            Color BlendColor = Color.HSVToRGB(BaseH, BaseS, AddV);
            var ResultColor = Color.Lerp(Base, BlendColor, Add.a);
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static Color BlendColorAlphaLerp(Color Base, Color Add)
        {
            float FinalAlpha, AddRevAlpha; (FinalAlpha, AddRevAlpha) = FinalAlphaAndReversCal(Base.a, Add.a);
            var ResultColor = Color.Lerp(Base, Add, Add.a / (Add.a + (AddRevAlpha * Base.a)));
            ResultColor.a = FinalAlpha;
            return ResultColor;
        }
        public static void NaNCheck(ref Color Color)
        {
            if (float.IsNaN(Color.r)) Color.r = 0f;
            if (float.IsNaN(Color.g)) Color.g = 0f;
            if (float.IsNaN(Color.b)) Color.b = 0f;
            if (float.IsNaN(Color.a)) Color.a = 0f;
        }
        public static void InfinityCheck(ref Color Color)
        {
            if (float.IsNegativeInfinity(Color.r)) Color.r = 0f;
            else if (float.IsPositiveInfinity(Color.r)) Color.r = 1f;
            if (float.IsNegativeInfinity(Color.g)) Color.g = 0f;
            else if (float.IsPositiveInfinity(Color.g)) Color.g = 1f;
            if (float.IsNegativeInfinity(Color.b)) Color.b = 0f;
            else if (float.IsPositiveInfinity(Color.b)) Color.b = 1f;
            if (float.IsNegativeInfinity(Color.a)) Color.a = 0f;
            else if (float.IsPositiveInfinity(Color.a)) Color.a = 1f;
        }
        public static void ColorClamp01(ref Color Color)
        {
            Color.r = Mathf.Clamp01(Color.r);
            Color.g = Mathf.Clamp01(Color.g);
            Color.b = Mathf.Clamp01(Color.b);
            Color.a = Mathf.Clamp01(Color.a);
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
            var mat = new Material(Shader.Find("Hidden/ColorMulShader"));
            mat.SetColor("_Color", Color);
            Graphics.Blit(MainTex, MainTexRt, mat);
        }
        public static void MultipleRenderTexture(RenderTexture renderTexture, Color Color)
        {
            var tempRt = RenderTexture.GetTemporary(renderTexture.descriptor);
            var mat = new Material(Shader.Find("Hidden/ColorMulShader"));
            mat.SetColor("_Color", Color);
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
            var unlitMat = new Material(Shader.Find("Hidden/UnlitColorAndAlpha"));
            unlitMat.SetColor("_Color", Color);
            Graphics.Blit(null, mulDecalTexture, unlitMat);
        }
    }

}
