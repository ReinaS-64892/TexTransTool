Shader "Hidden/LuminanceMappingShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MapTex ("Texture", 2D) = "white" {}
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
            #pragma multi_compile_local_fragment RGB Red Green Brue

            #include "UnityCG.cginc"
            #include "../../../../TexTransCore/BlendTexture/ShaderAsset/SetSL.hlsl"

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
            sampler2D _MapTex;

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

                return tex2Dlod(_MapTex,float4(GetLum(col),0,0,0));
            }
            ENDHLSL
        }
    }
}
