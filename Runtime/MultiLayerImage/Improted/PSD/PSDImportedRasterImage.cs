using System.Threading.Tasks;
using net.rs64.MultiLayerImage.Parser.PSD;
using Unity.Collections;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    internal class PSDImportedRasterImage : TTTImportedImage
    {
        [SerializeField] internal PSDImportedRasterImageData RasterImageData;

        internal override Vector2Int Pivot => new Vector2Int(RasterImageData.RectTangle.Left, CanvasDescription.Height - RasterImageData.RectTangle.Bottom);
        internal override void LoadImage(byte[] importSouse, RenderTexture WriteTarget)
        {
            // var timer = System.Diagnostics.Stopwatch.StartNew();
            Task<NativeArray<byte>>[] getImageTask = new Task<NativeArray<byte>>[4];
            getImageTask[0] = Task.Run(() => RasterImageData.R.GetImageData(importSouse, RasterImageData.RectTangle));
            getImageTask[1] = Task.Run(() => RasterImageData.G.GetImageData(importSouse, RasterImageData.RectTangle));
            getImageTask[2] = Task.Run(() => RasterImageData.B.GetImageData(importSouse, RasterImageData.RectTangle));
            if (RasterImageData.A != null) { getImageTask[3] = Task.Run(() => RasterImageData.A.GetImageData(importSouse, RasterImageData.RectTangle)); }

            var texWidth = RasterImageData.RectTangle.GetWidth();
            var texHeight = RasterImageData.RectTangle.GetHeight();

            var texR = new Texture2D(texWidth, texHeight, TextureFormat.R8, false);
            texR.filterMode = FilterMode.Point;
            var texG = new Texture2D(texWidth, texHeight, TextureFormat.R8, false);
            texG.filterMode = FilterMode.Point;
            var texB = new Texture2D(texWidth, texHeight, TextureFormat.R8, false);
            texB.filterMode = FilterMode.Point;
            var texA = RasterImageData.A != null ? new Texture2D(texWidth, texHeight, TextureFormat.R8, false) : null;
            if (RasterImageData.A != null) { texA.filterMode = FilterMode.Point; }

            var mat = new Material(MargeColorAndOffsetShader);
            mat.SetTexture("_RTex", texR);
            mat.SetTexture("_GTex", texG);
            mat.SetTexture("_BTex", texB);
            mat.SetTexture("_ATex", texA);
            mat.SetVector("_Offset", new Vector4(Pivot.x / (float)CanvasDescription.Width, Pivot.y / (float)CanvasDescription.Height, texWidth / (float)CanvasDescription.Width, texHeight / (float)CanvasDescription.Height));
            mat.EnableKeyword(SHADER_KEYWORD_SRGB);

            // timer.Stop(); Debug.Log(name + "+SetUp:" + timer.ElapsedMilliseconds + "ms"); timer.Restart();
            var image = WeightTask(getImageTask).Result;
            // timer.Stop(); Debug.Log("TaskAwait:" + timer.ElapsedMilliseconds + "ms"); timer.Restart();

            texR.LoadRawTextureData(image[0]); texR.Apply();
            texG.LoadRawTextureData(image[1]); texG.Apply();
            texB.LoadRawTextureData(image[2]); texB.Apply();
            texA?.LoadRawTextureData(image[3]); texA?.Apply();

            // timer.Stop(); Debug.Log("LoadRawDataAndApply:" + timer.ElapsedMilliseconds + "ms"); timer.Restart();

            Graphics.Blit(null, WriteTarget, mat);

            // timer.Stop(); Debug.Log("Blit:" + timer.ElapsedMilliseconds + "ms"); timer.Restart();

            UnityEngine.Object.DestroyImmediate(mat);
            image[0].Dispose();
            image[1].Dispose();
            image[2].Dispose();
            if (RasterImageData.A != null) { image[3].Dispose(); }

            UnityEngine.Object.DestroyImmediate(texR);
            UnityEngine.Object.DestroyImmediate(texG);
            UnityEngine.Object.DestroyImmediate(texB);
            if (RasterImageData.A != null) { UnityEngine.Object.DestroyImmediate(texA); }

            // timer.Stop(); Debug.Log("Dispose:" + timer.ElapsedMilliseconds + "ms"); timer.Restart();
        }

        internal const string MARGE_COLOR_AND_OFFSET_SHADER = "Hidden/MargeColorAndOffset";
        internal static Shader MargeColorAndOffsetShader;
        internal const string SHADER_KEYWORD_SRGB = "COLOR_SPACE_SRGB";





        internal static NativeArray<Color32> LoadPSDRasterLayerData(PSDImportedRasterImageData rasterImageData, byte[] importSouse)
        {
            Task<NativeArray<byte>>[] getImageTask = new Task<NativeArray<byte>>[4];

            getImageTask[0] = Task.Run(() => ChannelImageDataParser.ChannelImageData.HeightInvert(rasterImageData.R.GetImageData(importSouse, rasterImageData.RectTangle), rasterImageData.RectTangle.GetWidth(), rasterImageData.RectTangle.GetHeight()));
            getImageTask[1] = Task.Run(() => ChannelImageDataParser.ChannelImageData.HeightInvert(rasterImageData.G.GetImageData(importSouse, rasterImageData.RectTangle), rasterImageData.RectTangle.GetWidth(), rasterImageData.RectTangle.GetHeight()));
            getImageTask[2] = Task.Run(() => ChannelImageDataParser.ChannelImageData.HeightInvert(rasterImageData.B.GetImageData(importSouse, rasterImageData.RectTangle), rasterImageData.RectTangle.GetWidth(), rasterImageData.RectTangle.GetHeight()));
            if (rasterImageData.A != null) { getImageTask[3] = Task.Run(() => ChannelImageDataParser.ChannelImageData.HeightInvert(rasterImageData.A.GetImageData(importSouse, rasterImageData.RectTangle), rasterImageData.RectTangle.GetWidth(), rasterImageData.RectTangle.GetHeight())); }

            var image = WeightTask(getImageTask).Result;

            var textureData = new NativeArray<Color32>(rasterImageData.RectTangle.GetWidth() * rasterImageData.RectTangle.GetHeight(), Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var textureSpan = textureData.AsSpan();

            var r = image[0];
            var g = image[1];
            var b = image[2];
            var a = image[3];

            if (rasterImageData.A != null)
            {
                for (var i = 0; textureData.Length > i; i += 1)
                {
                    textureSpan[i].r = r[i];
                    textureSpan[i].g = g[i];
                    textureSpan[i].b = b[i];
                    textureSpan[i].a = a[i];
                }
            }
            else
            {
                for (var i = 0; textureData.Length > i; i += 1)
                {
                    textureSpan[i].r = r[i];
                    textureSpan[i].g = g[i];
                    textureSpan[i].b = b[i];
                    textureSpan[i].a = byte.MaxValue;
                }
            }

            image[0].Dispose();
            image[1].Dispose();
            image[2].Dispose();
            if (rasterImageData.A != null) { image[3].Dispose(); }

            return textureData;

        }
        async static Task<NativeArray<byte>[]> WeightTask(Task<NativeArray<byte>>[] tasks)
        {
            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

    }
}