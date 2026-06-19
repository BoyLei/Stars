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
    public class Texture_SizePowerOf2CheckDetail : CustomCheckDetail
    {
       
    }

    [Serializable]
    //注册特性，并绑定对应模块类型
    [CustomScanType(EnumScanModes.贴图资源检查)]
    public class Texture_SizePowerOf2 : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "贴图是2的幂次";
            ruleDescription = "[说 明]: 贴图是2的幂次。";
        }

        //定义检查规则列表
        [ListDrawerSettings(Expanded = true), LabelText("检查贴图尺寸是2的幂次")]
        public List<Texture_SizePowerOf2CheckDetail> checkDetailList = new List<Texture_SizePowerOf2CheckDetail>() { new Texture_SizePowerOf2CheckDetail() };

        //注册扫描的执行逻辑
        [CustomScanAction]
        public void Do_CustomExampleCheck()
        {
            //添加逻辑
            ProjectScanHelper.DoCustomRuleCheck(Do_TexturePowerOf2SizeCheck, this);
        }

        #region 贴图尺寸2的幂次统计
        public static bool Do_TexturePowerOf2SizeCheck(Texture_SizePowerOf2 customRule)
        {
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
                    if (!IsPowerOfTwo(width) || !IsPowerOfTwo(height))
                    {
                        customRule.Record(texture, methodName, $"贴图不是2的幂次,当前尺寸{width}x{height}", path);
                    }
                }
            }
            return true;
        }
        private static bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0 && x != 0;
        }
        #endregion
    }
}