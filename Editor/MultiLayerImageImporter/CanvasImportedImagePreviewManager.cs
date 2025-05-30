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
using UnityEditor.SceneManagement;



#if CONTAINS_TTCE_WGPU
using net.rs64.TexTransCoreEngineForWgpu;
#endif

namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    internal static class CanvasImportedImagePreviewManager
    {
        public const string PREVIEW_CACHE_PATH = "LayerPreviewImageCache";
        public static readonly string CachePath = Path.Combine(TTTLibrary.PATH, PREVIEW_CACHE_PATH);
        [TexTransInitialize]
        internal static void CanvasImportedImagePreviewInitialize()
        {
            CheckDirectory();

            PlaceHolderOrErrorTexture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            PlaceHolderOrErrorTexture.name = nameof(PlaceHolderOrErrorTexture);
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
            PlaceHolderOrErrorTexture.filterMode = FilterMode.Point;

            EditorApplication.update += ForgetPreloadCollectOnesAndReleaseMemory;

            AssemblyReloadEvents.beforeAssemblyReload += ReleaseManager;
            EditorApplication.playModeStateChanged += StateChangedEventLister;
            EditorSceneManager.activeSceneChangedInEditMode += ActiveSceneChanged;

        }
        static void StateChangedEventLister(PlayModeStateChange playModeStateChange)
        {
            switch (playModeStateChange)
            {
                default: return;
                case PlayModeStateChange.ExitingPlayMode:
                    {
                        ReleaseManager();
                        break;
                    }
            }
        }
        static void ActiveSceneChanged(UnityEngine.SceneManagement.Scene prev, UnityEngine.SceneManagement.Scene now)
        {
            ReleaseManager();
        }
#if CONTAINS_TTCE_WGPU
        static bool tryDeviceCreateLimiter = false;//セーフティ
        static void InitDevice()
        {
            if (tryDeviceCreateLimiter) { return; }
            Profiler.BeginSample("Init TTCE-Wgpu Device");
            try
            {
                s_ttceWgpuDevice = new(format: TexTransCoreTextureFormat.Byte);
            }
            catch (Exception e)
            {
                s_ttceWgpuDevice?.Dispose();
                s_ttceWgpuDevice = null;
                Debug.LogException(e);
                tryDeviceCreateLimiter = true;
            }
            Profiler.EndSample();
        }
#endif
        static void ReleaseManager()
        {
            ForgetPreloadCollect();
#if CONTAINS_TTCE_WGPU
            tryDeviceCreateLimiter = false;
            s_ttceWgpuDevice?.Dispose();
            s_ttceWgpuDevice = null;
            EditorApplication.update -= ForgetPreloadCollectOnesAndReleaseMemory;
#endif
            foreach (var tex in s_previewsDict.Values) { if (tex != null) { UnityEngine.Object.DestroyImmediate(tex); } }
            s_previewsTask.Clear();
            s_previewsDict.Clear();
            s_guid2Images.Clear();
            if (PlaceHolderOrErrorTexture != null) { UnityEngine.Object.DestroyImmediate(PlaceHolderOrErrorTexture); }
            PlaceHolderOrErrorTexture = null!;
            AssemblyReloadEvents.beforeAssemblyReload -= ReleaseManager;
            EditorApplication.playModeStateChanged -= StateChangedEventLister;
        }

        private static void CheckDirectory()
        {
            TTTLibrary.CheckTTTLibraryFolder();
            if (Directory.Exists(CachePath)) return;
            Directory.CreateDirectory(CachePath);
        }
#if CONTAINS_TTCE_WGPU
        static TTCEWgpuDeviceWithTTT4Unity? s_ttceWgpuDevice = null;
#endif
        public static Texture2D PlaceHolderOrErrorTexture { get; private set; } = null!;

        static Dictionary<TTTImportedImage, Task<Func<Texture2D?>>> s_previewsTask = new();
        static Dictionary<TTTImportedImage, Texture2D> s_previewsDict = new();
        static Dictionary<string, HashSet<TTTImportedImage>> s_guid2Images = new();
        public static Texture2D GetPreview(TTTImportedImage importedImage)
        {
            if (TryGetSyncCreated(importedImage, out var tex)) { return tex!; }
            if (s_previewsDict.TryGetValue(importedImage, out var omMemoryPreviewTex)) { return omMemoryPreviewTex; }
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(importedImage, out var guid, out long fileID) is false) { return PlaceHolderOrErrorTexture; }
            var createOutputPath = Path.Combine(CachePath, guid, fileID.ToString());

            if (File.Exists(createOutputPath)) { LoadFormFile(importedImage, createOutputPath); }
            else { CreatePreviewImageWithCache(importedImage); }

            if (TryGetSyncCreated(importedImage, out var tex2)) { return tex2!; }// true だった場合は必ず値がある
            if (s_previewsDict.TryGetValue(importedImage, out var texC)) { return texC; }
            else { return PlaceHolderOrErrorTexture; }//なんか失敗したらエラーのやつを返しとく


            static bool TryGetSyncCreated(TTTImportedImage importedImage, out Texture2D? texture)
            {
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(importedImage, out var guid, out long fileID) is false) { texture = null; return false; }
                if (s_previewsTask.TryGetValue(importedImage, out var task) is false) { texture = null; return false; }

                // DisplayProgressBar ってめちゃくちゃ重たいからやっぱなしで
                // EditorUtility.DisplayProgressBar("CreatePreviewImage", "GetResult", 0.7f);
                Profiler.BeginSample("GetResult", importedImage);
                var tex2D = task.Result.Invoke();
                Profiler.EndSample();
                // EditorUtility.ClearProgressBar();

                if (tex2D == null) { texture = null; return false; }

                s_previewsTask.Remove(importedImage);
                texture = s_previewsDict[importedImage] = tex2D;
                if (s_guid2Images.ContainsKey(guid)) { s_guid2Images[guid].Add(importedImage); }
                else { s_guid2Images[guid] = new() { importedImage }; }
                return true;
            }
        }

        private static void LoadFormFile(TTTImportedImage importedImage, string path)
        {
            if (s_previewsTask.TryGetValue(importedImage, out var _)) { return; }
            if (s_previewsDict.TryGetValue(importedImage, out var _)) { return; }

            var needResizing = Math.Max(importedImage.CanvasDescription.Width, importedImage.CanvasDescription.Height) > 1024;
            var previewTex = new Texture2D(needResizing ? 1024 : importedImage.CanvasDescription.Width, needResizing ? 1024 : importedImage.CanvasDescription.Height, TextureFormat.BC7, false, false);
            previewTex.name = "LoadPreviewFromFileCash-" + importedImage.name;
            var dataNa = previewTex.GetRawTextureData<byte>();

            var task = Task.Run(() =>
            {
                Profiler.BeginSample("load from file", importedImage);
                using var imageFile = File.OpenRead(path);
                imageFile.Read(dataNa.AsSpan());
                Profiler.EndSample();
            });

            var texFnTask = Task.Run(async () =>
            {
                await task.ConfigureAwait(false);

                Func<Texture2D?> tex2DResult = () =>
                {
                    if (previewTex == null) { return null; }
                    previewTex.Apply(true, true);
                    return previewTex;
                };
                return tex2DResult;
            });

            s_previewsTask[importedImage] = texFnTask;
        }

        public static void PreloadPreviewImage(TTTImportedImage importedImage, ITTImportedCanvasSource? canvasSource = null)
        {
            if (s_previewsTask.TryGetValue(importedImage, out var _)) { return; }
            if (s_previewsDict.TryGetValue(importedImage, out var _)) { return; }
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(importedImage, out var guid, out long fileID) is false) { return; }
            var createOutputPath = Path.Combine(CachePath, guid, fileID.ToString());

            if (File.Exists(createOutputPath) is false) { CreatePreviewImageWithCache(importedImage, canvasSource); }
            else { LoadFormFile(importedImage, createOutputPath); }
        }
        public static void CreatePreviewImageWithCache(TTTImportedImage importedImage, ITTImportedCanvasSource? canvasSource = null)
        {
#if CONTAINS_TTCE_WGPU
            if (s_ttceWgpuDevice is null) { InitDevice(); }
            if (s_ttceWgpuDevice is null) { return; }
#endif
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
                var previewTex = new Texture2D(needResizing ? 1024 : importedImage.CanvasDescription.Width, needResizing ? 1024 : importedImage.CanvasDescription.Height, TextureFormat.RGBA32, false, false);
                previewTex.name = "ImportedPreview-" + importedImage.name;
                var dataNa = previewTex.GetRawTextureData<byte>();

                Profiler.BeginSample("LoadCanvasSource");
                canvasSource ??= Memoize.Memo<(TTTImportedCanvasDescription, string), ITTImportedCanvasSource>((importedImage.CanvasDescription, guid), (t) => { return t.Item1.LoadCanvasSource(AssetDatabase.GUIDToAssetPath(t.Item2)); });
                Profiler.EndSample();

#if !CONTAINS_TTCE_WGPU

                var destroyHash = new HashSet<Texture2D>();
                var ttce4u = new TTCEUnityWithTTT4Unity(new UnityDiskUtil(false));

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
#else


                var task = Task.Run(() =>
                {
                    using var ttceWgpu = s_ttceWgpuDevice.GetTTCEWgpuContext();
                    var ttce4u = ttceWgpu;

                    Profiler.BeginSample("CreatePreviewImage", importedImage);
                    CreatePreviewImage(ttce4u, importedImage, canvasSource, dataNa);
                    Profiler.EndSample();

                });



                var texFnTask = Task.Run(async () =>
                {
                    await task.ConfigureAwait(false);

                    Func<Texture2D?> tex2DResult = () =>
                    {
                        if (previewTex == null) { return null; }
                        Profiler.BeginSample("CompressAndFinalize");
                        EditorUtility.CompressTexture(previewTex, TextureFormat.BC7, 100);
                        previewTex.Apply(true);
                        var compressedData = previewTex.GetRawTextureData<byte>();

                        Profiler.BeginSample("Write to file");
                        if (Directory.Exists(Path.GetDirectoryName(createOutputPath)))
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
        , ITexTransDriveStorageBufferHolder
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
                    ttce4u.DefaultResizing(resizedRt, loadRt);
                    extractRt = resizedRt;
                }
                else { extractRt = loadRt; }

                Profiler.BeginSample("DownloadTexture");
                ttce4u.DownloadTexture(writeTarget, TexTransCoreTextureFormat.Byte, extractRt);
                Profiler.EndSample();
            }
            finally
            {
                resizedRt?.Dispose();
            }
        }

        public static void ForgetPreloadCollectOnesAndReleaseMemory()
        {
            var image = s_previewsTask.Keys.FirstOrDefault();
            if (image is null) { return; }
            GetPreview(image);

            // 毎回 ロードされてしまうから破棄しない方針で行く
            // if (s_previewsDict.Remove(image, out var tex)) { UnityEngine.Object.DestroyImmediate(tex); }
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

        internal static void Reinitialize()
        {
            ReleaseManager();
            CanvasImportedImagePreviewInitialize();
        }
    }
}
