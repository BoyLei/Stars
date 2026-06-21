#ifndef PANDAVFX_FUNCTION
#define PANDAVFX_FUNCTION


struct appdata_full {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 texcoord : TEXCOORD0;
    float4 texcoord1 : TEXCOORD1;
    float4 texcoord2 : TEXCOORD2;
    half4 color : COLOR;
	uint instanceID : SV_InstanceID;
};

struct Input
{
	float4 uv_texcoord : TEXCOORD0;
	float3 worldPos;	
	float4 positionCS;
	float4 uv2_texcoord2;
	float4 uv3_texcoord3;
	float4 vertexColor : COLOR;
	float3 worldNormal;
	float4 screenPos;
	half ASEVFace : VFACE;
};
struct  Varyings
{
	//V2F_SHADOW_CASTER;
	float4 pos : SV_POSITION;
	float4 customPack1 : TEXCOORD0;
	float4 customPack2 : TEXCOORD1;
	float4 customPack3 : TEXCOORD2;
	float4 screenPos : TEXCOORD3;
	float3 normal : TEXCOORD4;
	float3 posWS : TEXCOORD5;
	half4 color : COLOR0;

	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

CBUFFER_START(UnityPerMaterial)
	uniform float _Scr;
	uniform float _Comparison;
	uniform float _Fail;
	uniform float _Ztest;
	uniform float _Dst;
	uniform float _Pass;
	uniform float _Zwrite;
	uniform float _Reference;
	uniform float _Cullmode;
	uniform float _AlphaAdd;
	uniform float4 _BackFaceColor;

	TEXTURE2D(_VTOTex);
	SAMPLER(sampler_VTOTex);
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

	TEXTURE2D(_MaskTex);
	SAMPLER(sampler_MaskTex);
	uniform float _MaskC;
	uniform float _Mask_Uspeed;
	uniform float _Mask_Vspeed;
	uniform float4 _MaskTex_ST;
	uniform float _DistortMask;
	uniform float _IfFlowmap;
	uniform float _Mask_scale;
	uniform float _MaskAlphaRA;
	uniform float _CustomdataMaskUV;
	uniform half4 _MaskOffsetUC2Vec4;
	uniform float _MaskOffsetUC1;
	uniform half4 _MaskOffsetVC2Vec4;
	uniform float _MaskOffsetVC1;
	uniform float _MaskCV;
	uniform float _IfMaskPlusTex;
	uniform float _MaskPlusAR;
	uniform half _Mask_rotat;
	uniform half _MaskPlusR;

	TEXTURE2D(_DistortTex);
	SAMPLER(sampler_DistortTex);
	uniform float _DistortTex_Uspeed;
	uniform float _DistortTex_Vspeed;
	uniform float4 _DistortTex_ST;
	uniform float _DistortTexAR;
	uniform float _CustomDistort;
	uniform float _DistortFactor;
	uniform half4 _DistortFactorC2Vec4;
	uniform float _DistortFactorC1;
	uniform float _DistortRemap;
	uniform float _IfBeingDistorted;
	
	//uniform sampler2D _DistortMaskTex;
	TEXTURE2D(_DistortMaskTex);
	SAMPLER(sampler_DistortMaskTex);
	uniform float _DistortMaskTexC;
	uniform float4 _DistortMaskTex_ST;
	uniform float _DistortMaskTexCV;
	uniform float _DistortMaskTexAR;
	uniform half _DistortMaskTexR;

	TEXTURE2D(_MaskPlusTex);
	SAMPLER(sampler_MaskPlusTex);
	uniform float _MaskPlusC;
	uniform float _MaskPlusUspeed;
	uniform float _MaskPlusVspeed;
	uniform float4 _MaskPlusTex_ST;
	uniform float _MaskPlusCV;
	uniform float _IfMaskColor;
	
	TEXTURE2D(_MainTex);
	uniform float _MainTex_ar;
	SAMPLER(sampler_MainTex);
	uniform float _MainUV2;
	uniform float _MaintexC;
	uniform float _MainTex_Uspeed;
	uniform float _MainTex_Vspeed;
	uniform float _CustomdataMainTexUV;
	uniform half4 _MainOffsetUC2Vec4;
	uniform float _MainOffsetUC1;
	uniform half4 _MainOffsetVC2Vec4;
	uniform float _MainOffsetVC1;
	uniform float4 _MainTex_ST;	
	uniform float _MaintexCV;
	uniform float4 _MainColor;
	uniform float _MainAlpha;
	uniform half _MainTex_rotat;
	uniform float4 _MainTexRefine;

