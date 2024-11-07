using UnityEngine;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransCoreEngineForUnity
{
    public abstract class TTComputeUnityObject : ScriptableObject, ITTComputeKey
    {
        public abstract TTComputeType ComputeType { get; }
        public ComputeShader Compute;
        public const string KernelDefine = "#pragma kernel CSMain\n";
    }
}
