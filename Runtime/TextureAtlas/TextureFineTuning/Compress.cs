using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    [Serializable]
    public class Compress : ITextureFineTuning
    {
        public FormatQuality FormatQualityValue = FormatQuality.Normal;
        public bool UseOverride = false;
        public TextureFormat OverrideTextureFormat = TextureFormat.BC7;
        [Range(0, 100)] public int CompressionQuality = 50;

        [Obsolete("V4SaveData", true)] public PropertyName PropertyNames = PropertyName.DefaultValue;
        public List<PropertyName> PropertyNameList = new() { PropertyName.DefaultValue };
        public PropertySelect Select = PropertySelect.Equal;

        public Compress() { }
        [Obsolete("V4SaveData", true)]
        public Compress(FormatQuality formatQuality, bool overrideFormat, TextureFormat overrideTextureFormat, int compressionQuality, PropertyName propertyNames, PropertySelect select)
        {
            FormatQualityValue = formatQuality;
            UseOverride = overrideFormat;
            OverrideTextureFormat = overrideTextureFormat;
            CompressionQuality = compressionQuality;
            PropertyNames = propertyNames;
            Select = select;
        }
        public Compress(FormatQuality formatQuality, bool overrideFormat, TextureFormat overrideTextureFormat, int compressionQuality, List<PropertyName> propertyNames, PropertySelect select)
        {
            FormatQualityValue = formatQuality;
            UseOverride = overrideFormat;
            OverrideTextureFormat = overrideTextureFormat;
            CompressionQuality = compressionQuality;
            PropertyNameList = propertyNames;
            Select = select;
        }

        public void AddSetting(Dictionary<string, TexFineTuningHolder> texFineTuningTargets)
        {
            foreach (var target in FineTuningUtil.FilteredTarget(PropertyNameList, Select, texFineTuningTargets))
            {
                var tuningHolder = target.Value;
                var compressionQualityData = tuningHolder.Get<TextureCompressionData>();

                compressionQualityData.FormatQualityValue = FormatQualityValue;
                compressionQualityData.CompressionQuality = CompressionQuality;

                compressionQualityData.UseOverride = UseOverride;
                compressionQualityData.OverrideTextureFormat = OverrideTextureFormat;

            }
        }

    }

    public enum FormatQuality
    {
        None,
        Low,
        Normal,
        High,
    }
    [Serializable]
    public class TextureCompressionData : ITuningData, ITTTextureFormat
    {
        public FormatQuality FormatQualityValue = FormatQuality.Normal;

        public bool UseOverride = false;
        public TextureFormat OverrideTextureFormat = TextureFormat.BC7;

        [Range(0, 100)] public int CompressionQuality = 50;

        public virtual (TextureFormat CompressFormat, int Quality) Get(Texture2D texture2D)
        {
            if (UseOverride) { return (OverrideTextureFormat, CompressionQuality); }

#if UNITY_STANDALONE_WIN
            var hasAlpha = HasAlphaChannel(texture2D).GetResult();
#else
            var hasAlpha = true;
#endif
            TextureFormat textureFormat = GetQuality2TextureFormat(FormatQualityValue, hasAlpha);
            return (textureFormat, CompressionQuality);
        }

        public static TextureFormat GetQuality2TextureFormat(FormatQuality formatQualityValue, bool hasAlpha)
        {
            var textureFormat = TextureFormat.RGBA32;
#if UNITY_STANDALONE_WIN
            switch (formatQualityValue, hasAlpha)
            {
                case (FormatQuality.None, false):
                case (FormatQuality.None, true):
                    textureFormat = TextureFormat.RGBA32;
                    break;
                case (FormatQuality.Low, false):
                case (FormatQuality.Normal, false):
                    textureFormat = TextureFormat.DXT1;
                    break;
                default:
                case (FormatQuality.Low, true):
                case (FormatQuality.Normal, true):
                    textureFormat = TextureFormat.DXT5;
                    break;
                case (FormatQuality.High, false):
                case (FormatQuality.High, true):
                    textureFormat = TextureFormat.BC7;
                    break;
            }
#elif UNITY_ANDROID
            switch (formatQualityValue)
            {
                case FormatQuality.None:
                    textureFormat = TextureFormat.RGBA32;
                    break;
                case FormatQuality.Low:
                    textureFormat = TextureFormat.ASTC_8x8;
                    break;
                default:
                case FormatQuality.Normal:
                    textureFormat = TextureFormat.ASTC_6x6;
                    break;
                case FormatQuality.High:
                    textureFormat = TextureFormat.ASTC_4x4;
                    break;
            }
#endif
            return textureFormat;
        }

        public static AlphaContainsResult HasAlphaChannel(Texture2D texture2D)
        {
            if (GraphicsFormatUtility.HasAlphaChannel(texture2D.format) is false) { return new(false); }

            switch (texture2D.format)
            {
                default: return new(true);
                case TextureFormat.RGBA32:
                    {
                        var res = new NativeArray<bool>(1, Allocator.TempJob);
                        var na = new NativeArray<Color32>(texture2D.GetRawTextureData<Color32>(), Allocator.TempJob);
                        return new(res, new AlphaContainsRGBA32() { Bytes = na, Result = res }.Schedule());
                    }
                case TextureFormat.RGBA64:
                    {
                        var res = new NativeArray<bool>(1, Allocator.TempJob);
                        var na = new NativeArray<Color64>(texture2D.GetRawTextureData<Color64>(), Allocator.TempJob);
                        return new(res, new AlphaContainsRGBA64() { Bytes = na, Result = res }.Schedule());
                    }
                case TextureFormat.RGBAHalf:
                    {
                        var res = new NativeArray<bool>(1, Allocator.TempJob);
                        var na = new NativeArray<Color64>(texture2D.GetRawTextureData<Color64>(), Allocator.TempJob);
                        return new(res, new AlphaContainsRGBAHalf() { Bytes = na, Result = res }.Schedule());
                    }
                case TextureFormat.RGBAFloat:
                    {
                        var res = new NativeArray<bool>(1, Allocator.TempJob);
                        var na = new NativeArray<Color>(texture2D.GetRawTextureData<Color>(), Allocator.TempJob);
                        return new(res, new AlphaContainsRGBAFloat() { Bytes = na, Result = res }.Schedule());
                    }
            }
        }
        public struct AlphaContainsResult : IDisposable
        {
            JobHandle jobHandle;
            NativeArray<bool> result;
            bool? value;
            public AlphaContainsResult(bool val)
            {
                result = default;
                jobHandle = default;
                value = val;
            }
            public AlphaContainsResult(NativeArray<bool> res, JobHandle handle)
            {
                result = res;
                jobHandle = handle;
                value = null;
            }

            public bool GetResult()
            {
                if (value is null)
                {
                    jobHandle.Complete();
                    value = result[0];
                    result.Dispose();
                }
                return value.Value;
            }
            public void Dispose() { GetResult(); }

        }

        struct AlphaContainsRGBA32 : IJob
        {
            [ReadOnly][DeallocateOnJobCompletion] public NativeArray<Color32> Bytes;
            [WriteOnly] public NativeArray<bool> Result;
            public void Execute()
            {
                var containsAlpha = false;
                for (var i = 0; Bytes.Length > i; i += 1)
                {
                    // 基本的に 0.01 の誤差を受け入れる方針です。
                    if (Bytes[i].a >= (byte.MaxValue - 3) is false) { containsAlpha = true; }
                }
                Result[0] = containsAlpha;
            }
        }
        struct AlphaContainsRGBA64 : IJob
        {
            [ReadOnly][DeallocateOnJobCompletion] public NativeArray<Color64> Bytes;
            [WriteOnly] public NativeArray<bool> Result;
            public void Execute()
            {
                var containsAlpha = false;
                for (var i = 0; Bytes.Length > i; i += 1)
                {
                    if (Bytes[i].a >= (ushort.MaxValue - 655) is false) { containsAlpha = true; }
                }
                Result[0] = containsAlpha;
            }
        }
        struct Color64
        {
            public ushort r;
            public ushort g;
            public ushort b;
            public ushort a;
        }
        struct AlphaContainsRGBAHalf : IJob
        {
            [ReadOnly][DeallocateOnJobCompletion] public NativeArray<Color64> Bytes;
            [WriteOnly] public NativeArray<bool> Result;
            public void Execute()
            {
                var containsAlpha = false;
                for (var i = 0; Bytes.Length > i; i += 1)
                {
                    if (Unity.Mathematics.math.f16tof32(Bytes[i].a) >= (1f - 0.01f) is false) { containsAlpha = true; }
                }
                Result[0] = containsAlpha;
            }
        }
        struct AlphaContainsRGBAFloat : IJob
        {
            [ReadOnly][DeallocateOnJobCompletion] public NativeArray<Color> Bytes;
            [WriteOnly] public NativeArray<bool> Result;
            public void Execute()
            {
                var containsAlpha = false;
                for (var i = 0; Bytes.Length > i; i += 1)
                {

                    if (Bytes[i].a >= (1f - 0.01f) is false) { containsAlpha = true; }
                }
                Result[0] = containsAlpha;
            }
        }

    }
    internal class CompressionQualityApplicant : ITuningApplicant
    {
        public int Order => 0;

        public void ApplyTuning(Dictionary<string, TexFineTuningHolder> texFineTuningTargets, IDeferTextureCompress compress)
        {
            foreach (var atlasTexFTData in texFineTuningTargets)
            {
                var compressSetting = atlasTexFTData.Value.Find<TextureCompressionData>();
                if (compressSetting == null) { continue; }

                compress.DeferredTextureCompress(compressSetting, atlasTexFTData.Value.Texture2D);
            }
        }
    }

}
