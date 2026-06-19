/*
 * @Description: 自定义bool类型
 */
using System;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class BoolVal : BaseBoolVal<bool>
    {
        /// <summary>
        /// 自定义bool类型
        /// </summary>
        /// <param name="defaultVal">默认值</param>
        public BoolVal(bool defaultVal = false) : base(defaultVal)
        {
        }

        /// <summary>
        /// 自定义bool类型
        /// </summary>
        /// <param name="title">字段标题</param>
        /// <param name="defaultVal">默认值</param>
        public BoolVal(string title, bool defaultVal = false) : base(title, defaultVal)
        {
        }

    }
}
