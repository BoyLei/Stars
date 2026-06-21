Shader "SGAME/Scene/SimpleSea"
{
	Properties
	{
		
		[Header(BaseColor)]
		
		_DeepColor("深水颜色", Color) = (0.03,0.32,0.4,0.7)
		_ShallowColor("浅水颜色", Color) = (0,0.6,0.52,0)
		
		[Header(Setting)]
		_DirectionParm("运动参数设置(xy为方向,z为速度)",Vector) = (1,1,1,0)
		_Smoothness("光滑度",Range(0,1)) = 0.5
		//        [Header(Noise)]
		//        _NoiseTex ("噪声贴图", 2D) = "white" {}
		//        _NoiseRange("噪声范围",Range( 0 , 1)) = 0.5
		
		[Header(Offset)]
		_MoveOffsetScale("偏移幅度",Range(0,3)) = 1
		
		[Header(Depth)]
		//_Depthdistance("水和其他物件的接触距离", Range( 0 , 10)) = 0.86
		_ColorDistance("深水浅水颜色控制,越高则贴近其他物体的水体更加偏向深水颜色", Range( 0 , 10)) = 1
		_AlphaDistance("AlphaDistance", Range( 0 ,1)) =1
		
		[Header(Spray)]
		_SprayRange("岸边水花的位置控制",Range(0.01,1)) = 0.98
		_SprayColor("岸边水花的颜色",Color) = (1,1,1,1)
		
		[Header(Normal)]
		_NormalTex("法线贴图",2D) = "white"{}
		_NormalStrength("法线强度",range(0,2)) = 1
		//        _DetailNormalTex("细节法线贴图",2D) = "white"{}
		//        _DetailStrength("细节法线强度",range(0,2)) = 1
		
		[Header(SHADOW)]
		[KeywordEnum(ON,OFF)]_RECEIVE_SHADOWS("是否接受投影",Float) = 0
		_ShadowStrenght("投影的强度",Range(0,1)) = 1
		
		[Header(Foam)]
		_FoamTex("波纹贴图", 2D) = "black" {}
		_FoamDensity("波纹密度",Range(0,3)) = 1
		_FoamColor("波纹颜色(RGB控制颜色,A控制透明度)", Color) = (1,1,1,0.8)
		
		
	}
	SubShader
	{
		

		Pass
		{
			Tags { "RenderPipeline"="UniversalPipeline" 
			"Queue" = "Transparent"}
			//LOD 100
			Cull OFF
			Blend SrcAlpha OneMinusSrcAlpha
			Zwrite OFF
			//            Stencil
			//            {
				//                Ref 1
				//                Comp Greater
				//                Pass Keep
				//                //Fail IncrSat
			//            }
			
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			// Universal Pipeline keywords
			#pragma multi_compile _RECEIVE_SHADOWS_ON _RECEIVE_SHADOWS_OFF
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			//#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			//#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			// #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
			//#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
			#pragma multi_compile_fragment _ _SHADOWS_SOFT
			#pragma multi_compile _ LIGHTMAP_ON
			//#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile_fragment _ _LIGHT_LAYERS
			#pragma multi_compile_fragment _ _RENDER_PASS_ENABLED

			#pragma shader_feature _ _SAMPLE_GI
			
			#define MAIN_LIGHT_CALCULATE_SHADOWS
			

			
			

			
			
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include  "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

			CBUFFER_START(UnityPerMaterial)
				float _ColorDistance;
				float4 _DeepColor;
				float4 _ShallowColor;

				float4 _DirectionParm;
				float _Smoothness;
				
				float _Depthdistance;
				float _AlphaDistance;
				float _NoiseRange;
				float _MoveOffsetScale;

				float _SprayRange;
				float4 _SprayColor;

				float _ShadowStrenght;

				float _NormalStrength;
				float _DetailStrength;
				float4 _NormalTex_ST;
				float4 _DetailNormalTex_ST;

				float4 _NoiseTex_ST;
				float4 _FoamTex_ST;
				float _FoamDensity;
				float4 _FoamColor;
			CBUFFER_END

			TEXTURE2D(_NoiseTex);   SAMPLER(sampler_NoiseTex); 
			TEXTURE2D(_MaskTex);   SAMPLER(sampler_MaskTex);
			TEXTURE2D(_MaskTex2);   SAMPLER(sampler_MaskTex2);
			TEXTURE2D(_FoamTex);   SAMPLER(sampler_FoamTex);
			TEXTURE2D(_NormalTex);   SAMPLER(sampler_NormalTex);
			TEXTURE2D(_DetailNormalTex);   SAMPLER(sampler_DetailNormalTex);
			float4 _MaskTex_ST;
			struct Attributes
			{
				float4 positionOS : POSITION;
				float4 texcoord : TEXCOORD0;
				float2 texcoord2: TEXCOORD1;
				float2 texcoord3 : TEXCOORD2;
				float4 tangentOS :  TANGENT;
				float3 normalOS : NORMAL;
				half4 vertColor : COLOR;
			};

			struct Varyings
			{
				float4 uv : TEXCOORD0;
				float4 positionCS : SV_POSITION;
				float4 positionWS :TEXCOORD1;
				float4 normalWS   :TEXCOORD2;
				float4 tangentWS : TEXCOORD3;
				float4 bioTangentWS : TEXCOORD4;
				float4 positionOS :TEXCOORD5;
				float4 screenPos  :TEXCOORD6;
				float4 shadowCoord : TEXCOORD7;
				
			};
			
			float3 GetCameraWorldDirection()
			{
				return unity_CameraToWorld._m02_m12_m22;
			}
			half ComputeFogFactorSimpleSea(float zPositionCS, float2 FogParams)//强制以linear形式返回雾气z
			{
				float clipZ_0Far = UNITY_Z_0_FAR_FROM_CLIPSPACE(zPositionCS);
				float fogFactor = saturate(clipZ_0Far * FogParams.x + FogParams.y);
				return fogFactor;
			}
			
			Varyings vert (Attributes v)
			{
				Varyings o = (Varyings)0;

				
				v.positionOS += _SinTime.z * 0.1 * _MoveOffsetScale;
				o.positionCS = TransformObjectToHClip(v.positionOS);
				o.positionWS.xyz = TransformObjectToWorld(v.positionOS);
				o.uv.xy = v.texcoord.xy;
				//o.uv.z = ComputeFogFactor(o.positionCS.z);    //uv的z值存储雾.
				float End =180;
				float Start = 0;
				float2 FogParams = float2(-1/(End-Start), End/(End-Start));
				o.uv.z = ComputeFogFactorSimpleSea(o.positionCS.z,FogParams);    //uv的z值存储雾.
				o.screenPos = ComputeScreenPos(o.positionCS );

				//法线
				o.normalWS.xyz = TransformObjectToWorldNormal(v.normalOS.xyz);		//世界法线
				o.tangentWS.xyz = 1.0 / length(o.normalWS.xyz )  * normalize(TransformObjectToWorldDir(v.tangentOS.xyz));		//转切线空间
				o.tangentWS.a = v.tangentOS.a;
				float crossSign = (o.tangentWS.a > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
				float3 bitang = crossSign * cross(o.normalWS.xyz, o.tangentWS.xyz);
				o.bioTangentWS.xyz =1.0 / length(o.normalWS.xyz )  *bitang;
				//o.shadowCoord = TransformWorldToShadowCoord(o.positionWS);

				//UNITY_TRANSFER_FOG(o,o.pos);
				return o;
				
				
				return o;
			}


			//法线强化函数
			void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
			{
				Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
			}
			//uv unpack
			float4 PackedUV(float2 sourceUV, float2 time, float2 flowmap, float speed)
			{
				#if _RIVER
					time *= flowmap;
					time.x = 0; //Only move in forward direction
				#endif
				
				float2 uv1 = sourceUV.xy + (time.xy * speed);
				#ifndef _RIVER
					//Second UV, 2x larger, twice as slow, in opposite direction
					float2 uv2 = (sourceUV.xy * 0.5) + ((1 - time.xy) * speed * 0.5);
				#else
					//2x larger, same direction/speed
					float2 uv2 = (sourceUV.xy * 0.5) + (time.xy * speed);
				#endif

				return float4(uv1.xy, uv2.xy);
			}
			//法线混合
			float3 SampleNormals(float2 uv, float3 wPos, float2 time, float2 flowmap, float speed, float slope) 
			{
				float4 uvs = PackedUV(uv, time, flowmap, speed);
				float3 n1 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, uvs.xy));
				float3 n2 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, uvs.zw));

				float3 blendedNormals = BlendNormal(n1, n2);

				#if _DISTANCE_NORMALS
					float pixelDist = length(_WorldSpaceCameraPos.xzz - wPos.xyz);
					float fadeFactor = saturate((_DistanceNormalParams.y - pixelDist) / (_DistanceNormalParams.y-_DistanceNormalParams.x));

					float3 largeBlendedNormals;
					
					uvs = PackedUV(uv * _DistanceNormalParams.z, time, flowmap, speed * 0.5);
					float3 n1b = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMapLarge, sampler_BumpMapLarge, uvs.xy));
					
					#if _ADVANCED_SHADING //Use 2nd texture sample
						float3 n2b = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMapLarge, sampler_BumpMapLarge, uvs.zw));
						largeBlendedNormals = BlendTangentNormals(n1b, n2b);
					#else
						largeBlendedNormals = n1b;
					#endif
					
					blendedNormals = lerp(largeBlendedNormals, blendedNormals, fadeFactor);
				#endif
				
				#if _RIVER
					uvs = PackedUV(uv, time, flowmap, speed * _SlopeParams.y);
					uvs.xy = uvs.xy * float2(1, 1-_SlopeParams.x);
					float3 n3 = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uvs.xy));

					blendedNormals = lerp(n3, blendedNormals, slope);
				#endif

				#ifdef WAVE_SIMULATION
					BlendWaveSimulation(wPos, blendedNormals);
				#endif
				
				return blendedNormals;
			}
			//强制雾气混合
			half3 MixFogColorSimpleSea(float3 fragColor, float fogFactor)
			{
				float fogIntensity = fogFactor;//ComputeFogIntensity(fogFactor);
				fragColor = lerp( unity_FogColor.rgb, fragColor, fogIntensity);
				//fragColor = half3(fogIntensity,fogIntensity,fogIntensity);
				return fragColor;
			}
			//强制BoxProjection反射
			half3 GlossyEnvironmentReflectionSimpleSea(half3 reflectVector, float3 positionWS, half perceptualRoughness, half occlusion)
			{
				#if !defined(_ENVIRONMENTREFLECTIONS_OFF)
					half3 irradiance;

					#ifdef _REFLECTION_PROBE_BLENDING
						//irradiance = CalculateIrradianceFromReflectionProbes(reflectVector, positionWS, perceptualRoughness);
					#else
						//#ifdef _REFLECTION_PROBE_BOX_PROJECTION
						// reflectVector = BoxProjectedCubemapDirection(reflectVector, positionWS, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
						//#endif // _REFLECTION_PROBE_BOX_PROJECTION
						half mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
						half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector, mip));

						#if defined(UNITY_USE_NATIVE_HDR)
							irradiance = encodedIrradiance.rgb;
						#else
							irradiance = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
						#endif // UNITY_USE_NATIVE_HDR
					#endif // _REFLECTION_PROBE_BLENDING
					return irradiance * occlusion;
				#else
					return _GlossyEnvironmentColor.rgb * occlusion;
				#endif // _ENVIRONMENTREFLECTIONS_OFF
			}

			//深度结构体
			struct SceneDepth
			{
				float raw;
				float linear01;
				float eye;
			};
			//深度
			SceneDepth SampleDepth(float4 screenPos)
			{
				SceneDepth depth = (SceneDepth)0;
				
				#ifndef _DISABLE_DEPTH_TEX
					screenPos.xyz /= screenPos.w;

					depth.raw = SampleSceneDepth(screenPos.xy);
					depth.eye = LinearEyeDepth(depth.raw, _ZBufferParams);
					depth.linear01 = Linear01Depth(screenPos.z, depth.eye);
				#else
					depth.raw = 1.0;
					depth.eye = 1.0;
					depth.linear01 = 1.0;
				#endif

				return depth;
			}
			//Reconstruct view-space position from depth.
			float3 ReconstructViewPos(float4 screenPos, float3 viewDir, SceneDepth sceneDepth)
			{
				#if UNITY_REVERSED_Z
					real rawDepth = sceneDepth.raw;
				#else
					// Adjust z to match NDC for OpenGL
					real rawDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, sceneDepth.raw);
				#endif
				
				#if defined(ORTHOGRAPHIC_SUPPORT)
					//View to world position
					float4 viewPos = float4((screenPos.xy/screenPos.w) * 2.0 - 1.0, rawDepth, 1.0);
					float4x4 viewToWorld = UNITY_MATRIX_I_VP;
					#if UNITY_REVERSED_Z //Wrecked since 7.3.1 "fix" and causes warping, invert second row https://issuetracker.unity3d.com/issues/shadergraph-inverse-view-projection-transformation-matrix-is-not-the-inverse-of-view-projection-transformation-matrix
						//Commit https://github.com/Unity-Technologies/Graphics/pull/374/files
						viewToWorld._12_22_32_42 = -viewToWorld._12_22_32_42;              
					#endif
					float4 viewWorld = mul(viewToWorld, viewPos);
					float3 viewWorldPos = viewWorld.xyz / viewWorld.w;
				#endif

				//Projection to world position
				float3 camPos = _WorldSpaceCameraPos.xyz;
				float3 worldPos = sceneDepth.eye * (viewDir/screenPos.w) - camPos;
				float3 perspWorldPos = -worldPos;

				#if defined(ORTHOGRAPHIC_SUPPORT)
					return lerp(perspWorldPos, viewWorldPos, unity_OrthoParams.w);
				#else
					return perspWorldPos;
				#endif

			}
			float DepthDistance(float3 wPos, float3 viewPos, float3 normal)
			{
				return length((wPos - viewPos) * normal);
			}
			
			half4 frag (Varyings i) : SV_Target
			{
				half4 ScreenPos = i.screenPos;
				// half2 ScreenUV = GetNormalizedScreenSpaceUV(i.positionCS);
				// half2 TexUV = float2(i.positionCS.x * _ScaleX/10,i.positionCS.y * _ScaleY/10) - float2(_SpeedX*_Time.x, _SpeedY*_Time.x);
				half4 col = float4(1,1,1,1);
				float4 ase_screenPosNorm = ScreenPos / ScreenPos.w;
				// ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				//             float screenDepth53 = LinearEyeDepth(SampleSceneDepth(ase_screenPosNorm),_ZBufferParams);
				// float distanceDepth53 = abs( ( screenDepth53 - LinearEyeDepth( ase_screenPosNorm.z,_ZBufferParams ) ) / ( _ColorDistance ) );
				//             float distanceDepth54 = smoothstep(0,1,distanceDepth53 / _AlphaDistance);
				// float smoothstepResult62 = smoothstep( 0.0 , 1.0 , distanceDepth53);
				// 	float4 lerpResult52 = lerp( _DeepColor , _ShallowColor , smoothstepResult62);

				//数据准备 1 
				float3 viewDir = (_WorldSpaceCameraPos - i.positionWS.xyz);
				float3 viewDirNorm = SafeNormalize(viewDir);
				float nDotV = dot(viewDirNorm,i.normalWS);
				
				//重新计算深度
				SceneDepth depth = SampleDepth(ScreenPos);
				float3 opaqueWorldPos = ReconstructViewPos(ScreenPos, viewDir, depth);
				float normalSign = ceil(dot(viewDirNorm, i.normalWS));
				normalSign = normalSign == 0 ? -1 : 1;
				float opaqueDist = DepthDistance(i.positionWS, opaqueWorldPos,  i.normalWS * normalSign);

				float _DepthHorizontal = _ColorDistance;
				float _DepthExp =1;		//和opaque的距离*距离偏向颜色
				float heightAttenuation = saturate(lerp(opaqueDist * _DepthHorizontal, 1.0 - exp(-opaqueDist * _DepthHorizontal), _DepthExp));
				float distanceDepth54 = heightAttenuation;
				float smoothstepResult62 = heightAttenuation;

				float4 lerpResult52 = lerp( _DeepColor , _ShallowColor ,smoothstepResult62);
				lerpResult52 = lerp( float4(0,0,0,1) , float4(1,1,1,1) ,smoothstepResult62);
				//return float4(lerpResult52.rgb,1);
				
				lerpResult52 = lerp( _DeepColor , _ShallowColor , smoothstepResult62);
				
				// 法线
				half3 normal;
				//half4 normalTex = SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,i.uv.xy);
				//normalTex.xyz = UnpackNormal(normalTex);
				//Unity_NormalStrength_float(normalTex.xyz, _NormalStrength, normal.xyz);
				//Unity_NormalStrength_float(normalTex.xyz, _DetailNormalStrength, detailNormal.xyz);

				//混合法线
				float2 NormalUV = i.uv.xy * _NormalTex_ST.xy + _NormalTex_ST.zw;
				float2 Time = (( _DirectionParm.z) * _DirectionParm.xy) * _Time.z;
				float3 BlendNormal = SampleNormals(NormalUV, i.positionWS, Time, 1, 0.2, 0) ;
				float normalStrength = lerp(0.2, _NormalStrength,smoothstepResult62)*nDotV ;
				
				//return float4(distanceDepth54,distanceDepth54,distanceDepth54,1);
				Unity_NormalStrength_float(BlendNormal.xyz, normalStrength, BlendNormal.xyz);
				//return float4(BlendNormal,1);
				//法线切线空间转世界空间
				normal = TransformTangentToWorld(BlendNormal.xyz,half3x3( i.tangentWS.xyz,i.bioTangentWS.xyz,i.normalWS.xyz));
				//detailNormal = TransformTangentToWorld(detailNormal.xyz,half3x3( i.tangentWS.xyz,i.bioTangentWS.xyz,i.normalWS.xyz));
				normal = normalize(normal);

				//数据准备 2 
				float NoV = dot(viewDirNorm,normal);
				
				//return float4(NoV,NoV,NoV,1);
				
				//水花
				float distanceDepth2 = depth.eye - LinearEyeDepth( ase_screenPosNorm.z,_ZBufferParams );
				float sprayRange = _SprayRange;
				float Spray1 = 1 - smoothstep(sprayRange,sprayRange + 0.007,1 - clamp(distanceDepth2/2,0,1));
				float Spray2 = 1 - smoothstep(sprayRange-0.05,sprayRange -0.05 + 0.007,1 - clamp(distanceDepth2/2,0,1));
				float Spray3 = clamp((Spray1 - Spray2),0,1);
				//岸边水花
				float Spray4 = 1 - smoothstep(sprayRange,sprayRange + 0.07,1 - clamp(distanceDepth2/2,0,1));
				float SeaSideRange =1 - (smoothstep(0.01,1,distanceDepth54) + (1 - clamp(0,1,distanceDepth54*5)))  ;
				//return float4(SeaSideRange,SeaSideRange,SeaSideRange,1);
				//return float4((1 - clamp(distanceDepth2/2,0,1)),0,0,1);
				

				
				//return float4(normal,1);

				//噪声
				// float2 NoiseUV = i.uv.xy * _NoiseTex_ST.xy + _NoiseTex_ST.zw + float2(-_Time.x,-_Time.x);
				// float4 Noise = SAMPLE_TEXTURE2D(_NoiseTex,sampler_NoiseTex,NoiseUV);
				// _NoiseRange = _NoiseRange/5;
				// float NoiseFade = smoothstep(_NoiseRange,_NoiseRange + 0.06,Noise.x);
				//return float4(NoiseFade,NoiseFade,NoiseFade,1);
				//波纹
				float2 foamUV = i.uv.xy * _FoamTex_ST.xy + _FoamTex_ST.zw + float2(_Time.x,_Time.x);
				float2 foamUV2 = i.uv.xy * _FoamTex_ST.xy *0.5 + _FoamTex_ST.zw - float2(_Time.x,_Time.x) ;
				float4 FoamTex = SAMPLE_TEXTURE2D(_FoamTex,sampler_FoamTex,foamUV);
				float4 FoamTex2 = SAMPLE_TEXTURE2D(_FoamTex,sampler_FoamTex,foamUV2);
				float Foam = saturate(FoamTex.r + FoamTex2.r);
				Foam = smoothstep(0.9 - _FoamDensity* 0.25, 1.0, Foam);

				//主体波纹
				float MainFoam = Foam * pow(distanceDepth54,3) * (1 - SeaSideRange);
				//岸边波纹
				float Foam2 = smoothstep(distanceDepth54 *0.7,distanceDepth54*0.7 + 0.1,FoamTex.b *2 + FoamTex2.g);
				float SeaSideFoam = Foam2 * SeaSideRange;
				//return float4(SeaSideFoam,SeaSideFoam,SeaSideFoam,1);

				//波纹和基础色混合
				
				//col.rgb = lerp(lerpResult52.rgb, _SprayColor.rgb * _SprayColor.a + lerpResult52.rgb * (1 - _SprayColor.a), Spray3);
				col.rgb  =lerpResult52.rgb;
				col.rgb = _FoamColor.rgb * (_FoamColor.a*MainFoam )+ col.rgb * (1 - _FoamColor.a*MainFoam);
				col.rgb = _SprayColor.rgb * (_SprayColor.a*SeaSideFoam )+ col.rgb * (1 - _SprayColor.a*SeaSideFoam);
				InputData inputData = (InputData)0;
				inputData.positionWS = i.positionWS;
				inputData.positionCS = i.positionCS;
				inputData.normalWS = i.normalWS;
				inputData.shadowCoord = float4(0, 0, 0, 0);
				//Light mainLight = GetMainLight(i.shadowCoord);
				AmbientOcclusionFactor aoFactor = (AmbientOcclusionFactor)0;
				
				#ifndef _RECEIVE_SHADOWS_OFF 
					float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS);
					Light mainLight = GetMainLight(shadowCoord, inputData.positionWS, half4(1,1,1,1));
					//return float4(mainLight.shadowAttenuation,mainLight.shadowAttenuation,mainLight.shadowAttenuation,1);
					//col.rgb = UniversalFragmentBlinnPhong(inputData, col.rgb, half4(1,1,1,1),0,half3(0,0,0),1,half3(0,0,1));
					col.rgb *= clamp(0,1,mainLight.shadowAttenuation + 0.4 + 1- _ShadowStrenght);
				#endif

				//倒影
				
				float3 reflectionVector = reflect(-viewDirNorm , normal);
				//float3 probe = saturate(GlossyEnvironmentReflection(reflectionVector, i.positionWS, 0, 1.0)).rgb;
				//float3 probe = saturate(GlossyEnvironmentReflectionSimpleSea(reflectionVector, i.positionWS, 0, 1.0)).rgb;
				float3 probe = GlossyEnvironmentReflection(reflectionVector,  i.positionWS, 0, 1.0).rgb;


				col.rgb += probe*(0.1 + (1-NoV)*(1-NoV)*1) ;

				//菲涅尔


				//         	half3 LightingSpecular(half3 lightColor, half3 lightDir, half3 normal, half3 viewDir, half4 specular, half smoothness)
				// {
					//     float3 halfVec = SafeNormalize(float3(lightDir) + float3(viewDir));
					//     half NdotH = half(saturate(dot(normal, halfVec)));
					//     half modifier = pow(NdotH, smoothness);
					//     half3 specularReflection = specular.rgb * modifier;
					//     return lightColor * specularReflection;
				// }
				//拆出来的BlingPhong高光
				// float3 specular = LightingSpecular(mainLight.color, mainLight.direction, normal, inputData.viewDirectionWS, half4(1,1,1, 1), _Smoothness);
				// float3 halfVec = SafeNormalize(float3(mainLight.direction) + float3(viewDir));
				// half NdotH = half(saturate(dot(normal, halfVec)));
				// half modifier = pow(NdotH, _Smoothness);
				// half3 specularReflection = specular.rgb * modifier;
				//return float4(specularReflection,1);
				

				float depthModified = clamp(0,1,(i.uv.z + (1-NoV) * 0.1)*1 + pow(i.uv.z,3) * smoothstepResult62);
				//float depth2 =pow(i.uv.z,3) * smoothstepResult62;
				col.rgb = MixFogColorSimpleSea(col.rgb, depthModified);
				//return float4(depth2,depth2,depth2,1);
				//col.rgb = MixFog(col.rgb, i.uv.z);
				col.a *= clamp(0,1,distanceDepth54*2);
				//col.a += Spray3 * _SprayColor.a;
				col.a += SeaSideFoam *_SprayColor.a * 2;
				//col.a = clamp(0,1,col.a );
				return col;
				return col;
			}
			ENDHLSL
		}
	}
	 FallBack "Hidden/InternalErrorShader"
}
