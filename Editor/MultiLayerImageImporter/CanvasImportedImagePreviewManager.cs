#nullable enable
// #undef CONTAINS_TTCE_WGPU
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEngine.Profiling;
using System.Runtime.CompilerServices;
using System.Linq;



#if CONTAINS_TTCE_WGPU
using net.rs64.TexTransCoreEngineForWgpu;
#endif

namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    internal static class CanvasImportedImagePreviewManager
    {
        public const string PREVIEW_CACHE_PATH = "LayerPreviewImageCache";
        public static readonly string CachePath = Path.Combine(TTTLibrary.PATH, PREVIEW_CACHE_PATH);
        [InitializeOnLoadMethod]
        internal static void Init()
        {
            CheckDirectory();

            PlaceHolderOrErrorTexture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            PlaceHolderOrErrorTexture.filterMode = FilterMode.Point;
            PlaceHolderOrErrorTexture.wrapMode = TextureWrapMode.Repeat;

            var data = PlaceHolderOrErrorTexture.GetPixelData<Color32>(0);

            var col1 = new Color32(0x00, 0xFF, 0xF5, 0xFF);
            var col2 = new Color32(0xFF, 0x5D, 0xB9, 0xFF);

            data[0] = col1;
            data[1] = col2;
            data[2] = col1;
            data[3] = col2;

            data[4] = col2;
            data[5] = col1;
            data[6] = col2;
            data[7] = col1;

            data[8] = col1;
            data[9] = col2;
            data[10] = col1;
            data[11] = col2;

            data[12] = col2;
            data[13] = col1;
            data[14] = col2;
            data[15] = col1;

            PlaceHolderOrErrorTexture.Apply(true, true);

#if CONTAINS_TTCE_WGPU
            s_ttceWgpuDevice = new TTCEWgpuDevice();
            s_ttceWgpuDevice.SetDefaultTextureFormat(TexTransCoreTextureFormat.Byte);
            s_shaderDictionary = ShaderFinder.RegisterShaders(s_ttceWgpuDevice, ShaderFinder.GetAllShaderPathWithCurrentDirectory());

            EditorApplication.update += ForgetPreloadCollectOnesAndReleaseMemory;
#endif
            AssemblyReloadEvents.beforeAssemblyReload += ReleaseManager;

        }
        static void ReleaseManager()
        {
#if CONTAINS_TTCE_WGPU
            ForgetPreloadCollect();
            s_ttceWgpuDevice.Dispose();
#endif
            foreach (var tex in s_previewsDict.Values) { UnityEngine.Object.DestroyImmediate(tex); }
            s_previewsDict.Clear();
            s_guid2Images.Clear();
        }

        private static void CheckDirectory()
        {
            TTTLibrary.CheckTTTLibraryFolder();
            if (Directory.Exists(CachePath)) return;
            Directory.CreateDirectory(CachePath);
        }
#if CONTAINS_TTCE_WGPU
        static TTCEWgpuDevice s_ttceWgpuDevice = null!;
        static ShaderFinder.ShaderDictionary s_shaderDictionary = null!;
        static Dictionary<TTTImportedImage, Task<Func<Texture2D>>> s_previewsTask = new();
#endif

        public static Texture2D PlaceHolderOrErrorTexture { get; private set; } = null!;

        static Dictionary<TTTImportedImage, Texture2D> s_previewsDict = new();
        static Dictionary<string, HashSet<TTTImportedImage>> s_guid2Images = new();
        public static Texture2D GetPreview(TTTImportedImage importedImage)
        {
#if CONTAINS_TTCE_WGPU
            if (TryGetSyncCreated(importedImage, out var tex)) { return tex!; }
#endif
            if (s_previewsDict.TryGetValue(importedImage, out var omMemoryPreviewTex)) { return omMemoryPreviewTex; }
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(importedImage, out var guid, out long fileID) is false) { return PlaceHolderOrErrorTexture; }
            var createOutputPath = Path.Combine(CachePath, guid, fileID.ToString());

            if (File.Exists(createOutputPath))
            {
                return LoadFormFile(importedImage, guid, createOutputPath);
            }
            else
            {
                CreatePreviewImageWithCache(importedImage);
#if CONTAINS_TTCE_WGPU
                if (TryGetSyncCreated(importedImage, out var tex2)) { return tex2!; }// true だった場合は必ず値がある
                else { return PlaceHolderOrErrorTexture; }//なんか失敗したらエラーのやつを返しとく
#else
                return s_previewsDict[importedImage];
#endif
            }

#if CONTAINS_TTCE_WGPU
            static bool TryGetSyncCreated(TTTImportedImage importedImage, out Texture2D? texture)
            {
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(importedImage, out var guid, out long fileID) is false) { texture = null; return false; }
                if (s_previewsTask.TryGetValue(importedImage, out var task) is false) { texture = null; return false; }


                Profiler.BeginSample("GetResult", importedImage);
                EditorUtility.DisplayProgressBar("CreatePreviewImage", "GetResult", 0.7f);
                var tex2D = task.Result.Invoke();
                Profiler.EndSample();
                EditorUtility.ClearProgressBar();

                s_previewsTask.Remove(importedImage);
                texture = s_previewsDict[importedImage] = tex2D;
                if (s_guid2Images.ContainsKey(guid)) { s_guid2Images[guid].Add(importedImage); }
                else { s_guid2Images[guid] = new() { importedImage }; }
                return true;
            }
