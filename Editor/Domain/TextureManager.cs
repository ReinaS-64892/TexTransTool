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

namespace net.rs64.TexTransTool
{
    internal class TextureManager : ITextureManager
    {
        IDeferredDestroyTexture _deferDestroyTextureManager;
        IOriginTexture _originTexture;
        IDeferTextureCompress _textureCompressManager;

        public bool IsPreview { get; private set; }

        public TextureManager(bool previewing, bool? useCompress = null)
        {
            IsPreview = previewing;
            _deferDestroyTextureManager = new DeferredDestroyer();
            _originTexture = new GetOriginTexture(previewing, _deferDestroyTextureManager.DeferredDestroyOf);
            _textureCompressManager = useCompress ?? !previewing ? new TextureCompress() : null;
        }
        public TextureManager(IDeferredDestroyTexture deferDestroyTextureManager, IOriginTexture originTexture, IDeferTextureCompress textureCompressManager)
        {
            _deferDestroyTextureManager = deferDestroyTextureManager;
            _originTexture = originTexture;
            _textureCompressManager = textureCompressManager;
        }



        public void DeferredDestroyOf(Texture2D texture2D) { _deferDestroyTextureManager?.DeferredDestroyOf(texture2D); }
        public void DestroyDeferred() { _deferDestroyTextureManager?.DestroyDeferred(); }

        public void DeferredInheritTextureCompress(Texture2D source, Texture2D target) { _textureCompressManager?.DeferredInheritTextureCompress(source, target); }
        public void DeferredTextureCompress(ITTTextureFormat compressFormat, Texture2D target) { _textureCompressManager?.DeferredTextureCompress(compressFormat, target); }
        public void CompressDeferred(IEnumerable<Renderer> renderers, OriginEqual originEqual) { _textureCompressManager?.CompressDeferred(renderers, originEqual); }


        public int GetOriginalTextureSize(Texture2D texture2D) { return _originTexture.GetOriginalTextureSize(texture2D); }
        public void WriteOriginalTexture(Texture2D texture2D, RenderTexture writeTarget) { _originTexture.WriteOriginalTexture(texture2D, writeTarget); }

        public void PreloadOriginalTexture(Texture2D texture2D)
        {
            _originTexture.PreloadOriginalTexture(texture2D);
        }

        public (int x, int y) PreloadAndTextureSizeForTex2D(Texture2D diskTexture)
        {
            var size = _originTexture.GetOriginalTextureSize(diskTexture);
            return (size, size);
        }

        public void LoadTexture(RenderTexture writeRt, Texture2D diskSource) { WriteOriginalTexture(diskSource, writeRt); }
    }

    internal class DeferredDestroyer : IDeferredDestroyTexture
    {
        protected List<Texture2D> DestroyList = new();
        public void DeferredDestroyOf(Texture2D texture2D)
        {
            DestroyList.Add(texture2D);
        }

        public void DestroyDeferred()
        {
            if (DestroyList == null) { return; }
            foreach (var tex in DestroyList)
            {
                if (tex == null || AssetDatabase.Contains(tex)) { continue; }
                UnityEngine.Object.DestroyImmediate(tex);
            }
            DestroyList.Clear();
        }
    }

    internal class GetOriginTexture : IOriginTexture
    {
        private readonly bool Previewing;
        private readonly Action<Texture2D> DeferDestroyCall;

        public GetOriginTexture(bool previewing, Action<Texture2D> deferDestroyCall)
        {
            Previewing = previewing;
            DeferDestroyCall = deferDestroyCall;
        }

        protected Dictionary<Texture2D, Texture2D> _originDict = new();
#if SYSTEM_DRAWING
        private Dictionary<Texture2D, Task<Func<Texture2D>>> _asyncOriginLoaders = new();
#endif
        protected Dictionary<TTTImportedCanvasDescription, ITTImportedCanvasSource> _canvasSource = new();

        public bool IsPreview => Previewing;

