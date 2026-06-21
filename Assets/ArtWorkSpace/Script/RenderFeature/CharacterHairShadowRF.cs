using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//为角色绘制头发投影的RenderFeature
public class CharacterHairShadowRF : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        private string hairPassName;
        private string headPassName;
        private FilteringSettings hairFiltering;
        private FilteringSettings headFiltering;
        private Material HairShadowMaskMaterial;
        private Material FaceShadowMaskMaterial;

        //该texture的名字需要和面部shader采样的texture命名一致
        private static readonly int hairShadowMapId = Shader.PropertyToID("_HairShadowMaskTexture");
        
        private string cmdName = "HairShadowMask";
        
        
        public void Initialize(RenderPassData data)
        {
            hairPassName = data.renderingHairPassName;
            headPassName = data.renderingHeadPassName;
            hairFiltering = data.hairFiltering;
            headFiltering = data.faceFiltering;
            HairShadowMaskMaterial = data.hairShadowMaskMaterial;
            FaceShadowMaskMaterial = data.faceShadowMaskMaterial;
            
        }
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in an performance manner.
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(hairShadowMapId,cameraTextureDescriptor.width,cameraTextureDescriptor.height, 1,FilterMode.Point, RenderTextureFormat.ARGB32);
            //cmd.GetTemporaryRT(hairShadowMapId,cameraTextureDescriptor);
            ConfigureTarget(hairShadowMapId);
            ConfigureClear(ClearFlag.All,Color.white);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            //设置Draw Setting
            ShaderTagId shaderTageId = new ShaderTagId("UniversalForward");
            var sortSetting = new SortingSettings(renderingData.cameraData.camera);
            DrawingSettings hairDrawingSetting = new DrawingSettings(shaderTageId, sortSetting);
            hairDrawingSetting.overrideMaterial = HairShadowMaskMaterial;
            hairDrawingSetting.overrideMaterialPassIndex = 0;
            shaderTageId = new ShaderTagId("SRPDefaultUnlit");
            DrawingSettings headDrawingSetting = new DrawingSettings(shaderTageId, sortSetting);
            headDrawingSetting.overrideMaterial = FaceShadowMaskMaterial;
            headDrawingSetting.overrideMaterialPassIndex = 1;
            
            CommandBuffer cmd = CommandBufferPool.Get(cmdName);
            //DrawRenderers批量渲染对应Layer的物体
            renderingData.cameraData.camera.TryGetCullingParameters(out var cullingParameters);
            var cullingResults = context.Cull(ref cullingParameters);
            
            
            context.DrawRenderers(cullingResults, ref hairDrawingSetting,ref hairFiltering);
            context.DrawRenderers(cullingResults, ref headDrawingSetting,ref headFiltering);
           
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
            
        }

        /// Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd)
        {
        }

    }

    CustomRenderPass m_ScriptablePass;

    public Material hairShadowMaskMaterial;
    public Material faceShadowMaskMaterial;

    public LayerMask cartoonHairLayerMask;
    public LayerMask CartoonFaceLayerMask;

    private RenderPassData renderPassData;
    public struct RenderPassData
    {
        public string renderingHairPassName;
        public string renderingHeadPassName;
        public FilteringSettings hairFiltering;
        public FilteringSettings faceFiltering;
        public Material hairShadowMaskMaterial;
        public Material faceShadowMaskMaterial;
    }

    public override void Create()
    {
        this.name = "CartoonCharacterHairShadowRF";
        m_ScriptablePass = new CustomRenderPass();

        //初始化ScriptablePass所需的数据
        renderPassData = new RenderPassData();
        renderPassData.renderingHairPassName = "CartoonCast";
        renderPassData.renderingHeadPassName = "CartoonReceive";
        renderPassData.hairFiltering = new FilteringSettings(RenderQueueRange.all, cartoonHairLayerMask);
        renderPassData.faceFiltering = new FilteringSettings(RenderQueueRange.all, CartoonFaceLayerMask);
        renderPassData.hairShadowMaskMaterial = hairShadowMaskMaterial;
        renderPassData.faceShadowMaskMaterial = faceShadowMaskMaterial;
        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (hairShadowMaskMaterial != null && faceShadowMaskMaterial != null)
        {
            m_ScriptablePass.Initialize(renderPassData);
            renderer.EnqueuePass(m_ScriptablePass);
        }

    }
}


