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

        _BumpMap                    ("Normal Map", 2D) = "bump" {}
        _BumpScale                  ("Scale", Range(-10,10)) = 1

        //----------------------------------------------------------------------------------------------------------------------
        // NormalMap 2nd
        _UseBump2ndMap              ("sNormalMap2nd", Int) = 0
        _Bump2ndMap_UVMode          ("UV Mode|UV0|UV1|UV2|UV3", Int) = 0

        _Bump2ndMap                 ("Normal Map", 2D) = "bump" {}
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

        _MatCapBumpMap              ("Normal Map", 2D) = "bump" {}
        _MatCapBumpScale            ("Scale", Range(-10,10)) = 1



        //----------------------------------------------------------------------------------------------------------------------
        // MatCap 2nd
        _UseMatCap2nd               ("sMatCap2nd", Int) = 0

        _MatCap2ndBlendMask         ("Mask", 2D) = "white" {}
        _MatCap2ndBlend             ("Blend", Range(0, 1)) = 1


        _MatCap2ndCustomNormal      ("sMatCapCustomNormal", Int) = 0

        _MatCap2ndBumpMap           ("Normal Map", 2D) = "bump" {}
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

            #pragma multi_compile_fragment // TODO

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
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);

                // TODO

                return col;
            }
            ENDHLSL
        }
    }
}
