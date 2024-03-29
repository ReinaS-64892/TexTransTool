            #include "UnityCG.cginc"
            #include "./Compute/TransMapperHelper.hlsl"

            struct appdata
            {
                #if NotDepth
                float2 uv : TEXCOORD0;
                #else
                float3 uv : TEXCOORD0;
                #endif
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                #if NotDepth
                float2 uv : TEXCOORD0;
                #else
                float3 uv : TEXCOORD0;
                #endif
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
            };
            sampler2D _MainTex;
            sampler2D _DepthTex;
            float _Padding;
            float _WarpRangeX;
            float _WarpRangeY;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
                o.vertex.x *= 2;
                o.vertex.y *= 2;
                o.vertex.x -=1;
                o.vertex.y -=1;
                o.vertex.z = 1;

                o.normal = v.normal;
#if UNITY_UV_STARTS_AT_TOP
                o.vertex.y = -1 * o.vertex.y;
                o.normal.y = -1 * v.normal.y;
#endif
                o.uv = v.uv;
                return o;
            }

            v2f PaddingCal(v2f center , v2f input,float Padding)
            {
                float PixelStep = (1 / length(_ScreenParams.xy));
                float LerpPixelStep  = length(input.vertex - center.vertex) / PixelStep;
                float PaddingValue =  (Padding / LerpPixelStep) * -1;
                v2f o;
                o.uv = lerp(input.uv ,center.uv ,PaddingValue);

#if HighQualityPadding
                o.vertex.xy = input.normal.xy * (PixelStep * Padding) + input.vertex.xy;
                o.vertex.w = input.vertex.w;
#else
                o.vertex = lerp(input.vertex ,center.vertex ,PaddingValue);
#endif

                o.vertex.z = 0;
                o.normal = input.normal;
                return o;
            }
            float2 ReCalUV(triangle v2f input[3], v2f Target)
            {
              float4 CrossT = CrossTriangle(input[0].vertex , input[1].vertex , input[2].vertex , Target.vertex);
              float2 recalUV = FromBarycentricCoordinateSystem(input[0].uv.xyy , input[1].uv.xyy , input[2].uv.xyy , ToBarycentricCoordinateSystem(CrossT)).xy;
              recalUV = isnan(recalUV) ? Target.uv : recalUV;
              return recalUV;
            }
            [maxvertexcount(18)]
            void geom (triangle v2f input[3], inout TriangleStream<v2f> stream)
            {
                v2f center;
                center.uv = lerp(lerp(input[0].uv ,input[1].uv ,0.5) ,input[2].uv ,0.5);
                center.vertex = lerp(lerp(input[0].vertex ,input[1].vertex ,0.5) ,input[2].vertex ,0.5);

                v2f g0 = PaddingCal(center ,input[0] ,_Padding);
                v2f g1 = PaddingCal(center ,input[1] ,_Padding);
                v2f g2 = PaddingCal(center ,input[2] ,_Padding);

#if HighQualityPadding
                g0.uv.xy = ReCalUV(input , g0);
                g1.uv.xy = ReCalUV(input , g1);
                g2.uv.xy = ReCalUV(input , g2);
#endif
#if !NotDepth
                g0.uv = input[0].uv;
                g1.uv = input[1].uv;
                g2.uv = input[2].uv;
#endif

                stream.Append(g0);
                stream.Append(input[0]);
                stream.Append(g1);
                stream.RestartStrip();

                stream.Append(input[0]);
                stream.Append(input[1]);
                stream.Append(g1);
                stream.RestartStrip();

                stream.Append(input[1]);
                stream.Append(g1);
                stream.Append(g2);
                stream.RestartStrip();

                stream.Append(input[1]);
                stream.Append(input[2]);
                stream.Append(g2);
                stream.RestartStrip();

                stream.Append(input[2]);
                stream.Append(g0);
                stream.Append(g2);
                stream.RestartStrip();

                stream.Append(input[0]);
                stream.Append(g0);
                stream.Append(input[2]);
                stream.RestartStrip();
            }


            float4 frag (v2f i) : SV_Target
            {

#if WarpRange
                float Rangeflag = 0;
                Rangeflag = max(Rangeflag, step(0.5f + _WarpRangeX ,abs(i.uv.x - 0.5))); // with
                Rangeflag = max(Rangeflag, step(0.5f + _WarpRangeY ,abs(i.uv.y - 0.5))); // haith

                Rangeflag  = 0.5 - Rangeflag;
                clip(Rangeflag);
#endif
#if DepthDecal
                float DepthValue = tex2Dlod(_DepthTex ,float4(i.uv.xy,0,0)).r;
                clip( ((1.0001 - i.uv.z) < DepthValue ) * -1 + 0.5);
#elif InvertDepth
                float DepthValue = tex2D(_DepthTex ,i.uv.xy).r;
                clip( ((1.0001 - i.uv.z) < DepthValue )  - 0.5);
#endif

                float4 DecalColor = tex2D(_MainTex ,i.uv.xy);
                return DecalColor;

            }
