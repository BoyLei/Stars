Shader "Hidden/DecalShadow" 
{
	Properties 
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
		_MainTex ("Main Texture", 2D) = "white" {}
	}

	SubShader 
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		Pass 
		{		
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_instancing
            	#pragma target 3.0


				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

				struct a2f 
				{
					float4 vertex : POSITION;
					float4 texcoord0: TEXCOORD0;

					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f 
				{
					float4 pos : SV_POSITION;
					half2 texcoord : TEXCOORD0;
					float4 posSS : TEXCOORD1;
					half3 ray : TEXCOORD2;

					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				TEXTURE2D(_MainTex);
				SAMPLER(sampler_MainTex);
				TEXTURE2D(_CameraDepthTexture);
				SAMPLER(sampler_CameraDepthTexture);
				half4 _Color;

				v2f vert (a2f v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
                	UNITY_TRANSFER_INSTANCE_ID(v, o);

					o.pos = TransformObjectToHClip(v.vertex);
					o.texcoord.xy = v.texcoord0.xy;
					o.posSS = ComputeScreenPos(o.pos);
					float3 posWS = TransformObjectToWorld(v.vertex);
					o.ray = TransformWorldToView(posWS) * half3(-1, -1, 1);

					return o;
				}

				half4 frag(v2f i):SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(i);

					i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
    				float buffDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.posSS.xy / i.posSS.w);
					half depth = Linear01Depth(buffDepth, _ZBufferParams);
					float4 posVS = float4(i.ray * depth, 1);
					float3 posWS = mul(unity_CameraToWorld, posVS).xyz;
					float3 posOS = TransformWorldToObject(posWS).xyz;

					clip (float3(0.5, 0.5, 0.5) - abs(posOS));
					half2 mainUV = posOS.xz + 0.5;
					half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainUV);

					color *= _Color;
					return color;
				}
			ENDHLSL
		}
	}
}
