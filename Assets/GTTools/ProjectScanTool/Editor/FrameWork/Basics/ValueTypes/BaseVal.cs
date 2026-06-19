/*
 * @Description: 自定义基础数据类型，包括可以定义下拉列表数据、范围选择器
 */

using System;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, CustomValueEnabled]
    public class BaseVal<T> where T : IComparable
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
        [CustomPropertyRange("$Min", "$Max", "$useRangeGUI")]
        public T value;

        /// <summary>
        /// 数值以下拉列表选择来呈现，这里指代某个外部传入的下拉列表获取函数，函数需要是static，如"@ProjectScanGlobalConfig.allShaders"
        /// </summary>
        [NonSerialized]
        public string dropDownMethod = "";


        /// <summary>
        /// 自定义int类型
        /// </summary>
        /// <param name="title">字段标题</param>
        /// <param name="defaultVal">默认值</param>
        public BaseVal(T val)
        {
            value = val;
            this.useRangeGUI = false;
        }

        /// <summary>
        /// 自定义int类型
        /// </summary>
        /// <param name="title">字段标题</param>
        /// <param name="defaultVal">默认值</param>
        public BaseVal(string title, T val)
        {
            description = title;
            value = val;
            this.useRangeGUI = false;
        }

        /// <summary>
        /// 自定义int类型
        /// </summary>
        /// <param name="val">默认值</param>
        /// <param name="min">显示一个取值范围显示器，最小值</param>
        /// <param name="max">显示一个取值范围显示器，最大值</param>
        public BaseVal(T val, T min, T max)
        {
            value = val;
            this.Min = min.CompareTo(max) < 0 ? min : max;
            this.Max = min.CompareTo(max) < 0 ? max : min;
            this.useRangeGUI = true;
        }

        /// <summary>
        /// 自定义int类型
        /// </summary>
        /// <param name="title">字段标题</param>
        /// <param name="val">默认值</param>
        /// <param name="min">显示一个取值范围显示器，最小值</param>
        /// <param name="max">显示一个取值范围显示器，最大值</param>
        public BaseVal(string title, T val, T min, T max)
        {
            description = title;
            value = val;
            this.Min = min.CompareTo(max) < 0 ? min : max;
            this.Max = min.CompareTo(max) < 0 ? max : min;
            this.useRangeGUI = true;
        }

        /// <summary>
		/// The minimum value.
		/// </summary>
        [NonSerialized]
		public T Min;

        /// <summary>
        /// The maximum value.
        /// </summary>
        [NonSerialized]
        public T Max;

        //是否启用范围显示器
        [NonSerialized]
        public bool useRangeGUI = false;

    }

}
