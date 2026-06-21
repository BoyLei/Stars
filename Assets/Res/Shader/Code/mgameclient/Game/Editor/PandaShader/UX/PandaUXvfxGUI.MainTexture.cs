// -----------------------------------------------------------------------
// This file is part of  PandaShader VFXGUI.
//
// (c) sunhainan   (2022/11/28 14:45:51)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Yoka.MGame.PandaShader
{
    public partial class PandaUXvfxGUI
    {
        //自定义主要贴图需要显示的属性
        private MaterialProperty mainTex_Sampler = null;
        private MaterialProperty TinColor = null;
        private MaterialProperty MainUspeed = null;
        private MaterialProperty MainVspeed = null;
        //private MaterialProperty MainTexrotat = null;
        private MaterialProperty MainTexC = null;
        private MaterialProperty MainTexCV = null;
        private MaterialProperty MainTexAR = null;
        private MaterialProperty BackFaceColor = null;
        private MaterialProperty Face = null;
        private MaterialProperty MainOffsetUC1 = null;

        private MaterialProperty MainOffsetVC1 = null;

        private MaterialProperty MainTexUVS = null;
        //private MaterialProperty TexCenter = null;
        //private MaterialProperty CenterU = null;
        //private MaterialProperty CenterV = null;

        private MaterialProperty MainOffsetUC2Vec4 = null;
        private CUSTOMDATA MainOffsetUC2Enum = CUSTOMDATA.X;
        private MaterialProperty MainOffsetVC2Vec4 = null;
        private CUSTOMDATA MainOffsetVC2Enum = CUSTOMDATA.Y;

        MaterialProperty MainRotate = null;
        MaterialProperty MainTexRefine = null;

        void OnGUIMainTexture(Material material)
        {
            //主帖图下拉菜单
            if (mainTex_Sampler.textureValue != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                _Maintextures_Foldout = Foldout(_Maintextures_Foldout, "主贴图(MainTexture)");

                if (_Maintextures_Foldout)
                {
                    EditorGUI.indentLevel++;
                    GUIMainTexture(material);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _Maintextures_Foldout = Foldout2(_Maintextures_Foldout, "主贴图(MainTexture)");

                if (_Maintextures_Foldout)
                {
                    EditorGUI.indentLevel++;
                    GUIMainTexture(material);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
        }

        //主帖图具体显示内容
        void GUIMainTexture(Material material)
        {
            m_MaterialEditor.TexturePropertySingleLine(new GUIContent("主贴图"), mainTex_Sampler, TinColor, BackFaceColor);
            m_MaterialEditor.ShaderProperty(MainTexAR, "使用R通道");
            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*主贴图用做主色纹理基底，后面跟着的两个颜色变量分别为正面主色和背面叠加色", style);
                EditorGUILayout.EndVertical();
            }

            if (mainTex_Sampler.textureValue != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _Mainxx = Foldouts(_Mainxx, "贴图设置(点击打开)");

                if (_Mainxx)
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.BeginHorizontal();
                    m_MaterialEditor.ShaderProperty(MainTexC, "UVClamp");

                    m_MaterialEditor.ShaderProperty(MainTexCV, "");

                    EditorGUILayout.EndHorizontal();

                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*两个复选框分别表示贴图U方向Clamp和V方向Clamp，可以勾选其中一个，也可以两个方向都勾选", style);
                        EditorGUILayout.EndVertical();
                    }

                    m_MaterialEditor.ShaderProperty(MainTexAR, "使用R通道");

                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*勾选后Red通道成为透明通道，不勾选，Alpha通道为透明通道，没有透明通道的黑白图可以勾选此选项，有透明通道不要勾选", style);
                        EditorGUILayout.EndVertical();
                    }

                    m_MaterialEditor.ShaderProperty(MainRotate, "贴图旋转");
                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*贴图旋转，处理贴图朝向，省去复制贴图改变朝向", style);
                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.LabelField("Refine（X:贴图1的对比度, Y:贴图2的对比度 Z:贴图2的pow值 W:0-1代表贴图1和2之间的过度)");
                    m_MaterialEditor.ShaderProperty(MainTexRefine, "Refine");


                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();

                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label("*展开后可以对贴图属性做一些设置，包含uvclamp，透明通道，旋转，校色", style);
                    EditorGUILayout.EndVertical();
                }

                m_MaterialEditor.TextureScaleOffsetProperty(mainTex_Sampler);
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("自定义偏移");
                if (material.GetFloat("_CustomdataMainTexUV") == 0)
                {
                    if (GUILayout.Button("已关闭", shortButtonStyle))
                    {
                        material.SetFloat("_CustomdataMainTexUV", 1);
                        material.EnableKeyword("_MAINTEX_CUSTOMDATA");
                    }
                }
                else
                {
                    if (GUILayout.Button("已开启", shortButtonStyle))
                    {
                        material.SetFloat("_CustomdataMainTexUV", 0);
                        material.DisableKeyword("_MAINTEX_CUSTOMDATA");
                    }
                }

                EditorGUILayout.EndHorizontal();

                if (material.GetFloat("_CustomdataMainTexUV") == 1)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    m_MaterialEditor.ShaderProperty(MainOffsetUC1, "U数据");
                    DrawCustomDataEnum(ref MainOffsetUC2Vec4, ref MainOffsetUC2Enum, "U通道");

                    GUILayout.Space(5);

                    m_MaterialEditor.ShaderProperty(MainOffsetVC1, "V数据");
                    DrawCustomDataEnum(ref MainOffsetVC2Vec4, ref MainOffsetVC2Enum, "V通道");

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PrefixLabel("影响多功能图");

                    if (material.GetFloat("_CAddTexUV") == 0)
                    {
                        if (GUILayout.Button("已关闭", shortButtonStyle))
                        {
                            material.SetFloat("_CAddTexUV", 1);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("已开启", shortButtonStyle))
                        {
                            material.SetFloat("_CAddTexUV", 0);
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    m_MaterialEditor.ShaderProperty(CAddTexUVT, "同速偏移");

                    EditorGUILayout.EndVertical();
                }

                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label(
                        "*开启自定义数据控制贴图UVOffset，制作贴图一次性流动使用，开启自定义数据时请先custom1.xyzw,再添加custom2.xyzw，然后默认custom1.x控制U方向Offset，custom1.y控制V方向Offset，可自定义数据",
                        style);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndVertical();

                GUILayout.Space(5);

                //EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                //m_MaterialEditor.ShaderProperty(MainTexUVS, "UV选择");
                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label(
                        "*包含normal默认UV，polar极坐标UV，cylinder圆筒UV三种模式，默认UV即模型自带UV，极坐标即中心向四周扩散UV，圆筒UV即世界坐标下一个圆筒包裹的UV，主帖图设置圆筒UV可以制作角色身上能量流动，或者渐变效果",
                        style);
                    EditorGUILayout.EndVertical();
                }

                // if (material.GetFloat("_MainTexUVS") == 1)
                // {
                //     EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                //     m_MaterialEditor.ShaderProperty(CenterU, "中心U");
                //     m_MaterialEditor.ShaderProperty(CenterV, "中心V");
                //     EditorGUILayout.EndVertical();
                //
                //     if (_tips == true)
                //     {
                //         EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                //         style.fontSize = 10;
                //         style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                //         GUILayout.Label("*极坐标UV中心点位置", style);
                //         EditorGUILayout.EndVertical();
                //     }
                // }
                //
                // if (material.GetFloat("_MainTexUVS") == 2)
                // {
                //     m_MaterialEditor.ShaderProperty(Face, "圆柱朝向");
                //
                //     if (_tips == true)
                //     {
                //         EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                //         style.fontSize = 10;
                //         style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                //         GUILayout.Label("*世界坐标系下圆柱UV的朝向", style);
                //         EditorGUILayout.EndVertical();
                //     }
                //
                //     m_MaterialEditor.ShaderProperty(TexCenter, "圆柱中心");
                //
                //     if (_tips == true)
                //     {
                //         EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                //         style.fontSize = 10;
                //         style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                //         GUILayout.Label("*世界坐标系下圆柱UV的位置", style);
                //         EditorGUILayout.EndVertical();
                //     }
                // }

                //EditorGUILayout.EndVertical();

                //GUILayout.Space(5);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                m_MaterialEditor.ShaderProperty(MainUspeed, "U流动");
                m_MaterialEditor.ShaderProperty(MainVspeed, "V流动");
                EditorGUILayout.EndVertical();

                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label("*主帖图的UV流动速度值", style);
                    EditorGUILayout.EndVertical();
                }

                GUILayout.Space(5);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("多功能图");
                if (material.GetFloat("_IfAddTex") == 0)
                {
                    if (GUILayout.Button("已关闭", shortButtonStyle))
                    {
                        material.SetFloat("_IfAddTex", 1);
                    }
                }
                else
                {
                    if (GUILayout.Button("已开启", shortButtonStyle))
                    {
                        material.SetFloat("_IfAddTex", 0);
                    }
                }

                EditorGUILayout.EndHorizontal();
                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label("*多功能图是在主贴图的基础上增加纹理细节，透明通道细节的额外贴图，可以叠加颜色，也可以充当遮罩等等", style);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndVertical();
            }

            if (mainTex_Sampler.textureValue == null)
            {
                material.SetFloat("_IfAddTex", 0);
            }

            if (material.GetFloat("_IfAddTex") == 0)
            {
                material.SetFloat("_AddTexBlend", 0);
                material.SetFloat("_AddTexBlendMode", 0);
            }
        }

        void FindPropertiesMainTexture(MaterialProperty[] props)
        {
            //主贴图属性指向
            mainTex_Sampler = FindProperty("_MainTex", props);
            TinColor = FindProperty("_MainColor", props);
            MainUspeed = FindProperty("_MainTex_Uspeed", props);
            MainVspeed = FindProperty("_MainTex_Vspeed", props);
            MainTexC = FindProperty("_MaintexC", props);
            MainTexCV = FindProperty("_MaintexCV", props);
            MainTexAR = FindProperty("_MainTex_ar", props);
            BackFaceColor = FindProperty("_BackFaceColor", props);
            Face = FindProperty("_Face", props);
            //MainTexUVS = FindProperty("_MainTexUVS", props);
            //TexCenter = FindProperty("_TexCenter", props);
            //CenterU = FindProperty("_CenterU", props);
            //CenterV = FindProperty("_CenterV", props);
            MainOffsetUC1 = FindProperty("_MainOffsetUC1", props);
            MainOffsetUC2Vec4 = FindProperty("_MainOffsetUC2Vec4", props);
            MainOffsetVC1 = FindProperty("_MainOffsetVC1", props);
            MainOffsetVC2Vec4 = FindProperty("_MainOffsetVC2Vec4", props);

            MainOffsetUC2Enum = ConvertCustomDataEnum(MainOffsetUC2Vec4);
            MainOffsetVC2Enum = ConvertCustomDataEnum(MainOffsetVC2Vec4);

            MainRotate = FindProperty("_MainTex_rotat", props);
            MainTexRefine = FindProperty("_MainTexRefine", props);
        }
    }
}