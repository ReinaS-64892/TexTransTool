// References - 参考資料
// MIT License Copyright (c) 2022 lilxyzw https://github.com/lilxyzw/lilMatCapGenerator/blob/2fa421e168b0a42526e1407456ad565b4db72911/Assets/lilMatCapGenerator/ShaderBase.txt#L142-L200
// https://web.archive.org/web/20230211165421/http://www.deepskycolors.com/archivo/2010/04/21/formulas-for-Photoshop-blending-modes.html
// http://www.simplefilter.de/en/basics/mixmods.html
// https://odashi.hatenablog.com/entry/20110921/1316610121
// https://qiita.com/kerupani129/items/4bf75d9f44a5b926df58

#include "./SetSL.hlsl"

float4 AlphaBlending(float4 BaseColor,float4 AddColor,float3 BlendColor)
{
  float BlendRatio = AddColor.a * BaseColor.a;
  float AddRatio = (1 - BaseColor.a) * AddColor.a;
  float BaseRatio = (1 - AddColor.a) * BaseColor.a;
  float Alpha = BlendRatio + AddRatio + BaseRatio;

#if Clip_Normal || Clip_Mul || Clip_Screen || Clip_Overlay || Clip_HardLight || Clip_SoftLight || Clip_ColorDodge || Clip_ColorBurn || Clip_LinearBurn || Clip_VividLight || Clip_LinearLight || Clip_Divide || Clip_Addition || Clip_Subtract || Clip_Difference || Clip_DarkenOnly || Clip_LightenOnly || Clip_Hue || Clip_Saturation || Clip_Color || Clip_Luminosity || Clip_Exclusion || Clip_DarkenColorOnly || Clip_LightenColorOnly || Clip_PinLight || Clip_HardMix || Clip_AdditionGlow || Clip_ColorDodgeGlow
  float3 ResultColor = (AddColor.a * BlendColor + BaseRatio * BaseColor.rgb)  / Alpha;
  ResultColor = (BaseColor.a * ResultColor + AddRatio * AddColor.rgb) / Alpha;
#else
  float3 ResultColor = (BlendColor * BlendRatio) + (AddColor.rgb * AddRatio) + (BaseColor.rgb * BaseRatio);
  ResultColor /= Alpha;
#endif
  return Alpha != 0 ? float4(ResultColor, Alpha) : float4(0, 0, 0, 0);
}

float4 ColorBlend(float4 BaseColor, float4 AddColor) {

  if(BaseColor.a <= 0.0){return AddColor;}
  if(AddColor.a <= 0.0){return BaseColor;}

  float3 Bcol = BaseColor.rgb;
  float3 Acol = AddColor.rgb;

  float aAlpha = AddColor.a;
  float bAlpha = BaseColor.a;

  float aAlphaG = LinearToGammaSpaceExact(AddColor.a);
  float bAlphaG = LinearToGammaSpaceExact(BaseColor.a);


  float3 Addc = Bcol + Acol;
  float3 Mulc = Bcol * Acol;
  float3 OneCol = float3(1, 1, 1);
  float3 Scrc = OneCol - (OneCol - Bcol) * (OneCol - Acol);

  float Bsum = Bcol.r + Bcol.g + Bcol.b;
  float Asum = Acol.r + Acol.g + Acol.b;

  float3 burn =  Acol == 0 ? Acol : max( 1.0 - (1.0 - Bcol) / Acol , 0.0);
  float3 dodge = Acol == 1 ? Acol : min( Bcol / (1.0 - Acol) , 1.0);

  float3 BlendColor = float3(0, 0, 0);
#if Normal || Clip_Normal
  BlendColor = Acol;
#elif Mul || Clip_Mul
  BlendColor = Mulc;
#elif Screen || Clip_Screen
  BlendColor = Scrc;
#elif Overlay || Clip_Overlay
  BlendColor = lerp(Mulc * 2, Scrc * 2 - 1, 1 - step(Bcol, 0.5)); // B >  0.5
#elif HardLight || Clip_HardLight
  BlendColor = lerp(Mulc * 2, Scrc * 2 - 1, 1 - step(Acol, 0.5));
#elif SoftLight || Clip_SoftLight
  BlendColor = Acol > 0.5 ? Bcol +(2 * Acol - 1) * (sqrt(Bcol) - Bcol) : Bcol - (1 - 2 * Acol) * Bcol * (1 - Bcol);
#elif ColorDodge || Clip_ColorDodge
  BlendColor = dodge;
#elif ColorBurn || Clip_ColorBurn
  BlendColor = burn;
#elif LinearBurn || Clip_LinearBurn
  BlendColor = Addc - 1;
#elif VividLight || Clip_VividLight
  BlendColor = Acol > 0.5 ? Bcol / ( 1 - 2 * (Acol - 0.5)) : 1 - (1 - Bcol) / (2 * Acol);
#elif LinearLight || Clip_LinearLight
  BlendColor = saturate(Bcol + 2.0 * Acol - 1.0);
  // BlendColor = saturate(Acol > 0.5 ? Bcol + 2 * (Acol - 0.5) : Bcol + 2.0 * Acol - 1.0);
#elif Divide || Clip_Divide
  BlendColor = Acol == 0 ? 1 : Bcol / Acol;
#elif Addition || Clip_Addition
  BlendColor = saturate(Addc);
#elif Subtract || Clip_Subtract
  BlendColor = Bcol - Acol;
#elif Difference || Clip_Difference
  BlendColor = abs(Bcol - Acol);
#elif DarkenOnly || Clip_DarkenOnly
  BlendColor = min(Bcol, Acol);
#elif LightenOnly || Clip_LightenOnly
  BlendColor = max(Bcol, Acol);
#elif Hue || Clip_Hue
  BlendColor = SetLum(SetSat(Acol,GetSat(Bcol)),GetLum(Bcol));
#elif Saturation || Clip_Saturation
  BlendColor = SetLum(SetSat(Bcol,GetSat(Acol)),GetLum(Bcol));
#elif Color || Clip_Color
  BlendColor = SetLum(Acol,GetLum(Bcol));
#elif Luminosity || Clip_Luminosity
  BlendColor = SetLum(Bcol,GetLum(Acol));
#elif Exclusion || Clip_Exclusion
  BlendColor = Bcol + Acol - 2 * Bcol * Acol;
#elif DarkenColorOnly || Clip_DarkenColorOnly
  BlendColor =  Bsum > Asum ?  Acol : Bcol;
#elif LightenColorOnly || Clip_LightenColorOnly
  BlendColor = Bsum > Asum ? Bcol : Acol;
#elif PinLight || Clip_PinLight
  BlendColor = Acol > 0.5 ? max(Bcol, 2.0 * Acol - 1.0) : min(Bcol, 2.0 * Acol);
#elif HardMix || Clip_HardMix
  BlendColor = ( Acol + Bcol ) > 1.0 ;
#elif AdditionGlow || Clip_AdditionGlow
  BlendColor = Bcol + Acol;
#elif ColorDodgeGlow || Clip_ColorDodgeGlow
  BlendColor = Bcol / (1.0 -  Acol * aAlpha);

#if Clip_ColorDodgeGlow
  return float4( BlendColor , AlphaBlending(BaseColor,AddColor,BlendColor).a);
#endif

#endif

  return AlphaBlending(BaseColor,AddColor,BlendColor);
}
