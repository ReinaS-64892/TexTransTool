Shader "Hidden/TransTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Padding ("Padding", Float) = 0
        _WarpRangeX("WarpRangeX" ,Float) = 0
        _WarpRangeY("WarpRangeY" ,Float) = 0

    }
    SubShader
    {
        Tags { "RenderType"="Geometry" }
        LOD 100
        Pass
        {
            Cull Off
            Stencil
            {
                ref 2
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma exclude_renderers metal
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local_fragment WarpRange

            #include "./TransTextureHelper.hlsl"

            ENDHLSL
        }
        Pass
        {
            Cull Off
            Stencil
            {
                ref 2
                Comp NotEqual
            }
            HLSLPROGRAM
            #pragma exclude_renderers metal
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma shader_feature_local_fragment WarpRange

            #include "./TransTextureHelper.hlsl"

            ENDHLSL
        }
    }
    SubShader
    {
        Tags { "RenderType"="Geometry" }
        LOD 100
        Pass
        {
            Cull Off
            Stencil
            {
                ref 2
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local_fragment WarpRange

            #include "./TransTextureHelper.hlsl"

            ENDHLSL
        }
    }
}
