Shader "Hidden/LevelAdjustment"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _InputFloor("input floor", float) = 0
        _InputCeiling("input celing", float) = 1
        _Gamma("gamm", float) = 1
        _OutputFloor("output floor", float) = 0
        _OutputCeiling("output celing", float) = 1

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

            float _InputFloor;
            float _InputCeiling;
            float _Gamma;
            float _OutputFloor;
            float _OutputCeiling;

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

            float Gamma(float input, float y)
            {
                return pow(input,1 / y);
            }

            float Level(float input)
            {
                float normalized = (clamp(input,_InputFloor,_InputCeiling) - _InputFloor) / (_InputCeiling - _InputFloor);
                return lerp(_OutputFloor,_OutputCeiling,Gamma(normalized,_Gamma));
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = LiniearToGamma(tex2Dlod(_MainTex,float4( i.uv,0,0)));

                #if RGB
                col.r = Level(col.r);
                col.g = Level(col.g);
                col.b = Level(col.b);
                #elif Red
                col.r = Level(col.r);
                #elif Green
                col.g = Level(col.g);
                #elif Brue
                col.b = Level(col.b);
                #endif

                return GammaToLinear(col);
            }
            ENDHLSL
        }
    }
}
