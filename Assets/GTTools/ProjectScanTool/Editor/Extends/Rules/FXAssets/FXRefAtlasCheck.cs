/*
 * @Description: 特效误引用图集
 */
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class FXRefAtlasCheckDetail : CustomCheckDetail
    {
        // [PropertySpace(10)]
        // [DisableContextMenu(true, true), LabelText("图集目录，用于识别不能引用当前目录图集资源"), DisplayAsString]
        // [ListDrawerSettings(Expanded = true, CustomAddFunction = "AddSpriteAtlasPath")]
        // public List<string> spriteAtlasDirs = new List<string>() { "Assets/Res/UI/UIAtlas" };
        // public void AddSpriteAtlasPath()
        // {
        //     string file = EditorUtility.OpenFolderPanel("添加目录", Application.dataPath, "");
        //     if (string.IsNullOrEmpty(file))
        //     {
        //         return;
        //     }

        //     spriteAtlasDirs.Add(file.Substring(file.IndexOf("Assets")));
        // }
    }

    [Serializable]
    [CustomScanType(EnumScanModes.动效资源检查, priority: 7)]
    public class FXRefAtlasCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "特效误引用图集";
            ruleDescription = "[说明]：特效中的图片资源不可用sprite图集，否则引用的贴图会被打成单个图集。这里只检查特效中的图片资源，材质的检查可以开启[基本资源检查-材质误引用图集]检查项";
        }


        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<FXRefAtlasCheckDetail> checkDetailList = new List<FXRefAtlasCheckDetail>() { new FXRefAtlasCheckDetail() };

        [CustomScanAction]
        public void Do_MaterialRefAtlasCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunFXAssetsCheck.Do_FXRefAtlasCheck, this);
        }
    }
}
