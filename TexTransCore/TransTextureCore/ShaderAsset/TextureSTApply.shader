Shader "Hidden/TextureSTApply"
{
    Properties
    {
        _OffSetTex ("Texture", 2D) = "white" {}
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

            sampler2D _OffSetTex;
            float4 _OffSetTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv , _OffSetTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return tex2D(_OffSetTex ,i.uv);
            }
            ENDHLSL
        }
    }
}
