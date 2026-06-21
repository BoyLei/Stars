Shader "SGAME/FX/SGAME_VFX_NastyFlowmap"
{
	Properties
	{
		_BaseMap ("Main Texture", 2D) = "white" {}
		[HDR]_BaseColor ("Main Color", Color) = (0, 0.66, 0.73, 1)
		
		[NoScaleOffset]_FlowMap ("Flow Map", 2D) = "white" {}
		_FlowStrength("Flow Strength", Range(-5, 5)) = 1
		_FlowSpeed("Flow Speed", Range(-10, 10)) = 10

		_ColorMask ("Colr Mask", 2D) = "black" {}
		[HDR]_MaskColorStart("Mask Color Start", Color) = (0, 0.66, 0.73, 1)
		[HDR]_MaskColorEnd("Mask Color End", Color) = (0, 0.66, 0.73, 1)
		_MaskFlowStrength("Mask Flow Strength", Range(-5, 5)) = 1
		_MaskFlowSpeed("Mask Flow Speed", Range(-10, 10)) = 10
		_MaskContrast("Mask Constrast", Range(0.01, 10)) = 10

		// Vertex offset
		[HideInInspector]_VTOTex_ST("VTOTex_ST", Vector) = (1,1,0,0)
		_VTOTex("VTOTex", 2D) = "white" {}
		_VTOFactor("VTOFactor", Float) = 0
		_VTOTex_Uspeed("VTOTex_Uspeed", Float) = 0
		_VTOTex_Vspeed("VTOTex_Vspeed", Float) = 0
		_VTOMaskTex("VTOMaskTex", 2D) = "white" {}
		[Toggle]_VTOFactorCustom("VTOFactorCustom", Float) = 0
		[Toggle]_VTOC("VTOC", Float) = 0
		[Toggle]_VTOCV("VTOCV", Float) = 0
		[Toggle]_VTOAR("VTOAR", Float) = 1
		[Toggle]_VTORemap("VTORemap", Float) = 0
		[Enum(Custom1,0,Custom2,1)]_VTOFactorC1("VTOFactorC1", Float) = 1
		_VTOFactorC2Vec4("_VTOFactorC2", Vector) = (0, 0, 0, 1)
		_VTOR("VTORotate", Range(0, 360)) = 0


		//_FlowSampleStep("Flow Sampe Step", Vector) = (0, 0.5, 1, 1.5)
		//[ToggleOff(_FLOWNOTAFFECTALPHA_OFF)]_FlowNotAffectAlpha_ON("FlowMap_On",Int) = 0

        [HideInInspector]_Cutoff("控制AlphaClip的閾值", Int) = 0.2
        [HideInInspector]_BlendMode("BlendMode", Float) = 0   
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcFactor", Float) = 1
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstFactor", Float) = 0
        [HideInInspector][Enum(UnityEngine.Rendering.CullMode)]_CullMode("CullMode", Float) = 0
		[HideInInspector] _ZWrite("__zw", Float) = 1.0
		[HideInInspector] _ZTest("__zt", Float) = 4
		[HideInInspector] _Cull("__cull", Float) = 2
	}
	SubShader 
	{
		HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		CBUFFER_START(UnityPerMaterial)
		float4 _BaseMap_ST;
		float4 _BaseColor;

		half _FlowStrength;
		half _FlowSpeed;

		half4 _MaskColorStart;
		half4 _MaskColorEnd;

		half _MaskFlowStrength;
		half _MaskFlowSpeed;

		
		uniform float _VTOC;
		uniform float _VTOTex_Uspeed;
		uniform float _VTOTex_Vspeed;
		uniform float4 _VTOTex_ST;
		uniform float _VTOCV;
		uniform float _VTOFactorCustom;
		uniform float _VTOFactor;
		uniform float _VTOFactorC2;
		uniform half4 _VTOFactorC2Vec4;
		uniform float _VTOFactorC1;
		uniform float _VTORemap;
		uniform float _VTOAR;
		uniform half _VTOR;
	
		//half4 _FlowSampleStep;
		CBUFFER_END

		TEXTURE2D(_FlowMap);
		SAMPLER(sampler_FlowMap);

		TEXTURE2D(_BaseMap);
		SAMPLER(sampler_BaseMap);

		float4 _ColorMask_ST;
		TEXTURE2D(_ColorMask);
		SAMPLER(sampler_ColorMask);

		TEXTURE2D(_VTOTex);
		SAMPLER(sampler_VTOTex);

		half _MaskContrast;
		half _CullMode;
		half _BlendMode;
		half _Cutoff;

		ENDHLSL

		Pass 
		{
			Name "FlowMap"
			Tags { "LightMode" = "UniversalForward" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }
			LOD 100

            Blend [_SrcBlend] [_DstBlend]
            Cull [_CullMode]
			ZWrite[_ZWrite]
			ZTest[_ZTest]
			
			HLSLPROGRAM

			#pragma multi_compile_fog
			#pragma prefer_hlslcc gles
			#pragma target 3.0
			#pragma exclude_renderers d3d11_9x ps3 psp2 xbox360 gles n3ds wiiu
            #pragma shader_feature_local _ _ALPHATEST_ON
			#pragma shader_feature_local  _VTO_TEX

			#pragma vertex VertexFunc
			#pragma fragment FragmentFunc
			

			// Structs
			struct Attributes 
			{
				float4 vertex	: POSITION;
				float3 normal : NORMAL;
				float4 texcoord		: TEXCOORD0;
				float4 texcoord1    : TEXCOORD1;
				float4 texcoord2    : TEXCOORD2;
				float4 color		: COLOR;
			};

			struct Varyings 
			{
				float4 positionCS 	: SV_POSITION;
				float2 uv		: TEXCOORD0;
				float2 uvMask	: TEXCOORD1;
				float4 color	: COLOR;
			};


			#define RADIAN 0.0174533
			float2 ConvertUV(float2 uv, float4 textureST, 
						half useCustom, half uDataIndex, half4 uDataChannel, half vDataIndex, half4 vDataChannel,half4 data1, half4 data2,
						half uSpeed, half vSpeed,
						half uClamp, half vClamp,
						half rotate)
			{
				float2 convertedUV = uv * textureST.xy + textureST.zw;
				half4 uData = lerp(data1, data2, uDataIndex);
				half u = dot(uDataChannel, uData);
				half4 vData = lerp(data1, data2, vDataIndex);
				half v = dot(vDataChannel, vData);
				half2 offset = lerp(half2(0, 0), half2(u, v), useCustom);

				convertedUV += _Time.y * half2(uSpeed, vSpeed) + offset;

				half cosValue = cos(rotate * RADIAN);
				half sinValue = sin(rotate * RADIAN);
				convertedUV = mul(convertedUV- half2(0.5, 0.5) , half2x2(cosValue, -sinValue, sinValue, cosValue)) + half2(0.5, 0.5);

				float2 clampUV = clamp(convertedUV, 0, 1);

				convertedUV.x = lerp(convertedUV.x, clampUV.x, uClamp);
				convertedUV.y = lerp(convertedUV.y, clampUV.y, vClamp);

				return convertedUV;
			}


			Varyings VertexFunc(Attributes IN)
			{
				Varyings OUT;
				
				OUT.uv = TRANSFORM_TEX(IN.texcoord, _BaseMap);
				OUT.uvMask = TRANSFORM_TEX(IN.texcoord, _ColorMask);

				half4 customData1 = half4(IN.texcoord.zw, IN.texcoord1.xy);
				half4 customData2 = half4(IN.texcoord1.zw, IN.texcoord2.xy);

				/////////////////////////////////////////////////////////////////////
				///顶点偏移贴图部分
				float VtoOffset = 0;
				#ifdef _VTO_TEX
					float2 VtoUV = ConvertUV(IN.texcoord.xy, _VTOTex_ST, 0, 0, (half4)0, 0, (half4)0, (half4)0, (half4)0,
									_VTOTex_Uspeed, _VTOTex_Vspeed, _VTOC, _VTOCV, _VTOR);
				
					//采样顶点偏移贴图
					float4 VtoTex = SAMPLE_TEXTURE2D_LOD(_VTOTex, sampler_VTOTex, VtoUV,0);

					//是否使用自定义数据 _VTOFactorCustom
					float4 CustomData = lerp(customData1, customData2, _VTOFactorC1);
					float VTOFactorCustomData = dot(_VTOFactorC2Vec4, CustomData);
		
					//使用顶点偏移贴图的a通道还是r通道?
					VtoOffset = lerp(VtoTex.a,VtoTex.r,_VTOAR);
					//是否Remap
					VtoOffset = lerp(VtoOffset,VtoOffset - 0.5, _VTORemap);
					VtoOffset *= lerp(_VTOFactor,VTOFactorCustomData, _VTOFactorCustom);
				#endif
				//顶点在法线方向上作偏移
				OUT.positionCS = TransformObjectToHClip((IN.vertex + float4(VtoOffset * normalize(IN.normal),0)).xyz);
				OUT.color = IN.color;
				return OUT;
			}

			half4 FragmentFunc(Varyings IN) : SV_Target 
			{
				float2 mainUV = IN.uv;
				float2 maskUV = IN.uvMask;

				half2 flowDir = SAMPLE_TEXTURE2D(_FlowMap, sampler_FlowMap, mainUV).rg;
				//flowDir = 2 * flowDir - 1;
				flowDir = flowDir - 0.5;

				float flowSpeed = _Time.x * _FlowSpeed;
				float2 phase0 = lerp(0, flowDir, (frac(flowSpeed) - 0.5) * 2.0);
				float2 phase1 = lerp(0, flowDir, (frac(flowSpeed - 0.5) - 0.5) * 2.0);

				half4 tex0 = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, mainUV + phase0 * _FlowStrength);
				half4 tex1 = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, mainUV + phase1 * _FlowStrength);

				float maskFlowSpeed = _Time.x * _MaskFlowSpeed;
				float2 maskPhase0 = lerp(0, flowDir, (frac(maskFlowSpeed) - 0.5) * 2.0);
				float2 maskPhase1 = lerp(0, flowDir, (frac(maskFlowSpeed - 0.5) - 0.5) * 2.0);

				half4 mask0 = SAMPLE_TEXTURE2D(_ColorMask, sampler_ColorMask, maskUV + maskPhase0 * _MaskFlowStrength);
				half4 mask1 = SAMPLE_TEXTURE2D(_ColorMask, sampler_ColorMask, maskUV + maskPhase1 * _MaskFlowStrength);

				float flowBlendFactor = abs(2 * frac(flowSpeed) - 1);

				float maskFlowBlendFactor = abs(2 * frac(maskFlowSpeed) - 1);

				half4 maskBlend = lerp(mask0, mask1, maskFlowBlendFactor);
				half4 maskColor = lerp(_MaskColorStart, _MaskColorEnd, pow(maskBlend.r, _MaskContrast));

				half4 finalColor = lerp(tex0, tex1, flowBlendFactor);
				finalColor.rgb += maskColor.rgb;

				#ifdef _ALPHATEST_ON
                    clip(finalColor.a - _Cutoff);
                #endif

				return finalColor * _BaseColor * IN.color;
			}
			ENDHLSL
		}
	}
	CustomEditor "SGame_ShaderGUI_NastyFlowMap"
}
