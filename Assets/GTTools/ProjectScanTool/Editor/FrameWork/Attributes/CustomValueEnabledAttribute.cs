/*
 * @Description: 自定义属性是否启用特性
 */
using System;
using System.Diagnostics;

namespace CasualEngine.ProjectScanTool
{
    /// <summary>
	/// <para>CustomValueEnabled，自定义属性是否启用特性，只能用于BaseVal这类自定义数据中</para>
	/// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class CustomValueEnabledAttribute : Attribute
    {

    }
}