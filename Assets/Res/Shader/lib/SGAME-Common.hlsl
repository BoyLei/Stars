#ifndef SGAME_COMMON_INCLUDED
#define SGAME_COMMON_INCLUDED

// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ImageBasedLighting.hlsl"
// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

//控制LightMapAO强度的全局参数
float _LightMapAOStrengthL;
float _LightMapAOStrengthD;
float4 _ColorL;
float4 _ColorD;

//全局控制烘焙LightMap的AO强度的函数
void LightMapAOStrengthControl(half4 shadowCoord, inout half3 bakedGI, inout half3 aoStengthL, float3 posWS)
{
    half3 aoStrength = half3(1,1,1);
    //烘焙Light的状态下,调整BakedGI中AO在亮部的效果
    Light mainLight = GetMainLight(shadowCoord, posWS);
    half bakeGIlightness = (bakedGI.r + bakedGI.g + bakedGI.b)*0.33333f;
    half bakeGIlightness2 = smoothstep(0,0.4,bakeGIlightness);
    bakeGIlightness = smoothstep(0,0.3,bakeGIlightness);
   
    half LightMask = 1 - mainLight.shadowAttenuation;
    aoStrength = clamp(pow(half3(bakeGIlightness,bakeGIlightness,bakeGIlightness),_LightMapAOStrengthL) + (LightMask),0,1);
    half3 newBakedGI = pow(bakedGI, _LightMapAOStrengthD);
    newBakedGI = bakedGI + (1 - bakeGIlightness2) * _LightMapAOStrengthD * 0.05f * _ColorD.rgb;
    bakedGI = lerp(bakedGI,newBakedGI,LightMask);
    aoStengthL = lerp(_ColorL.rgb,float3(1,1,1),aoStrength);
}

#endif