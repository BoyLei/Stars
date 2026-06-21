// -----------------------------------------------------------------------
// This file is part of PandaShader VFXGUI.
// 
// (c) sunhainan <sunhainan.dobest.com>
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Linq.Expressions;
using System;

namespace Yoka.MGame.PandaShader
{
    public partial class PandaUXvfxGUI : ShaderGUI
    {
        //自定义一个小按钮
        private GUILayoutOption[] shortButtonStyle = new GUILayoutOption[] { GUILayout.Width(100) };

        //自定义字体
        private GUIStyle style = new GUIStyle();

        //自定义下拉菜单的形状属性
        static bool Foldout(bool display, string title)
        {
            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle(EditorStyles.boldLabel).font;
            style.border = new RectOffset(15, 7, 4, 4);
            style.fixedHeight = 22;
            style.contentOffset = new Vector2(20f, -2f);
            style.fontSize = 11;
            style.normal.textColor = new Color(0.7f, 0.8f, 0.9f);

            var rect = GUILayoutUtility.GetRect(16f, 25f, style);
            GUI.Box(rect, title, style);

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }

            return display;
        }

        //自定义下拉菜单1的形状属性
        static bool Foldouts(bool display, string title)
        {
            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle(EditorStyles.boldLabel).font;
            style.border = new RectOffset(15, 7, 4, 4);
            style.fixedHeight = 18;
            style.contentOffset = new Vector2(30f, -2f);
            style.fontSize = 10;
            style.normal.textColor = new Color(0.75f, 0.75f, 0.75f);

            var rect = GUILayoutUtility.GetRect(16f, 15f, style);
            GUI.Box(rect, title, style);

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 15f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }

