using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public enum ScreenFogBlendMode
{
    Add,
    Multiple,
    Blend
}


[Serializable, DebuggerDisplay(k_DebuggerDisplay)]
public class ScreenFogBlendModeParameter : VolumeParameter<ScreenFogBlendMode>
{
    /// <summary>
    /// Creates a new <seealso cref="FloatParameter"/> instance.
    /// </summary>
    /// <param name="value">The initial value to store in the parameter</param>
    /// <param name="overrideState">The initial override state for the parameter</param>
    public ScreenFogBlendModeParameter(ScreenFogBlendMode value, bool overrideState = false)
        : base(value, overrideState) { }
}

[Serializable, DebuggerDisplay(k_DebuggerDisplay)]
public class RenderPassEventParameter : VolumeParameter<RenderPassEvent>
{
    /// <summary>
    /// Creates a new <seealso cref="FloatParameter"/> instance.
    /// </summary>
    /// <param name="value">The initial value to store in the parameter</param>
    /// <param name="overrideState">The initial override state for the parameter</param>
    public RenderPassEventParameter(RenderPassEvent value, bool overrideState = false)
        : base(value, overrideState) { }
}


[Serializable, VolumeComponentMenuForRenderPipeline("Fog And Cloud Shadow", typeof(UniversalRenderPipeline))]
public class ScreenFogAndCloudShadow : VolumeComponent, IPostProcessComponent
{
    public RenderPassEventParameter PassEvent = new RenderPassEventParameter(RenderPassEvent.BeforeRenderingPostProcessing);
    public BoolParameter PreviewFogInSceneView = new BoolParameter(false, true);

    public ScreenFogBlendModeParameter FogBlendMode = new ScreenFogBlendModeParameter(ScreenFogBlendMode.Add, true);
    public ClampedFloatParameter FogBlendFactor = new ClampedFloatParameter(0.5f, 0, 1, true);


    public BoolParameter IsFogEnable = new BoolParameter(false, true);
    public TextureParameter FogNoiseTexture = new TextureParameter(null, TextureDimension.Tex2D, true);
    public ClampedFloatParameter FogNoiseContract = new ClampedFloatParameter(1, 0, 2, true);
    public FloatParameter FogNoiseSize1 = new FloatParameter(100, true);
    public FloatParameter FogNoiseSize2 = new FloatParameter(50, true);
    [Tooltip("噪声随着深度做衰减")]
    public ClampedFloatParameter FogNoiseDepthFalloff = new ClampedFloatParameter(1, 0.01f, 10, true);


    public BoolParameter PlayerInteractivable = new BoolParameter(false, true);
    public FloatParameter InteractiveRadius = new FloatParameter(5, true);
    public ClampedFloatParameter InteractiveFalloff = new ClampedFloatParameter(1, 0.01f, 10, true);
    public IntParameter ScenePreviewInteractive = new IntParameter(0, true);


    public Vector2Parameter FogNoiseMoveSpeed1 = new Vector2Parameter(new Vector2(5f, 5f), true);
    public Vector2Parameter FogNoiseMoveSpeed2 = new Vector2Parameter(new Vector2(10f, 10f), true);

    public BoolParameter FadeToSky = new BoolParameter(false, true);

    [ColorUsage(true, true)]
    public ColorParameter SkyColor = new ColorParameter(new Color(0.4039f, 0.5843f, 0.6118f), true, true, true, true);

    public FloatParameter FogHeightStart = new FloatParameter(0, true);
    public ClampedFloatParameter FogHeight = new ClampedFloatParameter(5, 0, 100, true);

    [ColorUsage(true, true)]
    public ColorParameter FogColor = new ColorParameter(new Color(0.4039f, 0.5843f, 0.6118f), true, true, true, true);

    public Texture2DParameter FogLightmap = new Texture2DParameter(null, true);
    [Tooltip("指认地表的MeshRenderer可以自动获取尺寸")]
    public Vector4Parameter FogLightmapPosAndSize = new Vector4Parameter(new Vector4(0, 0, 100, 100), true);

    [ColorUsage(true, true)]
    public ColorParameter FogLightmapColor = new ColorParameter(new Color(0, 0, 0), true, true, true, true);
    public ClampedFloatParameter FogLightmapFalloff = new ClampedFloatParameter(1, 0, 10, true);
    public ClampedFloatParameter FogLightmapDistanceFalloff = new ClampedFloatParameter(1, 0.001f, 20, true);
    public ClampedFloatParameter FogLightmapDistanceMin = new ClampedFloatParameter(0, 0, 1, true);
    //public ClampedFloatParameter FogLightmapDistanceMax = new ClampedFloatParameter(1, 0, 1, true);




    public ClampedFloatParameter HeightFogFalloff = new ClampedFloatParameter(1, 0.01f, 10, true);
    public ClampedFloatParameter HeightFogDensity = new ClampedFloatParameter(1, 0, 2, true);

    [Range(0.01f, 10)]
    public ClampedFloatParameter DepthFogFalloff = new ClampedFloatParameter(1, 0.01f, 10, true);
    public FloatParameter DepthFogStart = new FloatParameter(15, true);
    public FloatParameter DepthFogEnd = new FloatParameter(100, true);
    public ClampedFloatParameter DepthFogDensity = new ClampedFloatParameter(1, 0f, 2, true);

    public BoolParameter PreviewCloudInSceneView = new BoolParameter(false, true);

    public BoolParameter IsCloudShadowEnable = new BoolParameter(false, true);
    public TextureParameter CloudShadowTexture = new TextureParameter(null, TextureDimension.Tex2D, true);

    public FloatParameter CloudShadowSize = new FloatParameter(100, true);
    public Vector2Parameter CloudShadowMoveSpeed = new Vector2Parameter(new Vector2(15f, 15f), true);
    public ColorParameter CloudShadowColor = new ColorParameter(Color.gray, false, true, true, true);

    public MeshRenderer TerrainMeshRenderer = null;


    private const string renderDataListFieldName = "m_RendererDataList";

    public bool IsActive()
    {
        if (IsFogEnable.value || IsCloudShadowEnable.value)
            return true;
        return false;
    }
    public bool IsTileCompatible()
    {
        return true;
    }
}
