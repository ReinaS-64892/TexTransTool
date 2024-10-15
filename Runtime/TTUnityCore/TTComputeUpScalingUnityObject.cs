using net.rs64.TexTransCore;
using UnityEngine;

namespace net.rs64.TexTransCoreForUnity
{
    public class TTComputeUpScalingUnityObject : TTComputeUnityObject, ITTUpScalingKey
    {
        public bool HasConsiderAlpha;
        public ComputeShader WithConsiderShader;
    }
}
