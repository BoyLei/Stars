Shader "SGAME/FX/SGAME_VFX_RadialBlur"
{
    //径向模糊shader
    Properties
    {
        [HideInInspector]_MainTex ("Texture", 2D) = "white" {}
        [Header(DistortTex)]
        [KeywordEnum(ON,OFF)] _Tex("是否启用速度线贴图",Range(0,1)) = 1
        _DistortTex("扭曲径向模糊的速度线贴图",2D) = "white"{}
        _RadialScale("速度线径向长度",range(0,11)) = 0.22
        _LengthScale("速度线密度",range(0,50)) = 20
        _SpeedMove("速度线径向运动速度",range(-5,5)) = 0
        _RotateRange("旋转速度",range(-10,10)) =0
       
        
        [Header(SecondDistortTex)]
        [KeywordEnum(ON,OFF)] _Tex2("是否启用第二张速度线贴图",Range(0,1)) = 1
        _DistortTex2("扭曲径向模糊的速度线贴图",2D) = "white"{}
        _RadialScale2("速度线径向长度",range(0,11)) = 0.22
        _LengthScale2("速度线密度",range(0,50)) = 20
        _SpeedMove2("速度线径向运动速度",range(-5,5)) = 0
        _RotateRange2("旋转速度",range(-10,10)) =0
//        _DistortScale2("速度线扭曲程度(Custom1.x)",range(1,3)) = 1.7
//        _DistortStrength2("速度线影响因子,越大则速度线的影响越强",range(0,1)) = 1
        
        [Space(20)]
        [KeywordEnum(ON,OFF)] _IfSeeSpeedLine("是否察看速度线",Range(0,1)) = 1
        _DistortScale("速度线扭曲程度(Custom1.x)",range(1,3)) = 1.7
        _DistortStrength("速度线影响因子,越大则速度线的影响越强",range(0,1)) = 1
        
        
        [Space(20)]
        [Header(MaskTexture)]
        [KeywordEnum(ON,OFF)] _MaskTexx("是否启用遮罩贴图",Range(0,1)) = 1
        _MaskTex("遮罩贴图",2D) = "white" {}
        
        [Space(20)]
        [Header(Blur)]
        _TexelSize("径向模糊距离(Custom1.y)",range(0,50)) = 13
        _SampleTime("采样次数,设置为整数,在保证效果的同时尽量调小",range(1,5)) = 5
        [KeywordEnum(SCREEN,MESH)] _Central("中心是参照屏幕uv,还是参照面片uv",Range(0,1)) = 1
        _CentralX("中心点UV的X坐标",range(0,1)) = 0.5
        _CentralY("中心点UV的Y坐标",range(0,1)) = 0.5
        
        [Space(20)]
        [Header(Mask)]
        _maskScale("中心遮罩的大小(Custom1.z)",range(0,1)) = 0.17
        _maskBlur("中心遮罩的模糊程度",range(0,1))=0.12
        
        [Space(20)]
        [Header(Advanced)]
        [Enum(UnityEngine.Rendering.CompareFunction)]_Ztest("Ztest(永远在最上层选Always,正常受物件遮挡则选LessEqual)", Float) = 8
        [KeywordEnum(ON,OFF)] _CustomData("是否启用CustomData",Range(0,1)) = 1
        _Alpha("整体透明度(Custom1.w)",range(0,1)) = 1
        
        //        float _Sector = 1;
//                    float _Angle = 40;
//                    float _Outline = 1;
    }
    SubShader
    {
        Tags { "RenderType" = "Overlay" "RenderPipeline" = "UniversalPipeline" "Queue" = "Overlay"  "LightMode"="Distortion" }
        LOD 100
        Cull off
		ZWrite off
		ZTest [_Ztest]
        //ZTest Always //永远在最上方
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _TEX_ON //_TEX_OFF
            #pragma shader_feature_local _TEX2_ON //_TEX_OFF
            #pragma shader_feature_local _CENTRAL_SCREEN //_CENTRAL_MESH
            #pragma shader_feature_local _MASKTEXX_ON
            #pragma shader_feature_local _CUSTOMDATA_ON
            #pragma shader_feature_local _IFSEESPEEDLINE_ON
            

           // #include "UnityCG.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 customData : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 color : TEXCOORD1;
                float4 screenPos :TEXCOORD2;
                float4 customData : TEXCOORD3;
            };

            //sampler2D _MainTex;
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_DistortTex);
            SAMPLER(sampler_DistortTex);
            TEXTURE2D(_CameraOpaqueAndTransparentTexture);
            SAMPLER(sampler_CameraOpaqueAndTransparentTexture);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            TEXTURE2D(_DistortTex2);
            SAMPLER(sampler_DistortTex2);
            

            // TEXTURE2D(_CameraOpaqueTexture);
            // SAMPLER(sampler_CameraOpaqueTexture);
            
            CBUFFER_START(UnityPerMaterial)
            float _RadialScale;
            float _LengthScale;
            float _SpeedMove;
            float _RotateRange;
            float _DistortScale;
            float _DistortStrength;
            float _TexelSize;
            float _maskScale;
            float _maskBlur;
            int _SampleTime;
            float _CentralX;
            float _CentralY;
            float _Alpha;
            float4 _CameraOpaqueAndTransparentTexture_TexelSize;

            float _RadialScale2;
            float _LengthScale2;
            float _SpeedMove2;
            float _RotateRange2;
            float _DistortScale2;
            float _DistortStrength2;
            CBUFFER_END


            float3 SampleOpaqueTransSceneColor(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_CameraOpaqueAndTransparentTexture, sampler_CameraOpaqueAndTransparentTexture, UnityStereoTransformScreenSpaceTex(uv)).rgb;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = v.uv;
                o.customData = float4(v.uv.zw,v.customData.xy);
                //float2 uvMoved = v.uv + -1 * dirToOriginalPnanoint *0.1*_Range;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {

                //采样扭曲贴图
                float2 screenUV = i.screenPos.xy/i.screenPos.w;
                
                float distortion =1;
                

                #ifdef _CUSTOMDATA_ON
                float distortScale = i.customData.x;
                #else
                float distortScale = _DistortScale;
                #endif


                
                
                #ifdef _TEX_ON
                #ifdef _CENTRAL_SCREEN
                float2 centralUV = screenUV - float2(_CentralX,_CentralY);
                float2 distortUV = (float2(( length( centralUV ) * (_RadialScale) * 2.0 ) , ( (atan2( centralUV.x , centralUV.y ) + _Time.x *_RotateRange) * ( 1.0 / TWO_PI ) * _LengthScale )));
                float2 panner = _Time.y * _SpeedMove + distortUV;
				float4 distortTex = SAMPLE_TEXTURE2D(_DistortTex,sampler_DistortTex,panner);
                distortion = (distortTex*2 -1) * distortScale;
                distortion = lerp(1,distortion,_DistortStrength);
                #else
                float2 centralUV = i.uv - float2(_CentralX,_CentralY);
                float2 distortUV = (float2(( length( centralUV ) * (_RadialScale)* 2.0 ) , ( (atan2( centralUV.x , centralUV.y ) + _Time.x *_RotateRange) * ( 1.0 / TWO_PI ) * _LengthScale )));
                float2 panner = _Time.y * _SpeedMove + distortUV;
				float4 distortTex = SAMPLE_TEXTURE2D(_DistortTex,sampler_DistortTex,panner);
                distortion = (distortTex*2 -1) * distortScale;
                distortion = lerp(1,distortion,_DistortStrength);
                #endif
                #endif


                #ifdef _TEX2_ON
                #ifdef _CENTRAL_SCREEN
                float2 centralUV2 = screenUV - float2(_CentralX,_CentralY);
                float2 distortUV2 = (float2(( length( centralUV2 ) * (_RadialScale2) * 2.0 ) , ( (atan2( centralUV2.x , centralUV2.y ) + _Time.x *_RotateRange2) * ( 1.0 / TWO_PI ) * _LengthScale2 )));
                float2 panner2 = _Time.y * _SpeedMove2 + distortUV2;
                float4 distortTex2 = SAMPLE_TEXTURE2D(_DistortTex2,sampler_DistortTex2,panner2);
                float distortion2 = (distortTex2*2 -1) * distortScale;
                distortion2 = lerp(0,distortion2,_DistortStrength);
                distortion = clamp(distortion2 + distortion,0,1);
                #else
                float2 centralUV2 = i.uv - float2(_CentralX,_CentralY);
                float2 distortUV2 = (float2(( length( centralUV2 ) * (_RadialScale2) * 2.0 ) , ( (atan2( centralUV2.x , centralUV2.y ) + _Time.x *_RotateRange2) * ( 1.0 / TWO_PI ) * _LengthScale2 )));
                float2 panner2 = _Time.y * _SpeedMove2 + distortUV2;
                float4 distortTex2 = SAMPLE_TEXTURE2D(_DistortTex2,sampler_DistortTex2,panner2);
                float distortion2 = (distortTex2*2 -1) * distortScale;
                distortion2 = lerp(0,distortion2,_DistortStrength);
                distortion = clamp(distortion2 + distortion,0,1);
                #endif
                #endif
                distortion = distortion*2-1;
                
                
                //扭曲
                float2 dir = float2(screenUV.x - _CentralX,screenUV.y - _CentralY);
                float dist = length(dir);
                float validdist =step(0.0,dist);
                dir = normalize(dir);
                
                //float3 sceneColor = SampleOpaqueTransSceneColor(screenUV);
                #ifdef _CUSTOMDATA_ON
                float texelSize = i.customData.y;
                #else
                float texelSize = _TexelSize;
                #endif
                
                float3 sum = 0;
                for(int j = 1; j<= _SampleTime; j++)
                {
                    float2 offset =(texelSize*10/j) *  dir * dist * validdist * distortion ;
                    sum += SampleOpaqueTransSceneColor(screenUV + offset * _CameraOpaqueAndTransparentTexture_TexelSize);
                    sum += SampleOpaqueTransSceneColor(screenUV - offset * _CameraOpaqueAndTransparentTexture_TexelSize);
                }
                
                sum = sum / (_SampleTime*2);


                #ifdef _CUSTOMDATA_ON
                float maskScale = i.customData.z;
                #else
                float maskScale = _maskScale;
                #endif
                
                //中心遮罩
                #ifdef _CENTRAL_SCREEN
                float mask = smoothstep(maskScale,maskScale + _maskBlur,dist);
                #else
                float2 dir2 = float2(i.uv.x - _CentralX,i.uv.y - _CentralY);
                float distToMeshUVCentral =length(dir2);
                float mask = smoothstep(maskScale,maskScale + _maskBlur,distToMeshUVCentral);
                #endif

                //遮罩贴图
                #ifdef _MASKTEXX_ON
                float maskTex = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv).rgb;
                mask *= maskTex.r;
                #endif
                

                #ifdef _CUSTOMDATA_ON
                mask *= i.customData.w;
                //return float4(i.customData.w,i.customData.w,i.customData.w,1);
                #else
                mask *=_Alpha;
                #endif
                
                //sum = lerp(sceneColor,sum,mask);

                #ifdef _IFSEESPEEDLINE_ON
                sum = (distortion+1)/2;
                mask = 1;
                #endif
                
                return float4(sum,mask);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
    //CustomEditor "SkillControlerGUI"
}