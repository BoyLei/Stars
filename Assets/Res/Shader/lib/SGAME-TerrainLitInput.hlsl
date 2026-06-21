#ifndef UNIVERSAL_TERRAIN_LIT_INPUT_INCLUDED
#define UNIVERSAL_TERRAIN_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


//CBUFFER_START(_Terrain)
CBUFFER_START(UnityPerMaterial)
    half _NormalScale0, _NormalScale1, _NormalScale2, _NormalScale3, _NormalScale4, _NormalScale5;
    half _Metallic0, _Metallic1, _Metallic2, _Metallic3, _Metallic4, _Metallic5;
    half _Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3, _Smoothness4, _Smoothness5;
    half4 _DiffuseRemapScale0, _DiffuseRemapScale1, _DiffuseRemapScale2, _DiffuseRemapScale3, _DiffuseRemapScale4, _DiffuseRemapScale5;
    int _IfAlpha_Smoothness0, _IfAlpha_Smoothness1, _IfAlpha_Smoothness2,_IfAlpha_Smoothness3, _IfAlpha_Smoothness4, _IfAlpha_Smoothness5;
    int _IfAlpha_Height0, _IfAlpha_Height1, _IfAlpha_Height2, _IfAlpha_Height3, _IfAlpha_Height4, _IfAlpha_Height5;
    float _HeightScale0,_HeightScale1,_HeightScale2,_HeightScale3,_HeightScale4,_HeightScale5;
    int _ControlSecond;
    int _NumLayersCount;

    float _HeightTransition;
    float4 _Control_ST;
    float4 _Control_TexelSize;
    half4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST, _Splat4_ST, _Splat5_ST;
    float4 _Color0,_Color1,_Color2,_Color3,_Color4,_Color5;

CBUFFER_END

