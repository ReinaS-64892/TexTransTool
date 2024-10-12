//#pragma HLSL_Version 2018

#include "./SetSL.hlsl"
#include "./AlphaBlending.hlsl"

float4 ColorBlend(float4 BaseColor, float4 AddColor) {

  if(BaseColor.a <= 0.0){return AddColor;}
  if(AddColor.a <= 0.0){return BaseColor;}

  float3 Bcol = BaseColor.rgb;
  float3 Acol = AddColor.rgb;

  float aAlpha = AddColor.a;
  float bAlpha = BaseColor.a;

  float3 Addc = Bcol + Acol;
  float3 Mulc = Bcol * Acol;
  float3 OneCol = float3(1, 1, 1);
  float3 Scrc = OneCol - (OneCol - Bcol) * (OneCol - Acol);

  float Bsum = Bcol.r + Bcol.g + Bcol.b;
  float Asum = Acol.r + Acol.g + Acol.b;

  float3 burn =  Acol == 0 ? Acol : max( 1.0 - (1.0 - Bcol) / Acol , 0.0);
  float3 dodge = Acol == 1 ? Acol : min( Bcol / (1.0 - Acol) , 1.0);

  float3 BlendColor = float3(0, 0, 0);
#if Normal
  BlendColor = Acol;
#elif Mul
  BlendColor = Mulc;
#elif Screen
  BlendColor = Scrc;
#elif Overlay
  BlendColor = lerp(Mulc * 2, Scrc * 2 - 1, 1 - step(Bcol, 0.5)); // B >  0.5
#elif HardLight
  BlendColor = lerp(Mulc * 2, Scrc * 2 - 1, 1 - step(Acol, 0.5));
#elif SoftLight
  BlendColor = Acol > 0.5 ? Bcol +(2 * Acol - 1) * (sqrt(Bcol) - Bcol) : Bcol - (1 - 2 * Acol) * Bcol * (1 - Bcol);
#elif ColorDodge
  BlendColor = dodge;
#elif ColorBurn
  BlendColor = burn;
#elif LinearBurn
  BlendColor = Addc - 1;
#elif VividLight
  BlendColor = Acol > 0.5 ? Bcol / ( 1 - 2 * (Acol - 0.5)) : 1 - (1 - Bcol) / (2 * Acol);
#elif LinearLight
  BlendColor = saturate(Bcol + 2.0 * Acol - 1.0);
  // BlendColor = saturate(Acol > 0.5 ? Bcol + 2 * (Acol - 0.5) : Bcol + 2.0 * Acol - 1.0);
#elif Divide
  BlendColor = Acol == 0 ? 1 : Bcol / Acol;
#elif Addition
  BlendColor = saturate(Addc);
#elif Subtract
  BlendColor = Bcol - Acol;
#elif Difference
  BlendColor = abs(Bcol - Acol);
#elif DarkenOnly
  BlendColor = min(Bcol, Acol);
#elif LightenOnly
  BlendColor = max(Bcol, Acol);
#elif Hue
  BlendColor = SetLum(SetSat(Acol,GetSat(Bcol)),GetLum(Bcol));
#elif Saturation
  BlendColor = SetLum(SetSat(Bcol,GetSat(Acol)),GetLum(Bcol));
#elif Color
  BlendColor = SetLum(Acol,GetLum(Bcol));
#elif Luminosity
  BlendColor = SetLum(Bcol,GetLum(Acol));
#elif Exclusion
  BlendColor = Bcol + Acol - 2 * Bcol * Acol;
#elif DarkenColorOnly
  BlendColor =  Bsum > Asum ?  Acol : Bcol;
#elif LightenColorOnly
  BlendColor = Bsum > Asum ? Bcol : Acol;
#elif PinLight
  BlendColor = Acol > 0.5 ? max(Bcol, 2.0 * Acol - 1.0) : min(Bcol, 2.0 * Acol);
#elif HardMix
  BlendColor = ( Acol + Bcol ) > 1.0 ;
#elif AdditionGlow
  BlendColor = Bcol + Acol;
#elif ColorDodgeGlow
  BlendColor = Bcol / (1.0 -  Acol * aAlpha);
#if Clip
  return float4( BlendColor , AlphaBlending(BaseColor,AddColor,BlendColor).a);
#endif
#endif

  return AlphaBlending(BaseColor,AddColor,BlendColor);
}
