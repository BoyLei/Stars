/*
 * @Description: 特效中引用的图片资源位置检查
 */
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class FXRefImageLocalPathCheckDetail : CustomCheckDetail
    {
        [PropertySpace(10)]
        [DisableContextMenu(true, true), LabelText("特效中引用的图片指定的目录列表"), DisplayAsString]
        [ListDrawerSettings(Expanded = true, CustomAddFunction = "AddRefImagePath")]
        public List<string> refImageDirs = new List<string>() { };
        public void AddRefImagePath()
        {
            string file = EditorUtility.OpenFolderPanel("添加目录", Application.dataPath, "");
            if (string.IsNullOrEmpty(file))
            {
                return;
            }

            refImageDirs.Add(file.Substring(file.IndexOf("Assets")));
        }
    }

    [Serializable]
    [CustomScanType(EnumScanModes.动效资源检查, priority: 8)]
    public class FXRefImageLocalPathCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "特效中引用的图片资源位置检查";
            ruleDescription = "[说明]：可以用于检查特效中引用的图片资源必须是在指定的目录下面";
        }


        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<FXRefImageLocalPathCheckDetail> checkDetailList = new List<FXRefImageLocalPathCheckDetail>() { new FXRefImageLocalPathCheckDetail() };

        [CustomScanAction]
        public void Do_FXRefImageLocalPathCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunFXAssetsCheck.Do_FXRefImageLocalPathCheck, this);
        }
    }
}
