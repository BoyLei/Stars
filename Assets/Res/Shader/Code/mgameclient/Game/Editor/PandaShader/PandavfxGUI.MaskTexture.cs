// -----------------------------------------------------------------------
// This file is part of  PandaShader VFXGUI.
//
// (c) sunhainan   (2022/11/25 16:23:37)
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
    public partial class PandavfxGUI
    {
        //Mask属性
        private MaterialProperty Mask_Sampler = null;
        private MaterialProperty MaskScale = null;
        //private MaterialProperty Maskrotat = null;
        private MaterialProperty MaskUspeed = null;
        private MaterialProperty MaskVspeed = null;
        private MaterialProperty MaskAR = null;
        private MaterialProperty MaskC = null;
        private MaterialProperty MaskCV = null;
        private MaterialProperty IfMaskColor = null;
        private MaterialProperty MaskTexUVS = null;
        private MaterialProperty IfMaskPlusTex = null;
        private MaterialProperty MaskPlusTex = null;
        private MaterialProperty MaskPlusAR = null;
        //private MaterialProperty MaskPlusR = null;
        private MaterialProperty MaskPlusC = null;
        private MaterialProperty MaskPlusCV = null;
        private MaterialProperty MaskPlusUspeed = null;
        private MaterialProperty MaskPlusVspeed = null;
        private MaterialProperty MaskOffsetUC1 = null;
        //private MaterialProperty MaskOffsetUC2 = null;
        private MaterialProperty MaskOffsetVC1 = null;
        //private MaterialProperty MaskOffsetVC2 = null;

        private MaterialProperty MaskOffsetUC2Vec4 = null;
        private CUSTOMDATA MaskOffsetUC2Enum = CUSTOMDATA.Z;

        private MaterialProperty MaskOffsetVC2Vec4 = null;
        private CUSTOMDATA MaskOffsetVC2Enum = CUSTOMDATA.W;

        MaterialProperty MaskRotate = null;
        MaterialProperty MaskPlusRotate = null;

        void OnGUIMaskTexture(Material material)
        {
            //mask下拉菜单
            if (Mask_Sampler.textureValue != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _Mask_Foldout = Foldout(_Mask_Foldout, "遮罩贴图(MaskTexture)");

                if (_Mask_Foldout)
                {
                    EditorGUI.indentLevel++;
                    GUIMaskTexture(material);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _Mask_Foldout = Foldout2(_Mask_Foldout, "遮罩贴图(MaskTexture)");

                if (_Mask_Foldout)
                {
                    EditorGUI.indentLevel++;
                    GUIMaskTexture(material);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
        }

        //Mask贴图内容
        void GUIMaskTexture(Material material)
        {
            m_MaterialEditor.TexturePropertySingleLine(new GUIContent("遮罩贴图"), Mask_Sampler);
            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*遮罩贴图主要是针对透明通道做处理，剔除不想要的部分", style);
                EditorGUILayout.EndVertical();
            }

            if (Mask_Sampler.textureValue != null)
            {
                material.EnableKeyword("_MASK_TEX");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _Maskxx = Foldouts(_Maskxx, "贴图设置(点击打开)");

                if (_Maskxx)
                {
                    EditorGUI.indentLevel++;

                    //   EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    m_MaterialEditor.ShaderProperty(MaskC, "UVClamp");
                    m_MaterialEditor.ShaderProperty(MaskCV, "");
                    EditorGUILayout.EndHorizontal();
                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*两个复选框分别表示贴图U方向Clamp和V方向Clamp，可以勾选其中一个，也可以两个方向都勾选", style);
                        EditorGUILayout.EndVertical();
                    }

                    m_MaterialEditor.ShaderProperty(MaskAR, "使用R通道");

                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*勾选后Red通道成为透明通道，不勾选，Alpha通道为透明通道，没有透明通道的黑白图可以勾选此选项，有透明通道不要勾选", style);
                        EditorGUILayout.EndVertical();
                    }

                    m_MaterialEditor.ShaderProperty(MaskRotate, "贴图旋转");
                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*贴图旋转，处理贴图朝向，省去复制贴图改变朝向", style);
                        EditorGUILayout.EndVertical();
                    }

                    m_MaterialEditor.ShaderProperty(IfMaskColor, "贴图颜色");
                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*开启后将Mask的RGB值也乘到主题图上，可以用来做一些颜色叠加", style);
                        EditorGUILayout.EndVertical();
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label("*展开后可以对贴图属性做一些设置，包含uvclamp，透明通道，旋转，启用遮罩颜色", style);
                    EditorGUILayout.EndVertical();
                }

                m_MaterialEditor.TextureScaleOffsetProperty(Mask_Sampler);
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel("自定义偏移");

                if (material.GetFloat("_CustomdataMaskUV") == 0)
                {
                    if (GUILayout.Button("已关闭", shortButtonStyle))
                    {
                        material.SetFloat("_CustomdataMaskUV", 1);
                    }
                }
                else
                {
                    if (GUILayout.Button("已开启", shortButtonStyle))
                    {
                        material.SetFloat("_CustomdataMaskUV", 0);
                    }
                }

                EditorGUILayout.EndHorizontal();

                if (material.GetFloat("_CustomdataMaskUV") == 1)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    m_MaterialEditor.ShaderProperty(MaskOffsetUC1, "U数据");
                    //m_MaterialEditor.ShaderProperty(MaskOffsetUC2, "U通道");
                    DrawCustomDataEnum(ref MaskOffsetUC2Vec4, ref MaskOffsetUC2Enum, "U通道");

                    GUILayout.Space(5);

                    m_MaterialEditor.ShaderProperty(MaskOffsetVC1, "V数据");
                    //m_MaterialEditor.ShaderProperty(MaskOffsetVC2, "V通道");
                    DrawCustomDataEnum(ref MaskOffsetVC2Vec4, ref MaskOffsetVC2Enum, "V通道");

                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndVertical();

                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label(
                        "*开启自定义数据控制贴图UVOffset，制作贴图一次性流动使用，开启自定义数据时请先添加custom1.xyzw,再添加custom2.xyzw，默认custom1.z控制U方向Offset，custom1.w控制V方向Offset",
                        style);
                    EditorGUILayout.EndVertical();
                }

                GUILayout.Space(5);

                m_MaterialEditor.FloatProperty(MaskScale, "遮罩强度");

                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label("*遮罩在透明通道上相乘的倍数", style);
                    EditorGUILayout.EndVertical();
                }

                GUILayout.Space(5);

                //EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                //m_MaterialEditor.ShaderProperty(MaskTexUVS, "UV选择");
                // if (_tips == true)
                // {
                //     EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                //     style.fontSize = 10;
                //     style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                //     GUILayout.Label(
                //         "*包含normal默认UV，polar极坐标UV，cylinder圆筒UV三种模式，默认UV即模型自带UV，极坐标即中心向四周扩散UV，圆筒UV即世界坐标下一个圆筒包裹的UV",
                //         style);
                //     EditorGUILayout.EndVertical();
                // }

                // if (material.GetFloat("_MaskTexUVS") == 1)
                // {
                //     m_MaterialEditor.ShaderProperty(CenterU, "中心U");
                //     m_MaterialEditor.ShaderProperty(CenterV, "中心V");
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
                // if (material.GetFloat("_MaskTexUVS") == 2)
                // {
                //     m_MaterialEditor.ShaderProperty(Face, "圆柱朝向");
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
                m_MaterialEditor.ShaderProperty(MaskUspeed, "U流动");

                m_MaterialEditor.ShaderProperty(MaskVspeed, "V流动");

                EditorGUILayout.EndVertical();

                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label("*遮罩图的UV流动速度值", style);
                    EditorGUILayout.EndVertical();
                }

                GUILayout.Space(5);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                m_MaterialEditor.ShaderProperty(IfMaskPlusTex, "开启额外遮罩图");

                if (material.GetFloat("_IfMaskPlusTex") == 1)
                {
                    material.EnableKeyword("_MASK_PLUS_TEX");
                    GUILayout.Space(5);

                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("额外遮罩图"), MaskPlusTex);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    _MaskPlusxx = Foldouts(_MaskPlusxx, "贴图设置(点击打开)");

                    if (_MaskPlusxx)
                    {
                        EditorGUI.indentLevel++;

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.BeginHorizontal();
                        m_MaterialEditor.ShaderProperty(MaskPlusC, "UVClamp");
                        m_MaterialEditor.ShaderProperty(MaskPlusCV, "");
                        EditorGUILayout.EndHorizontal();

                        m_MaterialEditor.ShaderProperty(MaskPlusAR, "使用R通道");

                        m_MaterialEditor.ShaderProperty(MaskPlusRotate, "贴图旋转");
                        if (_tips == true)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            style.fontSize = 10;
                            style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                            GUILayout.Label("*贴图旋转，处理贴图朝向，省去复制贴图改变朝向", style);
                            EditorGUILayout.EndVertical();
                        }

                        EditorGUILayout.EndVertical();
                        EditorGUI.indentLevel--;
                    }

                    m_MaterialEditor.TextureScaleOffsetProperty(MaskPlusTex);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    m_MaterialEditor.ShaderProperty(MaskPlusUspeed, "U流动");
                    m_MaterialEditor.ShaderProperty(MaskPlusVspeed, "V流动");
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);
                }
                else
                {
                    material.DisableKeyword("_MASK_PLUS_TEX");
                }

                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(5);

            if (Mask_Sampler.textureValue == null)
            {
                material.DisableKeyword("_MASK_TEX");
                material.SetFloat("_IfMaskPlusTex", 0);
                material.DisableKeyword("_MASK_PLUS_TEX");
            }
        }

        void FindPropertiesMaskTexture(MaterialProperty[] props)
        {
            //Mask属性指向
            Mask_Sampler = FindProperty("_MaskTex", props);
            MaskScale = FindProperty("_Mask_scale", props);
            //Maskrotat = FindProperty("_Mask_rotat", props);
            MaskUspeed = FindProperty("_Mask_Uspeed", props);
            MaskVspeed = FindProperty("_Mask_Vspeed", props);
            MaskAR = FindProperty("_MaskAlphaRA", props);
            MaskC = FindProperty("_MaskC", props);
            MaskCV = FindProperty("_MaskCV", props);
            IfMaskColor = FindProperty("_IfMaskColor", props);
            //MaskTexUVS = ShaderGUI.FindProperty("_MaskTexUVS", props);
            IfMaskPlusTex = FindProperty("_IfMaskPlusTex", props);
            MaskPlusTex = FindProperty("_MaskPlusTex", props);
            MaskPlusAR = FindProperty("_MaskPlusAR", props);
            //MaskPlusR = FindProperty("_MaskPlusR", props);
            MaskPlusC = FindProperty("_MaskPlusC", props);
            MaskPlusCV = FindProperty("_MaskPlusCV", props);
            MaskPlusUspeed = FindProperty("_MaskPlusUspeed", props);
            MaskPlusVspeed = FindProperty("_MaskPlusVspeed", props);
            MaskOffsetUC1 = ShaderGUI.FindProperty("_MaskOffsetUC1", props);
            //MaskOffsetUC2 = ShaderGUI.FindProperty("_MaskOffsetUC2", props);
            MaskOffsetVC1 = ShaderGUI.FindProperty("_MaskOffsetVC1", props);
            //MaskOffsetVC2 = ShaderGUI.FindProperty("_MaskOffsetVC2", props);
            MaskOffsetUC2Vec4 = ShaderGUI.FindProperty("_MaskOffsetUC2Vec4", props);
            MaskOffsetVC2Vec4 = ShaderGUI.FindProperty("_MaskOffsetVC2Vec4", props);

            MaskOffsetUC2Enum = ConvertCustomDataEnum(MaskOffsetUC2Vec4);
            MaskOffsetVC2Enum = ConvertCustomDataEnum(MaskOffsetVC2Vec4);

            MaskRotate = FindProperty("_Mask_rotat", props);
            MaskPlusRotate = FindProperty("_MaskPlusR", props);
        }
    }
}