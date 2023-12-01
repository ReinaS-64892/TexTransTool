Shader "Hidden/DepthWriter"
{
    SubShader
    {
        Tags { "RenderType"="Geometry" }
        LOD 100
        Pass
        {
            Cull Off


            HLSLPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag


            struct appdata
            {
                float3 uv : TEXCOORD0;
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex.xyz = v.uv;
                o.vertex.w = v.vertex.w;
                o.vertex.x *= 2;
                o.vertex.y *= 2;
                o.vertex.x -=1;
                o.vertex.y -=1;
                o.vertex.z = 1 - o.vertex.z ;

#if UNITY_UV_STARTS_AT_TOP
                o.vertex.y = -1 * o.vertex.y;
#endif
                return o;
            }


            float4 frag (v2f i) : SV_Target
            {
                return i.vertex.zzzz;
            }
            ENDHLSL
        }
    }
}
