using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SGameBloomRF : ScriptableRendererFeature
{

    class CustomRenderPass : ScriptableRenderPass
    {
        // bloom 控制参数
        public float Threshold;
        public float Intensity;
        public float Scatter;
        public Color Tint;
        public float Clamp;
        public bool HighQuatlityFilteringSettings;
        public int BlurTimes;    //卷积模糊次数
        public float BloomMaskStrength;  

        //bloom材质
        public Material bloomMat;   //卷积模糊,混合bloom用
        public Material maskMat;    //计算角色遮罩用

        public FilteringSettings maskFiltering;

        public int FilterDepthTexId = Shader.PropertyToID("_FilterTex");
        public int bloomMaskId = Shader.PropertyToID("_BloomMaskTexture");
        public int cameraColorTex = Shader.PropertyToID("_CameraColorTempTex");
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in an performance manner.
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(bloomMaskId,cameraTextureDescriptor.width/2,cameraTextureDescriptor.height/2, 1,FilterMode.Point, RenderTextureFormat.ARGB32);
            ConfigureTarget(bloomMaskId);
            ConfigureClear(ClearFlag.All,Color.white);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            //获取cmd
            CommandBuffer cmd = CommandBufferPool.Get("Render");
            
            ShaderTagId shaderTageId = new ShaderTagId("UniversalForward");
            var sortSetting = new SortingSettings(renderingData.cameraData.camera);
            DrawingSettings bloomMaskSetting = new DrawingSettings(shaderTageId, sortSetting);
            bloomMaskSetting.overrideMaterial = maskMat;
            bloomMaskSetting.overrideMaterialPassIndex = 0;
            
            renderingData.cameraData.camera.TryGetCullingParameters(out var cullingParameters);
            var cullingResults = context.Cull(ref cullingParameters);
            context.DrawRenderers(cullingResults, ref bloomMaskSetting,ref maskFiltering);
            cmd.SetGlobalTexture("_BloomMaskTex",bloomMaskId);
            
            //bloomFilter部分
            float scatter = Mathf.Lerp(0.05f, 0.95f, Scatter);
            float clamp = Clamp;
            float threshold = Mathf.GammaToLinearSpace(Threshold);
            float thresholdKnee = threshold * 0.5f; 
            bloomMat.SetVector("_Params",new Vector4(scatter,clamp,threshold,thresholdKnee)) ;// x: scatter, y: clamp, z: threshold (linear), w: threshold knee
            bloomMat.SetFloat("_BloomMaskStrength",BloomMaskStrength);
            
            cmd.GetTemporaryRT(FilterDepthTexId,renderingData.cameraData.camera.pixelWidth,renderingData.cameraData.camera.pixelHeight,0,FilterMode.Bilinear,GraphicsFormat.B10G11R11_UFloatPack32);
            cmd.Blit(Shader.PropertyToID("_CameraColorTexture"),FilterDepthTexId,bloomMat,0);
            //bloom模糊部分
            List<int> MipDownArray = new List<int>();
            List<int> MipUpArray = new List<int>();
            int width = renderingData.cameraData.camera.pixelWidth;
            int height = renderingData.cameraData.camera.pixelHeight;
            for (int i = 0; i < BlurTimes; i++)
            {
                width = width / 2;
                height = height / 2;
                MipDownArray.Add(Shader.PropertyToID("_MipDown"+i.ToString()));
                MipUpArray.Add(Shader.PropertyToID("_MipUp"+i.ToString()));
                cmd.GetTemporaryRT(MipDownArray[i],width,height, 0,FilterMode.Bilinear, GraphicsFormat.B10G11R11_UFloatPack32);
                cmd.GetTemporaryRT(MipUpArray[i],width,height, 0,FilterMode.Bilinear, GraphicsFormat.B10G11R11_UFloatPack32);;
            }
            //bloom 降采样
            cmd.Blit(FilterDepthTexId, MipDownArray[0], bloomMat, 1);
            cmd.Blit(MipDownArray[0], MipUpArray[0], bloomMat, 2);
                
            for (int i = 1; i < BlurTimes; i++)
            {
                cmd.Blit(MipUpArray[i-1], MipDownArray[i], bloomMat, 1);
                cmd.Blit(MipDownArray[i], MipUpArray[i], bloomMat, 2);
            }
            //bloom 升采样
            cmd.SetGlobalTexture("_MainTexLowMip",MipDownArray[BlurTimes-1]);
            cmd.Blit(MipDownArray[BlurTimes-2],MipUpArray[BlurTimes-2],bloomMat,3);
            
            for (int i = 1; i < BlurTimes; i++)
            {
                cmd.SetGlobalTexture("_MainTexLowMip",MipUpArray[BlurTimes-i]);
                cmd.Blit(MipDownArray[BlurTimes-i-1],MipUpArray[BlurTimes-i-1],bloomMat,3);
            }
            //bloom叠加部分
            //获取一份CameraColor的备份
            cmd.GetTemporaryRT(cameraColorTex,renderingData.cameraData.camera.pixelWidth,renderingData.cameraData.camera.pixelHeight,0,FilterMode.Bilinear,GraphicsFormat.B10G11R11_UFloatPack32);
            cmd.Blit(Shader.PropertyToID("_CameraColorTexture"),cameraColorTex);
            //设置颜色和其它参数
            cmd.SetGlobalTexture("_BloomBlitTex",cameraColorTex);
            //blendMat.SetTexture("_BloomBlitTex",);
            bloomMat.SetFloat("BloomIntensity", Intensity);
            bloomMat.SetColor("BloomTint",Tint);
            cmd.Blit(MipUpArray[0],Shader.PropertyToID("_CameraColorTexture"),bloomMat,4); //要求_MainTex为MipUpTex
            cmd.ReleaseTemporaryRT(cameraColorTex);
            context.ExecuteCommandBuffer(cmd);
            
        }

        /// Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd)
        {
            
        }

        public void Initialize(SGameBloomRF.RenderPassData renderPassData)
        {
            Threshold = renderPassData.Threshold;
            Intensity = renderPassData.Intensity;
            Scatter = renderPassData.Scatter;
            Tint = renderPassData.Tint;
            Clamp = renderPassData.Clamp;
            HighQuatlityFilteringSettings = renderPassData.HighQuatlityFilteringSettings;
            BlurTimes = renderPassData.BlurTime;
            
            bloomMat = renderPassData.bloomMat;
            maskMat = renderPassData.maskMat;
            BloomMaskStrength = renderPassData.BloomMaskStrength;

            maskFiltering = renderPassData.maskFiltering;
        }
        
    }

    // bloom 控制参数
    public float Threshold = 1;
    public float Intensity = 1;
    public float Scatter = 1;
    public Color Tint = Color.white;
    public float Clamp = float.MaxValue;
    //public bool HighQuatlityFilteringSettings = false;
    public int BlurTime = 4;    //卷积模糊次数
    public float BloomMaskStrength = 1; //控制BloomMask的强度
    
    //bloom材质
    public Material bloomMat;   //卷积模糊用
    public Material maskMat;    //计算角色遮罩用
    CustomRenderPass m_ScriptablePass;

    public LayerMask layerMask;  //标记哪些地方要较少Bloom的渲染.
    
    public struct RenderPassData
    {
        public float Threshold;
        public float Intensity;
        public float Scatter;
        public Color Tint;
        public float Clamp;
        public bool HighQuatlityFilteringSettings;
        public int BlurTime ;    //卷积模糊次数
        public float BloomMaskStrength; //控制BloomMask的强度

        //bloom材质
        public Material bloomMat;   //卷积模糊用
        public Material maskMat;    //计算遮罩用(标注哪些部分减弱bloom效果)
        
        public FilteringSettings maskFiltering;
    }

    private RenderPassData renderPassData;
    public override void Create()
    {
        this.name = "SGame Bloom";
        m_ScriptablePass = new CustomRenderPass();

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;//在渲染后处理之前进行

        renderPassData = new RenderPassData();

    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (bloomMat != null && maskMat != null)
        {
            renderPassData.maskFiltering = new FilteringSettings(RenderQueueRange.all, layerMask);
            //renderPassData.blendMat = blendMat;
            renderPassData.bloomMat = bloomMat;
            renderPassData.maskMat = maskMat;
            renderPassData.Threshold = Threshold;
            renderPassData.Intensity = Intensity;
            renderPassData.Scatter = Scatter;
            renderPassData.Tint = Tint;
            renderPassData.Clamp = Clamp;
           // renderPassData.HighQuatlityFilteringSettings = HighQuatlityFilteringSettings;
            renderPassData.BlurTime = BlurTime;
            renderPassData.BloomMaskStrength = BloomMaskStrength;
            
            m_ScriptablePass.Initialize(renderPassData);
            renderer.EnqueuePass(m_ScriptablePass);
        }
        
    }
}


