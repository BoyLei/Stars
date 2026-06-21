Shader "SGAME/SkyBox/SGAME_SkyBox_Boss"
{
    Properties {
        _Tint ("主颜色", Color) = (.5, .5, .5, .5)
        [Gamma] _Exposure ("天空球曝光度", Range(0, 8)) = 1.0
        _Rotation ("天空球旋转角度", Range(0, 360)) = 0
        [NoScaleOffset] _Tex ("Cubemap   (HDR)", Cube) = "grey" {}
        _NoiseDensity("噪声的密度",Range(0,20)) = 10
        _NoiseSpeed("噪声的运动速度(影响银河的运动和星星的闪烁速度)",Range(0,1)) = 0.5
        _UVmovedX("银河在X轴上的偏移程度",Range(0,1)) = 0.1
        _UVmovedY("银河在Y轴上的偏移程度",Range(0,1)) = 0.1
        _UVmovedZ("银河在Z轴上的偏移程度",Range(0,1)) = 0.1
        _StarLighting("星星闪烁的明度",Range(0,2)) = 1
        
       // _StarTex("StarCubeMap(HDR)",Cube) = 
    }

    SubShader {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite On

        Pass {

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

           // #include "UnityCG.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            samplerCUBE _Tex;
            half4 _Tex_HDR;
            half4 _Tint;
            half _Exposure;
            float _Rotation;
            float _NoiseDensity;
            float _NoiseSpeed;
            float _UVmovedX;
            float _UVmovedY;
            float _UVmovedZ;
            float _StarLighting;
            CBUFFER_END
            #ifdef UNITY_COLORSPACE_GAMMA
            #define unity_ColorSpaceDouble half4(2.0, 2.0, 2.0, 2.0)
            #else
            #define unity_ColorSpaceDouble half4(4.59479380, 4.59479380, 4.59479380, 2.0)
            #endif
            
            float3 RotateAroundYInDegrees (float3 vertex, float degrees)
            {
                float alpha = degrees * 3.14 / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }

            struct appdata_t {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };


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
            ///RGB转HSV
            half3 RGBtoHSV(half3 arg1)
            {
                half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                half4 P = lerp(half4(arg1.bg, K.wz), half4(arg1.gb, K.xy), step(arg1.b, arg1.g));
                half4 Q = lerp(half4(P.xyw, arg1.r), half4(arg1.r, P.yzx), step(P.x, arg1.r));
                half D = Q.x - min(Q.w, Q.y);
                half E = 1e-10;
                return half3(abs(Q.z + (Q.w - Q.y) / (6.0 * D + E)), D / (Q.x + E), Q.x);
            }
            ///////////////////////////////////////////////////////
            ///柏林噪声
            float2 randomVec(float2 uv)
            {
	            float vec = dot(uv, float2(127.1, 311.7));
	            return -1.0 + 2.0 * frac(sin(vec) * 43758.5453123);
            }

            float perlinNoise(float2 uv) 
            {				
	            float2 pi = floor(uv);
	            float2 pf = uv - pi;
	            float2 w = pf * pf * (3.0 - 2.0 *  pf);

	            float2 lerp1 = lerp(
		            dot(randomVec(pi + float2(0.0, 0.0)), pf - float2(0.0, 0.0)),
		            dot(randomVec(pi + float2(1.0, 0.0)), pf - float2(1.0, 0.0)), w.x);
                            
 	            float2 lerp2 = lerp(
		            dot(randomVec(pi + float2(0.0, 1.0)), pf - float2(0.0, 1.0)),
		            dot(randomVec(pi + float2(1.0, 1.0)), pf - float2(1.0, 1.0)), w.x);
		            
	            return lerp(lerp1, lerp2, w.y);
            }
            ///////////////////////////////////////////////////////
            ///// Decodes HDR textures
            // handles dLDR, RGBM formats
            inline half3 DecodeHDR(half4 data, half4 decodeInstructions, int colorspaceIsGamma)
            {
                // Take into account texture alpha if decodeInstructions.w is true(the alpha value affects the RGB channels)
                half alpha = decodeInstructions.w * (data.a - 1.0) + 1.0;

                // If Linear mode is not supported we can skip exponent part
                if(colorspaceIsGamma)
                    return (decodeInstructions.x * alpha) * data.rgb;

            #   if defined(UNITY_USE_NATIVE_HDR)
                return decodeInstructions.x * data.rgb; // Multiplier for future HDRI relative to absolute conversion.
            #   else
                return (decodeInstructions.x * pow(alpha, decodeInstructions.y)) * data.rgb;
            #   endif
            }

            // Decodes HDR textures
            // handles dLDR, RGBM formats
            inline half3 DecodeHDR (half4 data, half4 decodeInstructions)
            {
                #if defined(UNITY_COLORSPACE_GAMMA)
                return DecodeHDR(data, decodeInstructions, 1);
                #else
                return DecodeHDR(data, decodeInstructions, 0);
                #endif
            }
            ///////////////////////////////////////////////////////
            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 rotated = RotateAroundYInDegrees(v.vertex.xyz, _Rotation);
                o.vertex = TransformObjectToHClip(rotated);
                o.texcoord = v.vertex.xyz;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 GrandientNoiseUV = i.texcoord + _Time.xxx/((1-_NoiseSpeed)*10);
                float2 BerlinNoiseUV = i.texcoord.yz + _Time.xx/((1-_NoiseSpeed)*1);
                float noise =0;
                //float BerlinNoise = 0;
                Unity_GradientNoise_float(GrandientNoiseUV,1-_NoiseDensity,noise);
               //BerlinNoise = perlinNoise(BerlinNoiseUV);
                half4 tex = texCUBE (_Tex, i.texcoord);
                half3 hsvTex = RGBtoHSV(tex.xyz);
                float milkyWayMovedRange = hsvTex.z *clamp(step(hsvTex.z,0.3)+0.1,0,1) ;//smoothstep(tex.z,0,0.6);
                half4 tex1 = texCUBE (_Tex, i.texcoord + float3((noise - 0.5) * _UVmovedX *noise *milkyWayMovedRange,(noise - 0.5) * _UVmovedY *noise * milkyWayMovedRange,(noise - 0.5) * _UVmovedZ *noise * milkyWayMovedRange));
                
                half3 c = DecodeHDR(tex1,_Tex_HDR);
                
                float ShiningStar = (tex.a * pow(noise,4)) * 3 * _StarLighting;
                c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
                c *= _Exposure;
                //c = float3(ShiningStar,ShiningStar,ShiningStar);
                c += float3 (ShiningStar,ShiningStar,ShiningStar);
                return half4(c, 1);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}

