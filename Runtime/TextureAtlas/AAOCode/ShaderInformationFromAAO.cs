#nullable enable
using System.Linq;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.AAOCode
{
    internal static class ShaderInformationFromAAO
    {
        [TexTransCoreEngineForUnity.TexTransInitialize]
        internal static void ShaderInformationRegistering()
        {
            var lilOutLine = new LiltoonShaderInformationWithOutline();
            var lil = new LiltoonShaderInformationWithOutOutline();
            foreach (var shader in TexTransCoreEngineForUnity.TexTransCoreRuntime.LoadAssetsAtType(typeof(Shader)).Cast<Shader>())
            {
                if (shader.name.Contains("lilToon"))
                {
                    if (shader.name.Contains("Outline")) { TTShaderTextureUsageInformationRegistry.RegisterTTShaderTextureUsageInformation(shader, lilOutLine); }
                    else { TTShaderTextureUsageInformationRegistry.RegisterTTShaderTextureUsageInformation(shader, lil); }
                }
            }

            StandardShaderInformation.Register();
            VRCSDKStandardLiteShaderInformation.Register();
        }
    }
}
