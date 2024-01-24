using net.rs64.MultiLayerImageParser.PSD;
using net.rs64.TexTransCore.BlendTexture;
using Unity.Collections;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    internal class PSDImportedRasterMaskImage : TTTImportedImage
    {
        [SerializeField] internal PSDImportedRasterMaskImageData MaskImageData;

        internal override Vector2Int Pivot => new Vector2Int(MaskImageData.RectTangle.Left, CanvasDescription.Height - MaskImageData.RectTangle.Bottom);

        internal override void LoadImage(byte[] importSouse, RenderTexture WriteTarget)
        {

            var mat = new Material(PSDImportedRasterImage.MargeColorAndOffsetShader);
            var texR = new Texture2D(MaskImageData.RectTangle.GetWidth(), MaskImageData.RectTangle.GetHeight(), TextureFormat.R8, false);
            texR.filterMode = FilterMode.Point;

            using (var data = MaskImageData.MaskImage.GetImageData(importSouse, MaskImageData.RectTangle))
            {
                texR.LoadRawTextureData(data); texR.Apply();
            }


            mat.SetTexture("_RTex", texR);
            mat.SetTexture("_GTex", texR);
            mat.SetTexture("_BTex", texR);
            mat.SetTexture("_ATex", texR);

            mat.SetVector("_Offset", new Vector4(Pivot.x / (float)CanvasDescription.Width, Pivot.y / (float)CanvasDescription.Height, MaskImageData.RectTangle.GetWidth() / (float)CanvasDescription.Width, MaskImageData.RectTangle.GetHeight() / (float)CanvasDescription.Height));

            TextureBlend.ColorBlit(WriteTarget, new Color32(MaskImageData.DefaultValue, MaskImageData.DefaultValue, MaskImageData.DefaultValue, MaskImageData.DefaultValue));
            Graphics.Blit(null, WriteTarget, mat);
            UnityEngine.Object.DestroyImmediate(mat);
            UnityEngine.Object.DestroyImmediate(texR);
        }

        internal static NativeArray<byte> LoadPSDMaskImageData(PSDImportedRasterMaskImageData maskImageData, byte[] importSouse)
        {
            return maskImageData.MaskImage.GetImageData(importSouse, maskImageData.RectTangle);
        }

    }
}