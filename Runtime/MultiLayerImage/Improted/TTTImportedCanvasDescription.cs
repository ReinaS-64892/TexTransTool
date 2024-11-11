using UnityEngine;
using UnityEditor;
using Unity.Collections;
using System.Security.Cryptography;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransTool.MultiLayerImage
{

    public abstract class TTTImportedCanvasDescription : ScriptableObject
    {
        public int Width;
        public int Height;

        public abstract TexTransCoreTextureFormat ImportedImageFormat { get; }
        public abstract ITTImportedCanvasSource LoadCanvasSource(string path);
    }
    public interface ITTImportedCanvasSource { }
}
