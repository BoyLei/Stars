using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.Rendering;

//#if UNITY_EDITOR
public class CartoonCharacterShaderGUI : UnityEditor.ShaderGUI
{
    private static bool ifFrozenVFX = false;    //存储是否启动角色冰冻效果的变量
    private static bool ifHead = false;         //存储是否是面部的渲染的变量
    private static string FrozenTexName = "_FrozenTex";
    private static string FrozneTexSTName = "_FrozenTex_ST";
    private static string FrozenColorName = "_FrozenColor";
    private static string FrozenSSSColorName = "_FrozenSSSColor";
    private static string FrozenRimRangeName = "_FrozenRimRange";
    private static string FrozenRimStrengthName = "_FrozenRimStrength";
    private static string FrozenConverName = "_FrozenConver";
    private static string FronzeConverthresoldName = "_FronzeConverthresold";
    private static string BaseColorDarknessName = "_BaseColorDarkness";
    private static string BaseColorColorName = "_BaseColorColor";
    private static string OutlineColorControlName = "_OutlineColorControl";

    private static string HeadShodwTexName = "_HeadShodwTex";
    private static string FaceControlSpeedName = "_FaceControlSpeed";
    private static string FaceControlOffsetName = "_FaceControlOffset";
    private static string FaceControlExpName = "_FaceControlExp";
    
    private static bool ifHair = false;
    private static string HairBasedSpecularName = "_HairSpecularBase";
    private static string HairSpecularthresholdName = "_HairSpecularthreshold";
    private static string SpecularIndencityName = "_SpecualrIndencity";
    private static string SpecularStrengthName = "_SpecularStrength";
    private static string MetalIntensityName = "_MetalIntensity";
    private static string MatCapName = "_MatCapTex";
    private static string MatCapColorName = "_MatCapColor";
    private static string MetallicStrengthName = "_MetallicStrength";

    private static string NormalTex = "_NormalTex";
    private static string DissolveTex = "_DissolveTex";

    private static string[] RenderWay = new string[] { "Body", "Face", "Hair" };
    private int Flag = 0;
    private static GUIStyle LabelStyle = new GUIStyle();

