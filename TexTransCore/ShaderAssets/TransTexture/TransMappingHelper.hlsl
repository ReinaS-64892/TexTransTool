
float TriangleArea(float2 tri[3])
{
    float a = cross(float3(tri[1] - tri[0], 0), float3(tri[2] - tri[0], 0)).z;
    return a;
}
float3 Barycentric(float2 tri[3], float2 p)
{
    float u = cross(float3(tri[2] - tri[1], 0), float3(p - tri[1], 0)).z;
    float v = cross(float3(tri[0] - tri[2], 0), float3(p - tri[2], 0)).z;
    float w = cross(float3(tri[1] - tri[0], 0), float3(p - tri[0], 0)).z;
    float a = TriangleArea(tri);
    return float3(u, v, w) / a;
}
float2 CalculatePositionFromBarycentric(float2 tri[3], float3 bc)
{
    float2 pos = float2(0, 0);
    pos += tri[0] * bc.x;
    pos += tri[1] * bc.y;
    pos += tri[2] * bc.z;
    return pos;
}
float3 CalculatePositionFromBarycentricWithFloat3(float3 tri[3], float3 bc)
{
    float3 pos = float3(0, 0, 0);
    pos += tri[0] * bc.x;
    pos += tri[1] * bc.y;
    pos += tri[2] * bc.z;
    return pos;
}
float2 Line2Near(float2 v1, float2 v2, float2 p)
{
    float2 vLine = v2 - v1;
    float vLength = length(vLine);
    float2 vLineNormalized = normalize(vLine);
    float onLineLength = clamp(dot(vLineNormalized, p - v1), 0, vLength);
    return v1 + (vLineNormalized * onLineLength);
}
float3 Line2NearWthFloat3(float3 v1, float3 v2, float3 p)
{
    float3 vLine = v2 - v1;
    float vLength = length(vLine);
    float3 vLineNormalized = normalize(vLine);
    float onLineLength = clamp(dot(vLineNormalized, p - v1), 0, vLength);
    return v1 + (vLineNormalized * onLineLength);
}
float Distance(float2 tri[3], float2 p)
{
    float2 np0 = Line2Near(tri[0], tri[1], p) - p;
    float2 np1 = Line2Near(tri[1], tri[2], p) - p;
    float2 np2 = Line2Near(tri[2], tri[0], p) - p;

    float npl0 = np0.x * np0.x + np0.y * np0.y;
    float npl1 = np1.x * np1.x + np1.y * np1.y;
    float npl2 = np2.x * np2.x + np2.y * np2.y;
    return sqrt(min(min(npl0, npl1), npl2));
}

float InsideBarycentric(float3 bc)
{
    float v = 0;
    v += bc.x >= 0 ? 1 : -1;
    v += bc.y >= 0 ? 1 : -1;
    v += bc.z >= 0 ? 1 : -1;
    return ceil(saturate(abs(v) - 2.5));
}
bool IsNaNWithWGSLSafe(float v)
{
    float minFloat = 1.175494351e-38;
    return !(minFloat <= v);
}
float3 checkNaN(float3 val, float3 replace)
{
    if (IsNaNWithWGSLSafe(val.r))
    {
        val.r = replace.r;
    }
    if (IsNaNWithWGSLSafe(val.g))
    {
        val.g = replace.g;
    }
    if (IsNaNWithWGSLSafe(val.b))
    {
        val.b = replace.b;
    }
    return val;
}
