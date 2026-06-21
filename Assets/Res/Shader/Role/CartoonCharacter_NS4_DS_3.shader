Shader "SGAME/Roles/DS/CartoonChar_4_DS"
{
    Properties
    {
        [HideInInspector] _Flag("区分渲染位置的参数",int) = 0
//   
        [Space(20)]

//        [Header(GlobalLight)]
//        _GlobalLightParm("受环境光影响的因子(0受环境光影响最少,1受环境光影响最大)",Range(0.5,2)) = 0.5
        
//        _smoothness("控制全局光照的边缘参数",Range(0,5)) = 0.1
        [Main(GlobalSetting)]_group0 ("全局设置", float) = 0
            [Sub(GlobalSetting)]_DirectionLightControler("主光源强度,1为使用主光源本身的强度",Range(0,2)) = 1
            [SubToggle(GlobalSetting)]_ShadowStength("是否接受投影",float) = 0
            [Title(GlobalSetting,ToneMapping)]
            [Sub(GlobalSetting)]_ToneMappingStrength("ToneMapping强度",range(0,1)) = 0
        
//        [Header(Emission)]
//        [HDR]_EmissionColor("自发光颜色控制",Color) = (0,0,0,1)
        
//        [Header(Texture)]
//         [HideInInspector]_FaceControlSpeed("面部阴影过渡速度 默认1",Range(0,2)) = 1
//         [HideInInspector]_FaceControlOffset("面部阴影过渡偏移值 默认0",Range(-1,1)) = 0
//         [HideInInspector]_FaceControlExp("sdf贴图缩放值",range(0,3))=1
        //_FaceControlSmooth("面部阴影贴柔和度 默认为1",Range(0,1)) = 1
        
        [Main(MainTexture)] _group ("主贴图设置", float) = 0
            [Sub(MainTexture)]_MainTex ("基础色贴图", 2D) = "white" {}
            [Sub(MainTexture)][HDR]_MainColor("主颜色",Color)= (1, 1, 1,1)
            [Sub(MainTexture)]_LightMap("卡通光照X贴图(R:金属度 G:AO  B:光滑度 A:材质区分)", 2D) = "white" {}
            [Sub(MainTexture)]_AOIndencity("AO强度", Range(0.01, 0.99)) = 1
            [Sub(MainTexture)]_NormalMap("法线贴图",2D) = "bump" {}
            [Sub(MainTexture)]_NormalStrength("法线强度",Range(0,4)) = 1
            [Sub(MainTexture)][Space(20)]
       [Main(EmiTexture)]_group2 ("自发光", float) = 0 
            [Sub(EmiTexture)]_EmissionTexture("R:自发光区域 G:边缘光遮罩(白色区域出现边缘光, 黑色区域不出现)",2D) = "white" {}
            [Sub(EmiTexture)][HDR]_EmissionColor("自发光颜色",Color) = (1,1,1,1)
//        [Main(RampTexture)]_group2 ("Ramp贴图", float) = 0
//            [Sub(RampTexture)]_RampTexture("Ramp贴图集合",2D) = "white" {}
//            [Sub(RampTexture)]_RampStrength("Ramp贴图强度",Range(0,1)) = 1
//            [Sub(RampTexture)]_SkinRampStrength("皮肤部分(灰度1)Ramp贴图强度",Range(0,1)) = 1
//            [Sub(RampTexture)]_SilkRampStrength("丝绸部分(灰度0.7)Ramp贴图强度",Range(0,1)) = 1
//            [Sub(RampTexture)]_MetalRampStrength("金属部分(灰度0.5)Ramp贴图强度",Range(0,1)) = 1
//            [Sub(RampTexture)]_SoftRampStrength("柔软物体(灰度0.3)Ramp贴图强度",Range(0,1)) = 1
//            [Sub(RampTexture)]_HardRampStrength("硬的物体(灰度0)Ramp贴图强度",Range(0,1)) = 1
        
        [Main(RimLight)]_group3 ("边缘光", float) = 0
            [Sub(RimLight)]_RimLightThreshold("边缘光阈值,(越低则越多地方受边缘光影响)", Range(0,0.2)) = 0.02
            [Sub(RimLight)]_RimLightFadeout("边缘光淡出(越高则边缘光更柔和)", Range(0,1)) = 0.1
//            [KWEnum(RimLight,Right, _RIMLIGHTRIGHT,Left, _RIMLIGHTLEFT, Double, _DOUBLE)]_RimLightMode("边缘光模式(在右侧显示,在左侧显示或者在双边显示)",float) = 0
            //[Sub(RimLight)]_SkinDepthRimOffset("皮肤部分(灰度1)边缘光偏移量",Range(0,5)) = 0.3
            [Sub(RimLight)][HDR]_SkinRimColor("皮肤部分边缘光颜色修正", Color) = (0.5, 0.5, 0.5, 1)
            //[Sub(RimLight)]_NoSkinDepthRimOffset("非皮肤部分(灰度不为1)边缘光偏移量",Range(0,5)) = 0.3
            [Sub(RimLight)][HDR]_NoSkinRimColor("非皮肤部分边缘光颜色修正", Color) = (0.5, 0.5, 0.5, 1)
//            [Sub(RimLight)]_SilkDepthRimOffset("丝绸部分(灰度0.7)边缘光偏移量",Range(0,5)) = 0.3
//            [Sub(RimLight)][HDR]_SilkRimColor("丝绸部分边缘光颜色修正", Color) = (0.5, 0.5, 0.5, 1)
//            [Sub(RimLight)]_MetalDepthRimOffset("金属部分(灰度0.5)边缘光偏移量",Range(0,5)) = 0.3
//            [Sub(RimLight)][HDR]_MetalRimColor("金属部分边缘光颜色修正", Color) = (0.5, 0.5, 0.5, 1)
//            [Sub(RimLight)]_SoftDepthRimOffset("柔软物体(灰度0.3)边缘光偏移量",Range(0,5)) = 0.3
//            [Sub(RimLight)][HDR]_SoftRimColor("柔软部分边缘光颜色修正", Color) = (0.5, 0.5, 0.5, 1)
//            [Sub(RimLight)]_HardDepthRimOffset("硬的物体(灰度0.0)边缘光偏移量",Range(0,5)) = 0.23
//            [Sub(RimLight)][HDR]_HardRimColor("硬的物体边缘光颜色修正", Color) = (0.5,0.5, 0.5, 1)
          
        
        [Main(SkinTex)]_group4 ("皮肤材质区域(灰度1)", float) = 0
            [Title(SkinTex,Diffuse)]
            [Sub(SkinTex)]_FirstLayerColor1("受光区颜色", Color) = (1, 1, 1, 1)
		    [Sub(SkinTex)]_SecondLayerColor1("过渡区颜色", Color) = (0.848, 0.7497263, 0.721648, 1)
		    [Sub(SkinTex)]_ThirdLayerColor1("暗部颜色", Color) = (0.8, 0.5584577, 0.5215999, 1)
		    [Sub(SkinTex)]_ShadowThresholdCenter1("一层光照分色位置", Range(0, 1)) = 0.5
		    [Sub(SkinTex)]_SecondShadowThresholdCenter1("二层光照分色位置", Range(0, 1)) = 0.213
		    [Sub(SkinTex)]_ShadowThresholdSoftness1("一层光照柔和度", Range(0, 1)) = 0.083
		    [Sub(SkinTex)]_SecondShadowThresholdSoftness1("二层光照柔和度", Range(0, 1)) = 0.075
        
            [Title(SkinTex,Specular)]
            [Sub(SkinTex)]_SpecularSize1("高光大小",Range(0.001,1)) = 0.1
            [Sub(SkinTex)]_SpecularSoftness1("高光柔和度",Range(0.001,10)) = 0.5
            [Sub(SkinTex)]_SpecularAlbedoWeight1("高光强度(值越小亮度越大)",Range(-3,1)) = 0.5
            
        
        [Main(SilkTex)]_group5 ("丝绸材质区域(灰度0.7)", float) = 0
            [Sub(SilkTex)][Title(SilkTex,Diffuse)]
            [Sub(SilkTex)]_FirstLayerColor2("受光区颜色", Color) = (1, 1, 1, 1)
		    [Sub(SilkTex)]_SecondLayerColor2("过渡区颜色", Color) = (0.848, 0.7497263, 0.721648, 1)
		    [Sub(SilkTex)]_ThirdLayerColor2("暗部颜色", Color) = (0.8, 0.5584577, 0.5215999, 1)
		    [Sub(SilkTex)]_ShadowThresholdCenter2("一层光照分色位置", Range(0, 1)) = 0.5
		    [Sub(SilkTex)]_SecondShadowThresholdCenter2("二层光照分色位置", Range(0, 1)) = 0.213
		    [Sub(SilkTex)]_ShadowThresholdSoftness2("一层光照柔和度", Range(0, 1)) = 0.083
		    [Sub(SilkTex)]_SecondShadowThresholdSoftness2("二层光照柔和度", Range(0, 1)) = 0.075
//            [Title(SilkTex,Specular)]
//            [Sub(SilkTex)]_SpecularSize2("高光大小",Range(0.001,1)) = 0.1
//            [Sub(SilkTex)]_SpecularSoftness2("高光柔和度",Range(0.001,10)) = 0.5
//            [Sub(SilkTex)]_SpecularAlbedoWeight2("高光强度(值越小亮度越大)",Range(-3,1)) = 0.5
            [Title(SilkTex,Anisotropy HighLight)]
            [Sub(SilkTex)]_AnisotropyStrength("各向异性程度度",Range(0,1)) = 0.8
            [Sub(SilkTex)][HDR]_AnisSpecularColor("各向异性高光颜色(Alpha控制透明度)",Color) = (0.5,0.5,0.5,0.5)
            [Sub(SilkTex)]_AnisSmoothnessStrength("光滑度对各向异性高光的调整,光滑度越低,高光越弱(默认为1)",Range(0,3)) = 1
        
        [Main(MetalTex)]_group6 ("金属材质区域(灰度0.5)", float) = 0
            [Title(MetalTex,MatCap)]
            [Sub(MetalTex)]_MatCapTex("金属MatCap", 2D) = "Black" {}
            [Sub(MetalTex)][HDR]_MatCapColor("MatCap颜色", Color) = (0.5,0.5,0.5,1)
            //[Sub(MetalTex)]_MetallicStrength("金属颜色的明暗程度", Range(0,1)) = 1
            [Title(MetalTex,Diffuse)]
            [Sub(MetalTex)]_FirstLayerColor3("受光区颜色", Color) = (1, 1, 1, 1)
		    [Sub(MetalTex)]_SecondLayerColor3("过渡区颜色", Color) = (0.848, 0.7497263, 0.721648, 1)
		    [Sub(MetalTex)]_ThirdLayerColor3("暗部颜色", Color) = (0.8, 0.5584577, 0.5215999, 1)
		    [Sub(MetalTex)]_ShadowThresholdCenter3("一层光照分色位置", Range(0, 1)) = 0.5
		    [Sub(MetalTex)]_SecondShadowThresholdCenter3("二层光照分色位置", Range(0, 1)) = 0.213
		    [Sub(MetalTex)]_ShadowThresholdSoftness3("一层光照柔和度", Range(0, 1)) = 0.083
		    [Sub(MetalTex)]_SecondShadowThresholdSoftness3("二层光照柔和度", Range(0, 1)) = 0.075
            [Title(MetalTex,Specular)]
            [Sub(MetalTex)]_SpecularSize3("高光大小",Range(0.001,1)) = 0.1
            [Sub(MetalTex)]_SpecularSoftness3("高光柔和度",Range(0.001,10)) = 0.5
            [Sub(MetalTex)]_SpecularAlbedoWeight3("高光强度(值越小亮度越大)",Range(-3,1)) = 0.5
        
        [Main(SoftTex)]_group7 ("软材质区域(灰度0.3)", float) = 0
            [Title(SoftTex,Diffuse)]
            [Sub(SoftTex)]_FirstLayerColor4("受光区颜色", Color) = (1, 1, 1, 1)
		    [Sub(SoftTex)]_SecondLayerColor4("过渡区颜色", Color) = (0.848, 0.7497263, 0.721648, 1)
		    [Sub(SoftTex)]_ThirdLayerColor4("暗部颜色", Color) = (0.8, 0.5584577, 0.5215999, 1)
		    [Sub(SoftTex)]_ShadowThresholdCenter4("一层光照分色位置", Range(0, 1)) = 0.5
		    [Sub(SoftTex)]_SecondShadowThresholdCenter4("二层光照分色位置", Range(0, 1)) = 0.213
		    [Sub(SoftTex)]_ShadowThresholdSoftness4("一层光照柔和度", Range(0, 1)) = 0.083
		    [Sub(SoftTex)]_SecondShadowThresholdSoftness4("二层光照柔和度", Range(0, 1)) = 0.075
            [Title(SoftTex,Specular)]
            [Sub(SoftTex)]_SpecularSize4("高光大小",Range(0.001,1)) = 0.1
            [Sub(SoftTex)]_SpecularSoftness4("高光柔和度",Range(0.001,10)) = 0.5
            [Sub(SoftTex)]_SpecularAlbedoWeight4("高光强度(值越小亮度越大)",Range(-3,1)) = 0.5
        
        [Main(HardTex)]_group8 ("硬材质区域(灰度0.0)", float) = 0
            [Title(HardTex,Diffuse)]
            [Sub(HardTex)]_FirstLayerColor5("受光区颜色", Color) = (1, 1, 1, 1)
		    [Sub(HardTex)]_SecondLayerColor5("过渡区颜色", Color) = (0.848, 0.7497263, 0.721648, 1)
		    [Sub(HardTex)]_ThirdLayerColor5("暗部颜色", Color) = (0.8, 0.5584577, 0.5215999, 1)
		    [Sub(HardTex)]_ShadowThresholdCenter5("一层光照分色位置", Range(0, 1)) = 0.5
		    [Sub(HardTex)]_SecondShadowThresholdCenter5("二层光照分色位置", Range(0, 1)) = 0.213
		    [Sub(HardTex)]_ShadowThresholdSoftness5("一层光照柔和度", Range(0, 1)) = 0.083
		    [Sub(HardTex)]_SecondShadowThresholdSoftness5("二层光照柔和度", Range(0, 1)) = 0.075
            [Title(HardTex,Specular)]
            [Sub(HardTex)]_SpecularSize5("高光大小",Range(0.001,1)) = 0.1
            [Sub(HardTex)]_SpecularSoftness5("高光柔和度",Range(0.001,10)) = 0.5
            [Sub(HardTex)]_SpecularAlbedoWeight5("高光强度(值越小亮度越大)",Range(-3,1)) = 0.5
        //[Header(MatCap)]
        
        [Main(Alpha)]_group9 ("Alpha", float) = 0
            [SubToggle(Alpha)]_IfVertexAlpha("是否启用顶点色(顶点色G通道控制)控制透明度?",float) = 0
            [Sub(Alpha)]_VertexAlpha("顶点色控制的不透明度强度,(顶点色g通道控制)",range(0,1)) = 1
            [Sub(Alpha)]_SpecularToAlpha("高光对不透明度的影响,值越大则高光处越不透明",range(0,1)) = 0
            [Sub(Alpha)]_SpecularAlphaRange("半透明处的高光范围控制,值越大范围越小",range(0,1)) = 0
        
        [Main(Other)]_group10 ("杂项", float) = 0
        [Title(Other,Shining)]
        [Sub(Other)]_ShiningColor("闪烁颜色",Color) = (1,1,1,1)
        [Sub(Other)]_ColorChange("控制闪烁程度的参数(_ColorChange) 0-不影响  1-完全变为闪烁颜色",Range(0,1)) = 0
        
        [Title(Other,UVMove)]
        [Sub(Other)]_NormalTex_Uspeed("NormalTex_Uspeed", Float) = 0
        [Sub(Other)]_NormalTex_Vspeed("NormalTex_Vspeed", Float) = 0
        [Sub(Other)]_NormalTex("NormalTex", 2D) = "white" {}

        [Title(Other,Dissolve)]
        [Sub(Other)][KeywordEnum(On,Off)]_Dissolve("是否启动溶解效果",float) = 1
        [Sub(Other)]_DissolveTex("DissolveTex(溶解贴图)",2D) = "" {}
        [Sub(Other)]_Clip_Char("Clip",Range(0,1)) = 0
        [Sub(Other)][HDR]_DissolveEntityColor("溶解时颜色",Color) = (0.19,0.54,0.46,1)
        [Sub(Other)]_DissolveEntityColorStrength("值越高则越偏向溶解时颜色",Range(0,1)) = 0
        [Sub(Other)][HDR]_DissolveLightingColor("溶解时整体亮色(混合模式为相加,不用时设置为纯黑)",Color) = (0,0,0,1)
        [Sub(Other)]_DissolveDarkColor("溶解时整体暗色(混合模式为正片叠底,不用时设置为纯白)",Color) = (1,1,1,1)
        [Sub(Other)][HDR]_RampColor("溶解时边缘亮色",Color) = (0.5,1.9,3.7,0)
        [Sub(Other)]_RampRange("高亮范围",Range(0,1)) = 0.1
        
        [Title(Other,Alhpa)]
        [Sub(Other)]_Alpha("角色Alpha值",Range(0,1)) = 1
        
        [Title(Other,Stencil)]
        [Sub(Other)]_OutlineStencilComp("模板值",int) = 0
        [Sub(Other)][Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("模板缓冲区Comp的操作",int) = 0
        [Sub(Other)][Enum(UnityEngine.Rendering.StencilOp)]_StencilOp("模板缓冲区Pass的操作",int) = 0
        
        [Title(Other,Zwrite)]
        [Sub(Other)][Enum(Off, 0, On, 1)]_ZwriteOp_Char("深度写入开关",int) = 1
        
        [Title(Other,RenderFrontBack)]
        [Sub(Other)][Enum(UnityEngine.Rendering.CullMode)]_CullOp_Char("剔除面,Off-不剔除,Back-剔除背面,Front-剔除正面",float) = 0
        
        [HideInInspector]_FrontDirHeadWS("面部的前向方向 世界空间",Vector) = (-1,0,0)
        [HideInInspector]_LeftDirHeadWS("面部的左侧方向 世界空间",Vector) = (0,0,-1)

    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue" = "Geometry+15"
            "RenderPipeline" = "UniversalPipeline"

        }
        HLSLINCLUDE
        float _SGameCharMainLightMultipler;
        float _SGameCharGIMultipler;
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float _ShadowStength;
            int _Flag;
            float _GlobalLightParm;
            float _smoothness;
            float _DirectionLightControler;
            float _AOIndencity;
            float _SpecualrIndencity;
            float _SpecularStrength;

            float _NormalStrength;

            float _HeadShadowSoftness;
            
            float4 _MatCapColor;
            float _MetallicStrength;
            float _RimWidth;
            float _RimSmoothness;
            half4 _RimColor;
            half _DepthRimOffset;
            float _SkinDepthRimOffset;
            float4 _SkinRimColor;
            float _NoSkinDepthRimOffset;
            float4 _NoSkinRimColor;
            float _SilkDepthRimOffset;
            float4 _SilkRimColor;
            float _MetalDepthRimOffset;
            float4 _MetalRimColor;
            float _SoftDepthRimOffset;
            float4 _SoftRimColor;
            float _HardDepthRimOffset;
            float4 _HardRimColor;

            float _RimLightThreshold;
            float _RimLightFadeout;
            float _RimLightMode;
        
            uniform float _NormalTex_Uspeed;
            uniform float _NormalTex_Vspeed;

            float4 _MainColor;
            ///////////////////////////////////////
            ///Ramp过渡
            float _uvOffset;
            //顶点色不透明度
            float _IfVertexAlpha;
            float _VertexAlpha;
            float _SpecularToAlpha;
            float _SpecularAlphaRange;
            //溶解
            sampler2D _DissolveTex;
            float4 _DissolveTex_ST;
            float _Clip_Char;

            /////////////////////////////////////////
            float _SpecularSize1;
            float _SpecularSoftness1;
            float _SpecularAlbedoWeight1;
            float _SpecularSize2;
            float _SpecularSoftness2;
            float _SpecularAlbedoWeight2;
            float _SpecularSize3;
            float _SpecularSoftness3;
            float _SpecularAlbedoWeight3;
            float _SpecularSize4;
            float _SpecularSoftness4;
            float _SpecularAlbedoWeight4;
            float _SpecularSize5;
            float _SpecularSoftness5;
            float _SpecularAlbedoWeight5;
            /////////////////////////////////////////
            float _AnisotropyStrength;
            float4 _AnisSpecularColor;
            float _AnisSmoothnessStrength;

            float4 _SkinDarkColor;
            float _SkinDarkRange;
            float4 _Mat2DarkColor;
            float _Mat2DarkRange;
            float4 _Mat3DarkColor;
            float _Mat3DarkRange;
            float4 _Mat4DarkColor;
            float _Mat4DarkRange;
            float4 _Mat5DarkColor;
            float _Mat5DarkRange;

            ///////////////////////////////////////////////////////////
            half4 _FirstLayerColor1;
		    half4 _SecondLayerColor1;
		    half4 _ThirdLayerColor1;
		    half _ShadowThresholdCenter1;
		    half _SecondShadowThresholdCenter1;
		    half _ShadowThresholdSoftness1;
		    half _SecondShadowThresholdSoftness1;
            half4 _FirstLayerColor2;
		    half4 _SecondLayerColor2;
		    half4 _ThirdLayerColor2;
		    half _ShadowThresholdCenter2;
		    half _SecondShadowThresholdCenter2;
		    half _ShadowThresholdSoftness2;
		    half _SecondShadowThresholdSoftness2;
            half4 _FirstLayerColor3;
		    half4 _SecondLayerColor3;
		    half4 _ThirdLayerColor3;
		    half _ShadowThresholdCenter3;
		    half _SecondShadowThresholdCenter3;
		    half _ShadowThresholdSoftness3;
		    half _SecondShadowThresholdSoftness3;
            half4 _FirstLayerColor4;
		    half4 _SecondLayerColor4;
		    half4 _ThirdLayerColor4;
		    half _ShadowThresholdCenter4;
		    half _SecondShadowThresholdCenter4;
		    half _ShadowThresholdSoftness4;
		    half _SecondShadowThresholdSoftness4;
            half4 _FirstLayerColor5;
		    half4 _SecondLayerColor5;
		    half4 _ThirdLayerColor5;
		    half _ShadowThresholdCenter5;
		    half _SecondShadowThresholdCenter5;
		    half _ShadowThresholdSoftness5;
		    half _SecondShadowThresholdSoftness5;
            ///////////////////////////////////////////////////////////

            float4 _EmissionColor;
            // float _RampRange;
            // float4 _RampColor;
            // float _RampStrength;
            // float _SkinRampStrength;
            // float _SilkRampStrength;
            // float _MetalRampStrength;
            // float _SoftRampStrength;
            // float _HardRampStrength;
        
            float _Dissolve;
            float4 _DissolveLightingColor;
            float _DissolveEntityColorStrength;
            float4 _DissolveEntityColor;
            float4 _DissolveDarkColor;
            
            float4 _ShiningColor;
            float _ColorChange;
            float _MetalIntensity;
            
            ///////////////////////////////////////
            ///冰冻特效施工处  全局参数
        
            float _BaseColorDarkness;
            float4 _BaseColorColor;
            
            ///////////////////////////////////////
            
            float4 _FrontDirHeadWS;
            float4 _LeftDirHeadWS;

            float _ToneMappingStrength;
        
            float _Alpha;
            //float _OutlineAlpha;
            int _StencilComp;
            int _StencilOp;
            int _OutLineStencilComp;

            float _CullOp_Char;
        
        CBUFFER_END

       
        
        TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
        TEXTURE2D(_LightMap);   SAMPLER(sampler_LightMap);
        TEXTURE2D(_NormalMap);  SAMPLER(sampler_NormalMap);
        //TEXTURE2D(_RampTexture);   SAMPLER(sampler_RampTexture);
        TEXTURE2D(_EmissionTexture);  SAMPLER(sampler_EmissionTexture);
        TEXTURE2D(_MatCapTex);    SAMPLER(sampler_MatCapTex);
        //TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
        ENDHLSL
        Pass
        {
            Name "CharLighting"
            Tags { "LightMode" = "UniversalForward" }
            Cull [_CullOp_Char]
            Blend SrcAlpha OneMinusSrcAlpha
            //Blend One Zero
            
            ZWrite [_ZwriteOp_Char]
            
            Stencil
            {
                Ref [_OutlineStencilComp]
                Comp [_StencilComp]
                Pass [_StencilOp]
            }
//            Stencil
//            {
//                Ref 114
//                Comp Equal
//                Pass [_StencilOp]
//            }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_instancing

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE  //_MAIN_LIGHT_SHADOWS_SCREEN
            //关闭多光源
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT
           // #pragma multi_compile _ _RIMLIGHTRIGHT _RIMLIGHTLEFT
           // #pragma multi_compile  _DISSOLVE_ON

            #pragma shader_feature _ _CUSTOM_SHADOW_ON
            #pragma shader_feature _ _MAINLIGHT_CUSTOM_SHADOW_ON
            #pragma multi_compile _ _ESM


            //角色额外光照
           // #pragma shader_feature _ _GLOBAL_CHAR_DARKENVIROMENT
            //#pragma multi_compile _ _UVMOVE_COMMON
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Macros.hlsl"

            ////////////
            #include "../lib/NPRLighting/SGAME-NPRLighting.hlsl"
            ////////////

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
                float2 texcoord2: TEXCOORD1;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                float4 vertexColor : TEXCOORD4;
                float4  tangentWS : TEXCOORD5;
                float4 bioTangentWS : TEXCOORD6;
                //float4 positionOS :TEXCOORD5;
                float4 vertexSH : TEXCOORD7;
                //float4 positionSS : TEXCOORD5;
                //float4 posPlaneWS : TEXCOORD6;
            };
            

            Varyings vert (Attributes v)
            {
                Varyings o = (Varyings)0;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv.xy = v.texcoord;
                o.uv.z = ComputeFogFactor(o.positionCS.z);    //uv的z值存储雾.
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.shadowCoord = TransformWorldToShadowCoord(o.positionWS);
                o.vertexColor = v.color;
                //o.positionSS = ComputeScreenPos(o.positionCS);
                
                float3 originalPosVS = TransformWorldToView(TransformObjectToWorld(float3(0,0,0)+float3(0,0,0)));
                float3 posVS = TransformWorldToView(o.positionWS);
                posVS = float3(posVS.x, posVS.y,originalPosVS.z);
                //o.posPlaneWS = mul(UNITY_MATRIX_I_V,float4(posVS.xyz,1));
                //o.positionOS = v.positionOS;

                o.tangentWS.xyz = 1.0 / length(o.normalWS.xyz )  * normalize(TransformObjectToWorldDir(v.tangentOS.xyz));
                o.tangentWS.a = v.tangentOS.a;
                float crossSign = (o.tangentWS.a > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
                float3 bitang = crossSign * cross(o.normalWS.xyz, o.tangentWS.xyz);
                o.bioTangentWS.xyz =1.0 / length(o.normalWS.xyz )  *bitang;
                
                // #ifdef _DISSOLVE_ON
                // float crossSign = (v.tangent.a > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
                // v.tangent.xyz = 1.0 / length(v.tangent.xyz ) * normalize(v.tangent.xyz); 
                // float3 BioTangent =1.0 / length(v.normalOS.xyz )* cross(v.normalOS.xyz,v.tangent.xyz)* crossSign  /** v.tangentOS.w*/;
                // float2 normalXY = v.texcoord2.xy;
                // float3 normalZ = cross(float3(0,v.texcoord2.y,0),float3(v.texcoord2.x,0,0));
                // float3 BakeNormal = normalize(UnpackNormal(float4(normalXY.x,normalXY.y,1,1)));
                // float3x3 tangentTransform = float3x3(v.tangent.xyz, BioTangent.xyz, v.normalOS.xyz);
                // float3 SmoothNormalOS = float3(v.tangent.x * BakeNormal.x+ BioTangent.x * BakeNormal.y + v.normalOS.x * BakeNormal.z,
                //                                 v.tangent.y * BakeNormal.x + BioTangent.y  * BakeNormal.y + v.normalOS.y * BakeNormal.z,
                //                                 v.tangent.z * BakeNormal.x + BioTangent.z  * BakeNormal.y + v.normalOS.z * BakeNormal.z);
                // //在开启溶解的时候把PositionOS用来传平滑法线,为后续溶解时获得平滑法线
                // o.positionOS.xyz = TransformObjectToWorldDir(SmoothNormalOS.xyz);
                 OUTPUT_SH(o.normalWS.xyz, o.vertexSH);
                // #endif
                
                
                return o;
            }
            
            //法线强化函数
            void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
            {
                Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
            }
            
            half4 frag (Varyings i) : SV_Target
            {


                // 法线
                ////////////////////////////////////////////////////////
                float3 normal = float3(0,0,1);
                half4 normalTex = SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,i.uv.xy);
                //half AO = normalTex.z;
                half Inline = normalTex.a;
                normalTex.z = 1;
                normalTex.xyz = UnpackNormalScale(normalTex,_NormalStrength);
                normalTex.xyz = SafeNormalize(normalTex.xyz);
                //return float4(normalTex.xyz,1);
                Unity_NormalStrength_float(normalTex.xyz, 1, normal.xyz);
                //法线切线空间转世界空间
                half3x3 tangentToWorld = half3x3( i.tangentWS.xyz,i.bioTangentWS.xyz,i.normalWS.xyz);
                normal = TransformTangentToWorld(normal.xyz,tangentToWorld);
                normal = normalize(normal);

                //切线根据法线贴图转世界空间
                half3 tangentTS = normalize(normalTex.x * half3(0, 0, 1)  + half3(1, 0, 0));
                half3 T = TransformTangentToWorld(tangentTS, tangentToWorld);
                T = NormalizeNormalPerPixel(T);
                //副切线根据法线贴图转世界空间
                half3 bitangentTS = normalize(normalTex.y * half3(0, 0, 1)  + half3(0, 1, 0));
                half3 B = TransformTangentToWorld(bitangentTS, tangentToWorld);
                B = NormalizeNormalPerPixel(B);



                //初始化InputData
                InputData inputData = (InputData)0;
                inputData.positionWS = i.positionWS;
                inputData.positionCS = i.positionCS;
                inputData.normalWS = normal;
                inputData.viewDirectionWS = normalize(_WorldSpaceCameraPos - i.positionWS);
                inputData.shadowCoord = i.shadowCoord;
                inputData.fogCoord = i.uv.z;
                inputData.vertexLighting = half3(0,0,0);
                inputData.bakedGI = SAMPLE_GI(staticLmName, i.vertexSH, i.normalWS);
                inputData.normalizedScreenSpaceUV = float2(0,0);//暂时用不着
                inputData.shadowMask = float4(1,1,1,1);
                inputData.tangentToWorld = half3x3( i.tangentWS.xyz,i.bioTangentWS.xyz,i.normalWS.xyz);
                ////////////////////////////////////////////////////////
                half4 albedoAlpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);

                
                half3 albedo = albedoAlpha * _MainColor.rgb;
                half alpha = albedoAlpha.a;
                half4 MRATexture = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, i.uv);
                ///LightMap 根据绝区零的贴图分析
                half metallic = MRATexture.r ;//根据绝区零的效果显示,似乎是控制金属区域,并且金属区域有matcap
                half smoothness = MRATexture.b * _smoothness;
                half3 specularr = half3(0,0,0);
                half3 emmisison = half3(0,0,0);
                
                /////根据不同区域设置不同的Mask
                half SkinMask = step(0.95f,MRATexture.a);
                half SilkMask = step(0.65f,MRATexture.a);
                //return half4(MRATexture.r,MRATexture.r,MRATexture.r,1);
                half MetallicMask = step(0.45f,MRATexture.a);
                half SoftMask = step(0.25f,MRATexture.a);
                half HardMask = 1 - SoftMask;
                 SoftMask = SoftMask * (1 - MetallicMask);
                 MetallicMask = MetallicMask * (1 - SilkMask);
                 SilkMask = SilkMask * (1 - SkinMask);
               


                Light mainLight = GetMainLight(i.shadowCoord);
                mainLight.color = mainLight.color * _DirectionLightControler;
                float3 N = normal;//normalize(i.normalWS);
                float3 L = normalize(mainLight.direction);
                float3 V = normalize(_WorldSpaceCameraPos - i.positionWS);
                float3 H = normalize(L + V);
                half4 mainLightColor = half4(mainLight.color, 1);
                half NoL = dot(N, L);
                half NOLnoNormalTex = dot(i.normalWS,L);
                

                #ifdef _GLOBAL_CHAR_DARKENVIROMENT
                    // 
                    mainLightColor.rgb = mainLightColor.rgb * _SGameCharMainLightMultipler;
                   // return float4(_SGameCharMainLightMultipler,_SGameCharMainLightMultipler,_SGameCharMainLightMultipler,1);
                #endif
                
                // 贴图采样
                half4 diffuse = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);

                // 自发光采样
                half4 emissionTex = SAMPLE_TEXTURE2D(_EmissionTexture, sampler_EmissionTexture, i.uv.xy);
                half3 emission = emissionTex.r * _EmissionColor.rgb;
                half rimLightMask = emissionTex.g;
                
                diffuse.rgb *= _MainColor.rgb;
                half3 diffuseHSV = RgbToHsv(diffuse);
                half4 lightMapColor = MRATexture;

                half Emission = diffuse.a;            //自发光区域
                half Glossiness = lightMapColor.r;    // Glossiness贴图
                half Smoothness = lightMapColor.b;     //粗糙度
                half ao = lightMapColor.g;          // AO
                ao = clamp(ao + 1 - _AOIndencity,0,1);    // AO强度调节
                half highLight = lightMapColor.g;   // 高光
                
                
                 /////////////////////////////////////////////////////////
                //RimLight - DepthOffsetRing
                // NPRAddInputData nprInputData;
                // nprInputData.linearEyeDepth = DepthSamplerToLinearDepth(i.positionCS.z);
                // half depthRimOffset = _SkinDepthRimOffset * SkinMask + _NoSkinDepthRimOffset * (1 - SkinMask);
                // half3 depthRimColor = _SkinRimColor * SkinMask + _NoSkinRimColor * (1 - SkinMask);
                //
                // depthRimOffset = lerp(depthRimOffset*0.5,depthRimOffset,NOLnoNormalTex);
                // half NoV2 = pow(dot(i.normalWS,SafeNormalize(V)),1);
                //
                // half rimLight2 = DepthRimZZZ(depthRimOffset,i.positionCS,i.normalWS,_RimLightThreshold,_RimLightFadeout);
                // rimLight2 *= 1- NoV2;
                // rimLight2 = lerp(rimLight2 * 0.4, rimLight2, NOLnoNormalTex);
                // half3 rimColorr = depthRimColor * rimLight2;

                //NdotV边缘光
                half3 depthRimColor = _SkinRimColor * SkinMask + _NoSkinRimColor * (1 - SkinMask);
                half NoV2 = pow(dot(normal,SafeNormalize(V)),1);
                half rimlightNoV = pow(clamp(1- NoV2,0,1),5);
                rimlightNoV = lerp(rimlightNoV*0.2,rimlightNoV,NoL);
                rimlightNoV = smoothstep(_RimLightThreshold, _RimLightThreshold+_RimLightFadeout, rimlightNoV);
                half3 rimColorr = mainLight.color * depthRimColor * rimlightNoV * rimLightMask;
                /////////////////////////////////////////////////////////
                //3层颜色分开
                float remappedNoL = NoL * 0.5 + 0.5;

                ao = saturate(smoothstep(0.4,0.5,lightMapColor.g) + lightMapColor.g);
                remappedNoL *= ao;
                half4 firstLayerColor = _FirstLayerColor1*SkinMask + _FirstLayerColor2*SilkMask + _FirstLayerColor3*MetallicMask + _FirstLayerColor4*SoftMask
                                        + _FirstLayerColor5*HardMask;
                
                half4 secondLayerColor = _SecondLayerColor1*SkinMask + _SecondLayerColor2*SilkMask + _SecondLayerColor3*MetallicMask + _SecondLayerColor4*SoftMask
                                        + _SecondLayerColor5*HardMask;
                half4 thirdLayerColor = _ThirdLayerColor1*SkinMask + _ThirdLayerColor2*SilkMask + _ThirdLayerColor3*MetallicMask + _ThirdLayerColor4*SoftMask
                                        + _ThirdLayerColor5*HardMask;
                half shadowThresholdCenter = _ShadowThresholdCenter1*SkinMask + _ShadowThresholdCenter2*SilkMask + _ShadowThresholdCenter3*MetallicMask + _ShadowThresholdCenter4*SoftMask
                                        + _ShadowThresholdCenter5*HardMask;
                half shadowThresholdSoftness = _ShadowThresholdSoftness1*SkinMask + _ShadowThresholdSoftness2*SilkMask + _ShadowThresholdSoftness3*MetallicMask + _ShadowThresholdSoftness4*SoftMask
                                        + _ShadowThresholdSoftness5*HardMask;
                half secondShadowThresholdCenter = _SecondShadowThresholdCenter1*SkinMask + _SecondShadowThresholdCenter2*SilkMask + _SecondShadowThresholdCenter3*MetallicMask + _SecondShadowThresholdCenter4*SoftMask
                                        + _SecondShadowThresholdCenter5*HardMask;
                half secondShadowThresholdSoftness = _SecondShadowThresholdSoftness1*SkinMask + _SecondShadowThresholdSoftness2*SilkMask + _SecondShadowThresholdSoftness3*MetallicMask + _SecondShadowThresholdSoftness4*SoftMask
                                        + _SecondShadowThresholdSoftness5*HardMask;

                half mainLightShadow = smoothstep(shadowThresholdCenter,shadowThresholdCenter+shadowThresholdSoftness,remappedNoL);
                
                half secondMainLightShadow = smoothstep(secondShadowThresholdCenter,secondShadowThresholdCenter + secondShadowThresholdSoftness,remappedNoL);

                half4 rampColor = lerp(secondLayerColor,firstLayerColor,mainLightShadow);
                rampColor = lerp(thirdLayerColor, rampColor, secondMainLightShadow);
                
                //albedo *= DarkColor.rgb;
                albedo = lerp(albedo,albedo * rampColor.rgb, rampColor.a);
                
                
                
                
                //初始化BRDF相关数据
                BRDFData brdfData;
                /////////////////////////////
                half brdfMetallic = Glossiness;
                InitializeBRDFData(diffuse, brdfMetallic, 0.7f,_smoothness, diffuse.a, brdfData);
                diffuse.rgb = brdfData.diffuse;
                //环境光
                half3 bakedGI = SAMPLE_GI(staticLmName, i.vertexSH, inputData.normalWS);
                MixRealtimeAndBakedGI(mainLight, i.normalWS, bakedGI, half4(1,1,1,0));
                //half3 globalIllumination = GlobalIllumination(brdfData, bakedGI, ao, i.normalWS,V) *_GlobalLightParm;
                //globalIllumination = 0;

                
                // #ifdef _GLOBAL_CHAR_DARKENVIROMENT
                // globalIllumination *= _SGameCharGIMultipler;
                // #endif
                
                
                //float selfShade = clamp(NoL * 0.5 + 0.5,0,1);
                //half shadeLight = clamp(1 -selfShade * (1- mainLight.distanceAttenuation*mainLight.shadowAttenuation),0,1)  * selfShade * ao;
               // half materialIndex = MRATexture.a;
                //half2 rampUV = half2(clamp(0,1,shadeLight), saturate(1 - materialIndex));
                
                //rampUV = clamp(0.02,0.98,rampUV);
                //float4 rampTex = SAMPLE_TEXTURE2D(_RampTexture, sampler_RampTexture, rampUV);
                //return float4(shadeLight.xxx,1);
                // half3 rampColor = lerp(rampTex.rgb, float3(0.5f,0.5f,0.5f), rampTex.a);
                // rampColor = rampTex.rgb;
                ///正片叠底混合
                //half3 diffuseTerm =lerp(diffuse.rgb, rampTex.rgb * diffuse.rgb,_RampStrength);
                
                
                ////////////////////////////////////////////////
                //新DS
                //PBR 光照计算
                //丝袜区域不算普通高光而是算各向异性高光  所以把丝绸区域的高光清零
                _SpecularSize2 = 0;
                _SpecularSoftness2 = 0;
                _SpecularAlbedoWeight2 = 1;
                
                half specularSize = _SpecularSize1 * SkinMask + _SpecularSize2 * SilkMask + _SpecularSize3 * MetallicMask
                                    + _SpecularSize4 * SoftMask + _SpecularSize5 * HardMask;
                half specularSoftness = _SpecularSoftness1 * SkinMask + _SpecularSoftness2 * SilkMask + _SpecularSoftness3 * MetallicMask
                                    + _SpecularSoftness4 * SoftMask + _SpecularSoftness5 * HardMask;
                half specularABweight = _SpecularAlbedoWeight1 * SkinMask + _SpecularAlbedoWeight2 * SilkMask + _SpecularAlbedoWeight3 * MetallicMask
                                    + _SpecularAlbedoWeight4 * SoftMask + _SpecularAlbedoWeight5 * HardMask;
                SpecularData specularData;
                //NPRSpecularController(smoothness, specularData.specularSize,specularData.specularSoftness,specularData.specularABweight);
                //return half4(specularData.specularSize,specularData.specularSize,specularData.specularSize,1);
                //return float4(specularData.specularABweight,specularData.specularABweight,specularData.specularABweight,1);
                //return half4(specularData.specularSize,specularData.specularSize,specularData.specularSize,1);
                  specularData.specularSize = specularSize;
                  specularData.specularSoftness =specularSoftness;
                  specularData.specularABweight = specularABweight;
                //硬材质区域 高光受smoothness影响
                specularData.specularABweight =lerp(specularData.specularABweight,1,(1 - Smoothness) * HardMask);
                
                //计算丝绸区域的各项异性高光
                half3 specularCol = _AnisSpecularColor.rgb;
                half Anisotropy = _AnisotropyStrength;
                half AnisotropyRoughness =  clamp(0.001,0.999,(1- Smoothness) * _AnisSmoothnessStrength);
                half3 StockingSpecular = SlikBRDF_Specular(specularCol,AnisotropyRoughness,Anisotropy,N,B,T,V,L,mainLight.color,1);
                StockingSpecular = StockingSpecular * _AnisSpecularColor.a * 0.5 * Smoothness * SilkMask;
                albedo += StockingSpecular;
                
                //albedo = lerp(albedo.rgb, rampTex.rgb * albedo.rgb,rampStrength);
                //half3 halfDir = SafeNormalize(L + V);
                //half NdotHClamp = saturate(dot(normal,halfDir));
                //half3 highLight1 = StylizedSpecular(brdfData.albedo, NdotHClamp,specularData.specularSize,specularData.specularSoftness,specularData.specularABweight);
                
                half4 PBRColor = UniversalFragment_SGChar_ZZZ(inputData,albedo,metallic * 0.3,specularr,smoothness,ao,emission,alpha,specularData,_DirectionLightControler,_ShadowStength);
                ////////////////////////////////////////////////
                ///////////////////////////////////////////////
                //MatCap
                half3 matCapCol = SamplerMatCap(_MatCapColor/*, half2 uv*/, N, TEXTURE2D_ARGS(_MatCapTex, sampler_MatCapTex));
                PBRColor.rgb += matCapCol * metallic * albedo  ;
                
                ///////////////////////////////////////////////
                ///最终输出
                half4 finalColor = float4(0,0,0,1);
                #ifdef _HEAD
                    finalColor.rgb = PBRColor ;
                #else
                    finalColor.rgb = PBRColor ;
                #endif

               
                
                finalColor.rgb = finalColor.rgb + rimColorr;

                 //ToneMapping
                half3 ToneMappingColor = Uncharted2ToneMapping(finalColor.rgb);
                finalColor.rgb = lerp(finalColor.rgb,ToneMappingColor.rgb,_ToneMappingStrength);
                finalColor.a = alpha;

                //混合雾
                finalColor.rgb = MixFog(finalColor.rgb, i.uv.z);
                
                return finalColor;
                
                ////////////////////////////////////////////////
                