    public void SetKeyword(Material targetMat, bool toggle, string keyWord)
    {
        if (toggle == true)
            targetMat.EnableKeyword(keyWord);
        else
            targetMat.DisableKeyword(keyWord);
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        //获取目标材质球
        Material targetMat = materialEditor.target as Material;
        string[] keyWords = targetMat.shaderKeywords;
        EditorGUI.BeginChangeCheck();

        //绘制面板信息
        if (targetMat != null)
        {
            ifFrozenVFX = targetMat.IsKeywordEnabled("_ElemetalVFX");

            // Texture frozenTex = targetMat.GetTexture(FrozenTexName);
            // Vector4 frozenTexST = targetMat.GetVector(FrozneTexSTName);
            // Color frozenColor = targetMat.GetColor(FrozenColorName);
            // Color frozenSSSColor = targetMat.GetColor(FrozenSSSColorName);
            // float frozenRimRange = targetMat.GetFloat(FrozenRimRangeName);
            // float frozenRimStrength = targetMat.GetFloat(FrozenRimStrengthName);
            // float frozenCover = targetMat.GetFloat(FrozenConverName);
            // float frozenCoverThresold = targetMat.GetFloat(FronzeConverthresoldName);
            // float baseColorDarkness = targetMat.GetFloat(BaseColorDarknessName);
            // Color baseColorColor = targetMat.GetColor(BaseColorColorName);
            // float outlineColorControl = targetMat.GetFloat(OutlineColorControlName);

            Texture HeadShodwTex = targetMat.GetTexture(HeadShodwTexName);
            float FaceControlSpeed = targetMat.GetFloat(FaceControlSpeedName);
            float FaceControlOffset = targetMat.GetFloat(FaceControlOffsetName);
            float FaceControlExp = targetMat.GetFloat(FaceControlExpName);

            float hairBasedSpecular = targetMat.GetFloat(HairBasedSpecularName);
            float hairSpecularthreshold = targetMat.GetFloat(HairSpecularthresholdName);
            float specularIndencity = targetMat.GetFloat(SpecularIndencityName);
            float specularStrength = targetMat.GetFloat(SpecularStrengthName);
            float metalStrength = targetMat.GetFloat(MetalIntensityName);
            Texture matCapTex = targetMat.GetTexture(MatCapName);
            Color matCapColor = targetMat.GetColor(MatCapColorName);
            float MetallicStrength = targetMat.GetFloat(MetallicStrengthName);

            Texture normalTex = targetMat.GetTexture(NormalTex);
            Texture dissolveTex = targetMat.GetTexture(DissolveTex);

            Flag = targetMat.GetInt("_Flag");

            LabelStyle.fontStyle = FontStyle.Normal;
            LabelStyle.normal.textColor = new Color(126f, 126f, 126f);
            //绘制勾选按钮 控制材质是否是渲染面部的开关
            /*EditorGUI.BeginChangeCheck();
            ifHead = EditorGUILayout.Toggle("是否是面部渲染", ifHead);
            if (ifHead == true)
            {
                HeadShodwTex = (Texture)EditorGUILayout.ObjectField("头部阴影过度贴图",HeadShodwTex,typeof(Texture),false);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (ifHead == true)
                {
                    targetMat.EnableKeyword("_HEAD");
                    targetMat.SetTexture(HeadShodwTexName, HeadShodwTex);
                }
                else
                {
                    targetMat.DisableKeyword("_HEAD");
                }
            }*/
            /*//控制材质是否是头发渲染
            EditorGUI.BeginChangeCheck();
            ifHair = EditorGUILayout.Toggle("是否是头发渲染", ifHair);
            if (EditorGUI.EndChangeCheck())
            {
                if (ifHair == true)
                {
                    targetMat.EnableKeyword("_HAIR");
                }
                else
                {
                    targetMat.DisableKeyword("_HAIR");
                }
            }*/

            //绘制勾选按钮 控制material Keyword的开关

          //   ifFrozenVFX = EditorGUILayout.Toggle("是否开启冰冻特效", ifFrozenVFX);
          //   if (ifFrozenVFX == true)
          //   {
          //       frozenTex = (Texture)EditorGUILayout.ObjectField("冰冻基础颜色贴图", frozenTex, typeof(Texture), false);
          //       frozenTexST = EditorGUILayout.Vector4Field("冰冻贴图的 Tilling XY 和 OffsetXY:", frozenTexST);
          //       frozenColor = EditorGUILayout.ColorField("冰壳颜色", frozenColor);
          //       frozenSSSColor = EditorGUILayout.ColorField("冰壳次表面散射颜色", frozenSSSColor);
          //       frozenRimRange = EditorGUILayout.FloatField("冰冻边缘范围", frozenRimRange);
          //       frozenRimStrength = EditorGUILayout.Slider("冰冻边缘强度", frozenRimStrength,0f,10f);
          //       frozenCover = EditorGUILayout.FloatField("冰壳厚度", frozenCover);
          //       frozenCoverThresold = EditorGUILayout.FloatField("冰冻效果亮度阈值", frozenCoverThresold);
          //       baseColorDarkness = EditorGUILayout.FloatField("冰壳内角色暗度", baseColorDarkness);
          //       baseColorColor = EditorGUILayout.ColorField("冰壳内角色颜色倾向", baseColorColor);
          //       outlineColorControl = EditorGUILayout.FloatField("描边的颜色明暗调整", outlineColorControl);
          //   }
          //
          //   if (ifFrozenVFX == true)    //冰冻特效开启 则给材质内对应的参数进行赋值
          //   {
          //       targetMat.EnableKeyword("_ElemetalVFX");
          //
          //       targetMat.SetTexture(FrozenTexName, frozenTex);
          //       targetMat.SetVector(FrozneTexSTName, frozenTexST);
          //       targetMat.SetColor(FrozenColorName, frozenColor);
          //       targetMat.SetColor(FrozenSSSColorName, frozenSSSColor);
          //       targetMat.SetFloat(FrozenRimRangeName, frozenRimRange);
          //       targetMat.SetFloat(FrozenRimStrengthName, frozenRimStrength);
          //       targetMat.SetFloat(FrozenConverName, frozenCover);
          //       targetMat.SetFloat(FronzeConverthresoldName, frozenCoverThresold);
          //       targetMat.SetFloat(BaseColorDarknessName, baseColorDarkness);
          //       targetMat.SetColor(BaseColorColorName, baseColorColor);
          //       targetMat.SetFloat(OutlineColorControlName, outlineColorControl);
          //   }
          //   else
          //   {
          //       targetMat.DisableKeyword("_ElemetalVFX");
          //   }
          // //  Undo.RecordObject(targetMat, "_ElemetalVFX");


            Flag = EditorGUILayout.Popup("材质渲染的是物体:", Flag, RenderWay);
            if (Flag == 0)//渲染身体
            {
                EditorGUILayout.LabelField("Specular", LabelStyle);
                specularIndencity = EditorGUILayout.Slider("非金属部分高光范围", specularIndencity, 0f, 30f);
                specularStrength = EditorGUILayout.Slider("非金属部分高光整体强度", specularStrength, 0f, 3f);
                //metalStrength =  EditorGUILayout.Slider("金属部分高光强度", metalStrength, 0f, 10f);
                EditorGUILayout.LabelField("MatCap", LabelStyle);
                matCapTex = (Texture)EditorGUILayout.ObjectField("金属MatCap123321", matCapTex, typeof(Texture), false);
                GUIContent colorLabel = new GUIContent();
                colorLabel.text = "金属高光颜色";
                matCapColor = EditorGUILayout.ColorField(colorLabel, matCapColor, true, true, true);
                MetallicStrength = EditorGUILayout.Slider("金属颜色的明暗程度", MetallicStrength, 0f, 1f);

            }
            else if (Flag == 1)//渲染头部
            {
                HeadShodwTex = (Texture)EditorGUILayout.ObjectField("头部阴影过度贴图", HeadShodwTex, typeof(Texture), false);
                FaceControlSpeed = EditorGUILayout.Slider("面部阴影过渡速度 默认1", FaceControlSpeed, 0.5f, 1.5f);
                FaceControlOffset = EditorGUILayout.Slider("面部阴影过渡偏移值 默认0", FaceControlOffset, -0.5f, 0.5f);
                FaceControlExp = EditorGUILayout.Slider("sdf贴图缩放值 默认1", FaceControlExp, 1, 2);
            }
            else if (Flag == 2)//渲染头发
            {
                EditorGUILayout.LabelField("Specular", LabelStyle);
                hairBasedSpecular = EditorGUILayout.Slider("头发的基础高光度", hairBasedSpecular, 0f, 0.3f);
                hairSpecularthreshold = EditorGUILayout.Slider("头发的高光的上限阈值", hairSpecularthreshold, 0f, 1f);
                specularIndencity = EditorGUILayout.Slider("非金属部分高光范围", specularIndencity, 0f, 30f);
                specularStrength = EditorGUILayout.Slider("非金属部分高光整体强度", specularStrength, 0f, 3f);
            }



            if (Flag == 0) //渲染身体
            {
                targetMat.DisableKeyword("_HEAD");
                targetMat.DisableKeyword("_HAIR");
                targetMat.SetFloat(SpecularIndencityName, specularIndencity);
                targetMat.SetFloat(SpecularStrengthName, specularStrength);
                //targetMat.SetFloat(MetalIntensityName,metalStrength);
                targetMat.SetTexture(MatCapName, matCapTex);
                targetMat.SetColor(MatCapColorName, matCapColor);
                targetMat.SetFloat(MetallicStrengthName, MetallicStrength);
                targetMat.SetInt("_Flag", 0);
            }
            else if (Flag == 1) //渲染头部
            {
                targetMat.EnableKeyword("_HEAD");
                targetMat.DisableKeyword("_HAIR");
                //HeadShodwTex = (Texture)EditorGUILayout.ObjectField("头部阴影过度贴图",HeadShodwTex,typeof(Texture),false);
                targetMat.SetTexture(HeadShodwTexName, HeadShodwTex);
                targetMat.SetInt("_Flag", 1);
                targetMat.SetFloat(FaceControlSpeedName,FaceControlSpeed);
                targetMat.SetFloat(FaceControlOffsetName,FaceControlOffset);
                targetMat.SetFloat(FaceControlExpName,FaceControlExp);
            }
            else if (Flag == 2) //渲染头发
            {
                targetMat.EnableKeyword("_HAIR");
                targetMat.DisableKeyword("_HEAD");
                targetMat.SetFloat(HairBasedSpecularName, hairBasedSpecular);
                targetMat.SetFloat(HairSpecularthresholdName,hairSpecularthreshold);
                targetMat.SetFloat(SpecularIndencityName, specularIndencity);
                targetMat.SetFloat(SpecularStrengthName, specularStrength);
                targetMat.SetInt("_Flag", 2);
            }

            /*EditorGUI.BeginChangeCheck();
            if (ifHair = true)
            {
                hairBasedSpecular = EditorGUILayout.Slider("头发基础高光强度", hairBasedSpecular, 0f, 0.3f);
                specularIndencity = EditorGUILayout.Slider("中间高光范围", specularIndencity, 0f, 30f);
                specularStrength = EditorGUILayout.Slider("中间高光强度", specularStrength, 0f, 3f);
            }
            else
            {
                specularIndencity = EditorGUILayout.Slider("中间高光范围", specularIndencity, 0f, 30f);
                specularStrength = EditorGUILayout.Slider("中间高光强度", specularStrength, 0f, 3f);
            }*/

        }


        //绘制shader面板不隐藏的信息
        base.OnGUI(materialEditor, properties);
        bool openDissolve = false;
        MaterialProperty Dissolve_Tex = FindProperty("_DissolveTex", properties);
        if (Dissolve_Tex.textureValue != null)
        {
            openDissolve = true;

        }
        else
        {
            openDissolve = false;
        }

        bool openUVMove = false;
        MaterialProperty Normal_Tex = FindProperty("_NormalTex", properties);
        if (Normal_Tex.textureValue != null)
        {
            openUVMove = true;
        }
        else
        {
            openUVMove = false;
        }

        // if (EditorGUI.EndChangeCheck())
        // {
        if (openDissolve)
        {
            targetMat.EnableKeyword("_DISSOLVE_COMMON");
        }
        else
        {
            targetMat.DisableKeyword("_DISSOLVE_COMMON");
        }

        if (openUVMove)
        {
            targetMat.EnableKeyword("_UVMOVE_COMMON");
        }
        else
        {
            targetMat.DisableKeyword("_UVMOVE_COMMON");
        }
        //}
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetMat, "mat");
        }
    }

    public GUILayoutOption[] shortButtonStyle = new GUILayoutOption[] { GUILayout.Width(100) };
    void GUI_Save(Material material)
    {
        AssetDatabase.SaveAssetIfDirty(material);
    }
}

//#endif