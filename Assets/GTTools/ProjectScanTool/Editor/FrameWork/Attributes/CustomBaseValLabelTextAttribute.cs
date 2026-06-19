/*
 * @Description: 给BaseVal使用的LabelText，会优先取自定义BaseVal的描述字段，如果没有定义，则取定义的对象的名称
 */
namespace CasualEngine.ProjectScanTool
{
    using System;
    using Sirenix.OdinInspector;

    /// <seealso cref="TitleAttribute"/>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class CustomBaseValLabelTextAttribute : Attribute
    {
        /// <summary>
        /// The new text of the label.
        /// </summary>
        public string Text;

        /// <summary>
        /// Give a property a custom label.
        /// </summary>
        /// <param name="text">The new text of the label.</param>
        public CustomBaseValLabelTextAttribute(string text)
        {
            this.Text = text;
        }
    }
}