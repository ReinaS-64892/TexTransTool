
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
