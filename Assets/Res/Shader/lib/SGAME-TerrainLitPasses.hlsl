#ifndef UNIVERSAL_TERRAIN_LIT_PASSES_INCLUDED
#define UNIVERSAL_TERRAIN_LIT_PASSES_INCLUDED


#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


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
    float4 uvSplat45                : TEXCOORD8; // xy: splat4, zw: splat5

#if defined(_NORMALMAP)
    float4 normal                   : TEXCOORD3;    // xyz: normal, w: viewDir.x
    float4 tangent                  : TEXCOORD4;    // xyz: tangent, w: viewDir.y
    float4 bitangent                : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
#else
    float3 normal                   : TEXCOORD3;
    float3 viewDir                  : TEXCOORD4;
    //half3 vertexSH                  : TEXCOORD5; // SH
#endif

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

#if defined(_NORMALMAP) 
    half3 viewDirWS = half3(IN.normal.w, IN.tangent.w, IN.bitangent.w);
    input.normalWS = TransformTangentToWorld(normalTS, half3x3(-IN.tangent.xyz, IN.bitangent.xyz, IN.normal.xyz));
    SH = SampleSH(input.normalWS.xyz);
#else
    half3 viewDirWS = IN.viewDir;
    input.normalWS = IN.normal;
    //SH = IN.vertexSH;
#endif

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

