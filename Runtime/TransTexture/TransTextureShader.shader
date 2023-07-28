Shader "Hidden/TransTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Pading ("Pading", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Geometry" }
        LOD 100
        GrabPass {}
        Pass
        {
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma multi_compile_local_fragment Normal Mul Screen Overlay HardLight SoftLight ColorDodge ColorBurn LinearBurn VividLight LinearLight Divide Addition Subtract Difference DarkenOnly LightenOnly Hue Saturation Color Luminosity AlphaLerp NotBlend

            #include "UnityCG.cginc"
            #include "../ComputeShaders/BlendTextureHelper.hlsl"

            struct appdata
            {
                float2 uv : TEXCOORD0;
                float4 vertex : POSITION;
            };

            struct v2g
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            struct g2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            sampler2D _MainTex;
            sampler2D _GrabTexture;
            float _Pading;

            v2g vert (appdata v)
            {
                v2g o;
                o.vertex = v.vertex;
                o.vertex.x *= 2;
                o.vertex.y *= 2;
                o.vertex.x -=1;
                o.vertex.y -=1;
#if UNITY_UV_STARTS_AT_TOP
                o.vertex.y = -1 * o.vertex.y;
#endif
                o.uv = v.uv;
                return o;
            }

            g2f PadingCal(v2g InMain,v2g inBack,v2g InFora,float Pading)
            {
                float2 middleUVPos = lerp(inBack.uv ,InFora.uv ,0.5);
                float4 middleVartPos = lerp(inBack.vertex ,InFora.vertex ,0.5);
                float lerpValue = -1 * Pading;
                g2f o;
                o.uv = lerp(InMain.uv ,middleUVPos ,lerpValue);
                o.vertex = lerp(InMain.vertex ,middleVartPos ,lerpValue);
                return o;
            }

            [maxvertexcount(3)]
            void geom (triangle v2g input[3], inout TriangleStream<g2f> stream)
            {
                stream.Append(PadingCal(input[0] ,input[2] ,input[1] ,_Pading));
                stream.Append(PadingCal(input[1] ,input[0] ,input[2] ,_Pading));
                stream.Append(PadingCal(input[2] ,input[1] ,input[0] ,_Pading));
                stream.RestartStrip();
            }

            float4 LiniearToGamma(float4 col)
            {
                return float4(LinearToGammaSpaceExact(col.r), LinearToGammaSpaceExact(col.g), LinearToGammaSpaceExact(col.b), col.a);
            }
            float4 GammaToLinier(float4 col)
            {
                return float4(GammaToLinearSpaceExact(col.r), GammaToLinearSpaceExact(col.g), GammaToLinearSpaceExact(col.b), col.a);
            }

            fixed4 frag (g2f i) : SV_Target
            {
#if !UNITY_COLORSPACE_GAMMA
                float4 DistColor = LiniearToGamma(tex2D(_GrabTexture, i.uv));
                float4 AddColor = LiniearToGamma(tex2D(_MainTex ,i.uv));
#else
                float4 DistColor = tex2D(_GrabTexture, i.uv);
                float4 AddColor = tex2D(_MainTex ,i.uv);
#endif
                float4 BlendColor;

                #ifdef Normal
                    BlendColor = ColorBlendNormal(DistColor ,AddColor);
                #elif Mul
                    BlendColor = ColorBlendMul(DistColor ,AddColor);
                #elif Screen
                    BlendColor = ColorBlendScreen(DistColor ,AddColor);
                #elif Overlay
                    BlendColor = ColorBlendOverlay(DistColor ,AddColor);
                #elif HardLight
                    BlendColor = ColorBlendHardLight(DistColor ,AddColor);
                #elif SoftLight
                    BlendColor = ColorBlendSoftLight(DistColor ,AddColor);
                #elif ColorDodge
                    BlendColor = ColorBlendColorDodge(DistColor ,AddColor);
                #elif ColorBurn
                    BlendColor = ColorBlendColorBurn(DistColor ,AddColor);
                #elif LinearBurn
                    BlendColor = ColorBlendLinearBurn(DistColor ,AddColor);
                #elif VividLight
                    BlendColor = ColorBlendVividLight(DistColor ,AddColor);
                #elif LinearLight
                    BlendColor = ColorBlendLinearLight(DistColor ,AddColor);
                #elif Divide
                    BlendColor = ColorBlendDivide(DistColor ,AddColor);
                #elif Addition
                    BlendColor = ColorBlendAddition(DistColor ,AddColor);
                #elif Subtract
                    BlendColor = ColorBlendSubtract(DistColor ,AddColor);
                #elif Difference
                    BlendColor = ColorBlendDifference(DistColor ,AddColor);
                #elif DarkenOnly
                    BlendColor = ColorBlendDarkenOnly(DistColor ,AddColor);
                #elif LightenOnly
                    BlendColor = ColorBlendLightenOnly(DistColor ,AddColor);
                #elif Hue
                    BlendColor = ColorBlendHue(DistColor ,AddColor);
                #elif Saturation
                    BlendColor = ColorBlendSaturation(DistColor ,AddColor);
                #elif Color
                    BlendColor = ColorBlendColor(DistColor ,AddColor);
                #elif Luminosity
                    BlendColor = ColorBlendLuminosity(DistColor ,AddColor);
                #elif AlphaLerp
                    BlendColor = ColorBlendAlphaLerp(DistColor ,AddColor);
                #elif NotBlend
                    BlendColor = AddColor;
                #endif

#if !UNITY_COLORSPACE_GAMMA
                return GammaToLinier(BlendColor);
#else
                return BlendColor;
#endif

            }
            ENDHLSL
        }
    }
}
