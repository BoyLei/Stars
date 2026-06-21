// -----------------------------------------------------------------------
// This file is part of  PandaShader VFXGUI.
//
// (c) sunhainan   (2022/11/28 14:38:29)
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
        //溶解属性
        private MaterialProperty Dissolve_Tex = null;
        private MaterialProperty Dissolve_Color = null;
        private MaterialProperty Dissolve_Factor = null;
        private MaterialProperty Dissolve_Soft = null;
        private MaterialProperty Dissolve_Wide = null;
        private MaterialProperty Dissolve_Uspeed = null;
        private MaterialProperty Dissolve_Vspeed = null;
        //private MaterialProperty Dissolve_Rotation = null;
        private MaterialProperty DissolveAR = null;
        private MaterialProperty DissolveC = null;
        private MaterialProperty DissolveCV = null;
        private MaterialProperty DisslovePlusTex = null;
        private MaterialProperty DissolvePlusAR = null;
        private MaterialProperty DissolvePlusC = null;
        private MaterialProperty DissolvePlusCV = null;
        private MaterialProperty IfDissolvePlus = null;
        private MaterialProperty DissolveTexDivide = null;
        private MaterialProperty DissolveTexExp = null;
        private MaterialProperty IfDissolveColor = null;
        private MaterialProperty DissolveFactorC1 = null;
        private MaterialProperty DissolveOffsetUC1 = null;
 
        private MaterialProperty DissolveOffsetVC1 = null;
        private MaterialProperty IfDissolveOffsetC = null;

        private MaterialProperty DissolveFactorC2Vec4 = null; 
        private CUSTOMDATA DissolveFactorC2Enum = CUSTOMDATA.Z;

        private MaterialProperty DissolveOffsetUC2Vec4 = null;
        private CUSTOMDATA DissolveOffsetUC2Enum = CUSTOMDATA.X;

        private MaterialProperty DissolveOffsetVC2Vec4 = null;
        private CUSTOMDATA DissolveOffsetVC2Enum = CUSTOMDATA.Y;

        MaterialProperty DissolveRotate = null;
        MaterialProperty DissolvePlusRotate = null;


        void OnGUIDissolve(Material material)
        {
            //溶解下拉菜单
            if (Dissolve_Tex.textureValue != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _Dissolve_Foldout = Foldout(_Dissolve_Foldout, "溶解(Dissolve)");

                if (_Dissolve_Foldout)
                {
                    EditorGUI.indentLevel++;
                    GUIDissolve(material);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _Dissolve_Foldout = Foldout2(_Dissolve_Foldout, "溶解(Dissolve)");

                if (_Dissolve_Foldout)
                {
                    EditorGUI.indentLevel++;
                    GUIDissolve(material);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
        }

        //溶解内容
        void GUIDissolve(Material material)
        {
            m_MaterialEditor.TexturePropertySingleLine(new GUIContent("溶解贴图"), Dissolve_Tex, Dissolve_Color);

            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label(
                    "*制作溶解镂空效果，溶解是非常棒得像素动画模式，无限中间帧，是丝滑特效的最大秘密，后面的颜色变量为溶解边缘颜色，值得注意的是将这个颜色透明度调为0后，溶解边缘颜色即为贴图固有色，做烟雾溶解时非常好用。",
                    style);
                EditorGUILayout.EndVertical();
            }

            if (Dissolve_Tex.textureValue != null)
            {
                material.EnableKeyword("_DISSOLVE_TEX");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                _Dissolvexx = Foldouts(_Dissolvexx, "贴图设置(点击打开)");

                if (_Dissolvexx)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal();
                    m_MaterialEditor.ShaderProperty(DissolveC, "UVClamp");
                    m_MaterialEditor.ShaderProperty(DissolveCV, "");
                    EditorGUILayout.EndHorizontal();
                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*两个复选框分别表示贴图U方向Clamp和V方向Clamp，可以勾选其中一个，也可以两个方向都勾选", style);
                        EditorGUILayout.EndVertical();
                    }

                    m_MaterialEditor.ShaderProperty(DissolveAR, "使用R通道");

                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*勾选后Red通道成为溶解通道，不勾选，Alpha通道为溶解通道，没有透明通道的黑白图可以勾选此选项，有透明通道不要勾选", style);
                        EditorGUILayout.EndVertical();
                    }

                    m_MaterialEditor.ShaderProperty(DissolveRotate, "贴图旋转");
                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*贴图旋转，处理贴图朝向，省去复制贴图改变朝向", style);
                        EditorGUILayout.EndVertical();
                    }

                    m_MaterialEditor.ShaderProperty(DissolveTexExp, "PowerExp");
                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*贴图数据做power运算，使其棱角更加分明，溶解时边缘会更加锋利，power在0-1时相反，可以让溶解更加柔和", style);
                        EditorGUILayout.EndVertical();
                    }

                    m_MaterialEditor.ShaderProperty(IfDissolveColor, "溶解颜色受顶点色影响");

                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*边缘颜色是否受粒子系统的颜色影响", style);
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

                m_MaterialEditor.TextureScaleOffsetProperty(Dissolve_Tex);
                EditorGUILayout.EndVertical();

                GUILayout.Space(5);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel("附加溶解图");

                if (material.GetFloat("_IfDissolvePlus") == 0)
                {
                    if (GUILayout.Button("已关闭", shortButtonStyle))
                    {
                        material.SetFloat("_IfDissolvePlus", 1);
                    }
                }
                else
                {
                    if (GUILayout.Button("已开启", shortButtonStyle))
                    {
                        material.SetFloat("_IfDissolvePlus", 0);
                    }
                }

                EditorGUILayout.EndHorizontal();
                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label("*开始附加溶解图后，可以制作方向溶解了。比如从左到右，从上到下，从中心到四周等等效果", style);
                    EditorGUILayout.EndVertical();
                }

                if (material.GetFloat("_IfDissolvePlus") == 1)
                {
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("附加溶解贴图"), DisslovePlusTex);
                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label(
                            "*附加溶解图可以使用一张渐变图，比如从左到右的黑白渐变，这样溶解时就会有从左到右的溶解趋势，同理还可以使用其他方向或者一个扩散状的黑白渐变来增加溶解方向趋势感", style);
                        EditorGUILayout.EndVertical();
                    }

                    if (DisslovePlusTex.textureValue != null)
                    {
                        material.EnableKeyword("_DISSOLVE_PLUS_TEX");
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        _Dissolveplusxx = Foldouts(_Dissolveplusxx, "贴图设置(点击打开)");

                        if (_Dissolveplusxx)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.BeginHorizontal();
                            m_MaterialEditor.ShaderProperty(DissolvePlusC, "UVClamp");
                            m_MaterialEditor.ShaderProperty(DissolvePlusCV, "");
                            EditorGUILayout.EndHorizontal();
                            if (_tips == true)
                            {
                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                style.fontSize = 10;
                                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                                GUILayout.Label("*两个复选框分别表示贴图U方向Clamp和V方向Clamp，可以勾选其中一个，也可以两个方向都勾选", style);
                                EditorGUILayout.EndVertical();
                            }

                            m_MaterialEditor.ShaderProperty(DissolvePlusAR, "使用R通道");
                            if (_tips == true)
                            {
                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                style.fontSize = 10;
                                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                                GUILayout.Label("*勾选后Red通道成为溶解通道，不勾选，Alpha通道为溶解通道，没有透明通道的黑白图可以勾选此选项，有透明通道不要勾选", style);
                                EditorGUILayout.EndVertical();
                            }

                            m_MaterialEditor.ShaderProperty(DissolvePlusRotate, "贴图旋转");
                            if (_tips == true)
                            {
                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                style.fontSize = 10;
                                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                                GUILayout.Label("*贴图旋转，处理贴图朝向，省去复制贴图改变朝向", style);
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
                            GUILayout.Label("*展开后可以对贴图属性做一些设置，包含uvclamp，通道选择，旋转", style);
                            EditorGUILayout.EndVertical();
                        }

                        m_MaterialEditor.TextureScaleOffsetProperty(DisslovePlusTex);

                        EditorGUILayout.EndVertical();
                    }
                    else
                    {
                        material.DisableKeyword("_DISSOLVE_PLUS_TEX");
                    }

                    m_MaterialEditor.ShaderProperty(DissolveTexDivide, "溶解主贴图衰减");

                    GUILayout.Space(5);
                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*溶解图衰减后，附加溶解图的溶解趋势会变强，溶解的方向感会更强", style);
                        EditorGUILayout.EndVertical();
                    }
                }
                else
                {
                    material.DisableKeyword("_DISSOLVE_PLUS_TEX");
                }

                EditorGUILayout.EndVertical();

                GUILayout.Space(5);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("自定义偏移");

                if (material.GetFloat("_IfDissolveOffsetC") == 0)
                {
                    if (GUILayout.Button("已关闭", shortButtonStyle))
                    {
                        material.SetFloat("_IfDissolveOffsetC", 1);
                        material.DisableKeyword("_DISSOLVE_TEX_CUSTOMDATA_OFFSET");
                    }
                }
                else
                {
                    if (GUILayout.Button("已开启", shortButtonStyle))
                    {
                        material.SetFloat("_IfDissolveOffsetC", 0);
                        material.EnableKeyword("_DISSOLVE_TEX_CUSTOMDATA_OFFSET");
                    }
                }

                EditorGUILayout.EndHorizontal();

                if (material.GetFloat("_IfDissolveOffsetC") == 1)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    m_MaterialEditor.ShaderProperty(DissolveOffsetUC1, "U数据");
                    DrawCustomDataEnum(ref DissolveOffsetUC2Vec4, ref DissolveOffsetUC2Enum, "U通道");

                    GUILayout.Space(5);

                    m_MaterialEditor.ShaderProperty(DissolveOffsetVC1, "V数据");
                    DrawCustomDataEnum(ref DissolveOffsetVC2Vec4, ref DissolveOffsetVC2Enum, "V通道");
                    

                    EditorGUILayout.EndVertical();
                }

                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label(
                        "*开启自定义数据控制溶解偏移，开启自定义数据时请先添加custom1.xyzw,再添加custom2.xyzw，默认custom2.x和custom2.y控制溶解偏移程度", style);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndVertical();
                GUILayout.Space(5);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel("自定义溶解系数");

                if (material.GetFloat("_CustomdataDis") == 0)
                {
                    if (GUILayout.Button("已关闭", shortButtonStyle))
                    {
                        material.SetFloat("_CustomdataDis", 1);
                    }
                }
                else
                {
                    if (GUILayout.Button("已开启", shortButtonStyle))
                    {
                        material.SetFloat("_CustomdataDis", 0);
                    }
                }

                EditorGUILayout.EndHorizontal();

                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label(
                        "*开启自定义数据控制溶解程度，开启自定义数据时请先添加custom1.xyzw,再添加custom2.xyzw，默认custom2.z控制溶解程度，可以通过下面的两个选项自定义通道，开启自定义数据后，溶解系数将会失效，并隐藏",
                        style);
                    EditorGUILayout.EndVertical();
                }

                if (material.GetFloat("_CustomdataDis") == 1)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    m_MaterialEditor.ShaderProperty(DissolveFactorC1, "数据");

                    //m_MaterialEditor.ShaderProperty(DissolveFactorC2, "通道");
                    DrawCustomDataEnum(ref DissolveFactorC2Vec4, ref DissolveFactorC2Enum, "通道");
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);

                    if (material.GetFloat("_CustomdataDis") == 1)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("拖尾模式");
                        if (material.GetFloat("_CustomdataDisT") == 0)
                        {
                            if (GUILayout.Button("已关闭", shortButtonStyle))
                            {
                                material.SetFloat("_CustomdataDisT", 1);
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("已开启", shortButtonStyle))
                            {
                                material.SetFloat("_CustomdataDisT", 0);
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                        if (_tips == true)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            style.fontSize = 10;
                            style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                            GUILayout.Label(
                                "*自定义数据对粒子拖尾并不生效，这里我们开启拖尾溶解模式后，将不再使用自定义数据，而是改用粒子系统的ColoroverLifetime的Alpha通道来控制溶解程度",
                                style);
                            EditorGUILayout.EndVertical();
                        }

                        GUILayout.Space(5);
                    }
                }

                EditorGUILayout.EndVertical();

                if (material.GetFloat("_CustomdataDis") == 0)
                {
                    material.SetFloat("_CustomdataDisT", 0);
                }

                GUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel("溶解模式");

                if (material.GetFloat("_sot_sting_A") == 0)
                {
                    if (GUILayout.Button("软边溶解模式", shortButtonStyle))
                    {
                        //material.SetFloat("_soft_sting", 1);

                        material.SetFloat("_sot_sting_A", 1);
                    }
                }
                else
                {
                    if (GUILayout.Button("硬边溶解模式", shortButtonStyle))
                    {
                        //material.SetFloat("_soft_sting", 0);

                        material.SetFloat("_sot_sting_A", 0);
                    }
                }

                EditorGUILayout.EndHorizontal();

                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label("*溶解模式有软边溶解和硬边溶解两种模式软边溶解时边缘有透明度渐变过渡比较柔和，硬边溶解则是cutout的样式", style);
                    EditorGUILayout.EndVertical();
                }

                GUILayout.Space(5);

                if (material.GetFloat("_CustomdataDis") == 0)
                {
                    m_MaterialEditor.ShaderProperty(Dissolve_Factor, "溶解系数");
                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*溶解程度，0为刚好不溶解，1为恰好完全溶解，溶解系数在开启自定义数据后将失效并隐藏，将由自定义数据完全控制溶解", style);
                        EditorGUILayout.EndVertical();
                    }

                    GUILayout.Space(5);
                }

                if (material.GetFloat("_sot_sting_A") == 0)
                {
                    m_MaterialEditor.ShaderProperty(Dissolve_Soft, "软硬程度");

                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*软溶解模式溶解边缘的软硬程度，数值越高溶解边缘越锐利，越接近硬溶解模式，相反则溶解的越加柔和", style);
                        EditorGUILayout.EndVertical();
                    }

                    GUILayout.Space(5);
                }

                if (material.GetFloat("_sot_sting_A") == 1)
                {
                    m_MaterialEditor.ShaderProperty(Dissolve_Wide, "溶解宽度");
                    if (_tips == true)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        style.fontSize = 10;
                        style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                        GUILayout.Label("*溶解边缘的宽度，对软硬溶解都生效", style);
                        EditorGUILayout.EndVertical();
                    }

                    GUILayout.Space(5);
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                m_MaterialEditor.ShaderProperty(Dissolve_Uspeed, "U流动");

                m_MaterialEditor.ShaderProperty(Dissolve_Vspeed, "V流动");

                EditorGUILayout.EndVertical();

                if (_tips == true)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    style.fontSize = 10;
                    style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label("*溶解图的UV滚动速度", style);
                    EditorGUILayout.EndVertical();
                }
            }

            GUILayout.Space(5);

            if (Dissolve_Tex.textureValue == null)
            {
                material.DisableKeyword("_DISSOLVE_TEX");
                material.SetFloat("_IfDissolvePlus", 0);

                material.DisableKeyword("_DISSOLVE_PLUS_TEX");
            }
        }

        void FindPropertiesDissolve(MaterialProperty[] props)
        {
            //溶解属性指向
            Dissolve_Tex = FindProperty("_DissloveTex", props);
            Dissolve_Color = FindProperty("_DIssloveColor", props);
            Dissolve_Factor = FindProperty("_DIssloveFactor", props);
            Dissolve_Soft = FindProperty("_DIssloveSoft", props);
            Dissolve_Uspeed = FindProperty("_DisTex_Uspeed", props);
            Dissolve_Vspeed = FindProperty("_DisTex_Vspeed", props);
            //Dissolve_Rotation = FindProperty("_DIssolve_rotat", props);
            Dissolve_Wide = FindProperty("_DIssloveWide", props);
            DissolveAR = FindProperty("_DissolveAR", props);
            DissolveC = FindProperty("_DissolveC", props);
            DissolveCV = FindProperty("_DissolveCV", props);

            DisslovePlusTex = FindProperty("_DisslovePlusTex", props);
            IfDissolvePlus = FindProperty("_IfDissolvePlus", props);
            DissolvePlusAR = FindProperty("_DissolvePlusAR", props);
            DissolvePlusC = FindProperty("_DissolvePlusC", props);
            DissolvePlusCV = FindProperty("_DissolvePlusCV", props);

            DissolveTexDivide = ShaderGUI.FindProperty("_DissolveTexDivide", props);
            DissolveTexExp = ShaderGUI.FindProperty("_DissolveTexExp", props);
            IfDissolveColor = ShaderGUI.FindProperty("_IfDissolveColor", props);
            DissolveFactorC1 = ShaderGUI.FindProperty("_DissolveFactorC1", props);
            DissolveOffsetUC1 = ShaderGUI.FindProperty("_DissolveOffsetUC1", props);

            DissolveOffsetVC1 = ShaderGUI.FindProperty("_DissolveOffsetVC1", props);
            
            IfDissolveOffsetC = ShaderGUI.FindProperty("_IfDissolveOffsetC", props);
            DissolveFactorC2Vec4 = ShaderGUI.FindProperty("_DissolveFactorC2Vec4", props);
            DissolveOffsetUC2Vec4 = ShaderGUI.FindProperty("_DissolveOffsetUC2Vec4", props);
            DissolveOffsetVC2Vec4 = ShaderGUI.FindProperty("_DissolveOffsetVC2Vec4", props);

            DissolveFactorC2Enum = ConvertCustomDataEnum(DissolveFactorC2Vec4);
            DissolveOffsetUC2Enum = ConvertCustomDataEnum(DissolveOffsetUC2Vec4);
            DissolveOffsetVC2Enum = ConvertCustomDataEnum(DissolveOffsetVC2Vec4);

            DissolveRotate = FindProperty("_DIssolve_rotat", props);
            DissolvePlusRotate = FindProperty("_DissolvePlusR", props);
        }
    }
}