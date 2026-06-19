/*
 * @Description: 扫描规则显示预览
 */
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Examples;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    internal class ScanRulePreview
    {
        public UnityEngine.Object customRule;

        private PropertyTree tree;

        public ScanRulePreview(UnityEngine.Object rule)
        {
            this.customRule = rule;
        }

        public void Draw()
        {
            if (this.customRule == null)
            {
                return;
            }
            if (BeginToggleGroup(this.customRule.GetInstanceID(), this.customRule as CustomRule, SirenixEditorGUI.DefaultFadeGroupDuration))
            {
                this.tree = (this.tree ?? PropertyTree.Create(this.customRule));
                this.tree.Draw(false);
            }
            SirenixEditorGUI.EndToggleGroup();

        }

        public bool BeginToggleGroup(object key, CustomRule customRule, float animationDuration)
        {
            ProjectScanGlobalConfig.InitCustomStyle();
            var rect = GUILayoutUtility.GetRect(16, ProjectScanGlobalConfig.customToggleTitleStyle.fixedHeight, ProjectScanGlobalConfig.customToggleTitleStyle);
            rect = EditorGUI.IndentedRect(rect);
            GUIHelper.IndentRect(ref rect);
            rect.xMin += 3;
            rect.xMax -= 3;
            GUI.Box(rect, customRule.expend ? EditorIcons.TriangleDown.Active : EditorIcons.TriangleRight.Active, ProjectScanGlobalConfig.customToggleTitleStyle);

            //标题
            var titleRect = rect;
            // titleRect.y += 2;
            titleRect.xMin += 25;

            GUI.Label(titleRect, customRule.ruleTitle, ProjectScanGlobalConfig.customLableStyle);

            var enableBtnRect = new Rect(rect.xMax - 62f, rect.y + 1, 60f, rect.height - 5);
            var checkBtnRect = new Rect(rect.xMax - 132f, rect.y + 1, 60f, rect.height - 5);
            var toggleRect = new Rect(rect.x + 4f, rect.y + 4f, rect.xMax - 150, rect.height);

            var e = Event.current;

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                if (toggleRect.Contains(e.mousePosition))
                {
                    GUIHelper.RequestRepaint();
                    customRule.expend = !customRule.expend;
                }
            }

            //一定不要在if (Event.current.type == EventType.Repaint)里面绘制按钮，否则会让按钮冒泡事件影响到其他内部布局功能的点击事件

            //操作按钮
            if (GUI.Button(checkBtnRect, "执行"))
            {
                ProjectScanHelper.DoScanCustomRuleByEntry(customRule, true);
            }

            GUIHelper.PushColor(customRule.enable ? Color.green : Color.red);
            if (GUI.Button(enableBtnRect, customRule.enable ? "开启" : "关闭"))
            {
                customRule.enable = !customRule.enable;
            }
            GUIHelper.PopColor();


            var result = SirenixEditorGUI.BeginFadeGroup(key, customRule.expend, animationDuration);
            GUILayout.BeginVertical(SirenixGUIStyles.None);
            GUIHelper.PushGUIEnabled(true);
            GUIHelper.PushHierarchyMode(false);
            EditorGUI.indentLevel++;
            return result;
        }

    }
}