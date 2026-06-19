/*
 * @Description: 自定义扫描规则基类，包括是否开放扫描，标题描述，基础说明，目标文件夹、忽略文件夹、白名单等，所有其他扫描规则都应当继承此基类
 */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    [Serializable]
    public class CustomScanMode : ScriptableObject
    {
        // [HideInInspector]
        [NonSerialized]
        public EnumScanModes scanMode;
    }
}
