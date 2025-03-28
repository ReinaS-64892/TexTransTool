using System;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.Utils
{
    public class PFScope : IDisposable
    {
        public PFScope(string Name = "profile") { Profiler.BeginSample(B(Name)); }
        public PFScope(string Name = "profile", UnityEngine.Object obj = null) { Profiler.BeginSample(B(Name), obj); }

        public void Split(string Name = "profile") { Profiler.EndSample(); Profiler.BeginSample(B(Name)); }
        public void Split(string Name = "profile", UnityEngine.Object obj = null) { Profiler.EndSample(); Profiler.BeginSample(B(Name), obj); }
        public void Dispose() { Profiler.EndSample(); }

        string B(string n = "profile")
        {
            if (string.IsNullOrWhiteSpace(n)) { return "profile"; }
            return n;
        }
    }
}
