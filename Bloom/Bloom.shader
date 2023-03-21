Shader "CDC/Bloom"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
        LOD 100

        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_ST;
        uniform half4 _MainTex_TexelSize;
        float _luminanceThreshole;
        float _bloomIntensity;

        int _downSampleBlurSize;
        float _downSampleBlurSigma;

        int _upSampleBlurSize;
        float _upSampleBlurSigma;

        Texture2D _PrevMip; 
        SAMPLER(sampler_PrevMip);

        Texture2D _BloomTex;
        SAMPLER(sampler_BloomTex);


        CBUFFER_END

            struct appdata
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            TEXTURE2D(_MainTex);                          SAMPLER(sampler_MainTex);

            float GaussWeight2D(float x, float y, float sigma)
            {
                float pi = 3.14159265358;
                float e  = 2.71828182846;
                float sigma_2 = pow(sigma, 2);

                float a = -(x*x + y*y) / (2.0 * sigma_2);
                return pow(e, a) / (2.0 * pi * sigma_2);
            }

            float3 GaussNxN(Texture2D tex, float2 uv, int n, float2 stride, float sigma)
            {
                float3 color = float3(0, 0, 0);
                int r = n / 2;
                float weight = 0.0;

                for(int i=-r; i<=r; i++)
                {
                    for(int j=-r; j<=r; j++)
                    {
                        float w = GaussWeight2D(i, j, sigma);
                        float2 coord = uv + float2(i, j) * stride;
                        color += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,coord).rgb * w;
                        
                        weight += w;
                    }
                }

                color /= weight;
                return color;
            }

            float3 ACESToneMapping(float3 color, float adapted_lum)
            {
                const float A = 2.51f;
                const float B = 0.03f;
                const float C = 2.43f;
                const float D = 0.59f;
                const float E = 0.14f;

                color *= adapted_lum;
                return (color * (A * color + B)) / (color * (C * color + D) + E);
            }
        ENDHLSL

        //Get light
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert (appdata v)
            {
                v2f o;
                VertexPositionInputs  PositionInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = PositionInputs.positionCS;   
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {

                half4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);

                float lum = dot(float3(0.2126, 0.7152, 0.0722), col.rgb);

                if(lum > _luminanceThreshole) 
                {
                    return col;
                }
                return float4(0,0,0,1);
            }
            ENDHLSL
        }

        //Down Sample
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert (appdata v)
            {
                v2f o;
                VertexPositionInputs  PositionInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = PositionInputs.positionCS;   
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {

                half4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);

                float2 stride = _MainTex_TexelSize.xy;

                col.rgb = GaussNxN(_MainTex, i.uv, _downSampleBlurSize, stride, _downSampleBlurSigma);

                return col; 
            }
            ENDHLSL
        }

        //Up Sample
        pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert (appdata v)
            {
                v2f o;
                VertexPositionInputs  PositionInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = PositionInputs.positionCS;   
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {

                half4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);

                half4 test = SAMPLE_TEXTURE2D(_PrevMip,sampler_PrevMip,i.uv);

                float2 stride = _MainTex_TexelSize.xy;

                float2 prev_stride = 0.5 * _MainTex_TexelSize.xy;   
                float2 curr_stride = 1.0 * _MainTex_TexelSize.xy;  

                float3 prev_mip = GaussNxN(_PrevMip, i.uv, _upSampleBlurSize, prev_stride, _upSampleBlurSigma);
                float3 curr_mip = GaussNxN(_MainTex, i.uv, _upSampleBlurSize, curr_stride, _upSampleBlurSigma);
                col.rgb = prev_mip + curr_mip;

                return col;
            }
            ENDHLSL
        }

        //Add bloom
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert (appdata v)
            {
                v2f o;
                VertexPositionInputs  PositionInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = PositionInputs.positionCS;   
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                
                float3 bloom = SAMPLE_TEXTURE2D(_BloomTex,sampler_BloomTex,i.uv) * _bloomIntensity;
                bloom = ACESToneMapping(bloom, 1.0);

                float g = 1.0 / 2.2;
                bloom = saturate(pow(bloom, float3(g, g, g)));

                col.rgb += bloom;

                return float4(bloom, 1.0);

                return col;
            }
            ENDHLSL
        }
    }
}