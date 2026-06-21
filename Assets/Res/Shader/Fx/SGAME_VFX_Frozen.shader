// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SGAME/FX/SGAME_VFX_Frozen"
{
    Properties
    {
        [Header(FrozenVFX)]
        [HDR]_FrozenColor("冰壳颜色",Color) = (0.21,0.3,0.749,1)
        [HDR]_FrozenSSSColor("冰壳次表面散射颜色",Color) = (0.27,0.8,1.129,1)
        _FrozenTex("冰冻基础颜色贴图",2D) = "white" {}
        _FrozenRimRange("冰冻边缘范围",Range(0,1)) = 0.278
        _FrozenRimStrength("冰冻边缘强度",Range(0,5)) = 5
        _FrozenConver("冰壳厚度",Range(0,3)) = 1
        _FronzeConverthresold("冰冻效果亮度阈值",Range(0,1)) = 0.216
        _BaseColorDarkness("冰壳内角色暗度",Range(0,1)) = 0.674
        _BaseColorColor("冰壳内角色颜色倾向",Color) = (0.0041,0.1,0.291,0.8862)
        _OutlineColorControl("描边的颜色明暗调整",Range(0,1)) = 0.239
        [Header(Fresnel)]
        _FresenelRange("菲涅尔范围",Range(0,2)) = 0.42
        _FresenelSmoothness("菲涅尔柔和度",Range(0,1)) = 0.862
        [HDR]_FresnelColor("菲尼尔颜色",Color) = (0.389,0.8018,0.6921,1)

        [Header(Alhpa)]
        _Alpha("角色Alpha值",Range(0,1)) = 0.807
        
       // [Header(Zwrite)]
        //[Enum(Off, 0, On, 1)]_ZwriteOp_Char("深度写入开关",int) = 1
        
       //[Header(RenderFrontBack)]
        //[Enum(UnityEngine.Rendering.CullMode)]_CullOp_Char("剔除面,Off-不剔除,Back-剔除背面,Front-剔除正面",float) = 0
        
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue" = "Geometry+15"
            "RenderPipeline" = "UniversalPipeline"

        }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            ///冰冻特效施工处  全局参数
            half4 _FrozenColor;
            half4 _FrozenSSSColor;
            TEXTURE2D(_FrozenTex);    SAMPLER(sampler_FrozenTex);
            float4 _FrozenTex_ST;
            
            
            float _FrozenRimRange;
            float _FrozenRimStrength;
            float _FrozenConver;
            float _FronzeConverthresold;
            float _BaseColorDarkness;
            float4 _BaseColorColor;

            float _FresenelRange;
            float _FresenelSmoothness;
            float4 _FresnelColor;
        
            float _Alpha;
            CBUFFER_END
        ENDHLSL
        Pass
        {
            Name "CharLighting"
            Tags { "LightMode" = "UniversalForward" }
            Cull [_CullOp_Char]
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite on
            ZTest on
            
            Stencil
            {
                Ref [_OutlineStencilComp]
                Comp [_StencilComp]
                Pass [_StencilOp]
            }
//            Stencil
//            {
//                Ref 114
//                Comp Equal
//                Pass [_StencilOp]
//            }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_instancing

            #pragma multi_compile _ /*_MAIN_LIGHT_SHADOWS*/ _MAIN_LIGHT_SHADOWS_CASCADE //_MAIN_LIGHT_SHADOWS_SCREEN
            //关闭多光源
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            //#pragma multi_compile _ _ElemetalVFX  //元素效果VFX 用来支持冰冻,石化,等元素效果 目前只有冰冻
            //#pragma multi_compile _ _HEAD _HAIR       //标记该shader是用来渲染头部,头发还是身体 (默认是身体)
            #pragma multi_compile _DISSOLVE_ON
           // #pragma multi_compile _ _UVMOVE_COMMON
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Macros.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
               // float2 texcoord2: TEXCOORD1;
                //float2 texcoord3 : TEXCOORD2;
                float3 normalOS : NORMAL;
                //float4 tangent : TANGENT;
               // float4 color : COLOR;
            };

            struct Varyings
            {
                float4 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                //float4 vertexColor : TEXCOORD4;
                //float4 positionSS : TEXCOORD5;
                //float4 posPlaneWS : TEXCOORD6;
                //float4 positionOS :TEXCOORD7;
                float4 normalVS :TEXCOORD8;
            };
            

            Varyings vert (Attributes v)
            {
                Varyings o = (Varyings)0;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv.xy = v.texcoord;
                o.uv.z = ComputeFogFactor(o.positionCS.z);    //uv的z值存储雾.
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.shadowCoord = TransformWorldToShadowCoord(o.positionWS);
                //o.positionSS = ComputeScreenPos(o.positionCS);
                //o.vertexColor = v.color;
                
                float3 originalPosVS = TransformWorldToView(TransformObjectToWorld(float3(0,0,0)+float3(0,0,0)));
                float3 posVS = TransformWorldToView(o.positionWS);
                posVS = float3(posVS.x, posVS.y,originalPosVS.z);
               // o.posPlaneWS = mul(UNITY_MATRIX_I_V,float4(posVS.xyz,1));
               // o.positionOS = v.positionOS;
               
                
                return o;
            }

            
            half4 frag (Varyings i) : SV_Target
            {
                Light mainLight = GetMainLight(i.shadowCoord);
                mainLight.color = mainLight.color /** _DirectionLightControler*/;
                // //测试代码
                // //return mainLight.shadowAttenuation;
                float3 N = normalize(i.normalWS);
                float3 L = normalize(mainLight.direction);
                float3 V = normalize(_WorldSpaceCameraPos - i.positionWS);

                half4 finalCol = half4(0,0,0,1);
                
                half4 tempCol = half4(1,1,1,1);
                half VoN=dot(V,N);

                
                half rimlight = saturate(_FrozenRimStrength * step(_FrozenRimRange,pow(1 - VoN,3)) * pow(1 - VoN,4));
                rimlight = saturate(rimlight + clamp(0, _FronzeConverthresold,1 * pow(1 - VoN,_FrozenConver)));
                half2 frozenTexUV = TRANSFORM_TEX(i.uv.xy,_FrozenTex);
                half4 frozenTex = SAMPLE_TEXTURE2D(_FrozenTex, sampler_FrozenTex, frozenTexUV);
                half3 frozenRange = saturate(frozenTex + 0.1) * 4 * rimlight;
                tempCol = half4(frozenRange,1);
                tempCol = (1-tempCol.r) *0.2 * (1 - VoN)  + tempCol;

                //Fresnel
                half Fresnel = smoothstep(0.1,0.1 + _FresenelSmoothness/10,pow(1 - VoN,_FresenelRange*5));
                Fresnel =  smoothstep(0.1,0.1 + _FresenelSmoothness,pow(clamp(1 - VoN,0.01,0.99),_FresenelRange*5));
                half3 FresnelColor = _FresnelColor.rgb * Fresnel * clamp(frozenTex.r * 5,0,1);
                //return float4(FresnelColor,1);
            
                half4 frozenCol =  lerp(_FrozenSSSColor,_FrozenColor,saturate(pow(1 - VoN,2)*2));
                frozenCol = frozenCol * (dot(N,L)*mainLight.shadowAttenuation * 0.5 + 0.5);
                frozenCol.rgb += FresnelColor * _FresnelColor.a;
                half4 rimColor_mainLight = float4(0.1,0.1,0.1,1);

                //冰壳内部角色颜色
                float4 insideColor = float4(1,1,1,1);
                insideColor.rgb = _BaseColorColor.rgb * _BaseColorDarkness;
                insideColor.a = _BaseColorColor.a;
                finalCol.rgb = lerp(insideColor,frozenCol,saturate(tempCol + frozenRange)) ;//(frozenCol * tempCol * 2).rgb;
            
                    finalCol.a =saturate(tempCol + insideColor.a ) ;
                    //return float4(finalCol);
                    //finalCol = lerp((diffuseAndShadow  + specular + rimColor_mainLight) * _BaseColorDarkness * _BaseColorColor,frozenCol,tempCol);
                //#endif
                

                
                //混合雾
                finalCol.rgb = MixFog(finalCol.rgb, i.uv.z);
                //混合顶点色的透明度
                //finalCol.a = lerp(1,_VertexAlpha  + (VertexAlphaSpeuclar.r+VertexAlphaSpeuclar.g+VertexAlphaSpeuclar.b) * 0.3333 * _SpecularToAlpha,i.vertexColor.g * _IfVertexAlpha);
                //设置角色Alpha值(如有需要)
                finalCol.a *=_Alpha;
                return finalCol;
            }
            ENDHLSL
        }

