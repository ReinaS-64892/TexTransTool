using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.Utils
{
    public class PFScope : IDisposable
    {
        public PFScope(string Name = "") { Profiler.BeginSample(Name); }
        public void Dispose() { Profiler.EndSample(); }
    }
}
