Shader "Custom/ImageBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize("Blur Size", Float) = 1.0
    }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalRenderPipeline"}
        //Tags {"RenderPipeline"="UniversalRenderPipeline" "RenderType"="Opaque" }
        LOD 300


        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
        float _BlurSize;    
        float4 _MainTex_TexelSize;
        CBUFFER_END
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        struct Attributes
        {
            float4 positionOS: POSITION;
            float2 texcoord: TEXCOORD0;
        };
        struct Varyings
        {
            float4 positionCS:SV_POSITION;
            float2 uv:TEXCOORD0;
        };
        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.texcoord;
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                float2 uv = input.uv;
                half4 col = float4(0,0,0,0);
                col += 0.060 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-1,-1)*_MainTex_TexelSize.xy*_BlurSize);
                col += 0.098 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0,-1)*_MainTex_TexelSize.xy*_BlurSize);
                col += 0.060 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(1,-1)*_MainTex_TexelSize.xy*_BlurSize);
                col += 0.098 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-1,0)*_MainTex_TexelSize.xy*_BlurSize);
                col += 0.162 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                col += 0.098 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(1,0)*_MainTex_TexelSize.xy*_BlurSize);
                col += 0.060 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(1,-1)*_MainTex_TexelSize.xy*_BlurSize);
                col += 0.022 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(1,0)*_MainTex_TexelSize.xy*_BlurSize);
                col += 0.060 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(1,1)*_MainTex_TexelSize.xy*_BlurSize);

                return col;
            }
            ENDHLSL
        }
    }
}
