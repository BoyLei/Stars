/*
 * @Description: UI辅助工具设置
 */

using GameTechTools.CommonLibs.CommonExtends;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace GameTechTools.UIHelpsTool
{
    //菜单类型
    internal enum EnumMenuItemType
    {
        eGeneralSetting = 1,   //通用设置菜单
        eGeneralCompPreview = 2,    //prefab预览分类菜单
    }

    //相关文件存储路径
    internal class UIHelpsToolConfigure
    {
        public const string FolderName = "GTTools/UIHelpsTool/Editor";
        public static string UIHelpsDataPath = Application.dataPath + "/" + FolderName;
        public static string UIHelpsAssetPath = "Assets/" + FolderName;
        public static string PreviewPath = UIHelpsAssetPath + "/CachePreviews";
        public static string ConfigDataPath = UIHelpsAssetPath + "/ConfigData/";
    }

    [InitializeOnLoad]
    internal class UIHelpsToolGlobalConfig
    {
        /// <summary>
        /// 全局的通用设置
        /// </summary>
        public static UIHelpsToolSetting generalSetting;

        static UIHelpsToolGlobalConfig()
        {
            InitGeneralData();
        }

        //初始化通用设置配置
        public static void InitGeneralData()
        {
            generalSetting = AssetDatabase.LoadAssetAtPath<UIHelpsToolSetting>(UIHelpsToolConfigure.ConfigDataPath + typeof(UIHelpsToolSetting).Name + ".asset");
            if (generalSetting == null)
            {
                generalSetting = GTHelper.CreateAsset<UIHelpsToolSetting>(UIHelpsToolConfigure.ConfigDataPath, "UIHelpsToolSetting");
            }
        }
    }


    internal class UIHelpsToolSettingWindow : OdinEditorWindow
    {
        private static UIHelpsToolSettingWindow mainWindow;
        
        [HideLabel, InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public UIHelpsToolSetting generalSetting;

        public static void Open()
        {
            UIHelpsToolGlobalConfig.InitGeneralData();
            UIHelpsToolSettingWindow window = GetWindow<UIHelpsToolSettingWindow>("UI辅助设置");
            window.Show();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenterXY(500f, 400f);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            mainWindow = this;
            mainWindow.generalSetting = UIHelpsToolGlobalConfig.generalSetting;
        }

        protected override void OnDestroy()
        {
            mainWindow = null;
            base.OnDestroy();
        }


    }
}