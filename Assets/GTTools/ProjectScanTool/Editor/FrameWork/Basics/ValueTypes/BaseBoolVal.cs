/*
 * @Description: 自定义Bool基础数据类型
 */

using System;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, CustomValueEnabled]
    public class BaseBoolVal<T> where T : IComparable
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
        public T value;

        /// <summary>
        /// 自定义int类型
        /// </summary>
        /// <param name="defaultVal">默认值</param>
        public BaseBoolVal(T val)
        {
            value = val;
        }

        /// <summary>
        /// 自定义int类型
        /// </summary>
        /// <param name="title">字段标题</param>
        /// <param name="defaultVal">默认值</param>
        public BaseBoolVal(string title, T val)
        {
            description = title;
            value = val;
        }
    }

}
