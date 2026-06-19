using StarProject.Game;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class ScreenRgbSplitterRenderFeature : ScriptableRendererFeature
{
    Shader FeatureShader = null;
    ScreenRgbSplitterPass mRgbSplitterPass;
    Material mFeatureMaterial = null;

    public override void Create()
    {
        FeatureShader = Shader.Find("SGAME/Screen Rgb Splitter");
        if (FeatureShader)
            mFeatureMaterial = CoreUtils.CreateEngineMaterial(FeatureShader);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!FeatureShader)
            return;

        if (!renderingData.cameraData.postProcessEnabled)
            return;

        var rgbSplitterVolume = VolumeManager.instance.stack.GetComponent<ScreenRgbSplitter>();
        if (!rgbSplitterVolume && !rgbSplitterVolume.active)
            return;

        if (renderingData.cameraData.cameraType != CameraType.Game)
        {
            if (!rgbSplitterVolume.PreviewInSceneView.value)
                return;

            if (mRgbSplitterPass == null)
                mRgbSplitterPass = new ScreenRgbSplitterPass(FeatureShader, this, mFeatureMaterial);

            mRgbSplitterPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            mRgbSplitterPass.ApplyParams(rgbSplitterVolume);
            renderer.EnqueuePass(mRgbSplitterPass);
        }
        else
        {
            if (mRgbSplitterPass == null)
                mRgbSplitterPass = new ScreenRgbSplitterPass(FeatureShader, this, mFeatureMaterial);

            mRgbSplitterPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

            mRgbSplitterPass.ApplyParams(rgbSplitterVolume);
            renderer.EnqueuePass(mRgbSplitterPass);
        }
    }


    private void OnDestroy()
    {
        if (mRgbSplitterPass != null)
            mRgbSplitterPass.Cleanup();
        mRgbSplitterPass = null;
        CoreUtils.Destroy(mFeatureMaterial);
    }

    protected override void Dispose(bool disposing)
    {
        if (mRgbSplitterPass != null)
            mRgbSplitterPass.Cleanup();
        mRgbSplitterPass = null;
        CoreUtils.Destroy(mFeatureMaterial);
    }
}

class ScreenRgbSplitterPass : ScriptableRenderPass
{
    static readonly int ShaderID_RgbSplitOffset = Shader.PropertyToID("_RgbSplitOffset");
    static readonly int ShaderID_RgbSplitRadius = Shader.PropertyToID("_RgbSplitRadius");
    static readonly int ShaderID_RgbSplitRoundness = Shader.PropertyToID("_RgbSplitRoundness");
    static readonly int ShaderID_RgbSplitSmoothness = Shader.PropertyToID("_RgbSplitSmoothness");

    static readonly int ShaderID_EdgeNoise = Shader.PropertyToID("_EdgeNoiseTexture");
    static readonly int ShaderID_EdgeMoveSpeed = Shader.PropertyToID("_EdgeMoveSpeed");
    static readonly int ShaderID_EdgeNoiseSize1 = Shader.PropertyToID("_EdgeNoiseSize1");
    static readonly int ShaderID_EdgeNoiseSize2 = Shader.PropertyToID("_EdgeNoiseSize2");

    static readonly int ShaderID_EdgeRadius = Shader.PropertyToID("_EdgeRadius");
    static readonly int ShaderID_EdgeContract = Shader.PropertyToID("_EdgeContract");
    static readonly int ShaderID_EdgeDistortion = Shader.PropertyToID("_EdgeDistortion");


    static readonly int ShaderID_EdgeBodyColor = Shader.PropertyToID("_EdgeBodyColor");
    static readonly int ShaderID_EdgeBorderColor = Shader.PropertyToID("_EdgeBorderColor");
    static readonly int ShaderID_EdgeBorderThreshold = Shader.PropertyToID("_EdgeBorderThreshold");
    static readonly int ShaderID_EdgeBorderThickness = Shader.PropertyToID("_EdgeBorderThickness");

    static readonly int ShaderID_EdgeLightFlowSpeed = Shader.PropertyToID("_EdgeLightFlowSpeed");
    static readonly int ShaderID_EdgeLightFlowThickness = Shader.PropertyToID("_EdgeLightFlowThickness");
    static readonly int ShaderID_EdgeLightFlowColor = Shader.PropertyToID("_EdgeLightFlowColor");
    static readonly int ShaderID_EdgeLightFlowThreshold = Shader.PropertyToID("_EdgeLightFlowThreshold");

