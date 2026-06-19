/*
 * @Description: 修改ValueDropdown特性，增加当传入mehod名称为空时，特性不进行绘制，注意，这个时配套BaseVal自定义数据类型使用
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.CustomValueDropdownAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.CustomValueDropdownAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ValueDropdownItem`1" />
	/// <summary>
	/// Draws the property.
	/// </summary>
	// Token: 0x020003AD RID: 941
	[DrawerPriority(0.0, 0.0, 2002.0)]
	public sealed class CustomValueDropdownAttributeDrawer : OdinAttributeDrawer<CustomValueDropdownAttribute>
	{
        //实际的函数名称
        private string realMethodName;

        // Token: 0x04000B14 RID: 2836
		private string error;

		// Token: 0x04000B15 RID: 2837
		private GUIContent label;

		// Token: 0x04000B16 RID: 2838
		private bool isList;

		// Token: 0x04000B17 RID: 2839
		private bool isListElement;

		// Token: 0x04000B18 RID: 2840
		private Func<IEnumerable<ValueDropdownItem>> getValues;

		// Token: 0x04000B19 RID: 2841
		private Func<IEnumerable<object>> getSelection;

		// Token: 0x04000B1A RID: 2842
		private IEnumerable<object> result;

		// Token: 0x04000B1B RID: 2843
		private bool enableMultiSelect;

		// Token: 0x04000B1C RID: 2844
		private Dictionary<object, string> nameLookup;

		// Token: 0x04000B1D RID: 2845
		private InspectorPropertyValueGetter<object> rawGetter;

		// Token: 0x04000B1E RID: 2846
		private LocalPersistentContext<bool> isToggled;

		// Token: 0x04000B1F RID: 2847
		private GenericSelector<object> inlineSelector;

		// Token: 0x04000B20 RID: 2848
		private IEnumerable<object> nextResult;

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		// Token: 0x0600121D RID: 4637 RVA: 0x00050FF8 File Offset: 0x0004F1F8
		protected override void Initialize()
		{
			this.realMethodName = "";
            if (this.Property.ValueEntry == null || string.IsNullOrEmpty(this.Attribute.MemberName))
            {
                return;
            }
            var memberProperty = this.Property.FindParent(x => x.Info.GetMemberInfo() != null, true);
            var parentType = memberProperty.ParentType;

            //去掉$号，实际拿到的是变量dropDownMethod
            var expression = this.Attribute.MemberName.Substring(1);
            //获得dropDownMethod变量实际的内容表达式
            this.realMethodName = (string)parentType.GetField(expression).GetValue(memberProperty.Parent.ValueEntry.WeakSmartValue);
            if (string.IsNullOrEmpty(realMethodName))
            {
                return;
            }

            this.rawGetter = new InspectorPropertyValueGetter<object>(this.Property, this.realMethodName, true, true);
			this.isToggled = this.GetPersistentValue("Toggled", SirenixEditorGUI.ExpandFoldoutByDefault);
			this.error = this.rawGetter.ErrorMessage;
			this.isList = (base.Property.ChildResolver is IOrderedCollectionResolver);
			this.isListElement = (base.Property.Info.GetMemberInfo() == null);
			this.getSelection = (() => base.Property.ValueEntry.WeakValues.Cast<object>());
			this.getValues = delegate()
			{
				object value = this.rawGetter.GetValue();
				if (value != null)
				{
					return (from object x in value as IEnumerable
					where x != null
					select x).Select(delegate(object x)
					{
						if (x is ValueDropdownItem)
						{
							return (ValueDropdownItem)x;
						}
						if (x is IValueDropdownItem)
						{
							IValueDropdownItem valueDropdownItem = x as IValueDropdownItem;
							return new ValueDropdownItem(valueDropdownItem.GetText(), valueDropdownItem.GetValue());
						}
						return new ValueDropdownItem(null, x);
					});
				}
				return null;
			};
			this.ReloadDropdownCollections();
		}

		// Token: 0x0600121E RID: 4638 RVA: 0x000510A8 File Offset: 0x0004F2A8
		private void ReloadDropdownCollections()
		{
			if (this.error != null)
			{
				return;
			}
			object obj = null;
			object value = this.rawGetter.GetValue();
			if (value != null)
			{
				obj = (value as IEnumerable).Cast<object>().FirstOrDefault<object>();
			}
			bool flag = obj is IValueDropdownItem;
			if (flag)
			{
				IEnumerable<ValueDropdownItem> enumerable = this.getValues();
				this.nameLookup = new Dictionary<object, string>(new ICustomValueDropdownEqualityComparer(false));
				using (IEnumerator<ValueDropdownItem> enumerator = enumerable.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ValueDropdownItem valueDropdownItem = enumerator.Current;
						this.nameLookup[valueDropdownItem] = valueDropdownItem.Text;
					}
					return;
				}
			}
			this.nameLookup = null;
		}

		// Token: 0x0600121F RID: 4639 RVA: 0x0000BF7E File Offset: 0x0000A17E
		private static IEnumerable<ValueDropdownItem> ToValueDropdowns(IEnumerable<object> query)
		{
			return query.Select(delegate(object x)
			{
				if (x is ValueDropdownItem)
				{
					return (ValueDropdownItem)x;
				}
				if (x is IValueDropdownItem)
				{
					IValueDropdownItem valueDropdownItem = x as IValueDropdownItem;
					return new ValueDropdownItem(valueDropdownItem.GetText(), valueDropdownItem.GetValue());
				}
				return new ValueDropdownItem(null, x);
			});
		}

		/// <summary>
		/// Draws the property with GUILayout support. This method is called by DrawPropertyImplementation if the GUICallType is set to GUILayout, which is the default.
		/// </summary>
		// Token: 0x06001220 RID: 4640 RVA: 0x00051164 File Offset: 0x0004F364
		protected override void DrawPropertyLayout(GUIContent label)
		{
			this.label = label;
            if (this.Property.ValueEntry == null || string.IsNullOrEmpty(this.realMethodName) || string.IsNullOrEmpty(this.Attribute.MemberName))
            {
                this.CallNextDrawer(label);
                return;
            }

            if (this.rawGetter == null || this.rawGetter.GetValue() == null)
            {
                this.CallNextDrawer(label);
                return;
            }
			if (this.error != null)
			{
				SirenixEditorGUI.ErrorMessageBox(this.error, true);
				base.CallNextDrawer(label);
				return;
			}
			if (this.isList)
			{
				if (base.Attribute.DisableListAddButtonBehaviour)
				{
					base.CallNextDrawer(label);
					return;
				}
				CollectionDrawerStaticInfo.NextCustomAddFunction = new Action(this.OpenSelector);
				base.CallNextDrawer(label);
				if (this.result != null)
				{
					this.AddResult(this.result);
					this.result = null;
					return;
				}
			}
			else
			{
				if (base.Attribute.DrawDropdownForListElements || !this.isListElement)
				{
					this.DrawDropdown();
					return;
				}
				base.CallNextDrawer(label);
			}
		}

		// Token: 0x06001221 RID: 4641 RVA: 0x00051224 File Offset: 0x0004F424
		private void AddResult(IEnumerable<object> query)
		{
			if (this.isList)
			{
				IOrderedCollectionResolver orderedCollectionResolver = base.Property.ChildResolver as IOrderedCollectionResolver;
				if (this.enableMultiSelect)
				{
					orderedCollectionResolver.QueueClear();
				}
				using (IEnumerator<object> enumerator = query.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						object[] array = new object[base.Property.ParentValues.Count];
						for (int i = 0; i < array.Length; i++)
						{
							array[i] = Sirenix.Serialization.SerializationUtility.CreateCopy(obj);
						}
						orderedCollectionResolver.QueueAdd(array);
					}
					return;
				}
			}
			object obj2 = query.FirstOrDefault<object>();
			for (int j = 0; j < base.Property.ValueEntry.WeakValues.Count; j++)
			{
				base.Property.ValueEntry.WeakValues[j] = Sirenix.Serialization.SerializationUtility.CreateCopy(obj2);
			}
		}

		// Token: 0x06001222 RID: 4642 RVA: 0x00051310 File Offset: 0x0004F510
		private void DrawDropdown()
		{
			IEnumerable<object> enumerable;
			if (base.Attribute.AppendNextDrawer && !this.isList)
			{
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				float num = 15f;
				if (this.label != null)
				{
					num += GUIHelper.BetterLabelWidth;
				}
				enumerable = OdinSelector<object>.DrawSelectorDropdown(this.label, GUIContent.none, new Func<Rect, OdinSelector<object>>(this.ShowSelector), GUIStyle.none, GUILayoutOptions.Width(num));
				if (Event.current.type == EventType.Repaint)
				{
					Rect rect = GUILayoutUtility.GetLastRect().AlignRight(15f);
					rect.y += 4f;
					SirenixGUIStyles.PaneOptions.Draw(rect, GUIContent.none, 0);
				}
				GUILayout.BeginVertical(new GUILayoutOption[0]);
				bool disableGUIInAppendedDrawer = base.Attribute.DisableGUIInAppendedDrawer;
				if (disableGUIInAppendedDrawer)
				{
					GUIHelper.PushGUIEnabled(false);
				}
				base.CallNextDrawer(null);
				if (disableGUIInAppendedDrawer)
				{
					GUIHelper.PopGUIEnabled();
				}
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
			else
			{
				string currentValueName = this.GetCurrentValueName();
				if (!base.Attribute.HideChildProperties && base.Property.Children.Count > 0)
				{
					Rect rect2;
					this.isToggled.Value = SirenixEditorGUI.Foldout(this.isToggled.Value, this.label, out rect2, null);
					enumerable = OdinSelector<object>.DrawSelectorDropdown(rect2, currentValueName, new Func<Rect, OdinSelector<object>>(this.ShowSelector), null);
					if (SirenixEditorGUI.BeginFadeGroup(this, this.isToggled.Value))
					{
						EditorGUI.indentLevel++;
						for (int i = 0; i < base.Property.Children.Count; i++)
						{
							InspectorProperty inspectorProperty = base.Property.Children[i];
							inspectorProperty.Draw(inspectorProperty.Label);
						}
						EditorGUI.indentLevel--;
					}
					SirenixEditorGUI.EndFadeGroup();
				}
				else
				{
					enumerable = OdinSelector<object>.DrawSelectorDropdown(this.label, currentValueName, new Func<Rect, OdinSelector<object>>(this.ShowSelector), null, new GUILayoutOption[0]);
				}
			}
			if (enumerable != null && enumerable.Any<object>())
			{
				this.AddResult(enumerable);
			}
		}

		// Token: 0x06001223 RID: 4643 RVA: 0x00051518 File Offset: 0x0004F718
		private void OpenSelector()
		{
			this.ReloadDropdownCollections();
			Rect rect = new Rect(Event.current.mousePosition, Vector2.zero);
			OdinSelector<object> odinSelector = this.ShowSelector(rect);
			odinSelector.SelectionConfirmed += delegate(IEnumerable<object> x)
			{
				this.result = x;
			};
		}

		// Token: 0x06001224 RID: 4644 RVA: 0x0005155C File Offset: 0x0004F75C
		private OdinSelector<object> ShowSelector(Rect rect)
		{
			GenericSelector<object> genericSelector = this.CreateSelector();
			rect.x = (float)((int)rect.x);
			rect.y = (float)((int)rect.y);
			rect.width = (float)((int)rect.width);
			rect.height = (float)((int)rect.height);
			if (base.Attribute.AppendNextDrawer && !this.isList)
			{
				rect.xMax = GUIHelper.GetCurrentLayoutRect().xMax;
			}
			genericSelector.ShowInPopup(rect, new Vector2((float)base.Attribute.DropdownWidth, (float)base.Attribute.DropdownHeight));
			return genericSelector;
		}

		// Token: 0x06001225 RID: 4645 RVA: 0x00051600 File Offset: 0x0004F800
		private GenericSelector<object> CreateSelector()
		{
			base.Attribute.IsUniqueList = (base.Attribute.IsUniqueList || base.Attribute.ExcludeExistingValuesInList);
			IEnumerable<ValueDropdownItem> enumerable = this.getValues() ?? Enumerable.Empty<ValueDropdownItem>();
			if (enumerable.Any<ValueDropdownItem>())
			{
				if ((this.isList && base.Attribute.ExcludeExistingValuesInList) || (this.isListElement && base.Attribute.IsUniqueList))
				{
					List<ValueDropdownItem> list = enumerable.ToList<ValueDropdownItem>();
					InspectorProperty inspectorProperty = base.Property.FindParent((InspectorProperty x) => x.ChildResolver is IOrderedCollectionResolver, true);
					ICustomValueDropdownEqualityComparer comparer = new ICustomValueDropdownEqualityComparer(false);
					inspectorProperty.ValueEntry.WeakValues.Cast<IEnumerable>().SelectMany((IEnumerable x) => x.Cast<object>()).ForEach(delegate(object x)
					{
						list.RemoveAll((ValueDropdownItem c) => comparer.Equals(c, x));
					});
					enumerable = list;
				}
				if (this.nameLookup != null)
				{
					foreach (ValueDropdownItem valueDropdownItem in enumerable)
					{
						if (valueDropdownItem.Value != null)
						{
							this.nameLookup[valueDropdownItem.Value] = valueDropdownItem.Text;
						}
					}
				}
			}
			bool drawSearchToolbar = base.Attribute.NumberOfItemsBeforeEnablingSearch == 0 || (enumerable != null && enumerable.Take(base.Attribute.NumberOfItemsBeforeEnablingSearch).Count<ValueDropdownItem>() == base.Attribute.NumberOfItemsBeforeEnablingSearch);
			GenericSelector<object> genericSelector = new GenericSelector<object>(base.Attribute.DropdownTitle, false, from x in enumerable
			select new GenericSelectorItem<object>(x.Text, x.Value));
			this.enableMultiSelect = (this.isList && base.Attribute.IsUniqueList && !base.Attribute.ExcludeExistingValuesInList);
			if (base.Attribute.FlattenTreeView)
			{
				genericSelector.FlattenedTree = true;
			}
			if (this.isList && !base.Attribute.ExcludeExistingValuesInList && base.Attribute.IsUniqueList)
			{
				genericSelector.CheckboxToggle = true;
			}
			else if (!base.Attribute.DoubleClickToConfirm && !this.enableMultiSelect)
			{
				genericSelector.EnableSingleClickToSelect();
			}
			if (this.isList && this.enableMultiSelect)
			{
				genericSelector.SelectionTree.Selection.SupportsMultiSelect = true;
				genericSelector.DrawConfirmSelectionButton = true;
			}
			genericSelector.SelectionTree.Config.DrawSearchToolbar = drawSearchToolbar;
			IEnumerable<object> selection = Enumerable.Empty<object>();
			if (!this.isList)
			{
				selection = this.getSelection();
			}
			else if (this.enableMultiSelect)
			{
				selection = this.getSelection().SelectMany((object x) => (x as IEnumerable).Cast<object>());
			}
			genericSelector.SetSelection(selection);
			genericSelector.SelectionTree.EnumerateTree(false).AddThumbnailIcons(true);
			if (base.Attribute.ExpandAllMenuItems)
			{
				genericSelector.SelectionTree.EnumerateTree(delegate(OdinMenuItem x)
				{
					x.Toggled = true;
				});
			}
			if (base.Attribute.SortDropdownItems)
			{
				genericSelector.SelectionTree.SortMenuItemsByName(true);
			}
			return genericSelector;
		}

		// Token: 0x06001226 RID: 4646 RVA: 0x00051980 File Offset: 0x0004FB80
		private string GetCurrentValueName()
		{
			if (!EditorGUI.showMixedValue)
			{
				object weakSmartValue = base.Property.ValueEntry.WeakSmartValue;
				string name = null;
				if (this.nameLookup != null && weakSmartValue != null)
				{
					this.nameLookup.TryGetValue(weakSmartValue, out name);
				}
				return new GenericSelectorItem<object>(name, weakSmartValue).GetNiceName();
			}
			return "—";
		}

	}
}
