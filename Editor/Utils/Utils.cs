#if UNITY_EDITOR
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Decal;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace net.rs64.TexTransTool.Utils
{
    internal static class ToolUtils
    {
        //v0.3.x == 0
        //v0.4.x == 1
        //v0.5.x == 2
        public const int ThiSaveDataVersion = 2;
    }
}
#endif
