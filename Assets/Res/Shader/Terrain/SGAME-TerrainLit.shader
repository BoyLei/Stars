Shader "SGame/Terrain/TerrainLit"
{
    Properties
    {
        // set by terrain engine
       _Control("Control (RGBA)", 2D) = "red" {}
       [KeywordEnum(OFF,ON)]_ControlSecond("是否启用第二张Control2贴图", int) = 0
       _Control2("Contro2 (RGBA)", 2D) = "black" {}
       
       [Space(20)]
       [KeywordEnum(OFF,ON)]_TERRAIN_BLEND_HEIGHT("是否启用高度图",int) = 0
       [Space(20)]
       /*[NoScaleOffset]*/_Splat0("Layer 0 (Contorol-R)(A通道--高度图)", 2D) = "grey" {}
       [HDR]_Color0("MainColor0",Color) = (1,1,1,1)
       [Toggle]_IfAlpha_Height0("A通道是否存储高度贴图(使用高度贴图需开启)",int) = 0
       _HeightScale0("高度图的Amplitude(默认的1对应100)",Range(0,11)) = 1
           //[Toggle]_IfAlpha_Smoothness0("A通道是否存储光滑度贴图",int) = 0
           [Space(20)]
       /*[NoScaleOffset]*/_Splat1("Layer 1 (Contorol-G)(A通道--高度图)", 2D) = "grey" {}
       [HDR]_Color1("MainColor1",Color) = (1,1,1,1)
       [Toggle]_IfAlpha_Height1("A通道是否存储高度贴图(使用高度贴图需开启)",int) = 0
       _HeightScale1("高度图的Amplitude(默认的1对应100)",Range(0,11)) = 1
           //[Toggle]_IfAlpha_Smoothness1("A通道是否存储光滑度贴图",int) = 0
            [Space(20)]
       /*[NoScaleOffset]*/_Splat2("Layer 2 (Contorol-B)(A通道--高度图)", 2D) = "grey" {}
       [HDR]_Color2("MainColor2",Color) = (1,1,1,1)
        [Toggle]_IfAlpha_Height2("A通道是否存储高度贴图(使用高度贴图需开启)",int) = 0
        _HeightScale2("高度图的Amplitude(默认的1对应100)",Range(0,11)) = 1
           //[Toggle]_IfAlpha_Smoothness2("A通道是否存储光滑度贴图",int) = 0
       
            [Space(20)]
       /*[NoScaleOffset]*/_Splat3("Layer 3 (Contorol-A)(A通道--高度图)", 2D) = "grey" {}
       [HDR]_Color3("MainColor3",Color) = (1,1,1,1)
       [Toggle]_IfAlpha_Height3("A通道是否存储高度贴图(使用高度贴图需开启)",int) = 0
       _HeightScale3("高度图的Amplitude(默认的1对应100)",Range(0,11)) = 1
           //[Toggle]_IfAlpha_Smoothness3("A通道是否存储光滑度贴图",int) = 0
            [Space(20)]
       /*[NoScaleOffset]*/_Splat4("Layer 4 (Contorol2-R)(A通道--高度图)", 2D) = "grey" {}
       [HDR]_Color4("MainColor4",Color) = (1,1,1,1)
       [Toggle]_IfAlpha_Height4("A通道是否存储高度贴图(使用高度贴图需开启)",int) = 0
       _HeightScale4("高度图的Amplitude(默认的1对应100)",Range(0,11)) = 1
           //[Toggle]_IfAlpha_Smoothness4("A通道是否存储光滑度贴图",int) = 0
            [Space(20)]
       /*[NoScaleOffset]*/_Splat5("Layer 5 (Contorol2-G)(A通道--高度图)", 2D) = "grey" {}
       [HDR]_Color5("MainColor5",Color) = (1,1,1,1)
       [Toggle]_IfAlpha_Height5("A通道是否存储高度贴图(使用高度贴图需开启)",int) = 0
       _HeightScale5("高度图的Amplitude(默认的1对应100)",Range(0,11)) = 1
        //[Toggle]_IfAlpha_Smoothness5("A通道是否存储光滑度贴图",int) = 0

        //[Space(20)]
        //[Header(Smoothness)]
        //[Toggle]_ifSmoothnessTex("是否启用前四个Layer的光滑度贴图",int) = 0
        //_SmoothnessTex("光滑度图,rgba四个通道对应0-3Layer的光滑度贴图", 2D) =  "black" {}


        [Space(20)]
        [Header(HeightTransition)]
        _HeightTransition("HeightTransition",range(0,1)) = 0.07


        [Space(20)]
        [Header(NormalTex)]
        [KeywordEnum(OFF,ON)]_IfBSmoothness("法线的B通道是否存储光滑度图",int) = 0
        [Space(20)]
        [NoScaleOffset]_Normal0("Normal 0", 2D) = "bump" {}
             

        [Space(20)]
        [NoScaleOffset]_Normal1("Normal 1", 2D) = "bump" {}


        [Space(20)]
        [NoScaleOffset]_Normal2("Normal 2", 2D) = "bump" {}


        [Space(20)]
        [NoScaleOffset]_Normal3("Normal 3", 2D) = "bump" {}


        [Space(20)]
        [NoScaleOffset]_Normal4("Normal 4", 2D) = "bump" {}


        [Space(20)]
        [NoScaleOffset]_Normal5("Normal 5", 2D) = "bump" {}


        [Space(20)]

        [Gamma] _Metallic0("Metallic 0", Range(0.0, 1.0)) = 0.0
        [Gamma] _Metallic1("Metallic 1", Range(0.0, 1.0)) = 0.0
        [Gamma] _Metallic2("Metallic 2", Range(0.0, 1.0)) = 0.0
        [Gamma] _Metallic3("Metallic 3", Range(0.0, 1.0)) = 0.0
        [Gamma] _Metallic4("Metallic 4", Range(0.0, 1.0)) = 0.0
        [Gamma] _Metallic5("Metallic 5", Range(0.0, 1.0)) = 0.0

        _Smoothness0("Smoothness 0", Range(0.0, 1.0)) = 0.0
        _Smoothness1("Smoothness 1", Range(0.0, 1.0)) = 0.0
        _Smoothness2("Smoothness 2", Range(0.0, 1.0)) = 0.0
        _Smoothness3("Smoothness 3", Range(0.0, 1.0)) = 0.0
        _Smoothness4("Smoothness 4", Range(0.0, 1.0)) = 0.0
        _Smoothness5("Smoothness 5", Range(0.0, 1.0)) = 0.0


        [HideInInspector]_DiffuseRemapScale0("",vector) = (1,1,1,1)
        [HideInInspector]_DiffuseRemapScale1("",vector) = (1,1,1,1)
        [HideInInspector]_DiffuseRemapScale2("",vector) = (1,1,1,1)
        [HideInInspector]_DiffuseRemapScale3("",vector) = (1,1,1,1)
        [HideInInspector]_DiffuseRemapScale4("",vector) = (1,1,1,1)
        [HideInInspector]_DiffuseRemapScale5("",vector) = (1,1,1,1)

        [HideInInspector]_NormalScale0("",float) = 1
        [HideInInspector]_NormalScale1("",float) = 1
        [HideInInspector]_NormalScale2("",float) = 1
        [HideInInspector]_NormalScale3("",float) = 1
        [HideInInspector]_NormalScale4("",float) = 1
        [HideInInspector]_NormalScale5("",float) = 1

        // used in fallback on old cards & base map
        [HideInInspector] _MainTex("BaseMap (RGB)", 2D) = "grey" {}
        [HideInInspector] _BaseColor("Main Color", Color) = (1,1,1,1)

        [HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}

        [ToggleUI] _EnableInstancedPerPixelNormal("Enable Instanced per-pixel normal", Float) = 1.0
    }

   HLSLINCLUDE

    #pragma multi_compile_fragment __ _ALPHATEST_ON

   ENDHLSL

   SubShader
   {
        Tags { "Queue" = "Geometry" "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "False" "TerrainCompatible" = "True"}

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On

            HLSLPROGRAM
                // Required to compile gles 2.0 with standard srp library
                #pragma prefer_hlslcc gles
                #pragma exclude_renderers d3d11_9x
                #pragma target 3.0

                #pragma vertex SplatmapVert
                #pragma fragment SplatmapFragment

                #define _METALLICSPECGLOSSMAP 1
                #define _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A 1

                // // -------------------------------------
                // // Universal Pipeline keywords
                //
                // #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
                // #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
                // #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
                // #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
                // #pragma multi_compile _ _SHADOWS_SOFT
                // #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
                //
                // // -------------------------------------
                // // Unity defined keywords
                // #pragma multi_compile _ DIRLIGHTMAP_COMBINED
                // #pragma multi_compile _ LIGHTMAP_ON
                // #pragma multi_compile_fog
                //
                // #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
                //
                // #pragma shader_feature_local _NORMALMAP

               //使得shader代码可调试,上线时注释掉
                #pragma enable_d3d11_debug_symbols

                ////////////////////////////////////////////////////////////////////////
                            // -------------------------------------
                // Material Keywords
                #pragma shader_feature_local _NORMALMAP
                //#pragma shader_feature_local _PARALLAXMAP
                //#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
                #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
                //#pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
                #pragma shader_feature_local_fragment _ALPHATEST_ON
                #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
                //#pragma shader_feature_local_fragment _EMISSION
                //#pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
                //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
                //#pragma shader_feature_local_fragment _OCCLUSIONMAP
                #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
                #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
                #pragma shader_feature_local_fragment _SPECULAR_SETUP

                // -------------------------------------
                // Universal Pipeline keywords
                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
                #pragma multi_compile _ /*_ADDITIONAL_LIGHTS_VERTEX*/ _ADDITIONAL_LIGHTS
                //#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
                #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
                #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
                #pragma multi_compile_fragment _ _SHADOWS_SOFT
                //#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
                //#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
                #pragma multi_compile_fragment _ _LIGHT_LAYERS
                //#pragma multi_compile_fragment _ _LIGHT_COOKIES
                #pragma multi_compile _ _CLUSTERED_RENDERING

                // -------------------------------------
                // Unity defined keywords
                #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
                //#pragma multi_compile _ SHADOWS_SHADOWMASK
                #pragma multi_compile _ DIRLIGHTMAP_COMBINED
                #pragma multi_compile _ LIGHTMAP_ON
                //#pragma multi_compile _ DYNAMICLIGHTMAP_ON
                #pragma multi_compile_fog
                //#pragma multi_compile_fragment _ DEBUG_DISPLAY

                //--------------------------------------
                // GPU Instancing
                #pragma multi_compile_instancing
                #pragma instancing_options renderinglayer
                #pragma multi_compile _ DOTS_INSTANCING_ON

                //Sgame Terrain
                #pragma shader_feature_local _ _TERRAIN_BLEND_HEIGHT_ON 
                #pragma shader_feature_local _ _CONTROLSECOND_ON
                #pragma shader_feature_local _ _IFBSMOOTHNESS_ON

                ////////////////////////////////////////////////////////////////////////
                #include "../lib/SGAME-TerrainLitInput.hlsl"
                #include "../lib/SGAME-TerrainLitPasses.hlsl"

                ENDHLSL
            }

            Pass
            {
                Name "ShadowCaster"
                Tags{"LightMode" = "ShadowCaster"}

                ZWrite On
                ColorMask 0

                HLSLPROGRAM
                #pragma target 2.0

                #pragma vertex ShadowPassVertex
                #pragma fragment ShadowPassFragment

                //#pragma multi_compile_instancing
                #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

                // -------------------------------------
                // Universal Pipeline keywords

                // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
                #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

                #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitPasses.hlsl"
                ENDHLSL
            }

            //Pass
            //{
            //    Name "GBuffer"
            //    Tags{"LightMode" = "UniversalGBuffer"}

            //    HLSLPROGRAM
            //    #pragma exclude_renderers gles
            //    #pragma target 3.0
            //    #pragma vertex SplatmapVert
            //    #pragma fragment SplatmapFragment

            //    #define _METALLICSPECGLOSSMAP 1
            //    #define _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A 1

            //    // -------------------------------------
            //    // Universal Pipeline keywords
            //    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            //    //#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            //    //#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            //    #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            //    #pragma multi_compile_fragment _ _SHADOWS_SOFT
            //    #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            //    #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            //    #pragma multi_compile_fragment _ _LIGHT_LAYERS

            //    // -------------------------------------
            //    // Unity defined keywords
            //    #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            //    #pragma multi_compile _ SHADOWS_SHADOWMASK
            //    #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            //    #pragma multi_compile _ LIGHTMAP_ON
            //    #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            //    #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            //    #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED

            //    //#pragma multi_compile_fog
            //    #pragma multi_compile_instancing
            //    #pragma instancing_options norenderinglayer assumeuniformscaling nomatrices nolightprobe nolightmap

            //    #pragma shader_feature_local _TERRAIN_BLEND_HEIGHT
            //    #pragma shader_feature_local _NORMALMAP
            //    #pragma shader_feature_local _MASKMAP
            //    // Sample normal in pixel shader when doing instancing
            //    #pragma shader_feature_local _TERRAIN_INSTANCED_PERPIXEL_NORMAL
            //    #define TERRAIN_GBUFFER 1

            //    #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
            //    #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitPasses.hlsl"
            //    ENDHLSL
            //}

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

                // This pass is used when drawing to a _CameraNormalsTexture texture
//                Pass
//                {
//                    Name "DepthNormals"
//                    Tags{"LightMode" = "DepthNormals"}
//
//                    ZWrite On
//
//                    HLSLPROGRAM
//                    #pragma target 2.0
//                    #pragma vertex DepthNormalOnlyVertex
//                    #pragma fragment DepthNormalOnlyFragment
//
//                    #pragma shader_feature_local _NORMALMAP
//                    #pragma multi_compile_instancing
//                    #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap
//
//                    #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
//                    #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitDepthNormalsPass.hlsl"
//                    ENDHLSL
//                }

                //Pass
                //{
                //    Name "SceneSelectionPass"
                //    Tags { "LightMode" = "SceneSelectionPass" }

                //    HLSLPROGRAM
                //    #pragma target 2.0

                //    #pragma vertex DepthOnlyVertex
                //    #pragma fragment DepthOnlyFragment

                //    #pragma multi_compile_instancing
                //    #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

                //    #define SCENESELECTIONPASS
                //    #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
                //    #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitPasses.hlsl"
                //    ENDHLSL
                //}

                // This pass it not used during regular rendering, only for lightmap baking.
                Pass
                {
                   Name "Meta"
                    Tags{"LightMode" = "Meta"}

                    Cull Off

                    HLSLPROGRAM
                    #pragma vertex TerrainVertexMeta
                    #pragma fragment TerrainFragmentMeta

                    #pragma multi_compile_instancing
                    #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap
                    #define _METALLICSPECGLOSSMAP 11
                    #define _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A 1

                    #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
                    #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitMetaPass.hlsl"

                    ENDHLSL
                }

                UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
            }
//                Dependency "AddPassShader" = "Hidden/Universal Render Pipeline/Terrain/Lit (Add Pass)"
//                Dependency "BaseMapShader" = "Hidden/Universal Render Pipeline/Terrain/Lit (Base Pass)"
//                Dependency "BaseMapGenShader" = "Hidden/Universal Render Pipeline/Terrain/Lit (Basemap Gen)"

             //   CustomEditor "UnityEditor.Rendering.Universal.TerrainLitShaderGUI"
                CustomEditor "SGAMETerrainLitShaderGUI"

               // Fallback "Hidden/Universal Render Pipeline/FallbackError"
            FallBack "Hidden/InternalErrorShader"
}