//        Pass
//        {
//            Name "OutLine"
//            Tags { "LightMode" = "SRPDefaultUnlit" }
//            Cull Front
//            Blend SrcAlpha OneMinusSrcAlpha
//            ZTest on
//            ZWrite on  
//           // BlendOp Add,Add
//            //Blend One Zero,One Zero
////            Stencil
////            {
////                Ref 0
////                Comp [_OutLineStencilComp]
////                Pass [_StencilOp]
////            }
//            Stencil
//            {
//                Ref [_OutlineStencilComp]
//                Comp [_StencilComp]
//                Pass [_StencilOp]
//            }
//            
//            
//            HLSLPROGRAM
//            #pragma vertex vert 
//            #pragma fragment frag
//
//            #pragma prefer_hlslcc gles
//            #pragma exclude_renderers d3d11_9x
//            #pragma target 2.0
//            #pragma multi_compile_instancing
//            //关闭多光源
//            //#pragma multi_compile _ _ADDITIONAL_LIGHTS
//            #pragma multi_compile _ _ElemetalVFX
//            #pragma multi_compile _ _HEAD         //标记该shader是否用来渲染头部
//            #pragma multi_compile _ _DISSOLVE_ON
//            
//            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
//            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//
//            struct Attributes
//            {
//                float4 positionOS : POSITION;
//                float4 texcoord : TEXCOORD0;
//                float2 texcoord2: TEXCOORD1;
//                //float2 texcoord3 : TEXCOORD2;
//                float4 tangentOS :  TANGENT;
//                float3 normalOS : NORMAL;
//                half4 vertColor : COLOR;
//            };
//
//            struct Varyings
//            {
//                float2 uv : TEXCOORD0;
//                float4 positionCS : SV_POSITION;
//                float4 positionWS :TEXCOORD1;
//                float4 normalWS   :TEXCOORD2;
//                float4 color : TEXCOORD3;
//                float4 positionOS : TEXCOORD4;
//                
//            };
//            
//            
//
//            Varyings vert (Attributes v)
//            {
//                Varyings o = (Varyings)0;
//                
//                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
//                float ViewDistance = length(_WorldSpaceCameraPos - positionWS);
//
//                float width01 = _OutLineWidth * 0.003 * 1 *  v.vertColor.r;
//                float width02 = width01 * ViewDistance;
//                float width = lerp(width01, width02, _CameraOutlineRange);
//
//                #ifdef _DISSOLVE_ON
//                width = 0;
//                #endif
//                o.normalWS.xyz = TransformObjectToWorldDir(v.normalOS);
//                o.positionWS.xyz = positionWS;
//                half3 V = normalize(_WorldSpaceCameraPos - positionWS);
//
//                
//                
//                //读取Color中的平滑法线(切线空间) 转换到物体空间下
//                float crossSign = (v.tangentOS.a > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
//                v.tangentOS.xyz = 1.0 / length(v.tangentOS.xyz ) * normalize(v.tangentOS.xyz); 
//                float3 BioTangent =1.0 / length(v.normalOS.xyz )* cross(v.normalOS.xyz,v.tangentOS.xyz)* crossSign  /** v.tangentOS.w*/;
//                float2 normalXY = v.texcoord2.xy;
//                float3 normalZ = cross(float3(0,v.texcoord2.y,0),float3(v.texcoord2.x,0,0));
//                float3 BakeNormal = normalize(UnpackNormal(float4(normalXY.x,normalXY.y,1,1)));
//                float3x3 tangentTransform = float3x3(v.tangentOS.xyz, BioTangent.xyz, v.normalOS.xyz);
//                float3 SmoothNormalOS = float3(v.tangentOS.x * BakeNormal.x+ BioTangent.x * BakeNormal.y + v.normalOS.x * BakeNormal.z,
//                                                v.tangentOS.y * BakeNormal.x + BioTangent.y  * BakeNormal.y + v.normalOS.y * BakeNormal.z,
//                                                v.tangentOS.z * BakeNormal.x + BioTangent.z  * BakeNormal.y + v.normalOS.z * BakeNormal.z);
//                
//                #ifdef _HEAD
//                    float3 positionOS = v.positionOS.xyz + normalize(SmoothNormalOS.xyz) * width ;
//                #else
//                    float3 positionOS = v.positionOS.xyz + normalize(SmoothNormalOS.xyz) * width;
//                #endif
//                
//                o.positionCS = TransformObjectToHClip(positionOS.xyz);
//                o.positionOS = v.positionOS;
//                o.uv = v.texcoord.xy;
//                o.color = v.vertColor;
//                o.color.rgb = BakeNormal;
//                
//                return o;
//            }
//
//            half4 frag (Varyings i) : SV_Target
//            {
//               // return float4(i.color.rgb,1);
//                half4 diffuse = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
//                diffuse = diffuse * _OutlineColor;
//                
//                Light light = GetMainLight();
//                float NdotL = dot(-i.normalWS,light.direction);
//                half4 outLineColor = diffuse * saturate(NdotL * 0.5 + 0.6);
//                //return  float4(i.color.xyz,1);
//                 //计算冰冻特效
//                #ifdef _ElemetalVFX
//                    ///冰霜特效
//                    float3 V = normalize(_WorldSpaceCameraPos - i.positionWS);
//                    float3 N = i.normalWS;
//                    float VoN = dot(N,V);
//                    outLineColor =clamp(0,_FronzeConverthresold, _FrozenRimStrength*_FrozenColor * pow(1-VoN,2) * 0.3);
//                    half rimlight = saturate(_FrozenRimStrength * step(_FrozenRimRange,pow(1 - VoN,3)) * pow(1 - VoN,4));
//                    rimlight = saturate(rimlight + clamp(0, _FronzeConverthresold,1 * pow(1 - VoN,_FrozenConver)));
//                    outLineColor = clamp(0,_FronzeConverthresold,rimlight * _OutlineColorControl);
//                #endif
//
//                
//                // 额外灯（关闭）
//                /*
//                #ifdef _ADDITIONAL_LIGHTS
//                    // 像素灯的数量
//                    uint pixelLightCount = GetAdditionalLightsCount();
//                    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
//                    {
//                        // 获得灯光颜色、方向、衰减、阴影衰减
//                        Light addLight = GetAdditionalLight(lightIndex, i.positionWS);
//
//                        half3 normalPlaneWS = normalize(mul(UNITY_MATRIX_I_V,float3(0,0,1)));
//                        half NoLadd = saturate(dot(addLight.direction,normalPlaneWS));
//                        half3 diffuseAdd = NoLadd * addLight.distanceAttenuation * diffuse.rgb * addLight.color * 0.5;
//
//                        outLineColor.rgb += diffuseAdd;
//                    }
//                #endif
//                */
//
//                //输出Alpha为0
//                outLineColor.a =1;
//                outLineColor.a *= clamp(1 * _OutlineAlpha,0,1);
//                //outLineColor.a = 0;
//
//#ifdef _DISSOLVE_ON
//                // i.uv = i.uv.xy * _DissolveTex_ST.xy + _DissolveTex_ST.zw;
//                // float4 dissolveCol = tex2D(_DissolveTex, i.uv);
//                // outLineColor.rgb += (_DissolveLightingColor);
//               // return float4(0,0,0,0);
//                clip(-1);
//#endif
//
//                return outLineColor;
//            }
//            ENDHLSL
//        }

