#include "./TransHelper.hlsl"

float4 GetColorBiliner(RWStructuredBuffer<float4> Souse, uint2 SourceTexSize,float2 GetPos) {
  uint2 TexArrySize = SourceTexSize - uint2(1, 1);
  float2 pos = clamp(TexArrySize * GetPos,0,TexArrySize);

  uint XC = ceil(pos.x);
  uint XF = floor(pos.x);
  uint YC = ceil(pos.y);
  uint YF = floor(pos.y);

  float PosX  = pos.x - XF;
  float PosY = pos.y - YF;

  float4 UpColor = lerp(Souse[TwoDToOneDIndex(uint2(XF,YC),SourceTexSize.x)], Souse[TwoDToOneDIndex(uint2(XC, YC),SourceTexSize.x)], PosX);
  float4 DownColor = lerp(Souse[TwoDToOneDIndex(uint2(XF, YF),SourceTexSize.x)], Souse[TwoDToOneDIndex(uint2(XC, YF),SourceTexSize.x)],PosX);
  return lerp(DownColor, UpColor,PosY);
}