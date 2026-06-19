using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;



public class SGSRRenderPassFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        public Shader shader;
    }

    public Settings settings = new Settings();

    SGSRPass SGSRPass;



    public override void Create()
    {
        this.name = "SGSRPass";

        SGSRPass = new SGSRPass(settings.renderPassEvent, settings.shader);
    }
    

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {

        //SGSRPass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(SGSRPass);

    }


}


public class SGSRPass : ScriptableRenderPass
{
    static readonly string k_RenderTag = "SGSR Effects";
    static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    static readonly int TempTargetId = Shader.PropertyToID("_TempTargetSGSR");



    SGSRVolume SGSRVolume;


    Material SGSRMaterial;
    RenderTargetIdentifier currentTarget;



    public SGSRPass(RenderPassEvent evt,Shader SGSRShader )
    {
        renderPassEvent = evt;

        var shader = SGSRShader;


        if (shader == null) return;

        SGSRMaterial = CoreUtils.CreateEngineMaterial(SGSRShader);


    }


    public void Setup(in RenderTargetIdentifier currentTarget)
    {
        this.currentTarget = currentTarget;
    }



    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (SGSRMaterial == null) return;

        if (!renderingData.cameraData.postProcessEnabled) return;

        var stack = VolumeManager.instance.stack;

        SGSRVolume = stack.GetComponent<SGSRVolume>();

        if (SGSRVolume == null) return;

        var cmd = CommandBufferPool.Get(k_RenderTag);

        Setup(renderingData.cameraData.renderer.cameraColorTarget);

        Render(cmd, ref renderingData);

        context.ExecuteCommandBuffer(cmd);

        CommandBufferPool.Release(cmd);
    }



    void Render(CommandBuffer cmd,ref RenderingData renderingData)
    {
        ref var cameraData = ref renderingData.cameraData;
        var camera = cameraData.camera;
        var source = currentTarget;
        int destination = TempTargetId;

        float w = 0;
        float h = 0;

        //��ȡ����ʱ�ֱ���
        if(StarProject.Game.GameManager.Instance != null)
        {
            w = (int)Mathf.Ceil(ScalableBufferManager.widthScaleFactor * Screen.currentResolution.width);
            h = (int)Mathf.Ceil(ScalableBufferManager.heightScaleFactor * Screen.currentResolution.height);
            SGSRMaterial.SetFloat("_EdgeSharpness", StarProjectDef.GameConfig.SGSR_EdgeSharpness);
        }
        else
        {
            w = Screen.currentResolution.width;
            h = Screen.currentResolution.height;
            SGSRMaterial.SetFloat("_EdgeSharpness", SGSRVolume.EdgeSharpness.value);
        }


        SGSRMaterial.SetFloat("_ScreenWidth", w);
        SGSRMaterial.SetFloat("_ScreenHeight", h);


        


        cmd.SetGlobalTexture(MainTexId, source);
        cmd.GetTemporaryRT(destination, camera.scaledPixelWidth, camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
        cmd.Blit(source, destination);
        cmd.Blit(destination, source, SGSRMaterial, 0);
        cmd.ReleaseTemporaryRT(destination);
    }
}



public class SGSRVolume : VolumeComponent
{
    public FloatParameter EdgeSharpness = new FloatParameter(2f);
}


