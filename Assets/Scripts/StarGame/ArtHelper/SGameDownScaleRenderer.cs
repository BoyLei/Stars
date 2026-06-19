using System.CodeDom;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SGameDownScaleRenderer : ScriptableRendererFeature
{
    public string ShaderTag = "DownScaled";
    public bool EnableDownScale = true;

    public bool CareOpaques = true;
    public LayerMask LayerMask;
    [Range(0.25f, 0.99f)]
    public float DownScale = 0.85f;

    public bool CareTransparents = false;
    public LayerMask TransparentLayerMask;
    [Range(0.25f, 0.99f)]
    public float TransparentDownScale = 0.85f;

    DownScaledRenderPass mDownScalePassesOpaque;
    DownScaledRenderPass mDownScalePassesOpaqueTransparent;

    const float gLowestScaleThreshold = 0.84f;
    const string ScopeName = "Down Scaled Renderers";
    /// <inheritdoc/>
    public override void Create()
    {

    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        bool enableDownScale = EnableDownScale && DownScale < gLowestScaleThreshold;
        if (CareOpaques)
        {
            if (mDownScalePassesOpaque == null)
                mDownScalePassesOpaque = new DownScaledRenderPass(ScopeName, RenderQueueRange.opaque, ShaderTag);

            mDownScalePassesOpaque.renderPassEvent = enableDownScale ? RenderPassEvent.BeforeRenderingOpaques : RenderPassEvent.AfterRenderingOpaques;
            mDownScalePassesOpaque.LayerMask = LayerMask;

            mDownScalePassesOpaque.DownScale = DownScale;
            mDownScalePassesOpaque.EnableDownScale = enableDownScale;
            mDownScalePassesOpaque.IsOpaque = true;
            mDownScalePassesOpaque.Setup();

            // 如果没开启的时候，需要直接用Cmd.DrawRenderers绘制到主Framebuffer里面（调用的是DownScaled Tag的Pass）
            // 因此需要将ForwardLit的那个Tag关闭，关闭的方式就是通过控制相机的LayerMask去控制.
            //if (enableDownScale)
            //    renderingData.cameraData.camera.cullingMask |= LayerMask;
            //else
            //    renderingData.cameraData.camera.cullingMask &= ~LayerMask; 

            renderer.EnqueuePass(mDownScalePassesOpaque);
        }
        else
            Shader.SetGlobalFloat(DownScaledRenderPass.ShaderProperty_GlobalOpaqueScaleFator, 1);

        if (CareTransparents)
        {
            if (mDownScalePassesOpaqueTransparent == null)
                mDownScalePassesOpaqueTransparent = new DownScaledRenderPass(ScopeName, RenderQueueRange.transparent, ShaderTag);

            mDownScalePassesOpaqueTransparent.renderPassEvent = enableDownScale ? RenderPassEvent.BeforeRenderingTransparents : RenderPassEvent.AfterRenderingTransparents;
            mDownScalePassesOpaqueTransparent.LayerMask = TransparentLayerMask;

            mDownScalePassesOpaqueTransparent.DownScale = TransparentDownScale;
            mDownScalePassesOpaqueTransparent.EnableDownScale = enableDownScale;
            mDownScalePassesOpaqueTransparent.IsOpaque = false;
            mDownScalePassesOpaqueTransparent.Setup();

            //if (enableDownScale)
            //    renderingData.cameraData.camera.cullingMask |= TransparentLayerMask; 
            //else
            //    renderingData.cameraData.camera.cullingMask &= ~TransparentLayerMask;

            renderer.EnqueuePass(mDownScalePassesOpaqueTransparent);
        }
    }
}

class DownScaledRenderPass : ScriptableRenderPass
{
    LayerMask mLayerMask;
    FilteringSettings mFilteringSettings;
    ProfilingSampler mProfilingSampler;
    RenderQueueRange mRenderQueueRange;

    RenderTargetHandle mDownScaleRtHandle;

    static readonly string Name_GlobalDownScaledRt = "_GlobalDownScaledOpaques";
    static readonly string Name_GlobalDownScaledTransparentRt = "_GlobalDownScaledTransparents";
    public static readonly string ShaderProperty_GlobalOpaqueScaleFator = "_GlobalOpaqueScaleFator";


    static readonly int _GlobalDownScaledRt = Shader.PropertyToID(Name_GlobalDownScaledRt); // 这个Rt保留，可以给后续特效、水做地表的Grab.
    static readonly int _GlobalDownScaledTransparentRt = Shader.PropertyToID(Name_GlobalDownScaledTransparentRt);

    internal bool allocateDepth { get; set; } = true;

    public float DownScale = 1;
    public bool EnableDownScale = true;
    public bool IsOpaque = true;

    List<ShaderTagId> mShaderTagIdList = new List<ShaderTagId>();

    public DownScaledRenderPass(string profilerTag, RenderQueueRange renderQueueRange, string shaderTag)
    {
        mProfilingSampler = new ProfilingSampler(profilerTag);
        mRenderQueueRange = renderQueueRange;
        mShaderTagIdList.Clear();
        mShaderTagIdList.Add(new ShaderTagId(shaderTag));
    }

    public void Setup()
    {
        if (IsOpaque)
            mDownScaleRtHandle.Init(Name_GlobalDownScaledRt);
        else
            mDownScaleRtHandle.Init(Name_GlobalDownScaledTransparentRt);

        this.allocateDepth = EnableDownScale;
        mFilteringSettings = new FilteringSettings(mRenderQueueRange, mLayerMask);
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        if (EnableDownScale)
        {
            // 如果开启了降分辨率.
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            if (!renderingData.cameraData.isSceneViewCamera)
            {
                descriptor.width = Mathf.RoundToInt(descriptor.width * DownScale);
                descriptor.height = Mathf.RoundToInt(descriptor.height * DownScale);
            }
            descriptor.msaaSamples = 1;
            if (!IsOpaque)
                descriptor.colorFormat = RenderTextureFormat.ARGB32;

            if (allocateDepth)
                cmd.GetTemporaryRT(mDownScaleRtHandle.id, descriptor, FilterMode.Trilinear);
            ConfigureTarget(new RenderTargetIdentifier(mDownScaleRtHandle.Identifier(), 0, CubemapFace.Unknown, -1));
            if (IsOpaque)
                cmd.SetGlobalTexture(_GlobalDownScaledRt, mDownScaleRtHandle.id);
            else
                cmd.SetGlobalTexture(_GlobalDownScaledTransparentRt, mDownScaleRtHandle.id);

            cmd.SetGlobalFloat(ShaderProperty_GlobalOpaqueScaleFator, DownScale);

            ConfigureClear(ClearFlag.All, Color.black);
        }
        else
        {
            cmd.SetGlobalFloat(ShaderProperty_GlobalOpaqueScaleFator, 1);
            ConfigureClear(ClearFlag.None, Color.black);
        }
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;

        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, mProfilingSampler))
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            var drawingSettings = CreateDrawingSettings(mShaderTagIdList, ref renderingData, sortingCriteria);
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref mFilteringSettings);
        }
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        if (mDownScaleRtHandle != RenderTargetHandle.CameraTarget)
        {
            if (allocateDepth)
                cmd.ReleaseTemporaryRT(mDownScaleRtHandle.id);
            mDownScaleRtHandle = RenderTargetHandle.CameraTarget;
        }
    }

    public LayerMask LayerMask { set { mLayerMask = value; } get { return mLayerMask; } }
}