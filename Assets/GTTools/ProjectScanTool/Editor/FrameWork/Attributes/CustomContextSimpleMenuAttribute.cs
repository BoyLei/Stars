/*
 * @Description: 在原生基础上改动一个单纯的右键菜单，去掉系统内部的菜单项
 */
using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace CasualEngine.ProjectScanTool
{
	/// <summary>
	/// <para>在原生基础上改动一个单纯的右键菜单，去掉系统内部的菜单项</para>
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.DisableContextMenuAttribute" />
	// Token: 0x02000039 RID: 57
	[DontApplyToListElements]
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public sealed class CustomContextSimpleMenuAttribute : Attribute
	{
		/// <summary>
		/// Adds a custom option to the context menu of the property.
		/// </summary>
		/// <param name="menuItem">The name of the menu item.</param>
		/// <param name="methodName">The name of the callback method.</param>
		// Token: 0x0600006E RID: 110 RVA: 0x000025BF File Offset: 0x000007BF
		public CustomContextSimpleMenuAttribute(string menuItem, string methodName)
		{
			this.MenuItem = menuItem;
			this.MethodName = methodName;
		}

		/// <summary>
		/// The name of the menu item.
		/// </summary>
		// Token: 0x04000092 RID: 146
		public string MenuItem;

		/// <summary>
		/// The name of the callback method.
		/// </summary>
		// Token: 0x04000093 RID: 147
		public string MethodName;
	}
}
