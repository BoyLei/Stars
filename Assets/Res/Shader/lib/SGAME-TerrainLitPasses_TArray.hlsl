#ifndef UNIVERSAL_TERRAIN_LIT_PASSES_INCLUDED
#define UNIVERSAL_TERRAIN_LIT_PASSES_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "./SGAME-TerrainLitInput_TArray.hlsl"
#include "../lib/SGAME-Common.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 texcoord : TEXCOORD0;
    float2 staticLightmapUV : TEXCOORD1;
};

struct Varyings
{
    float4 uvMainAndLM              : TEXCOORD0; // xy: control, zw: lightmap
    float4 uvSplat01                : TEXCOORD1; // xy: splat0, zw: splat1
    float4 uvSplat23                : TEXCOORD2; // xy: splat2, zw: splat3
    float4 normal                   : TEXCOORD3;    // xyz: normal, w: viewDir.x
    float4 tangent                  : TEXCOORD4;    // xyz: tangent, w: viewDir.y
    float4 bitangent                : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
    #ifdef _ADDITIONAL_LIGHTS_VERTEX
        half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light
    #else
        half  fogFactor                 : TEXCOORD6;
    #endif
    float3 positionWS               : TEXCOORD7;
    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 9);
    float4 clipPos                  : SV_POSITION;
};

void InitializeInputData(Varyings IN, half3 normalTS, out InputData input)
{
    input = (InputData)0;

    input.positionWS = IN.positionWS;
    half3 SH = half3(0, 0, 0);

    half3 viewDirWS = half3(IN.normal.w, IN.tangent.w, IN.bitangent.w);
    input.normalWS = TransformTangentToWorld(normalTS, half3x3(-IN.tangent.xyz, IN.bitangent.xyz, IN.normal.xyz));
    SH = SampleSH(input.normalWS.xyz);

#if SHADER_HINT_NICE_QUALITY
    viewDirWS = SafeNormalize(viewDirWS);
#endif

    input.normalWS = NormalizeNormalPerPixel(input.normalWS);
    input.viewDirectionWS = viewDirWS;


#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    input.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
#else
    input.shadowCoord = float4(0, 0, 0, 0);
#endif

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
    inputData.fogCoord = InitializeInputDataFog(float4(IN.positionWS, 1.0), IN.fogFactorAndVertexLight.x);
    inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
    #else
    input.fogCoord = InitializeInputDataFog(float4(IN.positionWS, 1.0), IN.fogFactor);
    #endif

    input.bakedGI = SAMPLE_GI(IN.uvMainAndLM.zw, SH, input.normalWS);
}

//#ifdef _TERRAIN_BLEND_HEIGHT
void HeightBasedSplatModify(inout half4 splatControl/*, in half4 masks[4]*/,in half4 Height)
{
    // heights are in mask blue channel, we multiply by the splat Control weights to get combined height
    half4 splatHeight = half4(Height.r, Height.g, Height.b, Height.a) * splatControl.rgba;//当Amplitude为100时 Height相乘的变量为1
    half maxHeight = max(splatHeight.r, max(splatHeight.g, max(splatHeight.b, splatHeight.a)));
    
    // Ensure that the transition height is not zero.
    half transition = max(_HeightTransition, 1e-5);
    //half transition = max(0.07, 1e-5); //从framedebugger内截取的 写死

    // This sets the highest splat to "transition", and everything else to a lower value relative to that, clamping to zero
    // Then we clamp this to zero and normalize everything
    half4 weightedHeights = splatHeight + transition - maxHeight.xxxx;
    weightedHeights = max(0, weightedHeights);

    // We need to add an epsilon here for active layers (hence the blendMask again)
    // so that at least a layer shows up if everything's too low.
    weightedHeights = (weightedHeights + 1e-6) * splatControl;

    // Normalize (and clamp to epsilon to keep from dividing by zero)
    half sumHeight = max(dot(weightedHeights, half4(1, 1, 1, 1)), 1e-6);
    splatControl = weightedHeights / sumHeight.xxxx;
}
//#endif

float3 RebuildNormalFromRG(float2 normalRG)
{
    //  normalRG.xy = normalize(normalRG.xy);
    float normalZ = sqrt(1 - saturate(dot(normalRG.xy,normalRG.xy)));
    normalZ = clamp(normalZ,0.55,0.99);
    float3 OUT = normalize(float3(normalRG.xy,normalZ));
    return OUT;
}

