Shader "SGame/Terrain/TerrainLit_TArray"
{
    Properties
    {
        // set by terrain engine
        _Control("Control (RGBA)", 2D) = "red" {}
        _SplatArray("Layer Array",2DArray) = ""{}
        _NormalScaleRemap("Normal Scale Remap",Range(0, 8)) = 0.25
        _NormalArray("Normal Array",2DArray) = ""{}  
      
        [Header(Layer 0)]
        /*[HDR]*/_Color0("Color", Color) = (1,1,1,1)
        _NormalScale0("Normal Strength",Range(-5, 5)) = 1
        _Splat0("Layer 0 (Contorol-R)", 2D) = "grey" {}
         [Gamma] _Metallic0("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness0("Smoothness", Range(0.0, 1.0)) = 0.0
        [HideInInspector]_DiffuseRemapScale0("Diffuse Remap 0", Vector) = (1,1,1,1)


        [Header(Layer 1)]
        /*[HDR]*/_Color1("Color", Color) = (1,1,1,1)
        _NormalScale1("Normal Strength",Range(-5, 5)) = 1
        _Splat1("Layer (Contorol-G)", 2D) = "grey" {}
        [Gamma] _Metallic1("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness1("Smoothness", Range(0.0, 1.0)) = 0.0
        [HideInInspector]_DiffuseRemapScale1("Diffuse Remap 1", Vector) = (1,1,1,1)


        [Header(Layer 2)]
        /*[HDR]*/_Color2("Color", Color) = (1,1,1,1)
        _NormalScale2("Normal Strength",Range(-5, 5)) = 1
        _Splat2("Layer (Contorol-B)", 2D) = "grey" {}
         [Gamma] _Metallic2("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness2("Smoothness", Range(0.0, 1.0)) = 0.0
        [HideInInspector]_DiffuseRemapScale2("Diffuse Remap 2", Vector) = (1,1,1,1)


        [Header(Layer 3)]
        /*[HDR]*/_Color3("Color", Color) = (1,1,1,1)
        _NormalScale3("Normal Strength",Range(-5, 5)) = 1
        _Splat3("Layer (Contorol-A)", 2D) = "grey" {}
        [Gamma] _Metallic3("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness3("Smoothness", Range(0.0, 1.0)) = 0.0
        [HideInInspector]_DiffuseRemapScale3("Diffuse Remap 3", Vector) = (1,1,1,1)

        [Header(HeightTransition)]
        _HeightTransition("HeightTransition",range(0,1)) = 0.07
        // used in fallback on old cards & base map
        [HideInInspector] _MainTex("BaseMap (RGB)", 2D) = "grey" {}
        //[HideInInspector] _BaseColor("Main Color", Color) = (1, 1, 1, 1)
    }

   SubShader
   {

        Pass
        {
            Tags { "Queue" = "Geometry+2" 
                        "RenderType" = "Opaque" 
                        "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "False" "TerrainCompatible" = "True"}
            Name "DownScale"
            Tags { "LightMode" = "DownScaled" }
            ZWrite On

            HLSLPROGRAM
                // Required to compile gles 2.0 with standard srp library
                #pragma prefer_hlslcc gles
                #pragma exclude_renderers d3d11_9x
                #pragma target 3.0

                #pragma vertex SplatmapVert
                #pragma fragment SplatmapFragment
               
                // Material Keywords
                //#pragma shader_feature_local _NORMALMAP
                #define _MAIN_LIGHT_SHADOWS 1
                #pragma multi_compile _ /*_MAIN_LIGHT_SHADOWS*/ _MAIN_LIGHT_SHADOWS_CASCADE //_MAIN_LIGHT_SHADOWS_SCREEN
                #pragma multi_compile _ /*_ADDITIONAL_LIGHTS_VERTEX*/ _ADDITIONAL_LIGHTS
             
                #pragma multi_compile _ DIRLIGHTMAP_COMBINED
                #pragma multi_compile _ LIGHTMAP_ON
                //#pragma multi_compile _ DYNAMICLIGHTMAP_ON
                //#define FOG_LINEAR 1
                #define _SHADOWS_SOFT 1
                #pragma multi_compile _ FOG_LINEAR
                //#pragma multi_compile_fragment _ DEBUG_DISPLAY
                #pragma shader_feature _ _CUSTOM_SHADOW_ON
                #pragma shader_feature _ _MAINLIGHT_CUSTOM_SHADOW_ON
                #pragma multi_compile _ _ESM
                

                //--------------------------------------
            
                //Sgame Terrain
                //#pragma shader_feature_local _ _TERRAIN_BLEND_HEIGHT_ON
                #define _TERRAIN_BLEND_HEIGHT_ON 1
                //#pragma shader_feature_local _ _CONTROLSECOND_ON
                #define _IFBSMOOTHNESS_ON 1
                //#pragma shader_feature_local _ _IFBSMOOTHNESS_ON
                ////////////////////////////////////////////////////////////////////////
                #include "../lib/SGAME-TerrainLitInput_TArray.hlsl"
                #include "../lib/SGAME-TerrainLitPasses_TArray.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" "RenderPipeline" = "UniversalPipeline"}
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            TEXTURE2D(_GlobalDownScaledOpaques);
            SAMPLER(sampler_GlobalDownScaledOpaques);

            struct Attributes
            {
                // The positionOS variable contains the vertex positions in object
                // space.
                float4 positionOS   : POSITION;                 
            };


            struct Varyings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS  : SV_POSITION;
                float4 screenPos : TEXCOORD0;
            };            

            // The vertex shader definition with properties defined in the Varyings 
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
			    OUT.screenPos = ComputeScreenPos( OUT.positionHCS );
                return OUT;
            }

            // The fragment shader definition.            
            half4 frag(Varyings i) : SV_Target
            {
                half2 screen_uv = i.screenPos.xy / i.screenPos.w;
                return SAMPLE_TEXTURE2D(_GlobalDownScaledOpaques, sampler_GlobalDownScaledOpaques, screen_uv);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            //ColorMask 0

            HLSLPROGRAM
            #pragma target 2.0

            #pragma multi_compile _ _ESM

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            //#pragma multi_compile_instancing
            //#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            //#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitPasses.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

            #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitPasses.hlsl"
            ENDHLSL
        }

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}

            Cull Off

            HLSLPROGRAM
            #pragma vertex TerrainVertexMeta
            #pragma fragment TerrainFragmentMeta

            //#pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap
            //#define _METALLICSPECGLOSSMAP 11
            //#define _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A 1

            #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitMetaPass.hlsl"

            ENDHLSL
        }
        UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
    }
    CustomEditor "SGAMETerrainLitShaderGUI"
    FallBack "Hidden/InternalErrorShader"
}
