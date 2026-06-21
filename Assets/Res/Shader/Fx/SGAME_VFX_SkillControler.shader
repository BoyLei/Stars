Shader "SGAME/FX/SGAME_VFX_SkillControler"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CompareFunction)]_Ztest("Ztest", Float) = 4

        [HideInInspector]_MainTex ("Texture", 2D) = "white" {}
        [HideInInspector]_SDFTex ("Texture",2D) = "white" {}
        [HideInInspector][HDR]_Color ("基础色",Color) = (1,1,1,1)
        [HideInInspector][HDR]_GlowColor ("光圈颜色",Color) = (1,1,1,1)
        [HideInInspector][HDR]_GlowEndColor("光圈结束颜色",Color) = (1,1,1,1)
        [HideInInspector]_Range("控制大小的参数",range(0,2)) = 0
        [HideInInspector]_Length("控制长短的参数",range(0,2)) = 0
        [HideInInspector]_LengthAdjust("调整边长的参数",range(-0.5,0.5)) = 0
        [HideInInspector]_BorderWidth("边框的粗细,影响glowRange=1时光圈到达的位置",range(0,0.4)) = 0
        
        [HideInInspector]_RingAlpha("边框的透明度",range(0,1)) = 1
        [HideInInspector]_GlowRange("控制光晕的扩散参数",range(0,1.5)) = 0
        [HideInInspector]_GlowBeign("调整光晕的起始位置",range(0,1)) = 0
        [HideInInspector]_Flag("控制指示器模式的参数",int) = 0
        [HideInInspector]_Show("控制显示边框还是光圈的参数",int) = 0
        [HideInInspector]_GlowMode("光圈的移动方向",int) = 0
        [HideInInspector]_InsideLightColor("边框内发光颜色",Color) = (1,1,1,1)
        [HideInInspector]_InsideLightAlpha("边框内发光透明度",range(0,3)) = 1
        
        [HideInInspector]_Angle("扇形角度",Range(0,360)) = 60
        [HideInInspector]_Outline("外边的宽度",Range(0,5)) = 1
        [HideInInspector]_MiddleRingSize("中心圆圈的大小",range(0,0.5))= 0
        //        float _Sector = 1;
