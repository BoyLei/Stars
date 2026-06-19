/*
 * @Description: UI对齐工具
 */
using System;
using System.Linq;
using GameTechTools.CommonLibs.CommonExtends;
using GameTechTools.CommonLibs.UnityToolbarExtender;
using UnityEditor;
using UnityEngine;

namespace GameTechTools.UIHelpsTool
{
    //对齐模式类型
    internal enum EnumAlignType
    {
        eVerticalUp = 1, //顶对齐
        eVertical = 3, //垂直对齐
        eVerticalDown = 4, //底部对齐
        eHorziontalLeft = 5, //左对齐
        eHorziontal = 6, //水平对齐
        eHorziontalRight = 7, //右对齐


        eGridTool = 101, //网格工具
    }

    [InitializeOnLoad]
    internal class UIHelpsAlignTool
    {
        public static Texture2D mHorziontal_tex;
        public static Texture2D mHorziontalLeft_tex;
        public static Texture2D mHorziontalRight_tex;
        public static Texture2D mVertical_tex;
        public static Texture2D mVerticalDown_tex;
        public static Texture2D mVerticalUp_tex;
        public static Texture2D mAlignBg_tex;
        public static Texture2D mGridTool_tex;

        public static string res_dir = "Assets/GTTools/UIHelpsTool/Editor/Res/SketchImg/";
        public static string mAlignBg_path = res_dir + "UIHelpsTool_Align-bg.png";
        public static string mHorziontal_path = res_dir + "UIHelpsTool_Horziontal-icon.png";
        public static string mHorziontalLeft_path = res_dir + "UIHelpsTool_HorziontalLeft-icon.png";
        public static string mHorziontalRight_path = res_dir + "UIHelpsTool_HorziontalRight-icon.png";
        public static string mVertical_path = res_dir + "UIHelpsTool_Vertical-icon.png";
        public static string mVerticalDown_path = res_dir + "UIHelpsTool_VerticalDown-icon.png";
        public static string mVerticalUp_path = res_dir + "UIHelpsTool_VerticalUp-icon.png";
        public static string mGridTool_path = res_dir + "UIHelpsTool_GridTool-icon.png";


        private static bool initLine = false;
        static UIHelpsAlignTool()
        {
            if (GTHelper.LoadWithBatchmode)
            {
                return;
            }

            InitRes();

            ToolbarExtender.LeftToolbarGUI.Remove(OnToolbarGUI);
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptReload()
        {
            InitRes();
        }

        internal static void InitRes()
        {
            mAlignBg_tex = AssetDatabase.LoadAssetAtPath<Texture2D>(mAlignBg_path);
            mHorziontal_tex = AssetDatabase.LoadAssetAtPath<Texture2D>(mHorziontal_path);
            mHorziontalLeft_tex = AssetDatabase.LoadAssetAtPath<Texture2D>(mHorziontalLeft_path);
            mHorziontalRight_tex = AssetDatabase.LoadAssetAtPath<Texture2D>(mHorziontalRight_path);
            mVertical_tex = AssetDatabase.LoadAssetAtPath<Texture2D>(mVertical_path);
            mVerticalDown_tex = AssetDatabase.LoadAssetAtPath<Texture2D>(mVerticalDown_path);
            mVerticalUp_tex = AssetDatabase.LoadAssetAtPath<Texture2D>(mVerticalUp_path);
            mGridTool_tex = AssetDatabase.LoadAssetAtPath<Texture2D>(mGridTool_path);
        }

        private static int panddingX = 1, icon_width = 28, icon_height = 22;
        static void OnToolbarGUI()
        {
            InitRes();
            GUILayout.FlexibleSpace();

            int icon_Num = 6;
            float bgWidth = icon_Num * (icon_width + panddingX) + 10;
            int offsetX = 50;

            DrawAlignTypes(mVerticalUp_tex, offsetX, "顶对齐", EnumAlignType.eVerticalUp);
            offsetX += icon_width + panddingX;

            DrawAlignTypes(mVertical_tex, offsetX, "垂直居中对齐", EnumAlignType.eVertical);
            offsetX += icon_width + panddingX;

            DrawAlignTypes(mVerticalDown_tex, offsetX, "底对齐", EnumAlignType.eVerticalDown);
            offsetX += icon_width + panddingX;

            DrawAlignTypes(mHorziontalLeft_tex, offsetX, "左对齐", EnumAlignType.eHorziontalLeft);
            offsetX += icon_width + panddingX;

            DrawAlignTypes(mHorziontal_tex, offsetX, "水平居中对齐", EnumAlignType.eHorziontal);
            offsetX += icon_width + panddingX;

            DrawAlignTypes(mHorziontalRight_tex, offsetX, "右对齐", EnumAlignType.eHorziontalRight);

            offsetX += icon_width + panddingX + 20;
            DrawAlignTypes(mGridTool_tex, offsetX, "网格工具", EnumAlignType.eGridTool);
        }

        private static void DrawAlignTypes(Texture2D tex, int offsetX, string tips, EnumAlignType alignTyps)
        {
            var lastPadding = GUI.skin.button.padding;
            GUI.skin.button.padding = new RectOffset(3, 3, 1, 1);
            if (GUI.Button(new Rect(offsetX, 0, icon_width, icon_height), new GUIContent(tex, tips), GUI.skin.button))
            {
                SelectAlignTypes(alignTyps);
            };
            GUI.skin.button.padding = lastPadding;
        }

        private static void SelectAlignTypes(EnumAlignType alignTyps)
        {
            switch (alignTyps)
            {
                case EnumAlignType.eVerticalUp:
                    DoAlign_VerticalUp();
                    break;
                case EnumAlignType.eVertical:
                    DoAlign_Vertical();
                    break;
                case EnumAlignType.eVerticalDown:
                    DoAlign_VerticalDown();
                    break;
                case EnumAlignType.eHorziontalLeft:
                    DoAlign_HorziontalLeft();
                    break;
                case EnumAlignType.eHorziontal:
                    DoAlign_Horziontal();
                    break;
                case EnumAlignType.eHorziontalRight:
                    DoAlign_HorziontalRight();
                    break;
                case EnumAlignType.eGridTool:
                    UIHelpsGridToolSettingWindow.Open();
                    break;
            }
        }

        private static void DoAlign_VerticalUp()
        {
            float y = (float)Math.Round(Mathf.Max(Selection.gameObjects.Select(obj => obj.transform.localPosition.y +
        ((RectTransform)obj.transform).sizeDelta.y / 2).ToArray()), 6);
            foreach (GameObject gameObject in Selection.gameObjects)
            {
                Undo.RecordObject(gameObject.transform, "DoAlign_VerticalUp " + gameObject.transform.name);
                gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, y - ((RectTransform)gameObject.transform).sizeDelta.y / 2);
                EditorUtility.SetDirty(gameObject);
            }
        }

