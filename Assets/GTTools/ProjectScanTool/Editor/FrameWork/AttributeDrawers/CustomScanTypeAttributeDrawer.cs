// /*
//  * @Description: 自定义扫描类特性，包括标题、操作按钮等
//  */
// namespace CasualEngine.ProjectScanTool
// {
//     using UnityEngine;
//     using UnityEditor;
//     using Sirenix.OdinInspector.Editor;
//     using Sirenix.Utilities.Editor;

//     /// <seealso cref="CustomScanTypeAttribute"/>
//     public class CustomScanTypeAttributeDrawer<T> : OdinAttributeDrawer<CustomScanTypeAttribute, T>
//     {
//         protected override void Initialize()
//         {
//             base.Initialize();
//             ProjectScanGlobalConfig.InitCustomStyle();
//             // CustomNotifyHandler.CCNotifyScanAllTypes -= DoScanAllCustomRule;
//             // CustomNotifyHandler.CCNotifyScanAllTypes += DoScanAllCustomRule;

//             // CustomNotifyHandler.CCNotifyOpenOrCloseAllScan -= SetCustomRuleEnable;
//             // CustomNotifyHandler.CCNotifyOpenOrCloseAllScan += SetCustomRuleEnable;
//         }


//         //被通知打开或关闭所有的规则
//         // private void SetCustomRuleEnable(EnumScanEnable enable)
//         // {
//         //     var entry = this.ValueEntry;
//         //     var property = entry.Property;

//         //     var toggleProperty = property.Children.Get("enable");
//         //     //标记是否扫描的enable字段缺失
//         //     if (toggleProperty == null)
//         //     {
//         //         return;
//         //     }
//         //     if (toggleProperty.ValueEntry.TypeOfValue != typeof(bool))
//         //     {
//         //         return;
//         //     }
//         //     toggleProperty.ValueEntry.WeakSmartValue = (enable == EnumScanEnable.open ? true : false);
//         //     GUIHelper.RequestRepaint();
//         // }

//         //被通知执行扫描所有既定的规则
//         // private void DoScanAllCustomRule()
//         // {
//         //     var entry = this.ValueEntry;
//         //     var attribute = this.Attribute;
//         //     // var methodInfo = entry.Property.Context.Get(this, "method_info_" + "test", (MethodInfo)null);
//         //     //获取类中所有的函数
//         //     MethodInfo[] methods = entry.SmartValue.GetType().GetMethods();
//         //     foreach (MethodInfo method in methods)
//         //     {
//         //         //过滤出被[CreateWarp]修饰的函数
//         //         if (method.IsDefined(typeof(CustomScanActionAttribute), true))
//         //         {
//         //             // CustomScanActionAttribute p = (CustomScanActionAttribute)Attribute.GetCustomAttribute((System.Reflection.MemberInfo)method, typeof(CustomScanActionAttribute));

//         //             method.Invoke(entry.SmartValue, null);
//         //         }
//         //     }
//         // }

//         //被通知执行扫描既定的规则
//         private void DoScanCustomRule()
//         {
//             ProjectScanHelper.DoScanCustomRuleByEntry(this.ValueEntry);
//         }

//         /// <summary>
//         /// Draws the property.
//         /// </summary>
//         protected override void DrawPropertyLayout(GUIContent label)
//         {
//             var entry = this.ValueEntry;
//             var property = entry.Property;
//             var attribute = this.Attribute;

//             var toggleProperty = property.Children.Get("enable");
//             var titleProperty = property.Children.Get("ruleTitle");

//             //标记是否扫描的enable字段缺失
//             if (toggleProperty == null)
//             {
//                 SirenixEditorGUI.ErrorMessageBox("enable" + " is not a member of " + property.NiceName + ".");
//                 return;
//             }
//             if (toggleProperty.ValueEntry.TypeOfValue != typeof(bool))
//             {
//                 SirenixEditorGUI.ErrorMessageBox("enable" + " on " + property.NiceName + "  must be a boolean.");
//                 return;
//             }

