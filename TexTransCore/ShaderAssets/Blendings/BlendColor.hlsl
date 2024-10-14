// #pragma HLSL_Version 2018

#ifndef BLENDCOLOR_H
#define BLENDCOLOR_H

#include "./SetSL.hlsl"
#include "./AlphaBlending.hlsl"

float3 NormalComposite(float3 Bcol, float3 Acol) { return Acol; }
float3 MulComposite(float3 Bcol, float3 Acol) { return Bcol * Acol; }
float3 ScreenComposite(float3 Bcol, float3 Acol)
{
  float3 OneCol = float3(1, 1, 1);
  return OneCol - (OneCol - Bcol) * (OneCol - Acol);
}
float3 OverlayComposite(float3 Bcol, float3 Acol)
{
  float3 Mulc = MulComposite(Bcol, Acol);
  float3 Scrc = ScreenComposite(Bcol, Acol);
  return lerp(Mulc * 2, Scrc * 2 - 1, 1 - step(Bcol, 0.5));
}
float3 HardLightComposite(float3 Bcol, float3 Acol)
{
  float3 Mulc = MulComposite(Bcol, Acol);
  float3 Scrc = ScreenComposite(Bcol, Acol);
  return lerp(Mulc * 2, Scrc * 2 - 1, 1 - step(Acol, 0.5));
}
float3 SoftLightComposite(float3 Bcol, float3 Acol)
{
  return Acol > 0.5 ? Bcol + (2 * Acol - 1) * (sqrt(Bcol) - Bcol) : Bcol - (1 - 2 * Acol) * Bcol * (1 - Bcol);
}
float3 ColorDodgeComposite(float3 Bcol, float3 Acol)
{
  return Acol == 1 ? Acol : min(Bcol / (1.0 - Acol), 1.0);
}
float3 ColorBurnComposite(float3 Bcol, float3 Acol)
{
  return Acol == 0 ? Acol : max(1.0 - (1.0 - Bcol) / Acol, 0.0);
}
float3 LinearBurnComposite(float3 Bcol, float3 Acol)
{
  return Bcol + Acol - 1;
}
float3 VividLightComposite(float3 Bcol, float3 Acol)
{
  return Acol > 0.5 ? Bcol / (1 - 2 * (Acol - 0.5)) : 1 - (1 - Bcol) / (2 * Acol);
}
float3 LinearLightComposite(float3 Bcol, float3 Acol)
{
  return saturate(Bcol + 2.0 * Acol - 1.0);
}
float3 DivideComposite(float3 Bcol, float3 Acol)
{
  return Acol == 0 ? 1 : Bcol / Acol;
}
float3 AdditionComposite(float3 Bcol, float3 Acol)
{
  return saturate(Bcol + Acol);
}
float3 SubtractComposite(float3 Bcol, float3 Acol)
{
  return Bcol - Acol;
}
float3 DifferenceComposite(float3 Bcol, float3 Acol)
{
  return abs(Bcol - Acol);
}
float3 DarkenOnlyComposite(float3 Bcol, float3 Acol)
{
  return min(Bcol, Acol);
}
float3 LightenOnlyComposite(float3 Bcol, float3 Acol)
{
  return max(Bcol, Acol);
}
float3 HueComposite(float3 Bcol, float3 Acol)
{
  return SetLum(SetSat(Acol, GetSat(Bcol)), GetLum(Bcol));
}
float3 SaturationComposite(float3 Bcol, float3 Acol)
{
  return SetLum(SetSat(Bcol, GetSat(Acol)), GetLum(Bcol));
}
float3 ColorComposite(float3 Bcol, float3 Acol)
{
  return SetLum(Acol, GetLum(Bcol));
}
float3 LuminosityComposite(float3 Bcol, float3 Acol)
{
  return SetLum(Bcol, GetLum(Acol));
}
float3 ExclusionComposite(float3 Bcol, float3 Acol)
{
  return Bcol + Acol - 2 * Bcol * Acol;
}
float3 DarkenColorOnlyComposite(float3 Bcol, float3 Acol)
{
  float Bsum = Bcol.r + Bcol.g + Bcol.b;
  float Asum = Acol.r + Acol.g + Acol.b;
  return Bsum > Asum ? Acol : Bcol;
}
float3 LightenColorOnlyComposite(float3 Bcol, float3 Acol)
{
  float Bsum = Bcol.r + Bcol.g + Bcol.b;
  float Asum = Acol.r + Acol.g + Acol.b;
  return Bsum > Asum ? Bcol : Acol;
}
float3 PinLightComposite(float3 Bcol, float3 Acol)
{
  return Acol > 0.5 ? max(Bcol, 2.0 * Acol - 1.0) : min(Bcol, 2.0 * Acol);
}
float3 HardMixComposite(float3 Bcol, float3 Acol)
{
  return (Acol + Bcol) > 1.0;
}
float3 AdditionGlowComposite(float3 Bcol, float3 Acol)
{
  return Bcol + Acol;
}
float3 ColorDodgeGlowComposite(float3 Bcol, float4 Acolor)
{
  return Bcol / (1.0 - Acolor.rgb * Acolor.a);
}
#endif
