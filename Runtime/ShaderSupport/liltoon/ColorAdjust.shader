Shader "Hidden/ColorAdjustShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HSVG ("Color", Color) = (1,1,1,1)
        _Mask ("Mask", 2D) = "white" {}
        _UseMask ("Use Mask", Float) = 0
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

            #include "UnityCG.cginc"

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
            float4 _HSVG;
            sampler2D _Mask;
            float _UseMask;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            //https://github.com/lilxyzw/lilToon/blob/2ef370dc444172787c075ec3a822438c2bee26cb/Assets/lilToon/Shader/Includes/lil_common_functions.hlsl#L328C48-L328C48
            //Originally under MIT License
            //Copyright (c) 2020-2023 lilxyzw
            float3 lilToneCorrection(float3 c, float4 hsvg)
            {
                // gamma
                c = pow(abs(c), hsvg.w);
                // rgb -> hsv
                float4 p = (c.b > c.g) ? float4(c.bg,-1.0,2.0/3.0) : float4(c.gb,0.0,-1.0/3.0);
                float4 q = (p.x > c.r) ? float4(p.xyw, c.r) : float4(c.r, p.yzx);
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                float3 hsv = float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
                // shift
                hsv = float3(hsv.x+hsvg.x,saturate(hsv.y*hsvg.y),saturate(hsv.z*hsvg.z));
                // hsv -> rgb
                return hsv.z - hsv.z * hsv.y + hsv.z * hsv.y * saturate(abs(frac(hsv.x + float3(1.0, 2.0/3.0, 1.0/3.0)) * 6.0 - 3.0) - 1.0);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 MainColor = tex2D(_MainTex ,i.uv);
                float MaskValue = lerp(1,tex2D(_Mask ,i.uv).r,_UseMask);

                MainColor.rgb = lerp(MainColor.rgb,lilToneCorrection(MainColor.rgb,_HSVG),MaskValue);
                return MainColor;
            }
            ENDHLSL
        }
    }
}
