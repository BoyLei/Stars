/*
 * @Description: 在原生基础上改动一个单纯的右键菜单，去掉系统内部的菜单项
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
	/// <summary>
	/// Adds a generic menu option to properties marked with <see cref="T:Sirenix.OdinInspector.CustomContextSimpleMenuAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.CustomContextSimpleMenuAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DisableContextMenuAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.OnInspectorGUIAttribute" />
	// Token: 0x02000399 RID: 921
	[DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
	public sealed class CustomContextSimpleMenuAttributeDrawer : OdinAttributeDrawer<CustomContextSimpleMenuAttribute>, IDefinesGenericMenuItems
	{
		/// <summary>
		/// Populates the generic menu for the property.
		/// </summary>
		// Token: 0x060011EB RID: 4587 RVA: 0x0004FEE0 File Offset: 0x0004E0E0
		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			if (this.populated.Value)
			{
				return;
			}
			this.populated.Value = true;
			if (this.contextMenuInfos.Value != null && this.contextMenuInfos.Value.Count > 0)
			{
				//移除掉一些乱七八糟的
				if (genericMenu.GetItemCount() > 0)
				{
					genericMenu.RemoveMenuItems("Paste");
					genericMenu.RemoveMenuItems("Copy");
					genericMenu.RemoveMenuItems("Copy Special/Deep Copy (default)");
					genericMenu.RemoveMenuItems("Copy Special/Shallow Copy");
					genericMenu.RemoveMenuItems("Copy Special/Copy Reference");
					genericMenu.RemoveMenuItems("Set To Null");
				}

				// if (genericMenu.GetItemCount() > 0)
				// {
				// 	genericMenu.AddSeparator("");
				// }
				foreach (KeyValuePair<CustomContextSimpleMenuAttribute, CustomContextSimpleMenuAttributeDrawer.ContextMenuInfo> keyValuePair in from n in this.contextMenuInfos.Value
				orderby n.Key.MenuItem ?? ""
				select n)
				{
					CustomContextSimpleMenuAttributeDrawer.ContextMenuInfo info = keyValuePair.Value;
					if (info.MethodCaller == null)
					{
						genericMenu.AddDisabledItem(new GUIContent(keyValuePair.Key.MenuItem + " (Invalid)"));
					}
					else
					{
						genericMenu.AddItem(new GUIContent(info.Name), false, delegate()
						{
							for (int i = 0; i < property.ParentValues.Count; i++)
							{
								info.MethodCaller(property.ParentValues[i]);
							}
						});
					}
				}
			}
		}

		// Token: 0x060011EC RID: 4588 RVA: 0x00050020 File Offset: 0x0004E220
		protected override void Initialize()
		{
			InspectorProperty property = this.Property;
			CustomContextSimpleMenuAttribute attribute = this.Attribute;
			this.contextMenuInfos = property.Context.GetGlobal("CustomContextSimpleMenu", (Dictionary<CustomContextSimpleMenuAttribute, ContextMenuInfo>)null);
			this.populated = property.Context.GetGlobal<bool>("CustomContextMenu_Populated", false);
			if (this.contextMenuInfos == null)
			{
				return;
			}

			if (this.contextMenuInfos.Value == null)
			{
				this.contextMenuInfos.Value = new Dictionary<CustomContextSimpleMenuAttribute, CustomContextSimpleMenuAttributeDrawer.ContextMenuInfo>();
			}
			if (!this.contextMenuInfos.Value.TryGetValue(attribute, out this.info))
			{
				this.info = new CustomContextSimpleMenuAttributeDrawer.ContextMenuInfo();
				MethodInfo member = property.ParentType.FindMember().IsMethod().IsInstance().HasNoParameters().ReturnsVoid().IsNamed(attribute.MethodName).GetMember<MethodInfo>(out this.info.ErrorMessage);
				if (this.info.ErrorMessage == null)
				{
					this.info.Name = attribute.MenuItem;
					this.info.MethodCaller = EmitUtilities.CreateWeakInstanceMethodCaller(member);
				}
				this.contextMenuInfos.Value[attribute] = this.info;
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		// Token: 0x060011ED RID: 4589 RVA: 0x0000BD80 File Offset: 0x00009F80
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (this.info == null)
			{
				this.CallNextDrawer(label);
				return;
			}
			this.populated.Value = false;
			if (this.info.ErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(this.info.ErrorMessage, true);
			}
			this.CallNextDrawer(label);
		}

		// Token: 0x04000AF4 RID: 2804
		private CustomContextSimpleMenuAttributeDrawer.ContextMenuInfo info;

		// Token: 0x04000AF5 RID: 2805
		private PropertyContext<Dictionary<CustomContextSimpleMenuAttribute, CustomContextSimpleMenuAttributeDrawer.ContextMenuInfo>> contextMenuInfos;

		// Token: 0x04000AF6 RID: 2806
		private PropertyContext<bool> populated;

		// Token: 0x0200039A RID: 922
		private class ContextMenuInfo
		{
			// Token: 0x04000AF7 RID: 2807
			public string ErrorMessage;

			// Token: 0x04000AF8 RID: 2808
			public string Name;

			// Token: 0x04000AF9 RID: 2809
			public Action<object> MethodCaller;
		}
	}
}
