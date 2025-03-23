#nullable enable
#if UNITY_EDITOR_WIN
#define SYSTEM_DRAWING
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;
using System.Threading.Tasks;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using UnityEngine;
using net.rs64.TexTransTool.MultiLayerImage.Importer;
using UnityEngine.Experimental.Rendering;
using Unity.Jobs;
using Unity.Collections;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;

namespace net.rs64.TexTransTool
{
    internal static class TextureManagerUtility
    {
        public static TexTransToolTextureDescriptor GetTextureDescriptor(Texture2D texture)
        {
            var desc = new TexTransToolTextureDescriptor();
            desc.UseMipMap = texture.mipmapCount > 1;
            desc.TextureFormat = GetTTTextureFormat(texture);

            if (desc.TextureFormat is RefAtImporterFormat refAt)
                if (refAt.TextureImporter.textureType is TextureImporterType.NormalMap)
                    if (refAt.TextureFormat is TextureFormat.DXT5 or TextureFormat.DXT5Crunched)
                        refAt.TextureFormat = TextureFormat.BC5;

            desc.AsLinear = texture.isDataSRGB is false;

            desc.filterMode = texture.filterMode;
            desc.anisoLevel = texture.anisoLevel;
            desc.mipMapBias = texture.mipMapBias;
            desc.wrapModeU = texture.wrapModeU;
            desc.wrapModeV = texture.wrapModeV;
            desc.wrapModeW = texture.wrapModeW;
            desc.wrapMode = texture.wrapMode;

            return desc;
        }
        public static ITexTransToolTextureFormat GetTTTextureFormat(Texture2D texture2D)
        {
            static ITexTransToolTextureFormat GetDirect(Texture2D texture2D) { return new DirectFormat(texture2D.format, 50); }
            if (AssetDatabase.Contains(texture2D) is false) { return GetDirect(texture2D); }

            var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture2D));
            if (importer is not TextureImporter textureImporter) { return GetDirect(texture2D); }

