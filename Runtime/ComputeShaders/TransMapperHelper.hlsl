float2 NeaPointOnLine(float2 a, float2 b, float2 p)
{
    float2 ab = b - a;
    float Leng = length(ab);
    ab = normalize(ab);
    float lp = clamp(dot(p - a, ab), 0, Leng);
    return a + (lp * ab);
}

float2 FromBCS(float2 t1, float2 t2, float2 t3, float3 BCS)
{
    float2 RevPos2 = float2(0, 0);
    RevPos2 += t1 * BCS.x;
    RevPos2 += t2 * BCS.y;
    RevPos2 += t3 * BCS.z;
    return RevPos2;
}

float IsInCal(float w, float u, float v)
{
    return saturate(
        ceil(
            abs(
                ceil(clamp(w, -1, 1)) +
                ceil(clamp(u, -1, 1)) +
                ceil(clamp(v, -1, 1))) -
            2.00));
}

float3 DistansVartBase(float2 t1, float2 t2, float2 t3, float2 tp)
{

    float DistansA = distance(t1, tp);
    float DistansB = distance(t2, tp);
    float DistansC = distance(t3, tp);
    return float3(DistansA, DistansB, DistansC);
}
float3 DistansEdgeBase(float2 t1, float2 t2, float2 t3, float2 tp)
{
    float DistansA = distance(NeaPointOnLine(t1, t2, tp), tp);
    float DistansB = distance(NeaPointOnLine(t2, t3, tp), tp);
    float DistansC = distance(NeaPointOnLine(t3, t1, tp), tp);
    return float3(DistansA, DistansB, DistansC);
}

float4 ClossTraiangle(float3 t1, float3 t2, float3 t3, float3 tp)
{

    float w = cross(t3 - t2, tp - t2).z;
    float u = cross(t1 - t3, tp - t3).z;
    float v = cross(t2 - t1, tp - t1).z;
    float wuv = cross(t2 - t1, t3 - t1).z;
    return float4(w, u, v, wuv);
}

uint SelectTraiangle(uint id, uint ti)
{
    return ((id * 6) + ti);
}

float MinVector(float3 Vector)
{
    return min(Vector.x, min(Vector.y, Vector.z));
}

float3 ToBCS(float4 ClossT)
{
    return float3((ClossT.x/ClossT.w),(ClossT.y/ClossT.w),(ClossT.z/ClossT.w));
}
float isDistansCla(float3 Distans, float IsIn)
{
    return lerp((MinVector(Distans) * -1), 1, IsIn);
}