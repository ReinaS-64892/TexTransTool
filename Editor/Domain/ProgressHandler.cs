using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool
{
    internal class ProgressHandler : IProgressHandling
    {
        List<string> _progressDepth = new List<string>();
        public void ProgressStateEnter(string enterName)
        {
            Profiler.BeginSample(enterName);
            _progressDepth.Add(enterName);
        }

        public void ProgressStateExit()
        {
            Profiler.EndSample();
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
