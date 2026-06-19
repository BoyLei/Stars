/*
 * @Description: 自定义long类型
 */

using System;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class LongVal : BaseVal<long>
    {
        /// <summary>
        /// 自定义long类型
        /// </summary>
        /// <param name="defaultVal">默认值</param>
        public LongVal(long defaultVal = 0) : base(defaultVal)
        {
        }

        /// <summary>
        /// 自定义int类型
        /// </summary>
        /// <param name="title">字段标题</param>
        /// <param name="defaultVal">默认值</param>
        public LongVal(string title, long defaultVal = 0) : base(title, defaultVal)
        {

        }

        /// <summary>
        /// 自定义int类型
        /// </summary>
        /// <param name="defaultVal">默认值</param>
        /// <param name="min">显示一个取值范围显示器，最小值</param>
        /// <param name="max">显示一个取值范围显示器，最大值</param>
        public LongVal(long defaultVal, long min, long max) : base(defaultVal, min, max)
        {

        }

        /// <summary>
        /// 自定义int类型
        /// </summary>
        /// <param name="title">字段标题</param>
        /// <param name="defaultVal">默认值</param>
        /// <param name="min">显示一个取值范围显示器，最小值</param>
        /// <param name="max">显示一个取值范围显示器，最大值</param>
        public LongVal(string title, long defaultVal, long min, long max) : base(title, defaultVal, min, max)
        {

        }
    }
}
