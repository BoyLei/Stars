Shader "SGAME/FX/SGAME_VFX_BlackWhiteFalsh"
{
    //径向模糊shader
    Properties
    {
        [Header(BlackWhiteFlash)]
        [KeywordEnum(ON,OFF)] _BlackWhite("是否开启黑白闪",Range(0,1)) = 0
        _WhiteColor("黑白闪颜色1",Color) = (1,1,1,1)
        _BlackColor("黑白闪颜色2",Color) = (0,0,0,1)
        [Toggle]_ifInverseByColor("颜色反转是否由顶点色控制",int) = 0
        [Toggle]_ifInverse("颜色反转(由顶点色控制时该按钮无效)",int) = 0
        _BWTexStr("黑白闪纹理强度",range(0,1)) = 0.2
        _BWRange("黑白范围",range(0,1)) = 0.368
        _BWTrans("黑白过渡",range(0,0.1)) = 0.09
        
        [Header(DarkCorner)]
        [Toggle]_ifDarkCornerCenter("暗角的中心是否受UV偏移影响",int) = 0
        _darkCornerScale("暗角的大小",range(0,1)) = 0.4
        _darkCornerBlur("暗角的模糊程度",range(0,1))=0.7
        
        [HideInInspector]_MainTex ("Texture", 2D) = "white" {}
        [Header(DistortTex)]
        [KeywordEnum(ON,OFF)] _Tex("是否启用速度线贴图",Range(0,1)) = 0
        _DistortTex("扭曲径向模糊的速度线贴图",2D) = "white"{}
        _RadialScale("速度线径向长度",range(0,11)) = 0.25
        _LengthScale("速度线密度",range(0,100)) = 7
        _SpeedMove("速度线径向运动速度",range(-5,5)) = 0
        _RotateRange("旋转速度",range(-10,10)) =0
       
        
        [Header(SecondDistortTex)]
        [KeywordEnum(ON,OFF)] _Tex2("是否启用第二张速度线贴图",Range(0,1)) = 0
        _DistortTex2("扭曲径向模糊的速度线贴图",2D) = "white"{}
        _RadialScale2("速度线径向长度",range(0,11)) = 0.32
        _LengthScale2("速度线密度",range(0,50)) = 7.5
        _SpeedMove2("速度线径向运动速度",range(-5,5)) = 0
        _RotateRange2("旋转速度",range(-10,10)) =0
        
        
        [Space(20)]
        [KeywordEnum(ON,OFF)] _IfSeeSpeedLine("是否察看速度线(附带遮罩)",Range(0,1)) = 1
        //_DistortScale("速度线扭曲程度(Custom1.x)",range(1,3)) = 1.3
        _DistortStrength("速度线影响因子",range(0,2)) = 0.55
        
//        [Space(20)]
//        [Header(Mask)]
//        _maskScale("中心遮罩的大小(Custom1.z)",range(0,1)) = 0.17
//        _maskBlur("中心遮罩的模糊程度",range(0,1))=0.12
        
        [Space(20)]
        [Header(MaskTexture)]
        [KeywordEnum(ON,OFF)] _MaskTexx("是否启用遮罩贴图",Range(0,1)) = 1
        [Toggle]_OneMinusMask("遮罩图反相",float) = 0
        _MaskTex("遮罩贴图(越黑则速度线越弱,反之越强)",2D) = "white" {}
        _MaskTexStr("遮罩贴图强度",range(0,1)) = 1
        
        [Space(20)]
        [Header(Blur)]
        _TexelSize("径向偏移距离",range(-50,50)) = -7
        _SampleTime("模糊程度,设置为整数,在保证效果的同时尽量调小",range(1,4)) = 2
        //[KeywordEnum(SCREEN,MESH)] _Central("中心是参照屏幕uv,还是参照面片uv",Range(0,1)) = 1
        _CentralX("中心点UV的X坐标",range(0,1)) = 0.5
        _CentralY("中心点UV的Y坐标",range(0,1)) = 0.5
        
        
        
        //[Space(20)]
        //[Header(Advanced)]
        //[Enum(UnityEngine.Rendering.CompareFunction)]_Ztest("Ztest(永远在最上层选Always,正常受物件遮挡则选LessEqual)", Float) = 8
        //[KeywordEnum(ON,OFF)] _CustomData("是否启用CustomData",Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Overlay" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent"  "LightMode"="AfterPostProcess" }
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
            //#pragma shader_feature_local _CENTRAL_SCREEN //_CENTRAL_MESH
            #define _CENTRAL_SCREEN 1
            #pragma shader_feature_local _ _MASKTEXX_ON
            #pragma shader_feature_local _ _BLACKWHITE_ON
            //#pragma shader_feature_local _ _CUSTOMDATA_ON
            #pragma shader_feature_local _ _IFSEESPEEDLINE_ON
            

           // #include "UnityCG.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 customData : TEXCOORD1;
                float4 color : COLOR;
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
            //
            TEXTURE2D(_DistortTex2);
            SAMPLER(sampler_DistortTex2);
            

            // TEXTURE2D(_CameraOpaqueTexture);
            // SAMPLER(sampler_CameraOpaqueTexture);
            
            CBUFFER_START(UnityPerMaterial)
            float4 _MaskTex_ST;
            
            float4 _WhiteColor;
            float4 _BlackColor;
            float _ifInverseByColor;
            float _ifInverse;
            float _BWTexStr;
            float _BWRange;
            float _BWTrans;

            float _ifDarkCornerCenter;
            float _darkCornerScale;
            float _darkCornerBlur;
            
            float _RadialScale;
            float _LengthScale;
            float _SpeedMove;
            float _RotateRange;
            float _DistortScale;
            float _DistortStrength;
            float _TexelSize;
            // float _maskScale;
            // float _maskBlur;
            int _SampleTime;
            float _CentralX;
            float _CentralY;
            float _Alpha;
            float4 _CameraOpaqueAndTransparentTexture_TexelSize;

            float _RadialScale2;
            float _LengthScale2;
            float _SpeedMove2;
            float _RotateRange2;
            // float _DistortScale2;
            // float _DistortStrength2;

            float _OneMinusMask;
            float _MaskTexStr;

            float4 _DistortTex_ST;
            float4 _DistortTex2_ST;
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
                o.color = v.color.rgb;
                //float2 uvMoved = v.uv + -1 * dirToOriginalPnanoint *0.1*_Range;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {

                //采样扭曲贴图
                float2 screenUV = i.screenPos.xy/i.screenPos.w;
                
                float distortion =1;
                

                // #ifdef _CUSTOMDATA_ON
                // float distortScale = i.customData.x;
                // #else
                // float distortScale = _DistortScale;
                // #endif
                float distortScale = 1;

                
                
                #ifdef _TEX_ON
                float2 centralUV = screenUV - float2(_CentralX,_CentralY);
                centralUV = centralUV * _DistortTex_ST.xy + _DistortTex_ST.zw;
                float2 distortUV = (float2(( length( centralUV ) * (_RadialScale) * 2.0 ) , ( (atan2( centralUV.x , centralUV.y )) * ( 1.0 / TWO_PI ) * _LengthScale * 10 )));
                float2 panner = float2(_Time.y * _SpeedMove, _Time.x *_RotateRange*10) + distortUV;
				float4 distortTex = SAMPLE_TEXTURE2D(_DistortTex,sampler_DistortTex,panner);
                distortion = (distortTex) * distortScale;
                distortion = lerp(0,distortion,_DistortStrength);
                #endif


                #ifdef _TEX2_ON
                float2 centralUV2 = screenUV - float2(_CentralX,_CentralY);
                centralUV2 = centralUV2 * _DistortTex2_ST.xy + _DistortTex2_ST.zw;
                float2 distortUV2 = (float2(( length( centralUV2 ) * (_RadialScale2) * 2.0 ) , ( (atan2( centralUV2.x , centralUV2.y )) * ( 1.0 / TWO_PI ) * _LengthScale2 * 10 )));
                float2 panner2 = float2(_Time.y * _SpeedMove2,_Time.x *_RotateRange2*10) + distortUV2;
                float4 distortTex2 = SAMPLE_TEXTURE2D(_DistortTex2,sampler_DistortTex2,panner2);
                float distortion2 = (distortTex2) * distortScale;
                distortion2 = lerp(0,distortion2,_DistortStrength);
                distortion = clamp(distortion2 * distortion,0,1);
                #endif
                distortion = clamp(distortion,0,1);
                
                //扭曲
                float2 dir = float2(screenUV.x - _CentralX,screenUV.y - _CentralY);
                float dist = length(dir);
                float dist2Centre = length(float2(screenUV.x - 0.5,screenUV.y - 0.5));
                float validdist =step(0.0,dist);
                dir = normalize(dir);
                
                //float3 sceneColor = SampleOpaqueTransSceneColor(screenUV);
                #ifdef _CUSTOMDATA_ON
                float texelSize = i.customData.y;
                #else
                float texelSize = _TexelSize;
                #endif

                //  #ifdef _CUSTOMDATA_ON
                // float maskScale = i.customData.z;
                // #else
                // float maskScale = _maskScale;
                // #endif

                float mask = 1;
                // //中心遮罩
                // #ifdef _CENTRAL_SCREEN
                //  mask = smoothstep(maskScale,maskScale + _maskBlur,dist);
                // #else
                // float2 dir2 = float2(i.uv.x - _CentralX,i.uv.y - _CentralY);
                // float distToMeshUVCentral =length(dir2);
                //  mask = smoothstep(maskScale,maskScale + _maskBlur,distToMeshUVCentral);
                // #endif

                 //遮罩贴图
                #ifdef _MASKTEXX_ON
                float2 maskTexUV = screenUV.xy * _MaskTex_ST.xy + _MaskTex_ST.zw;
                float maskTex = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, maskTexUV).rgb;
                maskTex.r = lerp(maskTex.r,1 - maskTex.r,_OneMinusMask);
                mask *= 1 - (1- maskTex.r) *  _MaskTexStr;
                //return half4(mask,mask,mask,1);
                #endif
                //return half4(mask,mask,mask,1);
                
                float3 sum = 0;

                for(int j = 1; j<= _SampleTime ; j++)
                {
                    float2 offset =(texelSize*10/j) *  dir * dist * validdist  * mask * _CameraOpaqueAndTransparentTexture_TexelSize;
                    sum += SampleOpaqueTransSceneColor(screenUV + offset * distortion );
                    //sum += SampleOpaqueTransSceneColor(screenUV);
                    //sum += SampleOpaqueTransSceneColor(screenUV - offset * (1-distortion));
                }

               // return half4(distortion,distortion,distortion,1);
                sum = sum / (_SampleTime /** 2*/);

                
                
                //黑白闪
                #ifdef _BLACKWHITE_ON
                float Brightness = (sum.r +sum.g + sum.b)/3;
                float distortion3 = lerp(1,distortion,_BWTexStr * mask);
                //distortion3 = lerp(Brightness,distortion3,mask);
                float Brightness2 = distortion3 * Brightness + Brightness;
                float ColorCut = smoothstep(_BWRange,_BWRange + _BWTrans,Brightness2);
                ColorCut = lerp(ColorCut,1-ColorCut,lerp(_ifInverse, i.color.r,_ifInverseByColor));
                //暗角
                float darkCorner =  1- smoothstep(_darkCornerScale,_darkCornerScale + _darkCornerBlur,dist * _ifDarkCornerCenter  + dist2Centre * (1 - _ifDarkCornerCenter));
                half3 FlashColor = lerp(_BlackColor,_WhiteColor,ColorCut * darkCorner);
                sum = FlashColor;
                
                //return float4(FlashColor,1);
                #endif//输出速度线
                
                #ifdef _IFSEESPEEDLINE_ON
                return half4(distortion * mask,distortion * mask,distortion * mask,1);
                #endif
                // #ifdef _CUSTOMDATA_ON
                // mask *= i.customData.w;
                // //return float4(i.customData.w,i.customData.w,i.customData.w,1);
                // #else
                // mask *=_Alpha;
                // #endif
                
                mask = 1;
                return float4(sum,mask);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
    //CustomEditor "SkillControlerGUI"
}