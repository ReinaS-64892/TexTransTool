//#pragma HLSL_Version 2018

float4 AlphaBlending(float4 BaseColor,float4 AddColor,float3 BlendColor)
{
  float BlendRatio = AddColor.a * BaseColor.a;
  float AddRatio = (1 - BaseColor.a) * AddColor.a;
  float BaseRatio = (1 - AddColor.a) * BaseColor.a;
  float Alpha = BlendRatio + AddRatio + BaseRatio;

#if Clip
  float3 ResultColor = (AddColor.a * BlendColor + BaseRatio * BaseColor.rgb)  / Alpha;
  ResultColor = (BaseColor.a * ResultColor + AddRatio * AddColor.rgb) / Alpha;
#else
  float3 ResultColor = (BlendColor * BlendRatio) + (AddColor.rgb * AddRatio) + (BaseColor.rgb * BaseRatio);
  ResultColor /= Alpha;
#endif
  return Alpha != 0 ? float4(ResultColor, Alpha) : float4(0, 0, 0, 0);
}
