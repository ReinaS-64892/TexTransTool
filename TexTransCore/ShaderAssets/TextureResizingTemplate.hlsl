
cbuffer ResizeTargetParm
{
    // サイズなので Index の最大値ではないこと
    uint2 TargetTexSize;
}

RWTexture2D<float4> TargetTex;

//$$$SAMPLER_CODE$$$

[numthreads(16, 16, 1)] void CSMain(uint3 id : SV_DispatchThreadID)
{
    float scale = sqrt((float)(RTexWidth * RTexHeight)) / sqrt((float)(TargetTexSize.x * TargetTexSize.y));
    float2 normalizedPos = (id.xy + float2(0.5, 0.5)) / (float2)TargetTexSize;
    float4 sampledCol = TTSampling(normalizedPos, scale);
    TargetTex[id.xy] = sampledCol;
}
