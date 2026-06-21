#ifndef UNIVERSAL_TERRAIN_LIT_INPUT_INCLUDED
#define UNIVERSAL_TERRAIN_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


CBUFFER_START(UnityPerMaterial)
    half _NormalScale0, _NormalScale1, _NormalScale2, _NormalScale3;
    half _Metallic0, _Metallic1, _Metallic2, _Metallic3;
    half _Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3;
    half4 _DiffuseRemapScale0, _DiffuseRemapScale1, _DiffuseRemapScale2, _DiffuseRemapScale3;
    //float _HeightScale0, _HeightScale1, _HeightScale2, _HeightScale3;
    float _HeightTransition;
    float4 _Control_ST;
    float4 _Control_TexelSize;
    half4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
    half4 _Color0, _Color1, _Color2, _Color3;
    half _NormalScaleRemap;
CBUFFER_END

half _GlobalOpaqueScaleFator;

TEXTURE2D_ARRAY(_SplatArray); SAMPLER(sampler_SplatArray);
TEXTURE2D_ARRAY(_NormalArray);
TEXTURE2D(_Control);    SAMPLER(sampler_Control);
TEXTURE2D(_SmoothnessTex);
TEXTURE2D(_MainTex);       SAMPLER(sampler_MainTex);
#endif
