using SGF.Module.Framework;
using StarProjectDef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace StarProject.Service.UniversalRenderPipeline
{
    //获取管线，全局管理，forwardRender，开启Pass关联Scriptable脚本
    //控制自增的Render
    //处理UI和RenderQueue（世界）的关系
    public class UniRenderPipline : ServiceModule<UniRenderPipline>
    {
        private ScriptableRendererData m_ScriptableRendererData;
        private UniversalRenderPipelineAsset m_Pipeline;

        public void Init()
        {
            CheckSingleton();

            InitPipelineData();



            CloseAllRenderPassFeature();//关闭所有后处理效果



        }
        //获取
        //控制：全局，Foward


        public void InitPipelineData()
        {
            //更新 Pipeline设置
            m_Pipeline = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;

            if (m_Pipeline == null)
            {
                return;
            }

            GraphicsSettings.defaultRenderPipeline = m_Pipeline;

            //设置debug
            //m_Pipeline.shaderVariantLogLevel = ApkConfig.apkConfig.isDebug
            //    ? ShaderVariantLogLevel.AllShaders
            //    : ShaderVariantLogLevel.Disabled;


            m_Pipeline.shaderVariantLogLevel = ShaderVariantLogLevel.Disabled;
            //默认索引
           var m_DefaultRendererIndex=m_Pipeline.GetType().GetField("m_DefaultRendererIndex", BindingFlags.Instance | BindingFlags.NonPublic);
           //更新 RenderData
            FieldInfo propertyInfo = m_Pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
            m_ScriptableRendererData = ((ScriptableRendererData[])propertyInfo?.GetValue(m_Pipeline))?[(int)m_DefaultRendererIndex.GetValue(m_Pipeline)];
        }

        #region 后处理相关

        /// <summary>
        /// 打开所有后处理效果
        /// </summary>
        public void OpenAllRenderPassFeature()
        {
            SetFogRenderPassFeature(true);
            SetScreenShockyRenderPassFeature(true);
        }

        /// <summary>
        /// 关闭所有后处理效果
        /// </summary>
        public void CloseAllRenderPassFeature()
        {
            SetFogRenderPassFeature(false);
            SetScreenShockyRenderPassFeature(false);
        }

        /// <summary>
        /// 开关雾效果
        /// </summary>
        /// <param name="IsOpenFog"></param>
        public void SetFogRenderPassFeature(bool IsOpenFog)
        {
            var rf = GetRenderFeature<ScreenCloudRenderPassFeature>();
            if (rf)
                rf.SetActive(IsOpenFog);
        }

        /// <summary>
        /// 开关屏幕震荡后处理
        /// </summary>
        /// <param name="IsOpenScreenShocky"></param>
        public void SetScreenShockyRenderPassFeature(bool IsOpenScreenShocky)
        {
            var rf = GetRenderFeature<ScreenShockyRenderPassFeature>();
            if (rf)
                rf.SetActive(IsOpenScreenShocky);
        }

        /// <summary>
        /// SGSR处理
        /// </summary>
        /// <param name="IsOpenSGSR"></param>
        public void SetSGSRRenderPassFeature(bool IsOpenSGSR)
        {
            var rf = GetRenderFeature<SGSRRenderPassFeature>();
            if (rf)
                rf.SetActive(IsOpenSGSR);
        }

        #endregion


        #region 灯光

        /// <summary>
        /// 场景主光源
        /// </summary>
        public GameObject[] MainLights
        {
            get
            {
                return  GameObject.FindGameObjectsWithTag("MainLight");
            }
        }

        #endregion

        #region 镜头特效相关


        private GameObject go_bossEffect;

        /// <summary>
        /// 设置镜头效果
        /// </summary>
        /// <param name="b"></param>
        /// <param name="path"></param>
        public void SetCameraEffect(bool b, Transform cameraTr = null, string path = "")
        {
            if (b)
            {
                if (!string.IsNullOrEmpty(path) && cameraTr!=null)
                {
                    Action<GameObject> cb = (go) =>
                    {
                        if (go == null)
                        {
                            return;
                        }
                        go_bossEffect = GameObject.Instantiate(go);
                        go_bossEffect.transform.SetParent(cameraTr);
                        go_bossEffect.transform.localPosition = Vector3.zero;
                        go_bossEffect.transform.localScale = Vector3.one;
                        go_bossEffect.transform.localEulerAngles = Vector3.zero;
                    };
                    Service.Resource.ResourceFormalManager.Instance.RecursionLoadGameObject(path, E_AssetType.Effects, false, cb);
                }
            }
            else
            {
                if (go_bossEffect != null)
                {
                    GameObject.Destroy(go_bossEffect);
                    go_bossEffect = null;
                }
            }
        }

        #endregion



        /// <summary>
        /// 获取RenderFeature
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>若有多个，默认第一个</returns>
        public T GetRenderFeature<T>() where T : ScriptableRendererFeature
        {
            if (m_ScriptableRendererData == null)
            {
                InitPipelineData();
            }
            if (m_ScriptableRendererData == null)
            {
                Debug.LogError("m_ScriptableRendererData is null");
                return null;
            }
            return m_ScriptableRendererData.rendererFeatures.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// 获取RenderFeature列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetRenderFeatures<T>() where T : ScriptableRendererFeature
        {
            if (m_ScriptableRendererData == null)
            {
                InitPipelineData();
            }
            if (m_ScriptableRendererData == null)
            {
                Debug.LogError("m_ScriptableRendererData is null");
                return null;
            }
            return m_ScriptableRendererData.rendererFeatures.OfType<T>().ToList();
        }

        /// <summary>
        /// 启用 Depth Texture
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void UseDepthTexture(bool isUse)
        {
            if (m_ScriptableRendererData == null)
            {
                InitPipelineData();
            }
            if (m_Pipeline == null)
            {
                return;
            }
            m_Pipeline.supportsCameraDepthTexture = isUse;
        }

        /// <summary>
        /// 启用 Opaque Texture
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void UseOpaqueTexture(bool isUse, Downsampling targetDownsampling = Downsampling._4xBilinear)
        {
            if (m_ScriptableRendererData == null)
            {
                InitPipelineData();
            }
            if (m_Pipeline == null)
            {
                return;
            }
            m_Pipeline.supportsCameraOpaqueTexture = isUse;
            var downSampling = m_Pipeline.opaqueDownsampling;
            downSampling = targetDownsampling;
        }
        /// <summary>
        /// 是否启用 Depth Texture
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsUseDepthTexture()
        {
            if (m_ScriptableRendererData == null)
            {
                InitPipelineData();
            }
            if (m_Pipeline == null)
            {
                return false;
            }
            return m_Pipeline.supportsCameraDepthTexture;
        }
        /// <summary>
        /// 是否启用 Opaque Texture
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsUseOpaqueTexture()
        {
            if (m_ScriptableRendererData == null)
            {
                InitPipelineData();
            }
            if (m_Pipeline == null)
            {
                return false;
            }
            return m_Pipeline.supportsCameraDepthTexture;
        }

        /// <summary>
        /// 设置影子渲染距离
        /// </summary>
        /// <param name="maxDistance"></param>
        public void SetShadowDistance(float shadowDistance)
        {
            if (m_ScriptableRendererData == null)
            {
                InitPipelineData();
            }
            if (m_Pipeline == null)
            {
                return;
            }

            m_Pipeline.shadowDistance = shadowDistance;
        }

        /// <summary>
        /// 获取主灯光阴影贴图精度
        /// </summary>
        public int GetMainLightShadowMapResolution()
        {
            if (m_ScriptableRendererData == null)
            {
                InitPipelineData();
            }
            if (!m_Pipeline.supportsMainLightShadows)
            {
                return 0;
            }
            return m_Pipeline.mainLightShadowmapResolution;
        }

        /// <summary>
        /// 设置主灯光阴影贴图精度
        /// </summary>
        public void SetMainLightShadowMapResolution(int resolution)
        {
            if (m_ScriptableRendererData == null)
            {
                InitPipelineData();
            }
            if (resolution > 0)
            {
                //暂时不支持
                //m_Pipeline.mainLightShadowmapResolution = resolution;
                //m_Pipeline.supportsMainLightShadows = true;
            }
            else
            {
                //暂时不支持
                //m_Pipeline.supportsMainLightShadows = false;
            }
        }

        /// <summary>
        /// 获取主灯光阴影贴图精度
        /// </summary>
        public int GetAdditionalLightsShadowmapResolution()
        {
            if (m_ScriptableRendererData == null)
            {
                InitPipelineData();
            }
            if (!m_Pipeline.supportsAdditionalLightShadows)
            {
                return 0;
            }
            return m_Pipeline.additionalLightsShadowmapResolution;
        }

        /// <summary>
        /// 设置附加灯光阴影贴图精度
        /// </summary>
        public void SetAdditionalLightsShadowmapResolution(int resolution)
        {
            if (m_ScriptableRendererData == null)
            {
                InitPipelineData();
            }
            if (resolution > 0)
            {
                //暂时不支持
                //m_Pipeline.additionalLightsShadowmapResolution = resolution;
                //m_Pipeline.supportsAdditionalLightShadows = true;
            }
            else
            {
                //暂时不支持
                //m_Pipeline.supportsAdditionalLightShadows = false;
            }
        }

        /// <summary>
        /// 获取影子渲染距离
        /// </summary>
        /// <param name="maxDistance"></param>
        public float GetShadowDistance()
        {
            if (m_ScriptableRendererData == null)
            {
                InitPipelineData();
            }
            if (m_Pipeline == null)
            {
                return 1000f;
            }
            return m_Pipeline.shadowDistance;
        }

        /// <summary>
        /// 设置渲染尺寸
        /// </summary>
        /// <param name="renderScale"></param>
        public void SetRenderScale(float renderScale)
        {
            if (m_ScriptableRendererData == null)
            {
                InitPipelineData();
            }
            if (m_Pipeline == null)
            {
                return;
            }

            m_Pipeline.renderScale = renderScale;
        }

        /// <summary>
        /// 设置渲染尺寸
        /// </summary>
        /// <param name="renderScale"></param>
        public float GetRenderScale()
        {
            if (m_ScriptableRendererData == null)
            {
                InitPipelineData();
            }
            if (m_Pipeline == null)
            {
                return 1f;
            }

            return m_Pipeline.renderScale;
        }

        /// <summary>
        /// 获取Shdow Cascade Count
        /// </summary>
        public int GetShadowCascadeCount()
        {
            if (m_ScriptableRendererData == null)
            {
                InitPipelineData();
            }
            if (m_Pipeline == null)
            {
                return 0;
            }

            //暂时不支持
            //return m_Pipeline.shadowCascadeCount;
            return 0;
        }

        /// <summary>
        /// 附加光源数量
        /// </summary>
        /// <param name="cnt"></param>
        public void SetAdditionalLightCount(int cnt)
        {
            if (m_Pipeline == null)
            {
                return;
            }
            m_Pipeline.maxAdditionalLightsCount = Mathf.Clamp(cnt, 1, 8);
        }

        /// <summary>
        /// 附加光源数量
        /// </summary>
        /// <param name="cnt"></param>
        public int GetAdditionalLightCount()
        {
            if (m_Pipeline == null)
            {
                return 0;
            }

            return m_Pipeline.maxAdditionalLightsCount;
        }

        /// <summary>
        /// Msaa级别
        /// </summary>
        /// <param name="cnt"></param>
        public int GetMsaaSampleCount()
        {
            if (m_Pipeline == null)
            {
                return 0;
            }

            return m_Pipeline.msaaSampleCount;
        }
    }
}