#endif
        }

        private static Texture2D LoadFormFile(TTTImportedImage importedImage, string guid, string createOutputPath)
        {
            var needResizing = Math.Max(importedImage.CanvasDescription.Width, importedImage.CanvasDescription.Height) > 1024;
            var previewTex = new Texture2D(needResizing ? 1024 : importedImage.CanvasDescription.Width, needResizing ? 1024 : importedImage.CanvasDescription.Height, TextureFormat.BC7, false, true);
            previewTex.LoadRawTextureData(File.ReadAllBytes(createOutputPath));
            previewTex.Apply(true, true);
            previewTex.name = "ImportedPreview-" + importedImage.name;
            s_previewsDict[importedImage] = previewTex;
            if (s_guid2Images.ContainsKey(guid)) { s_guid2Images[guid].Add(importedImage); }
            else { s_guid2Images[guid] = new() { importedImage }; }

            return previewTex;
        }

        public static void PreloadPreviewImage(TTTImportedImage importedImage, ITTImportedCanvasSource? canvasSource = null)
        {
#if CONTAINS_TTCE_WGPU
            if (s_previewsTask.TryGetValue(importedImage, out var _)) { return; }
#endif
            if (s_previewsDict.TryGetValue(importedImage, out var _)) { return; }
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(importedImage, out var guid, out long fileID) is false) { return; }
            var createOutputPath = Path.Combine(CachePath, guid, fileID.ToString());

            if (File.Exists(createOutputPath) is false)
            {
                CreatePreviewImageWithCache(importedImage, canvasSource);
            }
            else { LoadFormFile(importedImage, guid, createOutputPath); }
        }
        public static void CreatePreviewImageWithCache(TTTImportedImage importedImage, ITTImportedCanvasSource? canvasSource = null)
        {
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(importedImage, out var guid, out long fileID) is false) { throw new NotImplementedException(); }
            try
            {
                Profiler.BeginSample("CreatePreviewImageWithCache", importedImage);
                EditorUtility.DisplayProgressBar("CreatePreviewImage", "LoadImage", 0.3f);

                var needResizing = Math.Max(importedImage.CanvasDescription.Width, importedImage.CanvasDescription.Height) > 1024;
                if (Directory.Exists(Path.Combine(CachePath, guid)) is false) { Directory.CreateDirectory(Path.Combine(CachePath, guid)); }
                var createOutputPath = Path.Combine(CachePath, guid, fileID.ToString());
                if (File.Exists(createOutputPath)) { File.Delete(createOutputPath); }


                // var length = extractRt.Width * extractRt.Hight * EnginUtil.GetPixelParByte(TexTransCoreTextureFormat.Byte, TexTransCoreTextureChannel.RGBA);
                // var na = new NativeArray<byte>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                var previewTex = new Texture2D(needResizing ? 1024 : importedImage.CanvasDescription.Width, needResizing ? 1024 : importedImage.CanvasDescription.Height, TextureFormat.RGBA32, false, true);
                previewTex.name = "ImportedPreview-" + importedImage.name;
                var dataNa = previewTex.GetRawTextureData<byte>();


                Profiler.BeginSample("LoadCanvasSource");
                canvasSource ??= Memoize.Memo<(TTTImportedCanvasDescription, string), ITTImportedCanvasSource>((importedImage.CanvasDescription, guid), (t) => { return t.Item1.LoadCanvasSource(AssetDatabase.GUIDToAssetPath(t.Item2)); });
                Profiler.EndSample();

#if CONTAINS_TTCE_WGPU

                var task = Task.Run(() =>
                {
                    // throw new NotImplementedException();

                    using var ttceWgpu = s_ttceWgpuDevice.GetContext<TTCEWgpuWithTTT4Unity>();
                    ttceWgpu.ShaderDictionary = s_shaderDictionary;
                    var ttce4u = ttceWgpu;

                    Profiler.BeginSample("CreatePreviewImage", importedImage);
                    CreatePreviewImage(ttce4u, importedImage, canvasSource, dataNa);
                    Profiler.EndSample();

                });



                var texFnTask = Task.Run(async () =>
                {
                    await task.ConfigureAwait(false);

                    Func<Texture2D> tex2DResult = () =>
                    {
                        Profiler.BeginSample("CompressAndFinalize");
                        EditorUtility.CompressTexture(previewTex, TextureFormat.BC7, 100);
                        previewTex.Apply(true);
                        var compressedData = previewTex.GetRawTextureData<byte>();

                        Profiler.BeginSample("Write to file");
                        using (var stream = File.Open(createOutputPath, FileMode.CreateNew, FileAccess.Write))
                            stream.Write(compressedData.AsSpan());
                        Profiler.EndSample();

                        previewTex.Apply(true, true);
                        Profiler.EndSample();
                        return previewTex;
                    };
                    return tex2DResult;
                });

                s_previewsTask[importedImage] = texFnTask;
#else
                var destroyHash = new HashSet<Texture2D>();
                var ttce4u = new TTCE4UnityWithTTT4Unity(new UnityDiskUtil(new GetOriginTexture(false, t => destroyHash.Add(t))));

                Profiler.BeginSample("CreatePreviewImage");
                CreatePreviewImage(ttce4u, importedImage, canvasSource, dataNa);
                Profiler.EndSample();

                EditorUtility.DisplayProgressBar("CreatePreviewImage", "Compress And Finalize", 0.6f);


                EditorUtility.CompressTexture(previewTex, TextureFormat.BC7, 100);
                previewTex.Apply(true);
                dataNa = previewTex.GetRawTextureData<byte>();

                using var stream = File.Open(createOutputPath, FileMode.CreateNew, FileAccess.Write);
                stream.Write(dataNa.AsSpan());

                previewTex.Apply(true, true);
                s_previewsDict[importedImage] = previewTex;

                if (s_guid2Images.ContainsKey(guid)) { s_guid2Images[guid].Add(importedImage); }
                else { s_guid2Images[guid] = new() { importedImage }; }

                foreach (var i in destroyHash) { UnityEngine.Object.DestroyImmediate(i); }
                EditorUtility.DisplayProgressBar("CreatePreviewImage", "End", 1f);
#endif
            }
            finally
            {
                Profiler.EndSample();
                EditorUtility.ClearProgressBar();
            }
        }


        static void CreatePreviewImage<TTCE>(TTCE ttce4u, TTTImportedImage importedImage, ITTImportedCanvasSource canvasSource, Span<byte> writeTarget)
         where TTCE : ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        , ITexTransCopyRenderTexture
        , ITexTransCreateTexture
        , ITexTransRenderTextureIO
        , ITexTransRenderTextureUploadToCreate
        {
            var needResizing = Math.Max(importedImage.CanvasDescription.Width, importedImage.CanvasDescription.Height) > 1024;
            using var loadRt = ttce4u.CreateRenderTexture(importedImage.CanvasDescription.Width, importedImage.CanvasDescription.Height);
            var resizedRt = needResizing ? ttce4u.CreateRenderTexture(1024, 1024) : null;
            try
            {
                Profiler.BeginSample("LoadImage");
                importedImage.LoadImage(canvasSource, ttce4u, loadRt);
                Profiler.EndSample();

                ITTRenderTexture extractRt;
                if (needResizing && resizedRt is not null)
                {
                    ttce4u.BilinearReScaling(resizedRt, loadRt);
                    extractRt = resizedRt;
                }
                else { extractRt = loadRt; }
                ttce4u.GammaToLinear(extractRt);

                Profiler.BeginSample("DownloadTexture");
                ttce4u.DownloadTexture(writeTarget, TexTransCoreTextureFormat.Byte, extractRt);
                Profiler.EndSample();
            }
            finally
            {
                resizedRt?.Dispose();
            }
        }

