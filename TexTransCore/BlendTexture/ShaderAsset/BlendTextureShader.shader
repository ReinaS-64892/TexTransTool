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
            #pragma multi_compile_local_fragment Normal Mul Screen Overlay HardLight SoftLight ColorDodge ColorBurn LinearBurn VividLight LinearLight Divide Addition Subtract Difference DarkenOnly LightenOnly Hue Saturation Color Luminosity AlphaLerp NotBlend

            #include "UnityCG.cginc"
            #include "../../TransTextureCore/ShaderAsset/Compute/BlendTextureHelper.hlsl"

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
                return float4(LinearToGammaSpaceExact(col.r), LinearToGammaSpaceExact(col.g), LinearToGammaSpaceExact(col.b), col.a);
            }
            float4 GammaToLinier(float4 col)
            {
                return float4(GammaToLinearSpaceExact(col.r), GammaToLinearSpaceExact(col.g), GammaToLinearSpaceExact(col.b), col.a);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = LiniearToGamma(tex2D(_DistTex, i.uv));
                float4 AddColor = LiniearToGamma(tex2D(_MainTex ,i.uv));
                float4 BlendColor;

                #ifdef Normal
                 BlendColor = ColorBlendNormal(col ,AddColor);
                #elif Mul
                 BlendColor = ColorBlendMul(col ,AddColor);
                #elif Screen
                 BlendColor = ColorBlendScreen(col ,AddColor);
                #elif Overlay
                 BlendColor = ColorBlendOverlay(col ,AddColor);
                #elif HardLight
                 BlendColor = ColorBlendHardLight(col ,AddColor);
                #elif SoftLight
                 BlendColor = ColorBlendSoftLight(col ,AddColor);
                #elif ColorDodge
                 BlendColor = ColorBlendColorDodge(col ,AddColor);
                #elif ColorBurn
                 BlendColor = ColorBlendColorBurn(col ,AddColor);
                #elif LinearBurn
                 BlendColor = ColorBlendLinearBurn(col ,AddColor);
                #elif VividLight
                 BlendColor = ColorBlendVividLight(col ,AddColor);
                #elif LinearLight
                 BlendColor = ColorBlendLinearLight(col ,AddColor);
                #elif Divide
                 BlendColor = ColorBlendDivide(col ,AddColor);
                #elif Addition
                 BlendColor = ColorBlendAddition(col ,AddColor);
                #elif Subtract
                 BlendColor = ColorBlendSubtract(col ,AddColor);
                #elif Difference
                 BlendColor = ColorBlendDifference(col ,AddColor);
                #elif DarkenOnly
                 BlendColor = ColorBlendDarkenOnly(col ,AddColor);
                #elif LightenOnly
                 BlendColor = ColorBlendLightenOnly(col ,AddColor);
                #elif Hue
                 BlendColor = ColorBlendHue(col ,AddColor);
                #elif Saturation
                 BlendColor = ColorBlendSaturation(col ,AddColor);
                #elif Color
                 BlendColor = ColorBlendColor(col ,AddColor);
                #elif Luminosity
                 BlendColor = ColorBlendLuminosity(col ,AddColor);
                #elif AlphaLerp
                 BlendColor = ColorBlendAlphaLerp(col ,AddColor);
                #elif NotBlend
                 BlendColor = AddColor;
                #endif


                return GammaToLinier(BlendColor);
            }
            ENDHLSL
        }
    }
}
