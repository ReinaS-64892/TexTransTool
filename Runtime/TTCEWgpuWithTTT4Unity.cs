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

    public class TTCEWgpuWithTTT4Unity : TTCEWgpuContextWithShaderDictionary, ITexTransToolForUnity
    {
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

#if UNITY_EDITOR
#if TTT_DEBUG
    internal static class TTCEWgpuRustCoreDebugUnityProxy
    {
        static Queue<string> s_logQueue = new();
        static Thread s_maiThread = null!; // Unityの Debug.Log はスレッドセーフっぽいけど MainThread が ブロックされてると死ぬらしい...多分
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
#endif


}
#endif
