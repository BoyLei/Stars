Shader "SGAME/Scene/ReflectionPlanar" {
    Properties {
    	[Main(GlobalSetting)]_group0 ("全局设置", float) = 0
			[Sub(GlobalSetting)]_ReflectionTex("第一层镜面反射",2D) = "white"{}
    		[Sub(GlobalSetting)]_ReflectionTex2("第二层镜面反射",2D) = "Green"{}
    		[Sub(GlobalSetting)][HDR]_ReflectTexCol("第一层镜面反射颜色调整(正片叠底)",Color) = (1,1,1,1)
    		[Sub(GlobalSetting)][HDR]_ReflectTexCol2("第二层镜面反射颜色调整(正片叠底)",Color) = (1,1,1,1)
    	
    	[Main(AlphaSetting)]_group1 ("透明度", float) = 0
    		[Title(AlphaSetting,Alpha)]
    		[Sub(AlphaSetting)]_FresnelAlphaRange("菲涅尔透明范围",range(0,1)) = 0.5
    		[Sub(AlphaSetting)]_FresnelAlphaSmoothness("菲涅尔透明度柔和度",range(0,0.5)) = 0.4
    		[Sub(AlphaSetting)]_Alpha("整体透明度",range(0,1)) = 1
    		[Sub(AlphaSetting)]_DepthFade("水的深度",range(0,10)) = 1
    	
    	[Main(SpraySetting)]_group2 ("波纹", float) = 0
    		[Title(SpraySetting,Spray)]
    		[KWEnum(SpraySetting,Yes,SPRAY_ON,No,SPRAY_OFF)]_SPRAY("是否开启波纹",float) = 1
    		[Sub(SpraySetting)]_SprayTex1("波纹法线贴图1",2D) = "Blue"{}
    		[Sub(SpraySetting)]_SpraySpeed1("波纹1运动速度",range(0,10)) = 1
    		[Sub(SpraySetting)]_SprayStrength1("波纹1强度",range(0,3)) = 1
    		[Sub(SpraySetting)]_SprayTex2("波纹法线贴图1",2D) = "Blue"{}
    		[Sub(SpraySetting)]_SpraySpeed2("波纹2运动速度",range(0,10)) = 1
    		[Sub(SpraySetting)]_SprayStrength2("波纹2强度",range(0,3)) = 1
    	
    	[Main(DisortSetting)]_group3 ("扭曲", float) = 0
    		[Title(SpraySetting,Distort)]
			[Sub(DisortSetting)]_FresenelDistortStrength("扭曲在低视角的削减程度",range(0,1)) = 0.5
    		[Sub(DisortSetting)]_DistortVector("第一层镜面在法线的三个分量上的扭曲程度",vector) = (0.1,0.1,0.1,0)
    		[Sub(DisortSetting)]_DistortVector2("第二层镜面反射在法线的三个分量上的扭曲程度",vector) = (0.1,0.1,0.1,0)
    	
    	[Main(HightLighting1Setting)]_group4 ("高光", float) = 0
    	[Title(HightLighting1Setting,HighLight)]
    	[Sub(HightLighting1Setting)][HDR]_HightLightColor("高光颜色",Color) = (1,1,1,1)
        [Sub(HightLighting1Setting)]_HightLightArea("高光出现范围",range(0,5)) = 1
    	[Sub(HightLighting1Setting)]_HightLightDir("调整高光方向",range(-10,10)) = 0
    	
    	
    	
    	[Main(HightLighting2Setting)]_group5 ("来自太阳的假高光", float) = 0
    		[Title(HightLighting1Setting,FakeLighting)]
    		[Sub(HightLighting2Setting)]_SprayTex3("假高光的法线贴图",2D) = "Blue"{}
    		[Sub(HightLighting2Setting)]_SprayStrength3("法线强度",range(0,2)) = 1
			[Sub(HightLighting2Setting)]_SprayStrength4("法线强度2",range(0,2)) = 1
    		[Sub(HightLighting2Setting)]_SpraySpeed3("波纹运动速度",range(0,3)) = 1
    		[Sub(HightLighting2Setting)][HDR]_HightLightColor2("高光颜色2",Color) = (1,1,1,1)
    		[Sub(HightLighting2Setting)]_HightLightDir2("调整高光方向2",range(-10,10)) = 0
			[Sub(HightLighting2Setting)]_Smoothness("水面光滑度",range(0,1)) = 0.5
    	
    	[Main(FresnelSetting)]_group6 ("菲涅尔", float) = 0
    		[Title(FresnelSetting,Fresnel)]
    		[Sub(FresnelSetting)][HDR]_FrenselColor("菲涅尔颜色",Color) = (1,1,1,1)
    		[Sub(FresnelSetting)]_FrenselArea("菲尼尔出现范围",range(0,5)) = 4
    	
    	[Header(Zwrite)]
    	[Enum(Off, 0, On, 1)]_ZWriteIn("是否开启深度写入",int) = 0

    	
    }
    SubShader {

        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" "UniversalMaterialType"="Lit" }

        Pass {
			Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
			Zwrite [_ZWriteIn]
        	
			HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            //#include "UnityCG.cginc"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

			//[KWEnum(SpraySetting,Yes,_OFF,No,_ON)]_SPRAY("是否开启波纹",float) = 0
			#pragma shader_feature_local _ SPRAY_ON

            uniform float4 _Color;

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            	float4 tangent : TANGENT;
            	float2 texcoord : TEXCOORD0;
            };
            struct v2f {
                float4 pos : SV_POSITION;
                float3 pos_world : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
                float4 screen_pos : TEXCOORD2;
            	float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
            	float4 uv : TEXCOORD6;
            };
			sampler2D _ReflectionTex;
			sampler2D _ReflectionTex2;
			sampler2D _SprayTex1;
			sampler2D _SprayTex2;
			sampler2D _SprayTex3;
			
			CBUFFER_START(UnityPerMaterial)

			float4 _FogColor;
			float _FogStart;
			float _FogEnd;
            float _Alpha;
			float _DepthFade;

			float4 _ReflectTexCol;
			float4 _ReflectTexCol2;

			float _FresenelDistortStrength;
			float4 _DistortVector;
			float4 _DistortVector2;
			
            float _FresnelAlphaRange;
            float _FresnelAlphaSmoothness;

			float _HightLightStrength;
			float _HightLightArea;
			float4 _HightLightColor;
			float4 _HightLightColor2;
			float _HightLightDir;
			float _HightLightDir2;
			float _Smoothness;

			float _FrenselStrength;
			float4 _FrenselColor;
			float _FrenselArea;

			float _SpraySpeed1;
			float _SprayStrength1;
			float _SpraySpeed2;
			float _SprayStrength2;
			float _SpraySpeed3;
			float _SprayStrength3;
			float _SprayStrength4;
			
			float4 _SprayTex1_ST;
			float4 _SprayTex2_ST;
			float4 _SprayTex3_ST;

			int _ZWriteIn;
			CBUFFER_END

			float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
			{
				original -= center;
				float C = cos( angle );
				float S = sin( angle );
				float t = 1 - C;
				float m00 = t * u.x * u.x + C;
				float m01 = t * u.x * u.y - S * u.z;
				float m02 = t * u.x * u.z + S * u.y;
				float m10 = t * u.x * u.y + S * u.z;
				float m11 = t * u.y * u.y + C;
				float m12 = t * u.y * u.z - S * u.x;
				float m20 = t * u.x * u.z - S * u.y;
				float m21 = t * u.y * u.z + S * u.x;
				float m22 = t * u.z * u.z + C;
				float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
				return mul( finalMatrix, original ) + center;
			}

			half3 LightingSpecularPlanar(half3 lightColor, half3 lightDir, half3 normal, half3 viewDir, half4 specular, half smoothness)
			{
			    float3 halfVec = SafeNormalize(float3(lightDir) + float3(viewDir));
			    half NdotH = half(saturate(dot(normal, halfVec)));
			    half modifier = pow(NdotH, smoothness);
			    half3 specularReflection = specular.rgb * modifier;
			    return lightColor * specularReflection;
			}

			half3 CalculateBlinnPhongPlanar(Light light, half3 normalWS,half3 viewDirectionWS, half smooothness, half3 Albedo)
			{
			    half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
			    half3 lightColor = LightingLambert(attenuatedLightColor, light.direction, normalWS);

			    lightColor *= Albedo;

			    //#if defined(_SPECGLOSSMAP) || defined(_SPECULAR_COLOR)
			    half smoothness = exp2(10 * smooothness + 1);

			    lightColor = LightingSpecularPlanar(attenuatedLightColor, light.direction, normalWS, viewDirectionWS, half4(/*surfaceData.specular*/1,1,1,1), smoothness);
			    //#endif

			    return lightColor;
			}
			
            v2f vert (appdata v) {
                v2f o = (v2f)0;
                o.pos_world = mul(unity_ObjectToWorld,v.vertex);
                o.normalDir = TransformObjectToWorldNormal(v.normal);         
				o.pos = TransformObjectToHClip(v.vertex);
				o.screen_pos = ComputeScreenPos(o.pos);
                //o.screen_pos.y = o.screen_pos.y * _ProjectionParams.x;
            	o.uv.xy = v.texcoord.xy * _SprayTex1_ST.xy + _SprayTex1_ST.zw;

            	float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				// float3 positionVS = TransformWorldToView( positionWS );
				// float4 positionCS = TransformWorldToHClip( positionWS );

            	VertexNormalInputs normalInput = GetVertexNormalInputs( v.normal, v.tangent );

            	o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);
                return o;
            }
			half4 frag(v2f i) : SV_Target {
                float3 normalDir = normalize(i.normalDir);
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.pos_world.xyz);
				Light mainLight = GetMainLight();
				float3 lightDir = mainLight.direction;
				float3 rotatedLightDir = RotateAroundAxis(float3(0,0,0),lightDir,float3(0,1,0),_HightLightDir); //float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
				float3 H = normalize(rotatedLightDir + viewDir );
                float NdotV = saturate(dot(viewDir, normalDir));
				float fade_fresnel = smoothstep(_FresnelAlphaSmoothness, _FresnelAlphaRange, NdotV);

				float3 Color = float3(0,0,0);
				
				#ifdef SPRAY_ON
				float3 WorldPosition = float3(i.tSpace0.w,i.tSpace1.w,i.tSpace2.w);
				//计算波纹
				float2 SprayUV1 = i.pos_world.xz * _SprayTex1_ST.xy + _SprayTex1_ST.zw + float2(0, _TimeParameters.x * 0.05 * _SpraySpeed1);
				float2 SprayUV2 = i.pos_world.xz * _SprayTex2_ST.xy + _SprayTex2_ST.zw + float2(0, _TimeParameters.x * 0.05 * -1 * _SpraySpeed2);
				float2 SprayUV3 = i.pos_world.xz * _SprayTex3_ST.xy + _SprayTex3_ST.zw + float2(0, _TimeParameters.x * 0.05 * _SpraySpeed3);
				float3 SprayTex1 = UnpackNormalScale(tex2D(_SprayTex1, SprayUV1),_SprayStrength1);
				float4 SprayTex2 = tex2D(_SprayTex2, SprayUV2);
				float3 SprayTex2Modifed = UnpackNormalScale(SprayTex2,_SprayStrength2);
				float3 SprayTex3 = UnpackNormalScale(tex2D(_SprayTex3, SprayUV3),_SprayStrength3);
				float3 BlendNormal = BlendNormalRNM(SprayTex1,SprayTex2Modifed);
				float3 SprayTex4 = UnpackNormalScale(SprayTex2,_SprayStrength4);
				float3 BlendNormal2 = BlendNormalRNM(SprayTex3,SprayTex4);
				

				float3 WorldNormal = normalize( i.tSpace0.xyz );
				float3 WorldTangent = i.tSpace1.xyz;
				float3 WorldBiTangent = i.tSpace2.xyz;
				float3 NormalWS = normalize(TransformTangentToWorld(BlendNormal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ))) ;
				half FresnelFactor = clamp(pow(1 - NdotV,5),0,1);
				float3 normalFresnel = lerp(NormalWS,normalDir,FresnelFactor);
				NormalWS = lerp(NormalWS,normalFresnel,_FresenelDistortStrength*3);
				float3 normalForFakeHighLightWS =  normalize(TransformTangentToWorld(BlendNormal2, half3x3( WorldTangent, WorldBiTangent, WorldNormal ))) ;
				//让底视角的世界法线效果减弱
				//_FresenelDistortStrength
				//return float4(NormalWS,1);
				

				//世界空间法线到屏幕坐标空间
				float3 MovedWorld = i.pos_world + float3(_DistortVector.x * NormalWS.x,_DistortVector.y * NormalWS.y,_DistortVector.z * NormalWS.z);
				float4 MovedClip = TransformWorldToHClip(MovedWorld);
				float4 MovedScreenPos = MovedClip;
				MovedScreenPos.y = MovedScreenPos.y * _ProjectionParams.x;
				float2 MovedScreen_uv = MovedScreenPos.xy/MovedScreenPos.w;
				MovedScreen_uv = (MovedScreen_uv + 1)*0.5;
				
				
				 //计算平面倒影/镜子效果
				half2 screen_uv = i.screen_pos.xy /i.screen_pos.w;
				screen_uv = (screen_uv + 1.0) * 0.5;
				screen_uv = float2(screen_uv.x ,screen_uv.y);
                float3 mirror_color = tex2D(_ReflectionTex, MovedScreen_uv);
				mirror_color *= _ReflectTexCol.rgb;

				//采样第二层反射贴图的uv扭曲
				float3 MovedWorld2 = i.pos_world + float3(_DistortVector2.x * NormalWS.x,_DistortVector2.y * NormalWS.y,_DistortVector2.z * NormalWS.z);
				float4 MovedClip2 = TransformWorldToHClip(MovedWorld2);
				float4 MovedScreenPos2 = MovedClip2;
				MovedScreenPos2.y = MovedScreenPos2.y * _ProjectionParams.x;
				float2 MovedScreen_uv2 = MovedScreenPos2.xy/MovedScreenPos2.w;
				MovedScreen_uv2 = (MovedScreen_uv2 + 1)*0.5;
				//采样第二层 平面反射贴图
				float3 mirror_color2 = tex2D(_ReflectionTex2,MovedScreen_uv2);
				mirror_color2 = mirror_color2;
				float3 greenColor = float3(0,1,0);
				float color2_Alpha1 = sqrt(pow(mirror_color2.r - greenColor.r,2) +pow(mirror_color2.g - greenColor.g,2) +pow(mirror_color2.b - greenColor.b,2));
				color2_Alpha1 = clamp(0.4,1,color2_Alpha1);
				color2_Alpha1 *=  _ReflectTexCol2.a;
				//return half4(color2_Alpha1,color2_Alpha1,color2_Alpha1,1);
				//越靠下,反射的角色越淡,模拟计算
				half3 deepPlane = half3(200, -10, 212);//暂时固定
				half distanceToDeep = distance(i.pos_world, half3(200, -10, 212))/40;
				distanceToDeep = saturate(distanceToDeep);
				half3 dirYup = half3(0,1,0);
				half YoV = dot(dirYup,viewDir) * 2;
				color2_Alpha1 *= YoV;
				//return half4(YoV,YoV,YoV,1);

				//混合第二层反射和第一层反射的颜色
				mirror_color.rgb = mirror_color2.rgb * _ReflectTexCol2.rgb * color2_Alpha1 + mirror_color.rgb  * (1 - color2_Alpha1);

				
				//微弱diffuse效果
				float NoL = dot(NormalWS , lightDir);
				Color = mirror_color * (NoL *0.3 + 0.8);

				//微弱高光
				float SpecularArea = /*_HightLightStrength **/ pow(max(0,dot(NormalWS,H)), 10);
				SpecularArea = step(0,SpecularArea) * SpecularArea;
				float3 Specular = SpecularArea * mainLight.color * _HightLightColor;
				
				Specular *= pow(NdotV,_HightLightArea);

				//太阳假高光
				mainLight.direction = SafeNormalize(mainLight.direction + float3(0,0,5));
				_Smoothness = saturate(_Smoothness + 2 * NdotV);
				float3 rotatedViewDir = RotateAroundAxis(float3(0,0,0),SafeNormalize(viewDir ),float3(0,1,0),_HightLightDir2);
				half3 blinPhong = CalculateBlinnPhongPlanar(mainLight, normalForFakeHighLightWS, rotatedViewDir, _Smoothness,mirror_color.rgb);
				blinPhong = clamp(0,0.8,blinPhong);
				blinPhong *=_HightLightColor2;
				//return float4(blinPhong,1);
				//Specular = step(float3(0.2,0.2,0.2));
				Color += Specular + blinPhong;
				//return float4(Specular,Specular,Specular,1);

				//菲尼尔
				float NdotV1 = 1 - saturate(dot(viewDir, NormalWS));
				float3 FresnelColor = pow(NdotV1,5) * 0.2 * _FrenselColor;
				FresnelColor *= pow(1 - NdotV, _FrenselArea * 2);
				//return float4(NdotV1,NdotV1,NdotV1,1);
				Color += FresnelColor;


				//深度影响透明度
				float4 screenPosNorm = i.screen_pos / i.screen_pos.w;
				float2 ScreenUV =i.pos.xy / _ScaledScreenParams.xy;
				screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? screenPosNorm.z : screenPosNorm.z * 0.5 + 0.5;
				float rawDepth =SampleSceneDepth( ScreenUV.xy );
				
				float screenDepth10 = LinearEyeDepth(rawDepth,_ZBufferParams);
				float distanceDepth10 = abs( ( screenDepth10 - LinearEyeDepth( screenPosNorm.z,_ZBufferParams ) ) / ( _DepthFade *10 ) );
				distanceDepth10 = saturate(distanceDepth10);
				half FinalAlpha = fade_fresnel * _Alpha * distanceDepth10;
				
				#else
				//计算平面倒影/镜子效果
				//世界空间法线到屏幕坐标空间
				half2 screen_uv2 = i.screen_pos.xy /i.screen_pos.w;
				//screen_uv2 = (screen_uv2 + 1.0) * 0.5;
				screen_uv2 = float2(screen_uv2.x ,screen_uv2.y);
                float3 mirror_color = tex2D(_ReflectionTex,screen_uv2);
				Color = mirror_color;
				half FinalAlpha = fade_fresnel * _Alpha;
				#endif

				
                return float4(Color, FinalAlpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
	CustomEditor "Yoka.Galaxy.YokaShaderGUI.YokaShaderGUI"
}
