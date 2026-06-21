Shader "SGAME/FX/SGAME_WSpos_Unlit"
{
    Properties
    {
       _MainTex ("主贴图", 2D) = "white" {}
       _ParallaxTex ("视差贴图",2D) = "White" {}
       _ParallaxStrength("视差强度",range(0,1)) = 0.1
       [HDR]_Color("Color", Color) = (1,1,1,1)
       _MaskTex("遮罩贴图",2D) = "white" {}
       _Alpha("整体透明度",range(0,1)) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"="Transparent" 
            "Queue"="Transparent"
        }
        LOD 100

        Pass
        {

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
            #include "./../../../ThirdParts/GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc"
            #pragma instancing_options procedural:setupGPUI
            #pragma multi_compile_instancing

            struct a2v
            {
                float4 posOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 posCS : SV_POSITION;
                float4 posWS : TEXCOORD1;
                float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
                float4 color : TEXCOORD6;
            };
            
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            half4 _MainTex_ST;
            half _ParallaxStrength;
            half _Alpha;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            TEXTURE2D(_ParallaxTex);
            SAMPLER(sampler_ParallaxTex);

            v2f vert(a2v v)
            {
                v2f o;

                VertexPositionInputs posInput = GetVertexPositionInputs(v.posOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs( v.normal, v.tangent );

            	o.tSpace0 = float4( normalInput.normalWS, posInput.positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS,posInput.positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, posInput.positionWS.z);
                
                o.posCS = posInput.positionCS;
                o.posWS.xyz = posInput.positionWS;
                o.uv = v.uv;
                o.color = v.color;
                
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                
                float2 mainTexUV = i.posWS.xz * (_MainTex_ST.xy * 0.2) + _MainTex_ST.zw;
                
                //ViewDirTan
                //float3 ase_worldNormal = i.tSpace0.xyz;
                //float3 ase_worldTangent = i.tSpace1.xyz;
				//float3 ase_worldBitangent = i.tSpace2.xyz;
                float3 WorldPosition = float3(i.tSpace0.w,i.tSpace1.w,i.tSpace2.w);
                //float3 tanToWorld0 = float3( ase_worldTangent.x, ase_worldBitangent.x, ase_worldNormal.x );
				// float3 tanToWorld1 = float3( ase_worldTangent.y, ase_worldBitangent.y, ase_worldNormal.y );
				// float3 tanToWorld2 = float3( ase_worldTangent.z, ase_worldBitangent.z, ase_worldNormal.z );
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
                ase_worldViewDir = SafeNormalize(ase_worldViewDir) ;
				// ase_worldViewDir = normalize(ase_worldViewDir);
				// float3 ase_tanViewDir =  tanToWorld0 * ase_worldViewDir.x + tanToWorld1 * ase_worldViewDir.y  + tanToWorld2 * ase_worldViewDir.z;
                //ase_tanViewDir = normalize(ase_tanViewDir);
                

                //采样视差
                half parallax = SAMPLE_TEXTURE2D(_ParallaxTex,sampler_ParallaxTex,mainTexUV).r;
                half height = parallax;
                half scale = _ParallaxStrength;
                half3 tanViewDir = ase_worldViewDir.xzy;
                
                half2 offsetMainTexUV = ( ( height - 1 ) * ( tanViewDir.xy / tanViewDir.z ) * scale ) + mainTexUV;
				
                half4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,offsetMainTexUV) * _Color;
                //透明遮罩
                half4 mask =SAMPLE_TEXTURE2D(_MaskTex,sampler_MaskTex,i.uv);
                col.a =saturate(mask.r * _Alpha) ;
                col *= i.color;
                
                
                
                return col;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
