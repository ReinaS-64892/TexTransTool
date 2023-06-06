#include "./TransHelper.hlsl"

static float4 OneColor = float4(1,1,1,1);

float2 FinalAlphaAndReversCal(float BaseAlpha, float AddAlpha) {
  float AddRevAlpha = 1 - AddAlpha;
  float Alpha = AddAlpha + (BaseAlpha * AddRevAlpha);
  return float2(Alpha, AddRevAlpha);
}
// RGBtoHSV and HSVtoRGB origin of https://qiita.com/Maron_Vtuber/items/7e8e5f55dfbdf4b5da9e
float3 RGBtoHSV(float3 rgb)
{
	float r = rgb.r;
	float g = rgb.g;
	float b = rgb.b;

	float max = r > g ? r : g;
	max = max > b ? max : b;
	float min = r < g ? r : g;
	min = min < b ? min : b;
	float h = max - min;

	float h_r = (g - b) / h;
	h_r += (h_r < 0.0) ? 6.0 : 0.0;
	float h_g = 2.0 + (b - r) / h;
	float h_b = 4.0 + (r - g) / h;

	h = (h > 0.0) ? ((max == r) ? h_r : ((max == g) ? h_g : h_b)) : h;

	h /= 6.0;
	float s = (max - min);
	s = (max != 0.0) ? s /= max : s;
	float v = max;

	float3 hsv;
	hsv.x = h;
	hsv.y = s;
	hsv.z = v;
	return hsv;
}
float3 HSVtoRGB(float3 hsv)
{
	float h = hsv.x;
	float s = hsv.y;
	float v = hsv.z;

	float r = v;
	float g = v;
	float b = v;

    h *= 6.0;
    float f = frac(h);
    switch (floor(h)) {
        default:
        case 0:
            g *= 1 - s * (1 - f);
            b *= 1 - s;
            break;
        case 1:
            r *= 1 - s * f;
            b *= 1 - s;
            break;
        case 2:
            r *= 1 - s;
            b *= 1 - s * (1 - f);
            break;
        case 3:
            r *= 1 - s;
            g *= 1 - s * f;
            break;
        case 4:
            r *= 1 - s * (1 - f);
            g *= 1 - s;
            break;
        case 5:
            g *= 1 - s;
            b *= 1 - s * f;
            break;
    }

	r = (s > 0.0) ? r : v;
	g = (s > 0.0) ? g : v;
	b = (s > 0.0) ? b : v;

	float3 rgb;
	rgb.r = r;
	rgb.g = g;
	rgb.b = b;
	return rgb;
}
//this far

