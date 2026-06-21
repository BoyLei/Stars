Shader "SGAME/Grass"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin][Enum(UnityEngine.Rendering.CullMode)]_CullModel("Cull Model", Float) = 0
		[HideInInspector]_ZGAMEURPWindRT("_ZGAMEURPWindRT", 2D) = "white" {}
		[HideInInspector]_AlphaClip("Alpha Clip", Float) = 0

		[Space(20)]
		[Header(Wind)][KeywordEnum(VertexColor,VertexUV,VertexPositionY,TexSoft)] _HardnessSource("Hardness Source", Float) = 0
		[HideInInspector]_MicrowaveIntensity("Microwave Intensity", Float) = 1
		[Toggle(_LOW_WIND_ON)] _WindLow("微风",Float) = 0 
		_WindControl("WindControl(x:XSpeed y:YSpeed z:ZSpeed w:windMagnitude)",vector) = (1,1,1,0.5)
        //前面几个分量表示在各个轴向上风浪的速度, w用来模拟地图的大小,值越小草摆动的越凌乱，越大摆动的越整体
        _WaveControl("WaveControl(x:XSpeed y:YSpeed z:ZSpeed w:worldSize)",vector) = (1,0,1,1)
		_GrassQuadSize("面片草宽高",vector) = (1,1,0,0)

		[Space(20)]
        [Toggle(_FIERCE_WIND_ON)] _FierceWind("开启麦浪",Float) = 0 
        _WindValue("Wind(x,y,z,风力强度,xyz为风的方向)",vector) = (1,0,1,10)
		//_StormParams("StormParams(Begin,Keep,End,Slient)",vector) = (1,100,40,100)
        //_StormStrength("强力风力",Range(0,40)) = 20
		_WindNoiseStrength("噪声扰乱强度",Range(0,40)) = 10
		_NoiseTillingOffset("xy为噪声的Tilling,z为噪声密度",vector) = (1,1,0.1,0)
        _NoiseStrength("x控制噪声的强度,y控制草的上下起伏程度",vector) = (1,1,1,1)
		_NoiseSpeed("噪声的速度",Range(0,5)) = 0.4
		_FierceWindTint("麦浪颜色影响(不影响则设为0)",range(0,1)) = 0.1

		[Space(20)]
		[Header(Emission)]
		_EmissionStrength("自发光强度,越小则自发光强度越弱",Range(0,10)) = 0
		_EmissionRange("自发光区域,值越大则尖端自发光的区域越大",Range(0,1)) = 0.2
		//_EmissionWindStrength("自发光受风场的影响,值越大受风场影响越大",Range(0,1)) = 0
		
		/*[HDR]*/[Header(Render)][MainColor]_BaseColor("Base Color", Color) = (1,1,1,1)
		/*[HDR]*/_ShadowColor("ShadowColor(阴影颜色,不影响则设置为纯灰,明度为50)",Color) = (0.5,0.5,0.5,1)
		_VarientColor("草地杂色,alpha为强度",Color) = (0,0,0,0)
		_VarientColorDensity("草地杂色的密度,为0则全部是杂色,为1则全部是基础色",Range(0,1)) = 0.3
		[HideInInspector]_VertexDarkening("草顶部随机暗色强度",Range(0,1)) = 0
		//[MainTexture]_BaseMap("Albedo", 2D) = "white" {}
		[Toggle(_NORMALFACE_ON)] _NormalFace("NormalFace", Float) = 0
		//_NormalMap("Normal Map", 2D) = "bump" {}
		//_MSAF("MSAF", 2D) = "white" {}
		_Metallic("Metallic", Range( 0.01 , 1)) = 0
		_FrontSmoothness("FrontSmoothness", Range( 0 , 1)) = 0
		_BackSmoothness("BackSmoothness", Range( 0 , 1)) = 0
		_Occlusion("Occlusion", Float) = 1
		//[Header(Translucecy)]_TranslucecyColor("Translucecy Color", Color) = (0,0,0,0)
		//[ASEEnd]_TranslucecyMap("Translucecy Map", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		
		[Header(Global Light)]
		_GIStrength("环境光强度,正常默认值为1",range(0,2)) = 1
		_ShadowStrength("投影强度,正常值为1,越偏0投影颜色越浅",range(0,1)) = 1

		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
//		_TransStrength( "Strength", Range( 0, 50 ) ) = 1
//		_TransNormal( "Normal Distortion", Range( 0, 1 ) ) = 0.5
//		_TransScattering( "Scattering", Range( 1, 50 ) ) = 2
//		_TransDirect( "Direct", Range( 0, 1 ) ) = 0.9
//		_TransAmbient( "Ambient", Range( 0, 1 ) ) = 0.1
//		_TransShadow( "Shadow", Range( 0, 1 ) ) = 0.5
		
		[Space(20)]
		[HideInInspector] _ColorMapUV("",Vector) = (1,1,1,1)
		//[Header(TheColorMap)] _ColorMap("地面颜色贴图", 2D) = "black" {}
		_ColorMapCol("地面颜色",Color) = (0,0,0,1)
		_ColorMapStrength("地面颜色强度",Range(0,1)) = 0
		_ColorMapHeight("地面颜色占草的比例",Range(0,1)) = 0
