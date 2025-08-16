#nullable enable
using System;
using System.IO;
using System.Linq;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEngine;
using Color = UnityEngine.Color;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class PhotoshopGradationMapLayer : AbstractLayer
    {
        internal const string ComponentName = "TTT " + nameof(PhotoshopGradationMapLayer);
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;

        public bool IsGradientReversed;
        public bool IsGradientDithered;
        public GradientInteropMethod InteropMethod;
        public float Smoothens;
        public ColorKey[] ColorKeys;
        public TransparencyKey[] TransparencyKeys;

        internal override LayerObject<ITexTransToolForUnity> GetLayerObject(GenerateLayerObjectContext ctx)
        {
            var domain = ctx.Domain;
            var engine = ctx.Engine;

            domain.Observe(this);
            domain.Observe(gameObject);

            var lm = GetAlphaMaskObject(ctx);
            var blKey = engine.QueryBlendKey(BlendTypeKey);
            var lumMap = new LuminanceMapping(new PhotoshopGradationWrapper(this));

            return new GrabBlendingAsLayer<ITexTransToolForUnity>(Visible, lm, Clipping, blKey, lumMap);
        }

#if UNITY_EDITOR
        [ContextMenu("Debug")]
        public void Debug()
        {
            var path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/DebugGradient.png");
            var grad = new PhotoshopGradationWrapper(this);

            var tex = new Texture2D(grad.RecommendedResolution, 1, TextureFormat.RGBAFloat, false);
            var na = tex.GetRawTextureData<TexTransCore.Color>();
            grad.WriteGradient(na.AsSpan());

            File.WriteAllBytes(path, tex.EncodeToPNG());
        }
#endif

        [Serializable]
        public struct ColorKey
        {
            public float KeyLocation;
            public float MidLocation;
            [ColorUsage(false)] public Color Color;
        }
        [Serializable]
        public struct TransparencyKey
        {
            public float KeyLocation;
            public float MidLocation;
            public float Transparency;
        }
        public enum GradientInteropMethod
        {
            Classic,
            Perceptual,
            Linear,
            Smooth,
            Stripes,
        }
        public class PhotoshopGradationWrapper : ILuminanceMappingGradient
        {
            public bool IsGradientReversed;
            public bool IsGradientDithered;
            public GradientInteropMethod InteropMethod;
            public float Smoothens;
            public ColorKey[] ColorKeys;
            public TransparencyKey[] TransparencyKeys;

            public PhotoshopGradationWrapper(PhotoshopGradationMapLayer gradient)
            {
                IsGradientReversed = gradient.IsGradientReversed;
                IsGradientDithered = gradient.IsGradientDithered;
                InteropMethod = gradient.InteropMethod;
                Smoothens = gradient.Smoothens;
                ColorKeys = gradient.ColorKeys;
                TransparencyKeys = gradient.TransparencyKeys;
                Array.Sort(ColorKeys, (l, r) => (int)Math.Ceiling(l.KeyLocation * 1000) - (int)Math.Ceiling(r.KeyLocation * 1000));
                Array.Sort(TransparencyKeys, (l, r) => (int)Math.Ceiling(l.KeyLocation * 1000) - (int)Math.Ceiling(r.KeyLocation * 1000));
            }
            public int RecommendedResolution => 256;

            public void WriteGradient(Span<TexTransCore.Color> writeSpan)
            {
                float maxValue = writeSpan.Length - 1;
                for (var i = 0; writeSpan.Length > i; i += 1)
                {
                    var position = i / maxValue;

                    var col = EvaluateColor(position);
                    var alpha = EvaluateAlpha(position);

                    writeSpan[i] = new(col.R, col.G, col.B, alpha);
                }
            }

            private ColorWOAlpha EvaluateColor(float position)
            {
                var col = new TexTransCore.ColorWOAlpha() { R = 1f, G = 0f, B = 1f };// 未実装の方式は、マテリアルエラー色にして人々を怖がらせましょう！

                var leftKeyNullable = ColorKeys.Reverse().FirstOrValueNull(k => position > k.KeyLocation);
                var rightKeyNullable = ColorKeys.FirstOrValueNull(k => position <= k.KeyLocation);

                if (leftKeyNullable is null) { leftKeyNullable = rightKeyNullable; }
                if (rightKeyNullable is null) { rightKeyNullable = leftKeyNullable; }
                if (leftKeyNullable is null || rightKeyNullable is null) { throw new NullReferenceException(); }

                var leftKey = leftKeyNullable.Value;
                var rightKey = rightKeyNullable.Value;

                var keyRange = rightKey.KeyLocation - leftKey.KeyLocation;
                var relativeLocation = position - leftKey.KeyLocation;
                var keyRangeScalePotion = TTMath.NotNaN(relativeLocation / keyRange, 0.5f);

                var lCol = leftKey.Color.ToTTCoreWOAlpha();
                var rCol = rightKey.Color.ToTTCoreWOAlpha();
                switch (InteropMethod)
                {
                    case GradientInteropMethod.Classic:
                        {
                            col.R = TTMath.Lerp(lCol.R, rCol.R, keyRangeScalePotion);
                            col.G = TTMath.Lerp(lCol.G, rCol.G, keyRangeScalePotion);
                            col.B = TTMath.Lerp(lCol.B, rCol.B, keyRangeScalePotion);
                            break;
                        }
                    case GradientInteropMethod.Perceptual:
                        {
                            break;
                        }
                    case GradientInteropMethod.Linear:
                        {
                            lCol = TTMath.GammaToLinear(lCol);
                            rCol = TTMath.GammaToLinear(rCol);

                            col.R = TTMath.Lerp(lCol.R, rCol.R, keyRangeScalePotion);
                            col.G = TTMath.Lerp(lCol.G, rCol.G, keyRangeScalePotion);
                            col.B = TTMath.Lerp(lCol.B, rCol.B, keyRangeScalePotion);

                            col = TTMath.LinearToGamma(col);
                            break;
                        }
                    case GradientInteropMethod.Smooth:
                        {
                            break;
                        }
                    case GradientInteropMethod.Stripes:
                        {
                            col = lCol;
                            // col = new(leftKey.KeyLocation,rightKey.KeyLocation,1f);
                            break;
                        }
                }

                return col;
            }
            private float EvaluateAlpha(float position)
            {
                var leftKeyNullable = TransparencyKeys.Reverse().FirstOrValueNull(k => position > k.KeyLocation);
                var rightKeyNullable = TransparencyKeys.FirstOrValueNull(k => position <= k.KeyLocation);

                if (leftKeyNullable is null) { leftKeyNullable = rightKeyNullable; }
                if (rightKeyNullable is null) { rightKeyNullable = leftKeyNullable; }
                if (leftKeyNullable is null || rightKeyNullable is null) { throw new NullReferenceException(); }

                var leftKey = leftKeyNullable.Value;
                var rightKey = rightKeyNullable.Value;

                var keyRange = rightKey.KeyLocation - leftKey.KeyLocation;
                var relativeLocation = position - leftKey.KeyLocation;
                var keyRangeScalePotion = TTMath.NotNaN(relativeLocation / keyRange, 0.5f);

                var lTp = leftKey.Transparency;
                var rTp = rightKey.Transparency;

                var alpha = 1f;

                alpha = TTMath.Lerp(lTp, rTp, keyRangeScalePotion);

                return alpha;
            }
        }

    }
}
