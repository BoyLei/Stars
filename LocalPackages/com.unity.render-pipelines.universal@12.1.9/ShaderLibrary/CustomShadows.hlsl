#ifndef ZGAME_CUSTOMSHADOW_INCLUDED
#define ZGAME_CUSTOMSHADOW_INCLUDED

TEXTURE2D_SHADOW(_CustomShadowMapStatic);
SAMPLER_CMP(sampler_CustomShadowMapStatic);

TEXTURE2D_SHADOW(_CustomShadowMapDynamic);
SAMPLER_CMP(sampler_CustomShadowMapDynamic);

//int MAX_SHADOW_COUNT = 8;
//public enum LightType
//{
//    Spot = 0,
//    Directional = 1,
//    Point = 2,
//    Area = 3,
//    Rectangle = 3,
//    Disc = 4
//}
/**
 * \brief 最後一個位置放空燈光
 */
real _CustomLightType[9];
/**
 * \brief 矩阵得用Float类型，否则在IOS平台，自定义 直射光组件阴影，采样数据异常
 */
float4x4 _CustomShadowMatrices[9]; 
float4x4 _CustomShadowMatricesDynamic[9];
float4 _CustomShadowUVRects[9]; 
real _LightIndicesToShadowMgr[9];
real4 _LightExtraParam[9];
real4 _ShadowDynamicSize;//x: pcf tap width, y: pcf tap height, z: invWidth, w: invHeight

float4x4 _ProjectorVP;
inline float3 WorldToShadowMapPos(float3 positionWS)
{
    float4 positionCS = mul(_ProjectorVP, float4(positionWS,1));
    positionCS /= positionCS.w;
    return positionCS.xyz;
}

real SampleShadowmapFiltered_4Tap(float3 shadowCoord)
{
    real4 shadowOffset0 = real4(-_ShadowDynamicSize.x,-_ShadowDynamicSize.y,0,0);
    real4 shadowOffset1 = real4(_ShadowDynamicSize.x,-_ShadowDynamicSize.y,0,0);
    real4 shadowOffset2 = real4(-_ShadowDynamicSize.x,_ShadowDynamicSize.y,0,0);
    real4 shadowOffset3 = real4(_ShadowDynamicSize.x,_ShadowDynamicSize.y,0,0);
    real attenuation;

    // 4-tap hardware comparison
    real4 attenuation4;
    attenuation4.x = SAMPLE_TEXTURE2D_SHADOW(_CustomShadowMapDynamic, sampler_CustomShadowMapDynamic, shadowCoord.xyz + shadowOffset0.xyz);
    attenuation4.y = SAMPLE_TEXTURE2D_SHADOW(_CustomShadowMapDynamic, sampler_CustomShadowMapDynamic, shadowCoord.xyz + shadowOffset1.xyz);
    attenuation4.z = SAMPLE_TEXTURE2D_SHADOW(_CustomShadowMapDynamic, sampler_CustomShadowMapDynamic, shadowCoord.xyz + shadowOffset2.xyz);
    attenuation4.w = SAMPLE_TEXTURE2D_SHADOW(_CustomShadowMapDynamic, sampler_CustomShadowMapDynamic, shadowCoord.xyz + shadowOffset3.xyz);
    attenuation = dot(attenuation4, 0.25);

    return attenuation;
}