            return display;
        }

        //自定义下拉菜单2的形状属性
        static bool Foldout2(bool display, string title)
        {
            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle(EditorStyles.boldLabel).font;
            style.border = new RectOffset(15, 7, 4, 4);
            style.fixedHeight = 22;
            style.contentOffset = new Vector2(20f, -2f);
            style.fontSize = 11;
            style.normal.textColor = new Color(0.65f, 0.55f, 0.55f);

            var rect = GUILayoutUtility.GetRect(16f, 25f, style);
            GUI.Box(rect, title, style);

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }

            return display;
        }

        enum CUSTOMDATA
        {
            X = 0,
            Y = 1,
            Z = 2,
            W = 3,
            NONE = 4
        }

        static Vector4 customDataEnumX = new Vector4(1, 0, 0, 0); 
        static Vector4 customDataEnumY = new Vector4(0, 1, 0, 0); 
        static Vector4 customDataEnumZ = new Vector4(0, 0, 1, 0); 
        static Vector4 customDataEnumW = new Vector4(0, 0, 0, 1); 
        static CUSTOMDATA ConvertCustomDataEnum(MaterialProperty vec4)
        {
            Vector4 vec = vec4.vectorValue;
            if (vec == customDataEnumX)
            {
                return CUSTOMDATA.X;
            }
            else if (vec == customDataEnumY)
            {
                return CUSTOMDATA.Y;
            }
            else if (vec == customDataEnumZ)
            {
                return CUSTOMDATA.Z;
            }
            else if (vec == customDataEnumW)
            {
                return CUSTOMDATA.W;
            }

            return CUSTOMDATA.NONE;
        }

        static void DrawCustomDataEnum(ref MaterialProperty vec4, ref CUSTOMDATA customdata, string title)
        {
            customdata = (CUSTOMDATA)EditorGUILayout.EnumPopup(title, customdata);

            switch(customdata)
            {
                case CUSTOMDATA.X:
                    vec4.vectorValue = new Vector4(1, 0, 0, 0); 
                    break;
                case CUSTOMDATA.Y:
                    vec4.vectorValue = new Vector4(0, 1, 0, 0);
                    break;
                case CUSTOMDATA.Z:
                    vec4.vectorValue = new Vector4(0, 0, 1, 0);
                    break;
                case CUSTOMDATA.W:
                    vec4.vectorValue = new Vector4(0, 0, 0, 1);
                    break;
                case CUSTOMDATA.NONE:
                    vec4.vectorValue = Vector4.zero;
                    break;
                default:
                    break;
            }
        }

        enum BLENDMODE
        {
            Alpha = 0,
            Add = 1,
            Multiply = 2,
        }
        static void DrawBlendModeEnum(ref MaterialProperty vec4, ref BLENDMODE blendmode, string title)
        {
            blendmode = (BLENDMODE)EditorGUILayout.EnumPopup(title, blendmode);
            switch(blendmode)
            {
                case BLENDMODE.Alpha:
                    vec4.vectorValue = new Vector4(1, 0, 0, 0);
                    break;
                case BLENDMODE.Add:
                    vec4.vectorValue = new Vector4(0, 1, 0, 0);
                    break;
                case BLENDMODE.Multiply:
                    vec4.vectorValue = new Vector4(0, 0, 1, 0);
                    break;
            }
        }

        //自定义变量
        private static bool _Base_Foldout = true;
        private static bool _Maintextures_Foldout = true;
        private static bool _Mask_Foldout = false;
        private static bool _Dissolve_Foldout = false;
        private static bool _Distort_Foldout = false;
        private static bool _FNL_Foldout = false;
        private static bool _VTO_Foldout = false;
        private static bool _Main_Foldout = true;
        private static bool _Tip_Foldout = false;
        private static bool _Mainxx = false;
        private static bool _Maskxx = false;
        private static bool _MaskPlusxx = false;
        private static bool _Dissolvexx = false;
        private static bool _Dissolveplusxx = false;
        private static bool _VTOxx = false;
        private static bool _VTOMaskxx = false;
        private static bool _AddTexxx = false;
        private static bool _AddTex = false;
        private static bool _tips = false;
        private MaterialEditor m_MaterialEditor;
        private static bool _NormalTexx = false;
        private static bool _NormalTexxx = false;
        private static bool _DistortTexxx = false;
        private static bool _DistortMaskTexxx = false;
        private static bool _shichax = false;
        private static bool _mengbanxx = false;
        private static bool _UV_Foldout = false;

        //自定义整体选项需要显示的属性
        private MaterialProperty MainAlpha = null;
        private MaterialProperty DepthFade = null;
        private MaterialProperty DepthColor = null;
        private MaterialProperty DepthF = null;
        private MaterialProperty Reference = null;
        private MaterialProperty Comparison = null;
        private MaterialProperty Pass = null;
        private MaterialProperty Fail = null;

        private MaterialProperty CenterU = null;
        private MaterialProperty CenterV = null;
        private MaterialProperty UsePolarUV = null;

        private MaterialProperty ToGray = null;

        //将自定义的需要显示的属性指向shader里的相应变量
        public void FindProperties(MaterialProperty[] props)
        {
            FindPropertiesMainTexture(props);

            FindPropertiesAddTex(props);

            FindPropertiesMaskTexture(props);

            FindPropertiesDissolve(props);

            FindPropertyUVDistort(props);

            FindPropertiesFresnel(props);

            FindPropertiesVertexOffset(props);

            //综合选项属性指向
            MainAlpha = FindProperty("_MainAlpha", props);
            DepthFade = FindProperty("_DepthfadeFactor", props);
           
            // basic setting
            Reference = FindProperty("_Stencil", props);
            Comparison = FindProperty("_StencilComp", props);
            Pass = FindProperty("_StencilOp", props);
            Fail = FindProperty("_Fail", props);

            // uv type setting
            CenterV = FindProperty("_CenterV", props);
            CenterU = FindProperty("_CenterU", props);
            UsePolarUV = FindProperty("_UVPolar", props);
            
            //ToGray
            ToGray = FindProperty("_ToGray", props);
        }

        //将上面定义的属性显示在面板上
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            FindProperties(props);

            m_MaterialEditor = materialEditor;

            Material material = materialEditor.target as Material;

            //基础设置下拉菜单
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _Base_Foldout = Foldout(_Base_Foldout, "基础设置(BasicSettings)");

            if (_Base_Foldout)
            {
                EditorGUI.indentLevel++;
                GUIBase(material);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();

            //GUIUV();

            OnGUIMainTexture(material);

            OnGUIAddTex(material);

            OnGUIMaskTexture(material);

            OnGUIDissolve(material);

            OnGUIUVDistort(material);

            OnGUIFresnel(material);

            OnGUIVertexOffset(material);

            //OnGUIParallax(material);

            //综合选项下拉菜单
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _Main_Foldout = Foldout(_Main_Foldout, "综合设置(AdditionalSettings)");
            if (_Main_Foldout)
            {
                EditorGUI.indentLevel++;
                GUIMain(material);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();

            //说明
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _Tip_Foldout = Foldout(_Tip_Foldout, "说明(Illustrate)");
            if (_Tip_Foldout)
            {
                EditorGUI.indentLevel++;
                GUITip(material);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }

        //基础设置内容显示
        void GUIBase(Material material)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel("混合模式");

            if (material.GetFloat("_Scr") == 1)
            {
                if (GUILayout.Button("Add", shortButtonStyle))
                {
                    material.SetFloat("_Scr", 5);
                    material.SetFloat("_Dst", 10);
                    material.SetFloat("_AlphaAdd", 0);
                }
            }
            else
            {
                if (GUILayout.Button("Alpha", shortButtonStyle))
                {
                    material.SetFloat("_Scr", 1);
                    material.SetFloat("_Dst", 1);
                    material.SetFloat("_AlphaAdd", 1);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                style.wordWrap = true;
                GUILayout.Label("*包含Alpha叠加和Add两种叠加模式", style);
                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel("剔除模式");

            if (material.GetFloat("_Cullmode") == 0)
            {
                if (GUILayout.Button("双面显示", shortButtonStyle))
                {
                    material.SetFloat("_Cullmode", 1);
                }
            }
            else
            {
                if (material.GetFloat("_Cullmode") == 1)
                {
                    if (GUILayout.Button("显示背面", shortButtonStyle))
                    {
                        material.SetFloat("_Cullmode", 2);
                    }
                }
                else
                {
                    if (GUILayout.Button("显示正面", shortButtonStyle))
                    {
                        material.SetFloat("_Cullmode", 0);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*包含显示正面，显示背面，双面显示三种模式", style);
                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("显示在最前层");
            if (material.GetFloat("_Ztest") == 4)
            {
                if (GUILayout.Button("否", shortButtonStyle))
                {
                    material.SetFloat("_Ztest", 8);
                }
            }
            else
            {
                if (GUILayout.Button("是", shortButtonStyle))
                {
                    material.SetFloat("_Ztest", 4);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*开启后深度测试ZTest设置为always，可以让特效显示在角色之前，避免穿模", style);
                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("写入深度");
            if (material.GetFloat("_Zwrite") == 0)
            {
                if (GUILayout.Button("否", shortButtonStyle))
                {
                    material.SetFloat("_Zwrite", 1);
                }
            }
            else
            {
                if (GUILayout.Button("是", shortButtonStyle))
                {
                    material.SetFloat("_Zwrite", 0);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*开启后深度写入ZWrite改为on，开启后可以一定程度防止模型破面,但是透明区域后的其他特效层级显示会出现问题", style);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }

        //综合设置内容显示
        void GUIMain(Material material)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            m_MaterialEditor.ShaderProperty(MainAlpha, "总透明度");
            if (_tips == true)
            {

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*整体最后结果的透明度倍增，最大值不会超过1", style);
                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(5);
            EditorGUILayout.EndVertical();

            GUISoftParticle(material);

            Gray();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel("开启初学者模式");
            if (_tips == false)
            {
                if (GUILayout.Button("已关闭", shortButtonStyle))
                {
                    _tips = true;
                }
            }
            else
            {
                if (GUILayout.Button("已开启", shortButtonStyle))
                {
                    _tips = false;
                }
            }

            EditorGUILayout.EndHorizontal();

            if (_tips == true)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                style.fontSize = 10;
                style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                GUILayout.Label("*开启后会显示每一个变量的详细功能信息，适合新使用材质的初学者", style);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.BeginChangeCheck();
            {
                MaterialProperty[] props = { };
                base.OnGUI(m_MaterialEditor, props);
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }

        void GUITip(Material material)
        {
            style.fontSize = 12;
            style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
            style.wordWrap = true;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(" 1.开启自定义数据时请先添加custom1.xyzw,再添加custom2.xyzw", style);

            GUILayout.Space(5);
            GUILayout.Label(" 2.开启溶解拖尾模式后，透明度改为溶解参数", style);

            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }

        void GUIUV()
        {
            //综合选项下拉菜单
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _UV_Foldout = Foldout(_UV_Foldout, "UV设置(UV Settings)");
            if (_UV_Foldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                m_MaterialEditor.ShaderProperty(UsePolarUV, "使用极坐标");
                m_MaterialEditor.ShaderProperty(CenterU, "中心U");
                m_MaterialEditor.ShaderProperty(CenterV, "中心V");
                EditorGUILayout.EndVertical();

                GUILayout.Space(5);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        void Gray()
        {
            m_MaterialEditor.ShaderProperty(ToGray, "变灰程度");
        }
    }
}