// -----------------------------------------------------------------------
// This file is part of  
//
// (c) sunhainan   (2022/11/28 15:9:48)
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
        void GUISoftParticle(Material material)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel("软粒子");

            if (material.GetFloat("_Depthfadeon") == 0)
            {
                if (GUILayout.Button("已关闭", shortButtonStyle))
                {
                    material.SetFloat("_Depthfadeon", 1);
                }
            }
            else
            {
                if (GUILayout.Button("已开启", shortButtonStyle))
                {
                    material.SetFloat("_Depthfadeon", 0);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*特效与地面人物穿插时，会软化掉穿插部分，使相交处没用明显痕迹，场景必须开启深度图此选项才能生效，如果相机是正交相机，此功能将失效，因为正交相机没有相机深度！",
                    style);
                EditorGUILayout.EndVertical();
            }

            if (material.GetFloat("_Depthfadeon") == 1)
            {
                material.EnableKeyword("_SOFT_PARTICAL");

                GUILayout.Space(5);

                m_MaterialEditor.ShaderProperty(DepthFade, "软粒子羽化程度");
                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label("*羽化程度，数值越高软化范围越大，需要根据模型比例尺调整，如果开启反向软粒子后，此选项变为强化交界处的宽度值", style);
                    EditorGUILayout.EndVertical();
                }
            }
            else
            {
                material.DisableKeyword("_SOFT_PARTICAL");
            }

            EditorGUILayout.EndVertical();
        }
    }
}