//		_ColorMapSaturation("地面颜色的饱和度",Range(0,2)) = 1
//		_ColorMapValue("地面颜色的明度",Range(0,2)) = 1
//		_VertexAOStrength("底部AO强度",Range(0,1)) = 0
		[HideInInspector]_ZGAMEURPMicrowaveWindStreng("ZGAMEURPMicrowaveWindStreng", vector) = (0,0,0,1)
		[HideInInspector]_ZGAMEURPWindStrengthMultipliers("ZGAMEURPWindStrengthMultipliers", vector) = (0,0,0,1)
		[HideInInspector]_ZGAMEURPWindDirSize("_ZGAMEURPWindDirSize", vector) = (0,0,0,1)
	}
	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue" = "Geometry+40"}
		Cull [_CullModel]
		AlphaToMask Off
		
		HLSLINCLUDE
		#pragma target 3.0

		#pragma prefer_hlslcc gles
		#pragma exclude_renderers d3d11_9x 

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		CBUFFER_START(UnityPerMaterial)
			//float4 _MSAF_ST;
			float4 _BaseColor;
			float4 _ShadowColor;
			//float4 _BaseMap_ST;
			float4 _NormalMap_ST;
			float4 _TranslucecyColor;
			float4 _TranslucecyMap_ST;
			float _CullModel;
			float _MicrowaveIntensity;
			float _Metallic;
			float _FrontSmoothness;
			float _BackSmoothness;
			float _GIStrength;
			float _ShadowStrength;
			float _Occlusion;
			float _AlphaClip;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			float4 _ZGAMEURPWindDirSize;
            sampler2D _ZGAMEURPWindRT;
            
   
            float4 _ZGAMEURPMicrowaveWindStreng;
            float4 _ZGAMEURPWindStrengthMultipliers;
            //sampler2D _MSAF;
            sampler2D _BaseMap;
            //sampler2D _NormalMap;
            sampler2D _TranslucecyMap;
   
            float4 _WindControl;
            float4 _WaveControl;
            float4 _GrassQuadSize;
   
            float4 _WindValue;
              float _WindNoiseStrength;
              float4 _StormParams;
              float _StormStrength;
			  float4 _NoiseTillingOffset;
			  float4 _NoiseStrength;
			  float _NoiseSpeed;
			  float _FierceWindTint;

			  
			  float4 _ColorMapUV;
			  float4 _ColorMapCol;
			  float _ColorMapStrength;
			  float _ColorMapHeight;
			  float _ColorMapSaturation;
			  float _ColorMapValue;
			  float _VertexDarkening;
			  float4 _VarientColor;
			  float _VarientColorDensity;
			  //float _VertexAOStrength;

			  float _EmissionStrength;
			  float _EmissionRange;
			  //float _EmissionWindStrength;
			CBUFFER_END
		    //Lightmap明度强化相关
		    float _TerrainLM_SaturateThreshold;
		    float _TerrainLM_SaturationIntensity;
		    float _TerrainLM_Threshold;
		    float _TerrainLM_ThresholdKnee;
		    float _TerrainLM_Strength;
	        //采样地表LightMap相关
		    float2 _TerrainWorldInfoMin;
		    float2 _TerrainWorldInfoMax;
		    float4 _TerrainWorldInfo;
			// TEXTURE2D(_ColorMap);
			// SAMPLER(sampler_ColorMap);
			TEXTURE2D(_TerrainWorldLightmap);
			SAMPLER(sampler_TerrainWorldLightmap);
			//TEXTURE2D(_ZGAMEURPWindRT);
            //SAMPLER(sampler_ZGAMEURPWindRT);
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}
		
		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS

		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" 
					"Queue"="Geometry"}
			
			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "./../../../ThirdParts/GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing



			
			#define _NORMAL_DROPOFF_WS 1
			#define _TRANSLUCENCY_ASE 1
			//#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 100600

			
			//#pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
			#define _MAIN_LIGHT_SHADOWS_CASCADE 1
			//#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE //_MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ /*_ADDITIONAL_LIGHTS_VERTEX*/ _ADDITIONAL_LIGHTS //_ADDITIONAL_OFF
			//#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#define _SHADOWS_SOFT 1
			//#pragma multi_compile _ _SHADOWS_SOFT
			//#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			//#pragma multi_compile _ _CLUSTERED_RENDERING
			
			//#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			//#pragma multi_compile _ SHADOWS_SHADOWMASK

			//#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			//#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ TERRAIN_LIGHTMAP_SAMPLE
			#define _FIERCE_WIND_ON 1
			//#pragma shader_feature _ _FIERCE_WIND_ON
			//#pragma shader_feature _ _LOW_WIND_ON
			
			#pragma shader_feature _ _CUSTOM_SHADOW_ON
            #pragma shader_feature _ _MAINLIGHT_CUSTOM_SHADOW_ON
			#pragma multi_compile _ _ESM

			#define StormFront _StormParams.x
            #define StormMiddle _StormParams.y
            #define StormEnd _StormParams.z
            #define StormSlient _StormParams.w

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_FORWARD

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			//#include "../lib/Lighting.hlsl"
			#include "../lib/Lighting_Urp_12.1.9.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			#if ASE_SRP_VERSION <= 70108
			#define REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
			#endif

			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
			    #define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_BITANGENT
			//#pragma shader_feature_local _HARDNESSSOURCE_VERTEXCOLOR _HARDNESSSOURCE_VERTEXUV _HARDNESSSOURCE_VERTEXPOSITIONY _HARDNESSSOURCE_TEXSOFT
			//#pragma shader_feature_local _NORMALFACE_ON


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord : TEXCOORD0;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 lightmapUVOrVertexSH : TEXCOORD0;
				half4 fogFactorAndVertexLight : TEXCOORD1;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord : TEXCOORD2;
				#endif
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 screenPos : TEXCOORD6;
				#endif
				float4 ase_texcoord7 : TEXCOORD7;
				float4 TestOutput : TEXCOORD8;
				float4 Color :TEXCOORD9;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				//UNITY_VERTEX_OUTPUT_STEREO
			};

			
			

			///////////////////////////////////////
            ///噪声生成函数
            float2 Unity_GradientNoise_Dir_float(float2 p)
            {
                // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
                p = p % 289;
                // need full precision, otherwise half overflows when p > 1
                float x = float(34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }
            void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
            {
                float2 p = UV * Scale;
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
                float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
            }
            ///////////////////////////////////////
            ///////////////////////////////////////
            
			 float3 applyWind(float3 positionWS,float3 grassUpWS,float3 windDir,float windStrength,float vertexHeight)
            {
                float rad = sin(windStrength * PI * 0.8 / 2);
				//风向与草生长方向的正交向量
                float3 windDir_new = windDir - dot(windDir,grassUpWS) * grassUpWS;
                float x,y;
                sincos(rad,x,y);

                float3 windPos = x * windDir_new +   y * grassUpWS;

                return positionWS + (windPos - grassUpWS) * vertexHeight;            
            }


            float applyStorm(float3 positionWS,float3 windDir,float windStrength)
            {

                float stormInterval = StormFront + StormMiddle + StormEnd + StormSlient;
                float disInWindDir = dot(positionWS - windDir * _Time.x * 10 * (windStrength + _StormStrength),windDir);
                
                float offsetInInterval = stormInterval - (disInWindDir % stormInterval) - step(disInWindDir,0) * stormInterval;

                float x = 0;
                
                if(offsetInInterval < StormFront){
                    x = offsetInInterval * rcp(StormFront);
                }else if(offsetInInterval < StormFront + StormMiddle){
                    x = 1;
                }
                else if(offsetInInterval < StormFront + StormMiddle + StormEnd){
					//从1-0
                    x = (StormFront + StormMiddle + StormEnd - offsetInInterval) / StormEnd;
                }
                
                return windStrength +  _StormStrength * x;
				//return windStrength;
            }

			
			float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
			{
				original -= center;
				float C = cos( angle );
				float S = sin( angle );
				float t = 1 - C;
				float m00 = t * u.x * u.x + C;
				float m01 = t * u.x * u.y - S * u.z;
				float m02 = t * u.x * u.z + S * u.y;
				float m10 = t * u.x * u.y + S * u.z;
				float m11 = t * u.y * u.y + C;
				float m12 = t * u.y * u.z - S * u.x;
				float m20 = t * u.x * u.z - S * u.y;
				float m21 = t * u.y * u.z + S * u.x;
				float m22 = t * u.z * u.z + C;
				float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
				return mul( finalMatrix, original ) + center;
			}
			
			//采样地面ColorMap
            // void SampleColorMapTexture_float(in float3 positionWS, out float4 color) 
            // {
            // 	//Note: Unrolled from the GetColorMapUV, check that these are always in sync!
            // 	float2 uv = (positionWS.xz * _ColorMapUV.z) - (_ColorMapUV.xy * _ColorMapUV.z);
            // 	
            // 	color = SAMPLE_TEXTURE2D(_ColorMap, sampler_ColorMap, uv).rgba;
            // }

			//草地杂色
			float ObjectPosRand01()
			{
				#if defined(UNITY_DOTS_INSTANCING_ENABLED)
				return _Seed;
				#else
				return frac(UNITY_MATRIX_M[0][3] + UNITY_MATRIX_M[1][3] + UNITY_MATRIX_M[2][3]);
				#endif
			}
			// //Color ma3p UV
			// float2 BoundsToWorldUV(in float3 wPos, in float4 b)
			// {
			// 	return (wPos.xz * b.z) - (b.xy * b.z);
			// }
			// float2 GetColorMapUV(in float3 wPos)
			// {
			// 	return BoundsToWorldUV(wPos, _ColorMapUV);
			// }
			// float4 SampleColorMapTexture(in float3 positionWS) 
			// {
			// 	#ifndef GRASS_COMMON_INCLUDED
			// 	return 0;
			// 	#else
			// 	float2 uv = GetColorMapUV(positionWS);
			//
			// 	return SAMPLE_TEXTURE2D(_ColorMap, sampler_ColorMap, uv).rgba;
			// 	#endif
			// }
			// float3 ApplyColorMap(float3 positionWS, float3 iColor, float s) 
			// {
			// 	return lerp(iColor, SampleColorMapTexture(positionWS).rgb, s);
			// }
			
			
			//unity的uv旋转函数
			void Unity_Rotate_Radians_float(float2 UV, float2 Center, float Rotation, out float2 Out)
	        {
	            //rotation matrix
	            UV -= Center;
	            float s = sin(Rotation);
	            float c = cos(Rotation);
	        
	            //center rotation matrix
	            float2x2 rMatrix = float2x2(c, -s, s, c);
	            rMatrix *= 0.5;
	            rMatrix += 0.5;
	            rMatrix = rMatrix*2 - 1;
	        
	            //multiply the UVs by the rotation matrix
	            UV.xy = mul(UV.xy, rMatrix);
	            UV += Center;
	        
	            Out = UV;
	        }
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			 	
			 	//测试ZGame 自定义参数的效果
				//_ZGAMEURPWindStrengthMultipliers = float4(1,1,1,1);
				//_ZGAMEURPMicrowaveWindStreng = float4(1,1,1,1);
				float4 break211 = _ZGAMEURPWindDirSize;
				float3 appendResult213 = (float3(break211.x , 0.0 , break211.z));
				float3 WindDir219 = appendResult213;
				float4 transform224 = mul(GetWorldToObjectMatrix(),float4( WindDir219 , 0.0 ));
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float temp_output_183_0 = ( ( _TimeParameters.x ) * _ZGAMEURPMicrowaveWindStreng.x );
				float2 appendResult194 = (float2(( ase_worldPos.x + ( -_ZGAMEURPWindDirSize.x * temp_output_183_0 ) ) , ( ase_worldPos.z + ( temp_output_183_0 * -_ZGAMEURPWindDirSize.z ) )));
				float SizeInWorldSpace192 = ( 1.0 / _ZGAMEURPWindStrengthMultipliers.w );
				float3 weightedBlendVar205 = float3(0.33,0.33,0.33);
				float weightedBlend205 = ( weightedBlendVar205.x*( tex2Dlod( _ZGAMEURPWindRT, float4( ( appendResult194 * SizeInWorldSpace192 ), 0, 0.0) ).r * _ZGAMEURPMicrowaveWindStreng.y ) + weightedBlendVar205.y*( tex2Dlod( _ZGAMEURPWindRT, float4( ( appendResult194 * SizeInWorldSpace192 ), 0, 0.0) ).g * _ZGAMEURPMicrowaveWindStreng.z ) + weightedBlendVar205.z*( tex2Dlod( _ZGAMEURPWindRT, float4( ( appendResult194 * SizeInWorldSpace192 ), 0, 0.0) ).b * _ZGAMEURPMicrowaveWindStreng.w ) );
				weightedBlend205 = 0;
				//o.TestOutput.x = weightedBlend205;
				//float2 uv_MSAF = v.texcoord.xy * _MSAF_ST.xy + _MSAF_ST.zw;
				//float4 tex2DNode16 = tex2Dlod( _MSAF, float4( uv_MSAF, 0, 0.0) );
				//float TexSoft96 = tex2DNode16.a;
				#if defined(_HARDNESSSOURCE_VERTEXCOLOR)
				float staticSwitch62 = v.ase_color.r;
				#elif defined(_HARDNESSSOURCE_VERTEXUV)
				float staticSwitch62 = v.texcoord.y;
				#elif defined(_HARDNESSSOURCE_VERTEXPOSITIONY)
				float staticSwitch62 = v.vertex.xyz.y;
				#elif defined(_HARDNESSSOURCE_TEXSOFT)
				float staticSwitch62 = TexSoft96;
				#else
				float staticSwitch62 = v.ase_color.r;
				#endif
				float HardnessSource63 = staticSwitch62;
				float Microwave212 = ( sin( weightedBlend205 ) * HardnessSource63 * _MicrowaveIntensity );
				float Foliage210 = _ZGAMEURPWindStrengthMultipliers.y;
				float Grass214 = _ZGAMEURPWindStrengthMultipliers.x;
				float3 rotatedValue227 = RotateAroundAxis( float3( 0,0,0 ), v.vertex.xyz, transform224.xyz, -( ( 0.0 + Microwave212 + ( Foliage210 * HardnessSource63 ) ) * Grass214 ) );
				float3 rotatedValue228 = RotateAroundAxis( float3( 0,0,0 ), rotatedValue227, float3(0,1,0), -1.5 );
				float3 VertexPostion230 = ( rotatedValue228 + float3( 0,0,0 ) );
				
				float3 rotatedValue248 = RotateAroundAxis( float3( 0,0,0 ), v.ase_normal, float3(0,1,0), -1.5 );
				
				o.ase_texcoord7.xy = v.texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord7.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = VertexPostion230;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = rotatedValue248;

				//设置顶点传递的颜色.
				float rand = frac( v.vertex.y * 1.1);
				float vertexDarkening = lerp(1,1-rand*rand, _VertexDarkening*2);
				o.Color.r =  clamp(vertexDarkening*1.8,0,1);  //随机顶点暗部
				o.Color.r = v.vertex.y;
				o.Color.g = ObjectPosRand01();  //草地杂色
				o.Color.b = v.ase_color.r;      //存在顶点内的AO
				//o.Color.a = smoothstep(_ColorMapHeight,_ColorMapHeight+0.05,v.vertex.y);
				o.Color.a = v.vertex.y;
				float localVertexHeight = v.vertex.y;
				v.vertex.xy *= _GrassQuadSize.xy;
				v.vertex.xz += v.ase_normal*_GrassQuadSize.z;
				
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
			 	

#ifdef _LOW_WIND_ON
				float2 samplePos = positionWS.xz;
                samplePos -= _Time.x * _WaveControl.xz;
				float waveSample = tex2Dlod( _ZGAMEURPWindRT, float4((samplePos - _Time.x), 0, 0)).r;

                positionWS.x += sin(waveSample * _WindControl.x) * _WaveControl.x * _WindControl.w * localVertexHeight;
                positionWS.z -= sin(waveSample * _WindControl.z) * _WaveControl.z * _WindControl.w * localVertexHeight;
#endif

				
#ifdef _FIERCE_WIND_ON        
                float3 grassUpDir = float3(0,1,0);
                //grassUpDir = mul(unity_ObjectToWorld, grassUpDir);
				
                float3 windDir = normalize(_WindValue.xyz);   
                //常态风力强度
                float windStrength = _WindValue.w;

                //windStrength = applyStorm(positionWS.xyz,windDir,windStrength);
               
                float time = _Time.y ;
				float uvRotate = asin(normalize(windDir.xz).y) + 1.67;
				//uvRotate = _NoiseStrength.w;
                float2 noiseUV = float2(positionWS.x * cos(uvRotate) - positionWS.z * sin(uvRotate),
                						positionWS.x * sin(uvRotate) + positionWS.z * cos(uvRotate));
				Unity_Rotate_Radians_float(positionWS.xz,float2(0.5,0.5),uvRotate,noiseUV);
				//noiseUV = (noiseUV - time*1/ 30) ;
                //float noiseValue3 = tex2Dlod( _ZGAMEURPWindRT, float4(noiseUV, 0, 0)).r;
				float noiseValue  = 0;
				
				//noiseUV = (positionWS.xz - time*_NoiseSpeed);
				float noiseValus2 = 0;
				//noiseUV *= _NoiseTillingOffset.xy;
				noiseUV = noiseUV.xy * float2(_NoiseTillingOffset.x,_NoiseTillingOffset.y) + float2(0, time*_NoiseSpeed * 30);
				
				Unity_GradientNoise_float(noiseUV, _NoiseTillingOffset.z, noiseValus2);
				float noiseValue3 =1;
				Unity_GradientNoise_float(noiseUV + float2(1,1), _NoiseTillingOffset.z/2, noiseValue3);
				noiseValus2 =clamp(noiseValus2,0,1);
				noiseValue = /*noiseValue +*/ pow(clamp(noiseValus2,0,1),2);
				noiseValue *= _NoiseStrength.x;
				o.TestOutput.x =noiseValus2.x * localVertexHeight *localVertexHeight;
				o.TestOutput.yz = noiseUV.xy;
				o.TestOutput.w = noiseValus2.x;

				//noiseValue = noiseValue3;
				
                noiseValue = sin(noiseValue * windStrength);
                windStrength += noiseValue * _WindNoiseStrength;
					
                windStrength = saturate(windStrength / 40);
             
                positionWS.xyz = applyWind(positionWS.xyz,grassUpDir,windDir,windStrength,localVertexHeight*localVertexHeight);

				//风场影响法线
				//v.ase_normal = normalize(float3(-windStrength.x,localVertexHeight , -windStrength.x)  + v.ase_normal);
				//v.ase_normal = normalize(-float3(windDir.x,0,windDir.z)* localVertexHeight * windStrength*10+ v.ase_normal*1.5);
				
				
				positionWS.y -= sin(noiseValue * _NoiseStrength.y) * localVertexHeight;
#endif


				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );
				
				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					o.lightmapUVOrVertexSH.zw = v.texcoord;
					o.lightmapUVOrVertexSH.xy = v.texcoord * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );
				#ifdef ASE_FOG
					half fogFactor = ComputeFogFactor( positionCS.z );
				#else
					half fogFactor = 0;
				#endif
				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				
				
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				
				o.clipPos = positionCS;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				o.screenPos = ComputeScreenPos(positionCS);
				#endif
				return o;
			}
			
			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				o.ase_color = v.ase_color;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			half4 frag ( VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						, FRONT_FACE_TYPE ase_vface : FRONT_FACE_SEMANTIC ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				//UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				// float3 WorldPosition1 = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				// float4 color1 = float4(1,1,1,1);
				// float2 uv2 = (WorldPosition1.xz * _ColorMapUV.z) - (_ColorMapUV.xy * _ColorMapUV.z);
				// SampleColorMapTexture_float(WorldPosition1,color1);
				// return float4(color1.x,color1.y,color1.z,1);
				//return float4(IN.Color.x,IN.Color.y,IN.Color.y,1);
				Light LL = GetMainLight();
				float NOL = dot(IN.tSpace0.xyz,LL.direction);
				//return float4(NOL,NOL,NOL,1);
				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float2 sampleCoords = (IN.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					float3 WorldNormal = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					float3 WorldTangent = -cross(GetObjectToWorldMatrix()._13_23_33, WorldNormal);
					float3 WorldBiTangent = cross(WorldNormal, -WorldTangent);
				#else
					float3 WorldNormal = normalize( IN.tSpace0.xyz );
					float3 WorldTangent = IN.tSpace1.xyz;
					float3 WorldBiTangent = IN.tSpace2.xyz;
				#endif
				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 ScreenPos = IN.screenPos;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#endif
	
				WorldViewDirection = SafeNormalize( WorldViewDirection );

				
				float4 tex2DNode8 = float4(1,1,1,1);
				float4 MapColorTex = _ColorMapCol;
				//SampleColorMapTexture_float(WorldPosition,MapColorTex);
				tex2DNode8.rgb *= _BaseColor;
				IN.Color.g = smoothstep(_VarientColorDensity,_VarientColorDensity+0.1,IN.Color.g);
				//return float4(IN.Color.g,IN.Color.g,IN.Color.g,1);
				tex2DNode8.rgb = lerp(tex2DNode8.rgb, _VarientColor.rgb, IN.Color.g *IN.Color.g*_VarientColor.a);
				// float3 MapColorHSV = RgbToHsv(MapColorTex.rgb);
				// MapColorHSV.b *=_ColorMapValue;
				// MapColorHSV.g *=_ColorMapSaturation;
				// MapColorHSV.r *= 0.8;
				// MapColorTex.rgb = HsvToRgb(MapColorHSV);
				
				IN.Color.a = smoothstep(_ColorMapHeight,_ColorMapHeight+0.3,IN.Color.a);
				MapColorTex.rgb = lerp(MapColorTex.rgb,tex2DNode8.rgb,IN.Color.a);
				tex2DNode8.rgb = lerp(MapColorTex.rgb,tex2DNode8.rgb,1 -_ColorMapStrength );
				float rand = frac( IN.Color.x * 1.1);
				float topDarkening = lerp(1,1-rand*rand, _VertexDarkening*2);
				topDarkening = clamp(topDarkening*1.8,0,1);
				tex2DNode8.rgb *= topDarkening;
				tex2DNode8.rgb *= clamp(1 /*- (1 - IN.Color.z*1.3) * _VertexAOStrength*/,0,1); //加ao
				// float2 uv_NormalMap = IN.ase_texcoord7.xy * _NormalMap_ST.xy + _NormalMap_ST.zw;
				// float3 tanToWorld0 = float3( WorldTangent.x, WorldBiTangent.x, WorldNormal.x );
				// float3 tanToWorld1 = float3( WorldTangent.y, WorldBiTangent.y, WorldNormal.y );
				// float3 tanToWorld2 = float3( WorldTangent.z, WorldBiTangent.z, WorldNormal.z );
				// float3 tanNormal15 = UnpackNormalScale( tex2D( _NormalMap, uv_NormalMap ), 1.0f );
				//float3 worldNormal15 = float3(dot(tanToWorld0,tanNormal15), dot(tanToWorld1,tanNormal15), dot(tanToWorld2,tanNormal15));
				float3 worldNormal15 = WorldNormal.xyz;
				float3 switchResult13 = (((ase_vface>0)?(worldNormal15):(-worldNormal15)));
				#ifdef _NORMALFACE_ON
				float3 staticSwitch238 = switchResult13;
				#else
				float3 staticSwitch238 = worldNormal15;
				#endif
				
				//float2 uv_MSAF = IN.ase_texcoord7.xy * _MSAF_ST.xy + _MSAF_ST.zw;
				//float4 tex2DNode16 = tex2D( _MSAF, uv_MSAF );
				float4 tex2DNode16 = float4(1,1,1,1);
				float switchResult27 = (((ase_vface>0)?(( tex2DNode16.g * _FrontSmoothness )):(( tex2DNode16.g * _BackSmoothness ))));
				
				float2 uv_TranslucecyMap = IN.ase_texcoord7.xy * _TranslucecyMap_ST.xy + _TranslucecyMap_ST.zw;
				
				float3 Albedo =  tex2DNode8.rgb;
				//基础色受风场影响
				Albedo += IN.TestOutput.x *_FierceWindTint * 5 * Albedo.rgb;
				float3 Normal = staticSwitch238;
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = ( _Metallic * tex2DNode16.r );
				float Smoothness = clamp(switchResult27,0,1);
				float Occlusion = ( tex2DNode16.b * _Occlusion );
				float Alpha = ( _BaseColor.a * tex2DNode8.a );
				float AlphaClipThreshold = _AlphaClip;
				float AlphaClipThresholdShadow = 0.5;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;//( _TranslucecyColor * tex2D( _TranslucecyMap, uv_TranslucecyMap ) ).rgb;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				inputData.shadowCoord = ShadowCoords;

				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
					inputData.normalWS = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));
					#elif _NORMAL_DROPOFF_OS
					inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
					inputData.normalWS = Normal;
					#endif
					inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				#else
					inputData.normalWS = WorldNormal;
				#endif

				#ifdef ASE_FOG
					inputData.fogCoord = IN.fogFactorAndVertexLight.x;
				#endif

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
				
				float3 SH = IN.lightmapUVOrVertexSH.xyz;

				#ifdef TERRAIN_LIGHTMAP_SAMPLE
					//地表烘焙的lightmap
					float2 worldUv;
				     worldUv.x = (WorldPosition.x - _TerrainWorldInfoMin.x) / (_TerrainWorldInfoMax.x - _TerrainWorldInfoMin.x);
				     worldUv.y = (WorldPosition.z - _TerrainWorldInfoMin.y) / (_TerrainWorldInfoMax.y - _TerrainWorldInfoMin.y);
					//worldUv = (WorldPosition.xz - _TerrainWorldInfo.xy) / _TerrainWorldInfo.zw;
					  // 归一化到0-1的范围
				    worldUv = clamp(worldUv, 0.0, 1.0);
					half4 decodeInstructions = half4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0h, 0.0h);
				    float4 worldGI = SAMPLE_TEXTURE2D_LIGHTMAP(_TerrainWorldLightmap, sampler_TerrainWorldLightmap,worldUv);
					worldGI.rgb = DecodeLightmap(worldGI, decodeInstructions) * _TerrainLM_Strength;
					
					//尝试Bloom的剔除
					half brightness = (worldGI.r +  worldGI.g +  worldGI.b)/3;//Max3(worldGI.r, worldGI.g, worldGI.b);
					half minRGB = 0;//Min3(worldGI.r, worldGI.g, worldGI.b);
					half maxRGB = brightness;
					half delta = maxRGB - minRGB;
					delta = clamp(delta - _TerrainLM_SaturateThreshold /*+ _TerrainLM_SaturateThreshold * 0.5*/,0.0, 1 /*_TerrainLM_SaturateThreshold*/) * _TerrainLM_SaturationIntensity;
					//half softness = clamp(brightness - _TerrainLM_Threshold + _TerrainLM_ThresholdKnee, 0.0, 2.0 * _TerrainLM_ThresholdKnee);
	        
					//softness = (softness * softness) / (4.0 * _TerrainLM_ThresholdKnee + 1e-4);
					half multiplier = max(brightness - _TerrainLM_Threshold  ,delta) / max(brightness, 1e-4);
					worldGI.rgb += worldGI.rgb * multiplier ;
					inputData.bakedGI = worldGI.rgb;
					//return float4(inputData.bakedGI,1);
				 #else
				 	inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
				 #endif
				
				half4 color = UniversalFragmentPBR(inputData, Albedo, Metallic, Specular, Smoothness, Occlusion, Emission, Alpha,_GIStrength,_ShadowStrength);

				

				
				//half4 color = UniversalFragmentBlinnPhong(inputData,Albedo,half4(0.5,0.5,0.5,1),Smoothness,Emission,1,float3(0,0,1));
				//half4 shadowMask = CalculateShadowMask(inputData);
				Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS);
				half3 A = color.rgb;//LinearToGamma20(color.rgb);
				half3 B = _ShadowColor.rgb;//LinearToGamma22(_ShadowColor.rgb) ;
				//柔光模式混合
				half3 shadowColor1 = B<0.5?(A*B*2) : (1.0 - 2.0 * (1.0 - A) * (1.0 - B));
				shadowColor1 = shadowColor1;//Gamma20ToLinear(shadowColor1);
				color.rgb = lerp(shadowColor1, color.rgb , mainLight.shadowAttenuation);
				// #ifdef _TRANSMISSION_ASE
				// {
				// 	float shadow = _TransmissionShadow;
				//
				// 	Light mainLight = GetMainLight( inputData.shadowCoord );
				// 	float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
				// 	mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );
				// 	half3 mainTransmission = max(0 , -dot(inputData.normalWS, mainLight.direction)) * mainAtten * Transmission;
				// 	color.rgb += Albedo * mainTransmission;
				//
				// 	#ifdef _ADDITIONAL_LIGHTS
				// 		int transPixelLightCount = GetAdditionalLightsCount();
				// 		for (int i = 0; i < transPixelLightCount; ++i)
				// 		{
				// 			Light light = GetAdditionalLight(i, inputData.positionWS);
				// 			float3 atten = light.color * light.distanceAttenuation;
				// 			atten = lerp( atten, atten * light.shadowAttenuation, shadow );
				//
				// 			half3 transmission = max(0 , -dot(inputData.normalWS, light.direction)) * atten * Transmission;
				// 			color.rgb += Albedo * transmission;
				// 		}
				// 	#endif
				// }
				// #endif

				// #ifdef _TRANSLUCENCY_ASE
				// {
				// 	float shadow = _TransShadow;
				// 	float normal = _TransNormal;
				// 	float scattering = _TransScattering;
				// 	float direct = _TransDirect;
				// 	float ambient = _TransAmbient;
				// 	float strength = _TransStrength;
				//
				// 	Light mainLight = GetMainLight( inputData.shadowCoord );
				// 	float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
				// 	mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );
				//
				// 	half3 mainLightDir = mainLight.direction + inputData.normalWS * normal;
				// 	half mainVdotL = pow( saturate( dot( inputData.viewDirectionWS, -mainLightDir ) ), scattering );
				// 	half3 mainTranslucency = mainAtten * ( mainVdotL * direct + inputData.bakedGI * ambient ) * Translucency;
				// 	color.rgb += Albedo * mainTranslucency * strength;
				//
				// 	#ifdef _ADDITIONAL_LIGHTS
				// 		int transPixelLightCount = GetAdditionalLightsCount();
				// 		for (int i = 0; i < transPixelLightCount; ++i)
				// 		{
				// 			Light light = GetAdditionalLight(i, inputData.positionWS);
				// 			float3 atten = light.color * light.distanceAttenuation;
				// 			atten = lerp( atten, atten * light.shadowAttenuation, shadow );
				//
				// 			half3 lightDir = light.direction + inputData.normalWS * normal;
				// 			half VdotL = pow( saturate( dot( inputData.viewDirectionWS, -lightDir ) ), scattering );
				// 			half3 translucency = atten * ( VdotL * direct + inputData.bakedGI * ambient ) * Translucency;
				// 			color.rgb += Albedo * translucency * strength;
				// 		}
				// 	#endif
				// }
				// #endif

				#ifdef _REFRACTION_ASE
					float4 projScreenPos = ScreenPos / ScreenPos.w;
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, float4( WorldNormal,0 ) ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos.xy ) * RefractionColor;
					color.rgb = lerp( refraction, color.rgb, color.a );
					color.a = 1;
				#endif

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
					#else
						color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
					#endif
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				color.a =1;

				//自发光添加
				float TopEmission = lerp(1,1-rand*rand, _EmissionRange*2);
				TopEmission = pow((1 - TopEmission),2) * _EmissionStrength;
				float windZoneToEmission /*=lerp(1,saturate(saturate((1 - IN.TestOutput.w))*2 -0.5),_EmissionWindStrength* 8)*/;
				windZoneToEmission =lerp(1,saturate(saturate((IN.TestOutput.w))*2 -0.5),IN.TestOutput.w *_FierceWindTint * 8) ;

					
				color += clamp(color * TopEmission * (windZoneToEmission),0,1);
				
				//[HideInInspector]_ZGAMEURPMicrowaveWindStreng("ZGAMEURPMicrowaveWindStreng", vector) = (1,1,1,1)
				//[HideInInspector]_ZGAMEURPWindStrengthMultipliers("ZGAMEURPWindStrengthMultipliers", vector) = (0.5,0.5,0.5,0.5)
				//_ZGAMEURPWindDirSize("_ZGAMEURPWindDirSize", vector) = (0,0,0,0)
				//color = 1.0 / _ZGAMEURPWindStrengthMultipliers.w;
				return color;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			AlphaToMask Off
			ColorMask 0

			HLSLPROGRAM
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "./../../../ThirdParts/GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing

			
			#define _NORMAL_DROPOFF_WS 1
			#define _TRANSLUCENCY_ASE 1
			//#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 100600

			
			#pragma vertex vert
			#pragma fragment frag
