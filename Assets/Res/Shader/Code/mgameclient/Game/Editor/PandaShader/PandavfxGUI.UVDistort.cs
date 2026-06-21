// -----------------------------------------------------------------------
// This file is part of  PandaShader VFXGUI.
//
// (c) sunhainan   (2022/11/28 14:55:16)
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
        //UV扭曲属性
        private MaterialProperty Distort_Tex = null;
        private MaterialProperty Distort_Factor = null;
        private MaterialProperty Distort_Uspeed = null;
        private MaterialProperty Distort_Vspeed = null;
        private MaterialProperty IfFlowmap = null;
        private MaterialProperty DistortTexAR = null;
 
        private MaterialProperty DistortMaskTexAR = null;
        private MaterialProperty DistortMaskTex = null;
        private MaterialProperty DistortMaskTexC = null;
        private MaterialProperty DistortMaskTexCV = null;
        //private MaterialProperty DistortMaskTexR = null;
        private MaterialProperty DistortRemap = null;
        private MaterialProperty DistortFactorC1 = null;

        private MaterialProperty DistortFactorC2Vec4 = null;
        private CUSTOMDATA DistortFactorC2Enum = CUSTOMDATA.Z;

        private MaterialProperty DistortSceenTex = null;
        private MaterialProperty DistortMask = null;

        MaterialProperty DistortMaskTexR = null;
        
        private MaterialProperty IfBeingDistorted = null;

        void OnGUIUVDistort(Material material)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            _Distort_Foldout = Foldout(_Distort_Foldout, "UV扭曲(UVDistort)");

            if (_Distort_Foldout)
            {
                EditorGUI.indentLevel++;
                GUIUVDistort(material);
                EditorGUI.indentLevel--;
            }

            CheckKeyword(material);

            EditorGUILayout.EndVertical();
        }

        //UV扭曲内容
        void GUIUVDistort(Material material)
        {
            m_MaterialEditor.TexturePropertySingleLine(new GUIContent("UV扭曲贴图"), Distort_Tex);

            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*扭曲贴图uv信息，可以根据需求选择需要扭曲的贴图，扭曲uv后贴图出现液化效果，很适合制作水波纹，写实类能量涌动效果", style);
                EditorGUILayout.EndVertical();
            }

            m_MaterialEditor.ShaderProperty(IfBeingDistorted, "是否被扭曲");
            if (Distort_Tex.textureValue != null)
            {                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _DistortTexxx = Foldouts(_DistortTexxx, "贴图设置(点击打开)");

                if (_DistortTexxx)
                {
                    EditorGUI.indentLevel++;

                    if (material.GetFloat("_IfFlowmap") == 0)
                    {
                        m_MaterialEditor.ShaderProperty(DistortTexAR, "使用R通道");
                        if (_tips == true)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            style.fontSize = 10;
                            style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                            GUILayout.Label("*开启后R通道将做为扭曲通道，不开启使用Alpha通道为扭曲通道，黑白图没有Alpha通道请勾选此选项，有Alpha通道的贴图请不要勾选",
                                style);
                            EditorGUILayout.EndVertical();
                        }
                    }

                    m_MaterialEditor.ShaderProperty(IfFlowmap, "使用FlowMap图");
                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*开启后使用FLowMap算法来做为扭曲贴图，可以用来制作烟尘消散效果", style);
                        EditorGUILayout.EndVertical();
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();

                m_MaterialEditor.TextureScaleOffsetProperty(Distort_Tex);
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);

                m_MaterialEditor.ShaderProperty(DistortSceenTex, "使用屏幕");
                


                //m_MaterialEditor.ShaderProperty(DistortMask, "UV扭曲遮罩图");
                
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.PrefixLabel("UV扭曲遮罩图");

                if (material.GetFloat("_DistortMask") == 0)
                {
                    if (GUILayout.Button("已关闭", shortButtonStyle))
                    {
                        material.SetFloat("_DistortMask", 1);
                        material.EnableKeyword("_DISORT_MASK_TEX");
                    }
                }
                else
                {
                    if (GUILayout.Button("已开启", shortButtonStyle))
                    {
                        material.SetFloat("_DistortMask", 0);
                        material.DisableKeyword("_DISORT_MASK_TEX");
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (DistortMask.floatValue > 0)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("UV扭曲遮罩贴图"), DistortMaskTex);

                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*uv扭曲遮罩图", style);
                        EditorGUILayout.EndVertical();
                    }
                    _DistortMaskTexxx = Foldouts(_DistortMaskTexxx, "贴图设置(点击打开)");

                    if (_DistortMaskTexxx)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUI.indentLevel++;

                        EditorGUILayout.BeginHorizontal();
                        m_MaterialEditor.ShaderProperty(DistortMaskTexC, "UVClamp");

                        m_MaterialEditor.ShaderProperty(DistortMaskTexCV, "");

                        EditorGUILayout.EndHorizontal();

                        if (_tips == true)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            style.fontSize = 10;
                            style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                            GUILayout.Label("*两个复选框分别表示贴图U方向Clamp和V方向Clamp，可以勾选其中一个，也可以两个方向都勾选", style);
                            EditorGUILayout.EndVertical();
                        }

                        m_MaterialEditor.ShaderProperty(DistortMaskTexAR, "使用R通道");
                        if (_tips == true)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            style.fontSize = 10;
                            style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                            GUILayout.Label("*开启后R通道将做为扭曲遮罩通道，不开启使用Alpha通道为扭曲遮罩通道，黑白图没有Alpha通道请勾选此选项，有Alpha通道的贴图请不要勾选",
                                style);
                            EditorGUILayout.EndVertical();
                        }

                        m_MaterialEditor.ShaderProperty(DistortMaskTexR, "贴图旋转");
                        if (_tips == true)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            style.fontSize = 10;
                            style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                            GUILayout.Label("*贴图旋转，处理贴图朝向，省去复制贴图改变朝向", style);
                            EditorGUILayout.EndVertical();
                        }

                        EditorGUI.indentLevel--;
                        EditorGUILayout.EndVertical();
                    }

                    m_MaterialEditor.TextureScaleOffsetProperty(DistortMaskTex);
                    EditorGUILayout.EndVertical();

                    GUILayout.Space(5);
                }
                

                GUILayout.Space(5);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel("自定义强度");

                if (material.GetFloat("_CustomDistort") == 0)
                {
                    if (GUILayout.Button("已关闭", shortButtonStyle))
                    {
                        material.SetFloat("_CustomDistort", 1);
                    }
                }
                else
                {
                    if (GUILayout.Button("已开启", shortButtonStyle))
                    {
                        material.SetFloat("_CustomDistort", 0);
                    }
                }

                EditorGUILayout.EndHorizontal();

                if (material.GetFloat("_CustomDistort") == 1)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    m_MaterialEditor.ShaderProperty(DistortFactorC1, "数据");
                    DrawCustomDataEnum(ref DistortFactorC2Vec4, ref DistortFactorC2Enum, "通道");

                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndVertical();
                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label(
                        "*开启自定义数据控制贴图扭曲程度，配合溶解可以用来制作flowmap溶解效果，开启自定义数据时请先custom1.xyzw,再添加custom2.xyzw，然后custom2.w控制扭曲程度，开启自定义数据后，扭曲程度将会失效，并隐藏",
                        style);
                    EditorGUILayout.EndVertical();
                }

                GUILayout.Space(5);

                if (material.GetFloat("_CustomDistort") == 0)
                {
                    m_MaterialEditor.ShaderProperty(Distort_Factor, "UV扭曲程度");

                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*扭曲程度，开启自定义数据后，扭曲程度将会失效，并隐藏", style);
                        EditorGUILayout.EndVertical();
                    }

                    GUILayout.Space(5);
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("    UV与主帖图一致");
                EditorGUILayout.EndVertical();

                GUILayout.Space(5);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                m_MaterialEditor.ShaderProperty(Distort_Uspeed, "U流动");

                m_MaterialEditor.ShaderProperty(Distort_Vspeed, "V流动");

                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label("*扭曲贴图的UV流动速度", style);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndVertical();
                /*
                GUILayout.Space(5);

                
                EditorGUILayout.BeginHorizontal();

                
                EditorGUILayout.PrefixLabel("扭动主帖图");

                if (material.GetFloat("_DistortMainTex") == 0)
                {
                    if (GUILayout.Button("已关闭", shortButtonStyle))
                    {
                        material.SetFloat("_DistortMainTex", 1);
                    }
                }
                else
                {
                    if (GUILayout.Button("已开启", shortButtonStyle))
                    {
                        material.SetFloat("_DistortMainTex", 0);
                    }
                }

                EditorGUILayout.EndHorizontal();
                
                if (material.GetFloat("_IfAddTex") == 1)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("扭动附加图");

                    if (material.GetFloat("_DistortAddTex") == 0)
                    {
                        if (GUILayout.Button("已关闭", shortButtonStyle))
                        {
                            material.SetFloat("_DistortAddTex", 1);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("已开启", shortButtonStyle))
                        {
                            material.SetFloat("_DistortAddTex", 0);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (Mask_Sampler.textureValue != null)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PrefixLabel("扭动遮罩图");

                    if (material.GetFloat("_DistortMask") == 0)
                    {
                        if (GUILayout.Button("已关闭", shortButtonStyle))
                        {
                            material.SetFloat("_DistortMask", 1);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("已开启", shortButtonStyle))
                        {
                            material.SetFloat("_DistortMask", 0);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (Dissolve_Tex.textureValue != null)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PrefixLabel("扭动溶解帖图");

                    if (material.GetFloat("_DistortDisTex") == 0)
                    {
                        if (GUILayout.Button("已关闭", shortButtonStyle))
                        {
                            material.SetFloat("_DistortDisTex", 1);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("已开启", shortButtonStyle))
                        {
                            material.SetFloat("_DistortDisTex", 0);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }

                
                if (NormalTex.textureValue != null && material.GetFloat("_IfCustomLight") == 1)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PrefixLabel("扭动法线图");

                    if (material.GetFloat("_DistortNormalTex") == 0)
                    {
                        if (GUILayout.Button("已关闭", shortButtonStyle))
                        {
                            material.SetFloat("_DistortNormalTex", 1);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("已开启", shortButtonStyle))
                        {
                            material.SetFloat("_DistortNormalTex", 0);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
                */
            }

            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*分别开启或关闭扭曲各个图层的UV信息", style);
                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(5);
        }

        void CheckKeyword(Material material)
        {
            if (Distort_Tex.textureValue == null)
            {
                material.DisableKeyword("_DISORT_TEX");
                material.DisableKeyword("_SCEENTEX_ON");
                material.DisableKeyword("_DISORT_MASK_TEX");
                DistortMaskTex.textureValue = null;
                material.SetShaderPassEnabled("Distortion", false);
                material.SetShaderPassEnabled("UniversalForward", true);
                material.SetShaderPassEnabled("AfterDistortion", false);
            }
            else
            {
                material.EnableKeyword("_DISORT_TEX");
                if (material.GetFloat("_SceenTex") != 0)
                {
                    material.EnableKeyword("_SCEENTEX_ON");
                    material.SetShaderPassEnabled("Distortion", true);
                    material.SetShaderPassEnabled("UniversalForward", false);
                    material.SetShaderPassEnabled("AfterDistortion", false);
                }
                else
                {
                    material.DisableKeyword("_SCEENTEX_ON");
                    material.SetShaderPassEnabled("Distortion", false);
                    material.SetShaderPassEnabled("UniversalForward", true);
                    material.SetShaderPassEnabled("AfterDistortion", false);
                }
            }
            
            
            
            if (material.GetFloat("_IfBeingDistorted") == 0)
            {
                material.SetShaderPassEnabled("AfterDistortion", true);
                material.SetShaderPassEnabled("Distortion", false);
                material.SetShaderPassEnabled("UniversalForward", false);
            }
            else if (material.GetFloat("_IfBeingDistorted") == 1)
            {
                
            }
        }

        void FindPropertyUVDistort(MaterialProperty[] props)
        {
            //UV扭曲
            Distort_Tex = FindProperty("_DistortTex", props);
            Distort_Factor = FindProperty("_DistortFactor", props);
            Distort_Uspeed = FindProperty("_DistortTex_Uspeed", props);
            Distort_Vspeed = FindProperty("_DistortTex_Vspeed", props);
            IfFlowmap = FindProperty("_IfFlowmap", props);

            DistortTexAR = FindProperty("_DistortTexAR", props);
            DistortMaskTexAR = FindProperty("_DistortMaskTexAR", props);
            DistortMaskTex = FindProperty("_DistortMaskTex", props);
            DistortMaskTexC = FindProperty("_DistortMaskTexC", props);
            DistortMaskTexCV = FindProperty("_DistortMaskTexCV", props);
            //DistortMaskTexR = FindProperty("_DistortMaskTexR", props);
            DistortRemap = FindProperty("_DistortRemap", props);
            DistortFactorC1 = FindProperty("_DistortFactorC1", props);
            DistortSceenTex = FindProperty("_SceenTex", props);
            IfBeingDistorted = FindProperty("_IfBeingDistorted", props);
            DistortMask = FindProperty("_DistortMask", props);

            DistortFactorC2Vec4 = FindProperty("_DistortFactorC2Vec4", props);
            DistortFactorC2Enum = ConvertCustomDataEnum(DistortFactorC2Vec4);

            DistortMaskTexR = FindProperty("_DistortMaskTexR", props);
        }
    }
}