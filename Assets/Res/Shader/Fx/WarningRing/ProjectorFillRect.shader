// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "SGAME/Indicators/FillRect" {
  Properties {
  	[Header(IfOnTop)]
  	[Enum(UnityEngine.Rendering.CompareFunction)]_Ztest("显示在最上层(Always), 默认(LessEqual)", Float) = 4
  	[Header(Offset)]
    _ScaleLeftAndRight ("横向延长长度 (米)", range(-0.5,10)) = 0.0
    _ScaleTop ("前向延长长度(米)", range(0,10)) = 0.0
    //_ScaleBottom ("Scale Bottom (m)", range(0,10)) = 0.0
    _SliceX ("Slice X (m)", Float) = 0.3
    _SliceY ("Slice Y (m)", Float) = 0.3
  	[Space(20)]
    [HDR]_MainColor ("外框颜色", Color) = (1,1,1,1)
    [HDR]_FillColor ("光圈颜色", Color) = (1,1,1,1)
  	[HDR]_FillLineColor ("光圈描边", Color) = (1,1,1,1)
    
  	[Space(20)]
  	_VerticalOffset("纵向边长矫正",Range(0,0.1)) = 0.5
  	_HorizontalOffset("横向边长矫正",Range(0,0.1)) = 0

  	[Space(20)]
  	
  	[KeywordEnum(Back,Middle,Center,Side2Middle)]_Start("光圈运行模式",float) = 2
  	_FillAngle("光圈展开程度",range(0,1)) = 0
    _ProjectTex ("框体贴图", 2D) = "" {}  
  	_NoProjectFigureTex ("非九宫格缩放框体花纹贴图", 2D) = "black" {}   
  	_FigureTex ("内部花纹", 2D) = "" {}
  	
//  	[Space(20)]
//  	[KeywordEnum(On,Off)]_InsideMask("是否开启内环遮罩",float) = 0
//  	_InsideRingRadious("内环半径",Range(0.1,10)) = 0.1

  	
  	
  	
  	_FigureAlpha ("内部花纹透明度", Range(0,1)) = 0.5
  	[HDR]_FigureColor ("内部花纹颜色", Color) = (1,1,1,1) 
  	
  	[HDR]_InsideRing ("内发光颜色", Color) = (0.1,0.1,0.1,1)
//    _Expand ("展开角度", Range (0,1)) = 0
//    _Fill ("光圈大小", Range (0,1.5)) = 0
  }
  
  Subshader {
    Tags 
    {
			"RenderPipeline" = "UniversalPipeline"
			"Queue"="Transparent"
		}
    Pass {
      ZWrite off
      ZTest [_Ztest]
      AlphaTest Greater 0
      ColorMask RGB
      Blend SrcAlpha OneMinusSrcAlpha
      Offset -1, -1
  
      HLSLPROGRAM

      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile_fog
      #pragma shader_feature_local _START_BACK _START_MIDDLE _START_CENTER _START_SIDE2MIDDLE
      
      //#include "UnityCG.cginc"
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			

      #define PI 3.1415926
      #define SqaureRoot2 1.4142
      #define SqaureRoot3 1.7320
      
      CBUFFER_START(UnityPerMaterial)
      float _ScaleLeftAndRight;
      float _ScaleTop;
      float _ScaleBottom;
      float _SliceX;
      float _SliceY;
      
      // half _Expand;
      // half _Fill;
      half4 _MainColor;      
      half4 _FillColor;
      half4 _FillLineColor;
      half _VerticalOffset;
      half _HorizontalOffset;
      half _InsideRingRadious;
      half _Start;
      half _FillAngle;
      half4 _FigureColor;
      half4 _MainTex_ST;
      half _FigureAlpha;
      half4 _InsideRing;
      float4x4 unity_Projector;
      
      //half _InsideMask;
      CBUFFER_END
      
      sampler2D _ProjectTex;
      sampler2D _NoProjectFigureTex;
      sampler2D _FigureTex;
      TEXTURE2D(_CameraDepthTexture);
	  SAMPLER(sampler_CameraDepthTexture);
      
      half gt_than(float x, float y) {
        return max(sign(x - y), 0);
      }

      half ls_than(float x, float y) {
        return max(sign(y - x), 0);
      }
      
      half dist(half2 from, half2 to)
      {
        return sqrt((to.x - from.x) * (to.x - from.x) + (to.y - from.y) * (to.y - from.y));
      }

      struct vInput {
        float4 vertex : POSITION;
        half2 texcoord : TEXCOORD0;        
      };

      struct vOutput {
        float4 uvMain : TEXCOORD0;
      	float4 CenterOS :TEXCOORD1;
        float4 ray :TEXCOORD2;
        float4 pos : SV_POSITION;
        float4 screenPos : TEXCOORD3;
      	float4 posOS : TEXCOORD4;
      	float4 CenterOS2 :TEXCOORD5;
      };

      ///////////////////////////////////////
      float4 NineSliceWithIndependentScale(float4 vertex)
      {
	      // 定义九宫格的边界
        float left = -_SliceX;
        float right = _SliceX;
        float bottom = -_SliceY;
        float top = _SliceY;
       
        // 左边缘
        if (vertex.x < left)
        {
            vertex.x -= _ScaleLeftAndRight/2;
        	vertex.x -= _HorizontalOffset;
        }
        // 右边缘
        else if (vertex.x > right)
        {
            vertex.x += _ScaleLeftAndRight/2;
        	vertex.x += _HorizontalOffset;
        }
       
        // 下边缘
        if (vertex.z < bottom)
        {
            vertex.z -= _ScaleBottom;
        	vertex.z -= _VerticalOffset;
        }
        // 上边缘
        else if (vertex.z > top)
        {
            vertex.z += _ScaleTop;
        	vertex.z += _VerticalOffset;
        }
      	
      	return vertex;
      }

      float2 NineSliceWithIndependentScaleUV(float2 uv)
      {
	       // 获取顶点的UV坐标
	       // float2 uv = v.uv;

	        // 定义九宫格的UV边界
	        float left = _SliceX;
	        float right = 1.0 - _SliceX;
	        float bottom = _SliceY;
	        float top = 1.0 - _SliceY;

	        // 根据UV进行缩放调整
	        if (uv.x < left)
	        {
	            uv.x -= (_ScaleLeftAndRight/2) * (left - uv.x) / left;
	        	
	        }
	        else if (uv.x > right)
	        {
	            uv.x += (_ScaleLeftAndRight/2) * (uv.x - right) / (1.0 - right);
	        }

	        if (uv.y < bottom)
	        {
	            uv.y -= _ScaleBottom * (bottom - uv.y) / bottom;
	        }
	        else if (uv.y > top)
	        {
	            uv.y += _ScaleTop * (uv.y - top) / (1.0 - top);
	        }
      	return uv;
      }

      float3 getProjectedObjectPos(float2 screenPos, float3 worldRay,out float depthOut, float Scale,float offset)
      {
		//get depth from depth texture
		//float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, screenPos);
      	float depth = SAMPLE_TEXTURE2D_LOD(_CameraDepthTexture,sampler_CameraDepthTexture, screenPos,0);
		depthOut = depth;
		depth = Linear01Depth (depth,_ZBufferParams) * _ProjectionParams.z;
		//get a ray thats 1 long on the axis from the camera away (because thats how depth is defined)
		worldRay = normalize(worldRay);
		//the 3rd row of the view matrix has the camera forward vector encoded, so a dot product with that will give the inverse distance in that direction
		worldRay /= dot(worldRay, -UNITY_MATRIX_V[2].xyz);
		//with that reconstruct world and object space positions
		float3 worldPos = _WorldSpaceCameraPos + worldRay * depth;
		float3 objectPos =  mul (unity_WorldToObject, float4(worldPos,1)).xyz;
		//discard pixels where any component is beyond +-0.5
      	objectPos /= Scale;
      	
      	objectPos.xz = (objectPos.xz - half2(0,0))*(1 - offset) + half2(0,0);
		//clip(0.5 - abs(objectPos));
		//get -0.5|0.5 space to 0|1 for nice texture stuff if thats what we want
		objectPos += 0.5;
      	//objectPos.xz = NineSliceWithIndependentScaleUV(objectPos.xz);
      	//objectPos.xz = (objectPos.xz - half2(0.5,0.5))*(1 - offset) + half2(0.5,0.5);
      	
		return objectPos;
      }

      ////////////////////////////////////////////
      vOutput vert (vInput v)
      {
        vOutput o;
      	 // 定义九宫格的边界
      	v.vertex = NineSliceWithIndependentScale(v.vertex);
        o.pos = TransformObjectToHClip(v.vertex);
      	
        o.uvMain.xy = v.texcoord;// mul(unity_Projector, v.vertex);        
        float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
		o.ray.xyz = worldPos - _WorldSpaceCameraPos;
        o.ray.w = ComputeFogFactor(o.pos.z); 
        o.screenPos = ComputeScreenPos (o.pos);
      	//底边左侧CenterWS
      	o.CenterOS.xyz = NineSliceWithIndependentScale(float4(-0.5,0,-0.5,1));
      	o.CenterOS2.xyz = NineSliceWithIndependentScale(float4(0,0,-0.5,1));
		o.posOS.xyz = v.vertex;
      	
        return o;
      }

      half2 RotateUV(half2 uv, float angleDegrees)
	 {
		float angleRadians = radians(angleDegrees);
	    //float angleRadians = Mathf.Deg2Rad * angleDegrees;
	    float cosTheta = cos(angleRadians);
	    float sinTheta = sin(angleRadians);
      	// 旋转矩阵
        float2x2 rotationMatrix = float2x2(
            cosTheta, -sinTheta,
            sinTheta, cosTheta
        );
      	// 应用旋转到UV坐标
      	uv -= 0.5;
        float2 outputUV = mul(rotationMatrix, uv);
      	outputUV += 0.5;
	    return outputUV;
	 }
      
      half4 frag (vOutput i) : SV_Target
      {
      	//获取圆形扩张的比例
      	//half scale = _OutsideRingRadious * 2;
        //获取decal uv
		// float2 screenUv = i.screenPos.xy / i.screenPos.w;
		// float depth = 0 ;
		// float2 uv = getProjectedObjectPos(screenUv, i.ray,depth, scale,_OutsideRingOffset).xz;
      	//把矫正值加到偏移值里去
      	_ScaleTop += 2 * _VerticalOffset;
      	_ScaleLeftAndRight += 2 * _HorizontalOffset;
      	
      	float2 uv = i.uvMain.xy;
      	
        half4 center = half4(0.5, 0.5, i.uvMain.z, i.uvMain.w);
        // half4 up = half4(0.5, 1.0, i.uvMain.z, i.uvMain.w) - center;
        // half4 current = i.uvMain - center;


      	float v =  (i.posOS.z - i.CenterOS.z)/(1+_ScaleTop);
      	float u = (i.posOS.x - i.CenterOS.x)/(1+_ScaleLeftAndRight);
      	half2 newUV = half2(u,v);
      	half2 figureUV = half2(((i.posOS.x - i.CenterOS2.x +0.5 + _ScaleTop/2)/(1+_ScaleTop))  ,v);
      	half2 NonProjectUV = half2(((i.posOS.x - i.CenterOS2.x +0.5 + _ScaleTop/2)/(1+_ScaleTop))  ,v);
        half4 mainTex = tex2D(_ProjectTex, half2(i.uvMain.x,1-i.uvMain.y));
      	half4 NonProjectTex = tex2D(_NoProjectFigureTex, newUV);
      	half4 figureTex = tex2D(_FigureTex,	figureUV);
      	half mask = mainTex.r;
        half4 main = half4(1,1,1,mainTex.a) * _MainColor;
      	
        half visBlit = 1;

        //half mainBlit = gt_than(dist(center, i.uvMain.xy), _Fill * 0.5);

      	
      	half fillBlit = 0;
      	half flowCircleLine  = 0;
      	//由中心向四周展开
      	
      	float2 uvFill = newUV * 2.0 - 1.0; // 将纹理坐标从[0,1]范围转换到[-1,1]范围，中心为(0,0)
      	// 计算当前像素到中心的最大轴向距离
      	#ifdef _START_CENTER
        float maxDistance = max(abs(uvFill.x), abs(uvFill.y));
      	// 使用smoothstep在光圈边界上产生平滑过渡
        float circle = smoothstep(_FillAngle, _FillAngle - 0.01, maxDistance);
      	float circle2 = smoothstep(_FillAngle - 0.01, _FillAngle  - 0.01 - 0.01, maxDistance);
      	fillBlit = circle;
      	flowCircleLine = circle - circle2;
		#elif _START_MIDDLE
		//由中间向两侧展开
      	// 计算到中心的最大轴向距离
        float distanceFromCenter = max(abs(uvFill.x), abs(uvFill.y));
        // 光圈从中间向两侧扩散
        float middleBand = smoothstep(_FillAngle, _FillAngle - 0.01, abs(uvFill.x));
      	float middleBand2 = smoothstep(_FillAngle - 0.01, _FillAngle - 0.01 - 0.01, abs(uvFill.x));
		fillBlit = middleBand;
      	flowCircleLine = middleBand - middleBand2;
		#elif _START_BACK 
      	//由后到前
      	//uvFill = uv;
      	// 计算当前像素的y坐标
        float y = uvFill.y * 0.5 + 0.5;
      	// 使用smoothstep在光圈边界上产生平滑过渡
      	float topScale = 1+_ScaleTop;
        float BackToFrontMask = smoothstep(_FillAngle, _FillAngle - 0.01/topScale, y);
      	float BackToFrontMask2 = smoothstep(_FillAngle  - 0.01/topScale, _FillAngle  - 0.01/topScale  - 0.01/topScale, y);
      	fillBlit = BackToFrontMask;
      	flowCircleLine = BackToFrontMask - BackToFrontMask2;
      	#elif _START_SIDE2MIDDLE
      	//由中间向两侧展开
      	// 计算到中心的最大轴向距离
        float distanceFromCenter = max(abs(uvFill.x), abs(uvFill.y));
      	//FillAngle调整
      	float fillAngle = 1 - _FillAngle;
        // 光圈从中间向两侧扩散
        float middleBand = smoothstep(fillAngle, fillAngle - 0.01, abs(uvFill.x));
      	float middleBand2 = smoothstep(fillAngle - 0.01, fillAngle - 0.01 - 0.01, abs(uvFill.x));
		fillBlit = 1 - middleBand;
      	flowCircleLine = middleBand - middleBand2 ;
      	#endif
      	half fillBlitWithoutMask = fillBlit;
      	fillBlit *=mask;
      	flowCircleLine *= mask;
      	
        half4 res = half4(0, 0, 0, 0);
        res += main /** half4(mainBlit, mainBlit, mainBlit, mainBlit)*/;
      	mainTex.a = saturate(mainTex.a + NonProjectTex.a);
      	//加内发光颜色
      	res.rgb = lerp(_InsideRing.rgb, res.rgb, mainTex.a);
      	//加光圈颜色
        res.rgba =flowCircleLine * _FillLineColor.rgba +  fillBlit * _FillColor.rgba + res.rgba;//lerp(res.rgba,_FillColor,fillBlit);
      	//加中间花纹颜色
      	res.rgb = lerp(res.rgb, _FigureColor, figureTex.a * fillBlit * _FigureAlpha);
        res *= half4(visBlit, visBlit, visBlit, visBlit);
      	
      	//中间花纹
		res.a += _FigureAlpha * figureTex.a * visBlit * fillBlit;

      	//光圈边框的颜色影响边框
      	res.rgb = lerp (res.rgb,_FillLineColor.rgb,fillBlitWithoutMask * mainTex.a);
      	
      	//内发光
      	res.a = res.a  + (mainTex.a + 0.8 *  _InsideRing.a) * visBlit * mainTex.r;

      	res.a = saturate(res.a) ;

        res.rgb = MixFog(res.rgb, i.ray.w);

        return res;
      }
      ENDHLSL
    }
  }
}
