using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class SkillControlerGUI : UnityEditor.ShaderGUI
{

    private static string MainTexName = "_MainTex";
    private static string SDFTexName = "_SDFTex";
    private static string ColorName = "_Color";
    private static string GlowColorName = "_GlowColor";
    private static string GlowEndColorName = "_GlowEndColor";
    private static string RangeName = "_Range";
    private static string LengthName = "_Length";
    private static string LengthAdjustName = "_LengthAdjust";
    private static string BorderWidthName = "_BorderWidth";
    private static string GlowRangeName = "_GlowRange";
    private static string GlowBeginName = "_GlowBeign";
    private static string RingAlphaName = "_RingAlpha";
    private static string FlagName = "_Flag";
    private static string ShowName = "_Show";
    private static string GlowModeName = "_GlowMode";
    private static string InsideLightColorName = "_InsideLightColor";
    private static string InsideLightAlphaName = "_InsideLightAlpha";
    private static string AngleName = "_Angle";
    private static string OutlineName = "_Outline";
    private static string MiddleRingSizeName = "_MiddleRingSize";
    
   
    
    private static string[] SkillMode = new string[] { "方形","圆形","扇形","箭头"};
    private static string[] ShowMode = new string[] { "边框", "中心" };
    private static string[] GlowMode = new string[] { "沿着中心朝四周扩散", "朝着一边扩散" };
    private int SkillModeFlag = 0; //记录技能指示器的模式 0-方形 1-圆形 2-扇形 


    //自定义一个小按钮

    public GUILayoutOption[] shortButtonStyle = new GUILayoutOption[] { GUILayout.Width(100) };


    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        //获取目标材质球
        Material targetMat = materialEditor.target as Material;
        string[] keyWords = targetMat.shaderKeywords;

        //绘制面板信息
        if (targetMat != null)
        {
            Texture MainTex = targetMat.GetTexture(MainTexName);
            Texture SDFTex = targetMat.GetTexture(SDFTexName);
            Color color = targetMat.GetColor(ColorName);
            Color GlowColor = targetMat.GetColor(GlowColorName);
            Color GlowEndColor = targetMat.GetColor(GlowEndColorName);
            float Range = targetMat.GetFloat(RangeName);
            float Length = targetMat.GetFloat(LengthName);
            float GlowRange = targetMat.GetFloat(GlowRangeName);
            float GlowBeign= targetMat.GetFloat(GlowBeginName);
            float RingAlpha = targetMat.GetFloat(RingAlphaName);
            float Angle = targetMat.GetFloat(AngleName);
            float Outline = targetMat.GetFloat(OutlineName);
            float MiddleRing = targetMat.GetFloat(MiddleRingSizeName);
            float LengthAdjust = targetMat.GetFloat(LengthAdjustName);
            float BorderWidth = targetMat.GetFloat(BorderWidthName);

            int Flag = targetMat.GetInt(FlagName); //获取shader的运行模式, 0-矩形  1-圆形 2-扇形
            int ShowFlag = targetMat.GetInt(ShowName); //获取shader的显示模式, 0-边框 1-中心
            int GlowFlag = targetMat.GetInt(GlowModeName); //获取方框的光圈移动方向 0-以中心向四周扩散 1-朝着一边扩散
            
            Color InsideLightColor = targetMat.GetColor(InsideLightColorName);
            float InsideLightAlpha = targetMat.GetFloat(InsideLightAlphaName);







            EditorGUI.BeginChangeCheck();




            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel("显示在最前层");


            if (targetMat.GetFloat("_Ztest") == 4)
            {
                if (GUILayout.Button("否", shortButtonStyle))
                {
                    targetMat.SetFloat("_Ztest", 8);

                }
            }
            else
            {
                if (GUILayout.Button("是", shortButtonStyle))
                {
                    targetMat.SetFloat("_Ztest", 4);
                }
            }

            EditorGUILayout.EndHorizontal();



            Flag = EditorGUILayout.Popup("技能指示器形状", Flag, SkillMode);
            ShowFlag = EditorGUILayout.Popup("技能指示器显示", ShowFlag, ShowMode);
            //GlowFlag = EditorGUILayout.Popup("正方形的光晕扩散模式", GlowFlag, GlowMode);
            if (Flag==0&&ShowFlag==0)
            {
                GlowFlag = EditorGUILayout.Popup("光晕扩散模式", GlowFlag, GlowMode);
            }

            var colorContent = new GUIContent();
            colorContent.text = "框体主颜色 "+ColorName;
            var glowColorContent = new GUIContent();
            glowColorContent.text = "光圈开始颜色 " + GlowColorName;
            var glowEndColorContent = new GUIContent();
            glowEndColorContent.text = "光圈结束颜色 "+GlowEndColorName;
            var insideLightColorContent = new GUIContent();
            insideLightColorContent.text = "内发光颜色" + InsideLightColorName;
            
            if (Flag == 0) //矩形
            {
                if (ShowFlag == 0) //显示边框
                {
                    MainTex = (Texture)EditorGUILayout.ObjectField("主贴图 "+MainTexName,MainTex,typeof(Texture),false);
                    SDFTex = (Texture)EditorGUILayout.ObjectField("SDF贴图 "+SDFTexName,SDFTex,typeof(Texture),false);
                    color = EditorGUILayout.ColorField(colorContent,color,true,true,true);
                    GlowColor =  EditorGUILayout.ColorField(glowColorContent,GlowColor,true,true,true);
                    GlowEndColor = EditorGUILayout.ColorField(glowEndColorContent,GlowEndColor,true,true,true);
                    Range = EditorGUILayout.Slider("高的长度(米) "+RangeName,Range,1f,100f);
                    Length = EditorGUILayout.Slider("宽的长度(米) "+LengthName,Length,1f,100f);
                    LengthAdjust =  EditorGUILayout.Slider("调整边长的参数,让框体大小符合宽高的数据 "+LengthAdjustName,LengthAdjust,-0.5f,0.5f);
                    BorderWidth = EditorGUILayout.Slider("外框粗细矫正值,调整此值使_GlowRange为1时,光圈刚好触及外框 "+BorderWidthName,BorderWidth,0f,0.4f);
                    GlowRange = EditorGUILayout.Slider("光圈的大小 "+GlowRangeName,GlowRange,0f,1.5f);
                    RingAlpha = EditorGUILayout.Slider("边框的透明度 "+RingAlphaName,RingAlpha,0f,1f);
                    InsideLightColor = EditorGUILayout.ColorField(insideLightColorContent,InsideLightColor,true,true,true);
                    InsideLightAlpha = EditorGUILayout.Slider("内发光的透明度 " + InsideLightAlphaName,InsideLightAlpha,0f,3f);
                }
                else if (ShowFlag == 1)//显示中心
                {
                    MainTex = (Texture)EditorGUILayout.ObjectField("主贴图 "+MainTexName,MainTex,typeof(Texture),false);
                    //SDFTex = (Texture)EditorGUILayout.ObjectField("SDF贴图",SDFTex,typeof(Texture),false);
                    color = EditorGUILayout.ColorField(colorContent,color,true,true,true);
                    //GlowColor =  EditorGUILayout.ColorField(glowColorContent,GlowColor,true,true,true);
                    Range = EditorGUILayout.Slider("在边长的基础上增加长度(米) "+RangeName,Range,0f,100f);
                    Length = EditorGUILayout.Slider("与边框的宽长同步即可(米) " +LengthName,Length,1f,100f);
                    RingAlpha = EditorGUILayout.Slider("边框的透明度 "+RingAlphaName,RingAlpha,0f,1f);
                    GlowRange = EditorGUILayout.Slider("光圈的大小 "+GlowRangeName,GlowRange,0f,2f);
                }
            }
            else if (Flag == 1) //圆形
            {
               // targetMat.EnableKeyword("_CIRCULAR");
                if (ShowFlag == 0) //显示边框
                {
                   // targetMat.EnableKeyword("_RING");
                    MainTex = (Texture)EditorGUILayout.ObjectField("主贴图 " + MainTexName,MainTex,typeof(Texture),false);
                    color = EditorGUILayout.ColorField(colorContent,color,true,true,true);
                    GlowColor =  EditorGUILayout.ColorField(glowColorContent,GlowColor,true,true,true);
                    GlowEndColor = EditorGUILayout.ColorField(glowEndColorContent,GlowEndColor,true,true,true);
                    Range = EditorGUILayout.Slider("在圆的半径(米) "+RangeName,Range,0.5f,100f);
                    LengthAdjust =  EditorGUILayout.Slider("调整半径的参数,让框体大小符合宽高的数据 "+LengthAdjustName,LengthAdjust,-0.5f,0.5f);
                    BorderWidth = EditorGUILayout.Slider("外框粗细矫正值,调整此值使_GlowRange为1时,光圈刚好触及外框 "+BorderWidthName,BorderWidth,0f,0.4f);

                    //Length = EditorGUILayout.Slider("控制边框长短的参数",Length,0f,2f);
                    GlowRange = EditorGUILayout.Slider("光圈的大小 " + GlowRangeName,GlowRange,0f,1.5f);
                    RingAlpha = EditorGUILayout.Slider("边框的透明度 " + RingAlphaName,RingAlpha,0f,1f);
                    InsideLightColor = EditorGUILayout.ColorField(insideLightColorContent,InsideLightColor,true,true,true);
                    InsideLightAlpha = EditorGUILayout.Slider("内发光的透明度 " + InsideLightAlphaName,InsideLightAlpha,0f,3f);
                }
                else if (ShowFlag == 1) //显示中心
                {
                    //targetMat.EnableKeyword("_GLOW");
                   // SDFTex = (Texture)EditorGUILayout.ObjectField("SDF贴图",SDFTex,typeof(Texture),false);
                    MainTex = (Texture)EditorGUILayout.ObjectField("主贴图 "+MainTexName,MainTex,typeof(Texture),false);
                    color = EditorGUILayout.ColorField(colorContent,color,true,true,true);
                    Range = EditorGUILayout.Slider("在中心边长的基础上增加长度(米) " + RangeName,Range,0.5f,100f);
                    RingAlpha = EditorGUILayout.Slider("边框的透明度 " + RingAlphaName,RingAlpha,0f,1f);
                }
            }
            else if (Flag == 2)//扇形
            {
                MainTex = (Texture)EditorGUILayout.ObjectField("主贴图 " + MainTexName,MainTex,typeof(Texture),false);
                color = EditorGUILayout.ColorField(colorContent,color,true,true,true);
                GlowColor =  EditorGUILayout.ColorField(glowColorContent,GlowColor,true,true,true);
                GlowEndColor = EditorGUILayout.ColorField(glowEndColorContent,GlowEndColor,true,true,true);
                Range = EditorGUILayout.Slider("扇形半径,最小为0.5(米) " + RangeName,Range,0.5f,100f);
                LengthAdjust =  EditorGUILayout.Slider("调整半径的参数,让框体大小符合宽高的数据 "+LengthAdjustName,LengthAdjust,-0.5f,0.5f);
                BorderWidth = EditorGUILayout.Slider("外框粗细矫正值,调整此值使_GlowRange为1时,光圈刚好触及外框 "+BorderWidthName,BorderWidth,0f,0.4f);
                GlowRange = EditorGUILayout.Slider("光圈的大小 " + GlowRangeName,GlowRange,0f,1.5f);
                GlowBeign = EditorGUILayout.Slider("光圈起始位置,如果从圆心开始则为0 " + GlowBeginName,GlowBeign,0f,0.9f);
                RingAlpha = EditorGUILayout.Slider("边框的透明度 "+RingAlphaName,RingAlpha,0f,1f);
                Outline = EditorGUILayout.Slider("描边的宽度 "+OutlineName,Outline,0f,5f);
                Angle = EditorGUILayout.Slider("扇形展开的角度 "+AngleName,Angle,0f,360);
                MiddleRing = EditorGUILayout.Slider("中心圆圈占整个圆的比例(若无中心圆则拉至最左) "+MiddleRingSizeName,MiddleRing,0.20f,0.80f);
                InsideLightColor = EditorGUILayout.ColorField(insideLightColorContent,InsideLightColor,true,true,true);
                InsideLightAlpha = EditorGUILayout.Slider("内发光的透明度 " + InsideLightAlphaName,InsideLightAlpha,0f,3f);
                //Angle = Angle - Outline;
                //targetMat.EnableKeyword("_SECTOR");
            }
            else //箭头
            {
                MainTex = (Texture)EditorGUILayout.ObjectField("主贴图 " + MainTexName,MainTex,typeof(Texture),false);
                color = EditorGUILayout.ColorField(colorContent,color,true,true,true);
                Range = EditorGUILayout.Slider("箭头长度,最小为3(米) " + RangeName,Range,3f,100f);
                Length = EditorGUILayout.Slider("箭头宽度,最小为1.5(米) "+LengthName,Length,1f,100f);
                RingAlpha = EditorGUILayout.Slider("箭头的透明度 "+RingAlphaName,RingAlpha,0f,1f);
                InsideLightAlpha = EditorGUILayout.Slider("内发光的透明度 " + InsideLightAlphaName,InsideLightAlpha,0f,5f);
            }

            // Debug.Log("Flag:" + Flag + "  ShowFlag:" + ShowFlag);
            if (EditorGUI.EndChangeCheck())
            {
                if (Flag == 0)  //矩形
                {
                    targetMat.EnableKeyword("_RECT");
                    targetMat.DisableKeyword("_CIRCULAR");
                    targetMat.DisableKeyword("_SECTOR");
                    targetMat.DisableKeyword("_ARROW");
                    if (ShowFlag == 0) //显示边框
                    {
                        targetMat.EnableKeyword("_RING");
                        targetMat.DisableKeyword("_MIDDLE");
                        // MainTex = (Texture)EditorGUILayout.ObjectField("主贴图",MainTex,typeof(Texture),false);
                        // color = EditorGUILayout.ColorField("主颜色",color);
                        // Range = EditorGUILayout.Slider("控制边框大小的参数",Range,0f,2f);
                        // Length = EditorGUILayout.Slider("控制边框长短的参数",Range,0f,2f);
                        if (GlowFlag == 0) //光晕向四周扩散
                        {
                            targetMat.EnableKeyword("_FROMMIDDLE");
                            targetMat.DisableKeyword("_FROMSIDE");
                        }
                        else //光晕从一侧向另一侧扩散
                        {
                            targetMat.EnableKeyword("_FROMSIDE");
                            targetMat.DisableKeyword("_FROMMIDDLE");
                        }
                    }
                    else if (ShowFlag == 1)//显示中心
                    {
                        targetMat.DisableKeyword("_RING");
                        targetMat.EnableKeyword("_MIDDLE");
                        // SDFTex = (Texture)EditorGUILayout.ObjectField("SDF贴图",SDFTex,typeof(Texture),false);
                        // color = EditorGUILayout.ColorField("主颜色",color);
                        // GlowRange = EditorGUILayout.Slider("光圈的大小",Range,0f,2f);
                    }

                }
                else if (Flag == 1) //圆形
                {
                    targetMat.DisableKeyword("_RECT");
                    targetMat.EnableKeyword("_CIRCULAR");
                    targetMat.DisableKeyword("_SECTOR");
                    targetMat.DisableKeyword("_ARROW");
                    if (ShowFlag == 0) //显示边框
                    {
                        targetMat.EnableKeyword("_RING");
                        targetMat.DisableKeyword("_MIDDLE");
                        //MainTex = (Texture)EditorGUILayout.ObjectField("主贴图",MainTex,typeof(Texture),false);
                        
                    }
                    else if (ShowFlag == 1) //显示光圈
                    {
                        targetMat.DisableKeyword("_RING");
                        targetMat.EnableKeyword("_MIDDLE");
                        //SDFTex = (Texture)EditorGUILayout.ObjectField("SDF贴图",SDFTex,typeof(Texture),false);
                    }
                }
                else if (Flag == 2)//扇形
                {
                    targetMat.DisableKeyword("_RECT");
                    targetMat.DisableKeyword("_CIRCULAR");
                    targetMat.EnableKeyword("_SECTOR");
                    targetMat.DisableKeyword("_ARROW");
                }
                else//箭头
                {
                    targetMat.DisableKeyword("_RECT");
                    targetMat.DisableKeyword("_CIRCULAR");
                    targetMat.DisableKeyword("_SECTOR");
                    targetMat.EnableKeyword("_ARROW");
                }
            }
            
            //设置Material里的参数
            targetMat.SetTexture(MainTexName,MainTex);
            targetMat.SetTexture(SDFTexName,SDFTex);
            targetMat.SetColor(ColorName,color);
            targetMat.SetColor(GlowColorName,GlowColor);
            targetMat.SetColor(GlowEndColorName,GlowEndColor);
            targetMat.SetFloat(RangeName,Range);
            targetMat.SetFloat(LengthName,Length);
            targetMat.SetFloat(GlowRangeName,GlowRange);
            targetMat.SetFloat(GlowBeginName,GlowBeign);
            targetMat.SetFloat(RingAlphaName,RingAlpha);
            targetMat.SetFloat(AngleName,Angle);
            targetMat.SetFloat(OutlineName,Outline);
            targetMat.SetFloat(MiddleRingSizeName,MiddleRing);
            targetMat.SetInt(FlagName,Flag);
            targetMat.SetInt(ShowName,ShowFlag);
            targetMat.SetInt(GlowModeName,GlowFlag);
            targetMat.SetColor(InsideLightColorName,InsideLightColor);
            targetMat.SetFloat(InsideLightAlphaName,InsideLightAlpha);
            targetMat.SetFloat(LengthAdjustName,LengthAdjust);
            targetMat.SetFloat(BorderWidthName,BorderWidth);


        }
        
        //绘制shader面板不隐藏的信息
        base.OnGUI(materialEditor, properties);
    }
}
