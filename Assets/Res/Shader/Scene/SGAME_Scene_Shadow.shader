Shader "SGAME/SGAME_Scene_Shadow"
{
    Properties
    {
        _BaseMap ("主贴图(A为透明通道)", 2D) = "white" {}
        
    	_Alpha("面片本身透明度,地编调整用", range(0,1)) = 1
        _AlphaClipThreshold("控制AlphaClip的閾值",range(0,1)) = 0
    	[HideInInspector]_LightDirection("",Vector) = (1,0,0,1)
	    [HideInInspector]_LightPosition("",Vector) = (0,0,0,1)
    }
    SubShader
    {
        Tags 
        {
            "RenderPipeline" = "UniversalPipeline"
			"Queue" = "Geometry+50"
        }
        LOD 100

		pass
		{
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			Blend SrcAlpha OneMinusSrcAlpha
			
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

			CBUFFER_START(UnityPerMaterial)
			float3 _LightDirection;
			float3 _LightPosition;
			float _AlphaClipThreshold;
			float _Alpha;
			CBUFFER_END
			


             //--------------------------------------
            // GPU Instancing
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			#include "./../../../ThirdParts/GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc"
			#pragma instancing_options procedural:setupGPUI
			#pragma multi_compile_instancing

			struct Attributes
			{
				float4 positionOS   : POSITION;
				 
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
 
			struct Varyings
			{
				float4 positionCS   : SV_POSITION;

				UNITY_VERTEX_INPUT_INSTANCE_ID 
			};
		
			Varyings vert(Attributes input)
			{
				Varyings output;

				UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);

				return output;
			}
			float4 frag(Varyings i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);				
				return float4(0,0,0,_Alpha);
			}
			ENDHLSL
        }


        Pass
        {
			Name "ShadowCast"
 
			Tags{ "LightMode" = "ShadowCaster" }
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShadowDepthInclude.hlsl"

			CBUFFER_START(UnityPerMaterial)
			float3 _LightDirection;
			float3 _LightPosition;
			float _AlphaClipThreshold;
			float _Alpha;
			CBUFFER_END

             //--------------------------------------
            // GPU Instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			#include "./../../../ThirdParts/GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc"
			#pragma instancing_options procedural:setupGPUI
			#pragma multi_compile_instancing
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

				half4 albedo = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv );
				half alpha = albedo.a;
				clip(alpha-_AlphaClipThreshold);
				
				return ShadowFrag(i.positionCS);
			}
			ENDHLSL
        }

	
    }
   FallBack "Hidden/InternalErrorShader"
}
