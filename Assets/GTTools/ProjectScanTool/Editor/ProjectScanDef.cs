/*
 * @Description: 扫描类型定义
 */
using System;
using System.Collections.Generic;
using GameTechTools.CommonLibs.CommonExtends;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    /// <summary>
    /// 扫描模块唯一标识，需要新增扫描模块时，在下面自增枚举值，相关自定义模块和规则特性需要注册当前标识，并不会序列化存储，所以顺序无关
    /// </summary>
    public enum EnumScanModes
    {
        通用设置 = 0,
        基本资源检查,
        贴图资源检查,
        音频资源检查,
        动效资源检查,
        场景检查,
    }

    public class ProjectScanDef
    {
        /// <summary>
        /// 获得左侧栏目的Icon图标，图标参考样例可以在Tools/Odin Inspector/Unity Icons菜单栏选择
        /// </summary>
        public static Texture GetScanModeIconTex(EnumScanModes scanMode)
        {
            switch (scanMode)
            {
                case EnumScanModes.通用设置:
                    return GTCommonMenuConfig.GetCustomOdinMenuIcon("GTOdinMenu_GeneralSettingIcon");
                case EnumScanModes.基本资源检查:
                    return GTCommonMenuConfig.GetCustomOdinMenuIcon("GTOdinMenu_DefaultPrefabIcon");
                case EnumScanModes.贴图资源检查:
                    return GTCommonMenuConfig.GetCustomOdinMenuIcon("GTOdinMenu_TextureIcon");
                case EnumScanModes.音频资源检查:
                    return GTCommonMenuConfig.GetCustomOdinMenuIcon("GTOdinMenu_AudioIcon");
                case EnumScanModes.动效资源检查:
                    return GTCommonMenuConfig.GetCustomOdinMenuIcon("GTOdinMenu_EffectIcon");
                case EnumScanModes.场景检查:
                    return GTCommonMenuConfig.GetCustomOdinMenuIcon("GTOdinMenu_SceneIcon");
                default:
                    return GTCommonMenuConfig.GetCustomOdinMenuIcon("GTOdinMenu_GeneralSettingIcon");
            }
        }
    }

}
