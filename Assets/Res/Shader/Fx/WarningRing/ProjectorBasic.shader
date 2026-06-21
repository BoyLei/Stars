// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "SGAME/Indicators/Basic" {
	Properties {
		[Header(IfOnTop)]
  		[Enum(UnityEngine.Rendering.CompareFunction)]_Ztest("显示在最上层(Always), 默认(LessEqual)", Float) = 4
		[Space(20)]
		[KeywordEnum(On,Off)]_Decal("是否开启贴花模式(贴花模型需要用立体的)",float) = 0
  		[Space(20)]
		[HDR]_MainColor ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Shape", 2D) = "" {}
		[Space(20)]
  		_OutsideRingRadious("外圈半径",Range(0.5,10)) = 0.5
  		_OutsideRingOffset("外圈半径矫正",Range(-0.3,0.3)) = 0
		[Space(20)]
		[KeywordEnum(On,Off)]_InsideMask("是否开启内环遮罩",float) = 0
		_InsideRingRadious("内环半径",Range(0.1,10)) = 0.1
	}
	
	Subshader {
		Tags 
		{
			"RenderPipeline" = "UniversalPipeline"
			"Queue"="Transparent"
		}
		Pass {
			ZTest [_Ztest]
			ZWrite Off
			AlphaTest Greater 0
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha
			Offset -1, -1
			Cull off
	
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			//#include "UnityCG.cginc"
			#pragma shader_feature_local _INSIDEMASK_ON
			#pragma shader_feature_local _DECAL_OFF
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			struct appdata{
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0; 
			};
			
			struct v2f {
				float4 uvMain : TEXCOORD0;
				float4 ray :TEXCOORD1;
				float4 pos : SV_POSITION;
				float4 screenPos : TEXCOORD2;
			};
			CBUFFER_START(UnityPerMaterial)
			half _OutsideRingRadious;
			half _OutsideRingOffset;
			half _InsideRingRadious;
			half4 _MainColor;
			sampler2D _MainTex;
			CBUFFER_END
			#define SqaureRoot2 1.4142
			#define SqaureRoot3 1.7320
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;

			TEXTURE2D(_CameraDepthTexture);
			SAMPLER(sampler_CameraDepthTexture);
			
			v2f vert (appdata v)
			{
				v2f o;
				///外框进行缩放
      			//外框半径
      			half outSideRing =  _OutsideRingRadious;
      			half3 dirFromCenter = (v.vertex.xyz - half3(0,0,0));
      			v.vertex.xz += half2(v.vertex.xz) * (outSideRing - 0.5) * SqaureRoot2 * SqaureRoot2 + _OutsideRingOffset * outSideRing * half2(v.vertex.xz/distance(v.vertex.xz,half2(0,0))); //dirFromCenter.xz * (outSideRing - 0.5);
      			///
				o.pos = TransformObjectToHClip (v.vertex);
				o.uvMain.xy =  v.texcoord;
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.ray.xyz = worldPos - _WorldSpaceCameraPos;
				o.ray.w = ComputeFogFactor(o.pos.z); 
				o.screenPos = ComputeScreenPos (o.pos);
				//UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}

			float3 getProjectedObjectPos(float2 screenPos, float3 worldRay,out float depthOut, float Scale,float offset){
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
				objectPos /= Scale;
      			objectPos.xz = (objectPos.xz - half2(0,0))*(1 - offset) + half2(0,0);
				clip(0.5 - abs(objectPos));
				//get -0.5|0.5 space to 0|1 for nice texture stuff if thats what we want
				objectPos += 0.5;
				return objectPos;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				//获取圆形扩张的比例
      			half scale = _OutsideRingRadious * 2;
				//获取decal uv
				float2 screenUv = i.screenPos.xy / i.screenPos.w;
				float depth = 0 ;

				#ifdef _DECAL_OFF
      			float2 uv =i.uvMain.xy;
      			#else
      			float2 uv = getProjectedObjectPos(screenUv, i.ray,depth, scale,_OutsideRingOffset).xz;
      			#endif
				
				half4 texS = tex2D (_MainTex, /*UNITY_PROJ_COORD*/(uv));
				texS.rgba *= _MainColor.rgba;
				half4 res = texS;

				half2 center = half2(0.5,0.5);
				half insideMask = 1;
				#ifdef _INSIDEMASK_ON
				//内环遮罩
				half insideRingRadious = _InsideRingRadious ;
				//half outsideRingRadious = _OutsideRingRadious;
				half distanceToCenter = distance(uv.xy,center.xy) * scale;
				half insideRingMask = step(distanceToCenter,insideRingRadious);
				insideMask =1 - insideRingMask; //insideTex.r * visBlit;
				// float insideRing =(step(distanceToCenter,insideRingRadious + 0.005) - insideRingMask) ; //insideTex.a * visBlit;
				#endif
				res.rgb = MixFog(res.rgb, i.ray.w);
				res.a *= insideMask;
				//UNITY_APPLY_FOG_COLOR(i.fogCoord, res, half4(0,0,0,0));
				return res;
			}
			ENDHLSL
		}
	}
}