	TEXTURE2D(_CameraOpaqueTexture);
	SAMPLER(sampler_CameraOpaqueTexture);
	uniform float4 _CameraOpaqueTexture_ST;

	TEXTURE2D(_CameraOpaqueAndTransparentTexture);
	SAMPLER(sampler_CameraOpaqueAndTransparentTexture);
	uniform float4 _CameraOpaqueAndTransparentTexture_ST;
	
	TEXTURE2D(_AddTex);
	SAMPLER(sampler_AddTex);
	uniform float _AddTexC;
	uniform float _AddTexUspeed;
	uniform float _AddTexVspeed;
	uniform float4 _AddTex_ST;
	uniform float _CAddTexUV;
	uniform float _CAddTexUVT;
	uniform float _IfAddTexAlpha;
	uniform half4 _AddTexBlendModeVec4;
	uniform float _AddTexAR;
	uniform float _AddTexCV;
	uniform float _AddTexBlend;
	uniform float _IfAddTex;
	uniform float _IfAddTexColor;
	uniform float4 _AddTexColor;
	uniform half _AddRotate;
	
	TEXTURE2D(_DissloveTex);
	SAMPLER(sampler_DissloveTex);
	uniform float _DissolveC;
	uniform float _DisTex_Uspeed;
	uniform float _DisTex_Vspeed;
	uniform float4 _DissloveTex_ST;
	uniform float _sot_sting_A;
	uniform float _DIssloveSoft;
	uniform float _CustomdataDis;
	uniform float _IfDissolveColor;
	uniform float _DIssloveFactor;
	uniform half4 _DissolveFactorC2Vec4;
	uniform float _DissolveFactorC1;
	uniform float _DissolveAR;
	uniform float _IfDissolveOffsetC;
	uniform half4 _DissolveOffsetUC2Vec4;
	uniform float _DissolveOffsetUC1;
	uniform half4 _DissolveOffsetVC2Vec4;
	uniform float _DissolveOffsetVC1;
	uniform float _DissolveCV;
	uniform float _DissolveTexExp;
	uniform float _DIssloveWide;
	uniform float _CustomdataDisT;
	TEXTURE2D(_DisslovePlusTex);
	SAMPLER(sampler_DisslovePlusTex);
	uniform float4 _DisslovePlusTex_ST;
	uniform float _IfDissolvePlus;
	uniform float _DissolvePlusAR;
	uniform float _DissolvePlusC;
	uniform float _DissolvePlusCV;
	uniform half _DissolveTexDivide;
	uniform half _DIssolve_rotat;
	uniform half _DissolvePlusR;

	TEXTURE2D(_CameraDepthTexture);
	SAMPLER(sampler_CameraDepthTexture);
	uniform float4 _CameraDepthTexture_TexelSize;
	uniform float _DepthfadeFactor;
	uniform float _softFacotr;
	uniform float _Depthfadeon;

	uniform float _FNLfanxiangkaiguan;
	uniform float _IfFNLAlpha;
	uniform float3 _Dir;
	uniform float _fnl_power;
	uniform float _fnl_sacle;
	uniform float4 _fnl_color;
	
	uniform float4 _DIssloveColor;

	uniform float _CenterU;
	uniform float _CenterV;
	uniform float _UVPolar;

	//粒子系统材质球适配Linerenderer时使用
	uniform float _TextureUScale;
	uniform float _TextureUValue;
	uniform float _TextureVScale;
	uniform float _TextureVValue;

	uniform float _CustomDataToZero;

	uniform float _DepthOffset;

