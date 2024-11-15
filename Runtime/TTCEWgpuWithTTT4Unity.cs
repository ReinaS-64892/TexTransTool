#if CONTAINS_TTCE_WGPU
#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForWgpu;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEditor;

using Debug = UnityEngine.Debug;

namespace net.rs64.TexTransTool
{

    public class TTCEWgpuWithTTT4Unity : TTCEWgpu, ITexTransToolForUnity
    {
        public ShaderFinder.ShaderDictionary ShaderDictionary = null!;//気を付けるようにね！
        public ITexTransStandardComputeKey StandardComputeKey => ShaderDictionary;

        public ITexTransComputeKeyDictionary<string> GrabBlend => ShaderDictionary;

        public ITexTransComputeKeyDictionary<ITTBlendKey> BlendKey => ShaderDictionary;

        public ITexTransComputeKeyDictionary<string> GenealCompute => ShaderDictionary.GenealCompute;

        public ITTBlendKey QueryBlendKey(string blendKeyName) => ShaderDictionary.QueryBlendKey(blendKeyName);
        private readonly Dictionary<TTTImportedCanvasDescription, ITTImportedCanvasSource> _canvasSource = new();

        public void LoadTexture(ITTRenderTexture writeTarget, ITTDiskTexture diskTexture)
        {
            if (writeTarget.Width != diskTexture.Width || writeTarget.Hight != diskTexture.Hight) { throw new ArgumentException(); }
            switch (diskTexture)
            {
                case ImportedDiskTexture importedDiskTexture:
                    {
#if UNITY_EDITOR
                        if (!_canvasSource.ContainsKey(importedDiskTexture.ImportedImage.CanvasDescription)) { _canvasSource[importedDiskTexture.ImportedImage.CanvasDescription] = importedDiskTexture.ImportedImage.CanvasDescription.LoadCanvasSource(UnityEditor.AssetDatabase.GetAssetPath(importedDiskTexture.ImportedImage.CanvasDescription)); }
                        importedDiskTexture.ImportedImage.LoadImage(_canvasSource[importedDiskTexture.ImportedImage.CanvasDescription], this, writeTarget);
#endif
                        break;
                    }
                case RenderTextureAsDiskTexture rtAsDisk:
                    {
                        CopyRenderTexture(writeTarget, rtAsDisk.TTRenderTexture);
                        break;
                    }
            }
        }

        public ITTDiskTexture Wrapping(TTTImportedImage imported)
        {
            return new ImportedDiskTexture(imported);
        }

        class ImportedDiskTexture : ITTDiskTexture
        {
            public readonly TTTImportedImage ImportedImage;

            public ImportedDiskTexture(TTTImportedImage importedImage)
            {
                ImportedImage = importedImage;
            }

            public int Width => ImportedImage.CanvasDescription.Width;

            public int Hight => ImportedImage.CanvasDescription.Height;

            public string Name { get => ImportedImage.name; set { } }

            public void Dispose() { }
        }
    }


    public static class ShaderFinder
    {
        public static ShaderDictionary RegisterShaders(this TTCEWgpuDevice device)
        {
            var shaderDicts = new Dictionary<TTComputeType, Dictionary<string, TTComputeShaderID>>();
            var ttcompPaths = GetAllShaderPathWithCurrentDirectory().ToArray();
            foreach (var path in ttcompPaths)
            {
                var computeName = Path.GetFileNameWithoutExtension(path);

                var srcText = File.ReadAllText(path);
                if (srcText.Contains("UnityCG.cginc")) { throw new InvalidDataException(" UnityCG.cginc は使用してはいけません！"); }

                var descriptions = TTComputeShaderUtility.Parse(srcText);
                if (descriptions is null) { continue; }

                if (shaderDicts.ContainsKey(descriptions.ComputeType) is false) { shaderDicts[descriptions.ComputeType] = new(); }

                switch (descriptions.ComputeType)
                {
                    case TTComputeType.General:
                        {
                            shaderDicts[descriptions.ComputeType][computeName] = device.RegisterComputeShaderFromHLSL(path);
                            break;
                        }
                    case TTComputeType.GrabBlend:
                        {
                            shaderDicts[descriptions.ComputeType][computeName] = device.RegisterComputeShaderFromHLSL(path);
                            break;
                        }
                    case TTComputeType.Blending:
                        {
                            var blendKey = descriptions["Key"];
                            var csCode = TexTransCoreEngineForUnity.TTBlendingComputeShader.KernelDefine + srcText + TTComputeShaderUtility.BlendingShaderTemplate;
                            shaderDicts[descriptions.ComputeType][blendKey] = device.RegisterComputeShaderFromHLSL(path, csCode);

                            break;
                        }
                }
            }

            return new(shaderDicts);
        }

