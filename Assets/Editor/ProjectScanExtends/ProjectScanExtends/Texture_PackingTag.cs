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
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    [Serializable]
    public class Texture_PackingTagCheckDetail : CustomCheckDetail
    {
    }

    [Serializable]
    //注册特性，并绑定对应模块类型
    [CustomScanType(EnumScanModes.贴图资源检查)]
    public class Texture_PackingTag : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "贴图PackingTag置空检查";
            ruleDescription = "[说 明]: 检查贴图PackingTag是否置空，此检查项提供自动修正和后处理功能。";
        }

        //定义检查规则列表
        [ListDrawerSettings(Expanded = true), LabelText("检查贴图PackingTag置空")]
        public List<Texture_PackingTagCheckDetail> checkDetailList = new List<Texture_PackingTagCheckDetail>() { new Texture_PackingTagCheckDetail() };

        //注册扫描的执行逻辑
        [CustomScanAction]
        public void Do_CustomExampleCheck(string[] assetPostprocessorPath = null)
        {
            //添加逻辑
            ProjectScanHelper.DoCustomRuleCheck(Do_Texture4BasedSizeCheck, this, assetPostprocessorPath);
        }

        #region 检查贴图PackingTag置空
        public static bool Do_Texture4BasedSizeCheck(Texture_PackingTag customRule, string[] assetPostprocessorPath = null)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.texture, assetPostprocessorPath);
                foreach (var path in paths)
                {
                    Texture texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture)) as Texture;
                    if (texture == null)
                        continue;
                    TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (!textureImporter)
                        continue;

                    if (!string.IsNullOrEmpty(textureImporter.spritePackingTag))
                    {
                        customRule.Record(texture, methodName, $"贴图PackingTag未置空，当前值{textureImporter.spritePackingTag}", path);
                        //开启了自动修正
                        if (customRule.autoCorrection)
                        {
                            textureImporter.spritePackingTag = string.Empty;
                            EditorUtility.SetDirty(textureImporter);
                            textureImporter.SaveAndReimport();
                            EditorUtility.SetDirty(texture);
                        }
                    }
                }
            }
            return true;
        }
        #endregion
    }
}