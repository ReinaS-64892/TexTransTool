using net.rs64.TexTransCore;
using UnityEngine;

namespace net.rs64.TexTransCoreEngineForUnity
{
    public class TTGrabBlendingComputeShader : TTComputeUnityObject
    {
        public override TTComputeType ComputeType => TTComputeType.GrabBlend;
        public bool IsLinerRequired;
    }
}
