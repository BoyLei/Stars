/*
 * @Description: 自定义枚举基础类型
 */

using System;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, CustomValueEnabled]
    public class BaseEnumVal<T>
    {
        /// <summary>
        /// 是否开启
        /// </summary>
        public bool enable = true;
        private void ChangeEnabledStatus()
        {
            enable = !enable;
            //记得设置状态改变，否则数据更改没有保存
            ProjectScanWindow.GetWindow().SetCurrentScanModeDirty();
        }

        /// <summary>
        /// 描述
        /// </summary>
        [NonSerialized]
        public string description = "NULL";

        /// <summary>
        /// 实际值
        /// </summary>
        [CustomBaseValLabelText("$description")]
        [CustomContextSimpleMenu("开启或关闭检查项", "ChangeEnabledStatus")]
        [ValueDropdown("@ProjectScanHelper.GetEnumPages(valType)")]
        public T value;

        /// <summary>
        /// 枚举类型
        /// </summary>
        private Type valType;

        public BaseEnumVal(T val, Type type)
        {
            value = val;
            valType = type;
        }

        public BaseEnumVal(string title, T val, Type type)
        {
            description = title;
            value = val;
            valType = type;
        }
    }

}
