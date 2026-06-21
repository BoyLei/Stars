// -----------------------------------------------------------------------
// This file is part of  PandaShader VFXGUI.
//
// (c) sunhainan   (2022/11/28 14:45:31)
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
        //菲尼尔
        private MaterialProperty Fnl_Scale = null;
        private MaterialProperty Fnl_Power = null;
        private MaterialProperty Fnl_Color = null;
        private MaterialProperty Fnl_Soft = null;
        private MaterialProperty Dir = null;
        private MaterialProperty IfFNLAlpha = null;

        void OnGUIFresnel(Material material)
        {
            //FNL下拉菜单
            if (material.GetFloat("_fnl_sacle") != 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _FNL_Foldout = Foldout(_FNL_Foldout, "菲尼尔效果(Fresnel)");

                if (_FNL_Foldout)
                {
                    EditorGUI.indentLevel++;
                    GUIFresnel(material);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
            else if (material.GetFloat("_FNLfanxiangkaiguan") != 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _FNL_Foldout = Foldout(_FNL_Foldout, "菲尼尔效果(Fresnel)");

                if (_FNL_Foldout)
                {
                    EditorGUI.indentLevel++;
                    GUIFresnel(material);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _FNL_Foldout = Foldout2(_FNL_Foldout, "菲尼尔效果(Fresnel)");

                if (_FNL_Foldout)
                {
                    EditorGUI.indentLevel++;
                    GUIFresnel(material);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
        }

        //Fresnel内容
        void GUIFresnel(Material material)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel("反向菲尼尔");

            if (material.GetFloat("_FNLfanxiangkaiguan") == 0)
            {
                if (GUILayout.Button("已关闭", shortButtonStyle))
                {
                    material.SetFloat("_FNLfanxiangkaiguan", 1);
                }
            }
            else
            {
                if (GUILayout.Button("已开启", shortButtonStyle))
                {
                    material.SetFloat("_FNLfanxiangkaiguan", 0);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*反向菲尼尔后可以软化模型边缘，减少特效的模型感", style);
                EditorGUILayout.EndVertical();
            }

            if (material.GetFloat("_FNLfanxiangkaiguan") == 1)
            {
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(Fnl_Soft, "模型软化程度");
                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label("*软化程度越高，模型边缘剔除越多，模型感越弱", style);
                    EditorGUILayout.EndVertical();
                }

                GUILayout.Space(5);

                /*
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel("软化剔除背面");

                if (material.GetFloat("_softback") == 0)
                {
                    if (GUILayout.Button("已关闭", shortButtonStyle))
                    {
                        material.SetFloat("_softback", 1);
                    }
                }
                else
                {
                    if (GUILayout.Button("已开启", shortButtonStyle))
                    {
                        material.SetFloat("_softback", 0);
                    }
                }

                EditorGUILayout.EndHorizontal();
                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label("*反向菲尼尔直接剔除掉背面，在开启菲尼尔颜色时，模型边缘容易破面，此选项可以一定程度减少破面，一般不用开启", style);
                    EditorGUILayout.EndVertical();
                }

                GUILayout.Space(5);
                */
            }

            if (material.GetFloat("_FNLfanxiangkaiguan") == 0)
            {
                material.SetFloat("_softFacotr", 0);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            m_MaterialEditor.ShaderProperty(IfFNLAlpha, "影响透明通道");
            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*开启后菲尼尔的数值会乘在透明通道上", style);
                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(5);
            m_MaterialEditor.ShaderProperty(Fnl_Color, "菲尼尔颜色");
            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*菲尼尔的叠加颜色", style);
                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(5);

            m_MaterialEditor.ShaderProperty(Fnl_Scale, "菲尼尔强度");
            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*菲尼尔的强度，颜色里也可以调强度，这里增加这个参数是为了方便k帧", style);
                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(5);

            m_MaterialEditor.ShaderProperty(Fnl_Power, "菲尼尔锐化");
            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*菲尼尔的PowerExp值，使菲尼尔效果更加锐利", style);
                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(5);

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();

            GUILayout.Space(5);
        }

        void FindPropertiesFresnel(MaterialProperty[] props)
        {
            //菲尼尔
            Fnl_Color = FindProperty("_fnl_color", props);
            Fnl_Scale = FindProperty("_fnl_sacle", props);
            Fnl_Power = FindProperty("_fnl_power", props);
            Fnl_Soft = FindProperty("_softFacotr", props);
            Dir = FindProperty("_Dir", props);
            IfFNLAlpha = FindProperty("_IfFNLAlpha", props);
        }
    }
}