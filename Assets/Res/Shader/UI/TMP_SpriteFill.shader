
Shader "Custom/SprFill"
{
    Properties
    {
        [Toggle] _Clockwise (" «∑ÒÀ≥ ±’Î?", Float) = 0

        _MainColor("Color",Color) = (1,0,0,1)
        [PerRendererData] _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Angle("Ω«∂»",Range(0,360)) = 0

    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #pragma shader_feature  _CLOCKWISE_ON

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _MainColor;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            half _Angle;
            fixed _Gradient;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord);
                half2 cuv = i.texcoord - half2(0.5, 0.5);;
                half2 luv = half2(1, 0);


                half2 s = cuv.x * luv.y - luv.x * cuv.y;
                half2 c = cuv.x * luv.x + cuv.y * luv.y;

                half2 angle = 0;
                #if _CLOCKWISE_ON
                angle = atan2(s, c) * (180 / 3.1416);
                #else
                angle = atan2(s, -c) * (180 / 3.1416);
                #endif

                angle += step(0, cuv.y) * 360;

                clip(_Angle - angle);


                return col * _MainColor;
            }
            ENDCG
        }
    }

}
