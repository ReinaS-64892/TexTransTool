// https://github.com/anatawa12/AvatarOptimizer/blob/6a63910a423d5b7d73e726fcccb4940716f5ee0d/Editor/APIInternal/ShaderInformation.VRCSDK.cs

using net.rs64.TexTransCoreEngineForUnity;
using UnityEngine;


namespace net.rs64.TexTransTool.TextureAtlas.AAOCode
{
    // VRChat SDK Mobile Shaders
    internal class VRCSDKStandardLiteShaderInformation : ITTShaderTextureUsageInformation
    {

        internal static void Register()
        {
            var information = new VRCSDKStandardLiteShaderInformation();
            var shader = TexTransCoreRuntime.LoadAsset("0b7113dea2069fc4e8943843eff19f70", typeof(Shader)) as Shader;
            if (shader == null) { return; }
            TTShaderTextureUsageInformationRegistry.RegisterTTShaderTextureUsageInformation(shader, information);
        }
        public void GetMaterialTextureUVUsage(ITTTextureUVUsageWriter writer)
        {
            GetMaterialInformation(new MaterialInformationTranslator(writer));
        }
        public void GetMaterialInformation(IMaterialInformationCallbackAbstractionInterface matInfo)
        {
            var mainTexST = matInfo.GetVector("_MainTex_ST");
            Matrix2x3? mainTexSTMat = mainTexST is { } st ? Matrix2x3.NewScaleOffset(st) : null;

            matInfo.RegisterTextureUVUsage("_MetallicGlossMap", "_MetallicGlossMap", UsingUVChannels.UV0, mainTexSTMat);
            matInfo.RegisterTextureUVUsage("_MainTex", "_MainTex", UsingUVChannels.UV0, mainTexSTMat);
            matInfo.RegisterTextureUVUsage("_BumpMap", "_BumpMap", UsingUVChannels.UV0, mainTexSTMat);
            matInfo.RegisterTextureUVUsage("_OcclusionMap", "_OcclusionMap", UsingUVChannels.UV0, mainTexSTMat);
            matInfo.RegisterTextureUVUsage("_EmissionMap", "_EmissionMap", UsingUVChannels.UV0, mainTexSTMat);
            matInfo.RegisterTextureUVUsage("_DetailMask", "_DetailMask", UsingUVChannels.UV0, mainTexSTMat);

            var detailMapST = matInfo.GetVector("_DetailAlbedoMap_ST");
            Matrix2x3? detailMapSTMat = detailMapST is { } st2 ? Matrix2x3.NewScaleOffset(st2) : null;
            matInfo.RegisterTextureUVUsage("_DetailAlbedoMap", "_DetailAlbedoMap", UsingUVChannels.UV0, mainTexSTMat);

            var detailMapUV = matInfo.GetFloat("_UVSec") switch
            {
                null => UsingUVChannels.UV0 | UsingUVChannels.UV1,
                0 => UsingUVChannels.UV0,
                _ => UsingUVChannels.UV1,
            };

            matInfo.RegisterTextureUVUsage("_DetailAlbedoMap", "_DetailAlbedoMap", detailMapUV, detailMapSTMat);
            matInfo.RegisterTextureUVUsage("_DetailNormalMap", "_DetailNormalMap", detailMapUV, detailMapSTMat);
        }
    }

    internal class VRCSDKToonLitShaderInformation : ITTShaderTextureUsageInformation
    {
        internal static void Register()
        {
            var information = new VRCSDKToonLitShaderInformation();
            var shader = TexTransCoreRuntime.LoadAsset("affc81f3d164d734d8f13053effb1c5c", typeof(Shader)) as Shader;
            if (shader == null) { return; }
            TTShaderTextureUsageInformationRegistry.RegisterTTShaderTextureUsageInformation(shader, information);
        }
        public void GetMaterialTextureUVUsage(ITTTextureUVUsageWriter writer)
        {
            GetMaterialInformation(new MaterialInformationTranslator(writer));
        }
        public void GetMaterialInformation(IMaterialInformationCallbackAbstractionInterface matInfo)
        {
            var mainTexST = matInfo.GetVector("_MainTex_ST");
            Matrix2x3? mainTexSTMat = mainTexST is { } st ? Matrix2x3.NewScaleOffset(st) : null;
            matInfo.RegisterTextureUVUsage("_MainTex", "_MainTex", UsingUVChannels.UV0, mainTexSTMat);
        }
    }
}
