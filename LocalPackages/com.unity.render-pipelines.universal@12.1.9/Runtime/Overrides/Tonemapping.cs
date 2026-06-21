using System;

namespace UnityEngine.Rendering.Universal
{
    public enum TonemappingMode
    {
        None,
        Neutral, // Neutral tonemapper
        ACES,    // ACES Filmic reference tonemapper (custom approximation)
        Genshin
    }

    [Serializable, VolumeComponentMenuForRenderPipeline("Post-processing/Tonemapping", typeof(UniversalRenderPipeline))]
    public sealed class Tonemapping : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Select a tonemapping algorithm to use for the color grading process.")]
        public TonemappingModeParameter mode = new TonemappingModeParameter(TonemappingMode.None);

        [Tooltip("filmSlopes")]
        public  ClampedFloatParameter filmSlopes = new ClampedFloatParameter(1f, 0f, 3f);

        [Tooltip("FilmToest")]
        public  ClampedFloatParameter filmToest = new ClampedFloatParameter(1f, 0f, 3f);

        [Tooltip("FilmShoulderg")]
        public ClampedFloatParameter filmShoulderg = new ClampedFloatParameter(0.32f, 0f, 3f);

        [Tooltip("FilmShouldergt")]
        public ClampedFloatParameter filmShouldergt = new ClampedFloatParameter(0.32f, 0f, 3f);

        [Tooltip("FilmBlackClipe")]
        public ClampedFloatParameter filmBlackClipe = new ClampedFloatParameter(1.33f, 0f, 3f);

        [Tooltip("FilmBlackClipet")]
        public ClampedFloatParameter filmBlackClipet = new ClampedFloatParameter(0f, 0f, 3f);

        
        public bool IsActive() => mode.value != TonemappingMode.None;

        public bool IsTileCompatible() => true;
    }

    [Serializable]
    public sealed class TonemappingModeParameter : VolumeParameter<TonemappingMode> { public TonemappingModeParameter(TonemappingMode value, bool overrideState = false) : base(value, overrideState) { } }
}
