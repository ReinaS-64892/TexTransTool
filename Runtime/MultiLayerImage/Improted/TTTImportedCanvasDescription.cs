using UnityEngine;
using UnityEditor;
using Unity.Collections;
using System.Security.Cryptography;

namespace net.rs64.TexTransTool.MultiLayerImage
{

    public abstract class TTTImportedCanvasDescription : ScriptableObject
    {
        public int Width;
        public int Height;
    }
}