    static readonly ProfilingSampler mProfilingSampler = new ProfilingSampler("RgbSplitter");
    Material mFeatureMaterial = null;

    public ScreenRgbSplitterPass(Shader featureShader, ScreenRgbSplitterRenderFeature parentRenderFeature, Material featureMaterial)
    {
        mFeatureMaterial = featureMaterial;
    }

    public void ApplyParams(ScreenRgbSplitter rgbSplitterVolume)
    {
        if (!mFeatureMaterial)
            return;

        mFeatureMaterial.SetFloat(ShaderID_RgbSplitOffset, rgbSplitterVolume.SplitOffset.value);
        mFeatureMaterial.SetFloat(ShaderID_RgbSplitRadius, rgbSplitterVolume.Radius.value);
        //mFeatureMaterial.SetFloat(ShaderID_RgbSplitRoundness, rgbSplitterVolume.Roundness.value);
        //mFeatureMaterial.SetFloat(ShaderID_RgbSplitSmoothness, rgbSplitterVolume.Smoothness.value);

        //mFeatureMaterial.SetTexture(ShaderID_EdgeNoise, rgbSplitterVolume.EdgeNoiseTexture.value);
        //mFeatureMaterial.SetVector(ShaderID_EdgeMoveSpeed, new Vector4(rgbSplitterVolume.EdgeNoiseMoveSpeed1.value.x
        //                                                            , rgbSplitterVolume.EdgeNoiseMoveSpeed1.value.y
        //                                                            , rgbSplitterVolume.EdgeNoiseMoveSpeed2.value.x
        //                                                            , rgbSplitterVolume.EdgeNoiseMoveSpeed2.value.y));
        //mFeatureMaterial.SetFloat(ShaderID_EdgeNoiseSize1, 1 / rgbSplitterVolume.EdgeNoiseSize1.value);
        //mFeatureMaterial.SetFloat(ShaderID_EdgeNoiseSize2, 1 / rgbSplitterVolume.EdgeNoiseSize2.value);


        //mFeatureMaterial.SetFloat(ShaderID_EdgeRadius, rgbSplitterVolume.EdgeRadius.value);
        //mFeatureMaterial.SetFloat(ShaderID_EdgeContract, 1 / rgbSplitterVolume.EdgeContract.value);
        //mFeatureMaterial.SetFloat(ShaderID_EdgeDistortion, rgbSplitterVolume.EdgeDistortion.value / Screen.width);


        //mFeatureMaterial.SetColor(ShaderID_EdgeBodyColor, rgbSplitterVolume.EdgeBodyColor.value);
        //mFeatureMaterial.SetColor(ShaderID_EdgeBorderColor, rgbSplitterVolume.EdgeBorderColor.value);
        //mFeatureMaterial.SetFloat(ShaderID_EdgeBorderThreshold, rgbSplitterVolume.EdgeBorderThreshold.value);
        //mFeatureMaterial.SetFloat(ShaderID_EdgeBorderThickness, rgbSplitterVolume.EdgeBorderThickness.value);


        //mFeatureMaterial.SetFloat(ShaderID_EdgeLightFlowSpeed, rgbSplitterVolume.EdgeLightFlowSpeed.value);
        //mFeatureMaterial.SetFloat(ShaderID_EdgeLightFlowThickness, rgbSplitterVolume.EdgeLightFlowThickness.value);
        //mFeatureMaterial.SetColor(ShaderID_EdgeLightFlowColor, rgbSplitterVolume.EdgeLightFlowColor.value);
        //mFeatureMaterial.SetFloat(ShaderID_EdgeLightFlowThreshold, rgbSplitterVolume.EdgeLightFlowThreshold.value);
    }


    static readonly string CommandBufferName = "Rgb Splitter";

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(CommandBufferName);
        using (new ProfilingScope(cmd, mProfilingSampler))
        {
            Blit(cmd, ref renderingData, mFeatureMaterial);
        }
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public void Cleanup()
    {
    }
}