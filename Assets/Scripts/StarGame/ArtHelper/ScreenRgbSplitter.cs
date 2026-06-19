using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;



[Serializable, VolumeComponentMenuForRenderPipeline("Rgb Splitter", typeof(UniversalRenderPipeline))]
public class ScreenRgbSplitter : VolumeComponent, IPostProcessComponent
{
    public BoolParameter PreviewInSceneView = new BoolParameter(false, true);
    public ClampedIntParameter SplitOffset = new ClampedIntParameter(0, 0, 40, true);

    public ClampedFloatParameter Radius = new ClampedFloatParameter(0.01f, 0, 2f, true);
    //public ClampedFloatParameter Roundness = new ClampedFloatParameter(1, 0, 5, true);
    //public ClampedFloatParameter Smoothness = new ClampedFloatParameter(1, 0.1f, 5, true);

    //[Header("Edge Noise:")]
    //public TextureParameter EdgeNoiseTexture = new TextureParameter(null, TextureDimension.Tex2D, true);

    //public FloatParameter EdgeNoiseSize1 = new FloatParameter(100, true);
    //public FloatParameter EdgeNoiseSize2 = new FloatParameter(50, true);

    //public Vector2Parameter EdgeNoiseMoveSpeed1 = new Vector2Parameter(new Vector2(5f, 5f), true);
    //public Vector2Parameter EdgeNoiseMoveSpeed2 = new Vector2Parameter(new Vector2(10f, 10f), true);

    //public ClampedFloatParameter EdgeRadius = new ClampedFloatParameter(0.75f, 0, 1, true);

    //public ClampedFloatParameter EdgeDistortion = new ClampedFloatParameter(1, 0, 50, true);
    //public ClampedFloatParameter EdgeContract = new ClampedFloatParameter(1, 0, 50, true);

    //public ColorParameter EdgeBodyColor = new ColorParameter(new Color(0.4039f, 0.5843f, 0.6118f), true, true, true, true);

    //[Header("Edge Border:")]
    //public ColorParameter EdgeBorderColor = new ColorParameter(new Color(0.4039f, 0.5843f, 0.6118f), true, true, true, true);

    //public ClampedFloatParameter EdgeBorderThreshold = new ClampedFloatParameter(0, 0, 1f, true);
    //public ClampedFloatParameter EdgeBorderThickness = new ClampedFloatParameter(0, 0, 1f, true);

    //[Header("Edge Light Flow:")]
    //public ClampedFloatParameter EdgeLightFlowSpeed = new ClampedFloatParameter(0, -100, 100, true);
    //public ClampedFloatParameter EdgeLightFlowThickness = new ClampedFloatParameter(0, 0, 200, true);
    //public ClampedFloatParameter EdgeLightFlowThreshold = new ClampedFloatParameter(0, 0, 1f, true);
    //public ColorParameter EdgeLightFlowColor = new ColorParameter(new Color(0.4039f, 0.5843f, 0.6118f), true, true, true, true);


    public bool IsActive() => true;
    public bool IsTileCompatible() => true;
}
