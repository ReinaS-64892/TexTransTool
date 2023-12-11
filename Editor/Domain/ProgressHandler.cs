#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal class ProgressHandler : IProgressHandling
    {
        List<string> _progressDepth = new List<string>();
        public void ProgressStateEnter(string enterName)
        {
            _progressDepth.Add(enterName);
        }

        public void ProgressStateExit()
        {
            _progressDepth.RemoveAt(_progressDepth.Count - 1);
        }

        public void ProgressUpdate(string state, float value)
        {
            EditorUtility.DisplayProgressBar(string.Join("-", _progressDepth), state, value);
        }

        public void ProgressFinalize()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
#endif