//        pass
//        {
//            Tags { "LightMode" = "ShadowCaster" }
//            Cull off
//            
//            HLSLPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//
//           
//            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
//            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
//
//            struct Attributes
//            {
//                float4 positionOS : POSITION;
//                float2 texcoord : TEXCOORD0;
//                float3 normalOS : NORMAL;
//            };
//
//            struct Varyings
//            {
//                float2 uv : TEXCOORD0;
//                float4 positionCS : SV_POSITION;
//            };
//
//            #pragma multi_compile _ _DISSOLVE_ON
//            CBUFFER_START(UnityPerMaterial)
//
//            CBUFFER_END
//
//            float4 GetShadowPositionHClip1(Attributes input)
//            {
//                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
//                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
//                Light l = GetMainLight();
//                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, l.direction));
//
//            #if UNITY_REVERSED_Z
//                positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
//            #else
//                positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
//            #endif
//
//                return positionCS;
//            }
//
//            Varyings vert(Attributes v)
//            {
//                Varyings o = (Varyings)0;
//
//                o.uv = v.texcoord.xy;
//                o.positionCS = GetShadowPositionHClip1(v);
//
//                return o;
//            }
//
//            half4 frag(Varyings i) : SV_TARGET
//            {
//                #ifdef _DISSOLVE_ON
//                float2 dissolveUV = i.uv * (_DissolveTex_ST.xy) + _DissolveTex_ST.zw;
//                float4 dissolveCol = tex2D(_DissolveTex, dissolveUV);
//                //float clipDissolve = _Clip_Char * (1 + 2 * _RampRange) - _RampRange;
//                clip(dissolveCol.r - (_Clip_Char));
//                #endif
//                return 0;
//            }
//
//            ENDHLSL
//        }
//        Pass
//        {
//            Name "DepthOnly"
//            Tags{"LightMode" = "DepthOnly"}
//
//            ZWrite On
//            ColorMask 0
//            Cull back
//            
//            HLSLPROGRAM
//            // Required to compile gles 2.0 with standard srp library
//            #pragma prefer_hlslcc gles
//            #pragma exclude_renderers d3d11_9x
//            #pragma target 2.0
//
//            #pragma vertex DepthOnlyVertexSGAME
//            #pragma fragment DepthOnlyFragmentSGAME
//
//            // -------------------------------------
//            // Material Keywords
//            #pragma shader_feature _ALPHATEST_ON
//            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
//
//            //--------------------------------------
//            // GPU Instancing
//            #pragma multi_compile_instancing
//
//            #pragma multi_compile _ _DISSOLVE_ON
//
//            //#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
//            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//            //#include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
//            struct Attributes
//            {
//                float4 position     : POSITION;
//                float2 texcoord     : TEXCOORD0;
//                UNITY_VERTEX_INPUT_INSTANCE_ID
//            };
//
//            struct Varyings
//            {
//                float2 uv           : TEXCOORD0;
//                float4 positionCS   : SV_POSITION;
//                UNITY_VERTEX_INPUT_INSTANCE_ID
//                UNITY_VERTEX_OUTPUT_STEREO
//            };
//            
//            half Alpha(half albedoAlpha, half4 color, half cutoff)
//            {
//            #if !defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A) && !defined(_GLOSSINESS_FROM_BASE_ALPHA)
//                half alpha = albedoAlpha * color.a;
//            #else
//                half alpha = color.a;
//            #endif
//
//            #if defined(_ALPHATEST_ON)
//                clip(alpha - cutoff);
//            #endif
//
//                return alpha;
//            }
//            half4 SampleAlbedoAlpha(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
//            {
//                return half4(SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv));
//            }
//            
//            Varyings DepthOnlyVertexSGAME(Attributes input)
//            {
//                Varyings output = (Varyings)0;
//                UNITY_SETUP_INSTANCE_ID(input);
//                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
//
//                output.uv = input.texcoord;//TRANSFORM_TEX(, _MainTex);
//                output.positionCS = TransformObjectToHClip(input.position.xyz);
//                return output;
//            }
//
//            half4 DepthOnlyFragmentSGAME(Varyings input) : SV_TARGET
//            {
//                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
//
//                Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_MainTex, sampler_MainTex)).a, float4(1,1,1,1), _Clip_Char);
//
//                #ifdef _DISSOLVE_ON
//                float2 dissolveUV = input.positionCS.xy * (_DissolveTex_ST.xy*0.001) + _DissolveTex_ST.zw;
//                float4 dissolveCol = tex2D(_DissolveTex, dissolveUV);
//                float clipDissolve = _Clip_Char * (1 + 2 * _RampRange) - _RampRange;
//                clip(dissolveCol.r - (_Clip_Char)-1);
//                #endif
//                return 0;
//            }
//            ENDHLSL
//        }
        
    }
    FallBack "Hidden/InternalErrorShader"
    //CustomEditor "CartoonCharacterShaderGUI"
}
