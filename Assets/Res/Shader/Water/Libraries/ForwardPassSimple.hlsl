//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

//#define DEBUG_NORMALS

//#define RESAMPLE_REFRACTION_DEPTH
//#define CAMERA_DEPTH_ABSORPTION

//#include "./WaterSSR.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"

//Note: Throws an error about a BLENDWEIGHTS vertex attribute on GLES when VR is enabled (fixed in URP 10+)
//Possibly related to: https://issuetracker.unity3d.com/issues/oculus-a-non-system-generated-input-signature-parameter-blendindices-cannot-appear-after-a-system-generated-value

#if SHADER_API_GLES3 && SHADER_LIBRARY_VERSION_MAJOR < 10
#define FRONT_FACE_SEMANTIC_REAL VFACE
#else
#define FRONT_FACE_SEMANTIC_REAL FRONT_FACE_SEMANTIC
#endif

#if UNDERWATER_ENABLED
half4 ForwardPassFragment(Varyings input, FRONT_FACE_TYPE vertexFace : FRONT_FACE_SEMANTIC_REAL) : SV_Target
#else

half3 GlossyEnvironmentReflection123(half3 reflectVector, float3 positionWS, half perceptualRoughness, half occlusion)
{
	#if !defined(_ENVIRONMENTREFLECTIONS_OFF)
	half3 irradiance;

	#ifdef _REFLECTION_PROBE_BLENDING
	irradiance = CalculateIrradianceFromReflectionProbes(reflectVector, positionWS, perceptualRoughness);
	#else
	#ifdef _REFLECTION_PROBE_BOX_PROJECTION
	reflectVector = BoxProjectedCubemapDirection(reflectVector, positionWS, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
	#endif // _REFLECTION_PROBE_BOX_PROJECTION
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


half4 ForwardPassFragment(Varyings input) : SV_Target
#endif
{
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
	
	
	half3 finalColor = 0;
	half alpha = 1;

	half vFace = 1.0;
	//0 = back face
	
	//half4 vertexColor = input.color; //Mask already applied in vertex shader
	//return float4(vertexColor.aaa, 1);

	//Vertex normal in world-space
	half3 normalWS = normalize(input.normal.xyz);
#if _NORMALMAP
	float3 WorldTangent = input.tangent.xyz;
	float3 WorldBiTangent = input.bitangent.xyz;
	float3 wPos = float3(input.normal.w, input.tangent.w, input.bitangent.w);
#else
	float3 wPos = input.wPos;
#endif
	//Not normalized for depth-pos reconstruction. Normalization required for lighting (otherwise breaks on mobile)
	float3 viewDir = (_WorldSpaceCameraPos - wPos);
	half3 viewDirNorm = SafeNormalize(viewDir);
	//return float4(viewDir, 1);
	
	half VdotN = 1.0 - saturate(dot(viewDirNorm, normalWS));

	//Returns mesh or world-space UV
	half2 uv = GetSourceUV(input.uv.xy, wPos.xz, _WorldSpaceUV);
	half2 flowMap = half2(1, 1);

	half slope = 0;

	half3 waveNormal = normalWS;

	float4 ShadowCoords = float4(0, 0, 0, 0);
	#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && !defined(UNLIT)
	ShadowCoords = input.shadowCoord;
	#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS) && !defined(UNLIT)
	ShadowCoords = TransformWorldToShadowCoord(wPos);
	#endif

	Light mainLight = GetMainLight(ShadowCoords);

	//half shadowMask = 1;

	//Normals
	half3 NormalsCombined = half3(0.5, 0.5, 1);
	half3 worldTangentNormal = waveNormal;
	
#if _NORMALMAP
	NormalsCombined = SampleNormals(uv * _NormalTiling, wPos, TIME, flowMap, _NormalSpeed, slope);
	//return float4((NormalsCombined.x * 0.5 + 0.5), (NormalsCombined.y * 0.5 + 0.5), 1, 1);

	worldTangentNormal = normalize(TransformTangentToWorld(NormalsCombined, half3x3(WorldTangent, WorldBiTangent, waveNormal)));
	
#endif

#ifdef SCREEN_POS
	float4 ScreenPos = input.screenPos;
#else
	float4 ScreenPos = 0;
#endif
	
	//return float4(depth.linear01, depth.linear01, depth.linear01, 1);

	#if _REFRACTION
	float4 refractedScreenPos = ScreenPos.xyzw + (float4(worldTangentNormal.xz, 0, 0) * (_RefractionStrength * 0.05));//lerp(0.1, 0.01,  unity_OrthoParams.w)));
	#endif

	float3 opaqueWorldPos = wPos;
	half opaqueDist = 1;
	half surfaceDepth = 1;// opaqueDist;
	half waterDensity = 1;
#if !_DISABLE_DEPTH_TEX
	SceneDepth depth = SampleDepth(ScreenPos);
	opaqueWorldPos = ReconstructViewPos(ScreenPos, viewDir, depth);
	opaqueDist = DepthDistance(wPos, opaqueWorldPos, normalWS);// * normalSign);
	//return float4(opaqueDist,opaqueDist,opaqueDist,1);
	

	surfaceDepth = SurfaceDepth(depth, input.positionCS);
	waterDensity = max(0, 1.0 - exp2(-surfaceDepth * _DepthVertical * 0.14427));
#endif
	
	half intersection = 0;
#if _SHARP_INERSECTION || _SMOOTH_INTERSECTION
	float interSecGradient = 1-saturate(exp(opaqueDist) * rcp(_IntersectionLength));

	#if _DISABLE_DEPTH_TEX
	interSecGradient = 0;
	#endif
	
	if (_IntersectionSource == 1) interSecGradient = 0.0;//vertexColor.r;
	if (_IntersectionSource == 2) interSecGradient = saturate(interSecGradient);// + vertexColor.r);
	//return float4(interSecGradient,0,0,1);

	intersection = SampleIntersection(uv.xy, interSecGradient, TIME * _IntersectionSpeed);
	intersection *= _IntersectionColor.a;
	
	//Flatten normals on intersection foam
	waveNormal = lerp(waveNormal, normalWS, intersection);
	//return half4(intersection, 0.0, 0.0, 1.0);
#endif
	//return float4(intersection,intersection,intersection,1);
	
	//Albedo
	half4 baseColor = lerp(_ShallowColor, _BaseColor, waterDensity);
	
	finalColor.rgb = baseColor.rgb;
	alpha = baseColor.a;
	float3 sparkles = 0;
	#if _NORMALMAP
	float NdotL = saturate(dot(UP_VECTOR, worldTangentNormal));
	half sunAngle = saturate(dot(UP_VECTOR, mainLight.direction));
	half angleMask = saturate(sunAngle * 10); /* 1.0/0.10 = 10 */
	sparkles = saturate(step(_SparkleSize, (saturate(NormalsCombined.y) * NdotL))) * _SparkleIntensity * mainLight.color * angleMask;
	
	finalColor.rgb += sparkles.rgb;
	#endif

	half4 sunSpec = 0;
#ifndef _SPECULARHIGHLIGHTS_OFF
	half3 sunReflectionNormals = worldTangentNormal;
	
	//Blinn-phong reflection
	sunSpec = SunSpecular(mainLight, viewDirNorm, sunReflectionNormals, _SunReflectionDistortion, _SunReflectionSize, _SunReflectionStrength);
	sunSpec.rgb *=  (1-intersection); //Hide
#endif

	//Reflection probe
#ifndef _ENVIRONMENTREFLECTIONS_OFF
	half3 refWorldTangentNormal = lerp(waveNormal, worldTangentNormal/*normalize(waveNormal + worldTangentNormal)*/, _ReflectionDistortion);

	
	half3 reflectionVector = reflect(-viewDirNorm , refWorldTangentNormal);
	//float2 reflectionPerturbation = lerp(waveNormal.xz * 0.5, worldTangentNormal.xy, _ReflectionDistortion).xy;
	half3 reflections = SampleReflectionsSimple(reflectionVector, _ReflectionBlur, wPos);
	//return float4(reflections,1);
	//half reflectionFresnel = ReflectionFresnel(refWorldTangentNormal, viewDirNorm, _ReflectionFresnel);
	//return float4(reflectionFresnel.xxx, 1);
	//finalColor.rgb = lerp(finalColor.rgb, reflections, _ReflectionStrength /** reflectionFresnel * vFace*/);

	//half NoV2 =  pow(1- saturate(dot(normalReflect,viewDirNorm)),0.1);
	half NoV2 =  pow((1- saturate(dot(refWorldTangentNormal,viewDirNorm)))*0.8,0.7);
	//half NoV3 = smoothstep(_ReflectionRange * 0.8 + 0.24,_ReflectionRange * 0.8 + 0.44,NoV2);
	NoV2 = smoothstep(_ReflectionRange * 0.8,_ReflectionRange* 0.8+0.2,NoV2);
	//这里对上面NoV3公式简化，两个SmoothStep中间只有0.24的偏移其余部分均一样,最后简化成一个MAD
	//half NoV3 = NoV2 + 0.24;
	//NoV2 += NoV3;
	NoV2 = NoV2 * 2 + 0.24 ;
	finalColor.rgb  = lerp(finalColor.rgb  * (1 + (1- NoV2) * _ReflectionStrength) ,reflections,saturate(NoV2  * _ReflectionStrength));
	
	//return float4(NoV2,NoV2,NoV2, 1);
#endif

#if _CAUSTICS
	half3 caustics = SampleCaustics(opaqueWorldPos.xz + lerp(waveNormal.xz, NormalsCombined.xz, _CausticsDistortion), TIME * _CausticsSpeed, _CausticsTiling) * _CausticsBrightness;
	
	//return float4(caustics, caustics, caustics, 1);

	half causticsMask = waterDensity;
	causticsMask = saturate(causticsMask + intersection);// + 1-vFace);

	//finalColor = lerp(finalColor + caustics, finalColor, causticsMask);
	//根据上式lerp展开后可简化成下面的式子
	finalColor += (1 - causticsMask) * caustics ;// 
#endif

	// Translucency
	TranslucencyData translucencyData = (TranslucencyData)0;
#if _TRANSLUCENCY
	translucencyData = PopulateTranslucencyData(_ShallowColor.rgb, mainLight.direction, mainLight.color, viewDirNorm, lerp(UP_VECTOR, waveNormal, vFace), worldTangentNormal, 0, _TranslucencyParams);
#endif

	//Foam application on top of everything up to this point

	#if _SHARP_INERSECTION || _SMOOTH_INTERSECTION
	//Layer intersection on top of everything
	finalColor.rgb = lerp(finalColor.rgb, _IntersectionColor.rgb, intersection);
	#endif

	//Full alpha on intersection and foam
	alpha = saturate(alpha + intersection);// + foam);
	
	//At this point, normal strength should affect lighting
	half normalMask = intersection;
	worldTangentNormal = lerp(waveNormal, worldTangentNormal, saturate(_NormalStrength - normalMask));
	
	//return float4(normalMask, normalMask, normalMask, 1);

	//Horizon color (note: not using normals, since they are perturbed by waves)
	half fresnel = saturate(pow(VdotN, _HorizonDistance));

	finalColor.rgb = lerp(finalColor.rgb, _HorizonColor.rgb, fresnel * _HorizonColor.a);
	
	//Final alpha
	half edgeFade = saturate(opaqueDist / (_EdgeFade * 0.01));
	

	alpha *= edgeFade;

	//Not yet implemented, does nothing now
	//SampleDiffuseProjectors(finalColor.rgb, wPos, ScreenPos);
	
	SurfaceData surfaceData = (SurfaceData)0;

	half density = 1;
	//return float4(density.rrr, 1.0);

	//alpha = lerp(density, alpha, vFace);
	
	surfaceData.albedo = finalColor.rgb;
	//return float4(finalColor.rgb,1);
	surfaceData.specular = sunSpec.rgb;
	//surfaceData.metallic = lerp(0.0, _Metallic, 1-(intersection+foam));
	surfaceData.metallic = 0;
	//surfaceData.smoothness = _Smoothness;
	surfaceData.smoothness = 0;
	surfaceData.normalTS = NormalsCombined;
	surfaceData.emission = 0;
	surfaceData.occlusion = 1;
	surfaceData.alpha = alpha;
	InputData inputData;
	inputData.positionWS = wPos;
	inputData.viewDirectionWS = viewDirNorm;
	inputData.shadowCoord = ShadowCoords;
	//Flatten normals for underwater lighting (distracting, peers through the fog)
	inputData.normalWS = worldTangentNormal;
	inputData.fogCoord = input.fogFactorAndVertexLight.x;
	inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
	inputData.bakedGI = SAMPLE_GI(input.lightmapUVOrVertexSH.xy, input.lightmapUVOrVertexSH.xyz, inputData.normalWS);

	///////////////////////////////////////////////////////////
	//reflect Probe模拟反射
	//half3 normalReflect = lerp(waveNormal, normalize(waveNormal + worldTangentNormal), _ReflectionDistortion);
	//half3 reflectVector = reflect(-inputData.viewDirectionWS, normalReflect);
	// BRDFData brdfData;
	// SurfaceData surfaceDataReflect = (SurfaceData)0;
	// surfaceDataReflect = surfaceData;
	// surfaceDataReflect.metallic = 1;
	// surfaceDataReflect.smoothness = 1;
	// InitializeBRDFData(surfaceDataReflect, brdfData);
	// half3 diffuseColor = GlobalIllumination(brdfData, inputData.bakedGI, 1, inputData.normalWS, inputData.viewDirectionWS);
	//half3 indirectSpecular = GlossyEnvironmentReflection123(reflectVector, wPos, PerceptualSmoothnessToPerceptualRoughness(1), 1.0h);

	//half NoV1 =  pow(1- saturate(dot(normalReflect,inputData.viewDirectionWS)),0.1);
	//half ReflectStep =0;//step(indirectSpecular.x,0.01);
	//surfaceData.albedo = lerp(surfaceData.albedo * (1 + 1- NoV1)  ,indirectSpecular,NoV1 * 0.8 );
	//return float4(NoV1,NoV1,NoV1,1);
	///////////////////////////////////////////////////////////

	
	float4 color = float4(ApplyLighting(surfaceData, inputData, translucencyData, density, _ShadowStrength, vFace), alpha);
	
	/*#if defined(DEBUG_DISPLAY)
	surfaceData.emission = translucencyData.mask;
	inputData.positionCS = input.positionCS;
	#if _NORMALMAP
	inputData.tangentToWorld = half3x3(WorldTangent, WorldBiTangent, waveNormal);
	#else
	inputData.tangentToWorld = 0;
	#endif
	inputData.shadowMask = TransformWorldToShadowCoord(wPos.xyz);
	inputData.normalizedScreenSpaceUV = ScreenPos.xy / ScreenPos.w;
	inputData.dynamicLightmapUV = input.lightmapUVOrVertexSH.xy;
	inputData.staticLightmapUV = input.lightmapUVOrVertexSH.xy;
	inputData.vertexSH = input.lightmapUVOrVertexSH.xyz;

	inputData.brdfDiffuse = surfaceData.albedo;
	inputData.brdfSpecular = surfaceData.specular;
	inputData.uv = uv;
	inputData.mipCount = 0;
	inputData.texelSize = float4(1/uv.x, 1/uv.y, uv.x, uv.y);
	inputData.mipInfo = 0;
	half4 debugColor;

	if (CanDebugOverrideOutputColor(inputData, surfaceData, debugColor))
	{
		return debugColor;
	}
	#endif*/
	
	#if _REFRACTION
		float3 sceneColor = SampleSceneColor(refractedScreenPos.xy / refractedScreenPos.w).rgb;
		//return float4(sceneColor,1);
	
		color.rgb = lerp(sceneColor, color.rgb, alpha);
		alpha = edgeFade;
	#endif

	
	
	///////////////////////////////////////////////////////////
	// //SSR
	// float3 reflection = normalize(reflect(-viewDirNorm, inputData.normalWS));
	// float NoV = saturate(dot(inputData.normalWS, viewDirNorm));
	// float3 uvz = GetSSRUVZ_NoStruct(input.positionCS, inputData.positionWS, reflection, NoV, ScreenPos.xy/ScreenPos.w, 40.7,25.8);
	// //float3 GetSSRUVZ_NoStruct(float3 clipPos, float3 worldPos, float3 reflectionDir ,float noV, float2 ScreenUV, float SSRMaxSampleCount,float SSRSampleStep)
	//
	// //half3 ssrColor = lerp(half3(0, 0, 0), tex2D(_BGWater_GrabTexture, uvz.xy) * _SSRIntensity, uvz.z > 0);
	// half4 ssrColor = half4(lerp(half3(0, 0, 0), SampleSceneColor(uvz.xy)* 1, uvz.z > 0),uvz.z);
	// color.rgb = color.rgb + lerp(/*lerp(float3(0,0,0), ssrColor.rgb, ssrColor.a)*/float3(0,0,0), ssrColor, ssrColor.a );
	// ///////////////////////////////////////////////////////////
		
	color.a = alpha * alpha;// - vertexColor.g);
	//return half4(vertexColor.g, 0.0, 0.0, 1.0);
	ApplyFog(color.rgb, input.fogFactorAndVertexLight.x, ScreenPos, wPos, vFace);
	
	return color;
}
