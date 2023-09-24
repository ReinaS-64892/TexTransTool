#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static net.rs64.TexTransTool.TextureLayerUtil;

namespace net.rs64.TexTransTool
{
    public class ProgressHandler : IProgressHandling
    {
        List<string> ProgressDepth = new List<string>();
        public void ProgressStateEnter(string EnterName)
        {
            ProgressDepth.Add(EnterName);
        }

        public void ProgressStateExit()
        {
            ProgressDepth.RemoveAt(ProgressDepth.Count - 1);
        }

        public void ProgressUpdate(string State, float Value)
        {
            EditorUtility.DisplayProgressBar(string.Join("-", ProgressDepth), State, Value);
        }

        public void ProgressFinalize()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
#endif