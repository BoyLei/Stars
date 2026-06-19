using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System;

public class SGame_ShaderGUI_NastyFlowMap : ShaderGUI
{
    public enum BlendModes
    {
        Opaque,
        Cutout,
        Transparent,
        Additive,
    }

    private const string AlphaClippingKeyWord = "_ALPHACLIP";
    private const string AlphaChippingThresholdName = "_Cutoff";

    private const string VotKeyWord = "_VTO_TEX";

    public static readonly string[] blendNames = Enum.GetNames(typeof(BlendModes));

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material targetMat = materialEditor.target as Material;

        CullMode cullMode = (CullMode)targetMat.GetFloat("_CullMode");
        BlendModes blendMode = (BlendModes)targetMat.GetFloat("_BlendMode");

        bool alphaTestOn = targetMat.IsKeywordEnabled(AlphaClippingKeyWord);
        float alphaTestThreshold = targetMat.GetFloat(AlphaChippingThresholdName);

        blendMode = (BlendModes)EditorGUILayout.Popup("混合模式", (int)blendMode, blendNames);
        alphaTestOn = blendMode == BlendModes.Cutout;

        if (alphaTestOn)
            alphaTestThreshold = EditorGUILayout.Slider("Alpha Clip Value", alphaTestThreshold, 0f, 1f);

        cullMode = (CullMode)EditorGUILayout.EnumPopup("Culling Mode", (CullMode)cullMode);
        if (EditorGUI.EndChangeCheck())
        {
            if (alphaTestOn)
            {
                targetMat.EnableKeyword(AlphaClippingKeyWord);
                targetMat.renderQueue = (int)RenderQueue.AlphaTest;
            }
            else
            {
                targetMat.DisableKeyword(AlphaClippingKeyWord);
                targetMat.renderQueue = -1;
            }
            targetMat.SetFloat("_CullMode", (int)cullMode);
            targetMat.SetFloat("_BlendMode", (int)blendMode);
            targetMat.SetFloat(AlphaChippingThresholdName, alphaTestThreshold);

            switch (blendMode)
            {
                case BlendModes.Opaque:
                    targetMat.SetOverrideTag("RenderType", "");
                    targetMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    targetMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    targetMat.SetInt("_ZWrite", 1);
                    targetMat.DisableKeyword("_ALPHATEST_ON");
                    targetMat.renderQueue = -1;
                    break;
                case BlendModes.Cutout:
                    targetMat.SetOverrideTag("RenderType", "TransparentCutout");
                    targetMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    targetMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    targetMat.SetInt("_ZWrite", 1);
                    targetMat.EnableKeyword("_ALPHATEST_ON");
                    targetMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    break;
                case BlendModes.Transparent:
                    targetMat.SetOverrideTag("RenderType", "Transparent");
                    targetMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    targetMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    targetMat.SetInt("_ZWrite", 0);
                    targetMat.DisableKeyword("_ALPHATEST_ON");
                    targetMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendModes.Additive:
                    targetMat.SetOverrideTag("RenderType", "Transparent");
                    targetMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    targetMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    targetMat.SetInt("_ZWrite", 0);
                    targetMat.DisableKeyword("_ALPHATEST_ON");
                    targetMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
            }
        }

        var votEnabled = targetMat.IsKeywordEnabled(VotKeyWord);
        votEnabled = EditorGUILayout.Toggle("VOT Enabled?", votEnabled);
        if (votEnabled)
            targetMat.EnableKeyword(VotKeyWord);
        else
            targetMat.DisableKeyword(VotKeyWord);

        base.OnGUI(materialEditor, properties);
    }
}
