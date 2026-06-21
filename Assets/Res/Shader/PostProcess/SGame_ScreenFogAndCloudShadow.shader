Shader "SGAME/Screen Fog And Cloud Shadow"
{
	Properties
    {
        _MainTex ("Texture", 2D) = "white"
        _FogNoiseTexture ("Fog Noise", 2D) = "white"
    }

    HLSLINCLUDE

        #pragma shader_feature_local _ _FOG_ENABLE
        #pragma shader_feature_local _ _FOG_MIX_ADD _FOG_MIX_BLEND
        #pragma shader_feature_local _ _CLOUDSHADOW_ENABLE
        #pragma shader_feature_local _ _FADE_SKY
        #pragma shader_feature_local _ _PLAYER_INTERACTIVE
        #pragma shader_feature_local _ _RGB_SPLIT_ENABLE


        //#pragma shader_feature_local _ _MAINLIGHT_COOKIES
        

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"


        float4 _FogColor;
        float4 _SkyColor;

        //float4 _FogInscatteringColor;
        //float4 _FogExtinctionColor;
        //float4 _FogSunColor;
        //float3 _SunLightDir;

        //float _InscatteringFallOff;
        //float _ExtinctionFallOff;
        
        float _FogHeightStart;
        float _FogHeight;
        float _HeightFogFalloff;
        float _HeightFogDensity;

        float _DepthFogDensity;
        float _DepthFogFalloff;
        float _LinearOffs;
        float _LinearGrad;
        float _DistanceOffset;


        TEXTURE2D(_CloudShadowTexture);
		SAMPLER(sampler_CloudShadowTexture);
		
        TEXTURE2D(_FogNoiseTexture);
        SAMPLER(sampler_FogNoiseTexture);

        float4 _FogMoveSpeed;
        float _FogNoiseSize1;
        float _FogNoiseSize2;
        float _FogNoiseDepthFalloff;


        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        float2 _CloudShadowMoveSpeed;
        float4 _CloudShadowColor;
        float _CloudShadowSize;
        float _FogNoiseContract;

        TEXTURE2D(_FogLightmap);
        SAMPLER(sampler_FogLightmap);

        TEXTURE2D(_TerrainWorldLightmap);
        SAMPLER(sampler_TerrainWorldLightmap);
        
        float4 _FogLightmapPosAndSize;
        float4 _FogLightmapColor;
        float _FogLightmapDistanceFalloff;

        float _FogLightmapFalloff;
        float2 _FogLightmapFalloffMin;

        float _FogBlendFactor;

        float4 _PlayerPosAndRadius;
        float _InteractiveFalloff;



        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
        };

        
        struct Varyings
        {
            float4 positionHCS : SV_POSITION;
            float2 texcoord : TEXCOORD0;
            float3 viewDir: TEXCOORD1;
        };

        Varyings Vert(Attributes IN)
        {
            Varyings OUT;
            OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
            OUT.texcoord = IN.uv;
            float3 viewDir = mul(unity_CameraInvProjection, float4(IN.uv * 2.0 - 1.0, 0, -1)).xyz;
            OUT.viewDir = mul(unity_CameraToWorld, float4(viewDir, 0)).xyz;

            return OUT;
        }

        // plane degined by p (p.xyz must be normalized)
        float rayPlaneIntersect(float3 ro, float3 rd, float4 p)
        {
            return -(dot(ro,p.xyz)+p.w)/dot(rd,p.xyz);
        }

        float Remap(float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo)
        {
	        return targetFrom + (source - sourceFrom) * (targetTo - targetFrom) / (sourceTo - sourceFrom);
        }

        float FastRemap(float source, float targetFrom, float targetTo)
        {
	        return targetFrom + source * (targetTo - targetFrom);
        }

        float4 Frag(Varyings input) : SV_Target
        {
            float4 sceneColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord);
            
            Light mainLight = GetMainLight();
            float3 viewDir = normalize(input.viewDir);
			float3 lightDir = normalize(mainLight.direction);
			float3 cameraPos = GetCameraPositionWS();

#if UNITY_REVERSED_Z
            float rawDepthForDepthFog = SampleSceneDepth(input.texcoord).x;
            float rawDepth = SampleSceneDepth(input.texcoord).x;
#else
            float depthTemp = SampleSceneDepth(input.texcoord).x;
            float rawDepthForDepthFog = depthTemp;
            float rawDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, depthTemp);
#endif
            float sceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
            float depth = Linear01Depth(rawDepth, _ZBufferParams);
            float depthForDepthFog = Linear01Depth(rawDepthForDepthFog, _ZBufferParams);

            float fadeSky = step(depth, 0.9999);

            
#if _CLOUDSHADOW_ENABLE
            float3 worldPosClooud = ComputeWorldSpacePosition(input.texcoord, rawDepth, UNITY_MATRIX_I_VP);
            float2 cloudUv = (worldPosClooud.xz + _Time.x * _CloudShadowMoveSpeed) * _CloudShadowSize;
            float4 cloudColor = SAMPLE_TEXTURE2D(_CloudShadowTexture, sampler_CloudShadowTexture, cloudUv);
            sceneColor.rgb = lerp(sceneColor.rgb, _CloudShadowColor.rgb, fadeSky * cloudColor.r * _CloudShadowColor.a);
#endif

