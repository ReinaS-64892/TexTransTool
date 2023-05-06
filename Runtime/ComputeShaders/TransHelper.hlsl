
uint TwoDToOneDIndex(uint2 id, uint Size)
{
    return (id.y * Size) + id.x;
}

float3 AsFloat3(float2 f)
{
    return float3(f.x, f.y, 0);
}

float4 AsFloat4(float3 f)
{
    return float4(f.x, f.y, f.z,0);
}

const float MinValue = -3.4028235E+38;
const float MaxValue = 3.4028235E+38F;

float NaNCheck(float Target){
    return isnan(Target) ? 0 : Target;
}