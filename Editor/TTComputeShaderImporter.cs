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
    [ScriptedImporter(1, new string[] { "ttcomp", "ttblend" }, new string[] { }, AllowCaching = true)]
    public class TTComputeShaderImporter : ScriptedImporter
    {

        static string? _textureResizingTemplatePath;
        static string TextureResizingTemplatePath
        {
            get
            {
                if (_textureResizingTemplatePath is null)
                {
                    var candidates = Directory.GetFiles("./", "TextureResizingTemplate.hlsl", SearchOption.AllDirectories);
                    _textureResizingTemplatePath = candidates.First(s => s.Contains("TexTransCore"));
                }
                return _textureResizingTemplatePath;
            }
        }
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

                        var shaderName = "Hidden/" + op.BlendTypeKey;
                        var csCode = TTComputeUnityObject.KernelDefine + srcText + TTComputeShaderUtility.BlendingShaderTemplate;
                        var scCode = TTBlendingComputeShader.ShaderNameDefine + shaderName + TTBlendingComputeShader.ShaderDefine + srcText + (op.IsLinerRequired ? TTBlendingComputeShader.ShaderTemplateWithLinear : TTBlendingComputeShader.ShaderTemplate);

                        var cs = op.Compute = ShaderUtil.CreateComputeShaderAsset(ctx, csCode);
                        var sc = op.Shader = ShaderUtil.CreateShaderAsset(ctx, scCode, true);
                        ctx.AddObjectToAsset("ComputeShader", cs);
                        ctx.AddObjectToAsset("BlendingShader", sc);
                        ctx.AddObjectToAsset("TTGrabBlendingComputeShader", op);
                        ctx.SetMainObject(op);

                        break;
                    }
                case TTComputeType.Sampler:
                    {
                        var op = ScriptableObject.CreateInstance<TTSamplerComputeShader>();
                        op.name = computeName;

                        var template = File.ReadAllText(TextureResizingTemplatePath);

                        var resizingCode = TTComputeUnityObject.KernelDefine + template.Replace("//$$$SAMPLER_CODE$$$", srcText);

                        var cs = op.ResizingCompute = ShaderUtil.CreateComputeShaderAsset(ctx, resizingCode);
                        ctx.AddObjectToAsset("ResizingCompute", cs);
                        ctx.AddObjectToAsset("TTSamplerComputeShader", op);
                        ctx.SetMainObject(op);
                        break;
                    }
            }
        }
    }
}
