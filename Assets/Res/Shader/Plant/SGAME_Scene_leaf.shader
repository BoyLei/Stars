Shader "SGAME/SGAME_Scene_leaf"
{
    Properties
    {
        [Header(BaseColor)]
        [Enum(NotBaseColor,0,BaseColor,1)]_IfBaseColor("主帖图的RGB通道是基础色还是普通的渐变色?",float) = 0
        [MainTexture]_BaseMap ("主贴图(A为透明通道)", 2D) = "white" {}
        _Cutoff("AlphaClip范围",Range(0,1)) = 0.2
        //[HideInInspector]_Cutoff("",Range(0,1)) = 0.5
        [HDR]_BaseColor("树叶主颜色",Color) = (1,1,1,1)
        _LightSideColorA("朝光面颜色A",Color) = (1,1,1,1)
        _LightSideColorB("朝光面颜色B",Color) = (1,1,1,1)
        _LightSideColorChange("朝光面颜色渐变",Range(0,1)) = 0.5
        _LightMaskStrength("朝光面Mask强度(主帖图的G通道控制)",Range(0,2)) = 1
        
        _DarkSideColor("背光面颜色",Color) = (1,1,1,1)
        _DarkSideColorChange("背光面颜色渐变",range(0,1)) = 0.5
        _DarkMaskStrength("背光面Mask强度(主帖图的R通道控制)",Range(0,2)) = 1
        
        [Space(10)]
        _ColorChangeControl1("颜色明暗丰富1",Range(0,1.5)) = 1
        _ColorChangeControl2("颜色明暗丰富2",Range(0,1.5)) = 1
        
        [Header(AO)]
        _AOColor("AO的颜色(顶点色r通道控制,r值越低则ao越深,若没有设置顶点色则把下面的ao强度调为0)",Color) = (0.2,0.2,0.2,1)
        _AOStrength("AO强度(越低则ao越弱)",Range(0,8)) = 0
        
        [Header(Shadow and Environment)]
        _DarkStrength("暗部强度",range(0,1)) = 1
        _ShadowStrength("投影强度",range(0,2)) = 1
        _GIstrength("环境光强度",range(0,2)) = 1
        
        [Header(LightMap)]
        _LocalLMAOStrength("烘焙在LightMap中AO的强度(仅在烘焙有用)",range(0,1)) = 0
        
        [Header(Wind)]
        _WindZoneTex("风场噪声贴图",2D) = "white" {}
        _WindZoneSpeed("风场噪声图运动速度",range(0,3)) = 1
        _WindZoneDensity("风场噪声图的密度",range(0,5)) = 1
        _WindDir("风的方向",Vector)= (1,0,1,1)
        _WindStrength("风的强度",range(0,0.2)) = 0
        _ShakeRange("树叶的摆动强度",range(0,2)) = 0
        
        [HideInInspector]_LightDirection("",Vector) = (0,0,0,0)
        
    }
    SubShader
    {
        Tags 
        { 
            //"Queue" = "Geometry"
            //"RenderPipeline" = "UniversalPipeline"
              "Queue" = "AlphaTest"
        }
        LOD 100
        HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float _IfBaseColor;
                
                float _AlphaClip;
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _WindZoneTex_ST;
                float _Cutoff;

                float4 _LightSideColorA;
                float4 _LightSideColorB;
                float _LightSideColorChange;
                float _LightMaskStrength;
        
                
                float4 _DarkSideColor;
                float _DarkSideColorChange;
                float _DarkMaskStrength;

                float _ColorChangeControl1;
                float _ColorChangeControl2;

                float4 _AOColor;
                float _AOStrength;
        
                float _AoScale;
                float _DarkStrength;
                float _ShadowStrength;
                float _GIstrength;

                float _LocalLMAOStrength;
        
                float4 _WindDir;
                float _WindZoneSpeed;
                float _WindZoneDensity;
                float _WindStrength;
                float _ShakeRange;
                //深度贴图
                
                float3 _LightDirection;

                //float _ClipDis;
                //float4 _Offset;
                //float _DitherMax;
                //float _UseDitherClip;
                //float3 _PlayerPos;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            // TEXTURE2D(_MainTex);
            // SAMPLER(sampler_MainTex);
            //TEXTURE2D (_CameraDepthTexture);
            //SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D (_WindZoneTex);
            SAMPLER(sampler_WindZoneTex);
           
            void LeafWindNoise(float3 positionOS, inout float3 positionOSout, inout float2 uvOffset)
            {
                //树叶风动相关代码
                float distanceToMiddle =distance(float2(positionOS.x,positionOS.z),float2(0,0));
                distanceToMiddle = pow(distanceToMiddle,1.4);
                float3 posWS = TransformObjectToWorld(positionOS);
                float3 windDir = normalize(_WindDir);
                float2 moveOffset = windDir.xz * _Time.x/(3*_WindZoneSpeed);
                float2 noiseUV = posWS * _WindZoneDensity + moveOffset;
                
                float noise = SAMPLE_TEXTURE2D_LOD(_WindZoneTex,sampler_WindZoneTex,noiseUV,0);
                noise = noise * 2 -1;
                float3 moveOffsetWS = clamp(float3(noise * _WindStrength,noise * _WindStrength,noise * _WindStrength),-0.2,0.2);
                positionOSout = TransformWorldToObject(moveOffsetWS * (windDir + float3(0.1,0.1,0.1)) * distanceToMiddle *1 + noise * _WindStrength *windDir * distanceToMiddle+ posWS);
                uvOffset = noise * _ShakeRange * 0.1;
                //mainTexUVOffset.xy = float2(noise,noise);
            }
        ENDHLSL
        
        Pass
        {
            Name"lightForward"
            Tags
            {
                "RenderPipeline" = "UniversalPipeline"
                "LightMode" = "UniversalForward"
            }
            //Blend SrcAlpha OneMinusSrcAlpha, One Zero
            //Blend One Zero, One Zero
            
            //Blend Off
            //Cull Back
            Cull Off
            ZTest LEqual
            //ZWrite on
            
            HLSLPROGRAM
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "./../../../ThirdParts/GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            //#pragma multi_compile _ _ADDITIONAL_LIGHTS
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS

            //#pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #define _MAIN_LIGHT_SHADOWS_CASCADE 1
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE  //_MAIN_LIGHT_SHADOWS_SCREEN
            #define _SHADOWS_SOFT 1
           // #pragma multi_compile_fragment _ _SHADOWS_SOFT
            //#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            //#pragma multi_compile _ SHADOWS_SHADOWMASK
            // #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            //#pragma shader_feature_local _ _DEPTH_RIM_LIGHT
            #pragma multi_compile_fog

            // #pragma shader_feature _ _CUSTOM_SHADOW_ON
            // #pragma shader_feature _ _MAINLIGHT_CUSTOM_SHADOW_ON
            // #pragma multi_compile _ _ESM
            
            
            // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
            #include "../lib//Lighting.hlsl"
            #include "../lib/SGAME-Common.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
                float2 texcoord2 : TEXCOORD1;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float4 normalWS : TEXCOORD1;
                float4 positionVS : TEXCOORD2;
                //float4 tangentWS : TEXCOORD3;
                //float4 bioTangentWS : TEXCOORD4;
                float4 color : TEXCOORD3;
                float3 positionWS : TEXCOORD4;
                float4 fogFactorAndVertexLight : TEXCOORD5;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 6);
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
         ///抖动剔除相关
        void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }
        
        void Unity_Dither_float(float In, float4 ScreenPosition, out float Out)
        {
            float2 uv = ScreenPosition.xy * _ScreenParams.xy;
            float DITHER_THRESHOLDS[16] =
            {
                1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            };
            uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
            Out = In - DITHER_THRESHOLDS[index];
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        /////////////////////////////////////////////////////////////////////////////////
        Varyings vert (Attributes v)
        {
            Varyings o = (Varyings)0;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            
            o.uv.xy = v.texcoord*_BaseMap_ST.xy + _BaseMap_ST.zw;
            // o.uv2.xy = v.texcoord*_NormalTex_ST.xy + _NormalTex_ST.zw;
            // o.uv2.zw = v.texcoord2*_MRAETex_ST.xy + _MRAETex_ST.zw;
            
            float3 posOS = float3(0,0,0);
            float2 mainTexUVOffset =float2(0,0);
            //mainTexUVOffset.xy = float2(noise,noise);
            LeafWindNoise(v.positionOS,posOS.xyz,mainTexUVOffset);
                
            o.positionCS = TransformObjectToHClip(posOS.xyz);
            o.uv.z = ComputeFogFactor(o.positionCS.z);    //uv的z值存储雾.
            o.positionWS = TransformObjectToWorld(posOS.xyz);
            o.normalWS.xyz = TransformObjectToWorldNormal(v.normalOS.xyz);
            o.positionVS.xyz = TransformWorldToView(o.positionWS.xyz);
            o.color.z = v.color.r;
            //o.color.a = distanceToMiddle;
            o.color.xy = mainTexUVOffset;
            half3 vertexLight = VertexLighting(o.positionWS, o.normalWS);
            half fogFactor = ComputeFogFactor( o.positionCS.z);
            o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
            OUTPUT_LIGHTMAP_UV(v.texcoord2, unity_LightmapST, o.lightmapUV);
            OUTPUT_SH(o.normalWS.xyz, o.vertexSH);
            return o;
        }

        half4 frag (Varyings i) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(i);
            
            half3 normal;
            half2 uvOffset = half2(0,0);
            half3 viewDirWS = normalize(GetCameraPositionWS() - i.positionWS);
            //暂时不需要法线贴图 直接使用顶点的法线
            normal = i.normalWS;
            
            // 主贴图
            half4 albedo = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv.xy + uvOffset  + i.color.xy);
            half4 mainTexCol = albedo;
            half alpha = albedo.a;
            //return float4(albedo.aaa,1);
            
            clip(alpha -_Cutoff);
            // 金属度 发光度 ao
            //half4 meatTex = SAMPLE_TEXTURE2D(_MRAETex,sampler_MRAETex,i.uv2.zw+ uvOffset);
            half metallic = 0;   //叶子专用 不需要金属度 直接赋0
            half3 emission = 0;  //自发光度暂时不需要
            half ao = clamp(pow(i.color.z,_AOStrength) ,0,1);         //ao选取存在顶点色的r通道内.
            //return float4(ao,ao,ao,1);
            //return float4(ao,ao,ao,1);
            half smoothness = 0;
            // half buttonRange = albedo.r;
            // half topRange = albedo.g;
            // half transluent = albedo.b;
            
            //初始化inputData
            InputData inputdata = (InputData) 0;
            //inputdata.positionWS = i.positionWS;
            inputdata.normalWS = normal;
            inputdata.viewDirectionWS = viewDirWS;
            inputdata.shadowCoord = TransformWorldToShadowCoord(i.positionWS);
            //nputdata.fogCoord = i.fogFactorAndVertexLight.x;
            //inputdata.vertexLighting = i.fogFactorAndVertexLight.yzw;
            inputdata.bakedGI = SAMPLE_GI(i.lightmapUV, i.vertexSH, i.normalWS);
            
            float4 col = float4(0,0,0,1);//UniversalFragmentPBR(inputdata, albedo, metallic, 0,  smoothness,ao,emission,alpha);
            /////////////////////////////////////////////////////////////////////////////////////////
            //来自lighting.hlsl的pbr计算部分///////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////
            BRDFData brdfData;

            Light mainLight = GetMainLight(inputdata.shadowCoord, i.positionWS);
             ////////////////////////////////////
            float NdotL = clamp(dot(mainLight.direction, normal), 0, 1);
            
            ///修改树叶基础色的颜色
            float NdotL2 = clamp((NdotL - _LightSideColorChange) * 2, 0, 1);
            float4 lightSideColor = lerp(_LightSideColorB,_LightSideColorA,NdotL2);
            float LightMask = clamp(albedo.g + _LightMaskStrength, 0, 1);
            lightSideColor *=LightMask;
            float lightSide = clamp((dot(mainLight.direction,normal)) * (1 - (1-mainLight.shadowAttenuation)*_ShadowStrength)/* * mainLight.shadowAttenuation*/,0,1);
            float Ndotl3 = clamp((dot(mainLight.direction, normal) * 0.5 + 1 - _DarkSideColorChange),0,1);
            float4 darkSideColor = lerp(_DarkSideColor,lightSideColor,Ndotl3);
            float DarkMask = clamp(albedo.r + _DarkMaskStrength,0,1);
            //return float4(LightMask.xxx,1);
            darkSideColor *= DarkMask;
            albedo.rgb = lightSide * lightSide* lightSideColor + (1 - lightSide) * darkSideColor;


            
            //往基础色里混入ao的颜色
            albedo.rgb = lerp(_AOColor,albedo.rgb,ao);
            albedo.rgb = lerp(albedo.rgb, albedo.rgb * mainTexCol.rgb, _IfBaseColor);
            
           
            ////////////////////////////////////
            InitializeBRDFData(albedo, 0, 0, smoothness, alpha, brdfData);

            
            MixRealtimeAndBakedGI(mainLight, inputdata.normalWS, inputdata.bakedGI, half4(0, 0, 0, 0));

            half3 aoSrengthL = half3(1,1,1);
            #if defined(LIGHTMAP_ON)//如果开启lightmap, 则计算亮部ao增强和暗部ao减弱的值
            float3 bakedAOArea = 1 - smoothstep(0,0.2,(inputdata.bakedGI.r + inputdata.bakedGI.g + inputdata.bakedGI.b) * 0.333);
            inputdata.bakedGI += bakedAOArea *  inputdata.bakedGI * _LocalLMAOStrength * 20;
            LightMapAOStrengthControl(inputdata.shadowCoord, inputdata.bakedGI, aoSrengthL, inputdata.positionWS);
            //return half4(inputdata.bakedGI,1);
            #endif
            half3 color = GlobalIlluminationNoSpecular(brdfData, inputdata.bakedGI, ao , inputdata.normalWS, inputdata.viewDirectionWS)* _GIstrength;
            color += LightingPhysicallyBasedLeaf(brdfData, mainLight, inputdata.normalWS, inputdata.viewDirectionWS,float2(_DarkStrength,_ShadowStrength));
            color *= aoSrengthL;
           
            color += emission;
            col = half4(color,alpha);
            col *= clamp(mainTexCol.r + _ColorChangeControl1,0,1);
            col *= clamp(mainTexCol.g + _ColorChangeControl2,0,1);
            //混合雾
            col.rgb = MixFog(col.rgb, i.uv.z);
            return float4(col.rgb,alpha);
        }
        ENDHLSL
    }
    //ShadowCaster部分
        Pass
        {
            Name "ShadowCasterLeaf"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            //ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "./../../../ThirdParts/GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing

            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma pragma multi_compile _ALPHATEST_ON _
            #pragma multi_compile _ _ESM
            // #define _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            //#pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            //  #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            // #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShadowDepthInclude.hlsl"
            
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 texcoord     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv           : TEXCOORD0;
                float4 positionCS   : SV_POSITION;
                float4 uvOffset     : TEXCOORD1;
            };
            half4 SampleAlbedoAlpha(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
            {
                return SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv);
            }
            
            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                return positionCS;
            }
            half AlphaLeaf(half albedoAlpha, half4 color, half cutoff)
            {
                #if !defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A) && !defined(_GLOSSINESS_FROM_BASE_ALPHA)
                    half alpha = albedoAlpha * color.a;
                #else
                    half alpha = color.a;
                #endif

                clip(albedoAlpha - cutoff);
                // #if defined(_ALPHATEST_ON)
                //     clip(alpha - cutoff);
                // #endif

                return alpha;
            }
             Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                //UNITY_TRANSFER_INSTANCE_ID(input, output);

               float2 mainTexUVOffset = float2(0,0);
                LeafWindNoise(input.positionOS,input.positionOS.xyz,mainTexUVOffset);
                
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.positionCS = GetShadowPositionHClip(input);
                output.uvOffset.xy = mainTexUVOffset.xy;
                return output;
            }


            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                AlphaLeaf(SampleAlbedoAlpha(clamp(input.uv + input.uvOffset.xy,float2(0,0),float2(1,1)), TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);

                return ShadowFrag(input.positionCS);
            }
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull off

            HLSLPROGRAM
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "./../../../ThirdParts/GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing

            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex DepthOnlyVertexSGAME
            #pragma fragment DepthOnlyFragmentSGAME

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHACLIP
            //#pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing

            //风场 
            //#pragma shader_feature_local _IFWINDZONE_OFF _IFWINDZONE_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 texcoord     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv           : TEXCOORD0;
                float4 positionCS   : SV_POSITION;
                float4 uvOffset     : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            half Alpha(half albedoAlpha, half4 color, half cutoff)
            {
            #if !defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A) && !defined(_GLOSSINESS_FROM_BASE_ALPHA)
                half alpha = albedoAlpha * color.a;
            #else
                half alpha = color.a;
            #endif

            //#if defined(_ALPHACLIP)
                clip(albedoAlpha - cutoff);
            //#endif

                return alpha;
            }
            half4 SampleAlbedoAlpha(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
            {
                return half4(SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv));
            }
            
            Varyings DepthOnlyVertexSGAME(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                //UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                float3 posOS =float3(0,0,0);
                float2 mainTexUVOffset = float2(0,0);

                LeafWindNoise(input.positionOS,posOS.xyz,mainTexUVOffset);
                
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.positionCS = TransformObjectToHClip(posOS);
                output.uvOffset.xy = mainTexUVOffset.xy;
                return output;
            }

            half4 DepthOnlyFragmentSGAME(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                Alpha(SampleAlbedoAlpha(input.uv + input.uvOffset.xy, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor,  _Cutoff/*clamp(_Cutoff + 0.1,0,0.99)*/);
                return 0;
            }
            ENDHLSL
        }
        
        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}

            Cull Off

            HLSLPROGRAM
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "./../../../ThirdParts/GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing

            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex UniversalVertexMetaSGame
            #pragma fragment UniversalFragmentMetaLitSGame

            #pragma shader_feature EDITOR_VISUALIZATION
            //#pragma shader_feature_local_fragment _SPECULAR_SETUP
            //#pragma shader_feature_local_fragment _EMISSION
            //#pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            //#pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            //#pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED

            //#pragma shader_feature_local_fragment _SPECGLOSSMAP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/Shaders/LitMetaPass.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UniversalMetaPass.hlsl"

            half4 SampleAlbedoAlpha(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
            {
                return half4(SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv));
            }
            
            Varyings UniversalVertexMetaSGame(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = UnityMetaVertexPosition(input.positionOS.xyz, input.uv1, input.uv2);
                output.uv = TRANSFORM_TEX(input.uv0, _BaseMap);
            #ifdef EDITOR_VISUALIZATION
                UnityEditorVizData(input.positionOS.xyz, input.uv0, input.uv1, input.uv2, output.VizUV, output.LightCoord);
            #endif
                return output;
            }
            
            half4 UniversalFragmentMetaLitSGame(Varyings input) : SV_Target
            {
                half4 diffuse = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv.xy);
                half alpha = diffuse.a - _Cutoff;
                clip(alpha); // 这个MetaPass下的clip为什么没有抠掉？
                
                MetaInput metaInput;
                metaInput.Albedo = diffuse;
                metaInput.Emission = float3(0,0,0);
                #ifdef EDITOR_VISUALIZATION
                    metaInput.VizUV = input.VizUV;
                    metaInput.LightCoord = input.LightCoord;
                #endif
                
                return UnityMetaFragment(metaInput);
            }

            ENDHLSL
        }
}
FallBack "Hidden/InternalErrorShader"
//CustomEditor "SceneShaderLeafGUI"
}
