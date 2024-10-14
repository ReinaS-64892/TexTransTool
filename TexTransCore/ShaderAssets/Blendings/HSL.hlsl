// #pragma HLSL_Version 2018

#ifndef HSL_H
#define HSL_H

// 円柱形 Hue Saturation Lightness
float3 RGBtoHSL(float3 rgb)
{
    float xMax = max(max(rgb.r, rgb.g), rgb.b);
    float xMin = min(min(rgb.r, rgb.g), rgb.b);

    float chroma = xMax - xMin;
    float lightness = (xMax + xMin) / 2; // つまり中央値
    float value = xMax;

    float saturation = 0;
    if (lightness != 1 && lightness != 0)
    {
        saturation = (value - lightness) / min(lightness, 1 - lightness);
    }

    float hue = 0;
    if (chroma != 0)
    {
        if (xMax == rgb.r)
        {
            hue = 60 * (((rgb.g - rgb.b) / chroma) % 6);
        }
        else if (xMax == rgb.g)
        {
            hue = 60 * (((rgb.b - rgb.r) / chroma) + 2);
        }
        else if (xMax == rgb.b)
        {
            hue = 60 * (((rgb.r - rgb.g) / chroma) + 4);
        }
    }

    return float3(frac(hue / 360), saturation, lightness);
}

float3 HSLtoRGB(float3 hsl)
{
    float hue = hsl.r * 360;
    float saturation = hsl.g;
    float lightness = hsl.b;
    if (saturation == 0)
    {
        return lightness.xxx;
    }

    float3 rgb = float3(0, 0, 0);
    float c = (1 - abs((2 * lightness) - 1)) * saturation;
    float x = c * (1 - abs(((hue / 60) % 2) - 1));
    float m = lightness - c / 2;
    switch (floor(hue / 60))
    {
    default:
    case 0: // 0-60
        rgb = float3(c, x, 0);
        break;
    case 1: // 60-120
        rgb = float3(x, c, 0);
        break;
    case 2: // 120-180
        rgb = float3(0, c, x);
        break;
    case 3: // 180-240
        rgb = float3(0, x, c);
        break;
    case 4: // 240-300
        rgb = float3(x, 0, c);
        break;
    case 5: // 300-360
        rgb = float3(c, 0, x);
        break;
    }
    rgb += m;
    return rgb;
}
#endif
