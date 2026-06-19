using StarProject.Game;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class ScreenFogAndCloudShadowRenderFeature : ScriptableRendererFeature
{
    Shader FeatureShader = null;
    SGameScreenFogAndCloudShadowPass mFogAndCloudShadowPass;
    Material mFeatureMaterial = null;

    public override void Create()
    {
        FeatureShader = Shader.Find("SGAME/Screen Fog And Cloud Shadow");
        if (FeatureShader)
            mFeatureMaterial = CoreUtils.CreateEngineMaterial(FeatureShader);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!FeatureShader)
            return;

        if (!renderingData.cameraData.postProcessEnabled)
            return;

        var screenfogCloudVolume = VolumeManager.instance.stack.GetComponent<ScreenFogAndCloudShadow>();
        if (!screenfogCloudVolume)
            return;

        if (!screenfogCloudVolume.active || !screenfogCloudVolume.IsActive())
            return;

        if (renderingData.cameraData.cameraType != CameraType.Game)
        {
            if (!screenfogCloudVolume.PreviewFogInSceneView.value && !screenfogCloudVolume.PreviewCloudInSceneView.value)
                return;

            if (mFogAndCloudShadowPass == null)
                mFogAndCloudShadowPass = new SGameScreenFogAndCloudShadowPass(FeatureShader, this, mFeatureMaterial);

            mFogAndCloudShadowPass.renderPassEvent = screenfogCloudVolume.PassEvent.value;

            mFogAndCloudShadowPass.ApplyParams(screenfogCloudVolume);

            mFogAndCloudShadowPass.SetFogEnable(screenfogCloudVolume.PreviewFogInSceneView.value);
            mFogAndCloudShadowPass.SetCloudEnable(screenfogCloudVolume.PreviewCloudInSceneView.value);

            renderer.EnqueuePass(mFogAndCloudShadowPass);
        }
        else
        {
            if (!screenfogCloudVolume.IsFogEnable.value && !screenfogCloudVolume.IsCloudShadowEnable.value)
                return;

            if (mFogAndCloudShadowPass == null)
                mFogAndCloudShadowPass = new SGameScreenFogAndCloudShadowPass(FeatureShader, this, mFeatureMaterial);

            mFogAndCloudShadowPass.renderPassEvent = screenfogCloudVolume.PassEvent.value;

            mFogAndCloudShadowPass.ApplyParams(screenfogCloudVolume);
            renderer.EnqueuePass(mFogAndCloudShadowPass);
        }
    }


    private void OnDestroy()
    {
        if (mFogAndCloudShadowPass != null)
            mFogAndCloudShadowPass.Cleanup();
        mFogAndCloudShadowPass = null;
        CoreUtils.Destroy(mFeatureMaterial);
    }

    protected override void Dispose(bool disposing)
    {
        if (mFogAndCloudShadowPass != null)
            mFogAndCloudShadowPass.Cleanup();
        mFogAndCloudShadowPass = null;
        CoreUtils.Destroy(mFeatureMaterial);
    }
}

class SGameScreenFogAndCloudShadowPass : ScriptableRenderPass
{
    static readonly string Keyword_EogEnable = "_FOG_ENABLE";
    static readonly string Keyword_FadeSkyEnable = "_FADE_SKY";
    static readonly string Keyword_FogMixAdd = "_FOG_MIX_ADD";
    static readonly string Keyword_FogMixBlend = "_FOG_MIX_BLEND";
    static readonly string Keyword_PlayerInterActive = "_PLAYER_INTERACTIVE";


    static readonly int ShaderID_FogColor = Shader.PropertyToID("_FogColor");
    static readonly int ShaderID_SkyColor = Shader.PropertyToID("_SkyColor");

    static readonly int ShaderID_FogInscatteringColor = Shader.PropertyToID("_FogInscatteringColor");
    static readonly int ShaderID_FogExtinctionColor = Shader.PropertyToID("_FogExtinctionColor");
    static readonly int ShaderID_FogSunColor = Shader.PropertyToID("_FogSunColor");
    static readonly int ShaderID_SunFalloff = Shader.PropertyToID("_FogSunFalloff");
    static readonly int ShaderID_FogBlendFactor = Shader.PropertyToID("_FogBlendFactor");



