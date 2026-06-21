// -----------------------------------------------------------------------
// This file is part of  PandaShader VFXGUI.
//
// (c) sunhainan   (2022/11/28 14:46:19)
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
        //VTO
        private MaterialProperty Vto_Scale = null;
        private MaterialProperty Vto_Tex = null;
        
        private MaterialProperty Vto_Uspeed = null;
        private MaterialProperty Vto_Vspeed = null;
        private MaterialProperty VTOAR = null;
        private MaterialProperty VTOC = null;
        private MaterialProperty VTOCV = null;


        //private MaterialProperty VTOTexExp = null;
        private MaterialProperty VTORemap = null;
        private MaterialProperty VTOFactorC1 = null;

        private MaterialProperty VTOFactorC2Vec4 = null;
        private CUSTOMDATA VTOFactorC2Enum = CUSTOMDATA.NONE;

        MaterialProperty VTORotate = null;

        void OnGUIVertexOffset(Material material)
        {
            //VTO下拉菜单
            if (Vto_Tex.textureValue != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _VTO_Foldout = Foldout(_VTO_Foldout, "顶点动画(VertexOffset)");

                if (_VTO_Foldout)
                {
                    EditorGUI.indentLevel++;
                    GUIVertexOffset(material);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _VTO_Foldout = Foldout2(_VTO_Foldout, "顶点动画(VertexOffset)");

                if (_VTO_Foldout)
                {
                    EditorGUI.indentLevel++;
                    GUIVertexOffset(material);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
        }

        //VTO内容
        void GUIVertexOffset(Material material)
        {
            m_MaterialEditor.TexturePropertySingleLine(new GUIContent("顶点偏移贴图"), Vto_Tex);

            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*用来制作顶点动画，这里我们这里只能根据法线方向进行顶点移动，如果需要改变移动方向，可以去修改模型的法线朝向", style);
                EditorGUILayout.EndVertical();
            }

            if (Vto_Tex.textureValue != null)
            {
                material.EnableKeyword("_VTO_TEX");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _VTOxx = Foldouts(_VTOxx, "贴图设置(点击打开)");

                if (_VTOxx)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal();
                    m_MaterialEditor.ShaderProperty(VTOC, "UVClamp");

                    m_MaterialEditor.ShaderProperty(VTOCV, "");

                    EditorGUILayout.EndHorizontal();

                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*两个复选框分别表示贴图U方向Clamp和V方向Clamp，可以勾选其中一个，也可以两个方向都勾选", style);
                        EditorGUILayout.EndVertical();
                    }

                    m_MaterialEditor.ShaderProperty(VTOAR, "使用R通道");

                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*勾选后Red通道成为偏移通道，不勾选，Alpha通道为偏移通道，没有透明通道的黑白图可以勾选此选项，有透明通道不要勾选", style);
                        EditorGUILayout.EndVertical();
                    }

                    m_MaterialEditor.ShaderProperty(VTORotate, "贴图旋转");
                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*贴图旋转，处理贴图朝向，省去复制贴图改变朝向", style);
                        EditorGUILayout.EndVertical();
                    }

                    m_MaterialEditor.ShaderProperty(VTORemap, "Remap");

                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*把贴图数据从0to1转换到0.5to-0.5", style);
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
                    GUILayout.Label("*展开后可以对贴图属性做一些设置，包含uvclamp，通道选择，旋转，贴图数据power处理", style);
                    EditorGUILayout.EndVertical();
                }

                m_MaterialEditor.TextureScaleOffsetProperty(Vto_Tex);
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
            }
            else
            {
                material.DisableKeyword("_VTO_TEX");
            }

          

            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label(
                    "*用来制作顶点动画的遮罩，防止破面，比如球形的上下两个位置非常容易破面，这里我们只需要添加一个上下有一个渐变小黑条的遮罩图，就能让圆球上下两个位置不进行顶点偏移，从而防止破面", style);
                EditorGUILayout.EndVertical();
            }

            if (Vto_Tex.textureValue != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel("自定义强度");

                if (material.GetFloat("_VTOFactorCustom") == 0)
                {
                    if (GUILayout.Button("已关闭", shortButtonStyle))
                    {
                        material.SetFloat("_VTOFactorCustom", 1);
                    }
                }
                else
                {
                    if (GUILayout.Button("已开启", shortButtonStyle))
                    {
                        material.SetFloat("_VTOFactorCustom", 0);
                    }
                }

                EditorGUILayout.EndHorizontal();

                if (material.GetFloat("_VTOFactorCustom") == 1)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    m_MaterialEditor.ShaderProperty(VTOFactorC1, "数据");

                    DrawCustomDataEnum(ref VTOFactorC2Vec4, ref VTOFactorC2Enum, "通道");
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndVertical();

                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label(
                        "*开启自定义数据控制模型膨胀程度，可以制作爆炸之类的效果，开启自定义数据时请先添加custom1.xyzw,再添加custom2.xyzw，默认custom2.w控制膨胀程度，开启自定义数据后，膨胀程度将会失效，并隐藏",
                        style);
                    EditorGUILayout.EndVertical();
                }

                GUILayout.Space(5);

                if (material.GetFloat("_VTOFactorCustom") == 0)
                {
                    m_MaterialEditor.ShaderProperty(Vto_Scale, "顶点偏移程度");
                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*膨胀程度，开启自定义数据后，膨胀程度将会失效，并隐藏", style);
                        EditorGUILayout.EndVertical();
                    }

                    GUILayout.Space(5);
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("    UV与主帖图一致");
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                m_MaterialEditor.ShaderProperty(Vto_Uspeed, "U流动");

                m_MaterialEditor.ShaderProperty(Vto_Vspeed, "V流动");

                EditorGUILayout.EndVertical();

                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label("*顶点偏移图的UV流动速度", style);
                    EditorGUILayout.EndVertical();
                }
            }

            if (Vto_Tex.textureValue == null)
            {
                material.SetFloat("_VTOFactor", 0);
            }

            GUILayout.Space(5);
        }

        void FindPropertiesVertexOffset(MaterialProperty[] props)
        {
            //VTO
            Vto_Tex = FindProperty("_VTOTex", props);

            Vto_Scale = FindProperty("_VTOFactor", props);
            Vto_Uspeed = FindProperty("_VTOTex_Uspeed", props);
            Vto_Vspeed = FindProperty("_VTOTex_Vspeed", props);
            VTOAR = FindProperty("_VTOAR", props);
            VTOC = FindProperty("_VTOC", props);
            VTOCV = FindProperty("_VTOCV", props);

            VTORemap = FindProperty("_VTORemap", props);
            //VTOTexExp = ShaderGUI.FindProperty("_VTOTexExp", props);
            VTOFactorC1 = FindProperty("_VTOFactorC1", props);
            VTOFactorC2Vec4 = FindProperty("_VTOFactorC2Vec4", props);

            VTOFactorC2Enum = ConvertCustomDataEnum(VTOFactorC2Vec4);
            VTORotate = FindProperty("_VTOR", props);
        }
    }
}