	uniform float4 _ClipRect;
	uniform float _ToGray;
	//uniform float _UVTest;
CBUFFER_END

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


/**
 * UV扭曲贴图部分
 * \param uv 
 * \param distortUV 
 */
float2 GetDistortTextureData(float2 uv, Texture2D tex, SamplerState samplerState, half distortFactor, half useRChannel = 0, half remap = 0, half isFlow = 0)
{
	float2 distortUV = float2(0,0);
	float4 distortTex =float4(0,0,0,0);
	float distort = 0;

	//采样扭曲贴图
	distortTex = SAMPLE_TEXTURE2D(tex, samplerState, uv);
	//是否使用a通道
	distort = lerp(distortTex.a, distortTex.r, useRChannel);
	//是否remap
	distort = lerp(distort,distort*0.5-1,remap);
	float4 distortTexRemap = lerp(distortTex,distortTex*0.5-1,remap);
	//扭曲强度  区分是customdata 还是输入参数
	float distortFract = distortFactor;
		
	//FlowMap化
	float3 disortFlowMap = float3(distortFract*(float2(distortTexRemap.r,distortTexRemap.g)-uv.xy),0);
	distortUV = (distort * distortFract).xx;

	distortUV = lerp(distortUV, disortFlowMap.xy, isFlow);

	return distortUV;
}

/**
 * MaskTexture
 */
half4 GetMaskTextureData(float2 uv, Texture2D tex, SamplerState samplerState, float3 scrColor, float scrAlpha, half useRChannel = 0, half isMaskColor = 0, half maskScale = 1)
{
	//采样遮罩贴图
	half4 maskTexColor = SAMPLE_TEXTURE2D(tex, samplerState,uv);
	//maskTexColor *= _AddTexColor.rgba;

	//遮罩贴图是否使用R通道作为遮罩源头
	half mask = lerp(maskTexColor.a, maskTexColor.r, useRChannel);
	mask *= maskScale;

	scrColor = lerp(scrColor, scrColor * maskTexColor.rgb, isMaskColor);
	scrAlpha =  scrAlpha * mask;

	return half4(scrColor, scrAlpha);
}

half4 AddTexture(float2 uv, Texture2D tex, SamplerState samplerState, 
		half3 srcColor, half srcAlpha,
		half4 addTexColor, half useRchannel, half blendFactor,
		half4 blendMode, half blendColor, half blendAlpha)
{
	half4 addColor = SAMPLE_TEXTURE2D(tex, samplerState, uv);
	addColor *= addTexColor.rgba;

	float3 reFineResultAddTex = addColor.rgb;

	float addColorAlpha = lerp(addColor.a, addColor.r, useRchannel);

	float4 blendColAddTex = lerp(float4(srcColor, srcAlpha), float4(reFineResultAddTex.rgb, addColorAlpha), blendFactor);
	float4 multiplyColAddTex =  float4(srcColor, srcAlpha) * float4(reFineResultAddTex.rgb, addColorAlpha);
	float4 addColAddTex = float4(srcColor, srcAlpha) + float4(reFineResultAddTex.rgb, addColorAlpha);

	float4x4 blendMatrix;
	blendMatrix._11_21_31_41 = blendColAddTex;
	blendMatrix._12_22_32_42 = addColAddTex;
	blendMatrix._13_23_33_43 = multiplyColAddTex;
	blendMatrix._14_24_34_44 = float4(srcColor, srcAlpha);

	float4 finalColorTemp = float4(0,0,0,0);
	//根据addTex的blendMode决定使用哪个数据				
	finalColorTemp = mul(blendMatrix, blendMode);

	srcColor = lerp(srcColor, finalColorTemp.rgb, blendColor);
	srcAlpha = lerp(srcAlpha, finalColorTemp.a, blendAlpha);

	return half4(srcColor, srcAlpha);
}

half3 GetLitColor(half3 normal, half3 color, float3 worldPos)
{
	// 主光，用半lambert计算
	half3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * color;
	half3 worldNomal = normal;
	half3 worldLightDir = normalize(_MainLightPosition.xyz);
	half3 lambert = 0.5 * dot(worldNomal, worldLightDir) + 0.5;
	half3 diffuse = lambert * color * _MainLightColor.xyz + ambient;

	// 增加其他光源的影响
	uint lightsCount = GetAdditionalLightsCount();
	for (uint lightIndex = 0u; lightIndex < lightsCount; ++lightIndex)
	{
		Light light = GetAdditionalLight(lightIndex, worldPos);
		diffuse += light.color * light.distanceAttenuation * light.shadowAttenuation;
	}

	return diffuse;
}
	
#endif
