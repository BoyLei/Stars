Shader "Unlit/ExportWorldLightmap"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Front

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityShaderVariables.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 lightmapUv : TEXCOORD1;
            };

            struct v2f
            {
                float2 lightmapUv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _TerrainWorldLightmap;
            float4 _LightmapScaleOffset;
            float4 _TerrainWorldInfo;

            v2f vert (appdata v)
            {
                v2f o;                  

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float2 horizontalMapping = (worldPos.xz - _TerrainWorldInfo.xy) / _TerrainWorldInfo.zw;
                o.vertex = float4(horizontalMapping, 0, 1);
                o.lightmapUv = v.lightmapUv * _LightmapScaleOffset.xy + _LightmapScaleOffset.zw;
                return o;
            }

            
            //-------------------------------------------------------------------------------------
            // Decodes HDR textures
            // handles dLDR, RGBM formats
            // Called by DecodeLightmap when UNITY_NO_RGBM is not defined.
            inline half3 _DecodeLightmapRGBM(half4 data, half4 decodeInstructions)
            {
		            return (decodeInstructions.x * pow(data.a, decodeInstructions.y)) * data.rgb;

	            // If Linear mode is not supported we can skip exponent part
	            #if defined(UNITY_COLORSPACE_GAMMA)
		            # if defined(UNITY_FORCE_LINEAR_READ_FOR_RGBM)
		            return (decodeInstructions.x * data.a) * sqrt(data.rgb);
		            # else
		            return (decodeInstructions.x * data.a) * data.rgb;
		            # endif
	            #else
		            return (decodeInstructions.x * pow(data.a, decodeInstructions.y)) * data.rgb;
	            #endif
            }

            // Decodes doubleLDR encoded lightmaps.
            inline half3 _DecodeLightmapDoubleLDR(fixed4 color, half4 decodeInstructions)
            {
	            // decodeInstructions.x contains 2.0 when gamma color space is used or pow(2.0, 2.2) = 4.59 when linear color space is used on mobile platforms
	            return unity_Lightmap_HDR.x * color.rgb;
            }


            inline half3 _DecodeLightmap(fixed4 color, half4 decodeInstructions)
            {
                return _DecodeLightmapRGBM(color, decodeInstructions);
	            #if defined(UNITY_LIGHTMAP_DLDR_ENCODING)
		            return _DecodeLightmapDoubleLDR(color, decodeInstructions);
	            #elif defined(UNITY_LIGHTMAP_RGBM_ENCODING)
		            return _DecodeLightmapRGBM(color, decodeInstructions);
	            #else //defined(UNITY_LIGHTMAP_FULL_HDR)
		            return color.rgb;
	            #endif
            }


            inline half3 _DecodeLightmap(fixed4 color)
            {
                half4 _unity_Lightmap_HDR = half4(34.493242, 2.2, 0, 0);
                //half4 _unity_Lightmap_HDR = half4(pow(5.0, 2.2), 2.2, 0.0, 0.0);
	            return _DecodeLightmap(color, _unity_Lightmap_HDR);
            }


            half3 _SampleLightmap(float2 lightmapUV)
            {
                half4 bakedColorTex = tex2D(_TerrainWorldLightmap, lightmapUV);
                half3 bakedColor = _DecodeLightmap(bakedColorTex);
                return bakedColor;
            }

            float4 frag (v2f i) : SV_Target
            {
                return float4(_SampleLightmap(i.lightmapUv), 1);
            }
            ENDCG
        }
    }
}
