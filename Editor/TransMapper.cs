using UnityEngine;
using UnityEditor;
using net.rs64.TexTransCore.TransTextureCore.TransCompute;

namespace net.rs64.TexTransTool
{
    public static class TransMapper
    {
        public const string TransCompilerPath = "Packages/net.rs64.tex-trans-tool/TexTransCore/TransTextureCore/ShaderAsset/Compute/TransCompiler.compute";
        public const string TransMapperPath = "Packages/net.rs64.tex-trans-tool/TexTransCore/TransTextureCore/ShaderAsset/Compute/TransMapper.compute";

        public static TransTextureCompute TransTextureCompute => new TransTextureCompute(
            AssetDatabase.LoadAssetAtPath<ComputeShader>(TransCompilerPath),
            AssetDatabase.LoadAssetAtPath<ComputeShader>(TransMapperPath));

        public const string BlendTextureCSPath = "Packages/net.rs64.tex-trans-tool/TexTransCore/TransTextureCore/ShaderAsset/Compute/BlendTexture.compute";

        public static ComputeShader BlendTextureCS => AssetDatabase.LoadAssetAtPath<ComputeShader>(BlendTextureCSPath);
    }


}