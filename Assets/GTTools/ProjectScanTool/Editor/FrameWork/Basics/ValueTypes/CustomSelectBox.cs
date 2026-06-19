/*
 * @Description: 自定义选择框
 */

using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    [Serializable]
    [PropertySpace(2)]
    [HideLabel]
    public class CustomSelectBox
    {
        /// <summary>
        /// 是否开启
        /// </summary>
        [HorizontalGroup("a", Width = 15)]
        [DisableContextMenu(true, true), HideLabel, GUIColor("GetCheckBoxColor")]
        public bool enable = false;
        private Color GetCheckBoxColor()
        {
            return enable ? new Color(0, 1, 0) : new Color(1, 0, 0);
        }

        /// <summary>
        /// 选项描述
        /// </summary>
        [HorizontalGroup("a")]
        [DisableContextMenu(true, true), HideLabel, DisplayAsString, NonSerialized, ShowInInspector]
        public string description = "";

        public CustomSelectBox(string desc = "", bool bEnable = false)
        {
            description = desc;
            enable = bEnable;
        }
    }
}
