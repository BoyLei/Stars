/*
 * @Description: 自定义基础数据类型，包括可以定义下拉列表数据
 */

using System;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, CustomValueEnabled]
    public class BaseStringVal<T> where T : IComparable
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
        /// Label描述
        /// </summary>
        [NonSerialized]
        public string description = "NULL";

        /// <summary>
        /// 实际值
        /// </summary>
        [CustomBaseValLabelText("$description")]
        [CustomContextSimpleMenu("开启或关闭检查项", "ChangeEnabledStatus")]
        [CustomValueDropdown("$dropDownMethod")]
        public T value;

        /// <summary>
        /// 数值以下拉列表选择来呈现，这里指代某个外部传入的下拉列表获取函数，函数需要是static，如"@ProjectScanGlobalConfig.allShaders"
        /// </summary>
        [NonSerialized]
        public string dropDownMethod = "";


        /// <summary>
        /// 自定义string类型
        /// </summary>
        /// <param name="title">字段标题</param>
        /// <param name="defaultVal">默认值</param>
        /// <param name="bEnable">是否启用，默认true</param>
        public BaseStringVal(T val)
        {
            value = val;
        }

        /// <summary>
        /// 自定义string类型
        /// </summary>
        /// <param name="title">字段标题</param>
        /// <param name="defaultVal">默认值</param>
        /// <param name="bEnable">是否启用，默认true</param>
        public BaseStringVal(string title, T val)
        {
            description = title;
            value = val;
        }
    }

}