////////////////////////////////////////////////////////////////////////////////////////////
//自定义聚光灯
float4 CustomSpotShadowCoord(int shadowMgrLightIndex,float3 positionWS)
{
    float4 shadowCoord = mul(_CustomShadowMatrices[shadowMgrLightIndex], float4(positionWS, 1.0));
    shadowCoord.xyz /= shadowCoord.w;
    return shadowCoord;
}
real CustomSpotShadow(float4 shadowCoord)
{
#if defined (_DISABLE_DYNAMIC_SHADOW)
    real shadowD = 1;
#else
    real shadowD = SAMPLE_TEXTURE2D_SHADOW(_CustomShadowMapDynamic, sampler_CustomShadowMapDynamic,shadowCoord.xyz);
#endif

#if defined (_DISABLE_STATIC_SHADOW)
    real shadowS = 1;
#else
    real shadowS = SAMPLE_TEXTURE2D_SHADOW(_CustomShadowMapStatic, sampler_CustomShadowMapStatic,shadowCoord.xyz);
#endif
    real shadow = min(shadowD,shadowS);

    return shadow;
}
////////////////////////////////////////////////////////////////////////////////////////////
//自定义方向光
real CustomDirShadow(int shadowMgrLightIndex,float3 positionWS)
{
    #if !defined (_CUSTOM_SHADOW_ON)
        #if defined (ZG_Character_Render) && defined (ZURP_CHARACTER_RECEIVESHADOW_INBATTLE)
            float3 shadowCoordCustomRT = WorldToShadowMapPos(positionWS);
            real sampleDepth= Linear01Depth(SAMPLE_TEXTURE2D_SHADOW(_ProjectorShadowDepth, sampler_ProjectorShadowDepth,shadowCoordCustomRT.xyz),_ZBufferParams);
            //sampleDepth = PCFSample(input.shadowCoord.z,input.shadowCoord.xyz);
            real customShadow= sampleDepth>shadowCoordCustomRT.z?1:0 /*_ShadowColor*/ ;
            return 1-customShadow;
        #else 
            return 1;
            //return SAMPLE_TEXTURE2D_LIGHTMAP(unity_ShadowMask, samplerunity_ShadowMask, input.uv.zw);
        #endif
    #endif

    float4 shadowCoord = mul(_CustomShadowMatrices[shadowMgrLightIndex], float4(positionWS, 1.0));
    ////动态坐标
    float4 shadowCoordDynamic = mul(_CustomShadowMatricesDynamic[shadowMgrLightIndex], float4(positionWS, 1.0));
    
    half minus = step(shadowCoordDynamic.z, 0);
    shadowCoordDynamic.z = lerp(shadowCoordDynamic.z, 1, minus);

#ifdef CHARA_SHADOW_HIGH
    real shadowD = SampleShadowmapFiltered_4Tap(shadowCoordDynamic.xyz);
#else
    #if defined (_DISABLE_DYNAMIC_SHADOW) 
        real shadowD = 1;
    #else
        float borderD = step(shadowCoordDynamic.y, 0) + step(1, shadowCoordDynamic.y) + step(shadowCoordDynamic.x, 0) + step(1, shadowCoordDynamic.x);
        borderD = saturate(borderD);
        real shadowD = SAMPLE_TEXTURE2D_SHADOW(_CustomShadowMapDynamic, sampler_CustomShadowMapDynamic, shadowCoordDynamic.xyz);
        shadowD = max(shadowD, borderD);
    #endif
#endif

#if defined (_DISABLE_STATIC_SHADOW)
    real shadowS = 1;
#else
    float borderS = step(shadowCoord.y, 0) + step(1, shadowCoord.y) + step(shadowCoord.x, 0) + step(1, shadowCoord.x);
    borderS = saturate(borderS);
    real shadowS = SAMPLE_TEXTURE2D_SHADOW(_CustomShadowMapStatic, sampler_CustomShadowMapStatic,shadowCoord.xyz);
    shadowS = max(shadowS, borderS);
#endif
    real shadow = min(shadowD,shadowS);
    
    //_LightExtraParam[shadowMgrLightIndex].x 约定阴影强度
    real shadowStrength = _LightExtraParam[shadowMgrLightIndex].x;
    return 1-(1-shadow) * shadowStrength;
}

real ShadowAttenuationByLightIndex(int shadowMgrLightIndex, float3 positionWS, real3 lightDir)
{
    real shadowAttenuation = 1;
    int lightType = _CustomLightType[shadowMgrLightIndex];
    if(lightType == 0)
    {
        float4 shadowCoord = CustomSpotShadowCoord(shadowMgrLightIndex,positionWS);
        shadowAttenuation = CustomSpotShadow(shadowCoord);
    }
    else if(lightType == 1)
    {
        shadowAttenuation = CustomDirShadow(shadowMgrLightIndex,positionWS);
    }
    else if(lightType == 2)
    {
        int faceId = CubeMapFaceID(-lightDir);
        shadowMgrLightIndex = shadowMgrLightIndex + faceId;
        float4 shadowCoord = CustomSpotShadowCoord(shadowMgrLightIndex,positionWS);
        shadowAttenuation = CustomSpotShadow(shadowCoord);
    }

    real shadowStrength = _LightExtraParam[shadowMgrLightIndex].x;
    return 1-(1-shadowAttenuation) * shadowStrength;
}

real CustomAdditionalShadow(int lightIndex, float3 positionWS, real3 lightDir)
{
    real shadowAttenuation = 1;
#if _MAINLIGHT_CUSTOM_SHADOW_ON
    int shadowMgrLightIndex = _LightIndicesToShadowMgr[lightIndex + 1];
    shadowAttenuation = ShadowAttenuationByLightIndex(shadowMgrLightIndex, positionWS, lightDir);
#elif _ADDILIGHT_CUSTOM_SHADOW_ON
    int shadowMgrLightIndex = _LightIndicesToShadowMgr[lightIndex];
    shadowAttenuation = ShadowAttenuationByLightIndex(shadowMgrLightIndex, positionWS, lightDir);
#endif
    return shadowAttenuation;
}

float4 TransformWorldToHClip(float3 posWS, float3 normalWS, float3 lightDir)
{
    float cosLightAngle = dot(lightDir, normalWS);
    float normalOffsetScale = 1 - saturate(cosLightAngle);
    half bias = lerp(_ShadowBias.x, _ShadowBias.x * 5, normalOffsetScale);
    posWS += lightDir * bias;

    float3 shadowOffset = normalWS.xyz * normalOffsetScale * _ShadowBias.y;
    posWS += shadowOffset;
    return TransformWorldToHClip(posWS);
}

#endif
