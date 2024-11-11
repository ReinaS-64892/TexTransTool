using System.IO;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.MultiLayerImage;

namespace net.rs64.TexTransTool.PSDImporter
{
    public class PSDImportedCanvasDescription : TTTImportedCanvasDescription
    {
        public int BitDepth;
        public bool IsPSB;

        public override ITTImportedCanvasSource LoadCanvasSource(string path) { return new PSDBinaryHolder(File.ReadAllBytes(path)); }
        public override TexTransCore.TexTransCoreTextureFormat ImportedImageFormat
        {
            get
            {
                switch (BitDepth)
                {
                    default:
                    case 1:
                    case 8: { return TexTransCore.TexTransCoreTextureFormat.Byte; }
                    case 16: { return TexTransCore.TexTransCoreTextureFormat.UShort; }
                    case 32: { return TexTransCore.TexTransCoreTextureFormat.Float; }
                }
            }
        }

        public class PSDBinaryHolder : ITTImportedCanvasSource
        {
            public readonly byte[] PSDByteArray;

            public PSDBinaryHolder(byte[] bytes)
            {
                PSDByteArray = bytes;
            }
        }
    }
}
