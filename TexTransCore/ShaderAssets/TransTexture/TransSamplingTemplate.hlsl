RWTexture2D<float2> TransMap;
RWTexture2D<float> DistanceMap;
RWTexture2D<float> ScalingMap;

RWTexture2D<float> TargetDistanceMap;
RWTexture2D<float4> TargetTex;

//$$$SAMPLER_CODE$$$

[numthreads(16, 16, 1)] void CSMain(uint3 id : SV_DispatchThreadID)
{
    uint2 i = id.xy;

    float distance = DistanceMap[i];
    float targetDistance = TargetDistanceMap[i];

    if(distance < targetDistance)
    {
        TargetTex[i] = TTSampling(TransMap[i], ScalingMap[i]);
        TargetDistanceMap[i] = distance;
    }
}