void SplatmapMix(float4 uvMainAndLM, float4 uvSplat01, float4 uvSplat23,float4 uvSplat45, inout half4 splatControl,inout half4 splatControl2, out half weight, out half4 mixedDiffuse, out half4 defaultSmoothness, out half4 defaultSmoothness2, inout half3 mixedNormal)
{
    float4 temp = float4(1,1,1,1);

    half4 diffAlbedo[6];

    diffAlbedo[0] = SAMPLE_TEXTURE2D(_Splat0, sampler_Splat0, uvSplat01.xy);
    diffAlbedo[1] = SAMPLE_TEXTURE2D(_Splat1, sampler_Splat0, uvSplat01.zw);
    diffAlbedo[2] = SAMPLE_TEXTURE2D(_Splat2, sampler_Splat0, uvSplat23.xy);
    diffAlbedo[3] = SAMPLE_TEXTURE2D(_Splat3, sampler_Splat0, uvSplat23.zw);

    #ifdef _CONTROLSECOND_ON
    diffAlbedo[4] = SAMPLE_TEXTURE2D(_Splat4, sampler_Splat0, uvSplat45.xy);
    diffAlbedo[5] = SAMPLE_TEXTURE2D(_Splat5, sampler_Splat0, uvSplat45.zw);
    #endif

    //混合splat和color
    diffAlbedo[0].rgb *= _Color0.rgb;
    diffAlbedo[1].rgb *= _Color1.rgb;
    diffAlbedo[2].rgb *= _Color2.rgb;
    diffAlbedo[3].rgb *= _Color3.rgb;
    #ifdef _CONTROLSECOND_ON
    diffAlbedo[4].rgb *= _Color4.rgb;
    diffAlbedo[5].rgb *= _Color5.rgb;
    #endif

    
   
    defaultSmoothness = half4(1,1,1,1);
    defaultSmoothness2 = half4(1,1,1,1);
    half3 nrm = 0.0f;
    half4 normalTex0,normalTex1,normalTex2,normalTex3;
    #ifdef _NORMALMAP
    //采样NormalMap 并从中提取出Height
    
    //half3 nrm = 0.0f;
    normalTex0 = SAMPLE_TEXTURE2D(_Normal0, sampler_Normal0, uvSplat01.xy);
    normalTex1 = SAMPLE_TEXTURE2D(_Normal1, sampler_Normal0, uvSplat01.zw);
    normalTex2 = SAMPLE_TEXTURE2D(_Normal2, sampler_Normal0, uvSplat23.xy);
    normalTex3 = SAMPLE_TEXTURE2D(_Normal3, sampler_Normal0, uvSplat23.zw);
    

    #ifdef _TERRAIN_BLEND_HEIGHT_ON
    // disable Height Based blend when there are more than 4 layers (multi-pass breaks the normalization)
    //if (_NumLayersCount <= 4)

    //对存在albedo贴图a通道中的值作gamma矫正,因为地编制作地面时候的M贴图勾选了srgb
    diffAlbedo[0].a = pow(diffAlbedo[0].a,2.2);
    diffAlbedo[1].a = pow(diffAlbedo[1].a,2.2);
    diffAlbedo[2].a = pow(diffAlbedo[2].a,2.2);
    diffAlbedo[3].a = pow(diffAlbedo[3].a,2.2);
    //
    
    half height0 = lerp(1, diffAlbedo[0].a * _HeightScale0, _IfAlpha_Height0);
    half height1 = lerp(1, diffAlbedo[1].a * _HeightScale1, _IfAlpha_Height1);
    half height2 = lerp(1, diffAlbedo[2].a * _HeightScale2, _IfAlpha_Height2);
    half height3 = lerp(1, diffAlbedo[3].a * _HeightScale3, _IfAlpha_Height3);
    half4 Height = half4(height0, height1, height2, height3);
    
    HeightBasedSplatModify(splatControl, Height/*, masks*/);
    //return float4(1,1,1,1);
    //temp = half4(splatControl.rgb,1);
    #endif
    #endif

    
    #ifdef _IFBSMOOTHNESS_ON

    // normalTex0.rg = float2(pow(normalTex0.r,2.2),pow(normalTex0.g,2.2));
    // normalTex1.rg = float2(pow(normalTex1.r,2.2),pow(normalTex1.g,2.2));
    // normalTex2.rg = float2(pow(normalTex2.r,2.2),pow(normalTex2.g,2.2));
    // normalTex3.rg = float2(pow(normalTex3.r,2.2),pow(normalTex3.g,2.2));

    // normalTex0.rg = normalize(float3(normalTex0.rg,1)).rg;
    // normalTex1.rg = normalize(float3(normalTex1.rg,1)).rg;
    // normalTex2.rg = normalize(float3(normalTex2.rg,1)).rg;
    // normalTex3.rg = normalize(float3(normalTex3.rg,1)).rg;
    
    // float4 normalRB00 = float4(RebuildNormalFromRG(normalTex0.rg),1);
    // float4 normalRB01 = float4(RebuildNormalFromRG(normalTex1.rg),1);
    // float4 normalRB02 = float4(RebuildNormalFromRG(normalTex2.rg),1);
    // float4 normalRB03 = float4(RebuildNormalFromRG(normalTex3.rg),1);

    float4 normalRB00 = float4(normalTex0.rg,1,1);
    float4 normalRB01 = float4(normalTex1.rg,1,1);
    float4 normalRB02 = float4(normalTex2.rg,1,1);
    float4 normalRB03 = float4(normalTex3.rg,1,1);

    
    nrm += splatControl.r * UnpackNormalScale(normalRB00.xyzw, _NormalScale0);
    nrm += splatControl.g * UnpackNormalScale(normalRB01.xyzw, _NormalScale1);
    nrm += splatControl.b * UnpackNormalScale(normalRB02.xyzw, _NormalScale2);
    nrm += splatControl.a * UnpackNormalScale(normalRB03.xyzw, _NormalScale3);

    //defaultSmoothness = half4(pow(normalTex0.b,2.2),pow(normalTex1.b,2.2),pow(normalTex2.b,2.2),pow(normalTex3.b,2.2));
    defaultSmoothness = half4(normalTex0.b,normalTex1.b,normalTex2.b,normalTex3.b);
    #else
    nrm += splatControl.r * UnpackNormalScale(normalTex0.xyzw, _NormalScale0);
    nrm += splatControl.g * UnpackNormalScale(normalTex1.xyzw, _NormalScale1);
    nrm += splatControl.b * UnpackNormalScale(normalTex2.xyzw, _NormalScale2);
    nrm += splatControl.a * UnpackNormalScale(normalTex3.xyzw, _NormalScale3);
    defaultSmoothness = half4(1,1,1,1);
    #endif

    // avoid risk of NaN when normalizing.
    #if HAS_HALF
    nrm.z += 0.01h;
    #else
    nrm.z += 1e-5f;
    #endif
   // mixedNormal = SAMPLE_TEXTURE2D(_Normal0, sampler_Normal0, uvSplat01.xy).xxx;
    mixedNormal = nrm;
    
    
    defaultSmoothness *= half4(_Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3);
    defaultSmoothness2 = 1;

  

    //defaultSmoothness *= float4(normalTex0.b,normalTex1.b,normalTex2.b,normalTex3.b);
    //temp.rgb = nrm.bbb;
    #ifdef _CONTROLSECOND_ON
    half4 normalTex4,normalTex5;
    normalTex4 = SAMPLE_TEXTURE2D(_Normal4, sampler_Normal0, uvSplat45.xy);
    normalTex5 = SAMPLE_TEXTURE2D(_Normal5, sampler_Normal0, uvSplat45.zw);

    #ifdef _IFBSMOOTHNESS_ON
    float4 normalRB04 = float4(RebuildNormalFromRG(normalTex4.rg),1);
    float4 normalRB05 = float4(RebuildNormalFromRG(normalTex5.rg),1);
    nrm += splatControl2.r * UnpackNormalScale(normalRB04.xyzw, _NormalScale4);
    nrm += splatControl2.g * UnpackNormalScale(normalRB05.xyzw, _NormalScale5);
    defaultSmoothness2 =  half4(normalTex4.b, normalTex5.b,0, 0);
    #else
    nrm += splatControl2.r * UnpackNormalScale(normalTex4, _NormalScale4);
    nrm += splatControl2.g * UnpackNormalScale(normalTex5, _NormalScale5);
    defaultSmoothness2 = half4(1,1,0,0);
    #endif
    defaultSmoothness2 *= half4(_Smoothness4, _Smoothness5, 0, 0);
    #endif
    


   #ifndef _TERRAIN_BLEND_HEIGHT_ON // density blending
    if(_NumLayersCount <= 4)
    {
        // 20.0 is the number of steps in inputAlphaMask (Density mask. We decided 20 empirically)
        half4 opacityAsDensity = saturate((half4(diffAlbedo[0].a, diffAlbedo[1].a, diffAlbedo[2].a, diffAlbedo[3].a) - (1 - splatControl)) * 20.0);
        opacityAsDensity += 0.001h * splatControl;      // if all weights are zero, default to what the blend mask says
        half4 useOpacityAsDensityParam = { _DiffuseRemapScale0.w, _DiffuseRemapScale1.w, _DiffuseRemapScale2.w, _DiffuseRemapScale3.w }; // 1 is off
        splatControl = lerp(opacityAsDensity, splatControl, useOpacityAsDensityParam);
    }
    #endif
    
    // Now that splatControl has changed, we can compute the final weight and normalize
    weight = dot(splatControl, 1.0h)+dot(splatControl2, 1.0h);
    splatControl /= (weight + HALF_MIN);
    splatControl2 /= (weight + HALF_MIN);

    mixedDiffuse = 0.0h;
    mixedDiffuse += diffAlbedo[0] * half4(_DiffuseRemapScale0.rgb * splatControl.rrr, 1.0h);
    mixedDiffuse += diffAlbedo[1] * half4(_DiffuseRemapScale1.rgb * splatControl.ggg, 1.0h);
    mixedDiffuse += diffAlbedo[2] * half4(_DiffuseRemapScale2.rgb * splatControl.bbb, 1.0h);
    mixedDiffuse += diffAlbedo[3] * half4(_DiffuseRemapScale3.rgb * splatControl.aaa, 1.0h);

    #ifdef _CONTROLSECOND_ON
    mixedDiffuse += diffAlbedo[5] * half4(_DiffuseRemapScale5.rgb * splatControl2.ggg, 1.0h);
    mixedDiffuse += diffAlbedo[4] * half4(_DiffuseRemapScale4.rgb * splatControl2.rrr, 1.0h);
    #endif

    //测试代码
    // nrm = SAMPLE_TEXTURE2D(_Normal0, sampler_Normal0, uvSplat01.xy);
    // nrm = normalize(float3(nrm.xy,1));
    // nrm = RebuildNormalFromRG(nrm.xy);
    //  mixedDiffuse = float4(nrm.rg,1,1);
    //mixedDiffuse = float4(height0.rgb,Height.r);
    //mixedNormal = float3(nrm.rg,1);

    
}


