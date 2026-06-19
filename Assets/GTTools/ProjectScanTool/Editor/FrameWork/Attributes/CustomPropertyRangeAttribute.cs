/*
 * @Description: 修改PropertyRange特性，增加动态控制是否绘制，注意，这个是配套BaseVal自定义数据类型使用
 */

namespace CasualEngine.ProjectScanTool
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// <para>修改版PropertyRange特性，增加动态控制是否绘制，注意，这个是配套BaseVal自定义数据类型使用.</para>
    /// </summary>
    /// <seealso cref="ShowInInspectorAttribute"/>
    /// <seealso cref="PropertySpaceAttribute"/>
    /// <seealso cref="PropertyTooltipAttribute"/>
    /// <seealso cref="PropertyOrderAttribute"/>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public sealed class CustomPropertyRangeAttribute : Attribute
    {
        //是否应用显示
        public string EnableMethod;

        /// <summary>
        /// The minimum value.
        /// </summary>
        public double Min;

        /// <summary>
        /// The maximum value.
        /// </summary>
        public double Max;

        /// <summary>
        /// The name of a field, property or method to get the min value from.
        /// </summary>
        public string MinMember;

        /// <summary>
        /// The name of a field, property or method to get the max value from.
        /// </summary>
        public string MaxMember;

        /// <summary>
        /// Creates a slider control to set the value of the property to between the specified range..
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public CustomPropertyRangeAttribute(double min, double max, string enableMethod)
        {
            this.Min = min < max ? min : max;
            this.Max = max > min ? max : min;
            this.EnableMethod = enableMethod;
        }

        /// <summary>
        /// Creates a slider control to set the value of the property to between the specified range..
        /// </summary>
        /// <param name="minMember">The name of a field, property or method to get the min value from.</param>
        /// <param name="max">The maximum value.</param>
        public CustomPropertyRangeAttribute(string minMember, double max, string enableMethod)
        {
            this.MinMember = minMember;
            this.Max = max;
            this.EnableMethod = enableMethod;
        }

        /// <summary>
        /// Creates a slider control to set the value of the property to between the specified range..
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="maxMember">The name of a field, property or method to get the max value from.</param>
        public CustomPropertyRangeAttribute(double min, string maxMember, string enableMethod)
        {
            this.Min = min;
            this.MaxMember = maxMember;
            this.EnableMethod = enableMethod;
        }

        /// <summary>
        /// Creates a slider control to set the value of the property to between the specified range..
        /// </summary>
        /// <param name="minMember">The name of a field, property or method to get the min value from.</param>
        /// <param name="maxMember">The name of a field, property or method to get the max value from.</param>
        public CustomPropertyRangeAttribute(string minMember, string maxMember, string enableMethod)
        {
            this.MinMember = minMember;
            this.MaxMember = maxMember;
            this.EnableMethod = enableMethod;
        }
    }
}