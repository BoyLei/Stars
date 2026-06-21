// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SGAME/FX/SGAME_VFX_Fossil"
{
    Properties
    {
        [Header(FrossilVFX)]
        _FossilTex("石化贴图",2D) = "white" {}
        [HDR]_LightColor("亮部颜色",color) = (0.8,0.8,0.8,1)
        [HDR]_DarkColor("暗部颜色",color) =  (0.3,0.3,0.3,1)
        [HDR]_BloomColor("裂痕发光颜色1",color) = (1,0.5,0.1,1)
        [HDR]_BloomColor2("裂痕发光颜色2",color) = (1,0.5,0.1,1)
        
       

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
            "Queue" = "Geometry+500"
            "RenderPipeline" = "UniversalPipeline"

        }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)

            half4 _LightColor;
            half4 _DarkColor;
            half4 _BloomColor;
            half4 _BloomColor2;
            float4 _FossilTex_ST;
            ///冰冻特效施工处  全局参数
            // half4 _FrozenColor;
            // half4 _FrozenSSSColor;
            // TEXTURE2D(_FrozenTex);    SAMPLER(sampler_FrozenTex);
            // float4 _FrozenTex_ST;
            //
            //
            // float _FrozenRimRange;
            // float _FrozenRimStrength;
            // float _FrozenConver;
            // float _FronzeConverthresold;
            // float _BaseColorDarkness;
            // float4 _BaseColorColor;
            //
            // float _FresenelRange;
            // float _FresenelSmoothness;
            // float4 _FresnelColor;
        
            float _Alpha;
            CBUFFER_END
        TEXTURE2D(_FossilTex);    SAMPLER(sampler_FossilTex);
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

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE //_MAIN_LIGHT_SHADOWS_SCREEN
            //关闭多光源
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            //#pragma multi_compile _ _ElemetalVFX  //元素效果VFX 用来支持冰冻,石化,等元素效果 目前只有冰冻
            //#pragma multi_compile _ _HEAD _HAIR       //标记该shader是用来渲染头部,头发还是身体 (默认是身体)
            //#pragma multi_compile _DISSOLVE_OFF _DISSOLVE_ON
            #pragma multi_compile _ _UVMOVE_COMMON
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
                float4 lightmapUVOrVertexSH : TEXCOORD4;
                //float4 vertexColor : TEXCOORD4;
                //float4 positionSS : TEXCOORD5;
                //float4 posPlaneWS : TEXCOORD6;
                //float4 positionOS :TEXCOORD7;
                //float4 normalVS :TEXCOORD8;
            };
            

            Varyings vert (Attributes v)
            {
                Varyings o = (Varyings)0;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv.xy = TRANSFORM_TEX(v.texcoord,_FossilTex);;
                o.uv.z = ComputeFogFactor(o.positionCS.z);    //uv的z值存储雾.
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                //o.shadowCoord = TransformWorldToShadowCoord(o.positionWS);
                //o.positionSS = ComputeScreenPos(o.positionCS);
                //o.vertexColor = v.color;
                
               // float3 originalPosVS = TransformWorldToView(TransformObjectToWorld(float3(0,0,0)+float3(0,0,0)));
                //float3 posVS = TransformWorldToView(o.positionWS);
                //posVS = float3(posVS.x, posVS.y,originalPosVS.z);
               // o.posPlaneWS = mul(UNITY_MATRIX_I_V,float4(posVS.xyz,1));
               // o.positionOS = v.positionOS;
                OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				OUTPUT_SH( o.normalWS, o.lightmapUVOrVertexSH.xyz );
               
                
                return o;
            }

            
           float3 ASEIndirectDiffuse( float2 uvStaticLightmap, float3 normalWS )
			{
			#ifdef LIGHTMAP_ON
				return SampleLightmap( uvStaticLightmap, normalWS );
			#else
				return SampleSH(normalWS);
			#endif
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

                half NoL = dot(N,L)*0.5+0.5;
                half4 finalCol = half4(0,0,0,1);

                half4 fossilTex = SAMPLE_TEXTURE2D(_FossilTex, sampler_FossilTex, i.uv.xy);
                

               // return float4(fossilTex.x,fossilTex.x,fossilTex.x,1);
                half3 fossilColor = lerp(_DarkColor,_LightColor,step(0.6,NoL)) * saturate(fossilTex.x + 0.2f);
                //裂隙区域
                half fossilArea = saturate(fossilTex.y*2);
                fossilColor = lerp(fossilColor,_BloomColor,fossilArea);

                //裂隙装饰区域
                fossilArea = saturate(fossilTex.z*2);
                fossilColor = lerp(fossilColor,_BloomColor2,fossilArea);

                //增加环境光
                float3 globalLight = ASEIndirectDiffuse(i.lightmapUVOrVertexSH.xy,N);
                fossilColor += globalLight * 0.2;

                
                return float4(fossilColor.rgb,1);
                
                half4 tempCol = half4(1,1,1,1);
                half VoN=dot(V,N);

                
                // half rimlight = saturate(_FrozenRimStrength * step(_FrozenRimRange,pow(1 - VoN,3)) * pow(1 - VoN,4));
                // rimlight = saturate(rimlight + clamp(0, _FronzeConverthresold,1 * pow(1 - VoN,_FrozenConver)));
                // half2 frozenTexUV = TRANSFORM_TEX(i.uv.xy,_FrozenTex);
                // half4 frozenTex = SAMPLE_TEXTURE2D(_FrozenTex, sampler_FrozenTex, frozenTexUV);
                // half3 frozenRange = saturate(frozenTex + 0.1) * 4 * rimlight;
                // tempCol = half4(frozenRange,1);
                // tempCol = (1-tempCol.r) *0.2 * (1 - VoN)  + tempCol;
                //
                // //Fresnel
                // half Fresnel = smoothstep(0.1,0.1 + _FresenelSmoothness/10,pow(1 - VoN,_FresenelRange*5));
                // Fresnel =  smoothstep(0.1,0.1 + _FresenelSmoothness,pow(clamp(1 - VoN,0.01,0.99),_FresenelRange*5));
                // half3 FresnelColor = _FresnelColor.rgb * Fresnel * clamp(frozenTex.r * 5,0,1);
                // //return float4(FresnelColor,1);
                //
                // half4 frozenCol =  lerp(_FrozenSSSColor,_FrozenColor,saturate(pow(1 - VoN,2)*2));
                // frozenCol = frozenCol * (dot(N,L)*mainLight.shadowAttenuation * 0.5 + 0.5);
                // frozenCol.rgb += FresnelColor * _FresnelColor.a;
                // half4 rimColor_mainLight = float4(0.1,0.1,0.1,1);
                //
                // //冰壳内部角色颜色
                // float4 insideColor = float4(1,1,1,1);
                // insideColor.rgb = _BaseColorColor.rgb * _BaseColorDarkness;
                // insideColor.a = _BaseColorColor.a;
                // finalCol.rgb = lerp(insideColor,frozenCol,saturate(tempCol + frozenRange)) ;//(frozenCol * tempCol * 2).rgb;
                //
                //     finalCol.a =saturate(tempCol + insideColor.a ) ;
                //     //return float4(finalCol);
                //     //finalCol = lerp((diffuseAndShadow  + specular + rimColor_mainLight) * _BaseColorDarkness * _BaseColorColor,frozenCol,tempCol);
                // //#endif
                //

                
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