    //static readonly int ShaderID_InscatteringFallOff = Shader.PropertyToID("_InscatteringFallOff");
    //static readonly int ShaderID_ExtinctionFallOff = Shader.PropertyToID("_ExtinctionFallOff");

    static readonly int ShaderID_SunLightDir = Shader.PropertyToID("_SunLightDir");

    static readonly int ShaderID_FogNoiseTexture = Shader.PropertyToID("_FogNoiseTexture");
    static readonly int ShaderID_FogNoiseSize1 = Shader.PropertyToID("_FogNoiseSize1");
    static readonly int ShaderID_FogNoiseSize2 = Shader.PropertyToID("_FogNoiseSize2");
    static readonly int ShaderID_FogMoveSpeed = Shader.PropertyToID("_FogMoveSpeed");
    static readonly int ShaderID_FogNoiseContract = Shader.PropertyToID("_FogNoiseContract");
    static readonly int ShaderID_FogNoiseDepthFalloff = Shader.PropertyToID("_FogNoiseDepthFalloff");


    static readonly int ShaderID_FogHeightStart = Shader.PropertyToID("_FogHeightStart");
    static readonly int ShaderID_FogHeight = Shader.PropertyToID("_FogHeight");
    static readonly int ShaderID_HeightFogFalloff = Shader.PropertyToID("_HeightFogFalloff");
    static readonly int ShaderID_HeightFogDensity = Shader.PropertyToID("_HeightFogDensity");

    static readonly int ShaderID_DepthFogDensity = Shader.PropertyToID("_DepthFogDensity");
    static readonly int ShaderID_DepthFogFalloff = Shader.PropertyToID("_DepthFogFalloff");
    static readonly int ShaderID_DepthFogLinearGrad = Shader.PropertyToID("_LinearGrad");
    static readonly int ShaderID_DepthFogLinearOffs = Shader.PropertyToID("_LinearOffs");
    static readonly int ShaderID_DepthFogDistanceOffset = Shader.PropertyToID("_DistanceOffset");

    static readonly string Keyword_CloudShadowEnable = "_CLOUDSHADOW_ENABLE";

    static readonly int ShaderID_CloudShadowTexture = Shader.PropertyToID("_CloudShadowTexture");
    static readonly int ShaderID_CloudShadowMoveSpeed = Shader.PropertyToID("_CloudShadowMoveSpeed");
    static readonly int ShaderID_CloudShadowColor = Shader.PropertyToID("_CloudShadowColor");
    static readonly int ShaderID_CloudShadowSize = Shader.PropertyToID("_CloudShadowSize");

    static readonly int ShaderID_TransmittanceLut = Shader.PropertyToID("_TransmittanceLut");
    static readonly int ShaderID_FogLightmap = Shader.PropertyToID("_FogLightmap");
    static readonly int ShaderID_FogLightmapPosAndSize = Shader.PropertyToID("_FogLightmapPosAndSize");
    static readonly int ShaderID_FogLightmapColor = Shader.PropertyToID("_FogLightmapColor");
    static readonly int ShaderID_FogLightmapFalloff = Shader.PropertyToID("_FogLightmapFalloff");
    static readonly int ShaderID_FogLightmapFalloffMin = Shader.PropertyToID("_FogLightmapFalloffMin");

    static readonly int ShaderID_FogLightmapDistanceFalloff = Shader.PropertyToID("_FogLightmapDistanceFalloff");


    static readonly int ShaderID_PlayerPosAndRadius = Shader.PropertyToID("_PlayerPosAndRadius");
    static readonly int ShaderID_InteractiveFalloff = Shader.PropertyToID("_InteractiveFalloff");




    public Texture2D TransmittanceLut = null;
    public float TransmittanceDistance = 100f;

    static readonly ProfilingSampler mProfilingSampler = new ProfilingSampler("ScreenFogAndCloudShadow");
    Material mFeatureMaterial = null;


    public SGameScreenFogAndCloudShadowPass(Shader featureShader, ScreenFogAndCloudShadowRenderFeature parentRenderFeature, Material featureMaterial)
    {
        mFeatureMaterial = featureMaterial;
    }

