using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal class TextureManager : ITextureManager
    {
        IDeferredDestroyTexture _deferDestroyTextureManager;
        IOriginTexture _originTexture;
        IDeferTextureCompress _textureCompressManager;
        ITexTransToolForUnity _ttt4u;//TODO : これ何とかしないと
        public TextureManager(bool previewing, bool? useCompress = null, ITexTransToolForUnity ttt4u = null)
        {
            _ttt4u = ttt4u;
            _deferDestroyTextureManager = new DeferredDestroyer();
            _originTexture = new GetOriginTexture(previewing, _deferDestroyTextureManager.DeferredDestroyOf, _ttt4u);
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
        public void CompressDeferred() { _textureCompressManager?.CompressDeferred(); }


        public int GetOriginalTextureSize(Texture2D texture2D) { return _originTexture.GetOriginalTextureSize(texture2D); }
        public void WriteOriginalTexture(Texture2D texture2D, RenderTexture writeTarget) { _originTexture.WriteOriginalTexture(texture2D, writeTarget); }
        public void WriteOriginalTexture(TTTImportedImage texture, ITTRenderTexture writeTarget) { _originTexture.WriteOriginalTexture(texture, writeTarget); }

        internal void SetTTCE4U(ITexTransToolForUnity ttce4U)
        {
            _ttt4u = ttce4U;
            if (_originTexture is GetOriginTexture got)
            {
                got._ttt4u = ttce4U;
            }
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
        internal ITexTransToolForUnity _ttt4u;
        private readonly bool Previewing;
        private readonly Action<Texture2D> DeferDestroyCall;

        public GetOriginTexture(bool previewing, Action<Texture2D> deferDestroyCall, ITexTransToolForUnity ttt4u)
        {
            _ttt4u = ttt4u;
            Previewing = previewing;
            DeferDestroyCall = deferDestroyCall;
        }

        protected Dictionary<Texture2D, Texture2D> _originDict = new();
        protected Dictionary<TTTImportedCanvasDescription, ITTImportedCanvasSource> _canvasSource = new();

        public void WriteOriginalTexture(Texture2D texture2D, RenderTexture writeTarget)
        {
            Graphics.Blit(GetOriginalTexture(texture2D), writeTarget);
        }
        public void WriteOriginalTexture(TTTImportedImage texture, ITTRenderTexture writeTarget)
        {
            if (Previewing)
            {
                Graphics.Blit(texture.PreviewTexture, writeTarget.Unwrap());
                _ttt4u.LinearToGamma(writeTarget);
            }
            else
            {
                if (!_canvasSource.ContainsKey(texture.CanvasDescription)) { _canvasSource[texture.CanvasDescription] = texture.CanvasDescription.LoadCanvasSource(AssetDatabase.GetAssetPath(texture.CanvasDescription)); }
                texture.LoadImage(_canvasSource[texture.CanvasDescription], _ttt4u, writeTarget);
            }
        }
        public int GetOriginalTextureSize(Texture2D texture2D)
        {
            return TexTransCoreEngineForUnity.Utils.TextureUtility.NormalizePowerOfTwo(GetOriginalTexture(texture2D).width);
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

        public virtual void CompressDeferred()
        {
            if (_compressDict == null) { return; }
            var compressTargets = _compressDict.Where(i => i.Key != null);// Unity が勝手にテクスチャを破棄してくる場合があるので Null が入ってないか確認する必要がある。

            foreach (var texAndFormat in compressTargets)
            {
                var compressFormat = texAndFormat.Value.Get(texAndFormat.Key);
                EditorUtility.CompressTexture(texAndFormat.Key, compressFormat.CompressFormat, compressFormat.Quality);
            }

            foreach (var tex in compressTargets.Select(i => i.Key))
            {
                tex.Apply(false, true);
            }

            foreach (var tex in compressTargets.Select(i => i.Key))
            {
                var sTexture = new SerializedObject(tex);

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
