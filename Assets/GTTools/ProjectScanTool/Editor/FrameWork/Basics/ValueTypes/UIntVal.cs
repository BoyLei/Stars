/*
 * @Description: 自定义uint类型
 */
using System;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class UIntVal : BaseVal<uint>
    {
        /// <summary>
        /// 自定义int类型
        /// </summary>
        /// <param name="defaultVal">默认值</param>
        public UIntVal(uint defaultVal = 0) : base(defaultVal)
        {
        }

        /// <summary>
        /// 自定义int类型
        /// </summary>
        /// <param name="title">字段标题</param>
        /// <param name="defaultVal">默认值</param>
        public UIntVal(string title, uint defaultVal = 0) : base(title, defaultVal)
        {

        }

        /// <summary>
        /// 自定义int类型
        /// </summary>
        /// <param name="defaultVal">默认值</param>
        /// <param name="min">显示一个取值范围显示器，最小值</param>
        /// <param name="max">显示一个取值范围显示器，最大值</param>
        public UIntVal(uint defaultVal, uint min, uint max) : base(defaultVal, min, max)
        {

        }

        /// <summary>
        /// 自定义int类型
        /// </summary>
        /// <param name="title">字段标题</param>
        /// <param name="defaultVal">默认值</param>
        /// <param name="min">显示一个取值范围显示器，最小值</param>
        /// <param name="max">显示一个取值范围显示器，最大值</param>
        /// <param name="bEnable">是否启用，默认true</param>
        public UIntVal(string title, uint defaultVal, uint min, uint max) : base(title, defaultVal, min, max)
        {

        }
    }
}

