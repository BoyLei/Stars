Shader "SGAME/Screen Rgb Splitter"
{
	Properties
    {
        _MainTex ("Texture", 2D) = "white"
    }

    HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

        float _RgbSplitOffset;
        float _RgbSplitRadius;
        // float _RgbSplitRoundness;
        // float _RgbSplitSmoothness;

        // TEXTURE2D(_EdgeNoiseTexture);
        // SAMPLER(sampler_EdgeNoiseTexture);

        // float4 _EdgeMoveSpeed;
        // float _EdgeNoiseSize1;
        // float _EdgeNoiseSize2;

        // float _EdgeRadius;
        // float _EdgeDistortion;
        // float _EdgeContract;
        // float4 _EdgeBodyColor;
        // float4 _EdgeBorderColor;
        // float _EdgeBorderThreshold;
        // float _EdgeBorderThickness;

        // float _EdgeLightFlowSpeed;
        // float _EdgeLightFlowThickness;
        // float4 _EdgeLightFlowColor;
        // float _EdgeLightFlowThreshold;



        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        float4 _MainTex_TexelSize;

        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
        };


        float sdBox(float2 p, float2 b)
        {
            float2 d = b - abs(p);
            return length(max(d, 0.0)) + min(max(d.x,d.y),0.0);
        }

        float sdRoundBox(float2 position, float2 halfSize, float cornerRadius) 
        {
           position = abs(position) - halfSize + cornerRadius;
           return length(max(position, 0.0)) + min(max(position.x, position.y), 0.0) - cornerRadius;
        }
        
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

        float4 Frag(Varyings input) : SV_Target
        {
            float2 dirToCenter = (0.5 - input.texcoord);
            float2 dist = abs(input.texcoord - 0.5);

            float offset = _RgbSplitOffset * _MainTex_TexelSize.x;
            float2 offsetVal = offset * dirToCenter;

            // float2 edgeNoiseUv1 = (input.texcoord + _Time.x * _EdgeMoveSpeed.xy) * _EdgeNoiseSize1;
            // float edgeNoise1 = SAMPLE_TEXTURE2D(_EdgeNoiseTexture, sampler_EdgeNoiseTexture, edgeNoiseUv1).r;

            // float2 edgeNoiseUv2 = (input.texcoord + _Time.x * _EdgeMoveSpeed.zw) * _EdgeNoiseSize2;
            // float edgeNoise2 = SAMPLE_TEXTURE2D(_EdgeNoiseTexture, sampler_EdgeNoiseTexture, edgeNoiseUv2).r;

            // float edgeNoise = (edgeNoise1 * edgeNoise2);
            // //float edgeNoise = min(edgeNoise1, edgeNoise2);
            // float edgeSdf = saturate(sdRoundBox(2 * input.texcoord - 1, edgeNoise + _EdgeRadius, 0.05));
            // edgeSdf = pow(edgeSdf, _EdgeContract);


            // float edgeBorder = smoothstep(0, _EdgeBorderThreshold, edgeSdf + _EdgeBorderThickness);
            // float edgeBorder2 = smoothstep(0, _EdgeBorderThreshold + _EdgeBorderThreshold, edgeSdf + _EdgeBorderThickness);
            // float edgeBorderValue =  1 - step(edgeBorder, edgeBorder2);

            float r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord + offsetVal).r;
            float4 original = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord);
            float b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord + offsetVal).b;

            float4 splittedColor = float4(r, original.g, b, 1);
            return splittedColor;

            // float2 distortedOffsetVal = offset * dirToCenter;

            // float rDistorted = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord + edgeSdf * (_EdgeDistortion + offset) * dirToCenter).r;
            // float4 originalDistorted = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord + edgeSdf * _EdgeDistortion * dirToCenter);
            // float bDistorted = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord + edgeSdf * (_EdgeDistortion + offset) * dirToCenter).b;

            // float4 distortedColor = float4(rDistorted, originalDistorted.g, bDistorted, 1);
            // distortedColor.rgb *= _EdgeBodyColor.rgb * _EdgeBodyColor.a; 

            // //distortedColor.rgb += edgeBorderValue * _EdgeBorderColor.rgb * _EdgeBorderColor.a; 
            // float4 edgeLightFlow = _EdgeLightFlowColor * edgeSdf * max(0, sin(_EdgeLightFlowThickness * edgeSdf + _EdgeLightFlowSpeed * _Time.x)) * step(edgeSdf, _EdgeLightFlowThreshold);
            // return lerp(splittedColor, distortedColor, edgeSdf) + edgeLightFlow;
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
            Name "Rgb Splitter"
        
            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment Frag
            ENDHLSL
        }
    }
}