#if ASE_SRP_VERSION >= 110000
			#pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW
#endif
			#define SHADERPASS_SHADOWCASTER

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			//#pragma shader_feature_local _HARDNESSSOURCE_VERTEXCOLOR _HARDNESSSOURCE_VERTEXUV _HARDNESSSOURCE_VERTEXPOSITIONY _HARDNESSSOURCE_TEXSOFT
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShadowDepthInclude.hlsl"

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				//UNITY_VERTEX_OUTPUT_STEREO
			};
            	

			float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
			{
				original -= center;
				float C = cos( angle );
				float S = sin( angle );
				float t = 1 - C;
				float m00 = t * u.x * u.x + C;
				float m01 = t * u.x * u.y - S * u.z;
				float m02 = t * u.x * u.z + S * u.y;
				float m10 = t * u.x * u.y + S * u.z;
				float m11 = t * u.y * u.y + C;
				float m12 = t * u.y * u.z - S * u.x;
				float m20 = t * u.x * u.z - S * u.y;
				float m21 = t * u.y * u.z + S * u.x;
				float m22 = t * u.z * u.z + C;
				float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
				return mul( finalMatrix, original ) + center;
			}
			

			float3 _LightDirection;
#if ASE_SRP_VERSION >= 110000 
			float3 _LightPosition;