real3 UnpackNormalFromRG(real4 normalTT, real scale)
{
    real3 normal;
    normal.xy = normalTT.rg * 2 - 1;
    normal.z = max(1e-5, sqrt(1 - dot(normal.xy, normal.xy)));
    return normal;
}

void SplatmapMix(float4 uvMainAndLM     
                    , float4 uvSplat01
                    , float4 uvSplat23
                    , inout half4 splatControl
                    , out half weight
                    , out half4 mixedDiffuse
                    , out half4 defaultSmoothness
                    , inout half3 mixedNormal)
{
    float4 temp = float4(1,1,1,1);

    half4 diffAlbedo[6];
    
    diffAlbedo[0] =/* _Color0 **/ SAMPLE_TEXTURE2D_ARRAY(_SplatArray, sampler_SplatArray, uvSplat01.xy, 0);
    diffAlbedo[1] =/* _Color1 **/ SAMPLE_TEXTURE2D_ARRAY(_SplatArray, sampler_SplatArray, uvSplat01.zw, 1);
    diffAlbedo[2] =/* _Color2 **/ SAMPLE_TEXTURE2D_ARRAY(_SplatArray, sampler_SplatArray, uvSplat23.xy, 2);
    diffAlbedo[3] =/* _Color3 **/ SAMPLE_TEXTURE2D_ARRAY(_SplatArray, sampler_SplatArray, uvSplat23.zw, 3);
    
    diffAlbedo[0].a = pow(diffAlbedo[0].a,2.2);
    diffAlbedo[1].a = pow(diffAlbedo[1].a,2.2);
    diffAlbedo[2].a = pow(diffAlbedo[2].a,2.2);
    diffAlbedo[3].a = pow(diffAlbedo[3].a,2.2);

    defaultSmoothness = 1;
    
    half3 nrm = 0.0f;
    //采样NormalMap 并从中提取出Height
    half4 normalTex0,normalTex1,normalTex2,normalTex3;
    normalTex0 = SAMPLE_TEXTURE2D_ARRAY(_NormalArray, sampler_SplatArray, uvSplat01.xy, 0);
    normalTex1 = SAMPLE_TEXTURE2D_ARRAY(_NormalArray, sampler_SplatArray, uvSplat01.zw, 1);
    normalTex2 = SAMPLE_TEXTURE2D_ARRAY(_NormalArray, sampler_SplatArray, uvSplat23.xy, 2);
    normalTex3 = SAMPLE_TEXTURE2D_ARRAY(_NormalArray, sampler_SplatArray, uvSplat23.zw, 3);
    
    half4 height = half4(diffAlbedo[0].a, diffAlbedo[1].a, diffAlbedo[2].a, diffAlbedo[3].a);
    HeightBasedSplatModify(splatControl, height/*, masks*/);
    
    float4 normalRB00 = float4(normalTex0.rg,1,1);
    float4 normalRB01 = float4(normalTex1.rg,1,1);
    float4 normalRB02 = float4(normalTex2.rg,1,1);
    float4 normalRB03 = float4(normalTex3.rg,1,1);
       
    // _GlobalOpaqueScaleFator是0~1的值，需求就是：如果_GlobalOpaqueScaleFator变小了，需要把法线强度增强
    half normalScaleRamp = lerp(_NormalScaleRemap, 0, _GlobalOpaqueScaleFator);
  
    nrm += splatControl.r * UnpackNormalScale(normalRB00.xyzw, _NormalScale0 + normalScaleRamp);
    nrm += splatControl.g * UnpackNormalScale(normalRB01.xyzw, _NormalScale1 + normalScaleRamp);
    nrm += splatControl.b * UnpackNormalScale(normalRB02.xyzw, _NormalScale2 + normalScaleRamp);
    nrm += splatControl.a * UnpackNormalScale(normalRB03.xyzw, _NormalScale3 + normalScaleRamp);

    defaultSmoothness = half4(normalTex0.b, normalTex1.b, normalTex2.b, normalTex3.b);

    // avoid risk of NaN when normalizing.
    #if HAS_HALF
    nrm.z += 0.01h;
    #else
    nrm.z += 1e-5f;
    #endif
   // mixedNormal = SAMPLE_TEXTURE2D(_Normal0, sampler_Normal0, uvSplat01.xy).xxx;
    mixedNormal = nrm;
    
    defaultSmoothness *= half4(_Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3);

   //#ifndef _TERRAIN_BLEND_HEIGHT_ON // density blending
    //{
    //    // 20.0 is the number of steps in inputAlphaMask (Density mask. We decided 20 empirically)
    //    half4 opacityAsDensity = saturate((half4(diffAlbedo[0].a, diffAlbedo[1].a, diffAlbedo[2].a, diffAlbedo[3].a) - (1 - splatControl)) * 20.0);
    //    opacityAsDensity += 0.001h * splatControl; // if all weights are zero, default to what the blend mask says
    //    half4 useOpacityAsDensityParam = { _DiffuseRemapScale0.w, _DiffuseRemapScale1.w, _DiffuseRemapScale2.w, _DiffuseRemapScale3.w }; // 1 is off
    //    splatControl = lerp(opacityAsDensity, splatControl, useOpacityAsDensityParam);
    //}
    //#endif
    // Now that splatControl has changed, we can compute the final weight and normalize
    weight = splatControl;
    //splatControl /= (weight + HALF_MIN);

    mixedDiffuse = 0.0h;
    mixedDiffuse.rgb += diffAlbedo[0] * _DiffuseRemapScale0.rgb * splatControl.r;
    mixedDiffuse.rgb += diffAlbedo[1] * _DiffuseRemapScale1.rgb * splatControl.g;
    mixedDiffuse.rgb += diffAlbedo[2] * _DiffuseRemapScale2.rgb * splatControl.b;
    mixedDiffuse.rgb += diffAlbedo[3] * _DiffuseRemapScale3.rgb * splatControl.a;
    mixedDiffuse.a = 1;

}


