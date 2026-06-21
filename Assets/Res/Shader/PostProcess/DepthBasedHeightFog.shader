Shader "SGAME/PostProcess/DepthBasedHeightFog"
{ 
    Properties
    {
    	//_FogIntensity("FogIntensity",range(-100,100)) = 0
    	
    	//_MainTex("MainTex",2d) = "white" {}
    	//_Volume ("Volume",3d) = ""{}
    	
    	[Space(20)]
    	[Header(Fog Color)]
    	[HDR]_HeightFogColorNear("高度雾近处的颜色",color) = (0.5,0.5,0.5,1)
	    [HDR]_HeightFogColorFar("高度雾远处的颜色",color) = (0.8,0.8,0.8,1)
    	_ColorDistance("近远颜色过度距离",range(0,1))= 0.2
    	_ColorChangeRange("近远颜色过渡平滑度",range(0,0.5)) = 0.1
    	
    	[Space(20)]
    	[Header(Height Fog)]
        _FogLowPlane("雾的下平面(世界坐标y轴)",range(-200,200)) = -10
    	_FogHighPlane("雾的上平面(世界坐标y轴)",range(-200,200)) = 100
    	_FogHeightFallOff("高度雾FallOff",range(0,10)) = 1
    	_FogHigh("雾的高度", range(0,1)) = 0.5
    	_HeightFogAlpha("高度雾的透明度",range(0,1)) = 1
    	
    	[Space(20)]
    	[Header(Fog Noise)]
    	//[toggle]
    	[KeywordEnum(ON,OFF)]_IfNoiseTex("是否启动噪声贴图",int) = 1
        [NoScaleOffset]_FogNoiseTex("雾气噪声贴图",2d) = "white" {}
    	_FogNoiseStength("雾气噪声强度",range(0,1)) = 0.5
    	_FogNoiseDensity("雾气噪声的密度",range(0,1)) = 0.04
    	_FogNoiseDir("雾气噪声的运动方向(XY控制)",Vector) = (1,1,0,0)
    	_FogNoiseSpeed("雾气噪声的运动速度",range(0,5)) = 1
    	
    	
    	[Space(20)]
    	[Header(Fog Distance)]
    	_FogDistance("雾靠近摄像机的距离",range(0,1)) = 0.1
    	_FogDisappearRange("雾从有到消失的过渡",range(0,0.3)) = 0.13
    	//[Space(20)]
    	
	    //[NoScaleOffset]_FogMaskTex("雾气遮罩贴图",2d)= "white" {}
	   // _FogIntensityA("雾气总强度",range(0,3)) = 0
    	
    	//高度雾部分
    }
	SubShader
	{ 
		
		Pass
		{ 
			Tags {"RenderType" = "Transparent" 
				  "IgnoreProjector" = "True"
				  "RenderPipeline" = "UniversalPipeline" 
				  "ShaderModel"="4.5"
				  "Queue"="Transparent+100"
			      }
			Blend SrcAlpha OneMinusSrcAlpha
			Zwrite off
			Cull off
			ZTest off
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _IFNOISETEX_ON _IFNOISETEX_OFF
			//#include "UnityCG.cginc"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			//sampler2D _CameraDepthTexture;
			/*TEXTURE2D(_CameraDepthTexture);
			SAMPLER(sampler_CameraDepthTexture);*/
			//TEXTURE2D_X(_MainTex);
            //SAMPLER(sampler_MainTex);
			//TEXTURE2D(_CameraOpaqueTexture) ;
			//SAMPLER(sampler_CameraOpaqueTexture);
			//TEXTURE2D(_FogMaskTex);
			//SAMPLER(sampler_FogMaskTex);
			TEXTURE2D(_FogNoiseTex);
			SAMPLER(sampler_FogNoiseTex);
			float4 _FogNoiseTex_ST;
			

			float4 _HeightFogColorNear;
            float4 _HeightFogColorFar;
			float _ColorDistance;
			float _ColorChangeRange;
			
			float4x4 _InvVP;
			//float _FogIntensity;
			float _FogHigh;

			int _IfNoiseTex;
			float _FogNoiseStength;
			float _FogNoiseDensity;
			float4 _FogNoiseDir;
			float _FogNoiseSpeed;
			
			
			float _FogLowPlane;
			float _FogHighPlane;
			float _FogHeightFallOff;
			float _HeightFogAlpha;

			float _FogDistance;
			float _FogDisappearRange;
			
			float4x4 _MatrixP;
			float4x4 _MatrixIV;
			float4x4 _MatrixV;
			
		
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{ 
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 ScreenPos : TEXCOORD1;
				float4 ray : TEXCOORD2;
				float4 color :TEXCOORD3;
			};

			inline float4 ASE_ComputeGrabScreenPos( float4 pos )
			{
				#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
				#else
				float scale = 1.0;
				#endif
				float4 o = pos;
				o.y = pos.w * 0.5f;
				o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
				return o;
			}
			
			v2f vert(appdata v)
			{ 
   
				v2f o;
				o.vertex = TransformObjectToHClip(v.vertex);
				o.ScreenPos = ComputeScreenPos(o.vertex);
				o.ray.xyz = TransformObjectToWorld(v.vertex) - _WorldSpaceCameraPos;
				o.ray.w = ComputeFogFactor(o.vertex.z); ;
				o.uv = v.uv;
				o.color = v.color;
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{ 
				////////////////////////////////////////////////////////////////////////////////////////////////
				///深度重构世界坐标
				float2 UV =i.vertex.xy / _ScaledScreenParams.xy;
				 #if UNITY_REVERSED_Z
                    real depth = SampleSceneDepth(UV);
                #else
                    // Adjust Z to match NDC for OpenGL ([-1, 1])
                    real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(UV));
                #endif
				float3 worldPos3 =ComputeWorldSpacePosition( UV, depth, UNITY_MATRIX_I_VP);
				//float ifSkybox = (depth ==0)? 0 : 1;
				//return float4(depth,depth,depth,1);
				////////////////////////////////////////////////////////////////////////////////////////////////

				//采样遮罩贴图
				//float mask = SAMPLE_TEXTURE2D_X(_FogMaskTex, sampler_FogMaskTex, i.uv).r;

				#ifdef _IFNOISETEX_ON
				//采样雾气噪声贴图 计算流动
				float2 fogNoiseuUV = worldPos3.xz * float2(_FogNoiseDensity,_FogNoiseDensity) + normalize(_FogNoiseDir.xy) * (_Time.y/20) * _FogNoiseSpeed;
				float fogNoise =  SAMPLE_TEXTURE2D(_FogNoiseTex, sampler_FogNoiseTex, fogNoiseuUV).r;
				fogNoise =1 -  (1 - fogNoise )*_FogNoiseStength;
				_FogHigh *=fogNoise;
				#endif
				// //测试深度
				// float depth =   Linear01Depth(tex2Dproj(_CameraDepthTexture,i.ScreenPos).r,_ZBufferParams);
				//depth = tex2Dproj(_CameraDepthTexture,i.ScreenPos).r;
				
				//高度雾计算.
				float fogLowPlane = _FogLowPlane;
				float fogHighPlane = _FogHighPlane;
				float fogVerticalDist = fogHighPlane - fogLowPlane;

				float4 fogColor = float4(1,1,1,1);
				//float fogFactor =clamp((worldPos3.y - fogVerticalDist * _FogHigh/* - fogLowPlane*/)/(fogVerticalDist),0,1) ;
				float fogFactor =  clamp((worldPos3.y+ (1- _FogHigh) * fogVerticalDist- fogLowPlane /*- fogVerticalDist * _FogHigh*//* - fogLowPlane*/)/(fogVerticalDist) ,0,1) ;
				//return float4(fogFactor,fogFactor,fogFactor,1);
				fogFactor = 1 - pow(fogFactor,_FogHeightFallOff+0.01);
				//fogFactor =clamp(fogFactor * 10,0,1);
				
				
				//远处雾计算
				float DistanceToCamera = distance(worldPos3.xz,GetCameraPositionWS().xz);
				float fogNearPlane = 1;
				float fogFarPlane = 100;
				float DistanceFactor = clamp((DistanceToCamera - 0 - fogNearPlane)/(fogFarPlane- fogNearPlane),0,1);
				float DistanaceRange = smoothstep(_FogDistance,_FogDistance + _FogDisappearRange,DistanceFactor);
				//return float4(DistanaceRange,DistanaceRange,DistanaceRange,1);
				//混合高度雾和深度雾
				fogFactor = fogFactor * DistanaceRange  + DistanceFactor* DistanaceRange ;
				fogFactor *=_HeightFogAlpha;

				//判断是否是天空球
				//fogFactor *= ifSkybox;

				float DistanceColorRange = smoothstep(_ColorDistance,_ColorDistance + _ColorChangeRange,DistanceFactor);
				//return float4(DistanceColorRange,DistanceColorRange,DistanceColorRange,1);
				fogColor.rgba =lerp(_HeightFogColorNear.rgba,_HeightFogColorFar.rgba,DistanceColorRange);
				//return float4(fogFactor,fogFactor,fogFactor,1);
				// float3 FinalColor = lerp(ScreenColor.rgb,fogColor,fogFactor);
				return float4(fogColor.rgb,fogFactor  * fogColor.a);
			}
			ENDHLSL
		}
	}
}
