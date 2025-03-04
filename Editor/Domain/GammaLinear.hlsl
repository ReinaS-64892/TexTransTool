// #pragma HLSL_Version 2018
//  Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see https://github.com/TwoTailsGames/Unity-Built-in-Shaders/blob/6a63f93bc1f20ce6cd47f981c7494e8328915621/license.txt)
//  https://github.com/TwoTailsGames/Unity-Built-in-Shaders/blob/6a63f93bc1f20ce6cd47f981c7494e8328915621/CGIncludes/UnityCG.cginc#L95-L124

inline float GammaToLinearSpaceExact(float value)
{
    if (value <= 0.04045F)
        return value / 12.92F;
    else if (value < 1.0F)
        return pow((abs(value) + 0.055F) / 1.055F, 2.4F);
    else
        return pow(abs(value), 2.2F);
}

float4 GammaToLinearSpaceExact(float4 value)
{
    return float4(GammaToLinearSpaceExact(value.r), GammaToLinearSpaceExact(value.g), GammaToLinearSpaceExact(value.b), value.a);
}
inline float LinearToGammaSpaceExact(float value)
{
    if (value <= 0.0F)
        return 0.0F;
    else if (value <= 0.0031308F)
        return 12.92F * value;
    else if (value < 1.0F)
        return 1.055F * pow(abs(value), 0.4166667F) - 0.055F;
    else
        return pow(abs(value), 0.45454545F);
}
float4 LinearToGammaSpaceExact(float4 value)
{
    return float4(LinearToGammaSpaceExact(value.r), LinearToGammaSpaceExact(value.g), LinearToGammaSpaceExact(value.b), value.a);
}
