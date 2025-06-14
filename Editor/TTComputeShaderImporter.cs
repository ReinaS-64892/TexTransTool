#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [ScriptedImporter(2, new string[] { "ttcomp", "ttblend" }, new string[] { }, AllowCaching = false)]
    public class TTComputeShaderImporter : ScriptedImporter
    {
        private static string FindTemplate(string fileName)
        {
            var candidates = Directory.GetFiles("./", fileName, SearchOption.AllDirectories);
            return candidates.First(s => (s.Contains("Tex") && s.Contains("Trans")) || (s.Contains("tex") && s.Contains("trans")));
        }
        static string? _textureResizingTemplatePath;
        static string TextureResizingTemplatePath => _textureResizingTemplatePath ??= FindTemplate("TextureResizingTemplate.hlsl");

        static string? _transSamplingTemplatePath;
        static string TransSamplingTemplatePath => _transSamplingTemplatePath ??= FindTemplate("TransSamplingTemplate.hlsl");

        static string? _atlasSamplingTemplatePath;
        static string AtlasSamplingTemplatePath => _atlasSamplingTemplatePath ??= FindTemplate("AtlasSamplingTemplate.hlsl");
        const string INCLUDE_SAMPLER_TEMPLATE_LIEN = "#include \"SamplerTemplate.hlsl\"";

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var srcText = File.ReadAllText(ctx.assetPath);
            if (srcText.Contains("UnityCG.cginc")) { throw new InvalidDataException(" UnityCG.cginc は使用してはいけません！"); }
            var descriptions = TTComputeShaderUtility.Parse(srcText);
            if (descriptions is null) { return; }

            var computeName = Path.GetFileNameWithoutExtension(ctx.assetPath);
            if (descriptions.ComputeType is not TTComputeType.Blending && Path.GetExtension(ctx.assetPath) == ".ttblend") { throw new InvalidDataException("拡張子 .ttblend は TTComputeType が Blending の場合にのみ許可されます。"); }

            switch (descriptions.ComputeType)
            {
                case TTComputeType.General:
                    {
                        var op = ScriptableObject.CreateInstance<TTGeneralComputeOperator>();
                        op.name = computeName;

                        var cs = op.Compute = ShaderUtil.CreateComputeShaderAsset(ctx, TTComputeUnityObject.KernelDefine + srcText);
                        ctx.AddObjectToAsset("ComputeShader", cs);
                        ctx.AddObjectToAsset("TTGeneralComputeOperator", op);
                        ctx.SetMainObject(op);
                        break;
                    }
                case TTComputeType.GrabBlend:
                    {
                        var op = ScriptableObject.CreateInstance<TTGrabBlendingComputeShader>();
                        op.name = computeName;

                        op.IsLinerRequired = descriptions["ColorSpace"] == "Linear";

                        var cs = op.Compute = ShaderUtil.CreateComputeShaderAsset(ctx, TTComputeUnityObject.KernelDefine + srcText);
                        ctx.AddObjectToAsset("ComputeShader", cs);
                        ctx.AddObjectToAsset("TTGrabBlendingComputeShader", op);
                        ctx.SetMainObject(op);
                        break;
                    }
                case TTComputeType.Blending:
                    {
                        var op = ScriptableObject.CreateInstance<TTBlendingComputeShader>();
                        op.name = computeName;

                        op.IsLinerRequired = descriptions["ColorSpace"] == "Linear";
                        op.BlendTypeKey = descriptions["Key"];

                        op.Locales = new();
                        foreach (var kv in descriptions.FindAll("KeyName"))
                        {
                            var (langCode, displayName) = TTComputeShaderUtility.GetKeyValueString(kv);
                            op.Locales.Add(new() { LangCode = langCode, DisplayName = displayName });
                        }

                        var csCode = TTComputeUnityObject.KernelDefine + srcText + TTComputeShaderUtility.BlendingShaderTemplate;
                        // var shaderName = "Hidden/" + op.BlendTypeKey;
                        // var scCode = TTBlendingComputeShader.ShaderNameDefine + shaderName + TTBlendingComputeShader.ShaderDefine + srcText + (op.IsLinerRequired ? TTBlendingComputeShader.ShaderTemplateWithLinear : TTBlendingComputeShader.ShaderTemplate);

                        var cs = op.Compute = ShaderUtil.CreateComputeShaderAsset(ctx, csCode);
                        // var sc = op.Shader = ShaderUtil.CreateShaderAsset(ctx, scCode, true);
                        ctx.AddObjectToAsset("ComputeShader", cs);
                        // ctx.AddObjectToAsset("BlendingShader", sc);
                        ctx.AddObjectToAsset("TTGrabBlendingComputeShader", op);
                        ctx.SetMainObject(op);

                        break;
                    }
                case TTComputeType.Sampler:
                    {
                        var op = ScriptableObject.CreateInstance<TTSamplerComputeShader>();
                        op.name = computeName;

                        var resizeTemplate = File.ReadAllText(TextureResizingTemplatePath);
                        var transSamplerTemplate = File.ReadAllText(TransSamplingTemplatePath);
                        var atlasSamplingTemplate = File.ReadAllText(AtlasSamplingTemplatePath);

                        // SamplerTemplate.hlsl の include の解決ができないゆえの苦肉の策
                        var resizingCode = TTComputeUnityObject.KernelDefine + resizeTemplate.Replace(INCLUDE_SAMPLER_TEMPLATE_LIEN, srcText);
                        var transSamplerCode = TTComputeUnityObject.KernelDefine + transSamplerTemplate.Replace(INCLUDE_SAMPLER_TEMPLATE_LIEN, srcText);
                        var atlasSamplerCode = TTComputeUnityObject.KernelDefine + atlasSamplingTemplate.Replace(INCLUDE_SAMPLER_TEMPLATE_LIEN, srcText);

                        var csr = op.ResizingCompute = ShaderUtil.CreateComputeShaderAsset(ctx, resizingCode);
                        var cst = op.TransSamplerCompute = ShaderUtil.CreateComputeShaderAsset(ctx, transSamplerCode);
                        var csa = op.AtlasSamplerCompute = ShaderUtil.CreateComputeShaderAsset(ctx, atlasSamplerCode);
                        ctx.AddObjectToAsset("ResizingCompute", csr);
                        ctx.AddObjectToAsset("TransSamplerCompute", cst);
                        ctx.AddObjectToAsset("AtlasSamplerCompute", csa);
                        ctx.AddObjectToAsset("TTSamplerComputeShader", op);
                        ctx.SetMainObject(op);
                        break;
                    }
            }
        }
    }
}
