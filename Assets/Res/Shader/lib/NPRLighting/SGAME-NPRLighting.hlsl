#ifndef UNIVERSAL_NPRLIGHTING_INCLUDED
#define UNIVERSAL_NPRLIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/AmbientOcclusion.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "./SGAME-NPRShaderUtils.hlsl"
#include "./SGAME-NPRInput.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/FernShaderLibrary/DeclareDepthShadowTexture.hlsl"
//#include "FernShaderUtils.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/FernShaderLibrary/NPRBSDF.hlsl"

// Global Property
half4 _DepthTextureSourceSize;
half _CameraAspect;
half _CameraFOV;

#define PI8 25.1327
#define INV_PI8 0.039789

struct SpecularData
{
    half specularSize;
    half specularSoftness;
    half specularABweight;
};

inline uint2 GetDepthUVOffset(half offset, half reverseX, half2 positionCSXY, half3 mainLightDir, half2 depthTexWH, NPRAddInputData addInputData)
{
    // 1 / depth when depth < 1 is wrong, this is like point light attenuation
    // 0.5625 is aspect, hard code for now
    // 0.333 is fov, hard code for now
    float2 UVOffset = _CameraAspect * (offset * 2 * _CameraFOV / (1 + addInputData.linearEyeDepth)); 
    half2 mainLightDirVS = TransformWorldToViewDir(mainLightDir, true).xz;
    mainLightDirVS.x *= lerp(1, -1, reverseX);
    UVOffset = mainLightDirVS * UVOffset;
    half2 downSampleFix = _DepthTextureSourceSize.zw / depthTexWH.xy;
    uint2 loadTexPos = positionCSXY / downSampleFix + UVOffset * depthTexWH.xy;
    loadTexPos = min(loadTexPos, depthTexWH.xy-1);
    return loadTexPos;
}

inline half DepthRim(half depthRimOffset, half reverseX, half rimDepthDiffThresholdOffset, half2 positionCSXY, half3 mainLightDir, NPRAddInputData addInputData)
{
    //_DepthTextureSourceSize = half4(0,0,1920,1080);
    int2 loadPos = GetDepthUVOffset(depthRimOffset, reverseX, positionCSXY, mainLightDir,  _DepthTextureSourceSize.zw, addInputData);
    float depthTextureValue = LoadSceneDepth(loadPos);
    float depthTextureLinearDepth = DepthSamplerToLinearDepth(depthTextureValue);
    half depthRim = saturate((depthTextureLinearDepth - (addInputData.linearEyeDepth + rimDepthDiffThresholdOffset)));
    depthRim = lerp(0, depthRim, addInputData.linearEyeDepth);
    return depthRim;
}

inline half DepthRimZZZ(half depthRimOffset, half3 positionCS, half3 normalWS, half RimLightThreshold, half RimLightFadeout/*, half3 mainLightDir, NPRAddInputData addInputData , half reverseX, half rimDepthDiffThresholdOffset*/)
{
    float linearEyeDepth = LinearEyeDepth(positionCS.z, _ZBufferParams);
    float3 normalVS = mul((float3x3)UNITY_MATRIX_V, normalWS);
    #ifdef _RIMLIGHTRIGHT
        float2 uvOffset = float2(abs(sign(normalVS.x)), sign(normalVS.y)) * depthRimOffset / (1 + linearEyeDepth) / 100;
    #elif  _RIMLIGHTLEFT
        float2 uvOffset = float2(-abs(sign(normalVS.x)), sign(normalVS.y)) * depthRimOffset / (1 + linearEyeDepth) / 100;
    #else
        float2 uvOffset = float2(sign(normalVS.x), sign(normalVS.y)) * depthRimOffset / (1 + linearEyeDepth) / 100;
    #endif

    float2 loadDepthTexPos = positionCS.xy + uvOffset * _ScaledScreenParams.xy; 
    loadDepthTexPos = max(0, min(loadDepthTexPos, _ScaledScreenParams.xy - 1));
    float sceneLinearEyeDepth = LoadSceneDepth(loadDepthTexPos);
    sceneLinearEyeDepth = LinearEyeDepth(sceneLinearEyeDepth, _ZBufferParams);
    // half _RimLightFadeout = 0.1;
    // half _RimLightThreshold = 0.01;
    float rimLight = saturate(sceneLinearEyeDepth - (linearEyeDepth + RimLightThreshold)) / RimLightFadeout;
    rimLight = saturate(rimLight);
    //rimLight = sceneLinearEyeDepth;
    return rimLight;
}
//三次拟合曲线, 根据point1 point2 point3 进行曲线拟合
half CurveFitting3(half x, half2 point1, half2 point2, half2 point3)
{
    half x1 = point1.x;
    half y1 = point1.y;
    half x2 = point2.x;
    half y2 = point2.y;
    half x3 = point3.x;
    half y3 = point3.y;
    half Xpow3 = pow(x,3);
    half X1pow3 = pow(point1.x,3);
    half X2pow3 = pow(point2.x,3);
    half X3pow3 = pow(point3.x,3);
    half y = -1 * (Xpow3*x1*y2 - Xpow3*x1*y3 - Xpow3*x2*y1 + Xpow3*x2*y3 + Xpow3*x3*y1 - Xpow3*x3*y2 - x*X1pow3*y2 +x*X1pow3*y3
            + x*X2pow3*y1 - x*X2pow3*y3 - x*X3pow3*y1 +x*X3pow3*y2 - X1pow3*x2*y3 + X1pow3*x3*y2 +x1*X2pow3*y3 - x1*X3pow3*y2 - X2pow3*x3*y1 + x2*X3pow3*y1);
    
    y = y/(x1-x2)*(x1-x3)*(x2-x3)*(x1+x2+x3);

    return y;
}

