/*
 * @Description: 修改ValueDropdown特性，增加当传入mehod名称为空时，特性不进行绘制，注意，这个时配套BaseVal自定义数据类型使用
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
	/// <summary>
	/// <para>修改版ValueDropdown特性.只能用于BaseVal这类自定义数据中</para>
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.ValueDropdownList`1" />
	// Token: 0x02000069 RID: 105
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public class CustomValueDropdownAttribute : Attribute
	{
		/// <summary>
		/// Creates a dropdown menu for a property.
		/// </summary>
		/// <param name="memberName">Name of any field, property or method member that implements IList. E.g. arrays or Lists.</param>
		// Token: 0x060000D3 RID: 211 RVA: 0x00002AAF File Offset: 0x00000CAF
		public CustomValueDropdownAttribute(string memberName)
		{
			this.NumberOfItemsBeforeEnablingSearch = 10;
			this.MemberName = memberName;
			this.DrawDropdownForListElements = true;
		}

		/// <summary>
		/// Name of any field, property or method member that implements IList. E.g. arrays or Lists.
		/// </summary>
		// Token: 0x04000100 RID: 256
		public string MemberName;

		/// <summary>
		/// The number of items before enabling search. Default is 10.
		/// </summary>
		// Token: 0x04000101 RID: 257
		public int NumberOfItemsBeforeEnablingSearch;

		/// <summary>
		/// False by default.
		/// </summary>
		// Token: 0x04000102 RID: 258
		public bool IsUniqueList;

		/// <summary>
		/// True by default. If the ValueDropdown attribute is applied to a list, then disabling this,
		/// will render all child elements normally without using the ValueDropdown. The ValueDropdown will
		/// still show up when you click the add button on the list drawer, unless <see cref="F:Sirenix.OdinInspector.CustomValueDropdownAttribute.DisableListAddButtonBehaviour" /> is true.
		/// </summary>
		// Token: 0x04000103 RID: 259
		public bool DrawDropdownForListElements;

		/// <summary>
		/// False by default.
		/// </summary>
		// Token: 0x04000104 RID: 260
		public bool DisableListAddButtonBehaviour;

		/// <summary>
		/// If the ValueDropdown attribute is applied to a list, and <see cref="F:Sirenix.OdinInspector.CustomValueDropdownAttribute.IsUniqueList" /> is set to true, then enabling this,
		/// will exclude existing values, instead of rendering a checkbox indicating whether the item is already included or not.
		/// </summary>
		// Token: 0x04000105 RID: 261
		public bool ExcludeExistingValuesInList;

		/// <summary>
		/// If the dropdown renders a tree-view, then setting this to true will ensure everything is expanded by default.
		/// </summary>
		// Token: 0x04000106 RID: 262
		public bool ExpandAllMenuItems;

		/// <summary>
		/// If true, instead of replacing the drawer with a wide dropdown-field, the dropdown button will be a little button, drawn next to the other drawer.
		/// </summary>
		// Token: 0x04000107 RID: 263
		public bool AppendNextDrawer;

		/// <summary>
		/// Disables the the GUI for the appended drawer. False by default.
		/// </summary>
		// Token: 0x04000108 RID: 264
		public bool DisableGUIInAppendedDrawer;

		/// <summary>
		/// By default, a single click selects and confirms the selection.
		/// </summary>
		// Token: 0x04000109 RID: 265
		public bool DoubleClickToConfirm;

		/// <summary>
		/// By default, the dropdown will create a tree view.
		/// </summary>
		// Token: 0x0400010A RID: 266
		public bool FlattenTreeView;

		/// <summary>
		/// Gets or sets the width of the dropdown. Default is zero.
		/// </summary>
		// Token: 0x0400010B RID: 267
		public int DropdownWidth;

		/// <summary>
		/// Gets or sets the height of the dropdown. Default is zero.
		/// </summary>
		// Token: 0x0400010C RID: 268
		public int DropdownHeight;

		/// <summary>
		/// Gets or sets the title for the dropdown. Null by default.
		/// </summary>
		// Token: 0x0400010D RID: 269
		public string DropdownTitle;

		/// <summary>
		/// False by default.
		/// </summary>
		// Token: 0x0400010E RID: 270
		public bool SortDropdownItems;

		/// <summary>
		/// Whether to draw all child properties in a foldout.
		/// </summary>
		// Token: 0x0400010F RID: 271
		public bool HideChildProperties;
	}


    public class ICustomValueDropdownEqualityComparer : IEqualityComparer<object>
	{
		// Token: 0x06001239 RID: 4665 RVA: 0x0000BFE0 File Offset: 0x0000A1E0
		public ICustomValueDropdownEqualityComparer(bool isTypeLookup)
		{
			this.isTypeLookup = isTypeLookup;
		}

		// Token: 0x0600123A RID: 4666 RVA: 0x00051A88 File Offset: 0x0004FC88
		public bool Equals(object x, object y)
		{
			if (x is ValueDropdownItem)
			{
				x = ((ValueDropdownItem)x).Value;
			}
			if (y is ValueDropdownItem)
			{
				y = ((ValueDropdownItem)y).Value;
			}
			if (EqualityComparer<object>.Default.Equals(x, y))
			{
				return true;
			}
			if (x == null != (y == null))
			{
				return false;
			}
			if (this.isTypeLookup)
			{
				Type type = (x as Type) ?? x.GetType();
				Type type2 = (y as Type) ?? y.GetType();
				if (type == type2)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600123B RID: 4667 RVA: 0x00051B0C File Offset: 0x0004FD0C
		public int GetHashCode(object obj)
		{
			if (obj == null)
			{
				return -1;
			}
			if (obj is ValueDropdownItem)
			{
				obj = ((ValueDropdownItem)obj).Value;
			}
			if (obj == null)
			{
				return -1;
			}
			if (this.isTypeLookup)
			{
				Type type = (obj as Type) ?? obj.GetType();
				return type.GetHashCode();
			}
			return obj.GetHashCode();
		}

		// Token: 0x04000B2E RID: 2862
		private bool isTypeLookup;
	}

    
}
