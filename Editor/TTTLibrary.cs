using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool
{
    internal static class TTTLibrary
    {
        public readonly static string PATH = Path.Combine("Library", "net.rs64.tex-trans-tool");

        public static void CheckTTTLibraryFolder()
        {
            if (Directory.Exists(PATH)) return;
            Directory.CreateDirectory(PATH);
        }
    }
}
