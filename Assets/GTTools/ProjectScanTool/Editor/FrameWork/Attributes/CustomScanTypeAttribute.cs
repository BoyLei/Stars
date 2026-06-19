/*
 * @Description: 自定义扫描类特性
 */
using System;
using System.Diagnostics;

namespace CasualEngine.ProjectScanTool
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class CustomScanTypeAttribute : Attribute
    {
        /// <summary>
        /// 是否默认展开
        /// </summary>
        public bool Expanded;

        /// <summary>
        /// 所属模块
        /// </summary>
        public EnumScanModes scanMode;

        /// <summary>
        /// 显示的权值，越低的显示在前面
        /// </summary>
        public int Priority = 99999;

        /// <summary>
        /// 自定义扫描类标签
        /// </summary>
        ///<param name="type">当前规则归属的模块</param>
        public CustomScanTypeAttribute(EnumScanModes type, bool expanded = false)
        {
            this.scanMode = type;
            this.Expanded = expanded;
            this.HasDefinedExpanded = true;
        }

        /// <summary>
        /// 自定义扫描类标签
        /// </summary>
        ///<param name="type">当前规则归属的模块</param>
        public CustomScanTypeAttribute(EnumScanModes type, int priority, bool expanded = false)
        {
            this.scanMode = type;
            this.Priority = priority;
            this.Expanded = expanded;
            this.HasDefinedExpanded = true;
        }

        /// <summary>
        /// 是否有定义展开状态
        /// </summary>
        public bool HasDefinedExpanded { get; private set; }

    }
}