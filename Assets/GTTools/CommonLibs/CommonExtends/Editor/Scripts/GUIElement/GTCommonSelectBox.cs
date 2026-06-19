/*
 * @Description: 自定义选择框
 */
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameTechTools.CommonLibs.CommonExtends
{
    [Serializable]
    [HideLabel]
    public class GTCommonSelectBox
    {
        /// <summary>
        /// 是否开启
        /// </summary>
        [HorizontalGroup("a", Width = 15)]
        [DisableContextMenu(true, true), HideLabel]
        public bool enable = false;

        /// <summary>
        /// 选项描述
        /// </summary>
        [HorizontalGroup("a")]
        [DisableContextMenu(true, true), HideLabel, DisplayAsString, NonSerialized, ShowInInspector]
        public string description = "";

        public GTCommonSelectBox(string desc = "", bool bEnable = false)
        {
            description = desc;
            enable = bEnable;
        }
    }
}
