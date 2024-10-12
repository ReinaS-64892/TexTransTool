//#pragma HLSL_Version 2018

// RGBtoHSV and HSVtoRGB origin of https://qiita.com/Maron_Vtuber/items/7e8e5f55dfbdf4b5da9e
// Hue Saturation Value
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
