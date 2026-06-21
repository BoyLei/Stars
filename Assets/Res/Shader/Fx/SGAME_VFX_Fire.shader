Shader "SGAME/FX/SGAME_VFX_Fire"
{
    Properties
    {
       [Header(Noise)]
       _NoiseTex ("噪声贴图", 2D) = "black" {}
       _NoiseStrength("噪声强度",Range(0,2)) = 1 
       _NoiseTex_Uspeed("噪声U方向上的速度", Range(-5,5)) = 0
       _NoiseTex_Vspeed("噪声V方向上的速度", Range(-5,5)) = 0 
       _ButtomFade("火焰底部的扰动减弱",Range(-0.3,1)) = -0.3
        
        [Header(Mask)]
        [KeywordEnum(ON,OFF)]_IfMask("是否开启遮罩贴图",float) = 1
        _MaskTex("遮罩贴图", 2D) = "white" {}
        _RotateMask("遮罩旋转", Range(0,360)) = 0
        [Toggle]_MaskUVClamp("遮罩贴图的UV是否clamp",float) = 0
        
        [Header(Dissolve)]
        _DissolveTex("溶解贴图", 2D) = "white" {}
        _DissolveFactor("溶解系数(CustomData1.x控制)",range(0,1)) = 0
        _DissolveSoothness("软硬程度",range(0,1)) = 0
       [Header(MainTex and Color)]
       _MainTex ("主帖图(RGBA四个通道用来分别控制颜色区域,且区域逐渐缩小)", 2D) = "white" {}
       [HDR]_mainTexRColor("R通道控制颜色 其alpha可控制透明度", Color) = (1,1,1,1)
       [HDR]_mainTexGColor("G通道控制颜色 其alpha可控制透明度", Color) = (1,1,1,1)
       [HDR]_mainTexBColor("B通道控制颜色 其alpha可控制透明度", Color) = (1,1,1,1)
       [HDR]_mainTexAColor("A通道控制颜色 其alpha可控制透明度", Color) = (1,1,1,1)
        
        [Header(CustomData)]
        [Toggle]_IfCustomData("是否由CustomData控制",float)= 0
        
        
//        [Header(Dissolve)]
//        _Dissolve("溶解阈值")
        
//       [Header(Alpha)]
//       _Alpha("透明度",range())
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        LOD 100

        Pass
        {
            Tags
            {
                "LightMode"="UniversalForward"
                
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Zwrite off
            Cull off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local _IFMASK_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            struct a2v
            {
                float4 posOS : POSITION;
                float4 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 posCS : SV_POSITION;
                float4 color : COLOR;
            };
            
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            float4 _MainTex_ST;
            float _ButtomFade;
            float _NoiseStrength;
            float _NoiseTex_Uspeed;
            float _NoiseTex_Vspeed;
            float4 _NoiseTex_ST;

            float4 _MaskTex_ST;
            float _RotateMask;
            float _MaskUVClamp;
            
            float4 _DissolveTex_ST;
            float _DissolveFactor;
            float _DissolveSoothness;

            float4 _mainTexRColor;
            float4 _mainTexGColor;
            float4 _mainTexBColor;
            float4 _mainTexAColor;

            float _IfCustomData;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);
            TEXTURE2D(_DissolveTex);
            SAMPLER(sampler_DissolveTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);


            
            v2f vert(a2v v)
            {
                v2f o;

                VertexPositionInputs posInput = GetVertexPositionInputs(v.posOS.xyz);
                o.posCS = posInput.positionCS;
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
            
            #define RADIAN 0.0174533
            half4 frag(v2f i) : SV_Target
            {
                float time = _Time.y;
                float2 NoiseTexUV = i.uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw + float2(_NoiseTex_Uspeed * time, _NoiseTex_Vspeed * time);
                float4 Noise = SAMPLE_TEXTURE2D(_NoiseTex,sampler_NoiseTex,NoiseTexUV);

                
                float2 MainTexUV = float2(i.uv.x, i.uv.y+ smoothstep(_ButtomFade,_ButtomFade+0.3,i.uv.y) * i.uv.y * Noise.r * _NoiseStrength );
                MainTexUV = MainTexUV.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                MainTexUV = clamp(MainTexUV,0.01,0.99);
                //return float4(MainTexUV.yyy,1);
                
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,MainTexUV);

                half R = mainTex.x;
                half G = mainTex.y;
                half B = mainTex.z ;
                half A = mainTex.a;

                //mask
                #ifdef _IFMASK_ON
                    float2 maskUV = i.uv * _MaskTex_ST.xy + _MaskTex_ST.zw;
                    half cosValue = cos(_RotateMask * RADIAN);
	                half sinValue = sin(_RotateMask * RADIAN);
	                maskUV = mul(maskUV- half2(0.5, 0.5) , half2x2(cosValue, -sinValue, sinValue, cosValue)) + half2(0.5, 0.5);
                    maskUV = lerp(maskUV, clamp(maskUV,0.01,0.99),_MaskUVClamp);
                    
                    float4 mask = SAMPLE_TEXTURE2D(_MaskTex,sampler_MaskTex,maskUV);
                #endif
                
                //dissolve
                float dissFactor =lerp(_DissolveFactor,i.uv.z,_IfCustomData) ;
                float2 dissolveUV = i.uv * _DissolveTex_ST.xy + _DissolveTex_ST.zw;
                float dissolve = SAMPLE_TEXTURE2D(_DissolveTex,sampler_DissolveTex,dissolveUV);
                
                #ifdef _IFMASK_ON
                    //dissolve和mask混合
                    dissolve *= mask.r + mask.r;
                #endif
                
                float powDissolve = saturate(pow(abs(dissolve), 1) + dissolve/2);
				float temp_output_2_0_g571 = (_DissolveSoothness + 1.0) * dissFactor;
				float smoothstepResult21_g571 = smoothstep((temp_output_2_0_g571 - _DissolveSoothness) , temp_output_2_0_g571 , powDissolve);
				float dissolveAlpha =smoothstepResult21_g571;
                
                half4 col = float4(1,1,1,1);
                col.rgba = R * _mainTexRColor + G * _mainTexGColor + B * _mainTexBColor + A * _mainTexAColor;
                col.a *= i.color.a * dissolveAlpha;
                return col;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
