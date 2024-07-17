            #include "UnityCG.cginc"
            #include "./Compute/TransMapperHelper.hlsl"

#define DEPTH_MIN Linear01Depth(0.1)
#define DEPTH_MAX Linear01Depth(0.9)

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

            v2f MappedScreenSpace(v2f o)
            {
                o.vertex.x *= 2;
                o.vertex.y *= 2;
                o.vertex.x -=1;
                o.vertex.y -=1;

                o.vertex.z = DEPTH_MIN;

#if UNITY_UV_STARTS_AT_TOP
                o.vertex.y = -1 * o.vertex.y;
                o.normal.y = -1 * o.normal.y;
#endif
                return o;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
                o.normal = v.normal;
                o.uv = v.uv;

#if NotGeometry
                return MappedScreenSpace(o);
#else
                return o;
#endif
            }


            v2f GetV2f()
            {
                v2f defVal;

                float val = 0;

                #if NotDepth
                defVal.uv = val.xx;
                #else
                defVal.uv = val.xxx;
                #endif

                defVal.normal = val.xxx;
                defVal.vertex = val.xxxx;

                return defVal;
            }

            v2f PaddingCal(v2f center , v2f input , float Padding)
            {
                float PixelStep = (1 / length(_ScreenParams.xy));
                float LerpPixelStep  = length(input.vertex - center.vertex) / PixelStep;
                #if NotDepth
                LerpPixelStep += 0.00000000000000000000000000001;
                #endif
                float PaddingValue =  (Padding / (LerpPixelStep)) * -1;
                v2f o;
                o.uv = lerp(input.uv ,center.uv ,PaddingValue);

#if HighQualityPadding
                o.vertex.xy = input.normal.xy * (PixelStep * Padding) + input.vertex.xy;
                o.vertex.w = input.vertex.w;
#else
                o.vertex = lerp(input.vertex ,center.vertex ,PaddingValue);
#endif

                o.vertex.z = DEPTH_MAX;
                o.normal = input.normal;
                return o;
            }
            void MappedScreenSpaceTriangel(inout v2f tri[3])
            {
                [unroll]
                for(int i = 0; 3 > i; i += 1)
                {
                    tri[i] = MappedScreenSpace(tri[i]);
                }

            }
            float2 ReCalUV(in v2f input[3], v2f Target)
            {
              float4 CrossT = CrossTriangle(input[0].vertex.xyz , input[1].vertex.xyz , input[2].vertex.xyz , Target.vertex.xyz);
              float2 recalUV = FromBarycentricCoordinateSystem(input[0].uv.xy , input[1].uv.xy , input[2].uv.xy , ToBarycentricCoordinateSystem(CrossT)).xy;
              recalUV = isnan(recalUV) ? Target.uv : recalUV;
              return recalUV;
            }
            void WriteStream(in v2f Origin[3],in v2f Padding[3],inout TriangleStream<v2f> stream)
            {
                stream.Append(Padding[0]);
                stream.Append(Origin[0]);
                stream.Append(Padding[1]);
                stream.RestartStrip();

                stream.Append(Origin[0]);
                stream.Append(Origin[1]);
                stream.Append(Padding[1]);
                stream.RestartStrip();

                stream.Append(Origin[1]);
                stream.Append(Padding[1]);
                stream.Append(Padding[2]);
                stream.RestartStrip();

                stream.Append(Origin[1]);
                stream.Append(Origin[2]);
                stream.Append(Padding[2]);
                stream.RestartStrip();

                stream.Append(Origin[2]);
                stream.Append(Padding[0]);
                stream.Append(Padding[2]);
                stream.RestartStrip();

                stream.Append(Origin[0]);
                stream.Append(Padding[0]);
                stream.Append(Origin[2]);
                stream.RestartStrip();
            }
            void tileNormalize(in v2f input[3],out v2f outPut[3])
            {
                float2 minPos = min(input[0].vertex.xy,min(input[1].vertex.xy,input[2].vertex.xy));
                float2 tile = floor(minPos);

                float2 normalizedMinPos = minPos - tile;


                [unroll]
                for(int i = 0; 3 > i; i += 1)
                {
                    outPut[i] = input[i];
                    outPut[i].vertex.xy = normalizedMinPos + (input[i].vertex.xy - minPos);
                }
            }
            void paddingCali(in v2f input[3],out v2f outPut[3])
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

                outPut[0] = g0;
                outPut[1] = g1;
                outPut[2] = g2;
            }

            [maxvertexcount(3)]
            void geom (triangle v2f input[3], inout TriangleStream<v2f> stream)
            {
                v2f Origin[3] = {input[0],input[1],input[2]};

#if !UnTileNormalize
                v2f tileNormalized[3] = {GetV2f(),GetV2f(),GetV2f()};
                tileNormalize(Origin , tileNormalized);
                Origin = tileNormalized;
#endif
                MappedScreenSpaceTriangel(Origin);

                stream.Append(Origin[0]);
                stream.Append(Origin[1]);
                stream.Append(Origin[2]);
                stream.RestartStrip();
            }

            [maxvertexcount(18)]
            void puddingGeom (triangle v2f input[3], inout TriangleStream<v2f> stream)
            {
                v2f Origin[3] = {input[0],input[1],input[2]};

#if !UnTileNormalize
                v2f tileNormalized[3] = {GetV2f(),GetV2f(),GetV2f()};
                tileNormalize(Origin , tileNormalized);
                Origin = tileNormalized;
#endif
                MappedScreenSpaceTriangel(Origin);

                v2f paddingV[3] = {GetV2f(),GetV2f(),GetV2f()};
                paddingCali(Origin , paddingV);

                WriteStream(Origin , paddingV , stream);
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
