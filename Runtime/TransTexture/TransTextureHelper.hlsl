            #include "UnityCG.cginc"

            struct appdata
            {
                float2 uv : TEXCOORD0;
                float4 vertex : POSITION;
            };

            struct UVandVart
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            sampler2D _MainTex;
            float _Padding;
            float _WarpRangeX;
            float _WarpRangeY;


            UVandVart vert (appdata v)
            {
                UVandVart o;
                o.vertex = v.vertex;
                o.vertex.x *= 2;
                o.vertex.y *= 2;
                o.vertex.x -=1;
                o.vertex.y -=1;
#if UNITY_UV_STARTS_AT_TOP
                o.vertex.y = -1 * o.vertex.y;
#endif
                o.uv = v.uv;
                return o;
            }

            UVandVart PaddingCal(UVandVart center , UVandVart input,float Padding)
            {
                float lerpValue = -1 * Padding;
                UVandVart o;
                o.uv = lerp(input.uv ,center.uv ,lerpValue);
                o.vertex = lerp(input.vertex ,center.vertex ,lerpValue);
                return o;
            }

            [maxvertexcount(3)]
            void geom (triangle UVandVart input[3], inout TriangleStream<UVandVart> stream)
            {
                UVandVart center;
                center.uv = lerp(lerp(input[0].uv ,input[1].uv ,0.5) ,input[2].uv ,0.5);
                center.vertex = lerp(lerp(input[0].vertex ,input[1].vertex ,0.5) ,input[2].vertex ,0.5);

                UVandVart g0 = PaddingCal(center ,input[0] ,_Padding);
                UVandVart g1 = PaddingCal(center ,input[1] ,_Padding);
                UVandVart g2 = PaddingCal(center ,input[2] ,_Padding);

                stream.Append(g0);
                stream.Append(g1);
                stream.Append(g2);
                stream.RestartStrip();
            }


            fixed4 frag (UVandVart i) : SV_Target
            {

#if WarpRange
                float Rangeflag = 0;
                Rangeflag = max(Rangeflag, step(0.5f + _WarpRangeX ,abs(i.uv.x - 0.5))); // with
                Rangeflag = max(Rangeflag, step(0.5f + _WarpRangeY ,abs(i.uv.y - 0.5))); // haith

                Rangeflag  = 0.5 - Rangeflag;
                clip(Rangeflag);
#endif

                float4 AddColor = tex2D(_MainTex ,i.uv);
                return AddColor;


            }