void NPRSpecularController(half smoothness, inout half specularSize, inout half specularSoftness, inout half albedoWeight)
{
    smoothness = saturate(smoothness);
    //specularSize = CurveFitting3(smoothness, half2(0,1), half2(0.15,0.778),half2(1,0));
    specularSoftness = 1 - smoothness;
    albedoWeight = CurveFitting3(smoothness, half2(0,1), half2(0.59,0.49),half2(1,-1));

    
    specularSize = (0.1111)/(smoothness + 0.1111);
    albedoWeight = 1 - 2 *smoothness;
}



half3 StylizedSpecular(half3 albedo, half ndothClamp, half specularSize, half specularSoftness, half albedoWeight)
{
    half specSize = 1 - (specularSize * specularSize);
    half ndothStylized = (ndothClamp - specSize * specSize) / (1 - specSize);
    half3 specular = LinearStep(0, specularSoftness, ndothStylized);
    specular = lerp(specular, albedo * specular, albedoWeight);
    return specular;
}

// Computes the scalar specular term for Minimalist CookTorrance BRDF
// NOTE: needs to be multiplied with reflectance f0, i.e. specular color to complete
half DirectBRDFSpecularNPR(half roughness2MinusOne, half roughness2, half normalizationTerm, half3 normalWS, half3 lightDirectionWS, half3 viewDirectionWS)
{
    float3 lightDirectionWSFloat3 = float3(lightDirectionWS);
    float3 halfDir = SafeNormalize(lightDirectionWSFloat3 + float3(viewDirectionWS));

    float NoH = saturate(dot(float3(normalWS), halfDir));
    half LoH = half(saturate(dot(lightDirectionWSFloat3, halfDir)));

    // GGX Distribution multiplied by combined approximation of Visibility and Fresnel
    // BRDFspec = (D * V * F) / 4.0
    // D = roughness^2 / ( NoH^2 * (roughness^2 - 1) + 1 )^2
    // V * F = 1.0 / ( LoH^2 * (roughness + 0.5) )
    // See "Optimizing PBR for Mobile" from Siggraph 2015 moving mobile graphics course
    // https://community.arm.com/events/1155

    // Final BRDFspec = roughness^2 / ( NoH^2 * (roughness^2 - 1) + 1 )^2 * (LoH^2 * (roughness + 0.5) * 4.0)
    // We further optimize a few light invariant terms
    // brdfData.normalizationTerm = (roughness + 0.5) * 4.0 rewritten as roughness * 4.0 + 2.0 to a fit a MAD.
    float d = NoH * NoH * roughness2MinusOne + 1.00001f;

    half LoH2 = LoH * LoH;
    half specularTerm = roughness2 / ((d * d) * max(0.1h, LoH2) * normalizationTerm);

    // On platforms where half actually means something, the denominator has a risk of overflow
    // clamp below was added specifically to "fix" that, but dx compiler (we convert bytecode to metal/gles)
    // sees that specularTerm have only non-negative terms, so it skips max(0,..) in clamp (leaving only min(100,...))
    #if defined (SHADER_API_MOBILE) || defined (SHADER_API_SWITCH)
    specularTerm = specularTerm - HALF_MIN;
    specularTerm = clamp(specularTerm, 0.0, 100.0); // Prevent FP16 overflow on mobiles
    #endif

    return specularTerm;
}

