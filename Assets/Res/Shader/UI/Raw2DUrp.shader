Shader "Custom/Raw2DUrp"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalRenderPipeline" "RenderType"="Transparent" "Queue"="Transparent"}
        //Tags {"RenderPipeline"="UniversalRenderPipeline" "RenderType"="Opaque" }
        LOD 300


        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            //ZWrite On;
            Blend SrcAlpha OneMinusSrcAlpha
            //Cull On
            //ZTest LEqual
                

            HLSLPROGRAM



            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);


		    struct VertexInput
		    {
			    float4 positionOS:POSITION;
                float2 uv:TEXCOORD0;
		    };

			struct VertexOutput
			{
				float4 positionCS:POSITION;
                float2 uv:TEXCOORD0;

			};


            VertexOutput vert(VertexInput i)
            {
                VertexOutput o;

                o.positionCS = TransformObjectToHClip(i.positionOS);
                o.uv = i.uv;

                return o;
                
            }

            float4 frag(VertexOutput i):SV_Target
            {
                float4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return c;

            }


            ENDHLSL
            
        }
    }
}
