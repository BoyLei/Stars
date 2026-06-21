Shader "SGAME/Roles/Cartoon_NonoBall"
{
    Properties
    {
        _BaseMap ("MainTex", 2D) = "white" {}
        _MaskTex ("MaskTex", 2D) = "white" {}
        _MatCapTex("MatCapTex", 2D) = "white" {}
        
        [Header(Color Trans Part)]
        _TransColor("底部渐变颜色",Color) = (1,1,1,1)
        _TransRange("控制渐变范围的参数",Range(-0.2,1)) = 0.3
        
        [Header(Rim Color Part)]
        _RimColor("环状轮廓基础色",Color) = (1,1,1,1)
        _RimColorRange("环状轮廓范围",Range(0.5,5)) = 3.0
        _RimColorSmoothness("环状轮廓柔和度",Range(0.1,5)) = 3.0
        
        [Header(Rim Light Part)]
        _RimLightColor("环状光颜色",Color) = (1,1,1,1)
        _RimLightStrength("环状光强度", Range(0,2)) =  1.0
        _RimLightRange("环状光轮廓范围",Range(0.5,5)) = 3.0
        _RimLightSmoothness("环状光轮廓柔和度", Range(0.1,10)) = 3.0
        
        [Header(Trans Color)]
        _TransmittingColor("透光颜色",Color) = (1,1,1,1)
        
        [Header(Ball High Light)]
        _SpecularColor("果冻部分高光颜色倾向",Color) = (1,1,1,1)
        _SpecularStrength("果冻部分高光强度",Range(0,2)) = 1.0
        
        [Header(Non Ball High Light)]
        _NBspecularColor("非果冻部分高光颜色倾向",Color) = (1,1,1,1)
        _WingspecularStrength("翅膀部分镜面反射强度",Range(0,1)) = 0.25
        
        [Header(Shadow)]
        _BallShadowColors("果冻部分暗部颜色倾向",Color) = (0,0,0,0)
        _NonBallShadowColors("非果冻体部分暗部颜色倾向",Color) = (0,0,0,0)
        
        [Header(Reflection)]
        _ReflectionColor1("果冻反射部分颜色倾向1",Color) = (1,1,1,1)
        _ReflectionColor2("果冻反射部分颜色倾向2",Color) = (1,1,1,1)
        _ReflectionStrength("果冻反射部分强度",Range(0,2)) = 1.0
        
        
        [Header(Outline)]
        _OutLineWidth("OutLineWidth(外描边粗细)", Range(0, 2)) = 0.8
        _OutlineColorControl("OutlineColorControl(外描边明暗控制)", Range(0, 1)) = 0.1
        _CameraOutlineRange("CameraOutlineRange(相机远近描边调节，默认0.5可不用调)", Range(0, 1)) = 0.5
        
        [Header(Global Setting)]
       // _smoothness("整体光滑度",Range(0,1)) = 0
        _metallic("受环境光影响程度",Range(0,1)) = 0
        
        [Header(Shining)]
        _ShiningColor("闪烁颜色",Color) = (1,1,1,1)
        _ColorChange("控制闪烁程度的参数(_ColorChange) 0-不影响  1-完全变为闪烁颜色",Range(0,1)) = 0
        
        
    }
    SubShader
    {
        LOD 100
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        CBUFFER_START(UnityPerMaterial)
            sampler2D _BaseMap;
            SAMPLER(sampler_BaseMap);
            float4 _BaseMap_ST;
            sampler2D _MaskTex;
            sampler2D _MatCapTex;

            float4 _TransColor;
            float _TransRange;
            
            float _smoothness;
            float _specular;
            float _metallic;

            float4 _RimColor;
            float _RimColorRange;
            float _RimColorSmoothness;
            
            float4 _RimLightColor;
            float _RimLightStrength;
            float _RimLightRange;
            float _RimLightSmoothness;

            float4 _SpecularColor;
            float _SpecularStrength;
            float4 _NBspecularColor;
            float _WingspecularStrength;

            float4 _TransmittingColor;
            
            float4 _BallShadowColors;
            float4 _NonBallShadowColors;
            
            float4 _ReflectionColor1;
            float4 _ReflectionColor2;
            float _ReflectionStrength;

            float4 _ShiningColor;
            float _ColorChange;

            // float _LightRange;
            // float _Hardness;
            float _OutLineWidth;
            float _CameraOutlineRange;
            half _OutlineColorControl;

            //TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
            
            half4 _FrozenColor;
            half _FrozenRimStrength;
            half _FronzeConverthresold;
            half _FrozenRimRange;
            half _FrozenConver;
            CBUFFER_END
        ENDHLSL
        Pass
        {
            Name"lightForward"
            Tags
            {
                "LightMode" = "UniversalForward"
                "Queue" = "Transparent+1"
            }
            Blend SrcAlpha OneMinusSrcAlpha, One Zero
            Cull Back
            ZTest LEqual
            ZWrite On
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            #pragma shader_feature _ _CUSTOM_SHADOW_ON
            #pragma shader_feature _ _MAINLIGHT_CUSTOM_SHADOW_ON
            #pragma multi_compile _ _ESM
            

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 lightmapUV : TEXCOORD1;
            };

            struct Varyings
            {
                float4 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float4 normalVS : TEXCOORD4;
                float3 positionWS : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                float4 DownDirOS : TEXCOORD5;
            };

            // CBUFFER_START(UnityPerMaterial)
            // sampler2D _BaseMap;
            // float4 _BaseMap_ST;
            // sampler2D _MaskTex;
            // sampler2D _MatCapTex;
            //
            // float4 _TransColor;
            // float _TransRange;
            //
            // float _smoothness;
            // float _specular;
            // float _metallic;
            //
            // float4 _RimColor;
            // float _RimColorRange;
            // float _RimColorSmoothness;
            //
            // float4 _RimLightColor;
            // float _RimLightStrength;
            // float _RimLightRange;
            // float _RimLightSmoothness;
            //
            // float4 _SpecularColor;
            // float _SpecularStrength;
            // float4 _NBspecularColor;
            // float _WingspecularStrength;
            //
            // float4 _TransmittingColor;
            //
            // float4 _BallShadowColors;
            // float4 _NonBallShadowColors;
            //
            // float4 _ReflectionColor1;
            // float4 _ReflectionColor2;
            // float _ReflectionStrength;
            //
            // float4 _ShiningColor;
            // float _ColorChange;
            //
            // // float _LightRange;
            // // float _Hardness;
            // float _OutLineWidth;
            // float _CameraOutlineRange;
            // half _OutlineColorControl;
            //
            // //TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
            //
            // half4 _FrozenColor;
            // half _FrozenRimStrength;
            // half _FronzeConverthresold;
            // half _FrozenRimRange;
            // half _FrozenConver;
            // CBUFFER_END

            //////////////////////////////////////////////////
            ///修改自URP Lighting的Specular项计算公式
            half3 DirectBDRF_Specular(BRDFData brdfData, half3 normalWS, half3 lightDirectionWS, half3 viewDirectionWS)
            {
                float3 halfDir = SafeNormalize(float3(lightDirectionWS) + float3(viewDirectionWS));

                float NoH = saturate(dot(normalWS, halfDir));
                half LoH = saturate(dot(lightDirectionWS, halfDir));
                
                float d = NoH * NoH * brdfData.roughness2MinusOne + 1.00001f;

                half LoH2 = LoH * LoH;
                half specularTerm = brdfData.roughness2 / ((d * d) * max(0.1h, LoH2) * brdfData.normalizationTerm);

                // On platforms where half actually means something, the denominator has a risk of overflow
                // clamp below was added specifically to "fix" that, but dx compiler (we convert bytecode to metal/gles)
                // sees that specularTerm have only non-negative terms, so it skips max(0,..) in clamp (leaving only min(100,...))
            #if defined (SHADER_API_MOBILE) || defined (SHADER_API_SWITCH)
                specularTerm = specularTerm - HALF_MIN;
                specularTerm = clamp(specularTerm, 0.0, 100.0); // Prevent FP16 overflow on mobiles
            #endif

                half3 color = specularTerm * brdfData.specular;
                return color;
            }
            //////////////////////////////////////////////////
            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionWS = TransformObjectToWorld(v.positionOS);
                o.normalWS = normalize(TransformObjectToWorldDir(v.normalOS));
                o.normalVS = mul(UNITY_MATRIX_V,o.normalWS);
                
                o.uv.xy = TRANSFORM_TEX(v.texcoord, _BaseMap);
                o.uv.z = ComputeFogFactor(o.positionCS.z);    //uv的z值存储雾
                o.shadowCoord = TransformWorldToShadowCoord(o.positionWS);

                // // // //将(0,1,0)转换回世界空间坐标系
                // half3 UpVStoWS = mul(UNITY_MATRIX_I_V,half3(0,1,0));
                // half3 V = normalize(GetCameraPositionWS() - o.positionWS);
                // //half3 V = normalize(GetCameraPositionWS() - TransformObjectToWorld(float4(0,0,0,1)));
                // half3 VSvector = normalize(cross(V,UpVStoWS));
                // UpVStoWS = normalize(cross( VSvector,V));
                // //V =  normalize(cross(UpVStoWS,VSvector));
                // //在新的摄像机空间下的Normal值
                // o.normalVS.xyz = half3(VSvector.x * o.normalWS.x + UpVStoWS.x * o.normalWS.y  + V.x * o.normalWS.z,
                //                    VSvector.y * o.normalWS.x + UpVStoWS.y * o.normalWS.y  + V.y * o.normalWS.z,
                //                    VSvector.z * o.normalWS.x + UpVStoWS.z * o.normalWS.y  + V.z * o.normalWS.z);
                
                

                //将物体空间下永远朝下的向量转移到世界空间下
                o.DownDirOS.xyz = TransformObjectToWorldDir(float3(0,0,-1));
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                // sample the texture
                half4 col = tex2D(_BaseMap, i.uv);
              
                half4 mask = tex2D(_MaskTex,i.uv);
                //常用数据计算
                float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                float3 V = normalize(GetCameraPositionWS() -  i.positionWS);
                half NoV = dot(i.normalWS,V);
                half NoL = dot(i.normalWS,mainLight.direction);
                _smoothness = mask.b * 1.6;
                //return float4(_smoothness,_smoothness,_smoothness,1);
                // _metallic = mask.b;

                //初始化BRDF相关数据
                BRDFData brdfData;
                InitializeBRDFData(col, _metallic, _specular,_smoothness, col.a, brdfData);
                col.rgb = brdfData.diffuse;
                //return half4(col.rgb,1);

                // 边缘基础色渐变计算
                half rimColor = 1 - pow(clamp(NoV *_RimColorRange,0,1),5);
                rimColor = rimColor * clamp(1 - mask.r * 0.1,0,1) * mask.g;
                rimColor = pow(rimColor,_RimColorSmoothness) * _RimColor.a;
                col.rgb = lerp(col.rgb,_RimColor,rimColor);
                // /return col;

                //球体内部异色渐变
                half transMask = saturate(dot(i.DownDirOS,i.normalWS)*0.5 + 0.5);
                transMask = smoothstep(saturate(_TransRange), saturate(_TransRange + 0.2), transMask) * mask.g * (1 - rimColor);
                col.rgb = lerp(col.rgb, _TransColor, transMask);
                //return  float4(col.rgb,1);

                //球体内部颜色渐变控制
                //half insideBallCol
                
                // 边缘光计算
                half rimLight =  1 - pow(clamp(NoV *_RimLightRange,0,1),5);
                rimLight = rimLight * clamp(1 - mask.r * 2,0,1);
                rimLight = pow(rimLight,_RimLightSmoothness);
                half3 rimLightCol = rimLight * lerp(_RimLightColor,_TransColor,transMask) * _RimLightStrength * mainLight.color * mask.g;
                rimLightCol = rimLightCol * (NoL * 0.5 +0.5);
                
                
                // 采样MatCap贴图
                //test
                float3 V1 = normalize(i.positionWS - GetCameraPositionWS());
                float3 temp = mul(UNITY_MATRIX_V,float4(reflect(V1, i.normalWS),0)).xyz;
                half4 MatCAP = tex2D(_MatCapTex, ( ( (temp).xy / ( 2.828427 * sqrt( ( (temp).z + 1.0 ) ) ) ) + 0.5 ));
                //return MatCAP;
                //return float4(i.normalVS.xy,1,1);
                half2 normalUV = clamp(0,1,i.normalVS.xy * 0.499 + 0.5);
                half4 MatCap = tex2D(_MatCapTex,normalUV);
                MatCap = MatCAP;
                //return MatCap;

                //环境光
                half3 bakedGI = half3(0,0,0);  //暂时不考虑烘焙光
                
                MixRealtimeAndBakedGI(mainLight, i.normalWS, bakedGI, half4(0,0,0,0));
                half3 globalIllumination = GlobalIllumination(brdfData, bakedGI, 1, i.normalWS, V);
                
                
                // 球体高光部分
                half3 specular = MatCap.g * _SpecularColor.rgb * mainLight.color *_SpecularStrength * mask.g;
                // 非球体高光部分
                //LightingSpecular(attenuatedLightColor, light.direction, inputData.normalWS, inputData.viewDirectionWS, specularGloss, smoothness);
                half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
                half3 nonBallSpecular = _NBspecularColor * (1-mask.g)* DirectBDRF_Specular(brdfData, i.normalWS,mainLight.direction, V);
                //翅膀高光部分
                nonBallSpecular += MatCap.g * mask.a *  mainLight.color * _WingspecularStrength * _NBspecularColor;
                specular = specular + nonBallSpecular ;

                //高光部分的衰减
                specular = specular * (NoL * 0.5 +0.5);
                
                //漫反射与投影
                NoL = clamp(NoL,0,1);
                half Radiance = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                float3 diffuseTerm = NoL * Radiance;
                diffuseTerm = lerp(_BallShadowColors,float3(1,1,1),diffuseTerm.r) * mask.g + lerp(_NonBallShadowColors,float3(1,1,1),diffuseTerm.r) * (1-mask.g);
                diffuseTerm = diffuseTerm * col * mainLight.color;
                

                //次表面散射部分
                half3 SSStemp = MatCap.r * mask.g;
                SSStemp = pow(SSStemp,2);
                half3 SSSterm = lerp(lerp(_ReflectionColor1, _TransColor,transMask), _ReflectionColor2, SSStemp) * _ReflectionStrength * mask.g * (1- saturate(mask.r * 0.5)) ;
                
                //厚度图控制的透光(翅膀部分)
                half thickness = pow((col.a)* ( mask.a),1) ;
                half3 transTerm = pow(saturate(NoV),10) * saturate(dot(i.normalWS, - mainLight.direction)) * mainLight.color * thickness * col.rgb ;
                transTerm = saturate(transTerm*3);
                //transTerm = MatCap.g * mask.a;
               // return float4(transTerm,1);
                
                //混合雾
                col.rgb = MixFog(col.rgb, i.uv.z);
                //剔除alpha
                clip(col.a - 0.02);
                
                col.rgb = diffuseTerm + globalIllumination + rimLightCol + specular + SSSterm + transTerm;
                //翅膀的透光
                col.rgb = lerp(col.rgb,_TransmittingColor, transTerm);
                col.a = saturate((1 - rimColor*1.4 + specular * 80 + SSStemp * 0.5) + 0.4);

                //处理闪烁效果
                col.rgb = lerp(col.rgb,_ShiningColor.rgb, _ColorChange);
                //return float4(col.a,col.a,col.a,1);
                //col.rgb = i.normalWS;
               // col.a = col.a * rimColor.r;
                return col;
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "OutLine"
            Tags { 
                    "LightMode" = "SRPDefaultUnlit"
                    "Queue" = "Transparent+1" 
                }
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ _ElemetalVFX 
            
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 texcoord : TEXCOORD0;
                float3 normalOS : NORMAL;
                half4 vertColor : COLOR;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float4 positionWS :TEXCOORD1;
                float4 normalWS   :TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                
            };

            // CBUFFER_START(UnityPerMaterial)
            // // float _LightRange;
            // // float _Hardness;
            // float _OutLineWidth;
            // float _CameraOutlineRange;
            // half _OutlineColorControl;
            //
            // //TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
            //
            // half4 _FrozenColor;
            // half _FrozenRimStrength;
            // half _FronzeConverthresold;
            // half _FrozenRimRange;
            // half _FrozenConver;
            // CBUFFER_END
            
            

            Varyings vert (Attributes v)
            {
                Varyings o = (Varyings)0;
                
                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                float ViewDistance = length(_WorldSpaceCameraPos - positionWS);

                float width01 = _OutLineWidth * 0.3 * 0.05* v.vertColor.a;
                float width02 = width01 * ViewDistance;
                float width = lerp(width01, width02, _CameraOutlineRange);

                float3 positionOS = v.positionOS.xyz + normalize(v.normalOS.xyz) * width;
                o.positionCS = TransformObjectToHClip(positionOS.xyz);
                o.uv = v.texcoord.xy;
                o.positionWS.xyz = positionWS;
                o.normalWS.xyz = TransformObjectToWorldDir(v.normalOS);
                o.shadowCoord = TransformWorldToShadowCoord(o.positionWS);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 diffuse = float4(0.5,0.5,0.5,1);//SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half4 outLineColor = diffuse * _OutlineColorControl;
                
                Light mainLight = GetMainLight(i.shadowCoord);
                half NoL = dot(i.normalWS,mainLight.direction);
                outLineColor = saturate(outLineColor * (NoL*0.5 + 0.49));
                
                return outLineColor;
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
                "RenderType"="Opaque" 
                "Queue" = "Geometry"
            }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            //提取自CommonMaterial.hlsl
            half LerpWhiteTo(half b, half t)
            {
                half oneMinusT = 1.0 - t;
                return oneMinusT + b * t;
            }
            
            //#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
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

                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.positionCS = GetShadowPositionHClip(input);
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                //Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, float4(1,1,1,1), 0);
                return 0;
            }
            ENDHLSL
        }
        
    }
    FallBack "Hidden/InternalErrorShader"
}
