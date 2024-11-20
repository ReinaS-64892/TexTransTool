using UnityEngine;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransCoreEngineForUnity
{
    public abstract class TTComputeUnityObject : ScriptableObject
    {
        public abstract TTComputeType ComputeType { get; }
        public const string KernelDefine = "#pragma kernel CSMain\n";
    }
}
