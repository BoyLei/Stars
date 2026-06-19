/*
 * @Description: 材质误引用图集
 */
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class MaterialRefAtlasCheckDetail : CustomCheckDetail
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
    [CustomScanType(EnumScanModes.基本资源检查, priority: 4)]
    public class MaterialRefAtlasCheck : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "材质误引用图集";
            ruleDescription = "[说明]：材质资源不能使用Sprite图集，否则引用的贴图会被打成单个图集。";
        }


        [ListDrawerSettings(Expanded = true), LabelText("检查规则")]
        public List<MaterialRefAtlasCheckDetail> checkDetailList = new List<MaterialRefAtlasCheckDetail>() { new MaterialRefAtlasCheckDetail() };

        [CustomScanAction]
        public void Do_MaterialRefAtlasCheck()
        {
            ProjectScanHelper.DoCustomRuleCheck(RunBasicAssetsCheck.Do_MaterialRefAtlasCheck, this);
        }
    }
}
