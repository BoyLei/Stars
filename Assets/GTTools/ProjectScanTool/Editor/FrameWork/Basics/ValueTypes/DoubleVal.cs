/*
 * @Description: 自定义double类型
 */

using System;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class DoubleVal : BaseVal<double>
    {
        /// <summary>
        /// 自定义double类型
        /// </summary>
        /// <param name="defaultVal">默认值</param>
        public DoubleVal(double defaultVal = 0f) : base(defaultVal)
        {
        }

        /// <summary>
        /// 自定义int类型
        /// </summary>
        /// <param name="title">字段标题</param>
        /// <param name="defaultVal">默认值</param>
        public DoubleVal(string title, double defaultVal) : base(title, defaultVal)
        {

        }

        /// <summary>
        /// 自定义int类型
        /// </summary>
        /// <param name="defaultVal">默认值</param>
        /// <param name="min">显示一个取值范围显示器，最小值</param>
        /// <param name="max">显示一个取值范围显示器，最大值</param>
        public DoubleVal(double defaultVal, double min, double max) : base(defaultVal, min, max)
        {

        }

        /// <summary>
        /// 自定义int类型
        /// </summary>
        /// <param name="title">字段标题</param>
        /// <param name="defaultVal">默认值</param>
        /// <param name="min">显示一个取值范围显示器，最小值</param>
        /// <param name="max">显示一个取值范围显示器，最大值</param>
        public DoubleVal(string title, double defaultVal, double min, double max) : base(title, defaultVal, min, max)
        {

        }
    }
}
