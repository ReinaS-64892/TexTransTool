using System;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.Utils
{
    public class PFScope : IDisposable
    {
        public PFScope(string Name = "") { Profiler.BeginSample(Name); }
        public PFScope(string Name = "", UnityEngine.Object obj = null) { Profiler.BeginSample(Name, obj); }

        public void Split(string Name = "") { Profiler.EndSample(); Profiler.BeginSample(Name); }
        public void Split(string Name = "", UnityEngine.Object obj = null) { Profiler.EndSample(); Profiler.BeginSample(Name, obj); }
        public void Dispose() { Profiler.EndSample(); }
    }
}
