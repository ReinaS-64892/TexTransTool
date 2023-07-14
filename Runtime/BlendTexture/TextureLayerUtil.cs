#if UNITY_EDITOR

using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
namespace Rs64.TexTransTool
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
    }
    public static class TextureLayerUtil
    {
        public const string BlendTextureCSPaht = "Packages/net.rs64.tex-trans-tool/Runtime/ComputeShaders/BlendTexture.compute";

        [System.Obsolete]
        public static Texture2D InBlendTexture(this ExecuteClient ClientSelect, Texture2D BaseTex, Texture2D AddTex, BlendType BlendType, ComputeShader blendTextureCS = null)
        {
            Texture2D BlendTextere;
            switch (ClientSelect)
            {
                default:
                case ExecuteClient.AsyncCPU:
                    {
                        BlendTextere = TextureLayerUtil.BlendTexture(BaseTex, AddTex, BlendType).Result;
                        break;
                    }
                case ExecuteClient.ComputeSheder:
                    {
                        BlendTextere = TextureLayerUtil.BlendTextureUseComputeSheder(blendTextureCS, BaseTex, AddTex, BlendType);
                        break;
                    }
            }

            return BlendTextere;

        }
        public static async Task<Texture2D> BlendTexture(Texture2D Base, Texture2D Add, BlendType blendType)
        {
            if (Base.width != Add.width && Base.height != Add.height) throw new System.ArgumentException("Textureの解像度が同一ではありません。。");

            var BaesPixels = Base.GetPixels();
            var AddPixels = Add.GetPixels();
            var ResultTexutres = new Texture2D(Base.width, Base.height);
            var ResultPixels = new Color[BaesPixels.Length];

            var PileTasks = new ConfiguredTaskAwaitable<Color>[BaesPixels.Length];

            var indexEnumretor = Enumerable.Range(0, BaesPixels.Length);
            switch (blendType)
            {
                default:
                case BlendType.Normal:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorNormal(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Mul:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorMul(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Screen:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorScreen(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Overlay:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorOverlay(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.HardLight:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorHardLight(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.SoftLight:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorSoftLight(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.ColorDodge:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorColorDodge(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.ColorBurn:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorColorBurn(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.LinearBurn:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorLinearBurn(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.VividLight:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorVividLight(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.LinearLight:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorLinearLight(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Divide:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorDivide(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Addition:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorAddition(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Subtract:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorSubtract(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Difference:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorDifference(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.DarkenOnly:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorDarkenOnly(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.LightenOnly:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorLightenOnly(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Hue:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorHue(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Saturation:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColoSaturation(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Color:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorColor(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.Luminosity:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorLuminosity(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }
                case BlendType.AlphaLerp:
                    {
                        foreach (var Index in indexEnumretor)
                        {
                            var PileTask = Task.Run<Color>(() => BlendColorAlphaLerp(BaesPixels[Index], AddPixels[Index])).ConfigureAwait(false);
                            PileTasks[Index] = PileTask;
                        }
                        break;
                    }



            }

            foreach (var Index in indexEnumretor)
            {
                ResultPixels[Index] = await PileTasks[Index];
            }

            ResultTexutres.SetPixels(ResultPixels);

            return ResultTexutres;
        }


        public static Texture2D BlendTextureUseComputeSheder(ComputeShader CS, Texture2D Base, Texture2D Add, BlendType PileType)
        {
            return BlendTextureUseComputeSheder(CS, new Texture2D[] { Base, Add }, PileType);
        }

        public static Texture2D BlendTextureUseComputeSheder(ComputeShader CS, IReadOnlyList<Texture2D> Textures, BlendType PileType)
        {
            if (!Textures.Any()) throw new System.ArgumentException("対象が存在しません");
            var FirstTex = Textures[0];
            var Size = FirstTex.NativeSize();
            if (Textures.Any(i => i.NativeSize() != Size)) throw new System.ArgumentException("Textureの解像度が同一ではありません。");
            if (CS == null) CS = AssetDatabase.LoadAssetAtPath<ComputeShader>(BlendTextureCSPaht);


            Compiler.NotFIlterAndReadWritTexture2D(ref FirstTex);
            var BaesPixels = FirstTex.GetPixels();
            var ResultTexutres = new Texture2D(Size.x, Size.y);
            int KarnelId = CS.FindKernel(PileType.ToString());

            CS.SetInt("Size", Size.x);

            var BaseTexCB = new ComputeBuffer(BaesPixels.Length, 16);
            var AddTexCB = new ComputeBuffer(BaesPixels.Length, 16);
            BaseTexCB.SetData(BaesPixels);
            CS.SetBuffer(KarnelId, "BaseTex", BaseTexCB);

            foreach (var tex in Textures.Skip(1))
            {
                var AddTex = tex;
                Compiler.NotFIlterAndReadWritTexture2D(ref AddTex);
                var AddPixels = AddTex.GetPixels();
                AddTexCB.SetData(AddPixels);
                CS.SetBuffer(KarnelId, "AddTex", AddTexCB);

                CS.Dispatch(KarnelId, Size.x / 32, Size.y / 32, 1);

            }

            BaseTexCB.GetData(BaesPixels);
            ResultTexutres.SetPixels(BaesPixels);

            BaseTexCB.Release();
            AddTexCB.Release();

            return ResultTexutres;
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
            NaNCheak(ref BlendColor);
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
            NaNCheak(ref BlendColor);
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
            NaNCheak(ref BlendColor);
            InfinityCheak(ref BlendColor);
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
            NaNCheak(ref BlendColor);
            InfinityCheak(ref BlendColor);
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
        public static void NaNCheak(ref Color Color)
        {
            if (float.IsNaN(Color.r)) Color.r = 0f;
            if (float.IsNaN(Color.g)) Color.g = 0f;
            if (float.IsNaN(Color.b)) Color.b = 0f;
            if (float.IsNaN(Color.a)) Color.a = 0f;
        }
        public static void InfinityCheak(ref Color Color)
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
            var ResizedTexture = new Texture2D(Size.x, Size.y);

            var Pixsels = new Color[Size.x * Size.y];

            foreach (var Index in Enumerable.Range(0, Pixsels.Length))
            {
                Pixsels[Index] = GetColorOnTexture(Souse, Index, Size);
            }

            ResizedTexture.SetPixels(Pixsels);
            ResizedTexture.Apply();



            return ResizedTexture;
        }

        public static Color GetColorOnTexture(Texture2D Texture, int Index, Vector2Int SorsSize)
        {
            var Pos = Utils.ConvertIndex2D(Index, SorsSize.x);
            return Texture.GetPixelBilinear(Pos.x / (float)SorsSize.x, Pos.y / (float)SorsSize.y);
        }

    }

}
#endif