// [Burley 2012, "Physically-Based Shading at Disney"]
float D_GGXaniso(float ax, float ay, float NoH, float XoH, float YoH)
{
    float a2 = ax * ay;
    float3 V = float3(ay * XoH, ax * YoH, a2 * NoH);
    float S = dot(V, V);

    return(1.0f / PI) * a2 * pow(a2 / S,2);
}

float4 GaussianBlurSample(Texture2D BlurTexture, SamplerState samplerBlurTexture, float2 uv)
{
    float4 col = float4(0,0,0,0);

    float blurSize = 3;
    float weight[5];
    weight[0] = 0.2270270270;
    weight[1] = 0.1945945946;
    weight[2] = 0.1216216216;
    weight[3] = 0.0540540541;
    weight[4] = 0.0162162162;

    // Horizontal blur
    for (int x = -4; x <= 4; x++)
    {
        col += SAMPLE_TEXTURE2D(BlurTexture,samplerBlurTexture, uv + float2(x, 0) * blurSize / _ScreenParams.xy) * weight[abs(x)];
    }

    // Vertical blur
    float4 col2 = float4(0, 0, 0, 0);
    for (int y = -4; y <= 4; y++)
    {
         col2 += SAMPLE_TEXTURE2D(BlurTexture,samplerBlurTexture, uv + float2(0, y) * blurSize / _ScreenParams.xy) * weight[abs(y)];
   }

    return (col + col2) / 2.0;
}

// [Heitz 2014, "Understanding the Masking-Shadowing Function in Microfacet-Based BRDFs"]
//这个函数里面的x就是tangent y就是bitangent 中间的o就是dot计算结果
float Vis_SmithJointAniso(float ax, float ay, float NoV, float NoL, float XoV, float XoL, float YoV, float YoL)
{
    float Vis_SmithV = NoL * length(float3(ax * XoV, ay * YoV, NoV));
    float Vis_SmithL = NoV * length(float3(ax * XoL, ay * YoL, NoL));
    return 0.5 * rcp(Vis_SmithV + Vis_SmithL);
}
// [Schlick 1994, "An Inexpensive BRDF Model for Physically-Based Rendering"]
float3 F_Schlick_UE4(float3 SpecularColor, float VoH)
{
    float Fc = pow(Pow4(1 - VoH),2);					// 1 sub, 3 mul
    //return Fc + (1 - Fc) * SpecularColor;		// 1 add, 3 mad
	
    // Anything less than 2% is physically impossible and is instead considered to be shadowing
    return saturate(50.0 * SpecularColor.g) * Fc + (1 - Fc) * SpecularColor;
}

float3 SlikBRDF_Specular(/*float3 DiffuseColor,*/ float3 SpecularColor, float Roughness, float Anisotropy,
float3 N, float3 T, float3 B, float3 V, float3 L, float3 LightColor, float Shadow)
{
    float Alpha = Roughness * Roughness;
    float a2 = Alpha * Alpha;
    // Anisotropic parameters: ax and ay are the Roughness along the tangent and bitangent
    // Kulla 2017, "Revisiting Physically Based Shading at Imageworks"
    float ax = max(Alpha * (1.0 + Anisotropy), 0.001f);
    float ay = max(Alpha * (1.0 - Anisotropy), 0.001f);
    float3 H = normalize(L + V);
    float NoH = saturate(dot(N, H));
    float NoV = saturate(abs(dot(N, V)) + 1e-5);
    float NoL = saturate(dot(N, L));
    float VoH = saturate(dot(V, H));

    float XoV = dot(T, V);
    float XoL = dot(T, L);
    float XoH = dot(T, H);
    float YoV = dot(B, V);
    float YoL = dot(B, L);
    float YoH = dot(B, H);

    float3 Radiance = NoL * LightColor * Shadow * PI;
	
    //直接光漫反射
    //float3 DiffuseTerm = Diffuse_Lambert(DiffuseColor) * Radiance;

    //直接光镜面反射
    float D = D_GGXaniso(ax, ay, NoH, XoH, YoH);
    float Vis = Vis_SmithJointAniso(ax, ay, NoV, NoL, XoV, XoL, YoV, YoL);
    float3 F = F_Schlick_UE4(SpecularColor, VoH);
    float3 SpecularTerm = ((clamp(0,1,D * Vis)) * F) * Radiance;

    return /*DiffuseTerm + */SpecularTerm;
}