        private static void DoAlign_Vertical()
        {
            int len = Selection.gameObjects.Length;
            float minY = (float)Math.Round(Mathf.Min(Selection.gameObjects.Select(obj => obj.transform.localPosition.y).ToArray()), 6);
            float maxY = (float)Math.Round(Mathf.Max(Selection.gameObjects.Select(obj => obj.transform.localPosition.y).ToArray()), 6);
            float avg = (maxY - minY) / (len - 1);
            var objects = Selection.gameObjects.ToList();
            objects.Sort((x, y) => (int)(x.transform.localPosition.y - y.transform.localPosition.y));
            for (int i = 0; i < len; i++)
            {
                Undo.RecordObject(objects[i].transform, "DoAlign_Vertical " + objects[i].transform.name);
                objects[i].transform.localPosition = new Vector3(objects[i].transform.localPosition.x, minY + avg);
                EditorUtility.SetDirty(objects[i]);
            }
        }

        private static void DoAlign_VerticalDown()
        {
            float y = (float)Math.Round(Mathf.Min(Selection.gameObjects.Select(obj => obj.transform.localPosition.y -
                ((RectTransform)obj.transform).sizeDelta.y / 2).ToArray()), 6);

            foreach (GameObject gameObject in Selection.gameObjects)
            {
                Undo.RecordObject(gameObject.transform, "DoAlign_VerticalDown " + gameObject.transform.name);
                gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, y + ((RectTransform)gameObject.transform).sizeDelta.y / 2);
                EditorUtility.SetDirty(gameObject);
            }
        }

        private static void DoAlign_HorziontalLeft()
        {
            float x = (float)Math.Round(Mathf.Min(Selection.gameObjects.Select(obj => obj.transform.localPosition.x -
                ((RectTransform)obj.transform).sizeDelta.x / 2).ToArray()), 6);
            foreach (GameObject gameObject in Selection.gameObjects)
            {
                Undo.RecordObject(gameObject.transform, "DoAlign_HorziontalLeft " + gameObject.transform.name);
                gameObject.transform.localPosition = new Vector2(x + ((RectTransform)gameObject.transform).sizeDelta.x / 2, gameObject.transform.localPosition.y);
                EditorUtility.SetDirty(gameObject);
            }
        }

        private static void DoAlign_Horziontal()
        {
            int len = Selection.gameObjects.Length;
            float minX = (float)Math.Round(Mathf.Min(Selection.gameObjects.Select(obj => obj.transform.localPosition.x).ToArray()), 6);
            float maxX = (float)Math.Round(Mathf.Max(Selection.gameObjects.Select(obj => obj.transform.localPosition.x).ToArray()), 6);
            float avg = (maxX - minX) / (len - 1);
            var objects = Selection.gameObjects.ToList();
            objects.Sort((x, y) => (int)(x.transform.localPosition.x - y.transform.localPosition.x));
            for (int i = 0; i < len; i++)
            {
                Undo.RecordObject(objects[i].transform, "DoAlign_Horziontal " + objects[i].transform.name);
                objects[i].transform.localPosition = new Vector3(minX + avg, objects[i].transform.localPosition.y);
                EditorUtility.SetDirty(objects[i]);
            }
        }

        private static void DoAlign_HorziontalRight()
        {
            float x = (float)Math.Round(Mathf.Max(Selection.gameObjects.Select(obj => obj.transform.localPosition.x +
                ((RectTransform)obj.transform).sizeDelta.x / 2).ToArray()), 6);
            foreach (GameObject gameObject in Selection.gameObjects)
            {
                Undo.RecordObject(gameObject.transform, "DoAlign_HorziontalRight " + gameObject.transform.name);
                gameObject.transform.localPosition = new Vector3(x -
                ((RectTransform)gameObject.transform).sizeDelta.x / 2, gameObject.transform.localPosition.y);
                EditorUtility.SetDirty(gameObject);
            }
        }


    }

}