//                 half4 diffuseAndShadow = float4(1,1,1,1);
//                 diffuseAndShadow.rgba = float4(diffuseTerm,1);
//                 
//
//                 // 高光
//                 half NoV = dot(N, V);
//                 half NoVPow = pow(abs(NoV), 10) * _SpecualrIndencity;
//                 // 高光乘以NoL可以在刚进入阴影时也有微弱高光
//                 half4 specularNoV = NoVPow;
//
//                 // 灯光高光BlinPhong
//                 half NoH = pow(abs(dot(N, H)), 100);
//                 half4 specularNoH = NoH;
//
//                 // 混合高光
//                 half4 specular;
//                 specular = lerp(specularNoV, specularNoH, 0.5);
//                 specular = saturate(specular /* * diffuse*/ * 3 * highLight * ao) * float4(mainLight.color,1) * diffuseHSV.b;  //混合阴影、漫反射颜色、高光MASK、ao
//                 //去除金属部分高光 单独计算
//                 specular = clamp(specular,0,1) * _SpecularStrength * (1-Glossiness) * float4(mainLight.color,1);
//                 
//                 //return float4(specular.rgb,1);
//                 float temp3 = highLight * (1-Glossiness);
//                 #ifdef _HAIR
//                     specular.rgb += _HairSpecularBase * mainLight.color * highLight;
//                     specular.rgb *= clamp(NoV,0,_HairSpecularthreshold);
//                     //return float4(specular.rgb,1);
//                 #endif
//
//                 //金属部分MatCap高光
//                 // 采样MatCap贴图
//                 #ifndef _HEAD
//                     #ifndef  _HAIR
//                         float3 V1 = V;
//                         float3 temp = mul(UNITY_MATRIX_V,float4(normalize(i.normalWS.xyz + float3(-mainLight.direction.x,0,0)),0).xyz);
//                         half4 MatCAP = SAMPLE_TEXTURE2D(_MatCapTex, sampler_MatCapTex,( ( (temp).xy / ( 2.828427 * sqrt( ( (temp).z + 1.0 ) ) ) ) + 0.5 ));
//                         MatCAP = SAMPLE_TEXTURE2D(_MatCapTex, sampler_MatCapTex,(temp.xy +float2(1,1))* 0.5 );
//                         float nov2 = pow(saturate(NoV),3);
//                         MatCAP *= pow(shadeLight,0.5);
//                         MatCAP.rgb *= Glossiness * nov2  * mainLight.color ;
//                         specular.rgb += MatCAP.rgb *_MatCapColor.rgb*highLight  ;
//                     #endif
//                 #endif
//                 //受顶点影响的半透明处高光 单独计算
//                 float4 VertexAlphaSpeuclar =  specular.rgba * smoothstep(_SpecularAlphaRange,_SpecularAlphaRange + 0.01,(specular.r + specular.g + specular.b)*0.3333);
//                 ///////////////////////////////////////////////////
//                 // 边缘光
//                 half rim =   step(1.01 - _RimWidth, clamp(1-NoV,0,1));
//                 rim = smoothstep(1.01-_RimWidth,1.01-_RimWidth + _RimSmoothness, clamp((1-NoV),0,1));
//                 half4 rimColor = rim  * ao * _RimColor /** shadeLight*/;
//                 half4 rimColor_mainLight =  rimColor;
//                 
//                 half4 finalSpecular = specular * (1 - i.vertexColor.g * _IfVertexAlpha)  + VertexAlphaSpeuclar * i.vertexColor.g * _IfVertexAlpha;
//                 
//                 half4 finalCol = (diffuseAndShadow  + finalSpecular ) * mainLight.distanceAttenuation * mainLightColor;
//                 finalCol = lerp(finalCol,rimColor_mainLight * _RimColor.a + finalCol * (1 - _RimColor.a) ,rim);
//                 
//                 // 额外灯
//                
//                 #ifdef _GLOBAL_CHAR_DARKENVIROMENT
//                    #ifndef _HEAD
//                     // 像素灯的数量
//                     uint pixelLightCount = GetAdditionalLightsCount();
//                     for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
//                     {
//                         //return float4(1,1,1,1)
//                         // 获得灯光颜色、方向、衰减、阴影衰减
//                         Light addLight = GetAdditionalLight(lightIndex, i.positionWS);
//                 
//                         //额外光只支持点光源 保证卡通渲染的效果
//                         //计算平面化的法线
//                         //half3 normalPlaneWS = normalize(mul(UNITY_MATRIX_I_V,float3(0,0,1)));
//                 
//                         //half NoLadd = saturate(dot(addLight.direction,normalPlaneWS));
//                         half NoLadd = saturate(dot(addLight.direction,N));
//                         NoLadd = step(0.5, NoLadd);
//                         //half NoLadd = 0.5;
//                         half3 diffuseAdd = NoLadd * addLight.distanceAttenuation;
//                         //为了防止再高光和边缘光处过曝 在高光和边缘光处适当减弱额外光源的影响
//                         half minusSpecularRimLight = saturate(1 - saturate(specular + rim) );
//                         diffuseAdd = diffuseAdd * saturate(ao+0.3f) * minusSpecularRimLight * diffuse.rgb * addLight.color;
//                         finalCol.rgb += diffuseAdd;
//                         
//                     }
//                   #else //如果该shader为头部
//                     //SDF控制面部阴影
//                     // 获得灯光颜色、方向、衰减、阴影衰减
//                      uint pixelLightCount = GetAdditionalLightsCount();
//                     for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
//                     {
//                         Light addLight = GetAdditionalLight(lightIndex, i.positionWS);
//                         float3 forward2 = _FrontDirHeadWS;
//                         float3 left2 = _LeftDirHeadWS;
//                         float3 lightDirH2 = normalize(float3(addLight.direction.x, 0, addLight.direction.z));
//                         float filpU2 = sign(dot(lightDirH2, left2));
//                         float lightAtten2 = saturate(dot(lightDirH2, forward2)*_FaceControlSpeed +  _FaceControlOffset);
//                         float3 shaodwRamp2 = SAMPLE_TEXTURE2D(_HeadShodwTex, sampler_HeadShodwTex,i.uv  * float2(filpU2, 1)).rgb;
//                  
//                         float shadowRampG2 = pow(1- shaodwRamp2.g,_FaceControlExp);
//                         float faceShadow2 = smoothstep(shadowRampG2,1- shaodwRamp2.g+0.02 /** _FaceControlSmooth*/,lightAtten2);
//                         half shadeLight2 = ao;
//                         //shadeLight *= hairMask.g;
//                         shadeLight2 *= faceShadow2;
//                         half minusSpecularRimLight = saturate(1 - saturate(specular + rim) );
//                         half3 diffuseAdd = saturate(ao+0.3f) * minusSpecularRimLight * diffuse.rgb * addLight.color * 0.7f;
//                         finalCol.rgb += diffuseAdd * shadeLight2;
//                     }
//                     
//                    #endif
//                 #endif
//                 
//                 // 加入自发光
//                 finalCol.rgb += Emission * _EmissionColor;
//                 //加入环境光
//                 finalCol.rgb = finalCol.rgb + 1*globalIllumination;
//                 
//                 //处理闪烁效果
//                 finalCol = lerp(finalCol,_ShiningColor, _ColorChange);
//
//                 
//
// #ifdef _UVMOVE_COMMON
//                 float2 uvMoveSpeed = float2(_NormalTex_Uspeed, _NormalTex_Vspeed);
//                 float2 movedUV = _Time.y * uvMoveSpeed + i.uv;
//                 float4 normalColor = SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, movedUV);
//                 finalCol += normalColor;
// #endif
//
//                 
// // #ifdef _DISSOLVE_ON
// //                 //return float4(i.positionOS.yy,1,1);
// //                 float2 dissolveUV = i.positionCS.xy * (_DissolveTex_ST.xy*0.001) + _DissolveTex_ST.zw;
// //                 float4 dissolveCol = tex2D(_DissolveTex, dissolveUV);
// //                 float clipDissolve = _Clip_Char * (1 + 2 * _RampRange) - _RampRange;
// //                 float NoVSmoothness = dot(i.positionOS.xyz, V);
// //                 float dissolveDir = clamp((dissolveCol.r + 0.2)* ((NoVSmoothness *0.5+0.5) *2),0,0.99);
// //                 float ColorRange = step(clipDissolve,dissolveDir) * (1 - step(clipDissolve + _RampRange,dissolveDir));
// //                 finalCol.rgb = (finalCol.rgb + _DissolveLightingColor.rgb)*_DissolveDarkColor;
// //                 finalCol.rgb = lerp(finalCol.rgb,_DissolveEntityColor, _DissolveEntityColorStrength);
// //                 finalCol.rgb = lerp(finalCol.rgb,_RampColor.rgb,ColorRange);
// //                 clip(dissolveDir - (clipDissolve));
// //                // return float4(dissolveDir,dissolveDir,dissolveDir,1);
// // #endif
//                 //混合雾
//                 finalCol.rgb = MixFog(finalCol.rgb, i.uv.z);
//                 //混合顶点色的透明度
//                 finalCol.a = lerp(1,_VertexAlpha  + (VertexAlphaSpeuclar.r+VertexAlphaSpeuclar.g+VertexAlphaSpeuclar.b) * 0.3333 * _SpecularToAlpha,i.vertexColor.g * _IfVertexAlpha);
//                 //设置角色Alpha值(如有需要)
//                 finalCol.a *=_Alpha;
//                 return finalCol;
            }
            ENDHLSL
        }

        pass
        {
            Tags 
            {
             "LightMode" = "ShadowCaster" 
             "RenderType"="Opaque" 
             "Queue" = "Geometry"
            }
            Cull off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

           
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            #pragma multi_compile _ _DISSOLVE_ON

            float4 GetShadowPositionHClip1(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                Light l = GetMainLight();
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, l.direction));

            #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
            #else
                positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
            #endif

                return positionCS;
            }

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;

                o.uv = v.texcoord.xy;
                o.positionCS = GetShadowPositionHClip1(v);

                return o;
            }

            half4 frag(Varyings i) : SV_TARGET
            {
                #ifdef _DISSOLVE_ON
                clip(-1);
                #endif
                #ifdef _DISSOLVE_ON
                float2 dissolveUV = i.uv * (_DissolveTex_ST.xy) + _DissolveTex_ST.zw;
                float4 dissolveCol = tex2D(_DissolveTex, dissolveUV);
                //float clipDissolve = _Clip_Char * (1 + 2 * _RampRange) - _RampRange;
                clip(dissolveCol.r - (_Clip_Char));
                #endif
                return 0;
            }

            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull back
            
            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex DepthOnlyVertexSGAME
            #pragma fragment DepthOnlyFragmentSGAME

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            //#pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma multi_compile _ _DISSOLVE_ON

            //#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            struct Attributes
            {
                float4 position     : POSITION;
                float2 texcoord     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv           : TEXCOORD0;
                float4 positionCS   : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            half Alpha(half albedoAlpha, half4 color, half cutoff)
            {
            #if !defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A) && !defined(_GLOSSINESS_FROM_BASE_ALPHA)
                half alpha = albedoAlpha * color.a;
            #else
                half alpha = color.a;
            #endif

            #if defined(_ALPHATEST_ON)
                clip(alpha - cutoff);
            #endif

                return alpha;
            }
            half4 SampleAlbedoAlpha(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
            {
                return half4(SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv));
            }
            
            Varyings DepthOnlyVertexSGAME(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.uv = input.texcoord;//TRANSFORM_TEX(, _MainTex);
                output.positionCS = TransformObjectToHClip(input.position.xyz);
                return output;
            }

            half4 DepthOnlyFragmentSGAME(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                #ifdef _DISSOLVE_ON
                clip(-1);
                #endif
                
                Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_MainTex, sampler_MainTex)).a, float4(1,1,1,1), _Clip_Char);

                #ifdef _DISSOLVE_ON
                // float2 dissolveUV = input.positionCS.xy * (_DissolveTex_ST.xy*0.001) + _DissolveTex_ST.zw;
                // float4 dissolveCol = tex2D(_DissolveTex, dissolveUV);
                // float clipDissolve = _Clip_Char * (1 + 2 * _RampRange) - _RampRange;
                // clip(dissolveCol.r - (_Clip_Char)-1);
                #endif
                return 0;
            }
            ENDHLSL
        }
        
    }
    FallBack "Hidden/InternalErrorShader"
    //CustomEditor "CartoonCharacterShaderGUI"
    CustomEditor "Yoka.Galaxy.YokaShaderGUI.YokaShaderGUI"
}


