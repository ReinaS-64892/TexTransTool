// References
// MIT License Copyright (c) 2022 lilxyzw
// https://github.com/lilxyzw/lilMatCapGenerator/blob/2fa421e168b0a42526e1407456ad565b4db72911/Assets/lilMatCapGenerator/ShaderBase.txt#L142-L200
// https://web.archive.org/web/20230211165421/http://www.deepskycolors.com/archivo/2010/04/21/formulas-for-Photoshop-blending-modes.html
// http://www.simplefilter.de/en/basics/mixmods.html
// https://odashi.hatenablog.com/entry/20110921/1316610121
// https://qiita.com/kerupani129/items/4bf75d9f44a5b926df58#31-%E7%94%BB%E5%83%8F%E5%87%A6%E7%90%86%E3%82%BD%E3%83%95%E3%83%88%E3%82%A6%E3%82%A7%E3%82%A2%E3%81%AB%E3%82%88%E3%81%A3%E3%81%A6%E3%81%AF%E7%90%86%E8%AB%96%E5%BC%8F%E3%81%A8%E4%B8%80%E8%87%B4%E3%81%97%E3%81%AA%E3%81%84

#include "./HSV.hlsl"

float4 AlphaBlending(float4 BaseColor,float4 AddColor,float3 BlendColor)
{
  float BlendRatio = AddColor.a * BaseColor.a;
  float AddRatio = (1 - BaseColor.a) * AddColor.a;
  float BaseRatio = (1 - AddColor.a) * BaseColor.a;
  float Alpha = BlendRatio + AddRatio + BaseRatio;

  float3 ResultColor = (BlendColor * BlendRatio) + (AddColor.rgb * AddRatio) + (BaseColor.rgb * BaseRatio);
  ResultColor /= Alpha;

  return Alpha != 0 ? float4(ResultColor, Alpha) : float4(0, 0, 0, 0);
}

float4 ColorBlend(float4 BaseColor, float4 AddColor) {

  if(BaseColor.a <= 0.0){return AddColor;}
  if(AddColor.a <= 0.0){return BaseColor;}

  float3 Bcol = BaseColor.rgb;
  float3 Acol = AddColor.rgb;


  float3 Addc = Bcol + Acol;
  float3 Mulc = Bcol * Acol;
  float3 OneCol = float3(1, 1, 1);
  float3 Scrc = OneCol - (OneCol - Bcol) * (OneCol - Acol);

  float3 BcolPM = BaseColor.rgb * LinearToGammaSpaceExact(BaseColor.a);
  float3 AcolPM = AddColor.rgb * LinearToGammaSpaceExact(AddColor.a);

  float3 burn =  Acol == 0 ? Acol : max( 1.0 - (1.0 - Bcol) / Acol , 0.0);
  float3 dodge = Acol == 1 ? Acol : min( Bcol / (1.0 - Acol) , 1.0);

  float3 Bhsv = RGBtoHSV(Bcol);
  float3 Ahsv = RGBtoHSV(Acol);

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
  BlendColor = HSVtoRGB(float3(Ahsv.r, Bhsv.g, Bhsv.b));
#elif Saturation
  BlendColor = HSVtoRGB(float3(Bhsv.r, Ahsv.g, Bhsv.b));
#elif Color
  BlendColor = HSVtoRGB(float3(Ahsv.r, Ahsv.g, Bhsv.b));
#elif Luminosity
  BlendColor = HSVtoRGB(float3(Bhsv.r, Bhsv.g, Ahsv.b));
#endif

  return AlphaBlending(BaseColor,AddColor,BlendColor);
}