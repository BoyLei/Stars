/*
 * @Description: 自定义string类型
 */
using System;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel]
    public class StringVal : BaseStringVal<string>
    {
        /// <summary>
        /// 自定义string类型
        /// </summary>
        /// <param name="defaultVal">默认值</param>
        public StringVal(string defaultVal = "") : base(defaultVal)
        {
        }


         /// <summary>
        /// 自定义string类型
        /// </summary>
        /// <param name="title">字段标题</param>
        /// <param name="defaultVal">默认值</param>
        public StringVal(string title, string defaultVal) : base(title, defaultVal)
        {
        }


    }
}
