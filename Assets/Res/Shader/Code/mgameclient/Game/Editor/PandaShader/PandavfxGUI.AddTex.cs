// -----------------------------------------------------------------------
// This file is part of  
//
// (c) sunhainan   (2022/11/28 14:59:59)
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
        //附加贴图
        private MaterialProperty AddTexUspeed = null;
        private MaterialProperty AddTexVspeed = null;
        //private MaterialProperty AddTexRo = null;
        private MaterialProperty AddTexC = null;
        private MaterialProperty AddTexCV = null;
        private MaterialProperty AddTexColor = null;
        private MaterialProperty AddTex_Sampler = null;
        private MaterialProperty AddTexBlend = null;
        private MaterialProperty AddTexAR = null;
        private MaterialProperty IfAddTexAlpha = null;
        private MaterialProperty IfAddTexColor = null;
        private MaterialProperty AddTexUVS = null;
        private MaterialProperty CAddTexUVT = null;
        private MaterialProperty AddTexBlendModeVec4 = null;
        private BLENDMODE AddTexBlendModeEnum = BLENDMODE.Alpha;
        void OnGUIAddTex(Material material)
        {
            //AddTex下拉菜单
            if (mainTex_Sampler.textureValue != null)
            {
                if (material.GetFloat("_IfAddTex") == 1)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    _AddTex = Foldout(_AddTex, "多功能图(AddTexture)");

                    if (_AddTex)
                    {
                        EditorGUI.indentLevel++;
                        GUIAddTex(material);
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.EndVertical();
                }
                else
                {
                    CloseAddTex(material);
                }
            }
        }

        void GUIAddTex(Material material)
        {
            m_MaterialEditor.TexturePropertySingleLine(new GUIContent("多功能图"), AddTex_Sampler, AddTexColor);

            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*附加图用来增加主帖图纹理细节", style);
                EditorGUILayout.EndVertical();
            }

            if (AddTex_Sampler.textureValue != null)
            {
                //开启材质的_Add_Tex相关的KeyWord
                material.EnableKeyword("_ADD_TEX");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _AddTexxx = Foldouts(_AddTexxx, "贴图设置(点击打开)");

                if (_AddTexxx)
                {
                    EditorGUI.indentLevel++;

                    //   EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    m_MaterialEditor.ShaderProperty(AddTexC, "UVClamp");
                    m_MaterialEditor.ShaderProperty(AddTexCV, "");
                    EditorGUILayout.EndHorizontal();

                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*两个复选框分别表示贴图U方向Clamp和V方向Clamp，可以勾选其中一个，也可以两个方向都勾选", style);
                        EditorGUILayout.EndVertical();
                    }

                    m_MaterialEditor.ShaderProperty(AddTexAR, "使用R通道");
                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*不勾选附加图的A通道为它的透明通道，勾选附加图的R通道为它的透明通道", style);
                        EditorGUILayout.EndVertical();
                    }

                    m_MaterialEditor.ShaderProperty(IfAddTexColor, "参与颜色通道运算");

                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*附加图的颜色也会影响整体的颜色", style);
                        EditorGUILayout.EndVertical();
                    }

                    m_MaterialEditor.ShaderProperty(IfAddTexAlpha, "参与透明通道运算");

                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*附加图的透明通道也会影响整体的透明度", style);
                        EditorGUILayout.EndVertical();
                    }

                    GUILayout.Space(5);

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

                m_MaterialEditor.TextureScaleOffsetProperty(AddTex_Sampler);
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);

                //m_MaterialEditor.ShaderProperty(AddTexBlendMode, "叠加模式");
                DrawBlendModeEnum(ref AddTexBlendModeVec4, ref AddTexBlendModeEnum, "叠加模式");
                
                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label("*附加图和主贴图的叠加方式，有lerp（即alpha）和add两种，根据效果需求选择叠加模式", style);
                    EditorGUILayout.EndVertical();
                }

                GUILayout.Space(5);

                if (material.GetVector("_AddTexBlendModeVec4") == new Vector4(1, 0, 0, 0))
                {
                    m_MaterialEditor.ShaderProperty(AddTexBlend, "融合系数");
                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*主贴图与附加图的融合比例，0为完全显示主贴图，1为完全显示附加图", style);
                        EditorGUILayout.EndVertical();
                    }

                    material.SetFloat("_AddTexAlpha", 0);

                    GUILayout.Space(5);
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                m_MaterialEditor.ShaderProperty(AddTexUspeed, "U流动");

                m_MaterialEditor.ShaderProperty(AddTexVspeed, "V流动");

                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label("*附加图UV流动速度", style);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndVertical();

                GUILayout.Space(5);
            }
            else
            {
                CloseAddTex(material);
            }
        }

        void CloseAddTex(Material material)
        {
            material.DisableKeyword("_ADD_TEX");
            material.SetVector("_AddTexBlendModeVec4", new Vector4(1, 0, 0, 0));
            material.SetFloat("_IfAddTexAlpha", 0);
        }

        void FindPropertiesAddTex(MaterialProperty[] props)
        {
            //附加贴图
            AddTex_Sampler = FindProperty("_AddTex", props);
            AddTexBlend = FindProperty("_AddTexBlend", props);
            AddTexC = FindProperty("_AddTexC", props);
            AddTexCV = FindProperty("_AddTexCV", props);
            AddTexColor = FindProperty("_AddTexColor", props);
            //AddTexRo = FindProperty("_AddTexRo", props);
            AddTexUspeed = FindProperty("_AddTexUspeed", props);
            AddTexVspeed = FindProperty("_AddTexVspeed", props);
            AddTexAR = ShaderGUI.FindProperty("_AddTexAR", props);
            IfAddTexAlpha = ShaderGUI.FindProperty("_IfAddTexAlpha", props);
            IfAddTexColor = ShaderGUI.FindProperty("_IfAddTexColor", props);
            //AddTexUVS = ShaderGUI.FindProperty("_AddTexUVS", props);
            CAddTexUVT = ShaderGUI.FindProperty("_CAddTexUVT", props);
            AddTexBlendModeVec4 = ShaderGUI.FindProperty("_AddTexBlendModeVec4", props);
            AddTexBlendModeEnum = ConvertBlendMode(AddTexBlendModeVec4);
        }

        BLENDMODE ConvertBlendMode(MaterialProperty blendModeVec4)
        {
            Vector4 vec4 = blendModeVec4.vectorValue;
            if (vec4 == new Vector4(1, 0, 0, 0))
            {
                return BLENDMODE.Alpha;
            }
            else if (vec4 == new Vector4(0, 1, 0, 0))
            {
                return BLENDMODE.Add;
            }
            else if (vec4 == new Vector4(0, 0, 1, 0))
            {
                return BLENDMODE.Multiply;
            }

            return BLENDMODE.Alpha;
        }
    }
}