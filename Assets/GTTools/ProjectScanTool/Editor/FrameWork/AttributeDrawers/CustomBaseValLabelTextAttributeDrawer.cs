/*
 * @Description: 给BaseVal使用的LabelText，会优先取自定义BaseVal的描述字段，如果没有定义，则取定义的对象的名称
 */
#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Utilities.Editor;
    using UnityEngine;
    using UnityEditor;
    using CasualEngine.ProjectScanTool;

    /// <summary>
    /// 给BaseVal使用的LabelText，会优先取自定义BaseVal的描述字段，如果没有定义，则取定义的对象的名称 <see cref="CustomBaseValLabelTextAttribute"/>.
    /// </summary>
    /// <seealso cref="CustomBaseValLabelTextAttribute"/>
    /// <seealso cref="HideLabelAttribute"/>
    /// <seealso cref="TooltipAttribute"/>
    /// <seealso cref="LabelWidthAttribute"/>
    /// <seealso cref="TitleAttribute"/>
    /// <seealso cref="HeaderAttribute"/>
    /// <seealso cref="GUIColorAttribute"/>

    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public sealed class CustomBaseValLabelTextAttributeDrawer : OdinAttributeDrawer<CustomBaseValLabelTextAttribute>
    {
        private GUIContent overrideLabel;
        private StringMemberHelper stringHelper;

        protected override void Initialize()
        {
            this.stringHelper = new StringMemberHelper(this.Property, this.Attribute.Text);
        }

        /// <summary>
        /// Draws the attribute.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.stringHelper.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.stringHelper.ErrorMessage);
            }
            GUIContent useLabel;

            var str = this.stringHelper.GetString(this.Property);
            if (str != null && str == "NULL")
            {
                str = this.Property.Parent.Label.text;
            }

            if (str == null)
            {
                useLabel = null;
            }
            else
            {
                if (overrideLabel == null) overrideLabel = new GUIContent();
                overrideLabel.text = str;
                useLabel = overrideLabel;
            }

            this.CallNextDrawer(useLabel);
        }
    }
}
#endif