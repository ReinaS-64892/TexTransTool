using UnityEngine;
using System;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;
using System.Collections.Generic;
using UnityEngine.Serialization;
using net.rs64.TexTransTool.TextureAtlas.IslandRelocator;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransTool.TextureAtlas.IslandFineTuner;
using net.rs64.TexTransCore.UVIsland;

namespace net.rs64.TexTransTool.TextureAtlas
{
    [Serializable]
    public partial class AtlasSetting
    {
        [PowerOfTwo] public int AtlasTextureSize = 2048;

        public bool CustomAspect = false;
        [PowerOfTwo] public int AtlasTextureHeightSize = 2048;

        public UVChannel AtlasTargetUVChannel = UVChannel.UV0;


        public bool UsePrimaryMaximumTexture;
        public PropertyName PrimaryTextureProperty = PropertyName.DefaultValue;

        [Range(0f, 0.05f)] public float IslandPadding = 0.01f;
        [FormerlySerializedAs("IncludeDisableRenderer")] public bool IncludeDisabledRenderer = false;
        public bool ForceSizePriority = false;
        public Color BackGroundColor = Color.white;
        [SerializeReference] internal List<IIslandFineTuner> IslandFineTuners = new();


        public PropertyBakeSetting PropertyBakeSetting = PropertyBakeSetting.NotBake;
        public bool ForceSetTexture = false;
        public bool PixelNormalize = true;


        [SerializeReference, SubclassSelector] public IIslandRelocatorProvider AtlasIslandRelocator = null;
        public bool WriteOriginalUV = false;
        [Range(0, 7)] public int OriginalUVWriteTargetChannel = 1;


        [SerializeReference, SubclassSelector]
        public List<ITextureFineTuning> TextureFineTuning = new List<ITextureFineTuning> {
            new FineTuning.Resize(){
                Size = 512,
                PropertyNameList = new(){
                    new("_MainTex"),
                    new PropertyName("_BumpMap").AsLilToon(),
                    new PropertyName("_Bump2ndMap").AsLilToon(),
                },
                Select = PropertySelect.NotEqual
            },
            new FineTuning.ReferenceCopy(){
                SourcePropertyName = new("_MainTex"),
                TargetPropertyNameList = new(){
                    new PropertyName("_BaseColorMap").AsUnknown(),
                    new PropertyName("_BaseMap").AsUnknown(),
                },
            },
            new FineTuning.ColorSpace(){
                PropertyNameList = new(){
                    new PropertyName("_BumpMap").AsLilToon(),
                    new PropertyName("_Bump2ndMap").AsLilToon(),
                },
                Select = PropertySelect.Equal,
                AsLinear = true,
            },
            new FineTuning.Compress(){// NormalMap は BC5(RG) のものが良いが、ASTCなどの環境でデフォルトで壊れてしまうから仕方がなく High(PCだと BC7 )にすることで誤魔化す
                PropertyNameList = new(){
                    new PropertyName("_BumpMap").AsLilToon(),
                    new PropertyName("_Bump2ndMap").AsLilToon(),
                },
                Select = PropertySelect.Equal,
                FormatQualityValue = FormatQuality.High,
                CompressionQuality = 100,
                UseOverride = false,
            },
            };
        public List<TextureIndividualTuning> TextureIndividualFineTuning = new();

        public string DownScaleAlgorithm = ITexTransToolForUnity.DS_ALGORITHM_DEFAULT;

        public bool AutoTextureSizeSetting = false;
        public bool AutoReferenceCopySetting = false;
        public bool AutoMergeTextureSetting = false;

        public List<TextureSelector> UnsetTextures = new();
    }
    public interface IIslandRelocatorProvider
    {
        IIslandRelocator GetIslandRelocator();
    }

    public enum PropertyBakeSetting
    {
        NotBake,
        Bake,
        BakeAllProperty,
    }
    [Serializable]
    public class TextureIndividualTuning
    {
        public string TuningTarget;

        [FormerlySerializedAs("OverrideAsMargeTexture")] public bool OverrideReferenceCopy = false;
        public string CopyReferenceSource = PropertyName.DefaultValue;

        public bool OverrideResize = false;
        [PowerOfTwo] public int TextureSize = 512;
        public string DownScaleAlgorithm = ITexTransToolForUnity.DS_ALGORITHM_DEFAULT;

        public bool OverrideCompression = false;
        public TextureCompressionData CompressionData = new();

        public bool OverrideMipMapRemove = false;
        public bool UseMipMap = true;

        public bool OverrideColorSpace = false;
        [FormerlySerializedAs("Linear")] public bool AsLinear = false;

        public bool OverrideRemove = false;
        public bool IsRemove = false;

        [FormerlySerializedAs("OverrideAsMargeTexture")] public bool OverrideMargeTexture = false;
        public string MargeRootProperty;
    }

}