//卡通角色用GI
half3 GlobalIllumination_SGChar(BRDFData brdfData, BRDFData brdfDataClearCoat, float clearCoatMask,
    half3 bakedGI, half occlusion, float3 positionWS,
    half3 normalWS, half3 viewDirectionWS)
{
    half3 reflectVector = reflect(-viewDirectionWS, normalWS);
    half NoV = saturate(dot(normalWS, viewDirectionWS));
    half fresnelTerm = Pow4(1.0 - NoV) * 0;

    half3 indirectDiffuse = bakedGI;
    half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, positionWS, brdfData.perceptualRoughness, 1.0h);

    half3 color = EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);

    if (IsOnlyAOLightingFeatureEnabled())
    {
        color = half3(1,1,1); // "Base white" for AO debug lighting mode
    }

    #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    half3 coatIndirectSpecular = GlossyEnvironmentReflection(reflectVector, positionWS, brdfDataClearCoat.perceptualRoughness, 1.0h);
    // TODO: "grazing term" causes problems on full roughness
    half3 coatColor = EnvironmentBRDFClearCoat(brdfDataClearCoat, clearCoatMask, coatIndirectSpecular, fresnelTerm);

    // Blend with base layer using khronos glTF recommended way using NoV
    // Smooth surface & "ambiguous" lighting
    // NOTE: fresnelTerm (above) is pow4 instead of pow5, but should be ok as blend weight.
    half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * fresnelTerm;
    return (color * (1.0 - coatFresnel * clearCoatMask) + coatColor) * occlusion;
    #else
    return color * occlusion;
        #endif
}


half3 LightingPhysicallyBased_SGChar(BRDFData brdfData, BRDFData brdfDataClearCoat,
    half3 lightColor, half3 lightDirectionWS, half lightAttenuation,
    half3 normalWS, half3 viewDirectionWS,
    half clearCoatMask, bool specularHighlightsOff)
{
    half NdotL = saturate(dot(normalWS, lightDirectionWS)) * 0.5 + 0.5;
    half3 radiance = lightColor * (lightAttenuation * NdotL);

    half3 brdf = brdfData.diffuse;
    #ifndef _SPECULARHIGHLIGHTS_OFF
    [branch] if (!specularHighlightsOff)
    {
        //brdf += brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS);
        brdf += clamp(half3(0,0,0),half3(0.5,0.5,0.5),brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS));
        
        
        #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
        // Clear coat evaluates the specular a second timw and has some common terms with the base specular.
        // We rely on the compiler to merge these and compute them only once.
        half brdfCoat = kDielectricSpec.r * DirectBRDFSpecular(brdfDataClearCoat, normalWS, lightDirectionWS, viewDirectionWS);

        // Mix clear coat and base layer using khronos glTF recommended formula
        // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_materials_clearcoat/README.md
        // Use NoV for direct too instead of LoH as an optimization (NoV is light invariant).
        half NoV = saturate(dot(normalWS, viewDirectionWS));
        // Use slightly simpler fresnelTerm (Pow4 vs Pow5) as a small optimization.
        // It is matching fresnel used in the GI/Env, so should produce a consistent clear coat blend (env vs. direct)
        half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * Pow4(1.0 - NoV);

        brdf = brdf * (1.0 - clearCoatMask * coatFresnel) + brdfCoat * clearCoatMask;
        #endif // _CLEARCOAT
    }
    #endif // _SPECULARHIGHLIGHTS_OFF

    return brdf * radiance;
}

half3 LightingPhysicallyBased_SGChar_ZZZ(BRDFData brdfData, BRDFData brdfDataClearCoat,
    half3 lightColor, half3 lightDirectionWS, half lightAttenuation,
    half3 normalWS, half3 viewDirectionWS,
    half clearCoatMask, bool specularHighlightsOff, SpecularData specularData)
{
    half NdotL = saturate(dot(normalWS, lightDirectionWS)) * 0.5 + 0.5;
    half3 halfDir = SafeNormalize(lightDirectionWS + viewDirectionWS);
    half NdotHClamp = saturate(dot(normalWS,halfDir));
    half3 radiance = lightColor * (lightAttenuation * NdotL);

    half3 brdf = brdfData.diffuse;
    #ifndef _SPECULARHIGHLIGHTS_OFF
    [branch] if (!specularHighlightsOff)
    {
        //brdf += brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS);
        //brdf += clamp(half3(0,0,0),half3(0.5,0.5,0.5),brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS));
        //使用风格化高光,是否和传统高光相融合 再议
        brdf += brdfData.specular * StylizedSpecular(brdfData.albedo, NdotHClamp,specularData.specularSize,specularData.specularSoftness,specularData.specularABweight);
        
        #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
        // Clear coat evaluates the specular a second timw and has some common terms with the base specular.
        // We rely on the compiler to merge these and compute them only once.
        half brdfCoat = kDielectricSpec.r * DirectBRDFSpecular(brdfDataClearCoat, normalWS, lightDirectionWS, viewDirectionWS);

        // Mix clear coat and base layer using khronos glTF recommended formula
        // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_materials_clearcoat/README.md
        // Use NoV for direct too instead of LoH as an optimization (NoV is light invariant).
        half NoV = saturate(dot(normalWS, viewDirectionWS));
        // Use slightly simpler fresnelTerm (Pow4 vs Pow5) as a small optimization.
        // It is matching fresnel used in the GI/Env, so should produce a consistent clear coat blend (env vs. direct)
        half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * Pow4(1.0 - NoV);

        brdf = brdf * (1.0 - clearCoatMask * coatFresnel) + brdfCoat * clearCoatMask;
        #endif // _CLEARCOAT
    }
    #endif // _SPECULARHIGHLIGHTS_OFF

    return brdf * radiance;
}

