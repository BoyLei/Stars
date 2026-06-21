
#ifndef WATER_SSR
#define WATER_SSR
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

#include "./WaterStruct.hlsl"
//
//


	float UVJitter(in float2 uv)
	{
		return frac((52.9829189 * frac(dot(uv, float2(0.06711056, 0.00583715)))));
	}

	void SSRRayConvert(float3 worldPos, out float4 clipPos, out float3 screenPos, out float2 grabPos)
	{
		clipPos = TransformWorldToHClip(worldPos);
		float k = ((1.0) / (clipPos.w));

		screenPos.xy = ComputeScreenPos(clipPos).xy * k;
		screenPos.z = k;

		//grabPos = ComputeGrabScreenPos(clipPos).xy * k;
		grabPos = ComputeScreenPos(clipPos).xy * k;
	}

	float3 SSRRayMarch(BGWaterVertexOutput vertexOutput, BGLightingData lightingData,float SSRMaxSampleCount,float SSRSampleStep)
	{
		float4 startClipPos;
		float3 startScreenPos;
		float2 startGrabPos;

		SSRRayConvert(lightingData.worldPos, startClipPos, startScreenPos, startGrabPos);

		float4 endClipPos;
		float3 endScreenPos;
		float2 endGrabPos;

		SSRRayConvert(lightingData.worldPos + lightingData.R, endClipPos, endScreenPos, endGrabPos);

		if (((endClipPos.w) < (startClipPos.w)))
		{
			return float3(0, 0, 0);
		}

		float3 screenDir = endScreenPos - startScreenPos;
		float2 grabDir = endGrabPos - startGrabPos;

		float screenDirX = abs(screenDir.x);
		float screenDirY = abs(screenDir.y);

		float dirMultiplier = lerp( 1 / (_ScreenParams.y * screenDirY), 1 / (_ScreenParams.x * screenDirX), screenDirX > screenDirY ) * SSRSampleStep;

		screenDir *= dirMultiplier;
		grabDir *= dirMultiplier;

		half lastRayDepth = startClipPos.w;

		half sampleCount = 1 + UVJitter(vertexOutput.pos) * 0.1;

		float lastDeltaDepth = 0;

#if defined (SHADER_API_OPENGL) || defined (SHADER_API_D3D11) || defined (SHADER_API_D3D12)
		[unroll(64)]
#else
		UNITY_LOOP
#endif
		for(int i = 0; i < SSRMaxSampleCount; i++)
		{
			float3 screenMarchUVZ = startScreenPos + screenDir * sampleCount;

			if((screenMarchUVZ.x <= 0) || (screenMarchUVZ.x >= 1) || (screenMarchUVZ.y <= 0) || (screenMarchUVZ.y >= 1))
			{
				break;
			}

			//float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenMarchUVZ.xy));
			float sceneDepth =LinearEyeDepth(SampleSceneDepth(screenMarchUVZ.xy),_ZBufferParams);
			half rayDepth = 1.0 / screenMarchUVZ.z;
			half deltaDepth = rayDepth - sceneDepth;

			if((deltaDepth > 0) && (sceneDepth > startClipPos.w) && (deltaDepth < (abs(rayDepth - lastRayDepth) * 2)))
			{
				float samplePercent = saturate(lastDeltaDepth / (lastDeltaDepth - deltaDepth));
				samplePercent = lerp(samplePercent, 1, rayDepth >= _ProjectionParams.z);
				float hitSampleCount = lerp(sampleCount-1, sampleCount, samplePercent);
				return float3(startGrabPos + grabDir * hitSampleCount, 1);
			}

			lastRayDepth = rayDepth;
			sampleCount += 1;

			lastDeltaDepth = deltaDepth;
		}

		float4 farClipPos;
		float3 farScreenPos;
		float2 farGrabPos;

		SSRRayConvert(lightingData.worldPos + lightingData.R * 100000, farClipPos, farScreenPos, farGrabPos);

		if((farScreenPos.x > 0) && (farScreenPos.x < 1) && (farScreenPos.y > 0) && (farScreenPos.y < 1))
		{
			//float farDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, farScreenPos.xy));
			float farDepth = LinearEyeDepth(SampleSceneDepth(farScreenPos.xy),_ZBufferParams);

			if(farDepth > startClipPos.w)
			{
				return float3(farGrabPos, 1);
			}
		}

		return float3(0, 0, 0);
	}

	float3 GetSSRUVZ(BGWaterVertexOutput vertexOutput, BGLightingData lightingData,float SSRMaxSampleCount,float SSRSampleStep)
	{
		#if defined(UNITY_SINGLE_PASS_STEREO)
			half ssrWeight = 1;

			half NoV = lightingData.NoV * 2;
			ssrWeight *= (1 - NoV * NoV);
		#else
			float screenUV = lightingData.screenUV * 2 - 1;
			screenUV *= screenUV;

			half ssrWeight = saturate(1 - dot(screenUV, screenUV));

			half NoV = lightingData.NoV * 2.5;
			ssrWeight *= 1;//(1 - NoV * NoV);
			//return float3(ssrWeight,ssrWeight,ssrWeight);
		#endif

		if (ssrWeight > 0.005)
		{
			float3 uvz = SSRRayMarch(vertexOutput, lightingData, SSRMaxSampleCount, SSRSampleStep);
			uvz.z *= ssrWeight;
			return uvz;
		}

		return float3(0, 0, 0);
	}


