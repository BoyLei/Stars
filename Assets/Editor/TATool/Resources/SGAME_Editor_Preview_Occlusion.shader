Shader "SGAME/Editor_Preview_Occlusion"
{
    Properties
    {
        [MainTexture]_MainTex ("主贴图(A为透明通道)", 2D) = "white" {}
        [HideInInspector]_EmissionCol("Meta的自发光颜色",color) = (0,0,0,1)
        _AoScale("AO强度",range(0,3)) = 1
        [HideInInspector]_Cutoff("控制AlphaClip的閾值",range(0,1)) = 0.2
        _TotalAlpha("整体透明度", range(0,1)) = 1
    }
    SubShader
    {
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
        HLSLINCLUDE
        #include "../../../Res/Shader/lib/Lighting_Urp_12.1.9.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float _TotalAlpha;
            float4 _MainColor;
            float4 _MainTex_ST;
            float _Cutoff;

            CBUFFER_END
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
        ENDHLSL

        Pass
        {
            Cull Off
            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 2.0
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			#pragma multi_compile _ LIGHTMAP_ON

            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv0          : TEXCOORD0;
                float2 uv1          : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS       : SV_POSITION;
                float2 uv               : TEXCOORD0;
                float2 staticLightmapUV : TEXCOORD1;
                float3 normalWS         : TEXCOORD2;
            };  

            Varyings vert(Attributes v)
            {
                Varyings output = (Varyings)0;
                output.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                output.uv = TRANSFORM_TEX(v.uv0, _MainTex);
                output.normalWS.xyz = TransformObjectToWorldNormal(v.normalOS.xyz);
                output.staticLightmapUV = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                return output;
            }

            // Sample baked lightmap. Non-Direction and Directional if available.
            half3 _SampleLightmap(float2 lightmapUV, half3 normalWS)
            {
                bool encodedLightmap = true;
                half4 decodeInstructions = half4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0h, 0.0h);
                // The shader library sample lightmap functions transform the lightmap uv coords to apply bias and scale.
                // However, universal pipeline already transformed those coords in vertex. We pass half4(1, 1, 0, 0) and
                // the compiler will optimize the transform away.
                half4 transformCoords = half4(1, 1, 0, 0);
                return SampleSingleLightmap(TEXTURE2D_ARGS(unity_Lightmap, samplerunity_Lightmap), lightmapUV, transformCoords, encodedLightmap, decodeInstructions);
            
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                half4 diffuse = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half alpha = diffuse.a - _Cutoff;
                clip(alpha); 
                float lightmapInfoClipLength = length(unity_LightmapST);
#if defined(LIGHTMAP_ON)
                half3 bakedLm = 0.0;
                float3 bakedColor = _SampleLightmap(input.staticLightmapUV, input.normalWS);
                //bakedColor = dot(bakedColor, float3(0.2126, 0.7152, 0.0722));
                return float4(bakedColor, 1);
#else
                discard;
                return 0;
#endif
            }
            ENDHLSL
        }
    }
}
