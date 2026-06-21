
Shader "SGAME/VFX/Pandavfx_v2.3_URP"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.CullMode)]_Cullmode("Cullmode", Float) = 0		
		[Enum(UnityEngine.Rendering.CompareFunction)]_Ztest("Ztest", Float) = 4
		[Enum(UnityEngine.Rendering.BlendMode)]_Scr("Scr", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)]_Dst("Dst", Float) = 10
		[KeywordEnum(X,Y,Z)] _Face("Face", Float) = 1
		_Zwrite("Zwrite", Float) = 0
		[HDR]_BackFaceColor("BackFaceColor", Color) = (1,1,1,0)
		_AddTexBlendModeVec4("AddTexBlendMode", Vector) = (1, 0, 0, 0)
		[Toggle]_AlphaAdd("AlphaAdd", Float) = 0
		//_Reference("Reference", Range( 0 , 200)) = 0
		[Enum(UnityEngine.Rendering.StencilOp)]_Pass("Pass", Float) = 0
		//[Enum(UnityEngine.Rendering.CompareFunction)]_Comparison("Comparison", Float) = 8
		[Enum(UnityEngine.Rendering.StencilOp)]_Fail("Fail", Float) = 0
		
		_CenterU("CenterU", Float) = 0.5
		_CenterV("CenterV", Float) = 0.5

		// Main
		[HideInInspector]_MainTex_ST("MainTex_ST", Vector) = (1,1,0,0)
		_MainTex("MainTex", 2D) = "white" {}
		[Toggle]_UVPolar("UVPOLAR", Float) = 0
		_MainAlpha("MainAlpha", Range(0, 10)) = 1	
		[HDR]_MainColor("MainColor", Color) = (1,1,1,1)
		_MainTex_Uspeed("MainTex_Uspeed", Float) = 0
		_MainTex_Vspeed("MainTex_Vspeed", Float) = 0
		[Toggle]_MaintexC("MaintexC", Float) = 0
		[Toggle]_MaintexCV("MaintexCV", Float) = 0
		[Toggle]_MainTex_ar("MainTex_a/r", Float) = 0
		[Toggle]_CustomdataMainTexUV("CustomdataMainTexUV", Float) = 0
		[Enum(Custom1,0,Custom2,1)]_MainOffsetVC1("MainOffsetVC1", Float) = 0
		[Enum(Custom1,0,Custom2,1)]_MainOffsetUC1("MainOffsetUC1", Float) = 0
		_MainOffsetUC2Vec4("MainOffsetUC2Vec4", Vector) = (1, 0, 0, 0)
		_MainOffsetVC2Vec4("MainOffsetVC2Vec4", Vector) = (0, 1, 0, 0)
		_MainTex_rotat("MainRoate", Range(0, 360)) = 0
		_MainTexRefine("MainTexRefine", Vector) = (1,1,2,0)

		// Add
		_AddTex("AddTex", 2D) = "white" {}
		[HideInInspector]_AddTex_ST("AddTex_ST", Vector) = (1,1,0,0)
		_AddTexUspeed("AddTexUspeed", Float) = 0
		_AddTexVspeed("AddTexVspeed", Float) = 0
		[HDR]_AddTexColor("AddTexColor", Color) = (0,0,0,0)
		_AddTexBlend("AddTexBlend", Range( 0 , 1)) = 0
		[Toggle]_IfAddTex("IfAddTex", Float) = 0
		[Toggle]_AddTexC("AddTexC", Float) = 0
		[Toggle]_AddTexCV("AddTexCV", Float) = 0
		[Toggle]_AddTexAR("AddTexAR", Float) = 0
		[Toggle]_IfAddTexAlpha("IfAddTexAlpha", Float) = 0
		[Toggle]_IfAddTexColor("IfAddTexColor", Float) = 1
		[Toggle]_CAddTexUV("CAddTexUV", Float) = 0
		[Toggle]_CAddTexUVT("CAddTexUVT", Float) = 1
		_AddRotate("AddRoate", Range(0, 360)) = 0
		
		// Distort
		[HideInInspector]_DistortTex_ST("DistortTex_ST", Vector) = (1,1,0,0)
		[HideInInspector]_DistortMaskTex_ST("DistortMaskTex_ST", Vector) = (1,1,0,0)
		_DistortTex("DistortTex", 2D) = "white" {}
		[Toggle]_SceenTex("SceenTex", Float) = 0
		[Toggle]_DistortTexAR("DistortTexAR", Float) = 1
		[Toggle]_DistortMask("DistortMask", Float) = 0
		_DistortFactor("DistortFactor", Range( 0 , 1)) = 0
		_DistortTex_Uspeed("DistortTex_Uspeed", Float) = 0
		_DistortTex_Vspeed("DistortTex_Vspeed", Float) = 0
		[Enum(Option1,0,Option2,1)]_CustomDistort("CustomDistort", Float) = 0
		[Toggle]_DistortMaskTexAR("DistortMaskTexAR", Float) = 1
		_DistortMaskTex("DistortMaskTex", 2D) = "white" {}
		[Toggle]_DistortMaskTexC("DistortMaskTexC", Float) = 0
		[Toggle]_DistortMaskTexCV("DistortMaskTexCV", Float) = 0
		[Toggle]_IfFlowmap("IfFlowmap", Float) = 0
		[Enum(Custom1,0,Custom2,1)]_DistortFactorC1("DistortFactorC1", Float) = 1
		_DistortFactorC2Vec4("DistortFactorC2Vec4", Vector) = (0, 0, 1, 0)	
		[Toggle]_DistortRemap("DistortRemap", Float) = 0
		_DistortMaskTexR("DistRoate", Range(0, 360)) = 0
		[Toggle]_IfBeingDistorted("IfBeingDistorted",int) = 1
		
		// Dissolve
		_DissloveTex("DissloveTex", 2D) = "white" {}
		[HideInInspector]_DissloveTex_ST("DissloveTex_ST", Vector) = (1,1,0,0)
		_DIssloveFactor("DIssloveFactor", Range( 0 , 1)) = 0.5
		_DIssloveWide("DIssloveWide", Range( 0 , 1)) = 0.1
		_DIssloveSoft("DIssloveSoft", Range( 0 , 1)) = 0.5
		[HDR]_DIssloveColor("DIssloveColor", Color) = (1,1,1,1)
		_DisTex_Uspeed("DisTex_Uspeed", Float) = 0
		_DisTex_Vspeed("DisTex_Vspeed", Float) = 0
		[Enum(Option1,0,Option2,1)]_CustomdataDisT("CustomdataDisT", Float) = 0	
		_DissolveTexExp("DissolveTexExp", Range( 0 , 10)) = 1
		[Toggle]_DissolveC("DissolveC", Float) = 0
		[Toggle]_DissolveCV("DissolveCV", Float) = 0
		[Toggle]_DissolveAR("DissolveAR", Float) = 1
		[Toggle]_sot_sting_A("sot_sting_A", Float) = 0
		[Toggle]_CustomdataDis("CustomdataDis", Float) = 0
		[Toggle]_IfDissolveColor("IfDissolveColor", Float) = 1
		[Enum(Custom1,0,Custom2,1)]_DissolveFactorC1("DissolveFactorC1", Float) = 1
		[Enum(off,0,on,1)]_IfDissolveOffsetC("IfDissolveOffsetC", Float) = 0
		[Enum(Custom1,0,Custom2,1)]_DissolveOffsetUC1("DissolveOffsetUC1", Float) = 1
		_DissolveFactorC2Vec4("DissolveFactorC2Vec4", Vector) = (0, 0, 1, 0)	
		_DissolveOffsetUC2Vec4("DissolveOffsetUC2", Vector) = (1, 0, 0, 0)
		_DissolveOffsetVC2Vec4("DissolveOffsetVC2", Vector) = (0, 1, 0, 0)
		[Enum(Custom1,0,Custom2,1)]_DissolveOffsetVC1("DissolveOffsetVC1", Float) = 1
		_DisslovePlusTex("DisslovePlusTex", 2D) = "white" {}
		_DissolveTexDivide("DissolveTexDivide", Range(1 , 10)) = 1
		[HideInInspector]_DisslovePlusTex_ST("DisslovePlusTex_ST", Vector) = (1,1,0,0)
		_IfDissolvePlus("IfDissolvePlus", Float) = 0
		[Toggle]_DissolvePlusAR("DissolvePlusAR", Float) = 0
		[Toggle]_DissolvePlusC("DissolvePlusC", Float) = 0
		[Toggle]_DissolvePlusCV("DissolvePlusCV", Float) = 0
		_DIssolve_rotat("DissRoate", Range(0, 360)) = 0
		_DissolvePlusR("DissPlusRotate", Range(0, 360)) = 0
							
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
		
		// Fresnel
		_fnl_power("fnl_power", Range( 1 , 10)) = 1
		_fnl_sacle("fnl_sacle", Range( 0 , 1)) = 0
		[HDR]_fnl_color("fnl_color", Color) = (1,1,1,0)
		_softFacotr("softFacotr", Range( 0 , 20)) = 1
		_DepthfadeFactor("DepthfadeFactor", Range( 0 , 10)) = 1
		[Toggle]_FNLfanxiangkaiguan("FNLfanxiangkaiguan", Float) = 0
		_Dir("Dir", Vector) = (0,0,0,0)
		[Toggle]_IfFNLAlpha("IfFNLAlpha", Float) = 0

		// Mask
		[HideInInspector]_MaskPlusTex_ST("MaskPlusTex_ST", Vector) = (1,1,0,0)
		[HideInInspector]_MaskTex_ST("MaskTex_ST", Vector) = (1,1,0,0)
		_MaskTex("MaskTex", 2D) = "white" {}
		_MaskPlusTex("MaskPlusTex", 2D) = "white" {}
		[Toggle]_CustomdataMaskUV("CustomdataMaskUV", Float) = 0
		_Mask_scale("Mask_scale", Float) = 1
		_Mask_Uspeed("Mask_Uspeed", Float) = 0
		_MaskPlusUspeed("MaskPlusUspeed", Float) = 0
		_MaskPlusVspeed("MaskPlusVspeed", Float) = 0
		_Mask_Vspeed("Mask_Vspeed", Float) = 0
		[Toggle]_IfMaskColor("IfMaskColor", Float) = 0
		[Toggle]_MaskPlusC("MaskPlusC", Float) = 0
		[Toggle]_MaskPlusCV("MaskPlusCV", Float) = 0
		[Toggle]_MaskC("MaskC", Float) = 0
		[Toggle]_MaskCV("MaskCV", Float) = 0
		[Toggle]_MaskPlusAR("MaskPlusAR", Float) = 1
		[Toggle]_MaskAlphaRA("MaskAlphaRA", Float) = 1
		[Toggle]_IfMaskPlusTex("IfMaskPlusTex", Float) = 0
		[Enum(Custom1,0,Custom2,1)]_MaskOffsetVC1("MaskOffsetVC1", Float) = 0
		[Enum(Custom1,0,Custom2,1)]_MaskOffsetUC1("MaskOffsetUC1", Float) = 0
		_MaskOffsetUC2Vec4("_MaskOffsetUC2", Vector) = (0, 0, 1, 0)
		_MaskOffsetVC2Vec4("_MaskOffsetVC2", Vector) = (0, 0, 0, 1)
		_Mask_rotat("MaskRotate", Range(0, 360)) = 0
		_MaskPlusR("MaskPlusRotate", Range(0, 360)) = 0

		// Soft particle
		[Toggle]_Depthfadeon("Depthfadeon", Float) = 0

		[HideInInspector] __dirty( "", Int ) = 1
		[HideInInspector] _TextureUScale("",float) = 1
		[HideInInspector] _TextureUValue("",float) = 0
		[HideInInspector] _TextureVScale("",float) = 1
		[HideInInspector] _TextureVValue("",float) = 0
		[HideInInspector] _CustomDataToZero("",float) = 1
		
		_StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		
		_DepthOffset("深度偏移",Range(-0.5,0.5)) = 0
		//_UVTest("UVTest",range(0,1)) = 0
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IsEmissive" = "true"  "Queue" = "Transparent+500"}
		Cull [_Cullmode]
		ZWrite [_Zwrite]
		ZTest [_Ztest]
