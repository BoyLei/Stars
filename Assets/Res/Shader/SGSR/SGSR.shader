Shader "SGSR/SGSR"
{
    Properties
    {

        _MainTex ("主贴图(A为透明通道)", 2D) = "white" {}
        _EdgeSharpness("Edge sharp",Range(0,2)) = 1

    }
    SubShader
    {
        Tags 
        {
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

        Pass
        {
            Tags
            {
                "LightMode"="UniversalForward"
            }


            HLSLPROGRAM

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"



			#pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            CBUFFER_START(UnityPerMaterial)
                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                float4 _MainTex_ST;
                float _EdgeSharpness;
                float _ScreenWidth;
                float _ScreenHeight;
			CBUFFER_END


            #define  SGSR_H 1

            half4 SGSRRH(float2 p)
            {
                half4 res = _MainTex.GatherRed(sampler_MainTex, p);
                return res;
            }
            half4 SGSRGH(float2 p)
            {
                half4 res = _MainTex.GatherGreen(sampler_MainTex, p);
                return res;
            }
            half4 SGSRBH(float2 p)
            {
                half4 res = _MainTex.GatherBlue(sampler_MainTex, p);
                return res;
            }
            half4 SGSRAH(float2 p)
            {
                half4 res = _MainTex.GatherAlpha(sampler_MainTex, p);
                return res;
            }
            half4 SGSRRGBH(float2 p)
            {
                half4 res = _MainTex.SampleLevel(sampler_MainTex, p, 0);
                return res;
            }

            half4 SGSRH(float2 p, uint channel)
            {
                if (channel == 0)
                    return SGSRRH(p);
                if (channel == 1)
                    return SGSRGH(p);
                if (channel == 2)
                    return SGSRBH(p);
                return SGSRAH(p);
            }

            #include "./sgsr_mobile.hlsl"


            struct a2v
            {
                float4 posOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 posCS : SV_POSITION;
            };
            
            half4 SnapdragonGameSuperResolution(float2 uv)
            {
                float4 ViewportInfo=float4(1.0/_ScreenWidth,1.0/_ScreenHeight,_ScreenWidth,_ScreenHeight);

	            half4 OutColor = half4(0, 0, 0, 1);
                SgsrYuvH(OutColor, uv, ViewportInfo,_EdgeSharpness);
                return OutColor;
            }
            

            v2f vert(a2v v)
            {
                v2f o;

                VertexPositionInputs posInput = GetVertexPositionInputs(v.posOS.xyz);
                o.posCS = posInput.positionCS;
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 col = SnapdragonGameSuperResolution(i.uv);
                return col;
            }
            ENDHLSL
        }

	
    }
       FallBack "Hidden/InternalErrorShader"
}
