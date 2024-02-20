Shader "Hidden/SelectiveColoringAdjustment"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _RedsCMYK("Reds CMYK", Vector) = (0,0,0,0)
        _YellowsCMYK("Yellows CMYK", Vector) = (0,0,0,0)
        _GreensCMYK("Greens CMYK", Vector) = (0,0,0,0)
        _CyansCMYK("Cyans CMYK", Vector) = (0,0,0,0)
        _BluesCMYK("Blues CMYK", Vector) = (0,0,0,0)
        _MagentasCMYK("Magentas CMYK", Vector) = (0,0,0,0)

        _WhitesCMYK("Whites CMYK", Vector) = (0,0,0,0)
        _NeutralsCMYK("Neutrals CMYK", Vector) = (0,0,0,0)
        _BlacksCMYK("Blacks CMYK", Vector) = (0,0,0,0)

        _IsAbsolute("Blacks CMYK", float) = 0

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

            float4 _RedsCMYK;
            float4 _YellowsCMYK;
            float4 _GreensCMYK;
            float4 _CyansCMYK;
            float4 _BluesCMYK;
            float4 _MagentasCMYK;
            float4 _WhitesCMYK;
            float4 _NeutralsCMYK;
            float4 _BlacksCMYK;

            float _IsAbsolute;

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
            float Max3(float3 src){return max(max(src.r,src.g),src.b);}
            float Min3(float3 src){return min(min(src.r,src.g),src.b);}
            float Med3(float3 src){return min(max(src.r,src.g),src.b);}//中央の値を選ぶ

            //Thanks for @bug@fosstodon.org !!!
            //https://blog.pkh.me/p/22-understanding-selective-coloring-in-adobe-photoshop.html

            bool IsSelectColorClassification(float3 src,int selectColor)//T
            {
                switch (selectColor)
                {
                    case 0:{return src.r == Max3(src);}//Reds
                    case 1:{return src.b == Min3(src);}//Yellows
                    case 2:{return src.g == Max3(src);}//Greens
                    case 3:{return src.r == Min3(src);}//Cyans
                    case 4:{return src.b == Max3(src);}//Blues
                    case 5:{return src.g == Min3(src);}//Magentas

                    case 6:{return src.r > 0.5 && src.g > 0.5 && src.b > 0.5;}//Whites
                    case 7:{return !(src.r <= 0.0 && src.g <= 0.0 && src.b <= 0.0) && !(src.r >= 1.0 && src.g >= 1.0 && src.b >= 1.0);}//Neutrals
                    case 8:{return src.r < 0.5 && src.g < 0.5 && src.b < 0.5;}//Blacks
                }
            }

            float ColorScale(float3 src,int selectColor)//Ω
            {
                switch (selectColor)
                {
                    case 0://Reds
                    case 2://Greens
                    case 4://Blues
                    {return Max3(src) - Med3(src);}

                    case 1://Yellows
                    case 3://Cyans
                    case 5://Magentas
                    {return Med3(src) - Min3(src);}


                    case 6:{return ( Min3(src) - 0.5 ) * 2;}//Whites

                    case 7:{return 1 - ( abs( Max3(src) - 0.5 ) + abs( Min3(src) - 0.5) );}//Neutrals

                    case 8:{return ( 0.5 - Max3(src) ) * 2 ;}//Blacks
                }
            }

            float ModeFactor(float v,bool isAbsolute)//m
            {
                if(isAbsolute){ return 1; }//Absolute
                else{ return 1 - v; }//Relative
            }

            float AdjustmentFunction(float v,float a,float aK,float m,float omega)//φ
            {
                return clamp( ( (-1 - a) * aK - a ) * m ,  -v  , 1 - v ) * omega ;
            }

            float3 SelectiveColor(float3 src,int selectColor ,float4 cmyk,bool isAbsolute)// selectColor == C
            {
                bool gamma = IsSelectColorClassification( src , selectColor );
                float omega = ColorScale( src , selectColor );

                float Red =    lerp( 0 , AdjustmentFunction( src.r , cmyk.r , cmyk.a , ModeFactor( src.r , isAbsolute) , omega ) , gamma );
                float Green =  lerp( 0 , AdjustmentFunction( src.g , cmyk.g , cmyk.a , ModeFactor( src.g , isAbsolute) , omega ) , gamma );
                float Blue =   lerp( 0 , AdjustmentFunction( src.b , cmyk.b , cmyk.a , ModeFactor( src.b , isAbsolute) , omega ) , gamma );

                return float3(Red , Green , Blue);
            }


            float4 frag (v2f i) : SV_Target
            {
                float4 col = LiniearToGamma(tex2Dlod(_MainTex,float4( i.uv,0,0)));
                float3 result = float3(0,0,0);

                bool isAbsolute = _IsAbsolute > 0.5;

                result += SelectiveColor(col.rgb , 0 , _RedsCMYK , isAbsolute);
                result += SelectiveColor(col.rgb , 1 , _YellowsCMYK , isAbsolute);
                result += SelectiveColor(col.rgb , 2 , _GreensCMYK , isAbsolute);
                result += SelectiveColor(col.rgb , 3 , _CyansCMYK , isAbsolute);
                result += SelectiveColor(col.rgb , 4 , _BluesCMYK , isAbsolute);
                result += SelectiveColor(col.rgb , 5 , _MagentasCMYK , isAbsolute);
                result += SelectiveColor(col.rgb , 6 , _WhitesCMYK , isAbsolute);
                result += SelectiveColor(col.rgb , 7 , _NeutralsCMYK , isAbsolute);
                result += SelectiveColor(col.rgb , 8 , _BlacksCMYK , isAbsolute);

                col.rgb += result;

                return GammaToLinear(col);
            }
            ENDHLSL
        }
    }
}
