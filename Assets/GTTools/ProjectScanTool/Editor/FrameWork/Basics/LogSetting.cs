/*
 * @Description: 通用设置-日志设置
 */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    [Serializable]
    [HideLabel]
    [InfoBox("点击打开目录按钮，可以查看所有检测日志输出的目录\n右键单个项检测规则的说明内容可以直接打开单个项检测的日志")]
    public class LogSetting
    {
        public CustomSelectBox printLogToFile = new CustomSelectBox("将检查结果输出到文本中", true);
        // public CustomSelectBox mergerLog = new CustomSelectBox("将检查结果文本合并");

        // public CustomSelectBox pringLogToList = new CustomSelectBox("将检查结果显示在界面中");

        [LabelText("右键查看单个检查项日志示例")]
		[InlineEditor(InlineEditorModes.LargePreview, PreviewWidth = 548, PreviewHeight = 246)]
		public Texture exampleMode;


        public void Init()
        {
            exampleMode = AssetDatabase.LoadAssetAtPath("Assets/GTTools/ProjectScanTool/Editor/Sketchmap/CustomRightClick.jpg", typeof(Texture)) as Texture;
        }

    }
}
