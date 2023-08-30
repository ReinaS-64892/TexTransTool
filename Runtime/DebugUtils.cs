#if UNITY_EDITOR
using System.Collections.Generic;
using System;
using net.rs64.TexTransTool;
using UnityEngine;
using System.Diagnostics;

namespace net.rs64.TexTransTool.DebugUtils
{
    public static class TriangleDebug
    {
        public static string TriangleToString(IEnumerable<TriangleIndex> triangle, IReadOnlyList<Vector3> vertex)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (var I in triangle)
            {
                sb.Append($"{vertex[I[0]].x},{vertex[I[0]].y},{vertex[I[0]].z},");
                sb.Append($"{vertex[I[1]].x},{vertex[I[1]].y},{vertex[I[1]].z},");
                sb.Append($"{vertex[I[2]].x},{vertex[I[2]].y},{vertex[I[2]].z}\n");
            }
            return sb.ToString();
        }
    }

    public class DebugTimer
    {
        Stopwatch _Stopwatch;
        Stopwatch _StepStopwatch;

        public DebugTimer()
        {
            _Stopwatch = new Stopwatch();
            _StepStopwatch = new Stopwatch();
            _Stopwatch.Start();
            _StepStopwatch.Start();
        }

        public void Log(string message = "")
        {
            _StepStopwatch.Stop();
            _Stopwatch.Stop();

            ELtoLog(_StepStopwatch.Elapsed, message);

            _StepStopwatch.Restart();
            _Stopwatch.Start();
        }

        public void EndLog(string message = "")
        {
            Log(message);
            _Stopwatch.Stop();
            ELtoLog(_Stopwatch.Elapsed, "Total");
        }

        public static void ELtoLog(TimeSpan el, string message = "")
        {
            UnityEngine.Debug.Log($"{message} {el.Hours}h {el.Minutes}m {el.Seconds}s {el.Milliseconds}ms");
        }
    }
}
#endif
