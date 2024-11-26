RWTexture2D<float2> DistanceAndScaling;
RWTexture2D<float2> TransMap;

RWTexture2D<float1> TargetDistanceMap;
RWTexture2D<float4> TargetTex;

//$$$SAMPLER_CODE$$$

[numthreads(16, 16, 1)] void CSMain(uint3 id : SV_DispatchThreadID)
{
    float2 distanceAndScaling = DistanceAndScaling[id.xy];
    float targetDistance = TargetDistanceMap[id.xy];

    if(distanceAndScaling.x < targetDistance)
    {
        TargetTex[id.xy] = TTSampling(TransMap[id.xy], distanceAndScaling.y);
        TargetDistanceMap[id.xy] = distanceAndScaling.x;
    }
}
