/*
 * @Description: 自定义扫描类特性，包括标题、操作按钮等
 */
namespace CasualEngine.ProjectScanTool
{
    using UnityEngine;
    using UnityEditor;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities.Editor;

    /// <seealso cref="CustomValueEnabledAttribute"/>
    public class CustomValueEnabledAttributeDrawer<T> : OdinAttributeDrawer<CustomValueEnabledAttribute, T>
    {

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var property = entry.Property;
            var attribute = this.Attribute;

            var enableProperty = property.Children.Get("enable");

            //标记是否扫描的enable字段缺失
            if (enableProperty == null)
            {
                SirenixEditorGUI.ErrorMessageBox("enable" + " is not a member of " + property.NiceName + ".");
                return;
            }
            if (enableProperty.ValueEntry.TypeOfValue != typeof(bool))
            {
                SirenixEditorGUI.ErrorMessageBox("enable" + " on " + property.NiceName + "  must be a boolean.");
                return;
            }

            bool isEnabled = (bool)enableProperty.ValueEntry.WeakSmartValue;

            GUIHelper.PushGUIEnabled(isEnabled);
            for (int i = 0; i < property.Children.Count; i++)
            {
                var child = property.Children[i];
                if (child != enableProperty)
                {
                    child.Draw(child.Label);
                }
            }
            GUIHelper.PopGUIEnabled();

            enableProperty.ValueEntry.WeakSmartValue = isEnabled;
        }

    }
}