//		Stencil
//		{
//			Ref [_Reference]
//			Comp [_Comparison]
//			Pass [_Pass]
//			Fail [_Fail]
//		}
		 Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
	    ColorMask [_ColorMask]
		Blend [_Scr] [_Dst]
		HLSLINCLUDE

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
		#include "Pandavfx_Function.hlsl"

		
		
		#define lerp(x, y, s) (mad((y)-(x), (s), (x)))
		#define POLAR_COEFFICIENT 0.15915

		//为了适应大量贴图的需要,需要根据贴图作变体来减少采样的次数
		//#pragma shader_feature_local _ _MAINTEX_CUSTOMDATA    //mainTex是否有customData

		//使得shader代码可调试,上线时注释掉
		#pragma enable_d3d11_debug_symbols
		
		#pragma shader_feature_local  _VTO_TEX
		
		#pragma shader_feature_local _DISORT_TEX
		#pragma shader_feature_local _DISORT_MASK_TEX
		
		#pragma shader_feature_local _MASK_TEX
		#pragma shader_feature_local _MASK_PLUS_TEX
		
		#pragma shader_feature_local _ADD_TEX
		
		#pragma shader_feature_local _DISSOLVE_TEX
		#pragma shader_feature_local _DISSOLVE_PLUS_TEX
		
		//#pragma shader_feature_local _ _SOFT_PARTICAL       //是否是软粒子
		#pragma shader_feature_local _ _SCEENTEX_ON
		
		half4 Func(Input i, half facing)
		{
			//float test = step(i.uv_texcoord.y,_UVTest);
			
			//return float4(i.uv2_texcoord2.www,1);
			half4 customData1 = half4(i.uv_texcoord.zw, i.uv2_texcoord2.xy);
			half4 customData2 = half4(i.uv2_texcoord2.zw, i.uv3_texcoord3.xy);

			//如果使用LineRenderer而不是粒子系统的话 在此将customData归零
			customData1.xyzw*=_CustomDataToZero;
			customData2.xyzw*=_CustomDataToZero;

			///DistortTexture
			float2 distortUV = 0;
			#ifdef _DISORT_TEX
				float2 distortTexcoord = i.uv_texcoord.xy;
				float2 convertedUV = ConvertUV(distortTexcoord, _DistortTex_ST, 0, 0, (half4)0, 0, (half4)0, (half4)0, (half4)0,
												_DistortTex_Uspeed, _DistortTex_Vspeed, 0, 0, 0);
				
				float4 customDataDistChannel = lerp(customData1, customData2, _DistortFactorC1);
				float distFactorCustom = dot(_DistortFactorC2Vec4, customDataDistChannel);
				distFactorCustom = lerp(_DistortFactor, distFactorCustom, _CustomDistort);
				
				distortUV = GetDistortTextureData(convertedUV, _DistortTex, sampler_DistortTex, distFactorCustom, _DistortTexAR, _DistortRemap, _IfFlowmap);

				#ifdef _DISORT_MASK_TEX
					float2 distortMaskTexcood = i.uv_texcoord.xy;	
					float2 distortMask = ConvertUV(i.uv_texcoord.xy, _DistortMaskTex_ST, 0, 0, (half4)0, 0, (half4)0, (half4)0, (half4)0,
											0, 0, _DistortMaskTexC, _DistortMaskTexCV, _DistortMaskTexR);

					half4 mask = SAMPLE_TEXTURE2D(_DistortMaskTex, sampler_DistortMaskTex, distortMask);
					distortUV *= lerp(mask.a, mask.r, _DistortMaskTexAR);
				#endif
			#endif

			///主贴图uv
			float2 mainTexcoord = i.uv_texcoord.xy;
			float2 mainUV = ConvertUV(mainTexcoord, _MainTex_ST,
								_CustomdataMainTexUV, _MainOffsetUC1, _MainOffsetUC2Vec4, _MainOffsetVC1, _MainOffsetVC2Vec4, customData1, customData2,
								_MainTex_Uspeed, _MainTex_Vspeed, _MaintexC, _MaintexCV, _MainTex_rotat);

			//采样主贴图
			//根据是采样材质贴图还是采样背景图做一个变体
			half4 mainColor = float4(0,0,0,1);
			#ifdef _SCEENTEX_ON
			//UnityStereoTransformScreenSpaceTex(float2 uv)
			float4 screenPos = i.screenPos;
			//mainColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture,sampler_CameraOpaqueTexture,i.screenPos.xy/i.screenPos.w+ distortUV);
			mainColor = SAMPLE_TEXTURE2D(_CameraOpaqueAndTransparentTexture,sampler_CameraOpaqueAndTransparentTexture,i.screenPos.xy/i.screenPos.w+ distortUV);
			#else
			mainColor = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,mainUV + distortUV);
			#endif
			//return float4(mainColor.a,mainColor.a,mainColor.a,1);
			i.ASEVFace = 0;
			//是否使用R通道来替代透明度
			mainColor.a = clamp(lerp(mainColor.a,mainColor.a * mainColor.r,_MainTex_ar),0,1);
			//混合前景色和后景色
			mainColor.rgb = mainColor.rgb * _MainColor.rgb;
			//mainColor.rgb = facing<0?mainColor.rgb*_BackFaceColor.rgb:mainColor.rgb;
			facing = step(facing, 0);
			mainColor.rgb = lerp(mainColor.rgb, mainColor.rgb*_BackFaceColor.rgb, facing);
			
			//mainColor与顶点色混合
			//mainColor.rgb *= ( _IfDissolveColor == 0.0 ? float3(1,1,1) : i.vertexColor.rgb );
			//mainColor.rgb *= lerp(i.vertexColor.rgb, float3(1,1,1), _IfDissolveColor);
			mainColor.rgb *= i.vertexColor.rgb;
			//return float4(i.vertexColor.rgb,1);

			//mainColor Refine
			_MainTexRefine.z = max(_MainTexRefine.z,0.01);
			mainColor.rgb = lerp((mainColor.rgb * _MainTexRefine.x), pow( mainColor.rgb , _MainTexRefine.zzz ) * _MainTexRefine.y, _MainTexRefine.w);
			////////////////////////////////////////////////////////////////////////////////
			///mainColor与其他贴图开始混合
			///mainColor与AddTex混合
			float3 finalColor = mainColor.rgb;
			float finalColorAlpha = mainColor.a;
			
			//原代码中mainTexAlpha会受_CustomdataDisT(溶解贴图的拖尾模式)影响,不知为何,只是照抄
			finalColorAlpha = finalColorAlpha * (i.vertexColor.a * (1 - _CustomdataDisT) + _CustomdataDisT);

			///多功能图(addTex)采样部分
			float2 addTexUV = float2(0,0);
			//float4 addColor = float4(1,1,1,1);
			#ifdef _ADD_TEX
				float2 addTexcoord = i.uv_texcoord.xy;
				float2 addUV = ConvertUV(addTexcoord, _AddTex_ST, 0, 0, (half4)0, 0, (half4)0, (half4)0, (half4)0,
								_AddTexUspeed, _AddTexVspeed, _AddTexC, _AddTexCV, _AddRotate);

				half4 addColor =  AddTexture(addUV, _AddTex, sampler_AddTex,
											finalColor, finalColorAlpha,
											_AddTexColor, _AddTexAR, _AddTexBlend, _AddTexBlendModeVec4, _IfAddTexColor, _IfAddTexAlpha);
				finalColor.rgb = addColor.rgb;
				finalColorAlpha = addColor.a;
			#endif
			////////////////////////////////////////////////////////////////////////////////

			
			////////////////////////////////////////////////////////////////////////////////
			///菲涅尔部分内容
			float3 ViewDir =normalize(_WorldSpaceCameraPos - i.worldPos) + _Dir;
			float NoV = abs(dot(normalize(i.worldNormal),ViewDir));
			float FNL = saturate( ( pow( ( 1.0 - saturate( abs( NoV ) ) ) , _fnl_power ) * _fnl_sacle ) );
			float4 fnlColor = FNL * _fnl_color;
			float softedge = pow(NoV,_softFacotr);
			//是否影响最终的Alpha值
			finalColorAlpha = finalColorAlpha * lerp(1,FNL,_IfFNLAlpha) * lerp(1,softedge,_FNLfanxiangkaiguan);
			finalColor +=fnlColor.rgb;

			
			////////////////////////////////////////////////////////////////////////////////

			////////////////////////////////////////////////////////////////////////////////
			///软粒子  需要开启深度图
			#ifdef _SOFT_PARTICAL
				float4 screenPosNorm =  i.screenPos/i.screenPos.w;
				float screenDepth88 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture,sampler_CameraDepthTexture, screenPosNorm.xy),_ZBufferParams);
				float distanceDepth = saturate( ( screenDepth88 - LinearEyeDepth( screenPosNorm.z ,_ZBufferParams) ) / ( _DepthfadeFactor ) );
				float softDepth = lerp(1.0, distanceDepth, _Depthfadeon);
				finalColorAlpha *=softDepth;
			#endif
			////////////////////////////////////////////////////////////////////////////////
			////////////////////////////////////////////////////////////////////////////////
			///与遮罩贴图混合
			//MaskTexture
			float4 maskTexColor = float4(1,1,1,1);
			float maskPlus = 1;
			//float3 maskColor = 1;
			float3 tempmaskcolor = 1;
			#ifdef _MASK_TEX
				float2 maskTexcoord = i.uv_texcoord.xy;
				float2 maskUV = ConvertUV(maskTexcoord, _MaskTex_ST, 
										_CustomdataMaskUV, _MaskOffsetUC1, _MaskOffsetUC2Vec4, _MaskOffsetVC1, _MaskOffsetVC2Vec4, customData1, customData2,
										_Mask_Uspeed, _Mask_Vspeed, _MaskC, _MaskCV, _Mask_rotat);

				half4 maskColor = GetMaskTextureData(maskUV, _MaskTex, sampler_MaskTex,
										finalColor, finalColorAlpha, _MaskAlphaRA, _IfMaskColor, _Mask_scale);

				finalColor = maskColor.rgb;
				finalColorAlpha = maskColor.a;
			#endif

			//启动额外遮罩贴图
			#ifdef _MASK_PLUS_TEX
				float2 maskPlusTexcoord = i.uv_texcoord.xy;
				float2 maskPlusUV = ConvertUV(maskPlusTexcoord, _MaskPlusTex_ST, 0, 0, (half4)0, 0, (half4)0, (half4)0, (half4)0,
											_MaskPlusUspeed, _MaskPlusVspeed, _MaskPlusC, _MaskPlusCV, _MaskPlusR);

				half4 maskPlusTexColor = GetMaskTextureData(maskPlusUV, _MaskPlusTex, sampler_MaskPlusTex,
						finalColor, finalColorAlpha, _MaskPlusAR, 0, 1);

				finalColorAlpha = maskPlusTexColor.a;
			#endif

			///与溶解贴图混合
			#ifdef _DISSOLVE_TEX
				float2 dissolveTexcoord = i.uv_texcoord.xy;
				half2 dissolveUV = ConvertUV(dissolveTexcoord, _DissloveTex_ST,
										_IfDissolveOffsetC, _DissolveOffsetUC1, _DissolveOffsetUC2Vec4, _DissolveOffsetVC1, _DissolveOffsetVC2Vec4, customData1, customData2,
										_DisTex_Uspeed, _DisTex_Vspeed, _DissolveC, _DissolveCV, _DIssolve_rotat);
				float4 dissolveTexColor = SAMPLE_TEXTURE2D(_DissloveTex,sampler_DissloveTex,dissolveUV);
				float dissolve = lerp(dissolveTexColor.a, dissolveTexColor.r, _DissolveAR);

				float4 customDataDissChannel = lerp(customData1, customData2, _DissolveFactorC1);
				float dissFactorCustom = dot(_DissolveFactorC2Vec4, customDataDissChannel);
				//float dissFactor = ( _CustomdataDis == 0.0 ? ( _DIssloveFactor + 0.001 ) : ( _CustomdataDisT == 0.0 ? dissFactorCustom : (1.0 + (i.vertexColor.a - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) ) );
				dissFactorCustom = lerp(dissFactorCustom, (1.0 - i.vertexColor.a), _CustomdataDisT);
				float dissFactor = lerp(_DIssloveFactor, dissFactorCustom, _CustomdataDis);
			//return float4(_CustomDataToZero,_CustomDataToZero,_CustomDataToZero,1);

				//附加溶解贴图
				#ifdef _DISSOLVE_PLUS_TEX
					float2 dissolveAddTexUV = i.uv_texcoord.xy * _DisslovePlusTex_ST.xy + _DisslovePlusTex_ST.zw;
					half cosValue = cos(_DissolvePlusR * RADIAN);
					half sinValue = sin(_DissolvePlusR * RADIAN);
					dissolveAddTexUV = mul(dissolveAddTexUV- half2(0.5, 0.5) , half2x2(cosValue, -sinValue, sinValue, cosValue)) + half2(0.5, 0.5);


					//附加溶解贴图Clamp
					float2 dissolvePlusUV = dissolveAddTexUV;
					float2 dissolvePlusUVClamp = clamp(dissolveAddTexUV,0,1);
					dissolvePlusUV.x = lerp(dissolvePlusUV.x, dissolvePlusUVClamp.x, _DissolvePlusC);
					dissolvePlusUV.y = lerp(dissolvePlusUV.y, dissolvePlusUVClamp.y, _DissolvePlusCV);
					//采样附加溶解贴图
					float4 dissolveAddTexColor = SAMPLE_TEXTURE2D(_DisslovePlusTex,sampler_DisslovePlusTex,dissolvePlusUV);
					//是否使用R通道
					float dissolveAdd = lerp(dissolveAddTexColor.a,dissolveAddTexColor.r,_DissolvePlusAR);
					//混合附加溶解贴图和溶解贴图的值
					
					//根据溶解参数做变化.
					float powDissolve = saturate(pow(abs(dissolve), _DissolveTexExp) + dissolveAdd/2);
					powDissolve = saturate(pow(dissolve,_DissolveTexExp)/_DissolveTexDivide + dissolveAdd/2);
				#else
					float powDissolve = saturate(pow(abs(dissolve), _DissolveTexExp) + dissolve/2);
				#endif

				//float temp_output_57_0_g571 = saturate( pow( ( (float)_DissolveAR == 0.0 ? dissolveTexColor.a : dissolveTexColor.r ) , _DissolveTexExp ) );
				//float powDissolve = saturate((((temp_output_57_0_g571 / _DissolveTexDivide) + ( _IfDissolvePlus == 0.0 ? temp_output_57_0_g571 : lerp(dissolveAddTexColor.a, dissolveAddTexColor.r, _DissolvePlusAR))) * 0.5));
				//float powDissolve = saturate(pow(abs(dissolve), _DissolveTexExp));//saturate(((temp_output_57_0_g571 + temp_output_57_0_g571) * 0.5));
				float temp_output_3_0_g571 = (dissFactor * (1.0 + _DIssloveWide));
				float temp_output_1783_43 = step((temp_output_3_0_g571 - _DIssloveWide) , powDissolve);
				float temp_output_2_0_g571 = (_DIssloveSoft + 1.0) * dissFactor;
				float smoothstepResult21_g571 = smoothstep((temp_output_2_0_g571 - _DIssloveSoft) , temp_output_2_0_g571 , powDissolve);
				float dissolveAlpha = lerp(smoothstepResult21_g571, temp_output_1783_43, _sot_sting_A);

				finalColorAlpha *=dissolveAlpha;
			
			#endif
			///////////////////////////////////////////////////////////////////////////////

			//调整总透明度
			finalColorAlpha *= _MainAlpha;

			//判断混合模式 如果是AlphaAdd则要乘以alpha
			finalColor.rgb = lerp(1.0, finalColorAlpha, _AlphaAdd) * finalColor.rgb;
			finalColor.rgb = clamp(finalColor.rgb,float3(0.0001,0.0001,0.0001),float3(20,20,20));
			finalColorAlpha = clamp(finalColorAlpha,0.00,1);
			//return float4(finalColorAlpha,finalColorAlpha,finalColorAlpha,1);
			return float4(finalColor.rgb,finalColorAlpha);
		}

		Input vertexFunc( inout appdata_full v )
		{
			Input o;
			//NITY_INITIALIZE_OUTPUT( Input, o );
			o = (Input)0;

			half4 customData1 = half4(v.texcoord.zw, v.texcoord1.xy);
			half4 customData2 = half4(v.texcoord1.zw, v.texcoord2.xy);

			/////////////////////////////////////////////////////////////////////
			///顶点偏移贴图部分
			float VtoOffset = 0;
			#ifdef _VTO_TEX
				float2 VtoUV = ConvertUV(v.texcoord.xy, _VTOTex_ST, 0, 0, (half4)0, 0, (half4)0, (half4)0, (half4)0,
								_VTOTex_Uspeed, _VTOTex_Vspeed, _VTOC, _VTOCV, _VTOR);
				
				//采样顶点偏移贴图
				float4 VtoTex = SAMPLE_TEXTURE2D_LOD(_VTOTex,sampler_VTOTex,VtoUV,0);

				//是否使用自定义数据 _VTOFactorCustom
				float4 CustomData = lerp(customData1, customData2, _VTOFactorC1);
				float VTOFactorCustomData = dot(_VTOFactorC2Vec4, CustomData);
		
				//使用顶点偏移贴图的a通道还是r通道?
				VtoOffset = lerp(VtoTex.a,VtoTex.r,_VTOAR);
				//是否Remap
				VtoOffset = lerp(VtoOffset,VtoOffset - 0.5,_VTORemap);
				VtoOffset *=lerp(_VTOFactor,VTOFactorCustomData,_VTOFactorCustom);
			#endif
			/////////////////////////////////////////////////////////////////////

			//顶点在法线方向上作偏移
			v.vertex = v.vertex + float4(VtoOffset * normalize(v.normal),0);
		
			return o;
		}

		Varyings vert( appdata_full v )
		{
			Varyings o;
			UNITY_SETUP_INSTANCE_ID( v );
			o = (Varyings)0;
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
			UNITY_TRANSFER_INSTANCE_ID( v, o );
			Input customInputData;
			customInputData = vertexFunc(v);
			o.posWS = mul(unity_ObjectToWorld, v.vertex).xyz;
			o.normal = TransformObjectToWorldNormal(v.normal);
			o.customPack1.xyzw = v.texcoord;
			//外界调整uv缩放值的时候使用, 默认缩放值为1
			o.customPack1.x = o.customPack1.x * _TextureUScale + _TextureUValue;
			o.customPack1.y = o.customPack1.y * _TextureVScale + _TextureVValue;
			
			o.customPack2.xyzw = v.texcoord1;
			o.customPack3.xyzw = v.texcoord2;
			o.pos = TransformObjectToHClip(v.vertex.xyz);
			o.screenPos = ComputeScreenPos( o.pos );
			o.color = v.color;
			//o.screenPos.z = -TransformWorldToView(TransformObjectToWorld(v.vertex.xyz)).z;
			return o;
		}
		half4 Frag(Varyings IN/*,half facing : VFACE*//*, out float outDepth : SV_Depth*/): SV_Target
		{
			UNITY_SETUP_INSTANCE_ID( IN );
			Input surfIN;
			surfIN = (Input)0;
			surfIN.uv_texcoord = IN.customPack1.xyzw;
			surfIN.uv2_texcoord2 = IN.customPack2.xyzw;
			surfIN.uv3_texcoord3 = IN.customPack3.xyzw;
			surfIN.worldPos = IN.posWS;
			surfIN.worldNormal = IN.normal;
			surfIN.screenPos = IN.screenPos;
			surfIN.vertexColor = IN.color;
			surfIN.positionCS = IN.pos; 

			//half4 color = Func(surfIN, facing);
			half4 color = Func(surfIN, 1);
			//outDepth = IN.pos.z/IN.pos.w - _DepthOffset;
			//float eyeDepth= IN.screenPos.z;
			//outDepth =  (1 - _ZBufferParams.w * eyeDepth) / (_ZBufferParams.z * eyeDepth)- _DepthOffset;
			//color = half4(IN.pos.w,IN.pos.w,IN.pos.w,1);
			return color;
		}
		ENDHLSL

		Pass
        {
        	Name "UniversalForward"
            Tags{"LightMode" = "UniversalForward" }
        	Blend [_Scr] [_Dst]
        	//ZTest On
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment Frag
			
			ENDHLSL
        }

//		Pass
//        {
//        	Name "Distortion"
//			Tags {"LightMode" = "Distortion"  }
//        	Blend [_Scr] [_Dst]
//        	//ZTest On
//			HLSLPROGRAM
//			#pragma vertex vert
//			#pragma fragment Frag
//			
//			ENDHLSL
//        }
//		Pass
//        {
//        	Name "AfterDistortion"
//			Tags {"LightMode" = "AfterDistortion"  }
//        	Blend [_Scr] [_Dst]
//        	//ZTest On
//			HLSLPROGRAM
//			#pragma vertex vert
//			#pragma fragment Frag
//			
//			ENDHLSL
//        }

	}
	Fallback "Hidden/InternalErrorShader"
	CustomEditor "Yoka.MGame.PandaShader.PandavfxGUI"
}
