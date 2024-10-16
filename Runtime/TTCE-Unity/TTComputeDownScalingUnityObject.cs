using net.rs64.TexTransCore;
using UnityEngine;

namespace net.rs64.TexTransCoreEngineForUnity
{
    public class TTComputeDownScalingUnityObject : TTComputeUnityObject, ITTDownScalingKey
    {
        public bool HasConsiderAlpha;
        public ComputeShader WithConsiderShader;
        public const string ConsiderAlphaDefine = "#define ConsiderAlpha 1\n";
    }
}
