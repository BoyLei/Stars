Shader "SGAME/Item/Crystal"
{
    Properties
    {
       _MainTex_Crystal ("主贴图", 2D) = "white" {}
       [HDR]_Color_Crystal("Color", Color) = (1,1,1,1)
       _NormalTex_Crystal("法线贴图",2D) = "bump" {}
       _MSAETex_Crystal("自发光贴图",2D) = "gray"{}
        [HDR]_EmissionColor_Crystal("自发光颜色",color)= (0.7,0.5,0.7,1)
       //[HDR]_Color_Crystal("自发光颜色",color) = (0.5,0.5,0.5,1)
       _RefractTex_Crystal("折射贴图",Cube) = "black" {}
       _InsideRefractionStrength("内部折射强度",range(0,1)) = 1
       _RefractionStrength("外部折射强度",range(0,1)) = 1
       _ReflectTex_Crystal("反射贴图",Cube) = "black" {}
       _ReflectionStrength("反射强度",range(0,1)) = 1
       [HDR]_FresnelColor_Crystal("菲尼尔边缘光颜色",color)= (0.7,0.5,0.7,1)
       _FresnelSmoothness("菲涅尔柔和度",range(0,0.1)) = 0.05
       _FresnelRange("菲涅尔范围",range(0,10)) = 1
       
       _TotalAlpha("整体不透明度",range(0,1)) = 0.1
       
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        //法线强化函数
        void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
        {
            Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
        }

            CBUFFER_START(UnityPerMaterial)
            half4 _Color_Crystal;
            half4 _FresnelColor_Crystal;
            half4 _EmissionColor_Crystal;
            half _InsideRefractionStrength;
            half _RefractionStrength;
            half _ReflectionStrength;
            half _FresnelSmoothness;
            half _FresnelRange;
            half _TotalAlpha;
            half4 _DissolveTex_ST;
            CBUFFER_END

            TEXTURE2D(_MainTex_Crystal);
            SAMPLER(sampler_MainTex_Crystal);
            TEXTURE2D(_NormalTex_Crystal);
            SAMPLER(sampler_NormalTex_Crystal);
            TEXTURE2D(_MSAETex_Crystal);
            SAMPLER(sampler_MSAETex_Crystal);
            //TEXTURE2D(_RefractTex_Crystal);
            samplerCUBE _RefractTex_Crystal;
            samplerCUBE _ReflectTex_Crystal;
        ENDHLSL
        Pass
        {
            Name "Crystal Back"
            Tags
            {
                "LightMode"="SRPDefaultUnlit"
            }

            cull front
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            

            struct a2v
            {
                float4 posOS : POSITION;
                float2 uv : TEXCOORD0;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 posCS : SV_POSITION;
                float4 posWS : TEXCOORD1;
				float4 tangentWS : TEXCOORD2;
				float4 normalWS : TEXCOORD3;
				float4 bioTangentWS : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
            };
            
            
            v2f vert(a2v v)
            {
                v2f o;

                VertexPositionInputs posInput = GetVertexPositionInputs(v.posOS.xyz);

                float3 worldTangent = TransformObjectToWorldDir(v.tangent);
				o.tangentWS.xyz = worldTangent;
				float3 worldNormal = TransformObjectToWorldDir(v.normal);
				o.normalWS.xyz = worldNormal;
				float vertexTangentSign = v.tangent.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 worldBitangent = cross( worldNormal, worldTangent ) * vertexTangentSign;
				o.bioTangentWS.xyz = worldBitangent;

                o.posWS.xyz = TransformObjectToWorld(v.posOS.xyz);
                o.posCS = posInput.positionCS;
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float3 vertexNormalWS = i.normalWS;
                float3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalTex_Crystal,sampler_NormalTex_Crystal,i.uv));
                float3 normalWithTexWS = TransformTangentToWorld(normalTS.xyz,half3x3( i.tangentWS.xyz,i.bioTangentWS.xyz,i.normalWS.xyz));

                float3 worldViewDir = GetWorldSpaceViewDir(i.posWS);
				worldViewDir = normalize(worldViewDir);
                float3 reflectionWS = reflect(-worldViewDir,vertexNormalWS);

                half4 refractionTex = texCUBE(_RefractTex_Crystal,reflectionWS);
                half4 reflectionTex = texCUBE(_ReflectTex_Crystal,reflectionWS);
                half4 maincolor = SAMPLE_TEXTURE2D(_MainTex_Crystal,sampler_MainTex_Crystal,i.uv) * _Color_Crystal;

                half3 environmentColor = refractionTex.rgb * _InsideRefractionStrength/*+ reflectionTex.rgb * 0.3 * _ReflectionStrength */+ maincolor * 0.1;
                
                float4 col = float4(0,0,0,1);
                col = float4(environmentColor.xyz,1);
                return col;
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "Crystal Front"
            Tags
            {
                "LightMode"="UniversalForward"
            }

            cull Back
            Blend SrcAlpha OneMinusSrcAlpha
            //Blend zero one
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

           // #pragma multi_compile _RECEIVE_SHADOWS_ON _RECEIVE_SHADOWS_OFF
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            

            struct a2v
            {
                float4 posOS : POSITION;
                float2 uv : TEXCOORD0;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 posCS : SV_POSITION;
                float4 posWS : TEXCOORD1;
				float4 tangentWS : TEXCOORD2;
				float4 normalWS : TEXCOORD3;
				float4 bioTangentWS : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
            };
            
            
            
            v2f vert(a2v v)
            {
                v2f o;

               // VertexPositionInputs posInput = GetVertexPositionInputs(v.posOS.xyz);

                float3 worldTangent = TransformObjectToWorldDir(v.tangent);
				o.tangentWS.xyz = worldTangent;
				float3 worldNormal = TransformObjectToWorldDir(v.normal);
				o.normalWS.xyz = worldNormal;
				float vertexTangentSign = v.tangent.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 worldBitangent = cross( worldNormal, worldTangent ) * vertexTangentSign;
				o.bioTangentWS.xyz = worldBitangent;

               
                o.posWS.xyz = TransformObjectToWorld(v.posOS);
                o.posCS = TransformObjectToHClip(v.posOS.xyz);
                o.uv.z = ComputeFogFactor(o.posCS.z);
                o.uv.xy = v.uv.xy;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float3 vertexNormalWS = i.normalWS;
                float3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalTex_Crystal,sampler_NormalTex_Crystal,i.uv));
                float3 normalWithTexWS = TransformTangentToWorld(normalTS.xyz,half3x3( i.tangentWS.xyz,i.bioTangentWS.xyz,i.normalWS.xyz));

                float3 worldViewDir = GetWorldSpaceViewDir(i.posWS);
				worldViewDir = normalize(worldViewDir);
                float3 reflectionWS = reflect(-worldViewDir,normalWithTexWS);
                float3 reflectionWSNoTex = reflect(-worldViewDir,vertexNormalWS);
                float voN = dot(worldViewDir,normalWithTexWS);
                //获取mainLight
                float4 shadowCoord = TransformWorldToShadowCoord(i.posWS);
                Light mainLight = GetMainLight(shadowCoord, i.posWS, half4(1,1,1,1));
               // return float4(mainLight.shadowAttenuation,mainLight.shadowAttenuation,mainLight.shadowAttenuation,1);
                float nDotl = dot(normalWithTexWS,mainLight.direction) * mainLight.shadowAttenuation;

                half4 refractionTex = texCUBE(_RefractTex_Crystal,reflectionWSNoTex);
                half4 reflectionTex = texCUBE(_ReflectTex_Crystal,reflectionWS);
                half4 maincolor = SAMPLE_TEXTURE2D(_MainTex_Crystal,sampler_MainTex_Crystal,i.uv) * _Color_Crystal;

                // //简单光照计算
                // maincolor.rgb = maincolor.rgb * (nDotl*0.5 + 0.5) * mainLight.color.rgb * 0.5;
                // half3 environmentColor = refractionTex.rgb * _RefractionStrength + reflectionTex.rgb * 0.3 * _ReflectionStrength;
                // //return float4(environmentColor,1);
                //
                // float4 col = float4(0,0,0,1);
                

                //Fresnel
                float normalizedVoN = saturate(1 - voN);
                float Fresnel = pow(normalizedVoN,_FresnelRange);
                Fresnel = smoothstep(0.1,0.1 + _FresnelSmoothness,Fresnel) * Fresnel;
                //Fresnel = smoothstep(0.1,0.12,Fresnel);
                float3 FresnelColor =_FresnelColor_Crystal.rgb *Fresnel * _FresnelColor_Crystal.a * (nDotl*0.7 + 0.7); /** reflectionTex.rgb*/;
                //return float4(FresnelColor.rgb,1);

                //Emission
                half4 MSAETex = SAMPLE_TEXTURE2D(_MSAETex_Crystal,sampler_MSAETex_Crystal,i.uv);
                half3 emissionColor = MSAETex.r * _EmissionColor_Crystal.rgb * _EmissionColor_Crystal.a;
                //return float4(MSAETex.r,MSAETex.r,MSAETex.r,1);

                 //简单光照计算
                maincolor.rgb = maincolor.rgb * (nDotl*0.5 + 0.5) * mainLight.color.rgb * 0.5;
                half3 environmentColor = refractionTex.rgb * _RefractionStrength *(1 - MSAETex.r) + reflectionTex.rgb * 0.3 * _ReflectionStrength;
                //return float4(environmentColor,1);
                
                float4 col = float4(0,0,0,1);
                
                float alpha = pow(normalizedVoN,0.9);
                col.rgb = environmentColor  +  maincolor + FresnelColor + emissionColor ;
                //混合雾
                col.rgb = MixFog(col.rgb, i.posCS.z);
                //col.rgb = lerp(col.rgb,emissionColor,MSAETex.r);
                //col = float4(col.rgb,1);
                col.a =clamp(alpha + Fresnel + MSAETex.r + _TotalAlpha,0,1)  ;
                //col = float4(col.a,col.a,col.a ,1);
                return col;
            }
            ENDHLSL
        }
        
        Pass
        {
            Tags { "LightMode" = "ShadowCaster" }
            Cull off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

           
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            #pragma multi_compile _ _DISSOLVE_ON
            CBUFFER_START(UnityPerMaterial)

            CBUFFER_END

            float4 GetShadowPositionHClip1(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                Light l = GetMainLight();
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, l.direction));

            #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
            #else
                positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
            #endif

                return positionCS;
            }

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;

                o.uv = v.texcoord.xy;
                o.positionCS = GetShadowPositionHClip1(v);

                return o;
            }

            half4 frag(Varyings i) : SV_TARGET
            {
                #ifdef _DISSOLVE_ON
                float2 dissolveUV = i.uv * (_DissolveTex_ST.xy) + _DissolveTex_ST.zw;
                float4 dissolveCol = tex2D(_DissolveTex, dissolveUV);
                //float clipDissolve = _Clip_Char * (1 + 2 * _RampRange) - _RampRange;
                clip(dissolveCol.r - (_Clip_Char));
                #endif
                return 0;
            }

            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull back
            
            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex DepthOnlyVertexSGAME
            #pragma fragment DepthOnlyFragmentSGAME

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma multi_compile _ _DISSOLVE_ON

            //#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
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
            
            half Alpha(half albedoAlpha, half4 color, half cutoff)
            {
            #if !defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A) && !defined(_GLOSSINESS_FROM_BASE_ALPHA)
                half alpha = albedoAlpha * color.a;
            #else
                half alpha = color.a;
            #endif

            #if defined(_ALPHATEST_ON)
                clip(alpha - cutoff);
            #endif

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
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.uv = input.texcoord;//TRANSFORM_TEX(, _MainTex);
                output.positionCS = TransformObjectToHClip(input.position.xyz);
                return output;
            }

            half4 DepthOnlyFragmentSGAME(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                //Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_MainTex, sampler_MainTex)).a, float4(1,1,1,1), _Clip_Char);

                #ifdef _DISSOLVE_ON
                float2 dissolveUV = input.positionCS.xy * (_DissolveTex_ST.xy*0.001) + _DissolveTex_ST.zw;
                float4 dissolveCol = tex2D(_DissolveTex, dissolveUV);
                float clipDissolve = _Clip_Char * (1 + 2 * _RampRange) - _RampRange;
                clip(dissolveCol.r - (_Clip_Char)-1);
                #endif
                return 0;
            }
            ENDHLSL
        }
        
    }

    FallBack Off
}