#if CONTAINS_TTCE_WGPU
        public static void ForgetPreloadCollectOnesAndReleaseMemory()
        {
            var image = s_previewsTask.Keys.FirstOrDefault();
            if (image is null) { return; }
            try { GetPreview(image); }
            finally { s_previewsTask.Remove(image); }
            if (s_previewsDict.Remove(image, out var tex)) { UnityEngine.Object.DestroyImmediate(tex); }
        }
        public static void ForgetPreloadCollect()
        {
            try
            {
                foreach (var t in s_previewsTask.Keys.ToArray())
                {
                    try
                    {
                        GetPreview(t);
                    }
                    catch (Exception e) { Debug.LogException(e); }
                }
            }
            finally //無いとは思うけど
            {
                s_previewsTask.Clear();
            }
        }
#endif

        public static void InvalidatesCacheAll()
        {
            foreach (var i in Directory.EnumerateDirectories(CachePath))
            {
                Directory.Delete(i, true);
            }
            foreach (var i in s_previewsDict.Values)
            {
                UnityEngine.Object.DestroyImmediate(i);
            }

            s_previewsDict.Clear();
            s_guid2Images.Clear();
        }
        public static void InvalidatesCache(string guid)
        {
            if (s_guid2Images.ContainsKey(guid) is false) return;

            var createOutputPath = Path.Combine(CachePath, guid);
            if (Directory.Exists(createOutputPath)) Directory.Delete(createOutputPath, true);

            foreach (var i in s_guid2Images[guid])
            {
                if (s_previewsDict.Remove(i, out var tex))
                {
                    UnityEngine.Object.DestroyImmediate(tex);
                }
            }
            s_guid2Images.Remove(guid);
        }
    }
}
