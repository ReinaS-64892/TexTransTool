Shader "Hidden/BlendTexture"
{
    Properties
    {
        _DistTex ("DistTexture", 2D) = "white" {}
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        LOD 100
        Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local_fragment Normal Dissolve Mul Screen Overlay HardLight SoftLight ColorDodge ColorBurn LinearBurn VividLight LinearLight Divide Addition Subtract Difference DarkenOnly LightenOnly Hue Saturation Color Luminosity AlphaLerp Exclusion DarkenColorOnly LightenColorOnly PinLight HardMix NotBlend
            #pragma shader_feature_local_fragment KeepAlpha

            #include "UnityCG.cginc"
            #include "./BlendColor.hlsl"

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
            sampler2D _DistTex;

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
                float4 BaseColor = LiniearToGamma(tex2Dlod(_DistTex,float4( i.uv,0,0)));
                float4 AddColor = LiniearToGamma(tex2Dlod(_MainTex ,float4(i.uv,0,0)));

                #if NotBlend
                float4 BlendColor = AddColor;
                #elif Dissolve
                float sAddAlpha = AddColor.a;
                AddColor.a = step(frac(sin(dot(i.uv, fixed2(12.9898f, 78.233f))) * 43758.5453),AddColor.a);
                float4 BlendColor = sAddAlpha <= 0.0 ? BaseColor : AlphaBlending(BaseColor,AddColor,AddColor.rgb);
                #else
                float4 BlendColor = ColorBlend(BaseColor,AddColor);
                #endif


                return GammaToLinear(BlendColor);
            }
            ENDHLSL
        }
    }
}
