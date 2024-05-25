Shader "Hidden/MergeColorAndOffset"
{
    Properties
    {
        _RTex ("TextureR", 2D) = "white" {}
        _GTex ("TextureG", 2D) = "white" {}
        _BTex ("TextureB", 2D) = "white" {}
        _ATex ("TextureA", 2D) = "white" {}
        _Offset ("Pivot and Size", Vector) = (0,0,0,0)
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
            #pragma shader_feature_local_fragment COLOR_SPACE_SRGB

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

            sampler2D _RTex;
            sampler2D _GTex;
            sampler2D _BTex;
            sampler2D _ATex;

            float4 _Offset;

            float4 GammaToLinear(float4 col)
            {
                return float4(GammaToLinearSpaceExact(col.r), GammaToLinearSpaceExact(col.g), GammaToLinearSpaceExact(col.b), (col.a));
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                float4 screenPos = ComputeScreenPos(o.vertex);

                o.vertex.x = lerp(_Offset.x , _Offset.x + _Offset.z , screenPos.x);
                o.vertex.y = lerp(_Offset.y , _Offset.y + _Offset.w , screenPos.y);

                o.vertex.x -= 0.5;
                o.vertex.y -= 0.5;

                o.vertex.x *= 2;
                o.vertex.y *= 2 * _ProjectionParams.x;

                o.uv = v.uv;
                o.uv.y = 1 - o.uv.y;

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = float4(tex2D(_RTex ,i.uv).r ,tex2D(_GTex ,i.uv).r ,tex2D(_BTex ,i.uv).r ,tex2D(_ATex ,i.uv).r );

                #if COLOR_SPACE_SRGB
                col = GammaToLinear(col);
                #endif

                return col;
            }
            ENDHLSL
        }
    }
}
