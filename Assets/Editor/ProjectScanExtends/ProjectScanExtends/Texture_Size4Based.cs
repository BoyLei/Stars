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
    public class Texture_Size4BasedCheckDetail : CustomCheckDetail
    {
        public IntVal checkNumBased = new IntVal(4, 1, 1024);
    }

    [Serializable]
    //注册特性，并绑定对应模块类型
    [CustomScanType(EnumScanModes.贴图资源检查)]
    public class Texture_Size4Based : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "贴图尺寸4的倍数";
            ruleDescription = "[说 明]: 贴图尺寸4的倍数。";
        }

        //定义检查规则列表
        [ListDrawerSettings(Expanded = true), LabelText("检查贴图尺寸4的倍数")]
        public List<Texture_Size4BasedCheckDetail> checkDetailList = new List<Texture_Size4BasedCheckDetail>() { new Texture_Size4BasedCheckDetail() };

        //注册扫描的执行逻辑
        [CustomScanAction]
        public void Do_CustomExampleCheck()
        {
            //添加逻辑
            ProjectScanHelper.DoCustomRuleCheck(Do_Texture4BasedSizeCheck, this);
        }

        #region 贴图尺寸4的倍数统计
        public static bool Do_Texture4BasedSizeCheck(Texture_Size4Based customRule)
        {
            foreach (var checkDetail in customRule.checkDetailList)
            {
                if (!checkDetail.checkNumBased.enable)
                {
                    continue;
                }
            }

            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.texture);
                foreach (var path in paths)
                {
                    Texture texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture)) as Texture;
                    if (texture == null)
                        continue;
                    TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (!textureImporter)
                        continue;

                    TextureUtils.GetTexWidthAndHeight(textureImporter, out int width, out int height);

                    if ((width % 4) != 0 || (height % 4) != 0)
                    {
                        customRule.Record(texture, methodName, $"贴图尺寸非4的倍数,当前尺寸{width}x{height}", path);
                    }
                }
            }
            return true;
        }
        #endregion
    }
}