void SplatmapFinalColor(inout half4 color, half fogCoord)
{
    color.rgb *= color.a;
    color.rgb = MixFog(color.rgb, fogCoord);
    //color.rgb = MixFogColor1(color.rgb,unity_FogColor.rgb, fogCoord);
}



///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard Terrain shader
Varyings SplatmapVert(Attributes v)
{
    Varyings o = (Varyings)0;

    VertexPositionInputs Attributes = GetVertexPositionInputs(v.positionOS.xyz);

    o.uvMainAndLM.xy = v.texcoord;
    o.uvMainAndLM.zw = v.texcoord * unity_LightmapST.xy + unity_LightmapST.zw;
    
    o.uvSplat01.xy = v.texcoord.xy * _Splat0_ST.xy + _Splat0_ST.zw;
    o.uvSplat01.zw = v.texcoord.xy * _Splat1_ST.xy + _Splat1_ST.zw;
    o.uvSplat23.xy = v.texcoord.xy * _Splat2_ST.xy + _Splat2_ST.zw;
    o.uvSplat23.zw = v.texcoord.xy * _Splat3_ST.xy + _Splat3_ST.zw;

    //o.uvSplat01.xy = TRANSFORM_TEX(v.texcoord, _Splat0);
    //o.uvSplat01.zw = TRANSFORM_TEX(v.texcoord, _Splat1);
    //o.uvSplat23.xy = TRANSFORM_TEX(v.texcoord, _Splat2);
    //o.uvSplat23.zw = TRANSFORM_TEX(v.texcoord, _Splat3);
    //o.uvSplat45.xy = TRANSFORM_TEX(v.texcoord, _Splat4);
    //o.uvSplat45.zw = TRANSFORM_TEX(v.texcoord, _Splat5);

    half3 viewDirWS = GetCameraPositionWS() - Attributes.positionWS;
#if !SHADER_HINT_NICE_QUALITY
    viewDirWS = SafeNormalize(viewDirWS);
#endif

    float4 vertexTangent = float4(cross(float3(0, 0, 1), v.normalOS), 1.0);
    VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS, vertexTangent);

    o.normal = half4(normalInput.normalWS, viewDirWS.x);
    o.tangent = half4(normalInput.tangentWS, viewDirWS.y);
    o.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);

    // o.fogFactorAndVertexLight.x = ComputeFogFactor(Attributes.positionCS.z);
    // o.fogFactorAndVertexLight.yzw = VertexLighting(Attributes.positionWS, o.normal.xyz);

    half fogFactor = 0;
    //#if !defined(_FOG_FRAGMENT)
    fogFactor = ComputeFogFactor(Attributes.positionCS.z);
    //#endif
    // #ifdef _ADDITIONAL_LIGHTS_VERTEX
    // o.fogFactorAndVertexLight.x = fogFactor;
    // o.fogFactorAndVertexLight.yzw = VertexLighting(Attributes.positionWS, o.normal.xyz);
    // #else
    o.fogFactor = fogFactor;
    //#endif
    o.positionWS = Attributes.positionWS;
    o.clipPos = Attributes.positionCS;

    
    OUTPUT_LIGHTMAP_UV(v.staticLightmapUV, unity_LightmapST, o.staticLightmapUV);
    OUTPUT_SH(o.normal.xyz, o.vertexSH);

    return o;
}