void SplatmapFinalColor(inout half4 color, half fogCoord)
{
    color.rgb *= color.a;

    color.rgb = MixFog(color.rgb, fogCoord);

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

    o.uvSplat01.xy = TRANSFORM_TEX(v.texcoord, _Splat0);
    o.uvSplat01.zw = TRANSFORM_TEX(v.texcoord, _Splat1);
    o.uvSplat23.xy = TRANSFORM_TEX(v.texcoord, _Splat2);
    o.uvSplat23.zw = TRANSFORM_TEX(v.texcoord, _Splat3);
    o.uvSplat45.xy = TRANSFORM_TEX(v.texcoord, _Splat4);
    o.uvSplat45.zw = TRANSFORM_TEX(v.texcoord, _Splat5);

    half3 viewDirWS = GetCameraPositionWS() - Attributes.positionWS;
#if !SHADER_HINT_NICE_QUALITY
    viewDirWS = SafeNormalize(viewDirWS);
#endif

#if defined(_NORMALMAP)
    float4 vertexTangent = float4(cross(float3(0, 0, 1), v.normalOS), 1.0);
    VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS, vertexTangent);

    o.normal = half4(normalInput.normalWS, viewDirWS.x);
    o.tangent = half4(normalInput.tangentWS, viewDirWS.y);
    o.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);