        public void PreloadOriginalTexture(Texture2D texture2D)
        {
            if (Previewing) { return; }
#if SYSTEM_DRAWING
            if (_originDict.ContainsKey(texture2D) || _asyncOriginLoaders.ContainsKey(texture2D)) return;

            var task = EditorTextureUtility.AsyncGetUncompressed(texture2D);

            _asyncOriginLoaders[texture2D] = task;
#endif
        }

        public void WriteOriginalTexture(Texture2D texture2D, RenderTexture writeTarget)
        {
            Graphics.Blit(GetOriginalTexture(texture2D), writeTarget);
        }


        public int GetOriginalTextureSize(Texture2D texture2D)
        {
            return TexTransTool.Utils.TextureUtility.NormalizePowerOfTwo(GetOriginalTexture(texture2D).width);
        }
        public Texture2D GetOriginalTexture(Texture2D texture2D)
        {
            if (Previewing)
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
                    DeferDestroyCall?.Invoke(_originDict[texture2D]);
                    _asyncOriginLoaders.Remove(texture2D);

                    return _originDict[texture2D];
                }
#endif
                else
                {
                    var originTex = texture2D.TryGetUnCompress();
                    _originDict[texture2D] = originTex;
                    DeferDestroyCall?.Invoke(originTex);
                    return originTex;
                }
            }
        }

        public (int x, int y) PreloadAndTextureSizeForTex2D(Texture2D diskTexture)
        {
            var size = GetOriginalTextureSize(diskTexture);
            return (size, size);
        }

        public void LoadTexture(RenderTexture writeRt, Texture2D diskSource)
        {
            throw new NotImplementedException();
        }
    }
    internal class TextureCompress : IDeferTextureCompress
    {
        private protected Dictionary<Texture2D, ITTTextureFormat> _compressDict = new();
        public IReadOnlyDictionary<Texture2D, ITTTextureFormat> CompressDict => _compressDict;

        public void DeferredTextureCompress(ITTTextureFormat compressSetting, Texture2D target)
        {
            if (_compressDict == null) { return; }
            _compressDict[target] = compressSetting;
        }
        public void DeferredInheritTextureCompress(Texture2D source, Texture2D target)
        {
            if (_compressDict == null) { return; }
            if (target == source) { return; }
            if (_compressDict.ContainsKey(source))
            {
                _compressDict[target] = _compressDict[source];
                _compressDict.Remove(source);
            }
            else
            {
                _compressDict[target] = GetTTTextureFormat(source);
            }
        }

        public virtual void CompressDeferred(IEnumerable<Renderer> renderers, OriginEqual originEqual)
        {
            if (_compressDict == null) { return; }
            var compressKV = _compressDict.Where(i => i.Key != null);// Unity が勝手にテクスチャを破棄してくる場合があるので Null が入ってないか確認する必要がある。

            var targetTextures = RendererUtility.GetFilteredMaterials(renderers)
                .SelectMany(m => MaterialUtility.GetAllTexture2DWithDictionary(m).Select(i => i.Value))
                .Where(t => t != null)
                .Distinct()
                .Select(t => (t, compressKV.FirstOrDefault(kv => originEqual(kv.Key, t))))
                .Where(kvp => kvp.Item2.Key is not null && kvp.Item2.Value is not null)
                .Select(kvp => (kvp.t, kvp.Item2.Value))
                .Where(kv => GraphicsFormatUtility.IsCompressedFormat(kv.t.format) is false)
                .ToArray();

            foreach (var tex in targetTextures)
            {
                var compressFormat = tex.Value.Get(tex.t);
                EditorUtility.CompressTexture(tex.t, compressFormat.CompressFormat, compressFormat.Quality);
            }

            foreach (var tex in targetTextures) tex.t.Apply(false, true);

            foreach (var tex in targetTextures)
            {
                var sTexture = new SerializedObject(tex.t);

                var sStreamingMipmaps = sTexture.FindProperty("m_StreamingMipmaps");
                sStreamingMipmaps.boolValue = true;

                sTexture.ApplyModifiedPropertiesWithoutUndo();
            }

            _compressDict.Clear();
        }

        public static ITTTextureFormat GetTTTextureFormat(Texture2D texture2D)
        {
            static ITTTextureFormat GetDirect(Texture2D texture2D) { return new DirectFormat(texture2D.format, 50); }
            if (AssetDatabase.Contains(texture2D) is false) { return GetDirect(texture2D); }

            var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture2D));
            if (importer is not TextureImporter textureImporter) { return GetDirect(texture2D); }

            return new RefAtImporterFormat(texture2D.format, textureImporter);
        }
        class DirectFormat : ITTTextureFormat
        {
            public (TextureFormat CompressFormat, int Quality) format;
            public DirectFormat(TextureFormat compressFormat, int quality) { format = (compressFormat, quality); }
            public (TextureFormat CompressFormat, int Quality) Get(Texture2D texture2D) { return format; }
        }

        internal class RefAtImporterFormat : ITTTextureFormat
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


    public class UnityDiskUtil : ITexTransUnityDiskUtil
    {
        private readonly IOriginTexture _texManage;
        private readonly Dictionary<TTTImportedCanvasDescription, ITTImportedCanvasSource> _canvasSource;

        public UnityDiskUtil(IOriginTexture texManage)
        {
            _texManage = texManage;
            _canvasSource = new();
        }

        public ITTDiskTexture Wrapping(Texture2D texture2D)
        {
            return new UnityDiskTexture(texture2D, _texManage.PreloadAndTextureSizeForTex2D(texture2D));
        }

        public ITTDiskTexture Wrapping(TTTImportedImage texture2D)
        {
            return new UnityImportedDiskTexture(texture2D, _texManage.IsPreview);
        }
        static ComputeShader CopyFromGammaTexture2D;
        [TexTransInitialize]
        internal static void Init()
        {
            CopyFromGammaTexture2D = TexTransCoreRuntime.LoadAsset("b1cd01a41aef7f443bafb8684546de39", typeof(ComputeShader)) as ComputeShader;
        }

        public void LoadTexture(ITexTransToolForUnity ttce4u, ITTRenderTexture writeTarget, ITTDiskTexture diskTexture)
        {
            switch (diskTexture)
            {
                case UnityDiskTexture tex2DWrapper:
                    {
                        _texManage.LoadTexture(writeTarget.Unwrap(), tex2DWrapper.Texture);
                        ttce4u.LinearToGamma(writeTarget);
                        break;
                    }
                case UnityImportedDiskTexture importedWrapper:
                    {
                        var texture = importedWrapper.Texture;
                        if (_texManage.IsPreview)
                        {
                            CopyFromGammaTexture2D.SetTexture(0, "Source", CanvasImportedImagePreviewManager.GetPreview(texture));
                            CopyFromGammaTexture2D.SetTexture(0, "Dist", writeTarget.Unwrap());
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
        public ITexTransLoadTexture GetLoader(ITexTransToolForUnity ttce4u) => new DiskLoaderFor(ttce4u, this);

        internal class DiskLoaderFor : ITexTransLoadTexture
        {
            ITexTransToolForUnity _texTransToolForUnity;
            UnityDiskUtil _texTransUnityDiskUtil;

            public DiskLoaderFor(ITexTransToolForUnity texTransToolForUnity, UnityDiskUtil texTransUnityDiskUtil)
            {
                _texTransToolForUnity = texTransToolForUnity;
                _texTransUnityDiskUtil = texTransUnityDiskUtil;
            }

            public void LoadTexture(ITTRenderTexture writeTarget, ITTDiskTexture diskTexture)
            {
                _texTransUnityDiskUtil.LoadTexture(_texTransToolForUnity, writeTarget, diskTexture);
            }
        }
        internal class UnityDiskTexture : ITTDiskTexture
        {
            internal Texture2D Texture;
            internal (int x, int y) LoadableTextureSize;
            public UnityDiskTexture(Texture2D texture, (int x, int y) loadableTextureSize)
            {
                Texture = texture;
                LoadableTextureSize = loadableTextureSize;
            }
            public int Width => LoadableTextureSize.x;

            public int Hight => LoadableTextureSize.y;

            public string Name { get => Texture.name; set => Texture.name = value; }

            public void Dispose() { }
        }
#nullable enable
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
