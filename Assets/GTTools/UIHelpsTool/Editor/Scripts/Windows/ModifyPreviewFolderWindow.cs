/*
 * @Description: 修改分类目录名称
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
    internal class ModifyPreviewFolderWindow : OdinEditorWindow
    {
        [PropertySpace(SpaceBefore = 5)]
        [DisplayAsString(false), HideLabel]
        public string tips = "提示\n目录名称可以如'通用按钮'\n也支持多目录结构，以'/'分割, 如 '常用/通用按钮'";

        [PropertySpace(SpaceBefore = 5)]
        [LabelText("目录名称(旧)"), ReadOnly, LabelWidth(90)]
        public string preName = "";

        [PropertySpace(SpaceBefore = 5)]
        [LabelText("目录名称(新)"), LabelWidth(90)]
        public string curName = "";

        private OdinMenuItem modifyItem;
        private static ModifyPreviewFolderWindow window;
        protected override void OnEnable()
        {
            base.OnEnable();
            window = this;
            GTEditorWindowFocusMgr.Instance.PushEditorWindow(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GTEditorWindowFocusMgr.Instance.PopEditorWindow(this);
        }

        public static void OpenWindow(OdinMenuItem menuItem)
        {
            window = GetWindow<ModifyPreviewFolderWindow>();
            window.titleContent = new GUIContent("修改目录名称");
            GUIHelper.GetEditorWindowRect().AlignCenterXY(300, 150);
            window.minSize = new Vector2(300, 170);
            window.maxSize = new Vector2(300, 170);
            window.modifyItem = menuItem;
            window.preName = menuItem.Name;
        }

        [HorizontalGroup("modify", PaddingLeft = 100, PaddingRight = 100)]
        [PropertySpace(SpaceBefore = 20)]
        [Button("修改", ButtonSizes.Large)]
        [LabelWidth(100)]
        private void SaveInfo()
        {
            if (string.IsNullOrEmpty(curName))
            {
                EditorUtility.DisplayDialog("", "类型名称不能为空", "确认");
                return;
            }

            if (curName == preName)
            {
                EditorUtility.DisplayDialog("", "新的名称不能和旧的名称相同", "确认");
                return;
            }

            string warning = "确认修改分类名称？\n";
            var sure = EditorUtility.DisplayDialog("提示", warning, "确定", "取消");
            if (!sure)
            {
                return;
            }

            if (GeneralCompWindow.mainWindow == null)
            {
                this.Close();
                return;
            }

            bool ret = GeneralCompWindow.GetWindow().Do_ModifyPreviewFolderName(window.modifyItem, preName, curName);
            if (ret)
            {
                this.Close();
            }
        }
    }
}