//                    float _Angle = 40;
//                    float _Outline = 1;
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent+0" "IsEmissive" = "true"  }
        LOD 100
        Cull Back
		ZWrite off
		//ZTest on
        //ZTest Always //永远在最上方
        ZTest [_Ztest]
		Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #pragma shader_feature_local _RING _MIDDLE            //判断是中心还是边框
            #pragma shader_feature_local _RECT _CIRCULAR _SECTOR _ARROW     //判断是方形 圆形 扇形 还是箭头
            #pragma shader_feature_local _FROMMIDDLE _FROMSIDE       //光晕是从中心朝四周扩散 开始从一侧向另一侧扩散

           // #include "UnityCG.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 color : TEXCOORD1;
                float3 originalPos :TEXCOORD2;
                float2 uv2 : TEXCOORD3;
            };

            //sampler2D _MainTex;
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_SDFTex);
            SAMPLER(sampler_SDFTex);
            
            CBUFFER_START(UnityPerMaterial)
            //SAMPLE_TEXTURE2D()
            float4 _MainTex_ST;
            float4 _Color;
            float4 _GlowColor;
            float4 _GlowEndColor;
            float _Range;
            float _Length;
            float _LengthAdjust;
            float _BorderWidth;
            float _GlowRange;
            float _GlowBeign;
            float _RingAlpha;
            float _Angle;
            float _Outline;
            float _MiddleRingSize;
            float4 _InsideLightColor;
            float _InsideLightAlpha;

            int _Flag;
            int _Show;
            int _GlowMode;
            CBUFFER_END
            
            v2f vert (appdata v)
            {
                v2f o;
                float4 vertexMoved = v.vertex;
                float vertexXdir = vertexMoved.x/abs(vertexMoved.x);
                float3 dirToOriginalPoint = normalize((v.vertex - float3(0,0,0)));
                half4 col = _MainTex.SampleLevel(sampler_MainTex,v.uv,0);
                //vertexMoved.xyz = dirToOriginalPoint*0.01*_Range * col.a + vertexMoved.xyz;
                //如果模型是圆形 则中心的原点不可移动
                half ifMove =1;// clamp(step(0.001,distance(v.vertex, float3(0,0,0))) - 0.1,0,1);
                //ifMove = distance(v.vertex, float3(0,0,0))>0.001?1:0;
                #ifdef _RECT
                #ifdef _MIDDLE
                    vertexMoved.x = normalize(vertexMoved.x) * 1 * (_Range/* - 1*/)  + vertexMoved.x;
                    //vertexMoved.y = normalize(vertexMoved.y) * 1 * (_Range - 1)  + vertexMoved.y;
                    vertexMoved.z = normalize(vertexMoved.z) * 1 * (_Range /*- 1*/)  + vertexMoved.z;
                    //方形框的中心得朝一个方向移动
                    vertexMoved.x = abs(normalize(vertexXdir))*0.5 * (_Length - 1) * ifMove  + vertexMoved.x;
                #elif _RING
                    //把拉长后的本地坐标当作uv值传递
                    float2 uv2 = float2(0,0);
                    //vertexMoved.x = normalize(vertexMoved.x/(abs(vertexMoved.x+0.00001))) * 1 * (_Range /*- 1*/)  + vertexMoved.x;
                    //vertexMoved.y = normalize(vertexMoved.y/(abs(vertexMoved.y+0.00001))) * 1 * (_Range - 1)  + vertexMoved.y;
                    vertexMoved.z = normalize(vertexMoved.z/(abs(vertexMoved.z+0.00001))) * 1 * ((_Range-1) /2) + vertexMoved.z + normalize(vertexMoved.z/(abs(vertexMoved.z+0.00001))) * _LengthAdjust ;
                    //方形框只能向一个方向扩展
                    ifMove = step( 0,vertexMoved.x);
                    vertexMoved.x = normalize(vertexXdir) * 1 * ((_Length-1) /*- 1*/) * ifMove  + vertexMoved.x +normalize(vertexXdir) *  _LengthAdjust ;
                    uv2.x= vertexMoved.x ;
                    uv2.y =vertexMoved.z + ((_Range-1)/2);
                #endif
                
                #elif  _CIRCULAR
                ifMove = clamp(step(0.001,distance(v.vertex, float3(0,0,0))) - 0.1,0,1);
                ifMove = distance(v.vertex, float3(0,0,0))>0.001?1:0;
                float uvToCenterDist = distance(v.uv2,float2(0.5,0.5));
                vertexMoved.xz = dirToOriginalPoint.xz * 1 * (_Range - 0.5) * ifMove *(uvToCenterDist/0.5)  /** distance(v.vertex, float3(0,0,0))/0.5*/+ vertexMoved.xz;
                vertexMoved.xz += dirToOriginalPoint.xz * 1 * (_Range) * ifMove *(uvToCenterDist/0.5) * _LengthAdjust/0.5;//dirToOriginalPoint.xz * _LengthAdjust *(uvToCenterDist/0.5) ; //调整框体大小的补偿参数
                #elif _SECTOR
                // ifMove = clamp(step(0.001,distance(v.vertex, float3(0,0,0))) - 0.1,0,1);
                // ifMove = distance(v.vertex, float3(0,0,0))>0.001?1:0;
                // vertexMoved.xy = dirToOriginalPoint * 0.01 * _Range  * ifMove   + vertexMoved.xy;
                
                vertexMoved.x = normalize(vertexMoved.x) * 1 * (_Range-0.5) + vertexMoved.x + normalize(vertexMoved.x) * 1 * (_Range)* _LengthAdjust/0.5;
                //vertexMoved.y = normalize(vertexMoved.y) * 1 * (_Range-0.5)  + vertexMoved.y;
                vertexMoved.z = normalize(vertexMoved.z) * 1 * (_Range-0.5)  + vertexMoved.z + normalize(vertexMoved.z)* 1 * (_Range)* _LengthAdjust/0.5;

                #elif _ARROW
                //如果当前指示器为箭头
                float distanceToOriginal = distance(v.vertex, float3(0,0,0));
                int ifmove = distanceToOriginal>0.3?1:0;
                int ifmove2 = distanceToOriginal<0.8?1:0;
                float3 LengthDir = float3(1,0,0);
                float3 WidthDir = float3(0,1,0);
                vertexMoved.z = -1 * (_Range - 1) * ifmove + (_Length - 0.2) * 2 * ifmove2 * ifmove + 0.03* ifmove2 * ifmove+ vertexMoved.z;
                vertexMoved.x = (vertexMoved.x/abs(vertexMoved.x)) * (_Length/2 - 0.2)  + vertexMoved.x;
                #endif
                o.vertex = TransformObjectToHClip(vertexMoved );
                //float2 uvMoved = v.uv + -1 * dirToOriginalPnanoint *0.1*_Range;
                o.uv = v.uv;
                o.uv2 = v.uv2;
                //UNITY_TRANSFER_FOG(o,o.vertex);
                o.color = normalize((v.vertex - float3(0,0,0)));
                o.color.xy = ifMove;
                o.color.z =abs(v.vertex.x) /*distance(v.vertex, float3(0,0,0))*/;
                o.originalPos = TransformObjectToHClip(float3(0,0,0));

                #ifdef _RECT
                #ifdef _RING
                    o.uv2 = uv2;
                #endif
                #endif
                
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
               
                // sample the texture
                half2 modifiedUV = half2(i.uv/**_Range + 0.5*(1-_Range)*/);
                modifiedUV = clamp(modifiedUV,0,1);
                half4 col = half4(0,0,0,0);
                float4 glowColor = lerp(_GlowColor,_GlowEndColor,_GlowRange);
                glowColor.a = clamp(0,1,glowColor.a);
                #ifdef _SECTOR
                    //扇形内环缩放功能
                    float distanceToMiddleUV = distance(modifiedUV,float2(0.5,0.5));
                    float ifMoveUV =1- step(0.4,distanceToMiddleUV);
                    float2 move =normalize(float2(0.5,0.5) - modifiedUV)*clamp((_MiddleRingSize-0.2)/2,0,0.295);
                    float moveDistance = distance(move,float2(0,0));
                    float distanceToMiddle = distance(modifiedUV,float2(0.5,0.5));
                    
                    //ifMove *= 1- step(0.4,distanceToMiddleUV);
                    modifiedUV = clamp(move*ifMoveUV +modifiedUV,0,1);
                    modifiedUV *= saturate(distanceToMiddle-moveDistance)/(abs(distanceToMiddle-moveDistance+0.0000001));
                    //return float4(modifiedUV.x,modifiedUV.y,0,1);
                    float mask2 = step((_MiddleRingSize-0.18)/2,distanceToMiddleUV); //为了屏蔽中心圆带来的遮罩
                    float mask3 = 1 - step((_MiddleRingSize +0.3 - 0.2)/2,distanceToMiddleUV);
                    float4 colTex = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, modifiedUV);
                    //return float4(colTex.rgb *mask2,1);
                    colTex *= mask2;
                    //扇形展开功能
                    float2 centerUV = (i.uv2 * 2 - 1);
                    float atan2UV = 1-abs(atan2(centerUV.g, centerUV.r)/3.14);
                    half sector =  1.0 - ceil(atan2UV - clamp(_Angle - _Outline-0.02,0,360)*0.002777778 +0.0005);
                
                    half sectorBig = 1.0 - ceil(atan2UV - clamp((_Angle -0.02),0,360) * 0.002777778+0.0005);
                    half outline = (sectorBig -sector) * _RingAlpha/** mainTex.g */;
                
                    float Sdf = 0;
                    float _FlowFade = 0.2;
                    float2 centerUV2 = i.uv*2-1;
                    //_GlowRange = _GlowRange * 0.52;
                    half disToCenture = distance(i.uv,half2(0.5,0.5))* (1 + _LengthAdjust)*(1 + _BorderWidth);
                    //return float4(disToCentre,disToCentre,disToCentre,1);
                    //光圈的启始区域
                    _GlowRange = (_GlowRange + _GlowBeign )/(1 + _GlowBeign);
                    half flowCircleInner = smoothstep(_GlowRange * 0.5 - _FlowFade, _GlowRange * 0.5, disToCenture);
                    half flowCircleMask = step(disToCenture, _GlowRange * 0.5);
                    Sdf = flowCircleInner* flowCircleMask * (colTex.b) ;
                    //Sdf = clamp(Sdf,0,1) * colTex.b;
                    //return half4(Sdf,Sdf,Sdf,1);
                
                    col.rgb = Sdf * (1 - colTex.r) * glowColor.rgb + (colTex.r+colTex.g + outline * colTex.b) * _Color.rgb;
                    //内发光效果
                    half sector2 =  0.7 * saturate(atan2UV - clamp((_Angle-15) - _Outline-0.05,0,360)*0.002777778 +0.0005);
                    float insideLight = max(colTex.w, sector2*10 ) * (1-colTex.x)* colTex.b ;
                    //return float4(insideLight,insideLight,insideLight,1);
                    col.rgb +=insideLight.xxx * 2 * _InsideLightColor.xyz ;
                    col.a = Sdf * (1 - colTex.r) * glowColor.a + (colTex.r+colTex.g) * _Color.a * _RingAlpha + insideLight.x * 2  * _InsideLightAlpha ;
                    col.a = clamp((col.a + outline * colTex.b)*sectorBig,0,1);
                
                    //col.a = clamp(col.a,0,1);
                    //col.a = 1;
                #endif
                 
                #ifdef _RING
                    #ifdef _RECT
                        float4 colTex = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, modifiedUV);
                        //col.a = col.r;
                        float2 uvSDF = float2((i.uv2.x+0.5)/_Length,(i.uv2.y+0.5)/_Range);
                        float4 sdfTex = SAMPLE_TEXTURE2D(_SDFTex,sampler_SDFTex, uvSDF);
                        float2 UV = i.uv - float2(0.5,0.5);
                        float Sdf =  length(max(abs(UV) ,float2(0,0)))/5;
                
                        #ifdef _FROMMIDDLE
                            //Sdf = (1 - smoothstep(_GlowRange-0.05,_GlowRange,1-sdfTex))* clamp(pow(Sdf,_GlowRange*1.5),0,0.4) ;
                            //Sdf = (1- step(_GlowRange*2,1-sdfTex) )* smoothstep(clamp(_GlowRange*2 - 0.8,0,1), clamp(_GlowRange*2,0,1), length(uvSDF*2-1)/2);
                            _GlowRange = _GlowRange *0.53;//调整参数 让_GlowRange为1时光圈刚好消失
                            Sdf = (1- smoothstep(_GlowRange*2-0.05,_GlowRange*2,1-sdfTex))* smoothstep(clamp(_GlowRange*2 - 0.8,0,1), clamp(_GlowRange*2,0,1), length(uvSDF*2-1)/2);
                
                        #elif _FROMSIDE
                            //float uvSDF =  (1 - abs(i.uv.x));
                           // Sdf = (1 - smoothstep(_GlowRange-0.001,_GlowRange,uvSDF)) * clamp(uvSDF +0.5 - _GlowRange,0,1)/** clamp(pow(uvSDF,_GlowRange*6),0,0.4)*/ ;
                            half sdfGlowRange = _GlowRange;
                            Sdf =  (1 - step(sdfGlowRange,1-i.uv.x)) * smoothstep(sdfGlowRange - 0.5,sdfGlowRange,(1 - i.uv.x)/1.2);
                        #endif
                        Sdf = clamp(Sdf,0,1) * colTex.b;
                        //return half4(Sdf,Sdf,Sdf,1);
                        col.rgb = Sdf * (1 - colTex.r) * glowColor.rgb + colTex.r * _Color.rgb;
                        //内发光效果
                        float insideLight = colTex.w * (1-colTex.x);
                        col.rgb +=insideLight.xxx * _InsideLightColor.xyz;
                        col.a = Sdf * (1 - colTex.r) * glowColor.a + colTex.r * _Color.a * _RingAlpha + insideLight.x * _InsideLightAlpha;
                        col.a = clamp(col.a,0,1);
                        //col.a = clamp(col.a,0,1)
                    #elif _CIRCULAR
                        float4 colTex = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, modifiedUV);
                        float2 UV = i.uv - float2(0.5,0.5);
                
                        float Sdf = 0;
                        float _FlowFade = 0.2;
                        _GlowRange = _GlowRange; //让光圈在_GlowRange=1的时候结束
                
                        float2 centerUV = i.uv*2-1;
                        half disToCenture = distance(i.uv,half2(0.5,0.5)) * (1 + _LengthAdjust)*(1 + _BorderWidth);
                        half flowCircleInner = smoothstep(_GlowRange * 0.5 - _FlowFade, _GlowRange * 0.5, disToCenture);
                        half flowCircleMask = step(disToCenture, _GlowRange * 0.5 )/* * (1 - step(length(centerUV), _GlowRange))*/;
                        //return float4(flowCircleMask,flowCircleMask,flowCircleMask,1);
                        Sdf = flowCircleInner* flowCircleMask;
                
                        //return float4(Sdf,Sdf,Sdf,1);
                        Sdf = clamp(Sdf,0,1) * colTex.b;
                        col.rgb = Sdf * (1 - colTex.r) * glowColor.rgb + colTex.r * _Color.rgb;
                        //内发光效果
                        float insideLight = colTex.w * (1-colTex.x);
                        col.rgb +=insideLight.xxx * 3 * _InsideLightColor.xyz;
                        col.a = Sdf * (1 - colTex.r) * glowColor.a + colTex.r * _Color.a * _RingAlpha + insideLight.x*3 * _InsideLightAlpha;
                        //return float4(col.a,col.a,col.a,1);
                        col.a = saturate(col.a);
                    #elif _ARROW
                        float4 mainTex = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv);
                        col.rgb =  _Color.rgb + mainTex.g * _Color.rgb;
                        col.a = clamp(mainTex.r * _Color.a * _RingAlpha + mainTex.g*_InsideLightAlpha*4,0,1);
                        
                        //float insideLight = clamp(0.4+mainTex.r,0,1) -0.4;
                        // col.rgb = float3(col.a,col.a,col.a);
                        // col.a = 1;
                    #endif
                
                #elif _MIDDLE
                    float4 mainTex = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, modifiedUV);
                    col.rgb = mainTex.g * _Color.rgb;
                    col.a = mainTex.g * _Color.a * _RingAlpha; 
                #endif
                return col;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
    CustomEditor "SkillControlerGUI"
}