half3 LightingPhysicallyBased_SGChar_ZZZ(BRDFData brdfData, BRDFData brdfDataClearCoat, Light light, half3 normalWS, half3 viewDirectionWS, half clearCoatMask, bool specularHighlightsOff,SpecularData specularData,float shadowStrength)
{
    //return half3(1 -(1 - light.distanceAttenuation * light.shadowAttenuation)*shadowStrength,1 -(1 - light.distanceAttenuation * light.shadowAttenuation)*shadowStrength,1 -(1 - light.distanceAttenuation * light.shadowAttenuation)*shadowStrength);
    return LightingPhysicallyBased_SGChar_ZZZ(brdfData, brdfDataClearCoat, light.color, light.direction, 1 -(1 - light.distanceAttenuation * light.shadowAttenuation)*shadowStrength, normalWS, viewDirectionWS, clearCoatMask, specularHighlightsOff,specularData);
    
}

//From Lighting.hlsl
half3 LightingPhysicallyBased_SGChar(BRDFData brdfData, BRDFData brdfDataClearCoat, Light light, half3 normalWS, half3 viewDirectionWS, half clearCoatMask, bool specularHighlightsOff)
{
    return LightingPhysicallyBased_SGChar(brdfData, brdfDataClearCoat, light.color, light.direction, light.distanceAttenuation * light.shadowAttenuation, normalWS, viewDirectionWS, clearCoatMask, specularHighlightsOff);
}


