// -----------------------------------------------------------------------
// This file is part of  PandaShader VFXGUI.
//
// (c) sunhainan   (2022/11/28 15:4:47)
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
        void OnGUIParallax(Material material)
        {
            /*
            if (material.GetFloat("_IfPara") == 1)
            {
                if (ParaTex.textureValue != null)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    _shichax = Foldout(_shichax, "视差映射(ParallaxMapping)");

                    if (_shichax)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.Space();
                        GUIParallax(material);
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    _shichax = Foldout2(_shichax, "视差映射(ParallaxMapping)");

                    if (_shichax)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.Space();
                        GUIParallax(material);
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.EndVertical();
                }
            }
            */
        }

        void GUIParallax(Material material)
        {
            /*
            m_MaterialEditor.TexturePropertySingleLine(new GUIContent("深度贴图"), ParaTex);

            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*灰度图即可，黑色部分会下凹，白色部分不变", style);
                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(5);

            m_MaterialEditor.ShaderProperty(Parallax, "深度值");

            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*下凹的强度值", style);
                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(5);

            if (ParaTex.textureValue == null)
            {
                material.SetFloat("_Parallax", 0);
            }
            */
        }
    }
}