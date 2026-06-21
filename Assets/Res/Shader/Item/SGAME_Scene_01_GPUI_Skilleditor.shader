Shader "GPUInstancer/SGAME/SGAME_Scene_01"
{
    Properties
    {
        [Header(Main Tex)]
        [space(30)]
        _MainTex ("主贴图(A为透明通道)", 2D) = "white" {}
        [HDR]_MainColor("主贴图颜色",Color) = (1,1,1,1)
        [space(30)]
        [Header(Normal Map)]
        [ToggleUI]_NormalUseFlag("是否启用法线贴图", Float) = 1
        [NoScaleOffset]_NormalTex("法线贴图(RGB通道为法线数据)", 2D) = "blue" {}
        _NormalScale("法线强度",range(0,3)) = 1
        [space(30)]
        [Header(PBR Mask)]
        [NoScaleOffset]_MRAETex("R为金属度 G为光滑度 B为AO A预留为自发光",2D) = "white"{}
        _MetallicScale("金属度强度, 默认值为1",range(0,10)) = 1
        _RoughnessScale("光滑度强度 默认值为0.5",range(0,10)) = 0.5
        _HighLightStrength("高光强度,默认为1",range(0,10)) = 1
        _EmissionScale("自发光强度",range(0,2)) = 1
        [HDR]_EmissionColor("自发光颜色",color) = (0,0,0,1)
        [HideInInspector]_EmissionCol("Meta的自发光颜色",color) = (0,0,0,1)
        _AoScale("AO强度",range(0,3)) = 1
        
        [Header(Detail Map)]
        _DetailTex("细节贴图 RGB(基础颜色)A(光滑度)",2D) = "white" {}
        _GrassRange("调节草的范围",range(0.5,10)) = 1
        _GrassDownRang("草出现的阈值,越高则草越难出现",range(0,1)) = 0
        _DetailNormalStrength("细节法线强度",range(0,2))= 1
        
        _DetailSmoothStrength("细节贴图的光滑度强度",range(0,2)) = 1
        [HideInInspector]DetailFlag("是否使用DetailTex",int) = 0
        [HideInInspector]_ButtonBlendFlag("是否启用底部融合效果",int) = 0
        [HideInInspector]_Cutoff("控制AlphaClip的閾值",range(0,1)) = 0.2
        [HideInInspector][Enum(UnityEngine.Rendering.CullMode)]_Cullmode("Cullmode", Float) = 2
        
        [ToggleUI]_FesColorFlag("是否启用菲涅尔", Float) = 0
        [HDR]_FesColor("菲涅尔", Color) = (1.545098, 1.356863, 0.6352941, 0)
        
        [Header(Wind Zone)]
        [space(30)]
        [KeywordEnum(OFF,ON)]_IfWindZone("是否启动风场(植物用)",int) = 0
        _PlantHeight("植物的高度/*重要*/ 单位m (scaleXYZ为1时)",float) = 1
        _PlantMessy("植物运动的凌乱程度",range(0,4)) = 0.2
        _PlantBend("植物弯曲的程度",range(0,4)) = 0.5
        _WindZoneTex ("风场噪声图", 2D) = "white" {}
        _WindDir ("风的方向 xy控制",Vector) = (-1,-1,0,0)
        _WindDensity("风场噪声的密度",range(0,50)) = 25
        _WindSpd ("风的速度",range(0,3)) = 1
        _UVStrength ("UV的抖动程度(调太高可能会贴图错位)",range(0,0.3)) = 0
        
       [Header(Vertex Color Blend)]
       [space(30)]
       [ToggleUI]_IfBlendVertexColor("是否混合顶点色(正片叠底模式)", Float) = 0
       
       [Header(Dither Clip)]
        [space(30)]
       [ToggleUI]_IsOpenDither("是否剔除", Float) = 0
        
        [Header(Alpha)]
        [space(30)]
        _TotalAlpha("整体透明度", range(0,1)) = 1
        //透明度相关
        [HideInInspector]_Blendmode("BlendMode", Float) = 0   // 0--直接覆盖  1--透明度混合
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)]_SrcFactorSG("SrcFactor", int) = 1
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)]_DstFactorSG("DstFactor", int) = 0
    }
    SubShader
    {
        Tags 
        { 
            //"RenderType"="Opaque" 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry+45"
        }
        LOD 100
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float _IsOpenDither;
            float _Cullmode;
            float _Blendmode;
            //float _ClipDis;
            //float2 _Offset;
            //float _DitherMax;

          
            float4 _MainColor;
            float4 _MainTex_ST;
        
            float4 _NormalTex_ST;     
            float4 _MRAETex_ST;
            float4 _WindZoneTex_ST;
            float4 _DetailTex_ST;
            float _NormalScale;
	        float _MetallicScale;
            float _RoughnessScale;
            float _HighLightStrength;
            float _EmissionScale;
            float _AoScale;
            float _ParallaxScale;
            half4 _EmissionColor;
            half4 _EmissionCol;
            float _FesColorFlag;
            float _NormalUseFlag;
            half4 _FesColor;
            float _GrassRange;
            float _GrassDownRang;
            float _DetailNormalStrength;
            int DetailFlag;
            float _Cutoff;
            
        

            //风场相关
            float _WindDensity;
            float4 _WindDir;
            float _WindSpd;
            float _UVStrength;
            float _PlantHeight;
            float _PlantMessy;
            float _PlantBend;

            //顶点色混合
            float _IfBlendVertexColor;

            //透明度
            float _TotalAlpha;
                
            CBUFFER_END
            TEXTURE2D(_MainTex);
            TEXTURE2D(_NormalTex);
            TEXTURE2D(_MRAETex);
            TEXTURE2D(_DetailTex);
            TEXTURE2D(_WindZoneTex);
            //TEXTURE2D (_CameraDepthTexture);

            SAMPLER(sampler_DetailTex);
            SAMPLER(sampler_MainTex);
            SAMPLER(sampler_NormalTex);
            SAMPLER(sampler_MRAETex);
            SAMPLER(sampler_WindZoneTex);
            //SAMPLER(sampler_CameraDepthTexture);
        
            void WindZoneNoise(float3 positionOS, float3 positionWS, inout float4 posCS, inout float2 uvOffset)
            {
                float2 direction = normalize(_WindDir.xy);
                float2 windZoneUV = positionWS.xz */* _WindDensity*/ _WindZoneTex_ST.xy  + direction * _Time.x*_WindSpd * _WindDensity;
                
                float windNoise = SAMPLE_TEXTURE2D_LOD(_WindZoneTex, sampler_WindZoneTex, windZoneUV,0);
                float rootRamp = pow(saturate(positionOS.y/_PlantHeight), 2); //让根部变得没有那么容易抖动
                float3 posWS = (/*_SinTime.z  * */_PlantMessy * windNoise + _PlantBend  +  _SinTime.z *0.2) * float3(direction.x,0,direction.y) *  rootRamp  *0.5 + positionWS.xyz;
                posCS.xyzw = TransformWorldToHClip(posWS);
                uvOffset= float2(windNoise*2 - 1, windNoise * 2 - 1) * _UVStrength * rootRamp;
            }
        ENDHLSL
        Pass
        {
            Name "LightForward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            //混合模式关闭
            Blend [_SrcFactorSG] [_DstFactorSG]//, One Zero


            Cull [_Cullmode]//Back
            ZTest LEqual
            ZWrite On
            
            
            HLSLPROGRAM
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "./../../../ThirdParts/GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #define _MAIN_LIGHT_SHADOWS 1
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE 
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #define _SHADOWS_SOFT 1

            //使得shader代码可调试,上线时注释掉
		    //#pragma enable_d3d11_debug_symbols
            //延迟贴花要用的
            //#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3 

            //#pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma shader_feature_local _ _DETAILTEX
            #pragma shader_feature_local _ _ALPHACLIP
            #define _EMISSION 1
            //角色遮挡剔除（网点）
            //#pragma shader_feature_local _ _OCCLUSIONCLIP

            //风场 
            #pragma shader_feature_local _ _IFWINDZONE_ON

            //顶点色混合
            //#pragma shader_feature_local _ _VERTEXCOLORBLEND_ON
            //#pragma shader_feature_local _NORMALMAP
            //#pragma multi_compile_fog

           // #include "../lib/SGAME-EntityLighting.hlsl"//修改后的EntityLight文件
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../lib/Lighting_Urp_12.1.9.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
                float2 staticLightmapUV : TEXCOORD1;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 uv : TEXCOORD0;
                float4 color :TEXCOORD1;
                float4 positionCS : SV_POSITION;
                float4 normalWS : TEXCOORD2;
                float4 tangentWS : TEXCOORD3;
                float4 bioTangentWS : TEXCOORD4;
                float3 positionWS : TEXCOORD5;
                float4 fogFactorAndVertexLight : TEXCOORD6;
                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 7);
                float4 screenPos : TEXCOORD8;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float3 _PlayerPos;//全局参数放外面，主要是用来做遮挡剔除
            
            //法线强化函数
            void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
            {
                Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
            }

            //网点采样
            void Unity_Dither_float(float In, float4 ScreenPosition, out float Out)
            {
                float2 uv = ScreenPosition.xy * _ScreenParams.xy;
                float DITHER_THRESHOLDS[16] =
                {
                    1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                    13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                    4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                    16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
                };
                uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
                Out = In - DITHER_THRESHOLDS[index];
            }

            //Remap
            void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }         
            
            //菲涅尔函数
            float fresnelEffect(float3 normal, float3 viewDir, float power)
            {
                return pow((1.0 - saturate(dot(normalize(normal), normalize(viewDir)))), power);
            }

            half DirectBRDFSpecular(float roughness2MinusOne,float roughness2, float normalizationTerm, half3 normalWS, half3 lightDirectionWS, half3 viewDirectionWS)
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
            
            Varyings vert (Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.uv.xy = v.texcoord*_MainTex_ST.xy + _MainTex_ST.zw;
                #ifdef _IFWINDZONE_ON
                //风动部分
                float2 uvOffset = float2(0,0);
                WindZoneNoise(v.positionOS, o.positionWS,o.positionCS, uvOffset);
                o.uv.xy += uvOffset;
                #else
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                #endif
                
                o.color = v.color;
                o.uv.z = ComputeFogFactor(o.positionCS.z);    //uv的z值存储雾.
                o.normalWS.xyz = TransformObjectToWorldNormal(v.normalOS.xyz);
                o.tangentWS.xyz = 1.0 / length(o.normalWS.xyz )  * normalize(TransformObjectToWorldDir(v.tangentOS.xyz));
                o.tangentWS.a = v.tangentOS.a;
                float crossSign = (o.tangentWS.a > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
                float3 bitang = crossSign * cross(o.normalWS.xyz, o.tangentWS.xyz);
                o.bioTangentWS.xyz =1.0 / length(o.normalWS.xyz )  *bitang;
                half3 vertexLight = VertexLighting(o.positionWS.xyz, o.normalWS.xyz);
                half fogFactor = ComputeFogFactor( o.positionCS.z);
                o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
                o.screenPos = ComputeScreenPos(o.positionCS);
                
                //
                OUTPUT_LIGHTMAP_UV(v.staticLightmapUV, unity_LightmapST, o.staticLightmapUV);
                OUTPUT_SH(o.normalWS.xyz, o.vertexSH);
                return o;
            }



            half4 frag (Varyings i) : SV_Target
            {
               // return float4(i.screenPos.xxx,1);
                UNITY_SETUP_INSTANCE_ID(i);
                
                half3 normal;
                half3 detailNormal;
                half2 uvOffset = half2(0,0);
                half3 viewDirWS = normalize(GetCameraPositionWS() - i.positionWS);
                
                if (_NormalUseFlag) 
                { 
                    // 法线
                    half4 normalTex = SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,i.uv.xy);
                    normalTex.xyz = UnpackNormal(normalTex);
                    //return float4(normalTex.xyz,1);
                    Unity_NormalStrength_float(normalTex.xyz, _NormalScale, normal.xyz);
                    Unity_NormalStrength_float(normalTex.xyz, _DetailNormalStrength, detailNormal.xyz);

                    //法线切线空间转世界空间
                    normal = TransformTangentToWorld(normal.xyz,half3x3( i.tangentWS.xyz,i.bioTangentWS.xyz,i.normalWS.xyz));
                    detailNormal = TransformTangentToWorld(detailNormal.xyz,half3x3( i.tangentWS.xyz,i.bioTangentWS.xyz,i.normalWS.xyz));
                    normal = normalize(normal);
                    
                }
                else
                {
                    normal=i.normalWS.xyz;
                }

             
                

                //提取金属度 粗糙度 AO Emission
                half4 MRAETex = SAMPLE_TEXTURE2D(_MRAETex,sampler_MRAETex,i.uv.xy+ uvOffset);
                
                // 主贴图
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv.xy + uvOffset);
                albedo.rgb *= _MainColor.rgb;
                half alpha = clamp(albedo.a * _TotalAlpha,0,1);

                //alpha clip
                #ifdef _ALPHACLIP
                    clip(alpha-_Cutoff);
                #endif

                // 金属度 发光度 ao
                half metallic = MRAETex.r * _MetallicScale;
                half3 emission = MRAETex.a * _EmissionScale * _EmissionColor.rgb;
                half ao = 1- (1 - MRAETex.b) * _AoScale;

                half smoothness = clamp(MRAETex.g * _RoughnessScale, 0, 1);

                #ifdef _DETAILTEX

                    //混合草与顶层贴图
                    float3 upDirection = float3(0,1,0);
                    float upDotN = dot(detailNormal,upDirection);
                    upDotN = pow(upDotN,_GrassRange);
                    upDotN = smoothstep(_GrassDownRang,1,upDotN);
                    clamp(upDotN,0,1);

                    
                    //细节贴图的Tilling Offset变化
                    float2 detailUV = i.uv * _DetailTex_ST.xy +_DetailTex_ST.zw;
                    float4 detailTex = SAMPLE_TEXTURE2D(_DetailTex,sampler_DetailTex,detailUV);
                    albedo.rgb = lerp(albedo.rgb,detailTex.rgb,upDotN);
                
                #endif
                
                //AHD光照相关
                half4 BakedLmDir = 0.0;
                half3 BakedLm = 0.0;
                
                //初始化inputData
                InputData inputdata = (InputData) 0;
                inputdata.positionWS = i.positionWS;
                inputdata.normalWS = normal;
                inputdata.viewDirectionWS = viewDirWS;
                inputdata.shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                inputdata.fogCoord = i.fogFactorAndVertexLight.x;
                inputdata.vertexLighting = i.fogFactorAndVertexLight.yzw;
                //inputdata.bakedGI = SAMPLE_GI(i.lightmapUV,float3(0,1,0)/*i.vertexSH*/, normal);
                //inputdata.bakedGI = SampleSH(normal);//暫時寫死為LightMap球諧光照 待處理
                //inputdata.bakedGI = SAMPLE_GI(i.staticLightmapUV, i.vertexSH, inputdata.normalWS);
                //使用支持AHD光照的获得GI的方法(若烘焙了两张光照贴图则会返回其数值至BakedLmDir和BakedLm)
                inputdata.bakedGI = SAMPLE_GI_AHD(i.staticLightmapUV, i.vertexSH, inputdata.normalWS,BakedLmDir,BakedLm);

                float4 col = UniversalFragmentPBR_SgameAHD(inputdata
                                                            , albedo.rgb
                                                            , metallic
                                                            , float3(0,0,0)
                                                            ,  smoothness
                                                            , ao
                                                            , emission
                                                            , alpha
                                                            , BakedLmDir
                                                            , BakedLm
                                                            , _HighLightStrength);

                float fresnelVal = fresnelEffect(normal , viewDirWS, 4.55);
                col.rgb += fresnelVal * _FesColor.rgb * _FesColorFlag;

                //顶点色混合
                col.rgb = lerp(col.rgb, col.rgb * i.color.rgb, _IfBlendVertexColor);
                //alpha通道控制透明度
                col.a  = clamp(alpha, 0.01, 1);
                col.rgb = MixFog(col.rgb, i.uv.z);  // apply fog.
                /*
                //遮挡剔除（用网点挖洞）
                #ifdef _OCCLUSIONCLIP
                float3 viewSpacePosition = TransformWorldToView(i.positionWS);
                float dis=distance(viewSpacePosition.xy,_Offset);
                //float dis=distance(viewSpacePosition.xy/viewSpacePosition.z,_Offset);
                if(dis<_ClipDis && _PlayerPos.z> i.positionWS.z)
                {
                    float dither=0;
                    Unity_Remap_float(dis,float2(0,_ClipDis),float2(0,_DitherMax),dither);
                    float ditherOut=0;
                    Unity_Dither_float(dither, float4(i.screenPos.xy / i.screenPos.w, 0, 0),ditherOut);
                    col.a=ditherOut;
                    clip(col.a-0.5);
                }
                    
                #endif
                */
                if (_IsOpenDither) 
                { 
                    float dither=1;
                    float ditherOut=0;
                    Unity_Dither_float(dither, float4(i.screenPos.xy / i.screenPos.w, 0, 0),ditherOut);
                    col.a = ditherOut;
                    clip(col.a-0.5);
                }
                return col;
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cullmode]

            HLSLPROGRAM
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "./../../../ThirdParts/GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing

            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // GPU Instancing
            #pragma shader_feature_local _ _ALPHACLIP

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            //风场 
            #pragma shader_feature_local _IFWINDZONE_ON
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            float3 _LightDirection;
            float3 _LightPosition;
            
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 texcoord     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float2 uv           : TEXCOORD0;
                float4 positionCS   : SV_POSITION;
            };
            half Alpha(half albedoAlpha, half4 color, half cutoff)
            {
                half alpha = albedoAlpha * color.a;
                #if defined(_ALPHACLIP)
                    clip(alpha - cutoff);
                #endif
                return alpha;
            }
            half4 SampleAlbedoAlpha(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
            {
                return SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv);
            }
            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
            
            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif
            
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
            
            #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #endif
                return positionCS;
            }
            
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.uv = input.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                half3 positionWSS = TransformObjectToWorld(input.positionOS.xyz);
                #ifdef _IFWINDZONE_ON
                //风动部分
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float2 uvOffset = float2(0,0);
                WindZoneNoise(input.positionOS, positionWS,output.positionCS, uvOffset);
                output.uv.xy +=uvOffset;
                #else
                output.positionCS.xyzw = GetShadowPositionHClip(input);;
                #endif
                return output;
            }
            
            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_MainTex, sampler_MainTex)).a, float4(1,1,1,1), _Cutoff);
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
            Cull[_Cullmode]

            HLSLPROGRAM
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "./../../../ThirdParts/GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing

            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex DepthOnlyVertexSGAME
            #pragma fragment DepthOnlyFragmentSGAME

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHACLIP

            //--------------------------------------
            // GPU Instancing

            //风场 
            #pragma shader_feature_local _IFWINDZONE_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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
            };
            
            half Alpha(half albedoAlpha, half4 color, half cutoff)
            {
                half alpha = albedoAlpha * color.a;
                clip(albedoAlpha - cutoff);
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

                output.uv = input.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                half3 positionWSS = TransformObjectToWorld(input.position.xyz);
                #ifdef _IFWINDZONE_ON
                //风动部分
                float3 positionWS = TransformObjectToWorld(input.position.xyz);
                float2 uvOffset = float2(0,0);
                WindZoneNoise(input.position, positionWS,output.positionCS, uvOffset);
                output.uv.xy +=uvOffset;
                #else
                output.positionCS = TransformObjectToHClip(input.position.xyz);
                #endif
                return output;
            }

            half4 DepthOnlyFragmentSGAME(Varyings input) : SV_TARGET
            {
                Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_MainTex, sampler_MainTex)).a, float4(1,1,1,1),  _Cutoff);
                return 0;
            }
            ENDHLSL
        }
         // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}

            Cull Off

            HLSLPROGRAM
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "./../../../ThirdParts/GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing

            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex UniversalVertexMetaSGAME
            #pragma fragment UniversalFragmentMetaLitSGAME

            #pragma shader_feature EDITOR_VISUALIZATION
            #define _EMISSION 1
            //#pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local _ _ALPHACLIP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv0          : TEXCOORD0;
                float2 uv1          : TEXCOORD1;
                float2 uv2          : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            #ifdef EDITOR_VISUALIZATION
                float2 VizUV        : TEXCOORD1;
                float4 LightCoord   : TEXCOORD2;
            #endif
            };

            half3 SampleNormal(float2 uv, TEXTURE2D_PARAM(bumpMap, sampler_bumpMap), half scale = half(1.0))
            {
                half4 n = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv);
                return UnpackNormalScale(n, scale);
            }
            
            inline void InitializeStandardLitSurfaceDataSGAME(float2 uv, out SurfaceData outSurfaceData)
            {
                half4 albedoAlpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,uv);
                //outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _AlphaCutoff);
                outSurfaceData.alpha = 1;

                half4 MRAETex = SAMPLE_TEXTURE2D(_MRAETex,sampler_MRAETex,uv);
               // half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
                half4 specGloss = MRAETex;
                outSurfaceData.albedo = albedoAlpha.rgb;

            #if _SPECULAR_SETUP
                outSurfaceData.metallic = half(1.0);
                outSurfaceData.specular = specGloss.rgb;
            #else
                outSurfaceData.metallic = MRAETex.r;
                outSurfaceData.specular = half3(0.0, 0.0, 0.0);
            #endif

                outSurfaceData.smoothness = specGloss.g;
                outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_NormalTex, sampler_NormalTex), _NormalScale);
                outSurfaceData.occlusion = specGloss.b;
                outSurfaceData.emission = specGloss.a;
                
            #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
                half2 clearCoat = SampleClearCoat(uv);
                outSurfaceData.clearCoatMask       = clearCoat.r;
                outSurfaceData.clearCoatSmoothness = clearCoat.g;
            #else
                outSurfaceData.clearCoatMask       = half(0.0);
                outSurfaceData.clearCoatSmoothness = half(0.0);
            #endif

            #if defined(_DETAIL)
                half detailMask = SAMPLE_TEXTURE2D(_DetailMask, sampler_DetailMask, uv).a;
                float2 detailUv = uv * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
                outSurfaceData.albedo = ApplyDetailAlbedo(detailUv, outSurfaceData.albedo, detailMask);
                outSurfaceData.normalTS = ApplyDetailNormal(detailUv, outSurfaceData.normalTS, detailMask);
            #endif
            }

            Varyings UniversalVertexMetaSGAME(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = UnityMetaVertexPosition(input.positionOS.xyz, input.uv1, input.uv2);
                output.uv = input.uv0 * _MainTex_ST.xy + _MainTex_ST.zw ;
            #ifdef EDITOR_VISUALIZATION
                UnityEditorVizData(input.positionOS.xyz, input.uv0, input.uv1, input.uv2, output.VizUV, output.LightCoord);
            #endif
                return output;
            }

            half4 SampleAlbedoAlpha(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
            {
                return half4(SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv));
            }

            half4 UniversalFragmentMetaSGAME(Varyings fragIn, MetaInput metaInput)
            {
            #ifdef EDITOR_VISUALIZATION
                metaInput.VizUV = fragIn.VizUV;
                metaInput.LightCoord = fragIn.LightCoord;
            #endif

                return UnityMetaFragment(metaInput);
            }
            
            half4 UniversalFragmentMetaLitSGAME(Varyings input) : SV_Target
            {
                SurfaceData surfaceData;
                InitializeStandardLitSurfaceDataSGAME(input.uv, surfaceData);

                BRDFData brdfData;
                InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);
                
                MetaInput metaInput;
                metaInput.Albedo = brdfData.diffuse + brdfData.specular * brdfData.roughness * 0.5;
                //metaInput.Emission = surfaceData.emission;

                // 主贴图
                //half4 albedo = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv.xy);
                half EmissionTemp = surfaceData.emission * _EmissionScale;
                //_EmissionCol.rgb = _EmissionColor.rgb;
                half3 Emissions = _EmissionColor.rgb/** EmissionTemp */;
                metaInput.Emission = Emissions;
                clip(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_MainTex, sampler_MainTex)).a - _Cutoff);
               
                return UniversalFragmentMetaSGAME(input, metaInput);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
    CustomEditor "SceneShader01GUI"
}
