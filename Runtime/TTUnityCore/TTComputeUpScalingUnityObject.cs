using net.rs64.TexTransCore;
using UnityEngine;

namespace net.rs64.TexTransUnityCore
{
    public class TTComputeUpScalingUnityObject : TTComputeUnityObject, ITTUpScalingKey
    {
        public bool HasConsiderAlpha;
        public ComputeShader WithConsiderShader;
    }
}