#if _FOG_ENABLE           
            //float rayViewShaft = max(dot(viewDir, _SunLightDir), 0);
            //float disToPlane = rayPlaneIntersect(cameraPos, viewDir, float4(0, 1, 0, _FogHeightStart + _FogHeight));
            //if(disToPlane > 0)
            float3 worldPos = ComputeWorldSpacePosition(input.texcoord, rawDepth, UNITY_MATRIX_I_VP);

            float fogFadeSky = fadeSky;
            #if _FADE_SKY
                    worldPos.y = lerp(0, worldPos.y, fadeSky);
                    fogFadeSky = 1;
            #endif  
            float depthFogCoord = max(0, depthForDepthFog * _ProjectionParams.z - _ProjectionParams.y - _DistanceOffset);
            depthFogCoord = 1 - saturate(depthFogCoord * _LinearGrad + _LinearOffs);

            float depthFogFactor = pow(depthFogCoord, _DepthFogFalloff) * _DepthFogDensity;

            float4 fogHighPlane = float4(0, 1, 0, _FogHeightStart + _FogHeight);
            //float3 rayToLight = normalize(worldPos - _MainLightPosition);

            float fogHeightFactor = pow((worldPos.y - _FogHeightStart) / _FogHeight, _HeightFogFalloff);

            float2 fogUV1 = (worldPos.xz + _Time.x * _FogMoveSpeed.xy) * _FogNoiseSize1;
            float fogNoise1 = SAMPLE_TEXTURE2D(_FogNoiseTexture, sampler_FogNoiseTexture, fogUV1).r;

            float2 fogUV2 = (worldPos.xz + _Time.x * _FogMoveSpeed.zw) * _FogNoiseSize2;
            float fogNoise2 = SAMPLE_TEXTURE2D(_FogNoiseTexture, sampler_FogNoiseTexture, fogUV2).r;

            float fogNoise = pow((fogNoise1 * fogNoise2), _FogNoiseContract);
            fogNoise = lerp(1, fogNoise, fadeSky);

            float heightFogFactor = smoothstep(1, 0, fogHeightFactor);
            heightFogFactor = heightFogFactor * _HeightFogDensity;

            fogNoise = lerp(fogNoise, 1, pow(heightFogFactor, _FogNoiseDepthFalloff));

            //float4 fogColor = lerp(_FogColor, _FogSunColor, pow(rayViewShaft, _FogSunFalloff));
            //float2 lutUV = float2(rayViewShaft, 0);
            
            float2 worldUv;
            worldUv.x = (worldPos.x - _FogLightmapPosAndSize.x) / _FogLightmapPosAndSize.z;
            worldUv.y = (worldPos.z - _FogLightmapPosAndSize.y) / _FogLightmapPosAndSize.w;

            float4 fogColor = _FogColor;

            float lightmapAlpha = 1;
#if _FOG_MIX_ADD
            float finalFogFactor = saturate(heightFogFactor + depthFogFactor) * fogNoise;
#elif _FOG_MIX_BLEND
            float finalFogFactor = saturate(lerp(heightFogFactor, depthFogFactor, _FogBlendFactor)) * fogNoise;
#else
            float finalFogFactor = max(0, (heightFogFactor * depthFogFactor) * fogNoise);
#endif

#if _PLAYER_INTERACTIVE
            float disToPlayer = distance(worldPos, _PlayerPosAndRadius.xyz);
            disToPlayer /= _PlayerPosAndRadius.w;
            float distToPlayerFactor = pow(saturate(disToPlayer), _InteractiveFalloff);
            finalFogFactor *= distToPlayerFactor;                
#endif

            half4 decodeInstructions = half4(34.493242, 2.2, 0.0h, 0.0h);
            float4 fogGI = SAMPLE_TEXTURE2D_LIGHTMAP(_FogLightmap, sampler_FogLightmap, worldUv);
            fogGI.rgb = DecodeLightmap(fogGI, decodeInstructions) * 1;
            half4 transformCoords = half4(1, 1, 0, 0);
            //half4 decodeInstructions = half4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0h, 0.0h);
            fogGI.rgb = SampleSingleLightmap(TEXTURE2D_ARGS(_FogLightmap, sampler_FogLightmap), worldUv, transformCoords, true, decodeInstructions);

            //float lightmapDepthFallOff = pow(depthFogCoord, _FogLightmapFalloffMinMax.z);
            //lightmapDepthFallOff = FastRemap(lightmapDepthFallOff, _FogLightmapFalloffMinMax.x, _FogLightmapFalloffMinMax.y);
            float lightmapDepthFallOff = 1 - saturate(depthFogCoord - _FogLightmapFalloffMin.x);
            lightmapDepthFallOff = pow(lightmapDepthFallOff, _FogLightmapFalloffMin.y);
            fogColor.rgb += pow(_FogLightmapColor * fogGI.rgb, _FogLightmapFalloff) * lightmapDepthFallOff;
            float3 sceneColorLerped = lerp(sceneColor.rgb, fogColor.rgb, lightmapAlpha * saturate(fogFadeSky * finalFogFactor * fogColor.a));
            sceneColor.rgb = lerp(lerp(sceneColor.rgb, _SkyColor.rgb, _SkyColor.a), sceneColorLerped, fadeSky);
#endif


#if _RGB_SPLIT_ENABLE

#endif
            sceneColor.a = 1;
            return sceneColor;
        }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ScreenCloud & HeightFog"
        
            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment Frag
            ENDHLSL
        }
    }
}