#else
    o.normal = TransformObjectToWorldNormal(v.normalOS);
    o.viewDir = viewDirWS;
    //o.vertexSH = SampleSH(o.normal);
#endif
    // o.fogFactorAndVertexLight.x = ComputeFogFactor(Attributes.positionCS.z);
    // o.fogFactorAndVertexLight.yzw = VertexLighting(Attributes.positionWS, o.normal.xyz);

    half fogFactor = 0;
    #if !defined(_FOG_FRAGMENT)
    fogFactor = ComputeFogFactor(Attributes.positionCS.z);
    #endif
    #ifdef _ADDITIONAL_LIGHTS_VERTEX
    o.fogFactorAndVertexLight.x = fogFactor;
    o.fogFactorAndVertexLight.yzw = VertexLighting(Attributes.positionWS, o.normal.xyz);
    #else
    o.fogFactor = fogFactor;
    #endif
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
    half4 splatControl2 = half4(0,0,0,0);
    
    #ifdef _CONTROLSECOND_ON
    splatControl2 = SAMPLE_TEXTURE2D(_Control2, sampler_Control, splatUV);
    #endif
    

    half alpha = dot(splatControl, 1.0h)+dot(splatControl2, 1.0h);
    
    half weight;
    half4 mixedDiffuse;
    half4 defaultSmoothness;
    half4 defaultSmoothness2;
    SplatmapMix(IN.uvMainAndLM, IN.uvSplat01, IN.uvSplat23, IN.uvSplat45,splatControl,splatControl2, weight, mixedDiffuse, defaultSmoothness, defaultSmoothness2, normalTS);
    half3 albedo = mixedDiffuse.rgb;
    //return float4(normalTS.xyz,1);
    //return half4(mixedDiffuse.rgb,1);

    half smoothness = dot(splatControl, defaultSmoothness)+dot(splatControl2, defaultSmoothness2);
    smoothness = clamp(smoothness,0,1);

    
    half4 defaultMetallic = half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3);
    half4 defaultMetallic2 = half4(_Metallic4, _Metallic5, 0, 0);
    half metallic = dot(splatControl, defaultMetallic)+dot(splatControl2, defaultMetallic2);

    half4 defaultOcclusion = 1;
    half4 defaultOcclusion2 = 1;
    half occlusion = dot(splatControl, defaultOcclusion)+dot(splatControl2, defaultOcclusion);
    
    //AHD光照相关
    half4 BakedLmDir = 0.0;
    half3 BakedLm = 0.0;

    InputData inputData;
    InitializeInputData(IN, normalTS, inputData);
    
    #if defined(DYNAMICLIGHTMAP_ON)
    inputdata.bakedGI = float3(0,0,0);//SAMPLE_GI(i.staticLightmapUV, i.dynamicLightmapUV, i.vertexSH, inputdata.normalWS);
    #else
    //inputData.bakedGI = SAMPLE_GI(IN.staticLightmapUV, IN.vertexSH, inputData.normalWS);
    //使用支持AHD光照的获得GI的方法(若烘焙了两张光照贴图则会返回其数值至BakedLmDir和BakedLm)
    inputData.bakedGI = SAMPLE_GI_AHD(IN.staticLightmapUV, IN.vertexSH, inputData.normalWS,BakedLmDir,BakedLm);
                
    #endif
    //half4 color = UniversalFragmentPBR(inputData, albedo, metallic, /* specular */ half3(0.0h, 0.0h, 0.0h), smoothness, occlusion, /* emission */ half3(0, 0, 0), alpha);
    //return float4(inputData.bakedGI.rgb,1);
    half4 color = UniversalFragmentPBR_SgameAHD(inputData, albedo, metallic, /* specular */ half3(0.0h, 0.0h, 0.0h), smoothness, occlusion, /* emission */ half3(0, 0, 0), alpha,BakedLmDir,BakedLm);
    SplatmapFinalColor(color, inputData.fogCoord);
    
    half3 baseColor =color.rgb; 
    /*#ifdef _DBUFFER
        ApplyDecalToBaseColor(IN.clipPos, baseColor);
    #endif*/
    color.rgb = baseColor;


    return half4(color.rgb, 1.0h);
}



#endif
