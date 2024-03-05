Shader "Hidden/HSLAdjustment"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Hue ("Hue", float) = 0
        _Saturation ("Saturation",float) = 0
        _Lightness ("Brightness",float) = 0

    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        LOD 100
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "../../../../TexTransCore/BlendTexture/ShaderAsset/HSV.hlsl"

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
            float _Hue;
            float _Saturation;
            float _Lightness;

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
                float4 col = LiniearToGamma(tex2Dlod(_MainTex,float4( i.uv,0,0)));
                float3 hsvCol = RGBtoHSV(col.rgb);

                hsvCol.r = frac(hsvCol.r + (_Hue * 0.5));

                col.rgb = HSVtoRGB(hsvCol);

                float cMin = min(col.r , min(col.g , col.b));
                float cMax = max(col.r , max(col.g , col.b));
                float delta = cMax - cMin;
                float value = cMax + cMin;

                float lightness = value * 0.5;
                float saturation = lightness < 0.5 ? delta / value  : delta / (2 - value);
                if(_Saturation >= 0.0)
                {
                    float moreVal = (_Saturation + saturation) >= 1.0 ? saturation : 1.000001 - _Saturation;
                    moreVal = 1.0 / moreVal - 1.0;
                    col.rgb = saturate( col.rgb + ( (col.rgb -  lightness)  *  moreVal ));
                }
                else
                {
                    col.rgb = saturate( lightness + ( (col.rgb - lightness)  *   (1 + _Saturation) ) );
                }

                col.rgb = _Lightness > 0 ? lerp( col.rgb , 1, _Lightness) : lerp( col.rgb , 0 ,-1 * _Lightness);

                return GammaToLinear(col);
            }
            ENDHLSL
        }
    }
}
