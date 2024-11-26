using net.rs64.TexTransCore;
using UnityEngine;

namespace net.rs64.TexTransCoreEngineForUnity
{
    public class TTGeneralComputeOperator : TTComputeUnityObject, ITTComputeKey
    {
        public override TTComputeType ComputeType => TTComputeType.General;
        public ComputeShader Compute;
    }
}