// Used in Standard Terrain shader
half4 SplatmapFragment(Varyings IN) : SV_TARGET
{

    half3 normalTS = half3(0.0h, 0.0h, 1.0h);

  //  half4 normalTex0 = SAMPLE_TEXTURE2D(_Normal0, sampler_Normal0, IN.uvSplat01);
  //  return float4(normalTex0.a,normalTex0.a,normalTex0.a,1);
    float2 splatUV = (IN.uvMainAndLM.xy * (_Control_TexelSize.zw - 1.0f) + 0.5f) * _Control_TexelSize.xy;
    half4 splatControl = SAMPLE_TEXTURE2D(_Control, sampler_Control, splatUV);
    half4 splatControl2 = 0;
    
    half alpha = dot(splatControl, 1.0h);
    
    half weight;
    half4 mixedDiffuse;
    half4 defaultSmoothness;
    half4 defaultSmoothness2;
    SplatmapMix(IN.uvMainAndLM, IN.uvSplat01, IN.uvSplat23, splatControl, weight, mixedDiffuse, defaultSmoothness, normalTS);
    half3 albedo = mixedDiffuse.rgb;
    //return float4(_Color1);
    
    half smoothness = dot(splatControl, defaultSmoothness);
    smoothness = clamp(smoothness,0,1);
    
    half4 defaultMetallic = half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3);
    //half4 defaultMetallic2 = half4(_Metallic4, _Metallic5, 0, 0);
    half metallic = dot(splatControl, defaultMetallic);

    half4 defaultOcclusion = 1;
    half4 defaultOcclusion2 = 1;
    half occlusion = dot(splatControl, defaultOcclusion) + dot(splatControl2, defaultOcclusion);
    
    //AHD光照相关
    half4 bakedLmDir = 0.0;
    half3 bakedLm = 0.0;

    InputData inputData;
    InitializeInputData(IN, normalTS, inputData);
    
    #if defined(DYNAMICLIGHTMAP_ON)
        inputdata.bakedGI = float3(0,0,0);//SAMPLE_GI(i.staticLightmapUV, i.dynamicLightmapUV, i.vertexSH, inputdata.normalWS);
    #else
        //inputData.bakedGI = SAMPLE_GI(IN.staticLightmapUV, IN.vertexSH, inputData.normalWS);
        //使用支持AHD光照的获得GI的方法(若烘焙了两张光照贴图则会返回其数值至bakedLmDir和BakedLm)
    inputData.bakedGI = SAMPLE_GI_AHD(IN.staticLightmapUV, IN.vertexSH, inputData.normalWS, bakedLmDir, bakedLm);
    #endif
    //half4 color = UniversalFragmentPBR(inputData, albedo, metallic, /* specular */ half3(0.0h, 0.0h, 0.0h), smoothness, occlusion, /* emission */ half3(0, 0, 0), alpha);
    //half4 color = UniversalFragmentBlinnPhong(inputData, albedo, half4(lerp(kDieletricSpec.rgb, albedo, metallic),1.0), smoothness, half3(0, 0, 0), alpha, normalTS);
    //return CalculateBlinnPhong()
    //return half4(smoothness.xxx, 1.0);
    //return float4(inputData.bakedGI.rgb,1);
    half3 aoSrengthL = half3(1,1,1);
    #if defined(LIGHTMAP_ON)//如果开启lightmap, 则计算亮部ao增强和暗部ao减弱的值
    LightMapAOStrengthControl(inputData.shadowCoord, inputData.bakedGI, aoSrengthL, inputData.positionWS);
    #endif
    half4 color = UniversalFragmentPBR_SgameAHD(inputData, albedo, metallic, /* specular */half3(0.0h, 0.0h, 0.0h), smoothness, occlusion, /* emission */half3(0, 0, 0), alpha, bakedLmDir, bakedLm);
    color.rgb *= aoSrengthL;
    //color.rgb *=  LightMapAOStrengthControl(inputData.shadowCoord, inputData.bakedGI);
    SplatmapFinalColor(color, inputData.fogCoord);
    //return float4(inputData.fogCoord,inputData.fogCoord,inputData.fogCoord,1);
    
    return half4(color.rgb, 1.0h);
}





#endif