            return new RefAtImporterFormat(texture2D.format, textureImporter);
        }
        internal class DirectFormat : ITexTransToolTextureFormat
        {
            public (TextureFormat CompressFormat, int Quality) format;
            public DirectFormat(TextureFormat compressFormat, int quality) { format = (compressFormat, quality); }
            public (TextureFormat CompressFormat, int Quality) Get(Texture2D texture2D) { return format; }
        }
        internal class RefAtImporterFormat : ITexTransToolTextureFormat
        {
            public TextureImporter TextureImporter;
            public TextureFormat TextureFormat;
            public RefAtImporterFormat(TextureFormat textureFormat, TextureImporter textureImporter)
            {
                TextureImporter = textureImporter;
                TextureFormat = textureFormat;
            }
            public (TextureFormat CompressFormat, int Quality) Get(Texture2D texture2D)
            {
                return (TextureFormat, TextureImporter.compressionQuality);
            }
        }
    }
    internal class RenderTextureDescriptorManager
    {
        private protected Dictionary<RenderTexture, ITTRenderTexture> _ref2RenderTexture = new();
        private protected Dictionary<ITTRenderTexture, TexTransToolTextureDescriptor> _descriptorRtDict = new();
        private readonly ITexTransToolForUnity _ttt4U;

        public RenderTextureDescriptorManager(ITexTransToolForUnity ttt4u)
        {
            _ttt4U = ttt4u;
        }

        public TexTransToolTextureDescriptor GetTextureDescriptor(Texture tex)
        {
            switch (tex)
            {
                default: throw new NotImplementedException();

                case Texture2D texture2D: { return TextureManagerUtility.GetTextureDescriptor(texture2D); }
                case RenderTexture rt:
                    {
                        var ttRenderTexture = _ref2RenderTexture[rt];
                        return new(_descriptorRtDict[ttRenderTexture]);
                    }
            }
        }
        public void RegisterPostProcessingAndLazyGPUReadBack(ITTRenderTexture rt, TexTransToolTextureDescriptor textureDescriptor)
        {
            var refUrt = _ttt4U.GetReferenceRenderTexture(rt);
            _ref2RenderTexture[refUrt] = rt;
            _descriptorRtDict[rt] = textureDescriptor;
        }

        public (
            Dictionary<Texture2D, TexTransToolTextureDescriptor> texDescDict
            , Dictionary<RenderTexture, Texture2D> textureReplace
            , IEnumerable<ITTRenderTexture> renderTextures
            ) DownloadTexture2D()
        {
            // TODO : 並列 ReadBack
            // TODO : MipMap の生成
            var replace = new Dictionary<RenderTexture, Texture2D>();
            var descDict = new Dictionary<Texture2D, TexTransToolTextureDescriptor>();
            foreach (var kv in _ref2RenderTexture)
            {
                var urt = kv.Key;
                var rt = kv.Value;
                var descriptor = _descriptorRtDict[rt];
                var tex2D = replace[urt] = _ttt4U.DownloadToTexture2D(rt, descriptor.UseMipMap, null, descriptor.AsLinear);
                descDict[tex2D] = descriptor;
                descriptor.WriteFillWarp(tex2D);
                tex2D.Apply(true);
            }
            var sourceRenderTextures = _descriptorRtDict.Keys.ToArray();

            _descriptorRtDict.Clear();
            _ref2RenderTexture.Clear();

            return (descDict, replace, sourceRenderTextures);
        }

    }
    internal class Texture2DCompressor
    {
        public readonly Dictionary<Texture2D, TexTransToolTextureDescriptor> TextureDescriptors;
        public Texture2DCompressor() { TextureDescriptors = new(); }
        public Texture2DCompressor(Dictionary<Texture2D, TexTransToolTextureDescriptor> td) { TextureDescriptors = td; }
        public void CompressDeferred(IRendererTargeting targeting)
        {
            var compressKV = TextureDescriptors.Where(i => i.Key != null).ToDictionary(i => i.Key, i => i.Value);// Unity が勝手にテクスチャを破棄してくる場合があるので Null が入ってないか確認する必要がある。


            var targetTextures = targeting.GetAllTextures().OfType<Texture2D>()
                .Where(t => t != null)
                .Distinct()
                .Select(t => (t, compressKV.FirstOrDefault(kv => targeting.OriginEqual(kv.Key, t))))
                .Where(kvp => kvp.Item2.Key is not null && kvp.Item2.Value is not null)
                .Select(kvp => (kvp.t, kvp.Item2.Value))
                .Where(kv => GraphicsFormatUtility.IsCompressedFormat(kv.t.format) is false)
                .ToArray();

            var needAlphaInfoTarget = new Dictionary<Texture2D, TextureCompressionData.AlphaContainsResult>();
            // ほかツールが増やした場合のために自分が情報を持っているやつから派生した場合にフォールバック設定で圧縮が行われる
            foreach (var (tex, fallBackCompressing) in targetTextures)
            {
                (TextureFormat CompressFormat, int Quality) GetCompressFormat(Texture2D tex, ITexTransToolTextureFormat fallBackCompressing)
                {
                    if (compressKV.TryGetValue(tex, out var compression)) return compression.TextureFormat.Get(tex);
                    else return fallBackCompressing.Get(tex);
                }
                var compressFormat = GetCompressFormat(tex, fallBackCompressing.TextureFormat);

                if (GraphicsFormatUtility.HasAlphaChannel(compressFormat.CompressFormat) is false)
                    needAlphaInfoTarget[tex] = TextureCompressionData.HasAlphaChannel(tex);

                EditorUtility.CompressTexture(tex, compressFormat.CompressFormat, compressFormat.Quality);
            }

            foreach (var tex in targetTextures) tex.t.Apply(false, true);

            foreach (var tex in targetTextures)
            {
                var sTexture = new SerializedObject(tex.t);

                var sStreamingMipmaps = sTexture.FindProperty("m_StreamingMipmaps");
                sStreamingMipmaps.boolValue = true;

                sTexture.ApplyModifiedPropertiesWithoutUndo();
            }

            TextureDescriptors.Clear();

            // アルファが存在するがフォーマット的に消えたやつら
            var alphaContainsFormatNeedTextures = needAlphaInfoTarget.Where(i => i.Value.GetResult()).ToArray();
            if (alphaContainsFormatNeedTextures.Any())
            {
                TTLog.Info("Common:info:AlphaContainsTextureCompressToAlphaMissingFormat", alphaContainsFormatNeedTextures.Select(i => i.Key));
            }
        }
    }


    internal class UnityDiskUtil : ITexTransUnityDiskUtil, IDisposable
    {
        private readonly bool IsPreview;

        public UnityDiskUtil(bool isPreview)
        {
            IsPreview = isPreview;
        }
        static ComputeShader CopyFromGammaTexture2D = null!;
        [TexTransInitialize]
        internal static void Init()
        { CopyFromGammaTexture2D = (ComputeShader)TexTransCoreRuntime.LoadAsset("b1cd01a41aef7f443bafb8684546de39", typeof(ComputeShader)); }

        public ITTDiskTexture Wrapping(Texture2D texture2D)
        {
            Func<(int x, int y)> func = () =>
            {
#if SYSTEM_DRAWING
                PreloadOriginalTexture(texture2D);
                if (_asyncOriginSize.TryGetValue(texture2D, out var size))
                {
                    return size;
                }
                else
                {
                    var originTex = GetOriginalTexture(texture2D);
                    return (originTex.width, originTex.height);
                }
#else
                var originTex = GetOriginalTexture(texture2D);
                return (originTex.width, originTex.height);
#endif
            };
            return new UnityDiskTexture(texture2D, func);
        }
        public ITTDiskTexture Wrapping(TTTImportedImage texture2D)
        { return new UnityImportedDiskTexture(texture2D, IsPreview); }

        public void LoadTexture(ITexTransToolForUnity ttce4u, ITTRenderTexture writeTarget, ITTDiskTexture diskTexture)
        {
            switch (diskTexture)
            {
                case UnityDiskTexture tex2DWrapper:
                    {
                        Graphics.Blit(GetOriginalTexture(tex2DWrapper.Texture), ttce4u.GetReferenceRenderTexture(writeTarget));
                        // sRGB なフォーマットだった場合は、(Unityが)勝手にリニア変換するお節介を逆補正する
                        // Texture.isDataSRGB は 16bit 等の SRGB ではないやつであっても、
                        // テクスチャ作成時の引数の isLiner が false だった場合に true になることがあるから この場合は 信用してはならない。
                        if (GraphicsFormatUtility.IsSRGBFormat(tex2DWrapper.Texture.graphicsFormat)) ttce4u.LinearToGamma(writeTarget);
                        break;
                    }
                case UnityImportedDiskTexture importedWrapper:
                    {
                        var texture = importedWrapper.Texture;
                        if (IsPreview)
                        {
                            CopyFromGammaTexture2D.SetTexture(0, "Source", CanvasImportedImagePreviewManager.GetPreview(texture));
                            CopyFromGammaTexture2D.SetTexture(0, "Dist", ttce4u.GetReferenceRenderTexture(writeTarget));
                            CopyFromGammaTexture2D.Dispatch(0, (writeTarget.Width + 31) / 32, (writeTarget.Hight + 31) / 32, 1);
                        }
                        else
                        {
                            if (!_canvasSource.ContainsKey(texture.CanvasDescription)) { _canvasSource[texture.CanvasDescription] = texture.CanvasDescription.LoadCanvasSource(AssetDatabase.GetAssetPath(texture.CanvasDescription)); }
                            texture.LoadImage(_canvasSource[texture.CanvasDescription], ttce4u, writeTarget);
                        }
                        break;
                    }
            }
        }


        private readonly Dictionary<Texture2D, Texture2D> _originDict = new();
        private readonly Dictionary<Texture2D, (bool isLoadableOrigin, bool IsNormalMap)> _originLoadInfoDict = new();
#if SYSTEM_DRAWING
        private readonly Dictionary<Texture2D, Task<Func<Texture2D>>> _asyncOriginLoaders = new();
        private readonly Dictionary<Texture2D, (int x, int y)> _asyncOriginSize = new();
#endif
        private readonly Dictionary<TTTImportedCanvasDescription, ITTImportedCanvasSource> _canvasSource = new();



        public void PreloadOriginalTexture(Texture2D texture2D)
        {
            if (IsPreview) { return; }
#if SYSTEM_DRAWING
            if (_originDict.ContainsKey(texture2D) || _asyncOriginLoaders.ContainsKey(texture2D)) return;
            _originLoadInfoDict[texture2D] = EditorTextureUtility.GetOriginInformation(texture2D);
            var task = EditorTextureUtility.AsyncGetUncompressed(texture2D);
            _asyncOriginLoaders[texture2D] = task.task;
            _asyncOriginSize[texture2D] = task.originalSize;
#endif
        }



        public Texture2D GetOriginalTexture(Texture2D texture2D)
        {
            if (IsPreview)
            {
                return texture2D;
            }
            else
            {
                if (_originDict.ContainsKey(texture2D) && _originDict[texture2D] != null)
                {
                    return _originDict[texture2D];
                }
#if SYSTEM_DRAWING
                else if (_asyncOriginLoaders.TryGetValue(texture2D, out var task))
                {
                    // 必要なテクスチャの読み込み終わってない間に他のテクスチャをApplyしましょう、と思いきや、それでは遅くなります。
                    // おそらく、まだ必要じゃないテクスチャのアップロードを待つことで、今できるBlitが後回しになっているからだと思われます。
                    // なので、テクスチャが初めて必要になったときにApplyする方針です。
                    if (!task.Wait(60_000))
                    {
                        // なんか壊れたらEditorが固まらないための安全装置
                        throw new TimeoutException("Async image loader timed out");
                    }
                    _originDict[texture2D] = task.Result();
                    _asyncOriginLoaders.Remove(texture2D);

                    return _originDict[texture2D];
                }
#endif
                else
                {
                    var originTex = EditorTextureUtility.TryGetUnCompress(texture2D);
                    _originDict[texture2D] = originTex;
                    _originLoadInfoDict[texture2D] = EditorTextureUtility.GetOriginInformation(texture2D);
                    return originTex;
                }
            }
        }

        public void Dispose()
        {
            foreach (var originTex in _originDict.Values) { UnityEngine.Object.DestroyImmediate(originTex); }
            _originDict.Clear();
            _originLoadInfoDict.Clear();
#if SYSTEM_DRAWING
            foreach (var task in _asyncOriginLoaders.Values)
            {
                try
                {
                    if (!task.Wait(60_000))
                    {
                        // なんか壊れたらEditorが固まらないための安全装置
                        throw new TimeoutException("Async image loader timed out");
                    }
                    var tex = task.Result();
                    UnityEngine.Object.DestroyImmediate(tex);
                }
                catch (Exception e) { TTLog.Exception(e); }
            }
            _asyncOriginLoaders.Clear();
            _asyncOriginSize.Clear();
#endif
        }

        internal class UnityDiskTexture : ITTDiskTexture
        {
            internal Texture2D Texture;
            internal Func<(int x, int y)> LoadedOriginalTextureSizeFunc;
            internal (int x, int y)? LoadedOriginalTextureSize;
            public UnityDiskTexture(Texture2D texture, Func<(int x, int y)> loadLoadableTextureSize)
            {
                Texture = texture;
                LoadedOriginalTextureSizeFunc = loadLoadableTextureSize;
            }
            public int Width => (LoadedOriginalTextureSize ??= LoadedOriginalTextureSizeFunc.Invoke()).x;
            public int Hight => (LoadedOriginalTextureSize ??= LoadedOriginalTextureSizeFunc.Invoke()).y;

            public string Name { get => Texture.name; set => Texture.name = value; }

            public void Dispose() { }
        }
        internal class UnityImportedDiskTexture : ITTDiskTexture
        {
            internal TTTImportedImage Texture;
            bool _isPreview;
            private Texture2D? _previewTex
            {
                get
                {
                    var tex = _isPreview ? CanvasImportedImagePreviewManager.GetPreview(Texture) : null;
                    if (_isPreview && tex == null) { throw new Exception("what happened? preview texture has destroyed?!"); }
                    return tex;
                }
            }

            public UnityImportedDiskTexture(TTTImportedImage texture, bool isPreview)
            {
                Texture = texture;
                _isPreview = isPreview;
                if (_isPreview) { CanvasImportedImagePreviewManager.PreloadPreviewImage(texture); }
            }
            public int Width => _previewTex?.width ?? Texture.CanvasDescription.Width;

            public int Hight => _previewTex?.height ?? Texture.CanvasDescription.Height;

            public string Name { get => Texture.name; set => Texture.name = value; }


            public void Dispose() { }
        }

    }
}
