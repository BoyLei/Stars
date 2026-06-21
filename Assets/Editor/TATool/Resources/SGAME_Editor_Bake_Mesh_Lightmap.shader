Shader "SGAME/Editor_Bake_Mesh_Lightmap"
{
    Properties
    {
        _unity_LightmapST("_unity_LightmapST", Vector) = (0, 0, 1, 1)
        _TerrainWorldInfoMin("_TerrainWorldInfoMin", Vector) = (0, 0, 1, 1)
        _TerrainWorldInfoMax("_TerrainWorldInfoMax", Vector) = (0, 0, 1, 1)
    }
    SubShader
    {
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
        HLSLINCLUDE
        #include "../../../Res/Shader/lib/Lighting_Urp_12.1.9.hlsl"
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


            #pragma vertex vert
            #pragma fragment frag
            float4 _unity_LightmapST;
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv0          : TEXCOORD0;
                float2 uv1          : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS       : SV_POSITION;
                float2 staticLightmapUV : TEXCOORD0;
            };  

            float2 _TerrainWorldInfoMin;
            float2 _TerrainWorldInfoMax;

            Varyings vert(Attributes v)
            {
                Varyings output = (Varyings)0;

                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
                worldPos.x = (worldPos.x - _TerrainWorldInfoMin.x) / (_TerrainWorldInfoMax.x - _TerrainWorldInfoMin.x);
                worldPos.y = (worldPos.z - _TerrainWorldInfoMin.y) / (_TerrainWorldInfoMax.y - _TerrainWorldInfoMin.y);
                output.positionCS = TransformObjectToHClip(worldPos);
               
                output.staticLightmapUV = v.uv1.xy * _unity_LightmapST.xy + _unity_LightmapST.zw;
                return output;
            }

            // Sample baked lightmap. Non-Direction and Directional if available.
            half3 _SampleLightmap(float2 lightmapUV)
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
                half3 bakedLm = 0.0;
                float3 bakedColor = _SampleLightmap(input.staticLightmapUV);
                return float4(1, 1, 0, 1);
            }
            ENDHLSL
        }
    }
}
