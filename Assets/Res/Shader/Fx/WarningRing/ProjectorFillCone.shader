// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "SGAME/Indicators/FillCone" {
  Properties {
  	[Header(IfOnTop)]
  	[Enum(UnityEngine.Rendering.CompareFunction)]_Ztest("显示在最上层(Always), 默认(LessEqual)", Float) = 4
	[Space(20)]
  	[KeywordEnum(On,Off)]_Decal("是否开启贴花模式(贴花模型需要用立体的)",float) = 0
    [Space(20)]
    [HDR]_MainColor ("外框颜色", Color) = (1,1,1,1)
    [HDR]_FillColor ("光圈颜色", Color) = (1,1,1,1)
  	[HDR]_FillLineColor ("光圈描边", Color) = (1,1,1,1)
    
  	[Space(20)]
  	_OutsideRingRadious("外圈半径",Range(0.5,10)) = 0.5
  	_OutsideRingOffset("外圈半径矫正",Range(0,0.1)) = 0

  	[Space(20)]
  	
  	[KeywordEnum(Left,Right,Middle,Middle2Side,Side2Middle)]_Start("光圈运行模式",float) = 2
  	_FillAngle("光圈展开程度",range(0,1)) = 0
    _ProjectTex ("框体贴图", 2D) = "Black" {}   
  	_FigureTex ("内部花纹", 2D) = "Black" {}
  	
  	[Space(20)]
  	[KeywordEnum(On,Off)]_InsideMask("是否开启内环遮罩",float) = 0
  	_InsideRingRadious("内环半径",Range(0.1,10)) = 0.1

  	
  	
  	
  	_FigureAlpha ("内部花纹透明度", Range(0,1)) = 0.5
  	[HDR]_FigureColor ("内部花纹颜色", Color) = (1,1,1,1) 
  	
  	[HDR]_InsideRing ("内部深色底色", Color) = (0.1,0.1,0.1,1)
    _Expand ("展开角度", Range (0,1)) = 0
  }
  
  Subshader {
    Tags 
    {
			"RenderPipeline" = "UniversalPipeline"
			"Queue"="Transparent"
		}
    Pass {
      ZTest [_Ztest]
      ZWrite Off
      AlphaTest Greater 0
      ColorMask RGB
      Blend SrcAlpha OneMinusSrcAlpha
      Offset -1, -1
  
      HLSLPROGRAM

      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile_fog
      #pragma multi_compile _ _START_LEFT _START_RIGHT _START_MIDDLE _START_MIDDLE2SIDE _START_SIDE2MIDDLE
      #pragma multi_compile _ _INSIDEMASK_ON
      #pragma multi_compile _ _DECAL_OFF
      
      //#include "UnityCG.cginc"
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			

      #define PI 3.1415926
      #define SqaureRoot2 1.4142
      #define SqaureRoot3 1.7320
      
      CBUFFER_START(UnityPerMaterial)
      half _Expand;
      half4 _MainColor;      
      half4 _FillColor;
      half4 _FillLineColor;
      half _OutsideRingRadious;
      half _OutsideRingOffset;
      half _InsideRingRadious;
      half _Start;
      half _FillAngle;
      half4 _FigureColor;
      half4 _MainTex_ST;
      half _FigureAlpha;
      half4 _InsideRing;
      float4x4 unity_Projector;
      sampler2D _ProjectTex;
      sampler2D _FigureTex;
      half _InsideMask;
      CBUFFER_END

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
      	float4 CenterPosWS :TEXCOORD1;
        float4 ray :TEXCOORD2;
        float4 pos : SV_POSITION;
        float4 screenPos : TEXCOORD3;
      };

      vOutput vert (vInput v)
      {
        vOutput o;
      	///外框进行缩放
      	//外框半径
      	half outSideRing =  _OutsideRingRadious;
      	v.vertex.xz += half2(v.vertex.xz) * (outSideRing - 0.5) * SqaureRoot2 * SqaureRoot2 + _OutsideRingOffset* (outSideRing)* half2(v.vertex.xz/distance(v.vertex.xz,half2(0,0))); //dirFromCenter.xz * (outSideRing - 0.5);
      	///
        o.pos = TransformObjectToHClip(v.vertex);
        o.uvMain.xy = v.texcoord;// mul(unity_Projector, v.vertex);        
        float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
		o.ray.xyz = worldPos - _WorldSpaceCameraPos;
        o.ray.w = ComputeFogFactor(o.pos.z); 
        o.screenPos = ComputeScreenPos (o.pos);
        return o;
      }

      float3 getProjectedObjectPos(float2 screenPos, float3 worldRay,out float depthOut, float Scale,float offset)
      {
		//get depth from depth texture
		float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, screenPos);
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
		clip(0.5 - abs(objectPos));
		//get -0.5|0.5 space to 0|1 for nice texture stuff if thats what we want
		objectPos += 0.5;
      	//objectPos.xz = (objectPos.xz - half2(0.5,0.5))*(1 - offset) + half2(0.5,0.5);
		return objectPos;
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
      	half scale = _OutsideRingRadious * 2;
        //获取decal uv
		float2 screenUv = i.screenPos.xy / i.screenPos.w;
		float depth = 0 ;

      	#ifdef _DECAL_OFF
      	float2 uv =i.uvMain.xy;
      	#else
      	float2 uv = getProjectedObjectPos(screenUv, i.ray,depth, scale,_OutsideRingOffset).xz;
      	#endif
      	
      	i.uvMain.xy = uv;
      	//return half4(i.uvMain.xy,0,1);
        half4 center = half4(0.5, 0.5, i.uvMain.z, i.uvMain.w);
        half4 up = half4(0.5, 1.0, i.uvMain.z, i.uvMain.w) - center;
        half4 current = i.uvMain - center;
        float currentAngle = acos(dot(up, current) / (length(up) * length(current))) * (180 / PI);
        float expandAngle = (_Expand + 0.01) * 180;


        half4 mainTex = tex2D(_ProjectTex, (i.uvMain));
      	half4 figureTex = tex2D(_FigureTex, (i.uvMain));

      	half mask = mainTex.r;
        half4 main = half4(1,1,1,mainTex.a) * _MainColor;
      	
      	//扇形范围遮罩
        half visBlit = 0;
        //visBlit += gt_than(expandAngle, currentAngle);
      	half4 VisAngle =  expandAngle * 2 ;
      	float2 uvVis = RotateUV(i.uvMain.xy, 90);
      	float2 centerUV1 = (uvVis * 2 - 1);
      	float atan2UV1 = 1-abs(atan2(centerUV1.g, centerUV1.r)/3.14);
		visBlit = (1.0 - ceil(atan2UV1 - clamp(VisAngle - 1-0.02,0,360)*0.002777778 +0.0005));

      	#ifdef _INSIDEMASK_ON
      	//内环遮罩
      	half insideRingRadious = _InsideRingRadious;
      	half outsideRingRadious = _OutsideRingRadious;
      	half distanceToCenter = distance(i.uvMain.xy,center.xy) * scale;
      	half insideRingMask = step(distanceToCenter,insideRingRadious);
      	float insideMask =1 - insideRingMask; //insideTex.r * visBlit;
      	float insideRing =(step(distanceToCenter,insideRingRadious + 0.003 * scale) - insideRingMask) * visBlit; //insideTex.a * visBlit;
		#endif
      	
      	
      	half fillBlit = 0;
      	half flowCircleLine  = 0;
      	half fillBlitWithoutMask = 0;
      	#ifdef _START_MIDDLE
      	//中央光圈
      	half fillRange = _FillAngle/2;
      	#ifdef _INSIDEMASK_ON
      	fillRange = (_FillAngle) * (1 -(_InsideRingRadious)/_OutsideRingRadious) + _InsideRingRadious/_OutsideRingRadious;
      	fillRange *= 0.5;
      	#endif
      	half distanceToCenter2 = distance(i.uvMain.xy, center.xy);
      	half flowCircleMask = step(distanceToCenter2,fillRange);
      	flowCircleLine = flowCircleMask - step(distanceToCenter2 + 0.003, fillRange) ;

      	fillBlitWithoutMask = flowCircleMask;
      	fillBlit = flowCircleMask * mask;
      	flowCircleLine = flowCircleLine * mask;
      	
		#elif _START_LEFT
      	//左至右光圈
      	// 将UV坐标转换为 -1 到 1 范围内的坐标
        float2 uvFill = i.uvMain * 2.0 - 1.0;
      	// 对UV坐标进行顺时针旋转90度
        float2 rotatedUV;
        rotatedUV.x = uvFill.y;
        rotatedUV.y = -uvFill.x;
      	// 计算当前UV坐标点与原点的距离和角度
        float dist = length(rotatedUV);
        float angle = atan2(rotatedUV.y, rotatedUV.x) * 57.2958; // 将弧度转换为角度
      	// 计算光圈位置的角度
      	float expandAngle2 = 360 -  (1 - (_Expand + 0.01) ) * 360;
        float arcAngle = lerp(-expandAngle2 * 0.5, expandAngle2 * 0.5, 1 -_FillAngle);
      	fillBlit =1;
      	half fillBlit2 =1;
        // 判断当前点是否在扇形内
        if (/*dist < _Radius &&*/ angle > -expandAngle2 * 0.5 && angle < arcAngle)
        {
            fillBlit =0;
        }
      	arcAngle += 1; 
      	if (/*dist < _Radius &&*/ angle > -expandAngle2 * 0.5 && angle < arcAngle)
        {
            fillBlit2 =0;
        }
      	flowCircleLine = (fillBlit-fillBlit2) * visBlit * mask;
      	fillBlitWithoutMask = fillBlit;
      	fillBlit *= visBlit * mask;
      	
      	 #elif _START_RIGHT
       	//右至左光圈
		// 将UV坐标转换为 -1 到 1 范围内的坐标
        float2 uvFill = i.uvMain * 2.0 - 1.0;
      	// 对UV坐标进行顺时针旋转90度
        float2 rotatedUV;
        rotatedUV.x = uvFill.y;
        rotatedUV.y = -uvFill.x;
      	// 计算当前UV坐标点与原点的距离和角度
        float dist = length(rotatedUV);
        float angle = atan2(rotatedUV.y, rotatedUV.x) * 57.2958; // 将弧度转换为角度
      	// 计算光圈位置的角度
      	float expandAngle2 = 360 -  (1 - (_Expand + 0.01) ) * 360;
        float arcAngle = lerp(-expandAngle2 * 0.5, expandAngle2 * 0.5, _FillAngle);
      	fillBlit =0;
      	half fillBlit2 =0;
        // 判断当前点是否在扇形内
        if (/*dist < _Radius &&*/ angle > -expandAngle2 * 0.5 && angle < arcAngle)
        {
            fillBlit =1;
        }
      	arcAngle += 1; 
      	if (/*dist < _Radius &&*/ angle > -expandAngle2 * 0.5 && angle < arcAngle)
        {
            fillBlit2 =1;
        }
      	flowCircleLine = (fillBlit2-fillBlit) * visBlit * mask;
      	fillBlitWithoutMask = fillBlit;
      	fillBlit *= visBlit * mask;

		#elif _START_MIDDLE2SIDE
      	//从中央到两侧的光圈
      	float2 rotateUV = RotateUV(i.uvMain, 90);
      	float2 relativeUV = rotateUV - float2(0.5,0.5);
      	float relativeAngle = atan2(relativeUV.y, relativeUV.x);
      	float angleInDegress = degrees(relativeAngle);
      	if (angleInDegress < 0)
      		angleInDegress += 360;
      	float halfFanAngle = (_Expand * 360 * 0.5 + 1) * ((_FillAngle)*1.05 - 0.05);
      	fillBlitWithoutMask = step(180 - halfFanAngle,angleInDegress) * step(angleInDegress,180 + halfFanAngle);
      	fillBlit = fillBlitWithoutMask * mask;
      	flowCircleLine = step(180 - halfFanAngle,angleInDegress + 1) * step(angleInDegress - 1,180 + halfFanAngle) - fillBlitWithoutMask;
		flowCircleLine *= mask;

      	#elif _START_SIDE2MIDDLE
      //从两侧到中央的光圈
	    float2 rotateUV = RotateUV(i.uvMain, 90);
	    float2 relativeUV = rotateUV - float2(0.5,0.5);
	    float relativeAngle = atan2(relativeUV.y, relativeUV.x);
	    float angleInDegress = degrees(relativeAngle);
	    if (angleInDegress < 0)
      		angleInDegress += 360;
	    float halfFanAngle = (_Expand * 360 * 0.5 + 1) * ((1 - _FillAngle)*1.05 - 0.05);
      	fillBlitWithoutMask =  1 - step(180 - halfFanAngle,angleInDegress) * step(angleInDegress,180 + halfFanAngle);
	    fillBlit =  fillBlitWithoutMask * mask;
	    flowCircleLine = step(180 - halfFanAngle,angleInDegress + 1) * step(angleInDegress - 1,180 + halfFanAngle) - (1 -fillBlitWithoutMask) ;
      	flowCircleLine *= mask;
      	#endif

      	
        half4 res = half4(0, 0, 0, 0);
        res += main /** half4(mainBlit, mainBlit, mainBlit, mainBlit)*/;
      	//加内发光颜色
      	res.rgb = lerp(_InsideRing.rgb, res.rgb, mainTex.a);
      	//加光圈颜色
        res.rgba =flowCircleLine * _FillLineColor.rgba +  fillBlit * _FillColor.rgba + res.rgba;//lerp(res.rgba,_FillColor,fillBlit);
      	//加中间花纹颜色
      	res.rgb = lerp(res.rgb, _FigureColor, figureTex.a * fillBlit * _FigureAlpha);
        res *= half4(visBlit, visBlit, visBlit, visBlit);
      	
      	//中间花纹
		res.a += _FigureAlpha * figureTex.a * visBlit * fillBlit;
      	
      	//内发光
      	res.a = res.a * _InsideRing.a + (mainTex.a + 0.5 * _InsideRing.a) * visBlit * mask;
      	
      	#ifdef _INSIDEMASK_ON
      	//内环遮罩
      	res.a *= insideMask;
      	res.a += insideRing;
      	res.rgb = lerp(res.rgb,_MainColor,insideRing);
		#endif

      	//光圈边框的颜色影响边框
      	#ifdef _INSIDEMASK_ON
      	res.rgb = lerp (res.rgb,_FillLineColor.rgb,fillBlitWithoutMask * (mainTex.a + insideRing));
      	#else
      	res.rgb = lerp (res.rgb,_FillLineColor.rgb,fillBlitWithoutMask * (mainTex.a ));
      	#endif
      	

      	res.a = saturate(res.a);
        res.rgb = MixFog(res.rgb, i.ray.w);

        return res;
      }
      ENDHLSL
    }
  }
}