//From Lighting.hlsl
half4 UniversalFragment_SGChar(InputData inputData, SurfaceData surfaceData)
{
    #if defined(_SPECULARHIGHLIGHTS_OFF)
    bool specularHighlightsOff = true;
    #else
    bool specularHighlightsOff = false;
    #endif
    BRDFData brdfData;

    // NOTE: can modify "surfaceData"...
    InitializeBRDFData(surfaceData, brdfData);

    #if defined(DEBUG_DISPLAY)
    half4 debugColor;

    if (CanDebugOverrideOutputColor(inputData, surfaceData, brdfData, debugColor))
    {
        return debugColor;
    }
    #endif

    // Clear-coat calculation...
    BRDFData brdfDataClearCoat = CreateClearCoatBRDFData(surfaceData, brdfData);
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    uint meshRenderingLayers = GetMeshRenderingLightLayer();
    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);
    //return  half4(mainLight.shadowAttenuation,mainLight.shadowAttenuation,mainLight.shadowAttenuation,1);

    // NOTE: We don't apply AO to the GI here because it's done in the lighting calculation below...
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);

    LightingData lightingData = CreateLightingData(inputData, surfaceData);

    lightingData.giColor = GlobalIllumination_SGChar(brdfData, brdfDataClearCoat, surfaceData.clearCoatMask,
                                              inputData.bakedGI, aoFactor.indirectAmbientOcclusion, inputData.positionWS,
                                              inputData.normalWS, inputData.viewDirectionWS);

    //lightingData.giColor; //暂时提亮试试
    //return half4(lightingData.giColor.rgb,1);
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
    {
        lightingData.mainLightColor = LightingPhysicallyBased_SGChar(brdfData, brdfDataClearCoat,
                                                              mainLight,
                                                              inputData.normalWS, inputData.viewDirectionWS,
                                                              surfaceData.clearCoatMask, specularHighlightsOff);
    }

    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    #if USE_CLUSTERED_LIGHTING
    for (uint lightIndex = 0; lightIndex < min(_AdditionalLightsDirectionalCount, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        {
            lightingData.additionalLightsColor += LightingPhysicallyBased_SGChar(brdfData, brdfDataClearCoat, light,
                                                                          inputData.normalWS, inputData.viewDirectionWS,
                                                                          surfaceData.clearCoatMask, specularHighlightsOff);
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        {
            lightingData.additionalLightsColor += LightingPhysicallyBased_SGChar(brdfData, brdfDataClearCoat, light,
                                                                          inputData.normalWS, inputData.viewDirectionWS,
                                                                          surfaceData.clearCoatMask, specularHighlightsOff);
        }
    LIGHT_LOOP_END
    #endif

    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
    lightingData.vertexLightingColor += inputData.vertexLighting * brdfData.diffuse;
    #endif

    return CalculateFinalColor(lightingData, surfaceData.alpha);
}


//From Lighting.hlsl
//展示台角色用PBR
half4 UniversalFragment_SGChar(InputData inputData, half3 albedo, half metallic, half3 specular,
    half smoothness, half occlusion, half3 emission, half alpha)
{
    SurfaceData surfaceData;

    surfaceData.albedo = albedo;
    surfaceData.specular = specular;
    surfaceData.metallic = metallic;
    surfaceData.smoothness = smoothness;
    surfaceData.normalTS = half3(0, 0, 1);
    surfaceData.emission = emission;
    surfaceData.occlusion = occlusion;
    surfaceData.alpha = alpha;
    surfaceData.clearCoatMask = 0;
    surfaceData.clearCoatSmoothness = 1;

    return UniversalFragment_SGChar(inputData, surfaceData);
}

//From Lighting.hlsl
//展示台角色用PBR_2
half4 UniversalFragment_SGChar_ZZZ(InputData inputData, SurfaceData surfaceData,SpecularData specularData,half mainLightStrength,half shadowStrength)
{
    #if defined(_SPECULARHIGHLIGHTS_OFF)
    bool specularHighlightsOff = true;
    #else
    bool specularHighlightsOff = false;
    #endif
    BRDFData brdfData;

    // NOTE: can modify "surfaceData"...
    InitializeBRDFData(surfaceData, brdfData);

    #if defined(DEBUG_DISPLAY)
    half4 debugColor;

    if (CanDebugOverrideOutputColor(inputData, surfaceData, brdfData, debugColor))
    {
        return debugColor;
    }
    #endif

    // Clear-coat calculation...
    BRDFData brdfDataClearCoat = CreateClearCoatBRDFData(surfaceData, brdfData);
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    uint meshRenderingLayers = GetMeshRenderingLightLayer();
    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);
    mainLight.color *= mainLightStrength;
    //mainLight.shadowAttenuation = saturate(1*(1 - shadowStrength) + mainLight.shadowAttenuation);
    //return  half4(mainLight.shadowAttenuation,mainLight.shadowAttenuation,mainLight.shadowAttenuation,1);

    // NOTE: We don't apply AO to the GI here because it's done in the lighting calculation below...
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);

    LightingData lightingData = CreateLightingData(inputData, surfaceData);

    lightingData.giColor = GlobalIllumination_SGChar(brdfData, brdfDataClearCoat, surfaceData.clearCoatMask,
                                              inputData.bakedGI, aoFactor.indirectAmbientOcclusion, inputData.positionWS,
                                              inputData.normalWS, inputData.viewDirectionWS);

    //ZZZ把GI的明度设置为原本的一半
    half3 giColorHSV = RgbToHsv(lightingData.giColor);
    //giColorHSV.g *= 0.7;
    giColorHSV.b *= 0.5;
    lightingData.giColor = HsvToRgb(giColorHSV);
    //lightingData.giColor; //暂时提亮试试
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
    {
        //不受投影影响
        //mainLight.shadowAttenuation = 1;
        lightingData.mainLightColor = LightingPhysicallyBased_SGChar_ZZZ(brdfData, brdfDataClearCoat,
                                                              mainLight,
                                                              inputData.normalWS, inputData.viewDirectionWS,
                                                              surfaceData.clearCoatMask, specularHighlightsOff,specularData,shadowStrength);
    }

    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    #if USE_CLUSTERED_LIGHTING
    for (uint lightIndex = 0; lightIndex < min(_AdditionalLightsDirectionalCount, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        {
            lightingData.additionalLightsColor += LightingPhysicallyBased_SGChar(brdfData, brdfDataClearCoat, light,
                                                                          inputData.normalWS, inputData.viewDirectionWS,
                                                                          surfaceData.clearCoatMask, specularHighlightsOff);
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        {
            lightingData.additionalLightsColor += LightingPhysicallyBased_SGChar(brdfData, brdfDataClearCoat, light,
                                                                          inputData.normalWS, inputData.viewDirectionWS,
                                                                          surfaceData.clearCoatMask, specularHighlightsOff);
        }
    LIGHT_LOOP_END
    #endif

    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
    lightingData.vertexLightingColor += inputData.vertexLighting * brdfData.diffuse;
    #endif

    return CalculateFinalColor(lightingData, surfaceData.alpha);
}

half4 UniversalFragment_SGChar_ZZZ(InputData inputData, half3 albedo, half metallic, half3 specular,
    half smoothness, half occlusion, half3 emission, half alpha, SpecularData specularData,half mainLightStrength,half shadowStrength)
{
    SurfaceData surfaceData;

    surfaceData.albedo = albedo;
    surfaceData.specular = specular;
    surfaceData.metallic = metallic;
    surfaceData.smoothness = smoothness;
    surfaceData.normalTS = half3(0, 0, 1);
    surfaceData.emission = emission;
    surfaceData.occlusion = occlusion;
    surfaceData.alpha = alpha;
    surfaceData.clearCoatMask = 0;
    surfaceData.clearCoatSmoothness = 1;

    return UniversalFragment_SGChar_ZZZ(inputData, surfaceData,specularData,mainLightStrength,shadowStrength);
}

inline half3 SamplerMatCap(half4 matCapColor/*, half2 uv*/, half3 normalWS, TEXTURE2D_PARAM(matCapTex, sampler_matCapTex))
{
    half3 finalMatCapColor = 0;
    //#if _NORMALMAP
    half3 normalVS = mul((float3x3)UNITY_MATRIX_V, normalWS);
    half2 matcapUV = normalVS.xy * 0.5 + 0.5;
    //#else
    //half2 matcapUV = uv;
    //#endif
    half3 matCap = SAMPLE_TEXTURE2D(matCapTex, sampler_matCapTex, matcapUV).xyz;
    finalMatCapColor = matCap.xyz * matCapColor.rgb;
    return finalMatCapColor;
}

//////////////////////////////////////////
///Specular
half GGXDirectBRDFSpecular(BRDFData brdfData, half3 LoH, half3 NoH)
{
    float d = NoH.x * NoH.x * brdfData.roughness2MinusOne + 1.00001f;
    half LoH2 = LoH.x * LoH.x;
    half specularTerm = brdfData.roughness2 / ((d * d) * max(0.1h, LoH2) * brdfData.normalizationTerm);

    #if defined (SHADER_API_MOBILE) || defined (SHADER_API_SWITCH)
    specularTerm = specularTerm - HALF_MIN;
    specularTerm = clamp(specularTerm, 0.0, 100.0); // Prevent FP16 overflow on mobiles
    #endif

    return specularTerm;
}


half BlinnPhongSpecular(half shininess, half ndoth)
{
    half phongSmoothness = exp2(10 * shininess + 1);
    half normalize = (phongSmoothness + 7) * INV_PI8;
    half specular = max(pow(ndoth, phongSmoothness) * normalize, 1e-4);
    return specular;
}

// inline half3 AnisotropyDoubleSpecular(BRDFData brdfData, half2 uv, half4 tangentWS, InputData inputData, LightingData lightingData,
//     AnisoSpecularData anisoSpecularData, TEXTURE2D_PARAM(anisoDetailMap, sampler_anisoDetailMap))
// {
//     half specMask = 1; // TODO ADD Mask
//     half4 detailNormal = SAMPLE_TEXTURE2D(anisoDetailMap,sampler_anisoDetailMap, uv);
//
//     float2 jitter =(detailNormal.y-0.5) * float2(anisoSpecularData.spread1,anisoSpecularData.spread2);
//
//     float sgn = tangentWS.w;
//     float3 T = normalize(sgn * cross(inputData.normalWS.xyz, tangentWS.xyz));
//     //float3 T = normalize(tangentWS.xyz);
//
//     float3 t1 = ShiftTangent(T, inputData.normalWS.xyz, anisoSpecularData.specularShift + jitter.x);
//     float3 t2 = ShiftTangent(T, inputData.normalWS.xyz, anisoSpecularData.specularSecondaryShift + jitter.y);
//
//     float3 hairSpec1 = anisoSpecularData.specularColor * anisoSpecularData.specularStrength *
//         D_KajiyaKay(t1, lightingData.HalfDir, anisoSpecularData.specularExponent);
//     float3 hairSpec2 = anisoSpecularData.specularSecondaryColor * anisoSpecularData.specularSecondaryStrength *
//         D_KajiyaKay(t2, lightingData.HalfDir, anisoSpecularData.specularSecondaryExponent);
//
//     float3 F = F_Schlick(half3(0.2,0.2,0.2), lightingData.LdotHClamp);
//     half3 anisoSpecularColor = 0.25 * F * (hairSpec1 + hairSpec2) * lightingData.NdotLClamp * specMask * brdfData.specular;
//     return anisoSpecularColor;
// }

// inline half3 AngleRingSpecular(AngleRingSpecularData specularData, InputData inputData, half radiance, LightingData lightingData)
// {
//     half3 specularColor = 0;
//     half mask = specularData.mask;
//     float3 normalV = mul(UNITY_MATRIX_V, half4(inputData.normalWS, 0)).xyz;
//     float3 halfV = mul(UNITY_MATRIX_V, half4(lightingData.HalfDir, 0)).xyz;
//     half ndh = dot(normalize(normalV.xz), normalize(halfV.xz));
//
//     ndh = pow(ndh, 6) * specularData.width * radiance;
//
//     half lightFeather = specularData.softness * ndh;
//
//     half lightStepMax = saturate(1 - ndh + lightFeather);
//     half lightStepMin = saturate(1 - ndh - lightFeather);
//
//     half brightArea = LinearStep(lightStepMin, lightStepMax, min(mask, 0.99));
//     half3 lightColor_B = brightArea * specularData.brightColor;
//     half3 lightColor_S = LinearStep(specularData.threshold, 1, mask) * specularData.shadowColor;
//     specularColor = (lightColor_S + lightColor_B) * specularData.intensity;
//     return specularColor;
// }
//////////////////////////////////////////
//ToneMapping
#define gamma  2.2

// half3 getBaseColor(int i)
// {
// 	if (i == 0) return half3(1.0, 0.4, 0.0);
// 	if (i == 1) return half3(0.4, 1.0, 0.0);
// 	if (i == 2) return half3(0.0, 1.0, 0.4);
// 	if (i == 3) return half3(0.0, 0.4, 1.0);
// 	if (i == 4) return half3(0.4, 0.0, 1.0);
// 	if (i == 5) return half3(1.0, 0.0, 0.4);
//
// 	return half3(1.);
// }
//
// half3 getBaseColor()
// {
// 	float colorPerSecond = 0.5;
// 	int i = int(mod(colorPerSecond * iTime, 7.));
// 	int j = int(mod(float(i) + 1., 7.));
//
// 	return mix(getBaseColor(i), getBaseColor(j), fract(colorPerSecond * iTime));
// }

half3 GammaPowColor(half3 Color)
{
    return Color;
    //return half3(pow(Color.r, 1/gamma),pow(Color.g, 1/gamma),pow(Color.b, 1/gamma));
}

half3 linearToneMapping(half3 color)
{
	float exposure = 1.;
	color = clamp(exposure * color, 0., 1.);
	color = GammaPowColor(color);
	return color;
}

half3 simpleReinhardToneMapping(half3 color)
{
	float exposure = 1.5;
	color *= exposure/(1. + color / exposure);
	color = GammaPowColor(color);
	return color;
}

half3 lumaBasedReinhardToneMapping(half3 color)
{
	float luma = dot(color, half3(0.2126, 0.7152, 0.0722));
	float toneMappedLuma = luma / (1. + luma);
	color *= toneMappedLuma / luma;
	color = GammaPowColor(color);
	return color;
}

half3 whitePreservingLumaBasedReinhardToneMapping(half3 color)
{
	float white = 2.;
	float luma = dot(color, half3(0.2126, 0.7152, 0.0722));
	float toneMappedLuma = luma * (1. + luma / (white*white)) / (1. + luma);
	color *= toneMappedLuma / luma;
	color = GammaPowColor(color);
	return color;
}

half3 RomBinDaHouseToneMapping(half3 color)
{
    color = exp( -1.0 / ( 2.72*color + 0.15 ) );
	color = GammaPowColor(color);
	return color;
}

half3 filmicToneMapping(half3 color)
{
	color = max(half3(0,0,0), color - half3(0.004,0.004,0.004));
	color = (color * (6.2 * color + .5)) / (color * (6.2 * color + 1.7) + 0.06);
	return color;
}

half3 Uncharted2ToneMapping(half3 color)
{
	float A = 0.15;
	float B = 0.50;
	float C = 0.10;
	float D = 0.20;
	float E = 0.02;
	float F = 0.30;
	float W = 11.2;
	float exposure = 2.;
	color *= exposure;
	color = ((color * (A * color + C * B) + D * E) / (color * (A * color + B) + D * F)) - E / F;
	float white = ((W * (A * W + C * B) + D * E) / (W * (A * W + B) + D * F)) - E / F;
	color /= white;
	color = GammaPowColor(color);
	return color;
}


#endif
