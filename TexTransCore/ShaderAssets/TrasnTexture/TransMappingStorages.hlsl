cbuffer gv
{
    uint2 TransTargetMapSize;
    uint2 TransSourceMapSize;
    float MaxDistance;

    float AlimentPadding1;
    float AlimentPadding2;
    float AlimentPadding3;
}

RWTexture2D<float2> TransMap;
RWTexture2D<float> DistanceMap; // MaxDistance の値を先に書き込んで初期化しておく必要がある。
RWTexture2D<float> ScalingMap;
RWTexture2D<float2> AdditionalDataMap;

// from to と 三つづつ三角形が並んでいる形、xy が 位置を意味し 0~1 の正規化された空間を想定するが、zw は何らかの情報の扱いとなる。(つまり AdditionalData ってこと)
StructuredBuffer<float4> FromPolygons;
StructuredBuffer<float2> ToPolygons;