    public void ApplyParams(ScreenFogAndCloudShadow volume)
    {
        if (!mFeatureMaterial)
            return;

        if (volume.IsFogEnable.value)
        {
            mFeatureMaterial.SetFloat(ShaderID_FogHeightStart, volume.FogHeightStart.value);

            mFeatureMaterial.SetFloat(ShaderID_FogHeight, volume.FogHeight.value);
            mFeatureMaterial.SetColor(ShaderID_FogColor, volume.FogColor.value);
            mFeatureMaterial.SetColor(ShaderID_SkyColor, volume.SkyColor.value);

            //mFeatureMaterial.SetColor(ShaderID_FogInscatteringColor, volume.FogInscatteringColor.value);
            //mFeatureMaterial.SetColor(ShaderID_FogExtinctionColor, volume.FogExtinctionColor.value);
            //mFeatureMaterial.SetColor(ShaderID_FogSunColor, volume.FogSunColor.value);
            //mFeatureMaterial.SetFloat(ShaderID_InscatteringFallOff, volume.InscatteringFallOff.value);
            //mFeatureMaterial.SetFloat(ShaderID_ExtinctionFallOff, volume.ExtinctionFallOff.value);
            //mFeatureMaterial.SetFloat(ShaderID_SunFalloff, volume.SunFalloff.value);
            //var sunlightDir = new Vector3(Mathf.Cos(Mathf.Deg2Rad * volume.SunLightAngle.value), Mathf.Deg2Rad * volume.SunLightVerticalAngle.value, Mathf.Sin(Mathf.Deg2Rad * volume.SunLightAngle.value));
            //mFeatureMaterial.SetVector(ShaderID_SunLightDir, sunlightDir.normalized);

            mFeatureMaterial.SetFloat(ShaderID_HeightFogDensity, volume.HeightFogDensity.value);
            mFeatureMaterial.SetFloat(ShaderID_HeightFogFalloff, volume.HeightFogFalloff.value);
            mFeatureMaterial.SetFloat(ShaderID_FogNoiseContract, volume.FogNoiseContract.value);

            mFeatureMaterial.SetFloat(ShaderID_DepthFogDensity, volume.DepthFogDensity.value);
            mFeatureMaterial.SetFloat(ShaderID_DepthFogFalloff, volume.DepthFogFalloff.value);

            var invDiff = 1.0f / Mathf.Max(volume.DepthFogEnd.value - volume.DepthFogStart.value, 1.0e-6f);

            mFeatureMaterial.SetFloat(ShaderID_DepthFogLinearGrad, -invDiff);
            mFeatureMaterial.SetFloat(ShaderID_DepthFogLinearOffs, volume.DepthFogEnd.value * invDiff);
            mFeatureMaterial.SetFloat(ShaderID_DepthFogDistanceOffset, volume.DepthFogStart.value);

            mFeatureMaterial.SetFloat(ShaderID_FogNoiseSize1, 1 / volume.FogNoiseSize1.value);
            mFeatureMaterial.SetFloat(ShaderID_FogNoiseSize2, 1 / volume.FogNoiseSize2.value);
            mFeatureMaterial.SetVector(ShaderID_FogMoveSpeed, new Vector4(volume.FogNoiseMoveSpeed1.value.x, volume.FogNoiseMoveSpeed1.value.y, volume.FogNoiseMoveSpeed2.value.x, volume.FogNoiseMoveSpeed2.value.y));
            mFeatureMaterial.SetTexture(ShaderID_FogNoiseTexture, volume.FogNoiseTexture.value);
            mFeatureMaterial.SetFloat(ShaderID_FogNoiseDepthFalloff, volume.FogNoiseDepthFalloff.value);


            //mFeatureMaterial.SetTexture(ShaderID_TransmittanceLut, volume.TransmittanceLut.value);
            mFeatureMaterial.SetTexture(ShaderID_FogLightmap, volume.FogLightmap.value);
            mFeatureMaterial.SetVector(ShaderID_FogLightmapPosAndSize, volume.FogLightmapPosAndSize.value);
            mFeatureMaterial.SetColor(ShaderID_FogLightmapColor, volume.FogLightmapColor.value);
            mFeatureMaterial.SetFloat(ShaderID_FogLightmapDistanceFalloff, volume.FogLightmapDistanceFalloff.value);
            mFeatureMaterial.SetFloat(ShaderID_FogLightmapFalloff, volume.FogLightmapFalloff.value);
            mFeatureMaterial.SetVector(ShaderID_FogLightmapFalloffMin, new Vector2(volume.FogLightmapDistanceMin.value, volume.FogLightmapDistanceFalloff.value));



            mFeatureMaterial.EnableKeyword(Keyword_EogEnable);

            if (volume.FadeToSky.value)
                mFeatureMaterial.EnableKeyword(Keyword_FadeSkyEnable);
            else
                mFeatureMaterial.DisableKeyword(Keyword_FadeSkyEnable);

            switch (volume.FogBlendMode.value)
            {
                case ScreenFogBlendMode.Add:
                    {
                        mFeatureMaterial.EnableKeyword(Keyword_FogMixAdd);
                        mFeatureMaterial.DisableKeyword(Keyword_FogMixBlend);
                    }
                    break;
                case ScreenFogBlendMode.Blend:
                    {
                        mFeatureMaterial.DisableKeyword(Keyword_FogMixAdd);
                        mFeatureMaterial.EnableKeyword(Keyword_FogMixBlend);
                        mFeatureMaterial.SetFloat(ShaderID_FogBlendFactor, volume.FogBlendFactor.value);
                    }
                    break;
                case ScreenFogBlendMode.Multiple:
                    {
                        mFeatureMaterial.DisableKeyword(Keyword_FogMixAdd);
                        mFeatureMaterial.DisableKeyword(Keyword_FogMixBlend);
                    }
                    break;
            }

            if (Application.isPlaying)
            {
                if (GameManager.Instance.M_MainPlayerCtrlBase != null && GameManager.Instance.M_MainPlayerCtrlBase.M_Curr != null && volume.PlayerInteractivable.value)
                {
                    mFeatureMaterial.EnableKeyword(Keyword_PlayerInterActive);
                    Vector3 playerPos = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();
                    mFeatureMaterial.SetVector(ShaderID_PlayerPosAndRadius, new Vector4(playerPos.x, playerPos.y, playerPos.z, volume.InteractiveRadius.value));
                    mFeatureMaterial.SetFloat(ShaderID_InteractiveFalloff, volume.InteractiveFalloff.value);
                }
                else
                    mFeatureMaterial.DisableKeyword(Keyword_PlayerInterActive);
            }
            else
            {
#if UNITY_EDITOR
                var previewInteractiveObj = UnityEditor.EditorUtility.InstanceIDToObject(volume.ScenePreviewInteractive.value);
                if (previewInteractiveObj && previewInteractiveObj is Transform && volume.PlayerInteractivable.value)
                {
                    mFeatureMaterial.EnableKeyword(Keyword_PlayerInterActive);
                    Vector3 previewPos = (previewInteractiveObj as Transform).transform.position;
                    mFeatureMaterial.SetVector(ShaderID_PlayerPosAndRadius, new Vector4(previewPos.x, previewPos.y, previewPos.z, volume.InteractiveRadius.value));
                    mFeatureMaterial.SetFloat(ShaderID_InteractiveFalloff, volume.InteractiveFalloff.value);
                }
                else
                {
                    mFeatureMaterial.DisableKeyword(Keyword_PlayerInterActive);
                }
#endif
            }
        }
        else
            mFeatureMaterial.DisableKeyword(Keyword_EogEnable);

        if (volume.IsCloudShadowEnable.value)
        {
            mFeatureMaterial.SetTexture(ShaderID_CloudShadowTexture, volume.CloudShadowTexture.value);
            mFeatureMaterial.SetVector(ShaderID_CloudShadowMoveSpeed, volume.CloudShadowMoveSpeed.value);
            mFeatureMaterial.SetColor(ShaderID_CloudShadowColor, volume.CloudShadowColor.value);
            mFeatureMaterial.SetFloat(ShaderID_CloudShadowSize, 1 / volume.CloudShadowSize.value);

            mFeatureMaterial.EnableKeyword(Keyword_CloudShadowEnable);
        }
        else
            mFeatureMaterial.DisableKeyword(Keyword_CloudShadowEnable);

    }

    public void SetFogEnable(bool enable)
    {
        if (enable)
            mFeatureMaterial.EnableKeyword(Keyword_EogEnable);
        else
            mFeatureMaterial.DisableKeyword(Keyword_EogEnable);
    }
    public void SetCloudEnable(bool enable)
    {
        if (enable)
            mFeatureMaterial.EnableKeyword(Keyword_CloudShadowEnable);
        else
            mFeatureMaterial.DisableKeyword(Keyword_CloudShadowEnable);
    }

    static readonly string CommandBufferName = "Screen Cloud And Fog";

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