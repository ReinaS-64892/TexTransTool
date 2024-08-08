Shader "Hidden/lilToonAtlasBaker"
{
    Properties
    {
        //https://github.com/lilxyzw/lilToon/blob/d9e1e06d2bc25961f8849ba2dd926ffebcef6bf7/Assets/lilToon/CustomShaderResources/Properties/Default.lilblock

        //----------------------------------------------------------------------------------------------------------------------
        // Main
        _MainTex                    ("Texture", 2D) = "white" {}
        _Color                      ("sColor", Color) = (1,1,1,1)
        _MainTexHSVG                ("sHSVGs", Vector) = (0,1,1,1)

        _MainColorAdjustMask        ("Adjust Mask", 2D) = "white" {}

        //----------------------------------------------------------------------------------------------------------------------
        // Main2nd
        _UseMain2ndTex              ("sMainColor2nd", Int) = 0
        _Main2ndTex_UVMode          ("UV Mode|UV0|UV1|UV2|UV3|MatCap", Int) = 0

        _Main2ndTex                 ("Texture", 2D) = "white" {}
        _Color2nd                   ("sColor", Color) = (1,1,1,1)

        _Main2ndBlendMask           ("Mask", 2D) = "white" {}

        _Main2ndDissolveMask        ("Dissolve Mask", 2D) = "white" {}

        //----------------------------------------------------------------------------------------------------------------------
        // Main3rd
        _UseMain3rdTex              ("sMainColor3rd", Int) = 0
        _Main3rdTex_UVMode          ("UV Mode|UV0|UV1|UV2|UV3|MatCap", Int) = 0

        _Main3rdTex                 ("Texture", 2D) = "white" {}
        _Color3rd                   ("sColor", Color) = (1,1,1,1)

        _Main3rdBlendMask           ("Mask", 2D) = "white" {}

        _Main3rdDissolveNoiseMask   ("Dissolve Noise Mask", 2D) = "gray" {}


        //----------------------------------------------------------------------------------------------------------------------
        // Alpha Mask
        _AlphaMaskMode              ("sAlphaMaskModes", Int) = 0

        _AlphaMask                  ("AlphaMask", 2D) = "white" {}
        _AlphaMaskScale             ("Scale", Float) = 1
        _AlphaMaskValue             ("Offset", Float) = 0

        //----------------------------------------------------------------------------------------------------------------------
        // NormalMap
        _UseBumpMap                 ("sNormalMap", Int) = 0

       [Normal] _BumpMap                    ("Normal Map", 2D) = "bump" {}
        _BumpScale                  ("Scale", Range(-10,10)) = 1

        //----------------------------------------------------------------------------------------------------------------------
        // NormalMap 2nd
        _UseBump2ndMap              ("sNormalMap2nd", Int) = 0
        _Bump2ndMap_UVMode          ("UV Mode|UV0|UV1|UV2|UV3", Int) = 0

        [Normal] _Bump2ndMap                 ("Normal Map", 2D) = "bump" {}
        _Bump2ndScale               ("Scale", Range(-10,10)) = 1
        _Bump2ndScaleMask           ("Mask", 2D) = "white" {}


        //----------------------------------------------------------------------------------------------------------------------
        // Anisotropy
        _UseAnisotropy              ("sAnisotropy", Int) = 0

        _AnisotropyTangentMap       ("Tangent Map", 2D) = "bump" {}

        _AnisotropyScaleMask        ("Scale Mask", 2D) = "white" {}
        _AnisotropyScale            ("Scale", Range(-1,1)) = 1


        //----------------------------------------------------------------------------------------------------------------------
        // Backlight
        _UseBacklight               ("sBacklight", Int) = 0

        _BacklightColorTex          ("Texture", 2D) = "white" {}
        _BacklightColor             ("sColor", Color) = (0.85,0.8,0.7,1.0)


        //----------------------------------------------------------------------------------------------------------------------
        // Shadow
        _UseShadow                  ("sShadow", Int) = 0

        _ShadowStrengthMask         ("sStrength", 2D) = "white" {}
        _ShadowStrength             ("sStrength", Range(0, 1)) = 1

        _ShadowBorderMask           ("sBorder", 2D) = "white" {}

        _ShadowBlurMask             ("sBlur", 2D) = "white" {}

        _ShadowColorTex             ("Shadow Color", 2D) = "black" {}
        _ShadowColor                ("Shadow Color", Color) = (0.82,0.76,0.85,1.0)

        _Shadow2ndColorTex          ("2nd Color", 2D) = "black" {}
        _Shadow2ndColor             ("2nd Color", Color) = (0.68,0.66,0.79,1)

        _Shadow3rdColorTex          ("3rd Color", 2D) = "black" {}
        _Shadow3rdColor             ("3rd Color", Color) = (0,0,0,0)

        //----------------------------------------------------------------------------------------------------------------------
        // Rim Shade
        _UseRimShade                ("RimShade", Int) = 0

        _RimShadeMask               ("Mask", 2D) = "white" {}
        _RimShadeColor              ("sColor", Color) = (0.5,0.5,0.5,1.0)

        //----------------------------------------------------------------------------------------------------------------------
        // Reflection
        _UseReflection              ("sReflection", Int) = 0

        _SmoothnessTex              ("Smoothness", 2D) = "white" {}
        _Smoothness                 ("Smoothness", Range(0, 1)) = 1

        _MetallicGlossMap           ("Metallic", 2D) = "white" {}
        _Metallic                   ("Metallic", Range(0, 1)) = 0

        _ReflectionColorTex         ("sColor", 2D) = "white" {}
        _ReflectionColor            ("sColor", Color) = (1,1,1,1)

        //----------------------------------------------------------------------------------------------------------------------
        // MatCap
        _UseMatCap                  ("sMatCap", Int) = 0

        _MatCapBlendMask            ("Mask", 2D) = "white" {}
        _MatCapBlend                ("Blend", Range(0, 1)) = 1


        _MatCapCustomNormal         ("sMatCapCustomNormal", Int) = 0

        [Normal] _MatCapBumpMap              ("Normal Map", 2D) = "bump" {}
        _MatCapBumpScale            ("Scale", Range(-10,10)) = 1



        //----------------------------------------------------------------------------------------------------------------------
        // MatCap 2nd
        _UseMatCap2nd               ("sMatCap2nd", Int) = 0

        _MatCap2ndBlendMask         ("Mask", 2D) = "white" {}
        _MatCap2ndBlend             ("Blend", Range(0, 1)) = 1


        _MatCap2ndCustomNormal      ("sMatCapCustomNormal", Int) = 0

        [Normal] _MatCap2ndBumpMap           ("Normal Map", 2D) = "bump" {}
        _MatCap2ndBumpScale         ("Scale", Range(-10,10)) = 1

        //----------------------------------------------------------------------------------------------------------------------
        // Rim
        _UseRim                     ("sRimLight", Int) = 0

        _RimColorTex                ("Texture", 2D) = "white" {}
        _RimColor                   ("sColor", Color) = (0.66,0.5,0.48,1)

        //----------------------------------------------------------------------------------------------------------------------
        // Glitter
        _UseGlitter                 ("sGlitter", Int) = 0
        _GlitterColorTex_UVMode     ("UV Mode|UV0|UV1|UV2|UV3", Int) = 0

        _GlitterColorTex            ("Texture", 2D) = "white" {}
        _GlitterColor               ("sColor", Color) = (1,1,1,1)

        //----------------------------------------------------------------------------------------------------------------------
        // Emmision
        _UseEmission                ("sEmission", Int) = 0
        _EmissionMap_UVMode         ("UV Mode|UV0|UV1|UV2|UV3|Rim", Int) = 0

        _EmissionMap                ("Texture", 2D) = "white" {}
        _EmissionColor              ("sColor", Color) = (1,1,1,1)

        _EmissionBlendMask          ("Mask", 2D) = "white" {}
        _EmissionBlend              ("Blend", Range(0,1)) = 1


        //----------------------------------------------------------------------------------------------------------------------
        // Emmision2nd
        _UseEmission2nd             ("sEmission2nd", Int) = 0
        _Emission2ndMap_UVMode      ("UV Mode|UV0|UV1|UV2|UV3|Rim", Int) = 0

        _Emission2ndMap             ("Texture", 2D) = "white" {}
        _Emission2ndColor           ("sColor", Color) = (1,1,1,1)

        _Emission2ndBlendMask       ("Mask", 2D) = "white" {}
        _Emission2ndBlend           ("Blend", Range(0,1)) = 1

        //----------------------------------------------------------------------------------------------------------------------
        // Parallax
        _UseParallax                ("sParallax", Int) = 0

        _ParallaxMap                ("Parallax Map", 2D) = "gray" {}
        _Parallax                   ("Parallax Scale", float) = 0.02

//AudioLink と Dissolve はよくわかんないのでパス

        //----------------------------------------------------------------------------------------------------------------------
        // Outline

        _OutlineTex                 ("Texture", 2D) = "white" {}
        _OutlineColor               ("sColor", Color) = (0.6,0.56,0.73,1)
        _OutlineTexHSVG             ("sHSVGs", Vector) = (0,1,1,1)

        _OutlineWidthMask           ("Width", 2D) = "white" {}
        _OutlineWidth               ("Width", Range(0,1)) = 0.08
        _OutlineWidth_MaxValue      ("Width", float) = 1


        _OutlineVectorUVMode        ("UV Mode|UV0|UV1|UV2|UV3", Int) = 0

        _OutlineVectorTex   ("Vector", 2D) = "bump" {}
        _OutlineVectorScale         ("Vector scale", Range(-10,10)) = 1


    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_local_fragment Bake_MainTex Bake_Main2ndTex Bake_Main3rdTex Bake_AlphaMask Bake_BumpMap Bake_Bump2ndMap Bake_AnisotropyScaleMask Bake_BacklightColorTex Bake_ShadowStrengthMask Bake_ShadowColorTex Bake_Shadow2ndColorTex Bake_Shadow3rdColorTex Bake_RimShadeMask Bake_SmoothnessTex Bake_MetallicGlossMap Bake_ReflectionColorTex Bake_MatCapBlendMask Bake_MatCapBumpMap Bake_MatCap2ndBlendMask Bake_MatCap2ndBumpMap Bake_RimColorTex Bake_GlitterColorTex Bake_EmissionMap Bake_EmissionBlendMask Bake_Emission2ndMap Bake_Emission2ndBlendMask Bake_ParallaxMap Bake_OutlineTex Bake_OutlineWidthMask Bake_OutlineVectorTex
            #pragma shader_feature_local_fragment Constraint_Invalid

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Color;
            float4 _MainTexHSVG;

            sampler2D _MainColorAdjustMask;

            sampler2D _Main2ndTex;
            float4 _Color2nd;

            sampler2D _Main2ndBlendMask;

            sampler2D _Main2ndDissolveMask;

            sampler2D _Main3rdTex;
            float4 _Color3rd;

            sampler2D _Main3rdBlendMask;

            sampler2D _Main3rdDissolveNoiseMask;

            sampler2D _AlphaMask;
            float _AlphaMaskScale;
            float _AlphaMaskValue;

            sampler2D _BumpMap;
            float _BumpScale;

            sampler2D _Bump2ndMap;
            float _Bump2ndScale;
            sampler2D _Bump2ndScaleMask;

            sampler2D _AnisotropyTangentMap;

            sampler2D _AnisotropyScaleMask;
            float _AnisotropyScale;

            sampler2D _BacklightColorTex;
            float4 _BacklightColor;

            sampler2D _ShadowStrengthMask;
            float _ShadowStrength;

            sampler2D _ShadowBorderMask;

            sampler2D _ShadowBlurMask;

            sampler2D _ShadowColorTex;
            float4 _ShadowColor;

            sampler2D _Shadow2ndColorTex;
            float4 _Shadow2ndColor;

            sampler2D _Shadow3rdColorTex;
            float4 _Shadow3rdColor;

            sampler2D _RimShadeMask;
            float4 _RimShadeColor;

            sampler2D _SmoothnessTex;
            float _Smoothness;

            sampler2D _MetallicGlossMap;
            float _Metallic;

            sampler2D _ReflectionColorTex;
            float4 _ReflectionColor;

            sampler2D _MatCapBlendMask;
            float _MatCapBlend;

            sampler2D _MatCapBumpMap;
            float _MatCapBumpScale;

            sampler2D _MatCap2ndBlendMask;
            float _MatCap2ndBlend;

            sampler2D _MatCap2ndBumpMap;
            float _MatCap2ndBumpScale;

            sampler2D _RimColorTex;
            float4 _RimColor;

            sampler2D _GlitterColorTex;
            float4 _GlitterColor;

            sampler2D _EmissionMap;
            float4 _EmissionColor;

            sampler2D _EmissionBlendMask;
            float _EmissionBlend;

            sampler2D _Emission2ndMap;
            float4 _Emission2ndColor;

            sampler2D _Emission2ndBlendMask;
            float _Emission2ndBlend;

            sampler2D _ParallaxMap;
            float _Parallax;

            sampler2D _OutlineTex;
            float4 _OutlineColor;
            float4 _OutlineTexHSVG;

            sampler2D _OutlineWidthMask;
            float _OutlineWidth;
            float _OutlineWidth_MaxValue;

            sampler2D _OutlineVectorTex;
            float _OutlineVectorScale;

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











            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
#if Bake_MainTex
                float4 col = tex2D(_MainTex ,i.uv);
                float3 tcCol = lilToneCorrection(col.rgb,_MainTexHSVG);
                col.rgb = lerp(col.rgb, tcCol, tex2D(_MainColorAdjustMask,i.uv));
                col *= _Color;
                return col;
#elif Bake_Main2ndTex
                return tex2D(_Main2ndTex ,i.uv) * _Color2nd;
#elif Bake_Main3rdTex
                return tex2D(_Main3rdTex ,i.uv) * _Color3rd;
#elif Bake_AlphaMask
#if Constraint_Invalid
                return float4(1,1,1,1);
#endif
                return saturate(tex2D(_AlphaMask ,i.uv) * _AlphaMaskScale + _AlphaMaskValue);
#elif Bake_BumpMap
#if Constraint_Invalid
                return float4(1,0.5,0.5,0.5);
#endif
                return tex2D(_BumpMap,i.uv);//いつかやるかもしれないかも
#elif Bake_Bump2ndMap
#if Constraint_Invalid
                return float4(1,0.5,0.5,0.5);
#endif
                return tex2D(_Bump2ndMap,i.uv);
#elif Bake_AnisotropyScaleMask
                return tex2D(_AnisotropyTangentMap,i.uv);
#elif Bake_BacklightColorTex
                return tex2D(_BacklightColorTex,i.uv) * _BacklightColor;
#elif Bake_ShadowStrengthMask
                return tex2D(_ShadowStrengthMask,i.uv) * _ShadowStrength;
#elif Bake_ShadowColorTex
                return tex2D(_ShadowColorTex,i.uv) * _ShadowColor;
#elif Bake_Shadow2ndColorTex
                return tex2D(_Shadow2ndColorTex,i.uv) * _Shadow2ndColor;
#elif Bake_Shadow3rdColorTex
                return tex2D(_Shadow3rdColorTex,i.uv) * _Shadow3rdColor;
#elif Bake_RimShadeMask
#if Constraint_Invalid
                return float4(0,0,0,0);
#endif
                return tex2D(_RimShadeMask,i.uv) * _RimShadeColor;
#elif Bake_SmoothnessTex
                return tex2D(_SmoothnessTex,i.uv) * _Smoothness;
#elif Bake_MetallicGlossMap
                return tex2D(_MetallicGlossMap,i.uv) * _Metallic;
#elif Bake_ReflectionColorTex
                return tex2D(_ReflectionColorTex,i.uv) * _ReflectionColor;
#elif Bake_MatCapBlendMask
                return tex2D(_MatCapBlendMask,i.uv) * _MatCapBlend;
#elif Bake_MatCapBumpMap
#if Constraint_Invalid
                return float4(1,0.5,0.5,0.5);
#endif
                return tex2D(_MatCapBumpMap,i.uv);
#elif Bake_MatCap2ndBlendMask
                return tex2D(_MatCap2ndBlendMask,i.uv) * _MatCap2ndBlend;
#elif Bake_MatCap2ndBumpMap
#if Constraint_Invalid
                return float4(1,0.5,0.5,0.5);
#endif
                return tex2D(_MatCap2ndBumpMap,i.uv);
#elif Bake_RimColorTex
                return tex2D(_RimColorTex,i.uv) * _RimColor;
#elif Bake_GlitterColorTex
                return tex2D(_GlitterColorTex,i.uv) * _GlitterColor;
#elif Bake_EmissionMap
#if Constraint_Invalid
                return float4(0,0,0,0);
#endif
                return tex2D(_EmissionMap,i.uv) * _EmissionColor;
#elif Bake_EmissionBlendMask
#if Constraint_Invalid
                return float4(0,0,0,0);
#endif
                return tex2D(_EmissionBlendMask,i.uv) * _EmissionBlend;
#elif Bake_Emission2ndMap
#if Constraint_Invalid
                return float4(0,0,0,0);
#endif
                return tex2D(_Emission2ndMap,i.uv) * _Emission2ndColor;
#elif Bake_Emission2ndBlendMask
#if Constraint_Invalid
                return float4(0,0,0,0);
#endif
                return tex2D(_Emission2ndBlendMask,i.uv) * _Emission2ndBlend;
#elif Bake_ParallaxMap
                return tex2D(_ParallaxMap,i.uv);
#elif Bake_OutlineTex
#if Constraint_Invalid
                return float4(0,0,0,0);
#endif
                float4 col = tex2D(_OutlineTex ,i.uv);
                float3 tcCol = lilToneCorrection(col.rgb,_OutlineTexHSVG);
                col.rgb = tcCol;
                col *= _OutlineColor;
                return col;
#elif Bake_OutlineWidthMask
#if Constraint_Invalid
                return float4(0,0,0,0);
#endif
                return tex2D(_OutlineWidthMask ,i.uv) * (_OutlineWidth / _OutlineWidth_MaxValue);
#elif Bake_OutlineVectorTex
                return tex2D(_OutlineVectorTex ,i.uv);
#endif
            }
            ENDHLSL
        }
    }
}
