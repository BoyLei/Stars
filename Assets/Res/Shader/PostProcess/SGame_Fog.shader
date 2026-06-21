Shader "SGAME/FX/SGame_Fog"
{
    Properties
    {
        _MainTex ("主贴图", 2D) = "white" {}
        _Color ("主颜色", color) = (1,1,1,1)
        [NoScaleOffset]_HeightTex("高度贴图",2D) = "Black" {}
        
        _ParallaxOffset("雾气体积强度",Range(0,100)) = 1
        _FogAlpha("雾气的透明度",Range(0,10)) = 1
        //_ParallaxStep("雾气采样步数",Range(0,6)) = 1
        
        _FogDensity("顶点波动密度",Range(0,50)) = 10
        _FogSpeed("顶点运动速度",Range(0,10)) = 1
        _NoiseDensity("噪声的密度",Range(0,20)) = 10
        _FogMoveRange("雾气上下运动的幅度",Range(0,1)) = 1
        
        _USpeed("U移动速度",Range(0,5)) = 0
        _VSpeed("V移动速度",Range(0,5)) = 0
        //_DetailTex2("Texture",2D) = "white"{}
        //_NormalTex("Texture",2D) = "white"{}
    }
    SubShader
    {
        Tags { "Queue"="Transparent"  "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
             Name"lightForward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back
            ZTest LEqual
            ZWrite On
            


             HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
           // #include "UnityCG.cginc"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../lib/PerPixelDisplacement.hlsl"  //视差贴图相关库文件
             
            CBUFFER_START(UnityPerMaterial)
                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                float4 _MainTex_ST;
                TEXTURE2D(_HeightTex);
                SAMPLER(sampler_HeightTex);
                float4 _HeightTex_ST;
                float4 _Color;
                float _ParallaxOffset;
                int _ParallaxStep;
                float _USpeed;
                float _VSpeed;

                float _FogDensity;
                float _FogSpeed;
                float _NoiseDensity;
                float _FogMoveRange;
                float _FogAlpha;

                //深度贴图
                TEXTURE2D(_CameraDepthTexture);
                SAMPLER(sampler_CameraDepthTexture);
            CBUFFER_END
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 normal: NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 positionCS : SV_POSITION;
                float4 normalWS : TEXCOORD1;
                float4 viewDirTS : TEXCOORD2;
                float4 positionWS :TEXCOORD3;
                float4 positionVS :TEXCOORD4;
            };

            // sampler2D _MainTex;
            // float4 _MainTex_ST;

            ///////////////////////////////////////////////////////
            ///  GradientNoise相关函数
             float2 Unity_GradientNoise_Dir_float(float2 p)
            {
                // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }
             void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
            { 
                float2 p = UV * Scale;
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
                float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
            }
            ///////////////////////////////////////////////////////

             
            v2f vert (appdata v)
            {
                v2f o;
                //fog 顶点偏移动画
                 

                float3 posWS = TransformObjectToWorld(v.vertex.xyz);
                float3 viewDirWS = normalize(_WorldSpaceCameraPos - posWS);

                //计算雾气的顶点动画
                 float2 moveSpeed = _Time.xx/_FogSpeed;
                 float2 NoiseUV = posWS.xz /_FogDensity + 2*moveSpeed;
                 //采样噪声图
                 float noise = 0;
                 Unity_GradientNoise_float(NoiseUV,_NoiseDensity,noise);
                 o.viewDirTS.w = noise;  //讲采样的noise放在viewDir的w通道中
                 noise = (noise * 2 -1) * _FogMoveRange/5;
                 float3 fogMove = v.normal * noise;
                 float3 posOSFog = fogMove +v.vertex.xyz; //挪动后的雾气

                o.positionWS.xyz = posWS;
                o.positionVS.xyz =TransformWorldToView(o.positionWS.xyz);
                o.positionCS = TransformObjectToHClip(posOSFog);
                o.uv = v.uv/*TRANSFORM_TEX(v.uv, _MainTex)*/;
                o.normalWS.xyz = TransformObjectToWorldNormal(v.normal.xyz) ;
                
                //计算TangentToWord矩阵
                float3 normalWS =  o.normalWS.xyz;
                float3 tangentWS = 1.0 / length(o.normalWS.xyz )  * TransformObjectToWorldDir(v.tangentOS);
                float crossSign = (v.tangentOS.a > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
                float3 bioTangentWS = crossSign * 1.0 / length(o.normalWS.xyz )  * normalize(cross(normalWS,tangentWS));
                half3x3 TangentToWorld = half3x3(tangentWS.xyz,bioTangentWS.xyz,normalWS.xyz);
                o.viewDirTS.xyz = TransformWorldToTangent(viewDirWS,TangentToWorld);  //获取切线空间下的视角方向
                //o.positionWS.xyz = posWS.xyz;
                //o.tangentWS.xyz = tangentWS;
               // o.tangentWS.w = v.tangentOS.w;
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                //half4 col = tex2D(_MainTex, i.uv);
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                Light mainLight = GetMainLight();
                float3 lightDir = mainLight.direction;

                //UV移动速度
                float2 uvSpeed = float2(_Time.x * _USpeed, _Time.x * _VSpeed);

                //return float4(i.viewDirTS.w,i.viewDirTS.w,i.viewDirTS.w,1);
               // i.uv = i.uv + i.viewDirTS.w * 0.05;
                float NdotL = dot(i.normalWS, lightDir) *0.5 + 0.5;
                PerPixelHeightDisplacementParam ppdParam = (PerPixelHeightDisplacementParam)0;
                ppdParam.uv = i.uv* _MainTex_ST.xy + _MainTex_ST.zw + uvSpeed;
                half height = 0;
                i.viewDirTS.xyz = i.viewDirTS.xyz /**  GetDisplacementObjectScale().xzy*/;
                
                half3 viewDirTS = normalize(half3(i.viewDirTS.xy *_ParallaxOffset *0.03  ,i.viewDirTS.z ));
                float2 POMuv = ParallaxOcclusionMapping(0,0,1/*_ParallaxStep*/,viewDirTS,ppdParam,height , TEXTURE2D_ARGS(_HeightTex, sampler_HeightTex));
                //return float4(height,height,height,1);
                //通过场景深度计算相关的物体.
                //float4 posSS = ComputeScreenPos(i.postionCS);
                float2 screenUV = i.positionCS.xy/_ScaledScreenParams.xy;
                float4 screenPos = ComputeScreenPos(i.positionCS);
                half4 depthMap = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV);
                
                half depthTex = LinearEyeDepth(depthMap, _ZBufferParams);
                float depthNew  = smoothstep(0,2,(depthTex + i.positionVS.z));
                //return half4(depthNew,depthNew,depthNew,1);
                float depthGradient = saturate(depthNew);
                //return float4(depthNew,depthNew,depthNew,1);
                
                float4 color = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,POMuv + i.uv * _MainTex_ST.xy + _MainTex_ST.zw + uvSpeed);
                color.xyz *= _Color.xyz;
                //return float4(GetDisplacementObjectScale().xzy,1);
                float alpha = pow(color.r,10- _FogAlpha) * depthGradient * saturate(i.viewDirTS.w + 0.7);
                //return float4(depthGradient,depthGradient,depthGradient,1);
                return float4(color.xyz, alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
