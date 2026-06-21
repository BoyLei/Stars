using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class SceneShader01GUI : UnityEditor.ShaderGUI
{

    private static string detailTexFlagName = "DetailFlag";
    private int DetailTexFlag = 0; //记录是否使用顶部的细节贴图, 比如显示在石头上方的草 


    private const string OcclusionClippingKeyWord = "_OCCLUSIONCLIP";  //開啓遮挡剔除的關鍵字

    private const string AlphaClippingKeyWord = "_ALPHACLIP";  //開啓AlphaClipping的關鍵字
    private const string AlphaChippingThresholdName = "_Cutoff"; //控制AlphaClip的閾值
    //private static string[] SkillMode = new string[] { "Opaque","Transparent"};
    private static string[] CullMode = new string[] { "显示双面", "显示背面", "显示正面" };
    private static string[] BlendMode = new string[] { "无视透明度直接覆盖", "透明度混合" };

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        //获取目标材质球
        Material targetMat = materialEditor.target as Material;

        EditorGUI.BeginChangeCheck();

        int detailTexFlag = targetMat.GetInt(detailTexFlagName);
        bool detailBool = false;
        bool alphaTestBool = targetMat.IsKeywordEnabled(AlphaClippingKeyWord); //是否開啓AlphaClipping

        float alphaTestThreshold = targetMat.GetFloat(AlphaChippingThresholdName); //AlphaClip的閾值
        float cullMode = targetMat.GetFloat("_Cullmode");
        float blendMode = targetMat.GetFloat("_Blendmode");//0-- 直接覆盖 1--透明度混合


        float normalUseFlag = targetMat.GetFloat("_NormalUseFlag");
        if (normalUseFlag == 1)
        {
            targetMat.EnableKeyword("_NORMALMAP");
        }
        else
        {
            targetMat.DisableKeyword("_NORMALMAP");
        }

        bool occlusionClippingBool = targetMat.IsKeywordEnabled(OcclusionClippingKeyWord);//是否開啓遮挡剔除

        if (detailTexFlag == 1)
        {
            detailBool = true;
        }
        else
        {
            detailBool = false;
        }

        detailBool = EditorGUILayout.Toggle("是否启用顶部的草", detailBool);

        if (detailBool)
        {
            detailTexFlag = 1;
            targetMat.EnableKeyword("_DETAILTEX");
        }
        else
        {
            detailTexFlag = 0;
            targetMat.DisableKeyword("_DETAILTEX");
        }
        //Flag = EditorGUILayout.Popup("技能指示器形状", Flag, SkillMode);

        //设置混合模式
        blendMode = (float)EditorGUILayout.Popup("混合模式", (int)blendMode, BlendMode);
        //是否開啓AlphaClipping
        alphaTestBool = EditorGUILayout.Toggle("是否开启AlphaClipping", alphaTestBool);

        if (alphaTestBool)
        {
            alphaTestThreshold = EditorGUILayout.Slider("阈值", alphaTestThreshold, 0f, 1f);
            // EditorGUILayout.LabelField(alphaTestThreshold.ToString());
        }
        //EditorGUILayout.LabelField(cullMode.ToString());
        cullMode = (float)EditorGUILayout.Popup("剔除模式", (int)cullMode, CullMode);
        if (EditorGUI.EndChangeCheck())
        {
            if (alphaTestBool)
            {
                targetMat.EnableKeyword(AlphaClippingKeyWord);
                targetMat.renderQueue = (int)RenderQueue.AlphaTest;

            }
            else
            {
                targetMat.DisableKeyword(AlphaClippingKeyWord);
                targetMat.renderQueue = -1;
            }
            targetMat.SetFloat("_Cullmode", cullMode);
            targetMat.SetFloat("_Blendmode", blendMode);
            targetMat.SetInt(detailTexFlagName, detailTexFlag);
            targetMat.SetFloat(AlphaChippingThresholdName, alphaTestThreshold);

            if (blendMode == 0)//无视透明度直接覆盖
            {
                targetMat.SetInt("_SrcFactorSG", (int)UnityEngine.Rendering.BlendMode.One);
                targetMat.SetInt("_DstFactorSG", (int)UnityEngine.Rendering.BlendMode.Zero);
            }
            else if (blendMode == 1)//透明度混合
            {
                targetMat.SetInt("_SrcFactorSG", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                targetMat.SetInt("_DstFactorSG", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }
        }


        /*
        occlusionClippingBool = EditorGUILayout.Toggle("是否开启角色遮挡剔除", occlusionClippingBool);
        if (occlusionClippingBool)
        {
            //alphaTestThreshold = EditorGUILayout.Slider("阈值", alphaTestThreshold, 0f, 1f);
        }
        if (EditorGUI.EndChangeCheck())
        {
            if (occlusionClippingBool)
            {
                targetMat.EnableKeyword(OcclusionClippingKeyWord);

            }
            else
            {
                targetMat.DisableKeyword(OcclusionClippingKeyWord);
            }
            //targetMat.SetInt(detailTexFlagName, detailTexFlag);
            //targetMat.SetFloat(AlphaChippingThresholdName, alphaTestThreshold);
        }
        */

        targetMat.SetInteger("m_LighmapFlags",2);
        targetMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
        targetMat.doubleSidedGI = true;
        
        //绘制shader面板不隐藏的信息
        base.OnGUI(materialEditor, properties);

        if (EditorGUI.EndChangeCheck())
        {
            //同步材质球里的_Emission
            //
            EditorUtility.SetDirty(targetMat);
            Undo.RecordObject(targetMat, "mat");
        }
        
        materialEditor.LightmapEmissionProperty(2);
        
    }
    void GUI_Save(Material material)
    {
        AssetDatabase.SaveAssetIfDirty(material);
    }
}