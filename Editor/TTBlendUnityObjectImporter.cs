using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using net.rs64.MultiLayerImage.LayerData;
using net.rs64.MultiLayerImage.Parser.PSD;
using net.rs64.TexTransCoreEngineForUnity;
using Unity.Collections;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool
{
    [ScriptedImporter(1, new string[] { "ttblend" }, new string[] { }, AllowCaching = true)]
    public class TTBlendUnityObjectImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var srcText = File.ReadAllText(ctx.assetPath);
            var lines = srcText.Split("\n");
            var pragmas = lines.Where(l => l.StartsWith("#pragma"));
            var pragmaKV = pragmas.Select(l => l.Split(" ")).Where(s => s.Length >= 3).ToDictionary(s => RemoveControls(s[1]), s => RemoveControls(s[2]));

            CheckUnityCGinc(lines);

            var obj = ScriptableObject.CreateInstance<TTBlendUnityObject>();
            obj.BlendTypeKey = pragmaKV["Key"];
            obj.BlendTypeKey = obj.BlendTypeKey.Substring(1, obj.BlendTypeKey.Length - 2);
            var colorSpaceDef = pragmaKV.GetValueOrDefault("ColorSpace");
            if (colorSpaceDef is not null) { obj.IsLinerRequired = RemoveControls(colorSpaceDef) == "Linear"; }// 他はガンマしかない

            obj.Locales = new();
            foreach (var kv in pragmaKV)
            {
                if (kv.Key.StartsWith("KeyName_"))
                {
                    var displayName = kv.Value;
                    displayName = displayName.Substring(1, displayName.Length - 2);
                    obj.Locales.Add(new() { LangCode = kv.Key.Replace("KeyName_", ""), DisplayName = displayName });
                }
            }

            var shaderCode = string.Join("\n", lines.Where(l => l.StartsWith("#pragma") is false));
            var csCode = TTBlendUnityObject.KernelDefine + shaderCode + TTBlendUnityObject.ComputeShaderTemplate;
            var scCode = TTBlendUnityObject.ShaderNameDefine + obj.BlendTypeKey + TTBlendUnityObject.ShaderDefine + shaderCode + (obj.IsLinerRequired ? TTBlendUnityObject.ShaderTemplateWithLinear : TTBlendUnityObject.ShaderTemplate);

            var cs = obj.Compute = ShaderUtil.CreateComputeShaderAsset(ctx, csCode);
            var sc = obj.Shader = ShaderUtil.CreateShaderAsset(ctx, scCode, true);
            ctx.AddObjectToAsset("BlendingComputeShader", cs);
            ctx.AddObjectToAsset("BlendingShader", sc);
            ctx.AddObjectToAsset("KeyObject", obj);
            ctx.SetMainObject(obj);

            // Debug.Log(shaderCode);
        }

        public static void CheckUnityCGinc(string[] lines)
        {
            if (lines.Where(l => l.StartsWith("#include")).Select(l => l.Split(" ")).Where(s => s.Length >= 2).Any(s => RemoveControls(s[1]) == "\"UnityCG.cginc\""))
            { throw new InvalidDataException(" UnityCG.cginc は使用してはいけませんん！"); }
        }

        public static string RemoveControls(string str)
        {
            return new string(str.Where(c => char.IsControl(c) is false).ToArray());
        }

    }
}
