Shader "SGAME/Common/SGAME_Common_ZwriteIn"
{
    Properties
    {
       [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
	{
		LOD 0
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Trasparent" "Queue"="Transparent"  }
		Cull Back
		Zwrite on

		HLSLINCLUDE
		#pragma target 3.5
		#pragma prefer_hlslcc gles
		
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
		
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForwardOnly" }

			Blend SrcAlpha OneMinusSrcAlpha/*, One Zero*/
			ZWrite on
			//ZTest LEqual
			//Offset 0 , 0
			ColorMask RGBA

			HLSLPROGRAM

			#pragma multi_compile_instancing
			#define ASE_SRP_VERSION 120109
			
			//#pragma multi_compile _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3

			//#pragma multi_compile _ LIGHTMAP_ON
			//#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			//#pragma shader_feature _ _SAMPLE_GI
			//#pragma multi_compile _ DEBUG_DISPLAY
			//#pragma multi_compile_fog

			#pragma vertex vert
			#pragma fragment frag
			
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"

			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_POSITION


			struct VertexInput
			{
				float4 vertex : POSITION;
				// float3 ase_normal : NORMAL;
				// float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			
			
			VertexOutput vert ( VertexInput v )
			{
				VertexOutput o = (VertexOutput) 0 ;
				o.clipPos = TransformObjectToHClip(v.vertex);
				return o;
			}
			

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );
				return half4( float3(0,0,0), 0 );
			}
			ENDHLSL
		}
	}
}
