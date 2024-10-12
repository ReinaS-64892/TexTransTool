using UnityEngine;
using System;
using System.Collections.Generic;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransUnityCore
{
    public class TTBlendUnityObject : ScriptableObject, ITTBlendKey
    {
        public string BlendTypeKey;

        public bool IsLinerRequired;

        public ComputeShader Compute;
        public Shader Shader;

        public List<Locale> Locales;
        [Serializable]
        public class Locale
        {
            public string LangCode;
            public string DisplayName;
        }
        public const string KernelDefine = "#pragma kernel CSMain\n";
        public const string ComputeShaderTemplate =

@"
RWTexture2D<float4> AddTex;
RWTexture2D<float4> DistTex;

[numthreads(32, 32, 1)] void CSMain(uint3 id : SV_DispatchThreadID)
{
    DistTex[id.xy] = ColorBlend( DistTex[id.xy] , AddTex[id.xy] );
}
";

        public const string ShaderDefine =
@"
Shader ""Hidden/BlendTexture""
{
    Properties
    {
        _DistTex (""DistTexture"", 2D) = ""white"" {}
        _MainTex (""Texture"", 2D) = ""white"" {}
    }
    SubShader
    {
        Tags { ""Queue"" = ""Transparent"" }
        LOD 100
        Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include ""UnityCG.cginc""

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
";

        public const string ShaderTemplate =
@"
            float4 frag (v2f i) : SV_Target
            {
                float4 BaseColor = LiniearToGamma(tex2Dlod(_DistTex,float4( i.uv,0,0)));
                float4 AddColor = LiniearToGamma(tex2Dlod(_MainTex ,float4(i.uv,0,0)));

                return GammaToLinear(ColorBlend(BaseColor,AddColor));
            }
            ENDHLSL
        }
    }
}
";
        public const string ShaderTemplateWithLinear =
@"
            float4 frag (v2f i) : SV_Target
            {
                float4 BaseColor = tex2Dlod(_DistTex,float4( i.uv,0,0));
                float4 AddColor = tex2Dlod(_MainTex ,float4(i.uv,0,0));

                return ColorBlend(BaseColor,AddColor);
            }
            ENDHLSL
        }
    }
}
";


    }
}
