Shader "Yoka/Stars/RectMask2D"
{
    Properties
    {
        // ...
        // #### required for Mask ####
        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255
        _ColorMask("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
    }

        SubShader
    {
        Tags
        {
            // ...
        }

        // #### required for Mask ####
        Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }
        ColorMask[_ColorMask]
        // ...

        Pass
        {
            CGPROGRAM
            // ...
            // #### required for RectMask2D ####
             #pragma target 3.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #pragma multi_compile  UNITY_UI_CLIP_RECT
            float4 _ClipRect;

        // #### required for Mask ####
        #pragma multi_compile  UNITY_UI_ALPHACLIP

        struct appdata_t
        {
            float4 vertex     : POSITION;
            // ...
        };

        struct v2f
        {
            // ...
            // #### required for RectMask2D ####
            float4 worldPosition    : TEXCOORD1;
        };

        v2f vert(appdata_t v)
        {
            v2f OUT;
            // ...
            // #### required for RectMask2D ####
            OUT.worldPosition = v.vertex;
            return OUT;
        }

        fixed4 frag(v2f IN) : SV_Target
        {
            // ...
            fixed4 color = fixed4(1,1,1,1);
            // #### required for RectMask2D ####
            #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
            #endif

        // #### required for Mask ####
        #ifdef UNITY_UI_ALPHACLIP
            clip(color.a - 0.001);
        #endif

        return color;
        }
    ENDCG
    }
    }
}