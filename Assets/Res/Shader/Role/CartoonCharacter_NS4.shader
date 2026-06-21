Shader "SGAME/Roles/CartoonChar_4"
{
    Properties
    {
        [HideInInspector] _Flag("区分渲染位置的参数",int) = 0
//   
        [Space(20)]

        [Header(GlobalLight)]
        _GlobalLightParm("受环境光影响的因子(0受环境光影响最少,1受环境光影响最大)",Range(0.5,2)) = 0.5
        _DirectionLightControler("主光源强度,1为使用主光源本身的强度",Range(0,2)) = 1
        _smoothness("控制全局光照的边缘参数",Range(0,1)) = 0.1
        
        [Header(Emission)]
        [HDR]_EmissionColor("自发光颜色控制",Color) = (0,0,0,1)
        
        [Header(Texture)]
         [HideInInspector]_FaceControlSpeed("面部阴影过渡速度 默认1",Range(0,2)) = 1
         [HideInInspector]_FaceControlOffset("面部阴影过渡偏移值 默认0",Range(-1,1)) = 0
         [HideInInspector]_FaceControlExp("sdf贴图缩放值",range(0,3))=1
        //_FaceControlSmooth("面部阴影贴柔和度 默认为1",Range(0,1)) = 1
        
        _MainTex ("基础色贴图", 2D) = "white" {}
        [HDR]_MainColor("主颜色",Color)= (1, 1, 1,1)
        _LightMap("卡通光照X贴图", 2D) = "white" {}
        [Space(20)]

        [Header(Alpha)]
        [ToggleUI]_IfVertexAlpha("是否启用顶点色(顶点色G通道控制)控制透明度?",float) = 0
        _VertexAlpha("顶点色控制的不透明度强度,(顶点色g通道控制)",range(0,1)) = 1
        _SpecularToAlpha("高光对不透明度的影响,值越大则高光处越不透明",range(0,1)) = 0
        _SpecularAlphaRange("半透明处的高光范围控制,值越大范围越小",range(0,1)) = 0
        
        [Header(Head Shadow)]
        [HideInInspector]_HeadShodwTex ("面部阴影贴图(若不是面部则不用管)", 2D) = "white" {}
        [Space(20)]

        [Heder(AO)]
        _AOIndencity("AO强度", Range(0.01, 0.99)) = 1
        [Space(20)]


        //[Header(Specular)]
        [HideInInspector]_HairSpecularBase("头发的基础高光度",Range(0,0.3)) = 0.2
        [HideInInspector]_HairSpecularthreshold("头发的高光的上限阈值",Range(0,1)) = 0.4
        [HideInInspector]_SpecualrIndencity("非金属部分高光范围", Range(0, 30)) = 18.9
        [HideInInspector]_SpecularStrength("非金属部分高光整体强度", Range(0,3)) = 1
        [HideInInspector]_MetalIntensity("金属部分高光强度",Range(0,10)) = 0.5
        [Space(20)]

        //[Header(MatCap)]
        [HideInInspector]_MatCapTex("金属MatCap", 2D) = "Black" {}
        [HideInInspector][HDR]_MatCapColor("MatCap颜色", Color) = (0.5,0.5,0.5,1)
        [HideInInspector]_MetallicStrength("金属颜色的明暗程度", Range(0,1)) = 1
        
        [Header(RimLight)]
        _RimWidth("边缘光宽度", Range(0, 1)) = 0.424
        _RimSmoothness("边缘光软硬程度",Range(0,1)) = 0
        [HDR]_RimColor("边缘光颜色修正", Color) = (1, 1, 1, 1)
        [Space(20)]

        
        [Header(RampTex)]
        _RampTexture("Ramp贴图集合",2D) = "white" {}
        
        [Header(Shining)]
        _ShiningColor("闪烁颜色",Color) = (1,1,1,1)
        _ColorChange("控制闪烁程度的参数(_ColorChange) 0-不影响  1-完全变为闪烁颜色",Range(0,1)) = 0
        
        
        [HideInInspector]_FrontDirHeadWS("面部的前向方向 世界空间",Vector) = (-1,0,0)
        [HideInInspector]_LeftDirHeadWS("面部的左侧方向 世界空间",Vector) = (0,0,-1)

         [Header(UVMove)]
        _NormalTex_Uspeed("NormalTex_Uspeed", Float) = 0
        _NormalTex_Vspeed("NormalTex_Vspeed", Float) = 0
        _NormalTex("NormalTex", 2D) = "white" {}

        [Header(Dissolve)]
        [KeywordEnum(On,Off)]_Dissolve("是否启动溶解效果",float) = 1
        _DissolveTex("DissolveTex(溶解贴图)",2D) = "" {}
        _Clip_Char("Clip",Range(0,1)) = 0
        [HDR]_DissolveEntityColor("溶解时颜色",Color) = (0.19,0.54,0.46,1)
        _DissolveEntityColorStrength("值越高则越偏向溶解时颜色",Range(0,1)) = 0
        [HDR]_DissolveLightingColor("溶解时整体亮色(混合模式为相加,不用时设置为纯黑)",Color) = (0,0,0,1)
        _DissolveDarkColor("溶解时整体暗色(混合模式为正片叠底,不用时设置为纯白)",Color) = (1,1,1,1)
        [HDR]_RampColor("溶解时边缘亮色",Color) = (0.5,1.9,3.7,0)
        _RampRange("高亮范围",Range(0,1)) = 0.1
        
        [Header(Alhpa)]
        _Alpha("角色Alpha值",Range(0,1)) = 1
        
        [Header(Stencil)]
        _OutlineStencilComp("模板值",int) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("模板缓冲区Comp的操作",int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilOp("模板缓冲区Pass的操作",int) = 0
        
        [Header(Zwrite)]
        [Enum(Off, 0, On, 1)]_ZwriteOp_Char("深度写入开关",int) = 1
        
        [Header(RenderFrontBack)]
        [Enum(UnityEngine.Rendering.CullMode)]_CullOp_Char("剔除面,Off-不剔除,Back-剔除背面,Front-剔除正面",float) = 0
        
        
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
            float _FaceControlSpeed;
            float _FaceControlOffset;
            float _FaceControlExp;
            float _FaceControlSmooth;
            int _Flag;
            float _GlobalLightParm;
            float _smoothness;
            float4 _EmissionColor;
            float _DirectionLightControler;
            float _AOIndencity;
            float _SpecualrIndencity;
            float _SpecularStrength;
            float _HairSpecularBase;
            float _HairSpecularthreshold;
        
            float4 _MatCapColor;
            float _MetallicStrength;
            float _RimWidth;
            float _RimSmoothness;
            half4 _RimColor;

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
            float _RampRange;
            float4 _RampColor;
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

            float _Alpha;
            //float _OutlineAlpha;
            int _StencilComp;
            int _StencilOp;
            int _OutLineStencilComp;

            float _CullOp_Char;
        
        CBUFFER_END

       
        
        TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
        TEXTURE2D(_LightMap);   SAMPLER(sampler_LightMap);
        TEXTURE2D(_RampTexture);   SAMPLER(sampler_RampTexture);
        ///头部头发的HairShadowMask
        //TEXTURE2D(_HairShadowMaskTexture);  SAMPLER(sampler_HairShadowMaskTexture);
        TEXTURE2D(_FrozenTex);    SAMPLER(sampler_FrozenTex);
        ///头部的SDF
        TEXTURE2D(_HeadShodwTex);  SAMPLER(sampler_HeadShodwTex);
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

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS /*_MAIN_LIGHT_SHADOWS_CASCADE*/ // _MAIN_LIGHT_SHADOWS_SCREEN
            //关闭多光源
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _HEAD _HAIR       //标记该shader是用来渲染头部,头发还是身体 (默认是身体)
            #pragma multi_compile _ _DISSOLVE_ON

            // #pragma shader_feature _ _CUSTOM_SHADOW_ON
            // #pragma shader_feature _ _MAINLIGHT_CUSTOM_SHADOW_ON
            // #pragma multi_compile _ _ESM
            


            //角色额外光照
            #pragma multi_compile _ _GLOBAL_CHAR_DARKENVIROMENT
            //#pragma multi_compile _ _UVMOVE_COMMON
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Macros.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
                float2 texcoord2: TEXCOORD1;
                float3 normalOS : NORMAL;
                float4 tangent : TANGENT;
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
                float4 positionOS :TEXCOORD5;
                float4 vertexSH : TEXCOORD6;
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
                o.positionOS = v.positionOS;

                #ifdef _DISSOLVE_ON
                float crossSign = (v.tangent.a > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
                v.tangent.xyz = 1.0 / length(v.tangent.xyz ) * normalize(v.tangent.xyz); 
                float3 BioTangent =1.0 / length(v.normalOS.xyz )* cross(v.normalOS.xyz,v.tangent.xyz)* crossSign  /** v.tangentOS.w*/;
                float2 normalXY = v.texcoord2.xy;
                float3 normalZ = cross(float3(0,v.texcoord2.y,0),float3(v.texcoord2.x,0,0));
                float3 BakeNormal = normalize(UnpackNormal(float4(normalXY.x,normalXY.y,1,1)));
                float3x3 tangentTransform = float3x3(v.tangent.xyz, BioTangent.xyz, v.normalOS.xyz);
                float3 SmoothNormalOS = float3(v.tangent.x * BakeNormal.x+ BioTangent.x * BakeNormal.y + v.normalOS.x * BakeNormal.z,
                                                v.tangent.y * BakeNormal.x + BioTangent.y  * BakeNormal.y + v.normalOS.y * BakeNormal.z,
                                                v.tangent.z * BakeNormal.x + BioTangent.z  * BakeNormal.y + v.normalOS.z * BakeNormal.z);
                //在开启溶解的时候把PositionOS用来传平滑法线,为后续溶解时获得平滑法线
                o.positionOS.xyz = TransformObjectToWorldDir(SmoothNormalOS.xyz);
                OUTPUT_SH(o.normalWS.xyz, o.vertexSH);
                #endif
                
                
                return o;
            }

            
            half4 frag (Varyings i) : SV_Target
            {
                Light mainLight = GetMainLight(i.shadowCoord, i.positionWS);
                mainLight.color = mainLight.color * _DirectionLightControler;
                float3 N = normalize(i.normalWS);
                float3 L = normalize(mainLight.direction);
                float3 V = normalize(_WorldSpaceCameraPos - i.positionWS);
                float3 H = normalize(L + V);
                half4 mainLightColor = half4(mainLight.color, 1);

                #ifdef _GLOBAL_CHAR_DARKENVIROMENT
                    //
                    
                    mainLightColor.rgb = mainLightColor.rgb * _SGameCharMainLightMultipler;
                   // return float4(_SGameCharMainLightMultipler,_SGameCharMainLightMultipler,_SGameCharMainLightMultipler,1);
                #endif
                
                half NoL = dot(N, L);
                // 贴图采样
                half4 diffuse = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
                diffuse.rgb *= _MainColor.rgb;
                half3 diffuseHSV = RgbToHsv(diffuse.rgb);
                half4 lightMapColor = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, i.uv.xy);
                //测试代码
               // return half4(diffuse.rgb,1);
                
                half Emission = diffuse.a;            //自发光区域
                half Glossiness = lightMapColor.r;    // Glossiness贴图
                half ao = lightMapColor.b;          // AO
                ao = clamp(ao + 1 - _AOIndencity,0,1);    // AO强度调节
                half highLight = lightMapColor.g;   // 高光
                
                //初始化BRDF相关数据
                BRDFData brdfData;
                /////////////////////////////
                half brdfMetallic = Glossiness * 0.5 * (1-_MetallicStrength);
                InitializeBRDFData(diffuse.xyz, brdfMetallic, 0.7f,_smoothness, diffuse.a, brdfData);
                diffuse.rgb = brdfData.diffuse;
                //环境光
                half3 bakedGI = SAMPLE_GI(staticLmName, i.vertexSH.xyz, i.normalWS.xyz);
                MixRealtimeAndBakedGI(mainLight, i.normalWS, bakedGI, half4(0,0,0,0));
                _GlobalLightParm = max(_GlobalLightParm,0.5f);
                half3 globalIllumination = GlobalIllumination(brdfData, bakedGI, 1, i.normalWS,V) *_GlobalLightParm;
                #ifdef _GLOBAL_CHAR_DARKENVIROMENT
                globalIllumination *= _SGameCharGIMultipler;
                #endif
                
                #ifdef _HEAD //如果该shader为头部
                    //sdf面部贴图统一 则写死
                    _FaceControlSpeed = 0.8;
                    _FaceControlOffset = 0.45;
                    //half4 hairMask = SAMPLE_TEXTURE2D(_HairShadowMaskTexture,sampler_HairShadowMaskTexture,i.positionSS.xy/i.positionSS.w);
                    //SDF控制面部阴影
                    float3 forward = _FrontDirHeadWS.xyz;
                    float3 left = _LeftDirHeadWS.xyz;
                    float3 lightDirH = normalize(float3(L.x, 0, L.z));
                    float filpU = sign(dot(lightDirH, left));
                    float lightAtten = saturate(dot(lightDirH, forward)*_FaceControlSpeed +  _FaceControlOffset);
                    float3 shaodwRamp = SAMPLE_TEXTURE2D(_HeadShodwTex, sampler_HeadShodwTex,i.uv.xy  * float2(filpU, 1)).rgb;
                //float faceShadow = step( 1- shaodwRamp.g,lightAtten);
                    float shadowRampG = pow(abs(1- shaodwRamp.g),_FaceControlExp);
                    float faceShadow = smoothstep(shadowRampG,1- shaodwRamp.g+0.02 /** _FaceControlSmooth*/,lightAtten);
                    half shadeLight = ao;
                    //shadeLight *= hairMask.g;
                    shadeLight *= faceShadow;
                #else//如果不是渲染头部
                    //新暗部方案
                    float selfShade = clamp(NoL,0,1);
                    selfShade = clamp(smoothstep(-0.1,1,NoL)+0.1,0,1);
                    half shadeLight = clamp(1 -selfShade * (1- mainLight.distanceAttenuation*mainLight.shadowAttenuation),0,1)  * selfShade * ao;
             ;
                #endif
                

                half4 materialIndex = lightMapColor.a;
                half2 rampUV = half2(clamp(0,1,shadeLight), saturate(1 - materialIndex.a));
                rampUV = clamp(0.02,0.98,rampUV);
                float4 rampTex = SAMPLE_TEXTURE2D(_RampTexture, sampler_RampTexture, rampUV);
                // half3 rampColor = lerp(rampTex.rgb, float3(0.5f,0.5f,0.5f), rampTex.a);
                // rampColor = rampTex.rgb;
                ///正片叠底混合
                half3 diffuseTerm = rampTex.rgb * diffuse.rgb;
    
                half4 diffuseAndShadow = float4(1,1,1,1);
                diffuseAndShadow.rgba = float4(diffuseTerm,1);
                

                // 高光
                half NoV = dot(N, V);
                half NoVPow = pow(abs(NoV), 10) * _SpecualrIndencity;
                // 高光乘以NoL可以在刚进入阴影时也有微弱高光
                half4 specularNoV = NoVPow;

                // 灯光高光BlinPhong
                half NoH = pow(abs(dot(N, H)), 100);
                half4 specularNoH = NoH;

                // 混合高光
                half4 specular;
                specular = lerp(specularNoV, specularNoH, 0.5);
                specular = saturate(specular /* * diffuse*/ * 3 * highLight * ao) * float4(mainLight.color,1) * diffuseHSV.b;  //混合阴影、漫反射颜色、高光MASK、ao
                //去除金属部分高光 单独计算
                specular = clamp(specular,0,1) * _SpecularStrength * (1-Glossiness) * float4(mainLight.color,1);
                
                //return float4(specular.rgb,1);
                float temp3 = highLight * (1-Glossiness);
                #ifdef _HAIR
                    specular.rgb += _HairSpecularBase * mainLight.color * highLight;
                    specular.rgb *= clamp(NoV,0,_HairSpecularthreshold);
                    //return float4(specular.rgb,1);
                #endif

                //金属部分MatCap高光
                // 采样MatCap贴图
                #ifndef _HEAD
                    #ifndef  _HAIR
                        float3 V1 = V;
                        float3 temp = mul(UNITY_MATRIX_V,float4(normalize(i.normalWS.xyz + float3(-mainLight.direction.x,0,0)),0)).xyz;
                        half4 MatCAP = SAMPLE_TEXTURE2D(_MatCapTex, sampler_MatCapTex,( ( (temp).xy / ( 2.828427 * sqrt( ( (temp).z + 1.0 ) ) ) ) + 0.5 ));
                        MatCAP = SAMPLE_TEXTURE2D(_MatCapTex, sampler_MatCapTex,(temp.xy +float2(1,1))* 0.5 );
                        float nov2 = pow(saturate(NoV),3);
                        MatCAP *= pow(shadeLight,0.5);
                        MatCAP.rgb *= Glossiness * nov2  * mainLight.color ;
                        specular.rgb += MatCAP.rgb *_MatCapColor.rgb*highLight  ;
                    #endif
                #endif
                //受顶点影响的半透明处高光 单独计算
                float4 VertexAlphaSpeuclar =  specular.rgba * smoothstep(_SpecularAlphaRange,_SpecularAlphaRange + 0.01,(specular.r + specular.g + specular.b)*0.3333);
                ///////////////////////////////////////////////////
                // 边缘光
                half rim =   step(1.01 - _RimWidth, clamp(1-NoV,0,1));
                rim = smoothstep(1.01-_RimWidth,1.01-_RimWidth + _RimSmoothness, clamp((1-NoV),0,1));
                half4 rimColor = rim  * ao * _RimColor /** shadeLight*/;
                half4 rimColor_mainLight =  rimColor;
                
                half4 finalSpecular = specular * (1 - i.vertexColor.g * _IfVertexAlpha)  + VertexAlphaSpeuclar * i.vertexColor.g * _IfVertexAlpha;
                
                half4 finalCol = (diffuseAndShadow  + finalSpecular ) * mainLight.distanceAttenuation * mainLightColor;
                finalCol = lerp(finalCol,rimColor_mainLight * _RimColor.a + finalCol * (1 - _RimColor.a) ,rim);
                
                // 额外灯
               
                #ifdef _GLOBAL_CHAR_DARKENVIROMENT
                    uint meshRenderingLayers = GetMeshRenderingLightLayer();
                   #ifndef _HEAD
                    // 像素灯的数量
                    uint pixelLightCount = GetAdditionalLightsCount();
                    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                    {
                        //return float4(1,1,1,1)
                        // 获得灯光颜色、方向、衰减、阴影衰减
                        Light addLight = GetAdditionalLight(lightIndex, i.positionWS);
                        if (IsMatchingLightLayer(addLight.layerMask, meshRenderingLayers))
                        {
                            //额外光只支持点光源 保证卡通渲染的效果
                            //计算平面化的法线
                            //half3 normalPlaneWS = normalize(mul(UNITY_MATRIX_I_V,float3(0,0,1)));
                    
                            //half NoLadd = saturate(dot(addLight.direction,normalPlaneWS));
                            half NoLadd = saturate(dot(addLight.direction,N));
                            NoLadd = step(0.5, NoLadd);
                            //half NoLadd = 0.5;
                            half3 diffuseAdd = NoLadd * addLight.distanceAttenuation;
                            //为了防止再高光和边缘光处过曝 在高光和边缘光处适当减弱额外光源的影响
                            half minusSpecularRimLight = saturate(1 - saturate(specular + rim)).x;
                            diffuseAdd = diffuseAdd * saturate(ao+0.3f) * minusSpecularRimLight * diffuse.rgb * addLight.color;
                            finalCol.rgb += diffuseAdd;
                        }
                    }
                  #else //如果该shader为头部
                    //SDF控制面部阴影
                    // 获得灯光颜色、方向、衰减、阴影衰减
                     uint pixelLightCount = GetAdditionalLightsCount();
                    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                    {
                        Light addLight = GetAdditionalLight(lightIndex, i.positionWS);
                        if (IsMatchingLightLayer(addLight.layerMask, meshRenderingLayers))
                        {
                            float3 forward2 = _FrontDirHeadWS.xyz;
                            float3 left2 = _LeftDirHeadWS.xyz;
                            float3 lightDirH2 = normalize(float3(addLight.direction.x, 0, addLight.direction.z));
                            float filpU2 = sign(dot(lightDirH2, left2));
                            float lightAtten2 = saturate(dot(lightDirH2, forward2)*_FaceControlSpeed +  _FaceControlOffset);
                            float3 shaodwRamp2 = SAMPLE_TEXTURE2D(_HeadShodwTex, sampler_HeadShodwTex,i.uv.xy  * float2(filpU2, 1)).rgb;
                 
                            float shadowRampG2 = pow(abs(1- shaodwRamp2.g),_FaceControlExp);
                            float faceShadow2 = smoothstep(shadowRampG2,1- shaodwRamp2.g+0.02 /** _FaceControlSmooth*/,lightAtten2);
                            half shadeLight2 = ao;
                            //shadeLight *= hairMask.g;
                            shadeLight2 *= faceShadow2;
                            half minusSpecularRimLight = saturate(1 - saturate(specular.x + rim.x) );
                            half3 diffuseAdd = saturate(ao+0.3f) * minusSpecularRimLight * diffuse.rgb * addLight.color * 0.7f;
                            finalCol.rgb += diffuseAdd * shadeLight2;
                        }
                    }
                    
                   #endif
                #endif
                
                // 加入自发光
                finalCol.rgb += Emission * _EmissionColor.rgb;
                //加入环境光
                finalCol.rgb = finalCol.rgb + 1*globalIllumination;

                
                //处理闪烁效果
                half tempNoV = pow(1- NoV + 0.1,2);
                half3 shingColor = smoothstep(0.2,0.4,tempNoV) * tempNoV * _ShiningColor;
                finalCol.rgb = lerp(finalCol.rgb,finalCol.rgb + shingColor, _ColorChange);

                

#ifdef _UVMOVE_COMMON
                float2 uvMoveSpeed = float2(_NormalTex_Uspeed, _NormalTex_Vspeed);
                float2 movedUV = _Time.y * uvMoveSpeed + i.uv;
                float4 normalColor = SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, movedUV);
                finalCol += normalColor;
#endif

                
#ifdef _DISSOLVE_ON
                //return float4(i.positionOS.yy,1,1);
                float2 dissolveUV = i.positionCS.xy * (_DissolveTex_ST.xy*0.001) + _DissolveTex_ST.zw;
                float4 dissolveCol = tex2D(_DissolveTex, dissolveUV);
                float clipDissolve = _Clip_Char * (1 + 2 * _RampRange) - _RampRange;
                float NoVSmoothness = dot(i.positionOS.xyz, V);
                float dissolveDir = clamp((dissolveCol.r + 0.2)* ((NoVSmoothness *0.5+0.5) *2),0,0.99);
                float ColorRange = step(clipDissolve,dissolveDir) * (1 - step(clipDissolve + _RampRange,dissolveDir));
                finalCol.rgb = (finalCol.rgb + _DissolveLightingColor.rgb)*_DissolveDarkColor.rgb;
                finalCol.rgb = lerp(finalCol.rgb,_DissolveEntityColor.rgb, _DissolveEntityColorStrength);
                finalCol.rgb = lerp(finalCol.rgb,_RampColor.rgb,ColorRange);
                clip(dissolveDir - (clipDissolve));
               // return float4(dissolveDir,dissolveDir,dissolveDir,1);
#endif
                //混合雾
                finalCol.rgb = MixFog(finalCol.rgb, i.uv.z);
                //混合顶点色的透明度
                finalCol.a = lerp(1,_VertexAlpha  + (VertexAlphaSpeuclar.r+VertexAlphaSpeuclar.g+VertexAlphaSpeuclar.b) * 0.3333 * _SpecularToAlpha,i.vertexColor.g * _IfVertexAlpha);
                //设置角色Alpha值(如有需要)
                finalCol.a *=_Alpha;
                return finalCol;
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
                float2 dissolveUV = input.positionCS.xy * (_DissolveTex_ST.xy*0.001) + _DissolveTex_ST.zw;
                float4 dissolveCol = tex2D(_DissolveTex, dissolveUV);
                float clipDissolve = _Clip_Char * (1 + 2 * _RampRange) - _RampRange;
                clip(dissolveCol.r - (_Clip_Char)-1);
                #endif
                return 0;
            }
            ENDHLSL
        }
        
    }
    FallBack "Hidden/InternalErrorShader"
    CustomEditor "CartoonCharacterShaderGUI"
}
