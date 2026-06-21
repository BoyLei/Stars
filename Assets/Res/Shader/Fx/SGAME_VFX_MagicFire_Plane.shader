Shader "SGAME/FX/SGAME_VFX_MagicFire_Plane"
{
    Properties
    {
        _MainTex ("NoiseTex", 2D) = "white" {}
        _ScaleX("Noise Scale X", Range(0.0, 0.2)) = 0.01
        _ScaleY("Noise Scale Y", Range(0.0, 0.2)) = 0.01
        _NoiseStrength("噪声贴图的强度",Range(0,5)) = 1
        _MaskTex("MaskTex",2D) = "white" {}
        _MaskTex2("MaskTex2",2D) = "white" {}
        _SpeedX("SpeedX",Range(-10, 10)) = 0
        _SpeedY("SpeedY",Range(-10, 10)) = 3
        _MiddleFireRange("中焰范围",Range(0,1))= 0.6
        _InsideFireRange("内焰范围",Range(0,1)) = 0.8
        [Header(FireColor)]
        [HDR]_InsideColor("内焰颜色",color) = (0.5,0.5,0.5,1)
        [HDR]_MiddleColor("中焰颜色",color) = (0.5,0.5,0.5,1)
        [HDR]_OutsideColor("外焰颜色",color) = (0.5,0.5,0.5,1)
        
    }
    SubShader
    {
        

        Pass
        {
            Tags { "RenderPipeline"="UniversalPipeline" 
                    "Queue" = "Transparent"}
            //LOD 100
            Cull OFF
            Blend SrcAlpha OneMinusSrcAlpha
            Zwrite OFF
            Stencil
            {
                Ref 10
                Comp Greater
                Pass Keep
                //Fail IncrSat
            }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float _NoiseStrength;
            float _OutLineWidth;
            float _ScaleX;
            float _ScaleY;
            float _SpeedX;
            float _SpeedY;
            float4 _InsideColor;
            float4 _MiddleColor;
            float4 _OutsideColor;
            
            float _MiddleFireRange;
            float _InsideFireRange;
            float4 _MaskTex_ST;
            CBUFFER_END

            TEXTURE2D(_MainTex);   SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);   SAMPLER(sampler_MaskTex);
            TEXTURE2D(_MaskTex2);   SAMPLER(sampler_MaskTex2);
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 texcoord : TEXCOORD0;
                float2 texcoord2: TEXCOORD1;
                float2 texcoord3 : TEXCOORD2;
                float4 tangentOS :  TANGENT;
                float3 normalOS : NORMAL;
                half4 vertColor : COLOR;
            };

            struct Varyings
            {
                float4 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float4 positionWS :TEXCOORD1;
                float4 normalWS   :TEXCOORD2;
                float4 positionOS :TEXCOORD3;
                
            };
            
            

            Varyings vert (Attributes v)
            {
				//billboard mesh towards camera
                Varyings o = (Varyings)0;
				float3 vpos = mul((float3x3)unity_ObjectToWorld, v.positionOS.xyz);
				float4 worldCoord = float4(unity_ObjectToWorld._m03, unity_ObjectToWorld._m13, unity_ObjectToWorld._m23, 1);
				float4 viewPos = mul(UNITY_MATRIX_V, worldCoord) + float4(vpos, 0);
				float4 outPos = mul(UNITY_MATRIX_P, viewPos);
				o.positionCS = outPos;
                
                
                o.uv.xy = v.texcoord.xy;
                o.uv.z = ComputeFogFactor(o.positionCS.z);    //uv的z值存储雾.
                

				//UNITY_TRANSFER_FOG(o,o.pos);
				return o;
               
                
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 ScreenPos = ComputeScreenPos(i.positionCS);
                half2 ScreenUV = GetNormalizedScreenSpaceUV(i.positionCS);
                half2 TexUV = float2(i.positionCS.x * _ScaleX/10,i.positionCS.y * _ScaleY/10) - float2(_SpeedX*_Time.x, _SpeedY*_Time.x);
                //TexUV = float2(i.uv.x* _ScaleX,i.uv.y* _ScaleY) - float2(_SpeedX*_Time.x, _SpeedY*_Time.x);
                //half2 ScreenUV =
                //return float4(ScreenUV.x,ScreenUV.y,1,1);
                // sample the texture
                half4 col = float4(1,1,1,1);
                half4 text = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, TexUV);
                half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv * _MaskTex_ST.xy + _MaskTex_ST.zw +  float2(0 * _Time.x, 0));
                half4 mask2 = SAMPLE_TEXTURE2D(_MaskTex2, sampler_MaskTex2, i.uv);
                
                //return float4(mask.xyz,1);
                
                half3 V = GetWorldSpaceNormalizeViewDir(i.positionWS.xyz);
                half3 N = normalize(i.normalWS);
                //return float4(N,1);
                Light L = GetMainLight();
                half3 Ldir = L.direction;
                half lambert = dot(N,Ldir) *0.5+0.5;
                //return half4(lambert,lambert,lambert,1);
                

                float NdotV =  saturate(dot(N,V));
                //float Rim = pow(NdotV,_RimPower);
                
                //Rim = saturate(Rim*_RimPow) - text.rrr;
                float Rim =  mask.r - text.rrr*_NoiseStrength ;
                Rim = saturate(Rim);
                //Rim = smoothstep(0,0.4,Rim) ;
                //return float4(Rim.rrr,1);
                //Rim = saturate(i.uv.w/20);
                //return float4(Rim,Rim,Rim,1);
                float rimInside = _InsideFireRange; //内焰范围  1 - rimInside
                float rimMiddle = _MiddleFireRange; //中焰范围  rimInside - rimMiddle

                float4 InsideRimColor = _InsideColor * Smootherstep(rimInside,rimInside+0.1,Rim);
                float4 MiddleRimColor = _MiddleColor * (1 -Smootherstep(rimInside,rimInside+0.1,Rim))*Smootherstep(rimMiddle,rimMiddle+0.02,Rim);
                float4 OutsideRimColor = _OutsideColor * (1- Smootherstep(rimMiddle,rimMiddle+0.05,Rim)) * Smootherstep(0,0.01,Rim);
                //return float4(OutsideRimColor.xyz,1);
                col.rgba = (InsideRimColor + MiddleRimColor + OutsideRimColor) ;
                col.a *= saturate(Rim*2) * Smootherstep(0,0.1,mask.r * mask2.r);
                //return float4(col.a,col.a,col.a,1);
                //Rim -= col;
                //return float4(NdotV,NdotV,NdotV,1);
                //混合雾
                col.rgb = MixFog(col.rgb, i.uv.z);
                //col.a = col.r * Rim;
                return col;
                return col;
            }
            ENDHLSL
        }
    }
}
