using System.IO;
using net.rs64.TexTransTool.MultiLayerImage;

namespace net.rs64.TexTransTool.PSDImporter
{
    public class PSDImportedCanvasDescription : TTTImportedCanvasDescription
    {
        public int BitDepth;
        public bool IsPSB;

        public override ITTImportedCanvasSource LoadCanvasSource(string path) { return new PSDBinaryHolder(File.ReadAllBytes(path)); }

        internal class PSDBinaryHolder : ITTImportedCanvasSource
        {
            public readonly byte[] PSDByteArray;

            public PSDBinaryHolder(byte[] bytes)
            {
                PSDByteArray = bytes;
            }
        }
    }
}