#endif
			VertexOutput VertexFunction( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float4 break211 = _ZGAMEURPWindDirSize;
				float3 appendResult213 = (float3(break211.x , 0.0 , break211.z));
				float3 WindDir219 = appendResult213;
				float4 transform224 = mul(GetWorldToObjectMatrix(),float4( WindDir219 , 0.0 ));
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float temp_output_183_0 = ( ( _TimeParameters.x ) * _ZGAMEURPMicrowaveWindStreng.x );
				float2 appendResult194 = (float2(( ase_worldPos.x + ( -_ZGAMEURPWindDirSize.x * temp_output_183_0 ) ) , ( ase_worldPos.z + ( temp_output_183_0 * -_ZGAMEURPWindDirSize.z ) )));
				float SizeInWorldSpace192 = ( 1.0 / _ZGAMEURPWindStrengthMultipliers.w );
				float3 weightedBlendVar205 = float3(0.33,0.33,0.33);
				float weightedBlend205 = ( weightedBlendVar205.x*( tex2Dlod( _ZGAMEURPWindRT, float4( ( appendResult194 * SizeInWorldSpace192 ), 0, 0.0) ).r * _ZGAMEURPMicrowaveWindStreng.y ) + weightedBlendVar205.y*( tex2Dlod( _ZGAMEURPWindRT, float4( ( appendResult194 * SizeInWorldSpace192 ), 0, 0.0) ).g * _ZGAMEURPMicrowaveWindStreng.z ) + weightedBlendVar205.z*( tex2Dlod( _ZGAMEURPWindRT, float4( ( appendResult194 * SizeInWorldSpace192 ), 0, 0.0) ).b * _ZGAMEURPMicrowaveWindStreng.w ) );
				// float2 uv_MSAF = v.ase_texcoord.xy * _MSAF_ST.xy + _MSAF_ST.zw;
				// float4 tex2DNode16 = tex2Dlod( _MSAF, float4( uv_MSAF, 0, 0.0) );
				//
				// float TexSoft96 = tex2DNode16.a;
				#if defined(_HARDNESSSOURCE_VERTEXCOLOR)
				float staticSwitch62 = v.ase_color.r;
				#elif defined(_HARDNESSSOURCE_VERTEXUV)
				float staticSwitch62 = v.ase_texcoord.y;
				#elif defined(_HARDNESSSOURCE_VERTEXPOSITIONY)
				float staticSwitch62 = v.vertex.xyz.y;
				// #elif defined(_HARDNESSSOURCE_TEXSOFT)
				// float staticSwitch62 = TexSoft96;
				#else
				float staticSwitch62 = v.ase_color.r;
				#endif
				float HardnessSource63 = staticSwitch62;
				float Microwave212 = ( sin( weightedBlend205 ) * HardnessSource63 * _MicrowaveIntensity );
				float Foliage210 = _ZGAMEURPWindStrengthMultipliers.y;
				float Grass214 = _ZGAMEURPWindStrengthMultipliers.x;
				float3 rotatedValue227 = RotateAroundAxis( float3( 0,0,0 ), v.vertex.xyz, transform224.xyz, -( ( 0.0 + Microwave212 + ( Foliage210 * HardnessSource63 ) ) * Grass214 ) );
				float3 rotatedValue228 = RotateAroundAxis( float3( 0,0,0 ), rotatedValue227, float3(0,1,0), -1.5 );
				float3 VertexPostion230 = ( rotatedValue228 + float3( 0,0,0 ) );
				
				float3 rotatedValue248 = RotateAroundAxis( float3( 0,0,0 ), v.ase_normal, float3(0,1,0), -1.5 );

				
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = VertexPostion230;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = rotatedValue248;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif
				float3 normalWS = TransformObjectToWorldDir(v.ase_normal);

		#if ASE_SRP_VERSION >= 110000 
			#if _CASTING_PUNCTUAL_LIGHT_SHADOW
				float3 lightDirectionWS = normalize(_LightPosition - positionWS);
			#else
				float3 lightDirectionWS = _LightDirection;
			#endif
				float4 clipPos = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
			#if UNITY_REVERSED_Z
				clipPos.z = min(clipPos.z, UNITY_NEAR_CLIP_VALUE);
			#else
				clipPos.z = max(clipPos.z, UNITY_NEAR_CLIP_VALUE);
			#endif
		#else
				float4 clipPos = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
			#if UNITY_REVERSED_Z
				clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
			#else
				clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
			#endif
		#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = clipPos;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_color = v.ase_color;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			half4 frag(	VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				//UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );
				
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				
				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif
				
				float Alpha = ( _BaseColor.a  );
				float AlphaClipThreshold = _AlphaClip;
				float AlphaClipThresholdShadow = 0.5;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					#ifdef _ALPHATEST_SHADOW_ON
						clip(Alpha - AlphaClipThresholdShadow);
					#else
						clip(Alpha - AlphaClipThreshold);
					#endif
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif
				return ShadowFrag(IN.clipPos);
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0
			AlphaToMask Off

			HLSLPROGRAM
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "./../../../ThirdParts/GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing

			
			#define _NORMAL_DROPOFF_WS 1
			#define _TRANSLUCENCY_ASE 1
			//#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 100600

			
			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			//#pragma shader_feature_local _HARDNESSSOURCE_VERTEXCOLOR _HARDNESSSOURCE_VERTEXUV _HARDNESSSOURCE_VERTEXPOSITIONY _HARDNESSSOURCE_TEXSOFT


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				//UNITY_VERTEX_OUTPUT_STEREO
			};
				
			

			float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
			{
				original -= center;
				float C = cos( angle );
				float S = sin( angle );
				float t = 1 - C;
				float m00 = t * u.x * u.x + C;
				float m01 = t * u.x * u.y - S * u.z;
				float m02 = t * u.x * u.z + S * u.y;
				float m10 = t * u.x * u.y + S * u.z;
				float m11 = t * u.y * u.y + C;
				float m12 = t * u.y * u.z - S * u.x;
				float m20 = t * u.x * u.z - S * u.y;
				float m21 = t * u.y * u.z + S * u.x;
				float m22 = t * u.z * u.z + C;
				float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
				return mul( finalMatrix, original ) + center;
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				
				float4 break211 = _ZGAMEURPWindDirSize;
				float3 appendResult213 = (float3(break211.x , 0.0 , break211.z));
				float3 WindDir219 = appendResult213;
				float4 transform224 = mul(GetWorldToObjectMatrix(),float4( WindDir219 , 0.0 ));
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float temp_output_183_0 = ( ( _TimeParameters.x ) * _ZGAMEURPMicrowaveWindStreng.x );
				float2 appendResult194 = (float2(( ase_worldPos.x + ( -_ZGAMEURPWindDirSize.x * temp_output_183_0 ) ) , ( ase_worldPos.z + ( temp_output_183_0 * -_ZGAMEURPWindDirSize.z ) )));
				float SizeInWorldSpace192 = ( 1.0 / _ZGAMEURPWindStrengthMultipliers.w );
				float3 weightedBlendVar205 = float3(0.33,0.33,0.33);
				float weightedBlend205 = ( weightedBlendVar205.x*( tex2Dlod( _ZGAMEURPWindRT, float4( ( appendResult194 * SizeInWorldSpace192 ), 0, 0.0) ).r * _ZGAMEURPMicrowaveWindStreng.y ) + weightedBlendVar205.y*( tex2Dlod( _ZGAMEURPWindRT, float4( ( appendResult194 * SizeInWorldSpace192 ), 0, 0.0) ).g * _ZGAMEURPMicrowaveWindStreng.z ) + weightedBlendVar205.z*( tex2Dlod( _ZGAMEURPWindRT, float4( ( appendResult194 * SizeInWorldSpace192 ), 0, 0.0) ).b * _ZGAMEURPMicrowaveWindStreng.w ) );
				//float2 uv_MSAF = v.ase_texcoord.xy * _MSAF_ST.xy + _MSAF_ST.zw;
				//float4 tex2DNode16 = tex2Dlod( _MSAF, float4( uv_MSAF, 0, 0.0) );
				//float TexSoft96 = tex2DNode16.a;
				#if defined(_HARDNESSSOURCE_VERTEXCOLOR)
				float staticSwitch62 = v.ase_color.r;
				#elif defined(_HARDNESSSOURCE_VERTEXUV)
				float staticSwitch62 = v.ase_texcoord.y;
				#elif defined(_HARDNESSSOURCE_VERTEXPOSITIONY)
				float staticSwitch62 = v.vertex.xyz.y;
				// #elif defined(_HARDNESSSOURCE_TEXSOFT)
				// float staticSwitch62 = TexSoft96;
				#else
				float staticSwitch62 = v.ase_color.r;
				#endif
				float HardnessSource63 = staticSwitch62;
				float Microwave212 = ( sin( weightedBlend205 ) * HardnessSource63 * _MicrowaveIntensity );
				float Foliage210 = _ZGAMEURPWindStrengthMultipliers.y;
				float Grass214 = _ZGAMEURPWindStrengthMultipliers.x;
				float3 rotatedValue227 = RotateAroundAxis( float3( 0,0,0 ), v.vertex.xyz, transform224.xyz, -( ( 0.0 + Microwave212 + ( Foliage210 * HardnessSource63 ) ) * Grass214 ) );
				float3 rotatedValue228 = RotateAroundAxis( float3( 0,0,0 ), rotatedValue227, float3(0,1,0), -1.5 );
				float3 VertexPostion230 = ( rotatedValue228 + float3( 0,0,0 ) );
				
				float3 rotatedValue248 = RotateAroundAxis( float3( 0,0,0 ), v.ase_normal, float3(0,1,0), -1.5 );
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = VertexPostion230;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = rotatedValue248;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_color = v.ase_color;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif
			half4 frag(	VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				//UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				// float2 uv_BaseMap = IN.ase_texcoord2.xy * _BaseMap_ST.xy + _BaseMap_ST.zw;
				// float4 tex2DNode8 = tex2D( _BaseMap, uv_BaseMap );
				
				float Alpha = ( _BaseColor.a /** tex2DNode8.a*/ );
				float AlphaClipThreshold = _AlphaClip;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				#ifdef ASE_DEPTH_WRITE_ON
				outputDepth = DepthValue;
				#endif

				return 0;
			}
			ENDHLSL
		}
	}	
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"	
}


