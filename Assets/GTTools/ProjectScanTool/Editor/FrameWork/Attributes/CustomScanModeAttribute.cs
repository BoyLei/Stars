/*
 * @Description: 自定义扫描模块特性
 */
using System;
using System.Diagnostics;

namespace CasualEngine.ProjectScanTool
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class CustomScanModeAttribute : Attribute
    {
        public EnumScanModes scanMode;

        /// <summary>
        /// 自定义扫描类标签
        /// </summary>
        ///<param name="type">当前规则归属的模块</param>
        public CustomScanModeAttribute(EnumScanModes type)
        {
            scanMode = type;
        }
    }
}