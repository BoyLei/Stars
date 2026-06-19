using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;



public class ScreenShockyRenderPassFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        public Shader shader;
    }

    public Settings settings = new Settings();

    ScreenShockyPass ScreenShockyPass;



    public override void Create()
    {
        this.name = "ScreenShockyPass";

        ScreenShockyPass = new ScreenShockyPass(settings.renderPassEvent, settings.shader);
    }
    

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {

        //ScreenShockyPass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(ScreenShockyPass);

    }


}


public class ScreenShockyPass : ScriptableRenderPass
{
    static readonly string k_RenderTag = "ScreenShocky Effects";
    static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    static readonly int TempTargetId = Shader.PropertyToID("_TempTargetScreenShocky");



    ScreenShockyVolume ScreenShockyVolume;


    Material ScreenShockyMaterial;
    RenderTargetIdentifier currentTarget;



    public ScreenShockyPass(RenderPassEvent evt,Shader ScreenShockyShader )
    {
        renderPassEvent = evt;

        var shader = ScreenShockyShader;


        if (shader == null) return;

        ScreenShockyMaterial = CoreUtils.CreateEngineMaterial(ScreenShockyShader);


    }


    public void Setup(in RenderTargetIdentifier currentTarget)
    {
        this.currentTarget = currentTarget;
    }



    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (ScreenShockyMaterial == null) return;

        if (!renderingData.cameraData.postProcessEnabled) return;

        var stack = VolumeManager.instance.stack;

        ScreenShockyVolume = stack.GetComponent<ScreenShockyVolume>();

        if (ScreenShockyVolume == null) return;

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

        ScreenShockyMaterial.SetVector("_FocalPoint", ScreenShockyVolume.FocalPoint.value);

        ScreenShockyMaterial.SetFloat("_Size", ScreenShockyVolume.Size.value);
        ScreenShockyMaterial.SetFloat("_Magnification", ScreenShockyVolume.Magnification.value);
        ScreenShockyMaterial.SetFloat("_Speed", ScreenShockyVolume.Speed.value);

        cmd.SetGlobalTexture(MainTexId, source);
        cmd.GetTemporaryRT(destination, camera.scaledPixelWidth, camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
        cmd.Blit(source, destination);
        cmd.Blit(destination, source, ScreenShockyMaterial, 0);
        cmd.ReleaseTemporaryRT(destination);
    }
}



public class ScreenShockyVolume : VolumeComponent
{
    public ColorParameter ColorChange = new ColorParameter(Color.white, true);

    public Vector2Parameter FocalPoint = new Vector2Parameter(new Vector2( 0.5f,0.5f));
    public FloatParameter Size = new FloatParameter(0.1f);
    public FloatParameter Magnification = new FloatParameter(0.1f);
    public FloatParameter Speed = new FloatParameter(1.0f);
}


