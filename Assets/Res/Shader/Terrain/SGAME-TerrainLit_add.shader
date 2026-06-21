Shader "SGame/TerrainLit_ADD"
{
    Properties
    {

        // set by terrain engine
        _Control("Control (RGBA)", 2D) = "red" {}
        _Control2("Contro2 (RGBA)", 2D) = "black" {}
        _Splat3("Layer 3 (A)", 2D) = "grey" {}
        _Splat2("Layer 2 (B)", 2D) = "grey" {}
        _Splat1("Layer 1 (G)", 2D) = "grey" {}
        _Splat0("Layer 0 (R)", 2D) = "grey" {}
        _Splat5("Layer 5 (G)", 2D) = "grey" {}
        _Splat4("Layer 4 (R)", 2D) = "grey" {}
        
        [NoScaleOffset]_Normal3("Normal 3 (A)", 2D) = "bump" {}
        [NoScaleOffset]_Normal2("Normal 2 (B)", 2D) = "bump" {}
        [NoScaleOffset]_Normal1("Normal 1 (G)", 2D) = "bump" {}
        [NoScaleOffset]_Normal0("Normal 0 (R)", 2D) = "bump" {}

        [Gamma] _Metallic0("Metallic 0", Range(0.0, 1.0)) = 0.0
        [Gamma] _Metallic1("Metallic 1", Range(0.0, 1.0)) = 0.0
        [Gamma] _Metallic2("Metallic 2", Range(0.0, 1.0)) = 0.0
        [Gamma] _Metallic3("Metallic 3", Range(0.0, 1.0)) = 0.0
        [Gamma] _Metallic4("Metallic 4", Range(0.0, 1.0)) = 0.0
        [Gamma] _Metallic5("Metallic 5", Range(0.0, 1.0)) = 0.0

        _Smoothness0("Smoothness 0", Range(0.0, 1.0)) = 0.5
        _Smoothness1("Smoothness 1", Range(0.0, 1.0)) = 0.5
        _Smoothness2("Smoothness 2", Range(0.0, 1.0)) = 0.5
        _Smoothness3("Smoothness 3", Range(0.0, 1.0)) = 0.5  
        _Smoothness4("Smoothness 4", Range(0.0, 1.0)) = 0.0
        _Smoothness5("Smoothness 5", Range(0.0, 1.0)) = 0.0

        [HideInInspector]_DiffuseRemapScale0("",vector) = (0,0,0,0)
        [HideInInspector]_DiffuseRemapScale1("",vector) = (0,0,0,0)
        [HideInInspector]_DiffuseRemapScale2("",vector) = (0,0,0,0)
        [HideInInspector]_DiffuseRemapScale3("",vector) = (0,0,0,0)
        [HideInInspector]_DiffuseRemapScale4("",vector) = (1,1,1,1)
        [HideInInspector]_DiffuseRemapScale5("",vector) = (1,1,1,1)
        
        [HideInInspector]_NormalScale0("",float) = 1
        [HideInInspector]_NormalScale1("",float) = 1
        [HideInInspector]_NormalScale2("",float) = 1
        [HideInInspector]_NormalScale3("",float) = 1
        [HideInInspector]_NormalScale4("",float) = 1
        [HideInInspector]_NormalScale5("",float) = 1   
        
        }

    HLSLINCLUDE


    ENDHLSL

    SubShader
    {
        Tags { "Queue" = "Geometry+1" "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "False"}

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 3.0

            #pragma vertex SplatmapVert
            #pragma fragment SplatmapFragment

            #define _METALLICSPECGLOSSMAP 1
            #define _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A 1

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3

            #pragma shader_feature_local _NORMALMAP

            #include "../lib/SGAME-TerrainLitInput.hlsl"
            #include "../lib/SGAME-TerrainLitPasses.hlsl"

            ENDHLSL
        }






        // This pass is used when drawing to a _CameraNormalsTexture texture
        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals"}

            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11
            #pragma target 2.0

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #include "../lib/SGAME-TerrainLitInput.hlsl"
           // #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 position     : POSITION;
                float2 texcoord     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv           : TEXCOORD0;
                float4 positionCS   : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.uv =input.texcoord;// TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.positionCS = TransformObjectToHClip(input.position.xyz);
                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                //Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }
    
    //CustomEditor "UnityEditor.Rendering.Universal.TerrainLitShaderGUI"
    CustomEditor "SGAMETerrainLitShaderGUI"
    
   // Fallback "Hidden/Universal Render Pipeline/FallbackError"
    Fallback "Hidden/InternalErrorShader"
}
