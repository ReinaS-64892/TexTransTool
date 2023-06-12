Shader "Hidden/RealTimeSimpleDecalPreview"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DecalTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local_fragment Normal Mul Screen Overlay HardLight SoftLight ColorDodge ColorBurn LinearBurn VividLight LinearLight Divide Addition Subtract Difference DarkenOnly LightenOnly Hue Saturation Color Luminosity AlphaLerp

            #include "UnityCG.cginc"
            #include "../ComputeShaders/BlendTextureHelper.hlsl"

            struct appdata
            {
                float2 uv : TEXCOORD0;
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : WORLD_POS;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _DecalTex;
            float4 _DecalTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            uniform float4x4 _WorldToDecal;

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);

                float decalflag = 0;
                float3 DecalMatrixpos = mul(_WorldToDecal ,i.worldPos).xyz;

                decalflag = max(decalflag, step(0.5,abs(DecalMatrixpos.x)));
                decalflag = max(decalflag, step(0.5,abs(DecalMatrixpos.y)));
                decalflag = max(decalflag, step(1,DecalMatrixpos.z));
                decalflag = max(decalflag, step(DecalMatrixpos.z, 0 ));

                float2 decalUvPos = float2(DecalMatrixpos.x + 0.5 ,DecalMatrixpos.y + 0.5);
                float4 DecalColor = tex2D(_DecalTex ,decalUvPos);

                float4 BlendColor;

                #ifdef Normal
                 BlendColor = ColorBlendNormal(col ,DecalColor);
                #elif Mul
                 BlendColor = ColorBlendMul(col ,DecalColor);
                #elif Screen
                 BlendColor = ColorBlendScreen(col ,DecalColor);
                #elif Overlay
                 BlendColor = ColorBlendOverlay(col ,DecalColor);
                #elif HardLight
                 BlendColor = ColorBlendHardLight(col ,DecalColor);
                #elif SoftLight
                 BlendColor = ColorBlendSoftLight(col ,DecalColor);
                #elif ColorDodge
                 BlendColor = ColorBlendColorDodge(col ,DecalColor);
                #elif ColorBurn
                 BlendColor = ColorBlendColorBurn(col ,DecalColor);
                #elif LinearBurn
                 BlendColor = ColorBlendLinearBurn(col ,DecalColor);
                #elif VividLight
                 BlendColor = ColorBlendVividLight(col ,DecalColor);
                #elif LinearLight
                 BlendColor = ColorBlendLinearLight(col ,DecalColor);
                #elif Divide
                 BlendColor = ColorBlendDivide(col ,DecalColor);
                #elif Addition
                 BlendColor = ColorBlendAddition(col ,DecalColor);
                #elif Subtract
                 BlendColor = ColorBlendSubtract(col ,DecalColor);
                #elif Difference
                 BlendColor = ColorBlendDifference(col ,DecalColor);
                #elif DarkenOnly
                 BlendColor = ColorBlendDarkenOnly(col ,DecalColor);
                #elif LightenOnly
                 BlendColor = ColorBlendLightenOnly(col ,DecalColor);
                #elif Hue
                 BlendColor = ColorBlendHue(col ,DecalColor);
                #elif Saturation
                 BlendColor = ColorBlendSaturation(col ,DecalColor);
                #elif Color
                 BlendColor = ColorBlendColor(col ,DecalColor);
                #elif Luminosity
                 BlendColor = ColorBlendLuminosity(col ,DecalColor);
                #elif AlphaLerp
                 BlendColor = ColorBlendAlphaLerp(col ,DecalColor);
                #endif


                return lerp(BlendColor, col, decalflag);
            }
            ENDHLSL
        }
    }
}
