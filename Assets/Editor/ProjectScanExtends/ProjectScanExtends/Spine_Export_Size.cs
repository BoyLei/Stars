//在ProjectScanDef.cs中添加新的模块类型 (如果需要添加新模块的话，否则直接添加检查项即可)

//扫描类型唯一标识
//public enum EnumScanModes
//{
//    通用设置 = 0,
//    基本资源检查,
//    贴图资源检查,
//    音频资源检查,
//    动效资源检查,
//    场景检查,
//    eExampleCheck
//}

//自定义某个检测项
using GameTechTools.CommonLibs.CommonExtends;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    [Serializable] 
    public class Spine_Export_SizeCheckDetail : CustomCheckDetail
    {
    }

    [Serializable]
    //注册特性，并绑定对应模块类型
    [CustomScanType(EnumScanModes.动效资源检查)]
    public class Spine_Export_Size : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "Spine导出数据大小检查";
            ruleDescription = "[说 明]: Spine导出数据大小检查";
        }

        //定义检查规则列表
        [ListDrawerSettings(Expanded = true), LabelText("Spine导出数据大小检查")]
        public List<Spine_Export_SizeCheckDetail> checkDetailList = new List<Spine_Export_SizeCheckDetail>() { new Spine_Export_SizeCheckDetail() };

        //注册扫描的执行逻辑
        [CustomScanAction]
        public void Do_CustomExampleCheck()
        {
            //添加逻辑
            ProjectScanHelper.DoCustomRuleCheck(Do_SpineExportJsonCheck, this);
        }

        #region Spine导出JSON数据大小检查
        public static bool Do_SpineExportJsonCheck(Spine_Export_Size customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.textAsset);
                foreach (var path in paths)
                {
                    TextAsset textAsset = AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)) as TextAsset;
                    if (textAsset == null)
                        continue;

                    if (path.EndsWith(".json") || path.EndsWith(".bytes"))
                    {
                        var filePath = Application.dataPath + "/../" + path;
                        FileInfo fileInfo = new FileInfo(filePath);
                        long size = fileInfo.Length / 1024;
                        if(size > 200)
                        {
                            customRule.Record(textAsset, methodName, $"Spine导出数据大小超过200KB，当前大小{size}KB", path);
                        }
                    }
                }
            }
            return true;
        }
        #endregion
    }
}