Shader "Unlit/PerspectiveShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color1 ("Color1", Color) = (1,1,1,0.5)
        _Color2 ("Color2", Color) = (1,1,1,0.5)
        _Width("Width", range(0,0.5)) = 0.1
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue" = "Transparent+1"
            "RenderPipeline" = "UniversalPipeline" 
        }
        LOD 100

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
                "Queue" = "Transparent+1"
            }
            Blend srcalpha oneminussrcalpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Width;
            float4 _Color1;
            float4 _Color2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);

                
                _Color2 += saturate(step(i.uv.x, _Width) + step(1 - _Width, i.uv.x) + step(i.uv.y, _Width) + step(1 - _Width, i.uv.y)) * _Color1;

                return _Color2;
            }
            ENDCG
        }
    }
}