float4 ColorBlendNormal(float4 BaseColor, float4 AddColor) {

  float2 Alpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w);
  float4 ResultColor = (AddColor * AddColor.w) + ((BaseColor * BaseColor.w) * Alpha.y);
  ResultColor.w = Alpha.x;

  return ResultColor;
}
float4 ColorBlendMul(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;
  float4 MulColor = BaseColor * AddColor;
  float4 ResultColor = lerp(BaseColor, MulColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor;
}
float4 ColorBlendScreen(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;
  float4 BlendColor = OneColor - (OneColor - BaseColor )* (OneColor - AddColor);
  float4 ResultColor = lerp(BaseColor, BlendColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor;
}
float4 ColorBlendOverlay(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;

  float4 MulColor = 2 *  BaseColor * AddColor;
  float4 ScreenColor = OneColor - (OneColor - BaseColor )* (OneColor - AddColor);
  float4 BlendColor = lerp(MulColor,ScreenColor,(1 - step(BaseColor.w, 0.5)));

  float4 ResultColor = lerp(BaseColor, BlendColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor;
}
float4 ColorBlendHardLight(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;

  float4 MulColor = 2 *  BaseColor * AddColor;
  float4 ScreenColor = OneColor - (OneColor - BaseColor )* (OneColor - AddColor);
  float4 BlendColor = lerp(MulColor,ScreenColor,(1 - step(AddColor.z, 0.5)));

  float4 ResultColor = lerp(BaseColor, BlendColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor;
}
float4 ColorBlendSoftLight(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;

  float4 BlendColor = (OneColor - 2 * AddColor) * (BaseColor * BaseColor) + 2 * BaseColor * AddColor;

  float4 ResultColor = lerp(BaseColor, BlendColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor;
}
float4 ColorBlendColorDodge(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;

  float4 BlendColor = float4(
    saturate(BaseColor.x / (1 - AddColor.x)),
    saturate(BaseColor.y / (1 - AddColor.y)),
    saturate(BaseColor.z / (1 - AddColor.z)),
    1
  );

  float4 ResultColor = lerp(BaseColor, BlendColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor;
}
float4 ColorBlendColorBurn(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;

    float4 BlendColor = float4(
    1 - saturate((1 - BaseColor.x) / AddColor.x),
    1 - saturate((1 - BaseColor.y) / AddColor.y),
    1 - saturate((1 - BaseColor.z) / AddColor.z),
    1
  );

  float4 ResultColor = lerp(BaseColor, BlendColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor;
}
float4 ColorBlendLinearBurn(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;

  float4 BlendColor = BaseColor + AddColor - OneColor;

  float4 ResultColor = lerp(BaseColor, BlendColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor;
}
float4 ColorBlendVividLight(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;

  float4 Dodge = float4(
    saturate(1 - (1 - BaseColor.r) / (2 * AddColor.r)),
    saturate(1 - (1 - BaseColor.g) / (2 * AddColor.g)),
    saturate(1 - (1 - BaseColor.b) / (2 * AddColor.b)),
    1
  );
  float4 Burn = float4(
   saturate( BaseColor.r / (1 - 2 * (AddColor.r - 0.5))),
    saturate(BaseColor.g / (1 - 2 * (AddColor.g - 0.5))),
    saturate(BaseColor.b / (1 - 2 * (AddColor.b - 0.5))),
    1
  );
  float4 BlendColor = float4(
    lerp(Dodge.x,Burn.x,(1 - step(AddColor.x, 0.5))),
    lerp(Dodge.y,Burn.y,(1 - step(AddColor.y, 0.5))),
    lerp(Dodge.z,Burn.z,(1 - step(AddColor.z, 0.5))),
    1
  );

  float4 ResultColor = lerp(BaseColor, BlendColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor; 
}
float4 ColorBlendLinearLight(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;
  float4 BlendColor = BaseColor + (2 * AddColor) - OneColor;
  float4 ResultColor = lerp(BaseColor, BlendColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor; 
}
float4 ColorBlendDivide(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;
  float4 BlendColor = float4(
    saturate(BaseColor.x / AddColor.x),
    saturate(BaseColor.y / AddColor.y),
    saturate(BaseColor.z / AddColor.z),
    1
  );
  float4 ResultColor = lerp(BaseColor, BlendColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor; 
}
float4 ColorBlendAddition(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;
  float4 BlendColor = float4(
    saturate(BaseColor.x + AddColor.x),
    saturate(BaseColor.y + AddColor.y),
    saturate(BaseColor.z + AddColor.z),
    1
  );
  float4 ResultColor = lerp(BaseColor, BlendColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor; 
}
float4 ColorBlendSubtract(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;
  float4 BlendColor = float4(
    saturate(BaseColor.x - AddColor.x),
    saturate(BaseColor.y - AddColor.y),
    saturate(BaseColor.z - AddColor.z),
    1
  );
  float4 ResultColor = lerp(BaseColor, BlendColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor; 
}
float4 ColorBlendDifference(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;
  float4 BlendColor = float4(
    abs(BaseColor.x - AddColor.x),
    abs(BaseColor.y - AddColor.y),
    abs(BaseColor.z - AddColor.z),
    1
  );
  float4 ResultColor = lerp(BaseColor, BlendColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor; 
}
float4 ColorBlendDarkenOnly(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;
  float4 BlendColor = float4(
    min(BaseColor.x ,AddColor.x),
    min(BaseColor.y , AddColor.y),
    min(BaseColor.z , AddColor.z),
    1
  );
  float4 ResultColor = lerp(BaseColor, BlendColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor; 
}
float4 ColorBlendLightenOnly(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;
  float4 BlendColor = float4(
    max(BaseColor.x , AddColor.x),
    max(BaseColor.y , AddColor.y),
    max(BaseColor.z , AddColor.z),
    1
  );
  float4 ResultColor = lerp(BaseColor, BlendColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor; 
}
float4 ColorBlendHue(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;
  float3 BaseHSV = RGBtoHSV((float3)BaseColor);
  float3 AddHSV = RGBtoHSV((float3)AddColor);
  float4 BlendColor = AsFloat4(HSVtoRGB(float3(AddHSV.x,BaseHSV.y,BaseHSV.z)));
  float4 ResultColor = lerp(BaseColor, BlendColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor;
}
float4 ColorBlendSaturation(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;
  float3 BaseHSV = RGBtoHSV((float3)BaseColor);
  float3 AddHSV = RGBtoHSV((float3)AddColor);
  float4 BlendColor = AsFloat4(HSVtoRGB(float3(BaseHSV.x,AddHSV.y,BaseHSV.z)));
  float4 ResultColor = lerp(BaseColor, BlendColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor;
}
float4 ColorBlendColor(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;
  float3 BaseHSV = RGBtoHSV((float3)BaseColor);
  float3 AddHSV = RGBtoHSV((float3)AddColor);
  float4 BlendColor = AsFloat4(HSVtoRGB(float3(AddHSV.x,AddHSV.y,BaseHSV.z)));
  float4 ResultColor = lerp(BaseColor, BlendColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor;
}
float4 ColorBlendLuminosity(float4 BaseColor, float4 AddColor) {

  float FinalAlpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w).x;
  float3 BaseHSV = RGBtoHSV((float3)BaseColor);
  float3 AddHSV = RGBtoHSV((float3)AddColor);
  float4 BlendColor = AsFloat4(HSVtoRGB(float3(BaseHSV.x,BaseHSV.y,AddHSV.z)));
  float4 ResultColor = lerp(BaseColor, BlendColor, AddColor.w);
  ResultColor.w = FinalAlpha;

  return ResultColor;
}
float4 ColorBlendAlphaLerp(float4 BaseColor, float4 AddColor) {

  float2 Alpha = FinalAlphaAndReversCal(BaseColor.w, AddColor.w);
  float4 ResultColor = lerp(BaseColor, AddColor, AddColor.w / (AddColor.w +( Alpha.y * BaseColor.w )));
  ResultColor.w = Alpha.x;

  return ResultColor;
}

float4 ColorBlender(float4 BaseColor, float4 AddColor){
  float4 ResultColor = float4(0,0,0,0);


  return ResultColor;
}