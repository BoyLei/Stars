Shader "SGAME/FX/SGAME_VFX_CharFadeOut"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CharacterAlpha("CharacterAlpha",range(0,1)) = 0
        _CharacterOutRange("CharacterOutRange",range(0,3)) = 0
        _RingColor("RingColor",Color) = (1,1,1,1)
        
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue" = "Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            //#include "UnityCG.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float _CharacterAlpha;
            float _CharacterOutRange;
            float4 _RingColor;
            CBUFFER_END

            struct appdata
            {
                float4 positionOS : POSITION;
                float4 texcoord : TEXCOORD0;
                float2 texcoord2: TEXCOORD1;
                float2 texcoord3 : TEXCOORD2;
                float4 tangentOS :  TANGENT;
                float3 normalOS : NORMAL;
                half4 vertColor : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 normalWS : TEXCOORD1;
                float4 vertexWS : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            //float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                //读取Color中的平滑法线(切线空间) 转换到物体空间下
                float3 BioTangent = normalize(cross(v.normalOS,v.tangentOS)) * v.tangentOS.w;
                float3 BakeNormal = normalize(UnpackNormal(float4(v.vertColor.rgb,1)));
                BakeNormal = normalize(UnpackNormal(float4(v.texcoord2.xy,v.texcoord3.x,1)));
                float3x3 tangentTransform = float3x3(v.tangentOS.xyz, BioTangent.xyz, v.normalOS.xyz);
                float3 SmoothNormalOS = float3(v.tangentOS.x * BakeNormal.x+ BioTangent.x * BakeNormal.y + v.normalOS.x * BakeNormal.z,
                                                v.tangentOS.y * BakeNormal.x + BioTangent.y  * BakeNormal.y + v.normalOS.y * BakeNormal.z,
                                                v.tangentOS.z * BakeNormal.x + BioTangent.z  * BakeNormal.y + v.normalOS.z * BakeNormal.z);
                
                v.positionOS.xyz += SmoothNormalOS * _CharacterOutRange * 0.1;
                o.vertex = TransformObjectToHClip(v.positionOS.xyz);
                o.vertexWS.xyz = TransformObjectToWorld(v.positionOS.xyz);
                o.uv =v.texcoord;
                o.normalWS.xyz = TransformObjectToWorldDir(v.normalOS);
                
                
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                
                float3 V =  GetWorldSpaceNormalizeViewDir(i.vertexWS.xyz);
                float3 N = normalize(i.normalWS);
                float NdotV =  1- dot(N,V);
                //return float4(NdotV,NdotV,NdotV,1);
                //return float4(NdotV,NdotV,NdotV,1);
                // sample the texture
                half4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                
                col.rgb = col.rgb *_RingColor.rgb;
                float alpha = 1 * NdotV *NdotV ;
                //col.a = smoothstep(0,0.4,alpha);
                col.a  = alpha;
                col.a = col.a *  _CharacterAlpha;
                clamp(col.a,0,1);
                //return float4(col.a,col.a,col.a,1);
                
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDHLSL
        }
    }
}
