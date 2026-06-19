/*
 * @Description: 自定义枚举类型
 */

using System;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class EnumVal : BaseEnumVal<int>
    {
        /// <summary>
        /// 自定义枚举类型
        /// </summary>
        /// <param name="title">字段标题</param>
        /// <param name="defaultVal">默认值</param>
        /// <param name="type">枚举类型</param>
        /// <param name="bEnable">是否启用，默认true</param>
        public EnumVal(int defaultVal, Type type) : base(defaultVal, type)
        {
        }

        /// <summary>
        /// 自定义枚举类型
        /// </summary>
        /// <param name="title">字段标题</param>
        /// <param name="defaultVal">默认值</param>
        /// <param name="type">枚举类型</param>
        /// <param name="bEnable">是否启用，默认true</param>
        public EnumVal(string title, int defaultVal, Type type) : base(title, defaultVal, type)
        {
        }
    }
}
