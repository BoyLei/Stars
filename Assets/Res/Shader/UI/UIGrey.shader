Shader "SGAME/UIGrey"
{
    Properties
    {
        _Grey("变灰程度",Range(0,1)) = 1
        _Color ("Tint", Color) = (1,1,1,1)

        [PerRendererData]_MainTex ("主贴图(A为透明通道)", 2D) = "white" {}
        

        //UI支持MASK
         [PerRendererData]_StencilComp ("Stencil Comparison", Float) = 8
         [PerRendererData]_Stencil ("Stencil ID", Float) = 0
         [PerRendererData]_StencilOp ("Stencil Operation", Float) = 0
         [PerRendererData]_StencilWriteMask ("Stencil Write Mask", Float) = 255
         [PerRendererData]_StencilReadMask ("Stencil Read Mask", Float) = 25
         [PerRendererData]_ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {

        

        Tags { "Queue"="Transparent" "IngoreProjector"="true" "RenderType"="Transparent" "CanUseSpriteAtlas"="true" "PreviewType"="Plane" }


        Pass
        {
           
            ZTest [unity_GUIZTestMode]
            ZWrite Off
            Cull Off
            Lighting Off
            Stencil 
            {
                Ref [_Stencil]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
                Comp [_StencilComp]
                Pass [_StencilOp]
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask [_ColorMask]


            HLSLPROGRAM

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


			#pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5


            CBUFFER_START(UnityPerMaterial)
                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                float4 _MainTex_ST;
                float _Grey;
                float4 _Color;
                float4 _ClipRect;
                float _UIMaskSoftnessX;
                float _UIMaskSoftnessY;

			CBUFFER_END


            struct a2v
            {
                float4 posOS : POSITION;
                float4 color    : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 posCS : SV_POSITION;
                float4 color    : COLOR;
                float4  mask : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };


            v2f vert(a2v v)
            {
                                
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);



                VertexPositionInputs posInput = GetVertexPositionInputs(v.posOS.xyz);
                o.posCS = posInput.positionCS;
                o.uv = v.uv;
                o.color  = v.color * _Color ;
                //遮罩计算
                half2 pixelSize = o.posCS.w;
                pixelSize /= half2(1, 1) * abs(mul((half2x2)UNITY_MATRIX_P, _ScreenParams.xy));
                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                float2 maskUV = (v.posOS.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
                o.uv = TRANSFORM_TEX(v.uv.xy, _MainTex);
                o.mask = float4(v.posOS.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));


                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) *i.color ;

                //计算中遮罩
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(i.mask.xy)) * i.mask.zw);
                col.a *= m.x * m.y;

                half grey=col.r*0.299+col.g*0.587+col.b*0.144;
                half4 col_grey=half4(grey,grey,grey,col.a);
                return lerp(col,col_grey,_Grey);


            }
            ENDHLSL
        }

	
    }
       FallBack "Hidden/InternalErrorShader"
}
