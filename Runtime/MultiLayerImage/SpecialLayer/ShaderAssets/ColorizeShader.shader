Shader "Hidden/ColorizeShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "../../../TTUnityCore/BlendTexture/ShaderAsset/SetSL.hlsl"
            #include "../../../TTUnityCore/BlendTexture/ShaderAsset/HSL.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 LiniearToGamma(float4 col)
            {
                return float4(LinearToGammaSpaceExact(col.r), LinearToGammaSpaceExact(col.g), LinearToGammaSpaceExact(col.b), (col.a));
            }
            float4 GammaToLinear(float4 col)
            {
                return float4(GammaToLinearSpaceExact(col.r), GammaToLinearSpaceExact(col.g), GammaToLinearSpaceExact(col.b), (col.a));
            }
            

            float4 frag (v2f i) : SV_Target
            {
                float4 col = LiniearToGamma(tex2Dlod(_MainTex,float4(i.uv,0,0)));

                float3 hsl = float3(0,0,0);
                float3 targetHSL = RGBtoHSL(LiniearToGamma(_Color).rgb);

                hsl.r = targetHSL.r;
                hsl.g = targetHSL.g;

                float lum = GetLum(col.rgb);
                float lightness = targetHSL.b * 2.0 - 1.0;
                if (lightness > 0.0)
                {
                    lum = (lum * (1.0 - lightness)) + (1.0 - (1.0 - lightness));
                }
                else
                {
                    lum = lum * (lightness + 1.0);
                }
                hsl.z = lum;

                col.rgb = HSLtoRGB(hsl);

                return GammaToLinear(col);
            }
            ENDCG
        }
    }
}