//The fuction no need structure
float3 SSRRayMarch_NoStruct(/*BGWaterVertexOutput vertexOutput,*/ /*BGLightingData lightingData,*/float3 clipPos, float3 worldPos, float3 reflectDir, float SSRMaxSampleCount,float SSRSampleStep)
	{
		float4 startClipPos;
		float3 startScreenPos;
		float2 startGrabPos;

		//SSRRayConvert(lightingData.worldPos, startClipPos, startScreenPos, startGrabPos);
		SSRRayConvert(worldPos, startClipPos, startScreenPos, startGrabPos);

		float4 endClipPos;
		float3 endScreenPos;
		float2 endGrabPos;

		//SSRRayConvert(lightingData.worldPos + lightingData.R, endClipPos, endScreenPos, endGrabPos);
		SSRRayConvert(worldPos+reflectDir, endClipPos, endScreenPos, endGrabPos);

		if (((endClipPos.w) < (startClipPos.w)))
		{
			return float3(0, 0, 0);
		}

		float3 screenDir = endScreenPos - startScreenPos;
		float2 grabDir = endGrabPos - startGrabPos;

		float screenDirX = abs(screenDir.x);
		float screenDirY = abs(screenDir.y);

		float dirMultiplier = lerp( 1 / (_ScreenParams.y * screenDirY), 1 / (_ScreenParams.x * screenDirX), screenDirX > screenDirY ) * SSRSampleStep;

		screenDir *= dirMultiplier;
		grabDir *= dirMultiplier;

		half lastRayDepth = startClipPos.w;

		//half sampleCount = 1 + UVJitter(vertexOutput.pos) * 0.1;
		half sampleCount = 1 + UVJitter(clipPos) * 0.1;
		float lastDeltaDepth = 0;

#if defined (SHADER_API_OPENGL) || defined (SHADER_API_D3D11) || defined (SHADER_API_D3D12)
		[unroll(64)]
#else
		UNITY_LOOP
#endif
		for(int i = 0; i < SSRMaxSampleCount; i++)
		{
			float3 screenMarchUVZ = startScreenPos + screenDir * sampleCount;

			if((screenMarchUVZ.x <= 0) || (screenMarchUVZ.x >= 1) || (screenMarchUVZ.y <= 0) || (screenMarchUVZ.y >= 1))
			{
				break;
			}

			//float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenMarchUVZ.xy));
			float sceneDepth =LinearEyeDepth(SampleSceneDepth(screenMarchUVZ.xy),_ZBufferParams);
			half rayDepth = 1.0 / screenMarchUVZ.z;
			half deltaDepth = rayDepth - sceneDepth;

			if((deltaDepth > 0) && (sceneDepth > startClipPos.w) && (deltaDepth < (abs(rayDepth - lastRayDepth) * 2)))
			{
				float samplePercent = saturate(lastDeltaDepth / (lastDeltaDepth - deltaDepth));
				samplePercent = lerp(samplePercent, 1, rayDepth >= _ProjectionParams.z);
				float hitSampleCount = lerp(sampleCount-1, sampleCount, samplePercent);
				return float3(startGrabPos + grabDir * hitSampleCount, 1);
			}

			lastRayDepth = rayDepth;
			sampleCount += 1;

			lastDeltaDepth = deltaDepth;
		}

		float4 farClipPos;
		float3 farScreenPos;
		float2 farGrabPos;

		//SSRRayConvert(lightingData.worldPos + lightingData.R * 100000, farClipPos, farScreenPos, farGrabPos);
		SSRRayConvert(worldPos+ reflectDir * 100000, farClipPos, farScreenPos, farGrabPos);

		if((farScreenPos.x > 0) && (farScreenPos.x < 1) && (farScreenPos.y > 0) && (farScreenPos.y < 1))
		{
			//float farDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, farScreenPos.xy));
			float farDepth = LinearEyeDepth(SampleSceneDepth(farScreenPos.xy),_ZBufferParams);

			if(farDepth > startClipPos.w)
			{
				return float3(farGrabPos, 1);
			}
		}

		return float3(0, 0, 0);
	}

	float3 GetSSRUVZ_NoStruct(float3 clipPos, float3 worldPos, float3 reflectionDir ,float noV, float2 ScreenUV, float SSRMaxSampleCount,float SSRSampleStep)
	{
#if defined(UNITY_SINGLE_PASS_STEREO)
		half ssrWeight = 1;

		//half NoV = lightingData.NoV * 2;
		half NoV = noV * 2;
		ssrWeight *= (1 - NoV * NoV);
#else
		//float screenUV = lightingData.screenUV * 2 - 1;
		float screenUV = ScreenUV * 2 - 1;
		screenUV *= screenUV;

		half ssrWeight = saturate(1 - dot(screenUV, screenUV));

		//half NoV = lightingData.NoV * 2.5;
		half NoV = noV * 2.5;
		ssrWeight *= (1 - NoV * NoV);
		//return float3(ssrWeight,ssrWeight,ssrWeight);
#endif

		if (ssrWeight > 0.005)
		{
			float3 uvz = SSRRayMarch_NoStruct(clipPos, worldPos, reflectionDir, SSRMaxSampleCount, SSRSampleStep);
			uvz.z *= ssrWeight;
			return uvz;
		}

		return float3(0, 0, 0);
	}
#endif

