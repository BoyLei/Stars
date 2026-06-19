/*
 * @Description: 创建新的分类
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
    internal class CreateNewPreviewTypeWindow : OdinEditorWindow
    {
        [PropertySpace(SpaceBefore = 5)]
        [DisplayAsString(false), HideLabel]
        public string tips = "提示\n分类名称可以如'通用按钮'\n也支持多目录结构，以'/'分割, 如 '常用/通用按钮'";

        [PropertySpace(SpaceBefore = 5)]
        [LabelText("分类名称"), LabelWidth(60)]
        public string typeName = "";

        protected override void OnEnable()
        {
            base.OnEnable();
            GTEditorWindowFocusMgr.Instance.PushEditorWindow(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GTEditorWindowFocusMgr.Instance.PopEditorWindow(this);
        }

        public static void OpenWindow()
        {
            CreateNewPreviewTypeWindow window = GetWindow<CreateNewPreviewTypeWindow>();
            window.titleContent = new GUIContent("添加类型");
            GUIHelper.GetEditorWindowRect().AlignCenterXY(300, 150);
            window.minSize = new Vector2(300, 150);
            window.maxSize = new Vector2(300, 150);
        }

        [HorizontalGroup("add", PaddingLeft = 100, PaddingRight = 100)]
        [PropertySpace(SpaceBefore = 20)]
        [Button("添加", ButtonSizes.Large)]
        [LabelWidth(100)]
        private void SaveInfo()
        {
            if (string.IsNullOrEmpty(typeName))
            {
                EditorUtility.DisplayDialog("", "类型名称不能为空", "确认");
                return;
            }

            if (GeneralCompWindow.mainWindow == null)
            {
                this.Close();
                return;
            }

            bool ret = GeneralCompWindow.GetWindow().Do_AddNewPreviewType(typeName);
            if (ret)
            {
                this.Close();
            }
        }
    }
}