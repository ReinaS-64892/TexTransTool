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
        public static string TriangleToString(IEnumerable<TriangleIndex> filtedTriangle, IReadOnlyList<Vector3> debugvarts)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (var I in filtedTriangle)
            {
                sb.Append($"{debugvarts[I[0]].x},{debugvarts[I[0]].y},{debugvarts[I[0]].z},");
                sb.Append($"{debugvarts[I[1]].x},{debugvarts[I[1]].y},{debugvarts[I[1]].z},");
                sb.Append($"{debugvarts[I[2]].x},{debugvarts[I[2]].y},{debugvarts[I[2]].z}\n");
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

        public void Log(string messeg = "")
        {
            _StepStopwatch.Stop();
            _Stopwatch.Stop();

            ELtoLog(_StepStopwatch.Elapsed, messeg);

            _StepStopwatch.Restart();
            _Stopwatch.Start();
        }

        public void EndLog(string messeg = "")
        {
            Log(messeg);
            _Stopwatch.Stop();
            ELtoLog(_Stopwatch.Elapsed, "Total");
        }

        public static void ELtoLog(TimeSpan el, string messeg = "")
        {
            UnityEngine.Debug.Log($"{messeg} {el.Hours}h {el.Minutes}m {el.Seconds}s {el.Milliseconds}ms");
        }
    }
}
#endif
