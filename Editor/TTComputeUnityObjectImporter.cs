using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using net.rs64.MultiLayerImage.LayerData;
using net.rs64.MultiLayerImage.Parser.PSD;
using net.rs64.TexTransUnityCore;
using Unity.Collections;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool
{
    [ScriptedImporter(1, new string[] { "ttcomp" }, new string[] { }, AllowCaching = true)]
    public class TTComputeUnityObjectImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var srcText = File.ReadAllText(ctx.assetPath);
            var lines = srcText.Split("\n");
            var pragmas = lines.Where(l => l.StartsWith("#pragma"));
            var pragmaKV = pragmas.Select(l => l.Split(" ")).Where(s => s.Length >= 2).ToDictionary(s => TTBlendUnityObjectImporter.RemoveControls(s[1]), s => s.Length > 2 ? TTBlendUnityObjectImporter.RemoveControls(s[2]) : null);

            TTBlendUnityObjectImporter.CheckUnityCGinc(lines);

            TTComputeUnityObject obj;
            switch (pragmaKV["TTComputeType"])
            {
                case "DownScaling":
                    {
                        var ds = ScriptableObject.CreateInstance<TTComputeDownScalingUnityObject>();
                        obj = ds;
                        ds.HasConsiderAlpha = pragmaKV.ContainsKey("ConsiderAlpha");
                        break;
                    }
                case "UpScaling":
                    {
                        var us = ScriptableObject.CreateInstance<TTComputeUpScalingUnityObject>();
                        obj = us;
                        us.HasConsiderAlpha = pragmaKV.ContainsKey("ConsiderAlpha");
                        break;
                    }
                case "GrabBlend":
                    {
                        var gb = ScriptableObject.CreateInstance<TTGrabBlendingUnityObject>();
                        obj = gb;
                        var colorSpaceDef = pragmaKV.GetValueOrDefault("ColorSpace");
                        if (colorSpaceDef is not null) { gb.IsLinerRequired = TTBlendUnityObjectImporter.RemoveControls(colorSpaceDef) == "Linear"; }// 他はガンマしかない
                        break;
                    }
                default:
                case "Operator": { obj = ScriptableObject.CreateInstance<TTComputeOperator>(); break; }
            }
            switch (obj)
            {
                case TTComputeDownScalingUnityObject:
                case TTComputeUpScalingUnityObject:
                    {
                        var hasConsiderAlpha = obj is TTComputeDownScalingUnityObject ds ? ds.HasConsiderAlpha : obj is TTComputeUpScalingUnityObject us ? us.HasConsiderAlpha : false;

                        var shaderCode = string.Join("\n", lines.Where(l => l.StartsWith("#pragma") is false));
                        var shaderCodeWithoutConsiderAlpha = TTComputeUnityObject.KernelDefine + shaderCode;

                        if (hasConsiderAlpha)
                        {
                            var shaderCodeWithConsiderAlpha = TTComputeUnityObject.KernelDefine + TTComputeDownScalingUnityObject.ConsiderAlphaDefine + shaderCode;

                            var cs = obj.Compute = ShaderUtil.CreateComputeShaderAsset(ctx, shaderCodeWithoutConsiderAlpha);
                            var csa = ShaderUtil.CreateComputeShaderAsset(ctx, shaderCodeWithConsiderAlpha);

                            ctx.AddObjectToAsset("ComputeShaderWithoutConsiderAlpha", cs);
                            ctx.AddObjectToAsset("ComputeShaderWithConsiderAlpha", csa);

                            if (obj is TTComputeDownScalingUnityObject ds2) { ds2.WithConsiderShader = csa; }
                            if (obj is TTComputeUpScalingUnityObject us2) { us2.WithConsiderShader = csa; }
                        }
                        else
                        {
                            var cs = obj.Compute = ShaderUtil.CreateComputeShaderAsset(ctx, shaderCodeWithoutConsiderAlpha);
                            ctx.AddObjectToAsset("ComputeShaderWithoutConsiderAlpha", cs);
                        }

                        ctx.AddObjectToAsset("TTComputeUnityObject", obj);
                        ctx.SetMainObject(obj);

                        break;
                    }

                default:
                case TTGrabBlendingUnityObject:
                case TTComputeOperator:
                    {
                        var shaderCode = string.Join("\n", lines.Where(l => l.StartsWith("#pragma") is false));
                        shaderCode = TTComputeUnityObject.KernelDefine + shaderCode;

                        var cs = obj.Compute = ShaderUtil.CreateComputeShaderAsset(ctx, shaderCode);
                        ctx.AddObjectToAsset("ComputeShader", cs);
                        ctx.AddObjectToAsset("TTComputeUnityObject", obj);
                        ctx.SetMainObject(obj);

                        break;
                    }
            }

        }
    }
}
