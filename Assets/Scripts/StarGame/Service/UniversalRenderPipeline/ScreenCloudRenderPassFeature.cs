using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;



public class ScreenCloudRenderPassFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        public Shader shader;
    }

    public Settings settings = new Settings();

    ScreenCloudPass ScreenCloudPass;



    public override void Create()
    {
        this.name = "ScreenCloudPass";

        ScreenCloudPass = new ScreenCloudPass(settings.renderPassEvent, settings.shader);
    }


    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {

        //ScreenCloudPass.Setup(renderer.cameraColorTarget);


        renderer.EnqueuePass(ScreenCloudPass);

    }


}


public class ScreenCloudPass : ScriptableRenderPass
{
    static readonly string k_RenderTag = "ScreenCloud Effects";
    static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    static readonly int TempTargetId = Shader.PropertyToID("_TempTargetScreenCloud");



    ScreenCloudVolume ScreenCloudVolume;


    Material ScreenCloudMaterial;
    RenderTargetIdentifier currentTarget;


    public ScreenCloudPass(RenderPassEvent evt,Shader ScreenCloudShader )
    {
        renderPassEvent = evt;

        var shader = ScreenCloudShader;


        if (shader == null) return;

        ScreenCloudMaterial = CoreUtils.CreateEngineMaterial(ScreenCloudShader);


    }


    public void Setup(in RenderTargetIdentifier currentTarget)
    {
        this.currentTarget = currentTarget;
    }



    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (ScreenCloudMaterial == null) return;

        if (!renderingData.cameraData.postProcessEnabled) return;
      
        var stack = VolumeManager.instance.stack;

        ScreenCloudVolume = stack.GetComponent<ScreenCloudVolume>();

        if (ScreenCloudVolume == null) return;

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

        ScreenCloudMaterial.SetColor("_Color", ScreenCloudVolume.ColorChange.value);
        ScreenCloudMaterial.SetFloat("_Fade", ScreenCloudVolume.Fade.value);

        //�����������
        if(StarProject.Game.GameManager.Instance!=null)
        {
            var pos = StarProject.Game.GameManager.Instance.GetEntityPosById(StarProject.Game.GameManager.Instance.mainPlayerId);
            ScreenCloudMaterial.SetVector("_PlayerPos", new Vector2(pos.x, pos.z));
            //ScreenCloudMaterial.SetVector("_PlayerPos", ScreenCloudVolume.PlayerPos.value);
        }


        cmd.SetGlobalTexture(MainTexId, source);
        cmd.GetTemporaryRT(destination, camera.scaledPixelWidth, camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
        cmd.Blit(source, destination);
        cmd.Blit(destination, source, ScreenCloudMaterial, 0);
        cmd.ReleaseTemporaryRT(destination);
    }
}



public class ScreenCloudVolume : VolumeComponent
{
    public ColorParameter ColorChange = new ColorParameter(new Color32(186,207, 193,255), true);
    public FloatParameter Fade = new FloatParameter(0.4f);
    public Vector3Parameter PlayerPos = new Vector3Parameter(Vector3.zero);
}


