Shader "SGAME/DecalCloudShadow"{
	Properties{
		_Color ("主颜色(rgb控制颜色,a控制透明度)", Color) = (0, 0, 0, 1)
		_MainTex ("主贴图", 2D) = "white" {}
		_FlipBookNum ("FlipBook贴图的动画帧数量,(比如8x8贴图则填8,非FlipBook填1)",int) = 1
		_FlipBookTime ("FlipBook动画帧播放速度",range(0,10)) =1
		_DeepthSmoothness("浓淡受深度的影响值(值越小受深度影响越小)",range(0.01,1)) =1
		//_Depthdistance("",range(0,5)) = 1
		}

	SubShader{
		//the material is completely transparent and is rendered before other transparent geometry by default (at 2500)
		Tags{ "RenderPipeline" = "UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+400" "DisableBatching"="True"}

		//Blend via alpha
		//Blend SrcAlpha OneMinusSrcAlpha
		Blend DstColor Zero
		ZWrite off
		Cull off

		Pass{
			Name"DecalCloudShadow"
			Tags{"LightMode" = "UniversalForward"}
			
			HLSLPROGRAM
			
			//include useful shader functions
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			//#include "UnityCG.cginc"


			#pragma multi_compile_fog
			
			#pragma vertex vert
			#pragma fragment frag

			CBUFFER_START(UnityPerMaterial)
			sampler2D _MainTex;
			float4 _MainTex_ST;
			int _FlipBookNum;
			float _FlipBookTime;
			float _DeepthSmoothness;

			half4 _Color;

			TEXTURE2D(_CameraDepthTexture);
			SAMPLER(sampler_CameraDepthTexture);
			CBUFFER_END
			//sampler2D _CameraDepthTexture;

			struct appdata{
				float4 vertex : POSITION;
			};

			struct v2f{
				float4 position : SV_POSITION;
				float4 screenPos : TEXCOORD0;
				float4 ray : TEXCOORD1;
			};
			
			v2f vert(appdata v){
				v2f o;
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.position = TransformWorldToHClip(worldPos);
				o.ray.xyz = worldPos - _WorldSpaceCameraPos;
				o.ray.w = ComputeFogFactor(o.position.z); ;
				o.screenPos = ComputeScreenPos (o.position);
				return o;
			}

			float3 getProjectedObjectPos(float2 screenPos, float3 worldRay,out float depthOut){
				//get depth from depth texture
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, screenPos);
				depthOut = depth;
				depth = Linear01Depth (depth,_ZBufferParams) * _ProjectionParams.z;
				//get a ray thats 1 long on the axis from the camera away (because thats how depth is defined)
				worldRay = normalize(worldRay);
				//the 3rd row of the view matrix has the camera forward vector encoded, so a dot product with that will give the inverse distance in that direction
				worldRay /= dot(worldRay, -UNITY_MATRIX_V[2].xyz);
				//with that reconstruct world and object space positions
				float3 worldPos = _WorldSpaceCameraPos + worldRay * depth;
				float3 objectPos =  mul (unity_WorldToObject, float4(worldPos,1)).xyz;
				//discard pixels where any component is beyond +-0.5
				clip(0.5 - abs(objectPos));
				//get -0.5|0.5 space to 0|1 for nice texture stuff if thats what we want
				objectPos += 0.5;
				return objectPos;
			}
			float3 MixFogColorForDeCalCloud(float3 fragColor, float3 fogColor, float fogFactor)
			{
			    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
			    if (IsFogEnabled())
			    {
			        float fogIntensity = ComputeFogIntensity(fogFactor);
			        fragColor = lerp(fogColor, fragColor, pow(fogIntensity,1.2));
			    }
			    #endif
			    return fragColor;
			}
			half4 frag(v2f i) : SV_TARGET{
				float2 screenUv = i.screenPos.xy / i.screenPos.w;

				
				float depth;
				float2 uv = getProjectedObjectPos(screenUv, i.ray,depth).xz;

				//根据深度边缘平滑
				float4 ase_screenPosNorm = i.screenPos / i.screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float screenDepth41 = LinearEyeDepth(depth,_ZBufferParams);
				float distanceDepth41 = abs( ( screenDepth41 - LinearEyeDepth( ase_screenPosNorm.z,_ZBufferParams ) ) / ( 3/*_Depthdistance*/ ) );
				distanceDepth41 = clamp(distanceDepth41,0,1);
				//return float4(distanceDepth41,distanceDepth41,distanceDepth41,1);
				//flipControl
				float2 FlipBookV2 = float2(_FlipBookNum,_FlipBookNum);
				float2 FlipBookTotalLength = float2 (_FlipBookNum*_FlipBookNum, _FlipBookNum);
				float clampResult42_g1 = clamp( 0.0 , 0.0001 , ( _FlipBookNum*_FlipBookNum - 1.0 ) );
				float temp_output_35_0_g1 = frac( ( ( _TimeParameters.x * _FlipBookTime /10 + clampResult42_g1 ) / _FlipBookNum*_FlipBookNum ) );
				float2 appendResult29_g1 = (float2(temp_output_35_0_g1 , ( 1.0 - temp_output_35_0_g1 )));
				float2 FlipBookUV = ( ( uv / FlipBookV2 ) + ( floor( ( FlipBookTotalLength * appendResult29_g1 ) ) / FlipBookV2 ) );
			    //read the texture color at the uv coordinate
				half4 col = tex2D(_MainTex, FlipBookUV).rrrr;
				col.a*=saturate(distanceDepth41 * (1 / _DeepthSmoothness));
				col.rgb *=col.a;
				col *= _Color;
				col.rgb = lerp(float3(1,1,1),col.rgb,col.a);

				//混合雾
                col.rgb = MixFogColorForDeCalCloud(col.rgb,float3(1,1,1), i.ray.w);
				return col;
			}

			ENDHLSL
		}
	}
	FallBack "Hidden/InternalErrorShader"
}