// References
// MIT License Copyright (c) 2022 lilxyzw https://github.com/lilxyzw/lilMatCapGenerator/blob/2fa421e168b0a42526e1407456ad565b4db72911/Assets/lilMatCapGenerator/ShaderBase.txt#L142-L200
// https://web.archive.org/web/20230211165421/http://www.deepskycolors.com/archivo/2010/04/21/formulas-for-Photoshop-blending-modes.html
// http://www.simplefilter.de/en/basics/mixmods.html
// https://odashi.hatenablog.com/entry/20110921/1316610121
// https://qiita.com/kerupani129/items/4bf75d9f44a5b926df58#31-%E7%94%BB%E5%83%8F%E5%87%A6%E7%90%86%E3%82%BD%E3%83%95%E3%83%88%E3%82%A6%E3%82%A7%E3%82%A2%E3%81%AB%E3%82%88%E3%81%A3%E3%81%A6%E3%81%AF%E7%90%86%E8%AB%96%E5%BC%8F%E3%81%A8%E4%B8%80%E8%87%B4%E3%81%97%E3%81%AA%E3%81%84

#include "./HSV.hlsl"

float4 ColorBlend(float4 BaseColor, float4 AddColor) {

  float3 Bcol = BaseColor.rgb;
  float3 Acol = AddColor.rgb;
  float3 BlendColor = float3(0, 0, 0);

  float3 Addc = Bcol + Acol;
  float3 Mulc = Bcol * Acol;
  float3 OneCol = float3(1, 1, 1);
  float3 Scrc = OneCol - (OneCol - Bcol) * (OneCol - Acol);

  float3 burn = Bcol == 1 ? 1 : Acol == 0 ? 0 : 1.0 - (1.0 - Bcol) / Acol;
  float3 dodge = Bcol == 0 ? 0 : Acol == 1 ? 1 : Bcol / (1.0 - Acol);

  float3 Bhsv = RGBtoHSV(Bcol);
  float3 Ahsv = RGBtoHSV(Acol);

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
  BlendColor = lerp((Acol * 2.0 - Bcol) * Bcol,
                    2.0 * (Bcol - Mulc + sqrt(Bcol) * (Acol - 0.5)),
                    1 - step(Acol, 0.5));
#elif ColorDodge
  BlendColor = dodge;
#elif ColorBurn
  BlendColor = burn;
#elif LinearBurn
  BlendColor = Addc - 1;
#elif VividLight
  BlendColor = lerp(burn * 2.0 - 1.0, dodge * 2.0, 1 - step(Acol, 0.5));
#elif LinearLight
  BlendColor = Bcol + 2.0 * Acol - 1.0;
#elif Divide
  BlendColor = Acol == 0 ? 1 : Bcol / Acol;
#elif Addition
  BlendColor = Addc;
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

  float BlendRatio = AddColor.a * BaseColor.a;
  float AddRatio = (1 - BaseColor.a) * AddColor.a;
  float BaseRatio = (1 - AddColor.a) * BaseColor.a;
  float Alpha = BlendRatio + AddRatio + BaseRatio;

  float3 ResultColor = (BlendColor * BlendRatio) + (AddColor.rgb * AddRatio) + (BaseColor.rgb * BaseRatio) / Alpha;

  return Alpha != 0 ? float4(ResultColor, Alpha) : float4(0, 0, 0, 0);
}