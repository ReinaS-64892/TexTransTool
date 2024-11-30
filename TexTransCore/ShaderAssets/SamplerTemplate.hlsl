#ifndef READ_TEXTURE_PARM
#define READ_TEXTURE_PARM

cbuffer ReadTextureParm
{
    uint RTexWidth;
    uint RTexHeight;

    float AlimentPadding1;
    float AlimentPadding2;
}
RWTexture2D<float4> ReadTex;

float4 ClampWithLoad(int2 pos)
{
    int2 clampedPos = clamp(pos, int2(0, 0), int2(RTexWidth - 1, RTexHeight - 1));
    return ReadTex[clampedPos];
}

float4 BilinearSampling(float2 pos)
{
    float2 sourceScalePos = pos * float2(RTexWidth, RTexHeight) - float2(0.5,0.5);
    int2 ceilPos = ceil(sourceScalePos);
    int2 floorPos = floor(sourceScalePos);

    float4 upCol = lerp(ClampWithLoad(int2(floorPos.x, ceilPos.y)), ClampWithLoad(int2(ceilPos.x, ceilPos.y)), frac(sourceScalePos.x));
    float4 downCol = lerp(ClampWithLoad(int2(floorPos.x, floorPos.y)), ClampWithLoad(int2(ceilPos.x, floorPos.y)), frac(sourceScalePos.x));

    return lerp(downCol, upCol, frac(sourceScalePos.y));
}

#endif