//             //标记扫描的类的ruleTitle字段缺失
//             if (titleProperty == null)
//             {
//                 SirenixEditorGUI.ErrorMessageBox("ruleTitle" + " is not a member of " + property.NiceName + ".");
//                 return;
//             }

//             if (titleProperty.ValueEntry.TypeOfValue != typeof(string))
//             {
//                 SirenixEditorGUI.ErrorMessageBox("ruleTitle" + " on " + property.NiceName + "  must be a string.");
//                 return;
//             }

//             string title = (string)titleProperty.ValueEntry.WeakSmartValue;
//             bool isEnabled = (bool)toggleProperty.ValueEntry.WeakSmartValue;
//             var isVisible = property.Context.GetPersistent(this, "isVisible", this.Attribute.HasDefinedExpanded ? this.Attribute.Expanded : SirenixEditorGUI.ExpandFoldoutByDefault);

//             PropertyContext<string> openGroup = null;

//             bool prev = isVisible.Value;
//             bool visibleBuffer = isVisible.Value;
//             if (BeginToggleGroup(UniqueDrawerKey.Create(property, this), ref isEnabled, ref visibleBuffer, title, SirenixEditorGUI.DefaultFadeGroupDuration))
//             {
//                 for (int i = 0; i < property.Children.Count; i++)
//                 {
//                     var child = property.Children[i];
//                     if (child != toggleProperty)
//                     {
//                         child.Draw(child.Label);
//                     }
//                 }
//             }
//             SirenixEditorGUI.EndToggleGroup();

//             isVisible.Value = visibleBuffer;
//             if (openGroup != null && prev != isVisible.Value && isVisible.Value)
//             {
//                 openGroup.Value = property.Path;
//             }

//             toggleProperty.ValueEntry.WeakSmartValue = isEnabled;
//         }


//         public bool BeginToggleGroup(object key, ref bool enabled, ref bool visible, string title, float animationDuration)
//         {
//             var rect = GUILayoutUtility.GetRect(16, ProjectScanGlobalConfig.customToggleTitleStyle.fixedHeight, ProjectScanGlobalConfig.customToggleTitleStyle);
//             rect = EditorGUI.IndentedRect(rect);
//             GUIHelper.IndentRect(ref rect);
//             rect.xMin += 3;
//             rect.xMax -= 3;
//             GUI.Box(rect, visible ? EditorIcons.TriangleDown.Active : EditorIcons.TriangleRight.Active, ProjectScanGlobalConfig.customToggleTitleStyle);

//             //标题
//             var titleRect = rect;
//             // titleRect.y += 2;
//             titleRect.xMin += 25;

//             GUI.Label(titleRect, title, ProjectScanGlobalConfig.customLableStyle);

//             var enableBtnRect = new Rect(rect.xMax - 62f, rect.y + 1, 60f, rect.height - 5);
//             var checkBtnRect = new Rect(rect.xMax - 132f, rect.y + 1, 60f, rect.height - 5);
//             var toggleRect = new Rect(rect.x + 4f, rect.y + 4f, rect.xMax - 150, rect.height);

//             var e = Event.current;

//             if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
//             {
//                 if (toggleRect.Contains(e.mousePosition))
//                 {
//                     GUIHelper.RequestRepaint();
//                     visible = !visible;
//                 }
//             }

//             //一定不要在if (Event.current.type == EventType.Repaint)里面绘制按钮，否则会让按钮冒泡事件影响到其他内部布局功能的点击事件

//             //操作按钮
//             if (GUI.Button(checkBtnRect, "执行"))
//             {
//                 DoScanCustomRule();
//             }

//             GUIHelper.PushColor(enabled ? Color.green : Color.red);
//             if (GUI.Button(enableBtnRect, enabled ? "开启" : "关闭"))
//             {
//                 enabled = !enabled;
//             }
//             GUIHelper.PopColor();


//             var result = SirenixEditorGUI.BeginFadeGroup(key, visible, animationDuration);
//             GUILayout.BeginVertical(SirenixGUIStyles.None);
//             GUIHelper.PushGUIEnabled(true);
//             GUIHelper.PushHierarchyMode(false);
//             EditorGUI.indentLevel++;
//             return result;
//         }

//     }
// }