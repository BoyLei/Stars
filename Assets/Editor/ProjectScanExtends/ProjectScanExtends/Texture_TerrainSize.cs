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
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CasualEngine.ProjectScanTool
{
    [Serializable] 
    public class Texture_TerrainSizeCheckDetail : CustomCheckDetail
    {
       
    }

    [Serializable]
    //注册特性，并绑定对应模块类型
    [CustomScanType(EnumScanModes.贴图资源检查)]
    public class Texture_TerrainSize : CustomRule
    {
        void OnEnable()
        {
            ruleTitle = "Terrain贴图尺寸优化";
            ruleDescription = "[说 明]:这个terrainTexture 是在导场景的时候用的  导出之后就没有用了 一直存在项目中 占用内存 这里把尺寸改到最小 可以优化百分之99的内存";
        }

        //定义检查规则列表
        [ListDrawerSettings(Expanded = true), LabelText("Terrain贴图尺寸优化")]
        public List<Texture_TerrainSizeCheckDetail> checkDetailList = new List<Texture_TerrainSizeCheckDetail>() { new Texture_TerrainSizeCheckDetail() };

        //注册扫描的执行逻辑
        [CustomScanAction]
        public void Do_CustomExampleCheck()
        {
            //添加逻辑
            ProjectScanHelper.DoCustomRuleCheck(Do_TextureTerrainSizeCheck, this);
        }

        #region Terrain贴图尺寸优化
        public static bool Do_TextureTerrainSizeCheck(Texture_TerrainSize customRule)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            foreach (var checkDetail in customRule.checkDetailList)
            {
                string[] paths = ProjectScanHelper.GetAssetPathsByType(customRule, checkDetail, AssetType.prefab);
                foreach (var path in paths)
                {
                    GameObject terrainData = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                    if (terrainData == null) continue;
                    if (terrainData.GetComponent<Terrain>() == null) continue;
                    customRule.Record(terrainData, methodName, $"Terrain贴图尺寸 path:{path} ", path);
                }
            }
           
            return true;
        }
        #endregion
    }
}