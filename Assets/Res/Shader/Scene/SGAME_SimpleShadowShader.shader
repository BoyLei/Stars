Shader "SGAME/SimpleShadowShader"
{
    Properties
    {
    	/*[HideInInspector]*/[MainTexture]_MainTex ("主贴图(A为透明通道)", 2D) = "gray" {}
        _RenderSelf("Render Self", Int) = 1
    	[HideInInspector]_LightDirection("",Vector) = (1,1,1,1)
    	[HideInInspector]_LightPosition("",Vector) = (1,1,1,1)
    	[HideInInspector]_AlphaClipThreshold("",float) = 1
    }
    SubShader
    {
        Tags 
        {
            "RenderPipeline" = "UniversalPipeline"
			   "Queue" = "Geometry+35"
        }
        LOD 100

		HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		CBUFFER_START(UnityPerMaterial)
		float4 MainTex_ST;
		CBUFFER_END
		
		TEXTURE2D(_MainTex);
		SAMPLER(sampler_MainTex);
		ENDHLSL
        Pass
        {
            Tags
            {
                "LightMode"="UniversalForward"
            }

            HLSLPROGRAM
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "./../../../ThirdParts/GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"



            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


            struct a2v
            {
                float4 posOS : POSITION;
                float2 uv : TEXCOORD0;
            	float3 normalOS : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 posCS : SV_POSITION;
            	float4 normalWS : TEXCOORD1;
            };
            
            CBUFFER_START(UnityPerMaterial)
			float3 _LightDirection;
			float3 _LightPosition;
			float _AlphaClipThreshold;
			CBUFFER_END
            

            v2f vert(a2v v)
            {
                v2f o;

                VertexPositionInputs posInput = GetVertexPositionInputs(v.posOS.xyz);
                o.posCS = posInput.positionCS;
                o.uv = v.uv;
				o.normalWS.xyz = TransformObjectToWorldDir(v.normalOS.xyz);
				
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
            	Light mainLight = GetMainLight();
            	float NoL = clamp(dot(mainLight.direction, i.normalWS.xyz) *0.5 + 0.5,0.01,0.99);
                half4 col = float4(NoL,NoL,NoL,1);//SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv) * _Color;
                
                return col;
            }
            ENDHLSL
        }
        Pass
        {
			Name "ShadowCast"
 
			Tags{ "LightMode" = "ShadowCaster" }
			HLSLPROGRAM
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "./../../../ThirdParts/GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.5

			#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShadowDepthInclude.hlsl"

			CBUFFER_START(UnityPerMaterial)
			float3 _LightDirection;
			float3 _LightPosition;
			float _AlphaClipThreshold;
			CBUFFER_END

             //--------------------------------------
            // GPU Instancing
			#pragma multi_compile _ DOTS_INSTANCING_ON
            #pragma multi_compile _ _ESM

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

				UNITY_VERTEX_INPUT_INSTANCE_ID 
			};
 
			//TEXTURE2D(_BaseMap) ;
			//SAMPLER(sampler_BaseMap);
			//float4 _BaseMap_ST;

			
			float4 GetShadowPositionHClip(Attributes input)
			{
				float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
				float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

				#if _CASTING_PUNCTUAL_LIGHT_SHADOW
					float3 lightDirectionWS = normalize(_LightPosition - positionWS);
				#else
					float3 lightDirectionWS = _LightDirection;
				#endif

				float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

				#if UNITY_REVERSED_Z
					positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#else
					positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#endif


				return positionCS;
			}

 
			Varyings vert(Attributes input)
			{
				Varyings output;

				UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

				output.positionCS = GetShadowPositionHClip(input);
				output.uv= input.texcoord;

				return output;
			}
			float4 frag(Varyings i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);

				float4 color;

				//half4 albedo = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv );
				half alpha = 1;
				clip(alpha-0);

				return ShadowFrag(i.positionCS);
			}
			ENDHLSL
        }
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

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            #pragma exclude_renderers gles gles3 glcore
            #pragma target 2.0

            #pragma vertex UniversalVertexMetaSGAME
            #pragma fragment UniversalFragmentMetaLitSGAME
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv0          : TEXCOORD0;
                float2 uv1          : TEXCOORD1;
                float2 uv2          : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            #ifdef EDITOR_VISUALIZATION
                float2 VizUV        : TEXCOORD1;
                float4 LightCoord   : TEXCOORD2;
            #endif
            };


            Varyings UniversalVertexMetaSGAME(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = UnityMetaVertexPosition(input.positionOS.xyz, input.uv1, input.uv2);
                output.uv = input.uv0;
            #ifdef EDITOR_VISUALIZATION
                UnityEditorVizData(input.positionOS.xyz, input.uv0, input.uv1, input.uv2, output.VizUV, output.LightCoord);
            #endif
                return output;
            }

            half4 UniversalFragmentMetaSGAME(Varyings fragIn, MetaInput metaInput)
            {
            #ifdef EDITOR_VISUALIZATION
                metaInput.VizUV = fragIn.VizUV;
                metaInput.LightCoord = fragIn.LightCoord;
            #endif

                return UnityMetaFragment(metaInput);
            }
            
            half4 UniversalFragmentMetaLitSGAME(Varyings input) : SV_Target
            {
                half4 diffuse = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half alpha = 0 - 0.9;
            	//clip(alpha);
            	//return half4(diffuse.rrr,1);
                clip(alpha); // 这个MetaPass下的clip为什么没有抠掉？

                MetaInput metaInput;
                metaInput.Albedo = float3(1,1,1);
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
       //FallBack "Hidden/InternalErrorShader"
}