// #ifdef UNITY_DOTS_INSTANCING_ENABLED
// UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
//     UNITY_DOTS_INSTANCED_PROP(half, _NormalScale0)
//     UNITY_DOTS_INSTANCED_PROP(half, _NormalScale1)
//     UNITY_DOTS_INSTANCED_PROP(half, _NormalScale2)
//     UNITY_DOTS_INSTANCED_PROP(half, _NormalScale3)
//     UNITY_DOTS_INSTANCED_PROP(half, _NormalScale4)
//     UNITY_DOTS_INSTANCED_PROP(half, _NormalScale5)
//
//     UNITY_DOTS_INSTANCED_PROP(half, _Metallic0)
//     UNITY_DOTS_INSTANCED_PROP(half, _Metallic1)
//     UNITY_DOTS_INSTANCED_PROP(half, _Metallic2)
//     UNITY_DOTS_INSTANCED_PROP(half, _Metallic3)
//     UNITY_DOTS_INSTANCED_PROP(half, _Metallic4)
//     UNITY_DOTS_INSTANCED_PROP(half, _Metallic5)
//
//     UNITY_DOTS_INSTANCED_PROP(half, _Smoothness0)
//     UNITY_DOTS_INSTANCED_PROP(half, _Smoothness1)
//     UNITY_DOTS_INSTANCED_PROP(half, _Smoothness2)
//     UNITY_DOTS_INSTANCED_PROP(half, _Smoothness3)
//     UNITY_DOTS_INSTANCED_PROP(half, _Smoothness4)
//     UNITY_DOTS_INSTANCED_PROP(half, _Smoothness5)
//
//     UNITY_DOTS_INSTANCED_PROP(half4, _DiffuseRemapScale0)
//     UNITY_DOTS_INSTANCED_PROP(half4, _DiffuseRemapScale1)
//     UNITY_DOTS_INSTANCED_PROP(half4, _DiffuseRemapScale2)
//     UNITY_DOTS_INSTANCED_PROP(half4, _DiffuseRemapScale3)
//     UNITY_DOTS_INSTANCED_PROP(half4, _DiffuseRemapScale4)
//     UNITY_DOTS_INSTANCED_PROP(half4, _DiffuseRemapScale5)
//
//     UNITY_DOTS_INSTANCED_PROP(flaoat4, _Control_ST)
//     UNITY_DOTS_INSTANCED_PROP(flaoat4, _Control_TexelSize)
//
//     UNITY_DOTS_INSTANCED_PROP(half4,_Splat0_ST)
//     UNITY_DOTS_INSTANCED_PROP(half4,_Splat1_ST)
//     UNITY_DOTS_INSTANCED_PROP(half4,_Splat2_ST)
//     UNITY_DOTS_INSTANCED_PROP(half4,_Splat3_ST)
//     UNITY_DOTS_INSTANCED_PROP(half4,_Splat4_ST)
//     UNITY_DOTS_INSTANCED_PROP(half4,_Splat5_ST)
//
//     UNITY_DOTS_INSTANCED_PROP(float4,_Color0)
//     UNITY_DOTS_INSTANCED_PROP(float4,_Color1)
//     UNITY_DOTS_INSTANCED_PROP(float4,_Color2)
//     UNITY_DOTS_INSTANCED_PROP(float4,_Color3)
//     UNITY_DOTS_INSTANCED_PROP(float4,_Color4)
//     UNITY_DOTS_INSTANCED_PROP(float4,_Color5)
//
// UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
//
// #define _NormalScale0  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half , Metadata_NormalScale0)
// #define _NormalScale1  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half , Metadata_NormalScale1)
// #define _NormalScale2  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half , Metadata_NormalScale2)
// #define _NormalScale3  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half , Metadata_NormalScale3)
// #define _NormalScale4  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half , Metadata_NormalScale4)
// #define _NormalScale5  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half , Metadata_NormalScale5)
//
// #define _Metallic0  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half , Metadata_Metallic0)
// #define _Metallic1  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half , Metadata_Metallic1)
// #define _Metallic2  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half , Metadata_Metallic2)
// #define _Metallic3  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half , Metadata_Metallic3)
// #define _Metallic4  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half , Metadata_Metallic4)
// #define _Metallic5  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half , Metadata_Metallic5)
//
// #define _Smoothness0  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half , Metadata_Smoothness0)
// #define _Smoothness1  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half , Metadata_Smoothness1)
// #define _Smoothness2  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half , Metadata_Smoothness2)
// #define _Smoothness3  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half , Metadata_Smoothness3)
// #define _Smoothness4  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half , Metadata_Smoothness4)
// #define _Smoothness5  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half , Metadata_Smoothness5)
//
// #define _DiffuseRemapScale0  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half4 , Metadata_DiffuseRemapScale0)
// #define _DiffuseRemapScale1  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half4 , Metadata_DiffuseRemapScale1)
// #define _DiffuseRemapScale2  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half4 , Metadata_DiffuseRemapScale2)
// #define _DiffuseRemapScale3  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half4 , Metadata_DiffuseRemapScale3)
// #define _DiffuseRemapScale4  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half4 , Metadata_DiffuseRemapScale4)
// #define _DiffuseRemapScale5  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half4 , Metadata_DiffuseRemapScale5)
//
// #define _Control_ST  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata_Control_ST)
// #define _Control_TexelSize  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata_Control_TexelSize)
//
// #define _Splat0_ST  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half4 , Metadata_Splat0_ST)
// #define _Splat1_ST  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half4 , Metadata_Splat1_ST)
// #define _Splat2_ST  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half4 , Metadata_Splat2_ST)
// #define _Splat3_ST  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half4 , Metadata_Splat3_ST)
// #define _Splat4_ST  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half4 , Metadata_Splat4_ST)
// #define _Splat5_ST  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half4 , Metadata_Splat5_ST)
//
// #define _Color0  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half4 , Metadata_Color0)
// #define _Color1  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half4 , Metadata_Color1)
// #define _Color2  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half4 , Metadata_Color2)
// #define _Color3  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half4 , Metadata_Color3)
// #define _Color4  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half4 , Metadata_Color4)
// #define _Color5  UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(half4 , Metadata_Color5)
//
// #endif


TEXTURE2D(_Control);    SAMPLER(sampler_Control);
TEXTURE2D(_Control2);
TEXTURE2D(_Splat0);     SAMPLER(sampler_Splat0);
TEXTURE2D(_Splat1);
TEXTURE2D(_Splat2);
TEXTURE2D(_Splat3);
TEXTURE2D(_Splat4);
TEXTURE2D(_Splat5);

#ifdef _NORMALMAP
TEXTURE2D(_Normal0);     SAMPLER(sampler_Normal0);
TEXTURE2D(_Normal1);
TEXTURE2D(_Normal2);
TEXTURE2D(_Normal3);
TEXTURE2D(_Normal4);
TEXTURE2D(_Normal5);
#endif

TEXTURE2D(_SmoothnessTex);
TEXTURE2D(_MainTex);       SAMPLER(sampler_MainTex);

#endif