        public static IEnumerable<string> GetAllShaderPathWithCurrentDirectory()
        {
            return Directory.GetFiles(Directory.GetCurrentDirectory(), "*.ttcomp", SearchOption.AllDirectories).Concat(Directory.GetFiles(Directory.GetCurrentDirectory(), "*.ttblend", SearchOption.AllDirectories));
        }

        public class ShaderDictionary : ITexTransStandardComputeKey, ITexTransComputeKeyDictionary<string>, ITexTransComputeKeyDictionary<ITTBlendKey>
        {
            private Dictionary<TTComputeType, Dictionary<string, TTComputeShaderID>> _shaderDict;


            public ITTComputeKey this[string key] => _shaderDict[TTComputeType.GrabBlend][key];

            public ITTComputeKey this[ITTBlendKey key] => ((BlendKey)key).ComputeKey;

            public ITTComputeKey AlphaFill { get; private set; }
            public ITTComputeKey AlphaCopy { get; private set; }
            public ITTComputeKey AlphaMultiply { get; private set; }
            public ITTComputeKey AlphaMultiplyWithTexture { get; private set; }
            public ITTComputeKey ColorFill { get; private set; }
            public ITTComputeKey ColorMultiply { get; private set; }
            public ITTComputeKey BilinearReScaling { get; private set; }
            public ITTComputeKey GammaToLinear { get; private set; }
            public ITTComputeKey LinearToGamma { get; private set; }

            public ITTComputeKey Swizzling { get; private set; }

            public ITTBlendKey QueryBlendKey(string blendKeyName)
            {
                return new BlendKey(_shaderDict[TTComputeType.Blending][blendKeyName]);
            }

            public ITexTransComputeKeyDictionary<string> GenealCompute { get; private set; }

            public ShaderDictionary(Dictionary<TTComputeType, Dictionary<string, TTComputeShaderID>> dict)
            {
                _shaderDict = dict;
                AlphaFill = _shaderDict[TTComputeType.General][nameof(AlphaFill)];
                AlphaCopy = _shaderDict[TTComputeType.General][nameof(AlphaCopy)];
                AlphaMultiply = _shaderDict[TTComputeType.General][nameof(AlphaMultiply)];
                AlphaMultiplyWithTexture = _shaderDict[TTComputeType.General][nameof(AlphaMultiplyWithTexture)];
                ColorFill = _shaderDict[TTComputeType.General][nameof(ColorFill)];
                ColorMultiply = _shaderDict[TTComputeType.General][nameof(ColorMultiply)];
                BilinearReScaling = _shaderDict[TTComputeType.General][nameof(BilinearReScaling)];
                GammaToLinear = _shaderDict[TTComputeType.General][nameof(GammaToLinear)];
                LinearToGamma = _shaderDict[TTComputeType.General][nameof(LinearToGamma)];
                Swizzling = _shaderDict[TTComputeType.General][nameof(Swizzling)];
                GenealCompute = new GeneralComputeObject(_shaderDict[TTComputeType.General]);
            }

            class GeneralComputeObject : ITexTransComputeKeyDictionary<string>
            {
                private Dictionary<string, TTComputeShaderID> dictionary;

                public GeneralComputeObject(Dictionary<string, TTComputeShaderID> dictionary)
                {
                    this.dictionary = dictionary;
                }

                public ITTComputeKey this[string key] => dictionary[key];
            }
        }

        class BlendKey : ITTBlendKey
        {
            public ITTComputeKey ComputeKey;

            public BlendKey(ITTComputeKey computeKey)
            {
                ComputeKey = computeKey;
            }
        }
    }



#if UNITY_EDITOR
    internal static class TTCEWgpuRustCoreDebugUnityProxy
    {
        static Queue<string> s_logQueue = new();
        static Thread s_maiThread = null!; // Unityの Debug.Log はスレッドセーフっぽいけど MainThread が ブロックされてると死ぬらしい
        [InitializeOnLoadMethod]
        internal static void Init()
        {
            TTCEWgpuRustCoreDebug.LogHandlerInitialize();

            s_maiThread = Thread.CurrentThread;
            EditorApplication.update += QueueToLog;

            TTCEWgpuRustCoreDebug.DebugLog += DebugLogWithUnity;

            AssemblyReloadEvents.beforeAssemblyReload += TTCEWgpuRustCoreDebug.LogHandlerDeInitialize;
        }

        static void DebugLogWithUnity(string str)
        {
            var logStr = "[TTCEWgpuRustCodeDebug]:" + str;
            if (s_maiThread == Thread.CurrentThread) { Debug.Log(logStr); }
            else { s_logQueue.Enqueue("[AnotherCall]" + logStr + "\n\n" + new StackTrace().ToString()); }
        }

        static void QueueToLog()
        {
            while (s_logQueue.TryDequeue(out var str)) { Debug.Log(str); }
        }

    }
#endif


}
#endif
