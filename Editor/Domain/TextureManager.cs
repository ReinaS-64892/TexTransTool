#if UNITY_EDITOR_WIN
#define SYSTEM_DRAWING
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace net.rs64.TexTransTool
{
    internal class TextureManager : ITextureManager
    {
        IDeferredDestroyTexture _deferDestroyTextureManager;
        IOriginTexture _originTexture;
        IDeferTextureCompress _textureCompressManager;
        public TextureManager(bool previewing, bool? useCompress = null)
        {
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
        public void WriteOriginalTexture(TTTImportedImage texture, RenderTexture writeTarget) { _originTexture.WriteOriginalTexture(texture, writeTarget); }
        public void PreloadOriginalTexture(Texture2D texture2D)
        {
            _originTexture.PreloadOriginalTexture(texture2D);
        }
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
        protected Dictionary<TTTImportedCanvasDescription, byte[]> _canvasSource = new();

        public IReadOnlyDictionary<Texture2D, Texture2D> OriginDict => _originDict;
        public IReadOnlyDictionary<TTTImportedCanvasDescription, byte[]> CanvasSource => _canvasSource;


        public void PreloadOriginalTexture(Texture2D texture2D)
        {
            if (Previewing) { return; }
#if SYSTEM_DRAWING
            if (_originDict.ContainsKey(texture2D) || _asyncOriginLoaders.ContainsKey(texture2D)) return;

            var task = TextureUtility.AsyncGetUncompressed(texture2D);

            _asyncOriginLoaders[texture2D] = task;
#endif
        }

        public void WriteOriginalTexture(Texture2D texture2D, RenderTexture writeTarget)
        {
            Graphics.Blit(GetOriginalTexture(texture2D), writeTarget);
        }
        public void WriteOriginalTexture(TTTImportedImage texture, RenderTexture writeTarget)
        {
            if (Previewing)
            {
                Graphics.Blit(texture.PreviewTexture, writeTarget);
            }
            else
            {
                if (!_canvasSource.ContainsKey(texture.CanvasDescription)) { _canvasSource[texture.CanvasDescription] = File.ReadAllBytes(AssetDatabase.GetAssetPath(texture.CanvasDescription)); }
                texture.LoadImage(_canvasSource[texture.CanvasDescription], writeTarget);
            }
        }


        public int GetOriginalTextureSize(Texture2D texture2D)
        {
            return TexTransCore.Utils.TextureUtility.NormalizePowerOfTwo(GetOriginalTexture(texture2D).width);
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

            var targetTextures = TexTransCore.Utils.RendererUtility.GetFilteredMaterials(renderers)
                .SelectMany(m => TexTransCore.Utils.MaterialUtility.GetAllTexture2D(m).Select(i => i.Value))
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
}
