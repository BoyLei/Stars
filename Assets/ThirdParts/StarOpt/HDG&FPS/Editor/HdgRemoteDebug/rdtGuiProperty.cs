using GameDLL.Hdg;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameEditor.Hdg
{
    public class rdtGuiProperty
	{
		public class ValueChangedEvent
		{
			public ValueChangedEvent()
			{
				NewArraySize = -1;
				ArrayIndex = -1;
			}

			public ValueChangedEvent(int arrayIndex)
			{
				ArrayIndex = arrayIndex;
				NewArraySize = -1;
			}

			public ValueChangedEvent(object oldValue, object newValue, bool updateProperty, int arrayIndex)
			{
				OldValue = oldValue;
				NewValue = newValue;
				UpdateProperty = updateProperty;
				ArrayIndex = arrayIndex;
				NewArraySize = -1;
			}

			public rdtTcpMessageComponents.Component Component { get; set; }

			public Stack<rdtTcpMessageComponents.Property> Hierarchy { get; set; }

			public object OldValue { get; set; }

			public object NewValue { get; set; }

			public bool UpdateProperty { get; set; }

			public int ArrayIndex { get; set; }

			public int NewArraySize { get; set; }
		}

		private rdtExpandedCache m_expandedCache;

		private ComponentValueChangedHandler m_componentValueChangedHandler;

		private Stack<rdtTcpMessageComponents.Property> m_currentHierarchy;

		private rdtTcpMessageComponents.Component m_currentComponent;

		public delegate void ValueChangedHandler(ValueChangedEvent valueChangedEvent);

		public delegate void ComponentValueChangedHandler(ValueChangedEvent valueChangedEvent);

		private void DrawList(string label, IList list, ref bool foldout, ValueChangedHandler onValueChanged, string foldoutKey)
		{
			EditorGUILayout.BeginVertical();
			GUILayout.Label("");
			Rect rect = GUILayoutUtility.GetLastRect();
			foldout = EditorGUI.Foldout(rect, foldout, label, true);
			if (foldout)
			{
				EditorGUI.indentLevel++;
				int oldSize = (list != null) ? list.Count : 0;
				Type elementType = (list != null) ? list.GetType().GetListElementType() : null;
				bool isUserStruct = elementType == typeof(List<rdtTcpMessageComponents.Property>) || elementType == null;
				List<rdtTcpMessageComponents.Property> subProperties = list as List<rdtTcpMessageComponents.Property>;
				if (subProperties != null)
					DrawComponent(subProperties, foldoutKey, onValueChanged);
				else
				{
					EditorGUILayout.BeginHorizontal();
					Space();
					int newSize = EditorGUILayout.IntField("Size", oldSize);
					EditorGUILayout.EndHorizontal();
					if (oldSize != newSize)
					{
						ValueChangedEvent evt = new ValueChangedEvent
						{
							NewArraySize = newSize
						};
						onValueChanged(evt);
					}
					for (int i = 0; i < oldSize; i++)
					{
						int index = i;
						ValueChangedHandler handler = delegate(ValueChangedEvent valueChangedEvent)
						{
							if (isUserStruct)
							{
								valueChangedEvent.UpdateProperty = true;
								valueChangedEvent.ArrayIndex = index;
								onValueChanged(valueChangedEvent);
								return;
							}
							list[index] = valueChangedEvent.NewValue;
							valueChangedEvent.OldValue = null;
							valueChangedEvent.NewValue = null;
							valueChangedEvent.UpdateProperty = false;
							onValueChanged(valueChangedEvent);
						};
						Draw("Element " + i, list[i], handler, foldoutKey + ">Element" + i, false);
					}
				}
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndVertical();
		}

		private Matrix4x4 DrawMatrix(string label, Matrix4x4 matrix, ref bool foldout)
		{
			Matrix4x4 i = matrix;
			EditorGUILayout.BeginVertical();
			GUILayout.Label("");
			Rect rect = GUILayoutUtility.GetLastRect();
			foldout = EditorGUI.Foldout(rect, foldout, label, true);
			if (foldout)
			{
				EditorGUI.indentLevel++;
				i.m00 = DrawFloat("E00", i.m00);
				i.m01 = DrawFloat("E01", i.m01);
				i.m02 = DrawFloat("E02", i.m02);
				i.m03 = DrawFloat("E03", i.m03);
				i.m10 = DrawFloat("E10", i.m10);
				i.m11 = DrawFloat("E11", i.m11);
				i.m12 = DrawFloat("E12", i.m12);
				i.m13 = DrawFloat("E13", i.m13);
				i.m20 = DrawFloat("E20", i.m20);
				i.m21 = DrawFloat("E21", i.m21);
				i.m22 = DrawFloat("E22", i.m22);
				i.m23 = DrawFloat("E23", i.m23);
				i.m30 = DrawFloat("E30", i.m30);
				i.m31 = DrawFloat("E31", i.m31);
				i.m32 = DrawFloat("E32", i.m32);
				i.m33 = DrawFloat("E33", i.m33);
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndVertical();
			return i;
		}

		public rdtGuiProperty(ComponentValueChangedHandler componentValueChangedHandler)
		{
			m_expandedCache = new rdtExpandedCache();
			m_componentValueChangedHandler = componentValueChangedHandler;
		}

		public void DrawComponent(int gameObjInstanceId, rdtTcpMessageComponents.Component component, List<rdtTcpMessageComponents.Property> properties)
		{
			m_currentHierarchy = new Stack<rdtTcpMessageComponents.Property>();
			m_currentComponent = component;
			string foldoutKeyRoot = string.Format("{0}>{1}({2})", gameObjInstanceId.ToString(), component.m_name, component.m_instanceId);
			DrawComponent(properties, foldoutKeyRoot, null);
		}

		private void DrawComponent(List<rdtTcpMessageComponents.Property> properties, string foldoutKeyInit = "", ValueChangedHandler valueChangedHandler = null)
		{
			if (properties == null || properties.Count == 0)
				return;
			foreach (rdtTcpMessageComponents.Property property in properties)
			{
				object propValue = property.m_value;
				if (propValue != null || property.m_isArray)
				{
					EditorGUILayout.BeginHorizontal();
					string name = ObjectNames.NicifyVariableName(property.m_name);
					Type type = (propValue != null) ? propValue.GetType() : null;
					m_currentHierarchy.Push(property);
					string foldoutKey = property.m_name;
					if (!string.IsNullOrEmpty(foldoutKeyInit))
						foldoutKey = foldoutKeyInit + ">" + foldoutKey;
					if (type == typeof(List<rdtTcpMessageComponents.Property>))
					{
						EditorGUILayout.BeginVertical();
						GUILayout.Label("");
						Rect lastRect = GUILayoutUtility.GetLastRect();
						bool foldout = m_expandedCache.IsExpanded(m_currentComponent, foldoutKey);
						foldout = EditorGUI.Foldout(lastRect, foldout, name, true);
						if (foldout)
						{
							EditorGUI.indentLevel++;
							List<rdtTcpMessageComponents.Property> subProperties = (List<rdtTcpMessageComponents.Property>)propValue;
							DrawComponent(subProperties, foldoutKey, valueChangedHandler);
							EditorGUI.indentLevel--;
						}
						m_expandedCache.SetExpanded(foldout, m_currentComponent, foldoutKey);
						EditorGUILayout.EndVertical();
					}
					else
					{
						ValueChangedHandler onValueChanged = valueChangedHandler;
						onValueChanged = delegate(ValueChangedEvent valueChangedEvent)
						{
							valueChangedEvent.Component = m_currentComponent;
							valueChangedEvent.Hierarchy = m_currentHierarchy;
							m_componentValueChangedHandler(valueChangedEvent);
						};
						Draw(name, propValue, onValueChanged, foldoutKey, property.m_isArray);
					}
					m_currentHierarchy.Pop();
					EditorGUILayout.EndHorizontal();
				}
			}
		}

		private void Draw(string propName, object propValue, ValueChangedHandler onValueChanged, string foldoutKey, bool isArray = false)
		{
			bool foldout = m_expandedCache.IsExpanded(m_currentComponent, foldoutKey);
			if (propValue == null && !isArray)
				return;
			Type type = (propValue != null) ? propValue.GetType() : null;
			bool hasSpace = false;
			if (type != null && !type.IsArray && !type.IsGenericList() && type != typeof(Vector4) && type != typeof(Matrix4x4) && type != typeof(Quaternion))
			{
				hasSpace = true;
				EditorGUILayout.BeginHorizontal();
				Space();
			}
			if (type == typeof(float))
			{
				float oldValue = (float)propValue;
				float newValue = EditorGUILayout.FloatField(propName, oldValue);
				if (!oldValue.Equals(newValue))
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(double))
			{
				double oldValue = (double)propValue;
				double newValue = EditorGUILayout.DoubleField(propName, oldValue);
				if (!oldValue.Equals(newValue))
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(int))
			{
				int oldValue = (int)propValue;
				int newValue = EditorGUILayout.IntField(propName, oldValue);
				if (!oldValue.Equals(newValue))
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(uint))
			{
				int oldValue = (int)((uint)propValue);
				int newValue = EditorGUILayout.IntField(propName, oldValue);
				if (!oldValue.Equals(newValue))
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, (uint)newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(Vector2))
			{
				Vector2 oldValue = (Vector2)propValue;
				Vector2 newValue = EditorGUILayout.Vector2Field(propName, oldValue);
				if (!oldValue.Equals(newValue))
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(Vector2Int))
			{
				Vector2Int oldValue = (Vector2Int)propValue;
				Vector2Int newValue = EditorGUILayout.Vector2IntField(propName, oldValue);
				if (!oldValue.Equals(newValue))
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(Vector3))
			{
				Vector3 oldValue = (Vector3)propValue;
				Vector3 newValue = EditorGUILayout.Vector3Field(propName, oldValue);
				if (oldValue != newValue)
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(Vector3Int))
			{
				Vector3Int oldValue = (Vector3Int)propValue;
				Vector3Int newValue = EditorGUILayout.Vector3IntField(propName, oldValue);
				if (oldValue != newValue)
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(Vector4))
			{
				Vector4 oldValue = (Vector4)propValue;
				Vector4 newValue = DrawVector4(propName, oldValue, ref foldout);
				if (!oldValue.Equals(newValue))
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(Matrix4x4))
			{
				Matrix4x4 oldValue = (Matrix4x4)propValue;
				Matrix4x4 newValue = DrawMatrix(propName, oldValue, ref foldout);
				if (!oldValue.Equals(newValue))
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(bool))
			{
				bool oldValue = (bool)propValue;
				bool newValue = EditorGUILayout.Toggle(propName, oldValue);
				if (!oldValue.Equals(newValue))
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type != null && type.IsEnum)
			{
				Enum oldValue = (Enum)propValue;
				Enum newValue = EditorGUILayout.EnumPopup(propName, oldValue);
				if (!oldValue.Equals(newValue))
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(char))
			{
				string oldValue = new string((char)propValue, 1);
				string newValue = EditorGUILayout.TextField(propName, oldValue);
				if (!oldValue.Equals(newValue))
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, (newValue.Length > 0) ? newValue[0] : ((char)propValue), true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(string))
			{
				string oldValue = (string)propValue;
				string newValue = EditorGUILayout.TextField(propName, oldValue);
				if (!oldValue.Equals(newValue))
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(Color))
			{
				Color oldValue = (Color)propValue;
				Color newValue = EditorGUILayout.ColorField(propName, oldValue);
				if (!oldValue.Equals(newValue))
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(Color32))
			{
				Color32 oldValue = (Color32)propValue;
				Color32 newValue = EditorGUILayout.ColorField(propName, oldValue);
				if (!oldValue.Equals(newValue))
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(Quaternion))
			{
				Quaternion oldValue = (Quaternion)propValue;
				Vector4 v = new Vector4(oldValue.x, oldValue.y, oldValue.z, oldValue.w);
				v = DrawVector4(propName, v, ref foldout);
				Quaternion newValue = new Quaternion(v.x, v.y, v.z, v.w);
				if (!oldValue.Equals(newValue))
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(Bounds))
			{
				Bounds oldValue = (Bounds)propValue;
				Bounds newValue = EditorGUILayout.BoundsField(propName, oldValue);
				if (!oldValue.Equals(newValue))
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(BoundsInt))
			{
				BoundsInt oldValue = (BoundsInt)propValue;
				BoundsInt newValue = EditorGUILayout.BoundsIntField(propName, oldValue);
				if (!oldValue.Equals(newValue))
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(Rect))
			{
				Rect oldValue = (Rect)propValue;
				Rect newValue = EditorGUILayout.RectField(propName, oldValue);
				if (!oldValue.Equals(newValue))
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(RectInt))
			{
				RectInt oldValue = (RectInt)propValue;
				RectInt newValue = EditorGUILayout.RectIntField(propName, oldValue);
				if (!oldValue.Equals(newValue))
				{
					ValueChangedEvent evt = new ValueChangedEvent(oldValue, newValue, true, -1);
					onValueChanged(evt);
				}
			}
			else if ((type == null && isArray) || type.IsArray)
			{
				Array a = (Array)propValue;
				DrawList(propName, a, ref foldout, onValueChanged, foldoutKey);
			}
			else if (type != null && type.IsGenericList())
			{
				IList list = (IList)propValue;
				DrawList(propName, list, ref foldout, onValueChanged, foldoutKey);
			}
			else if (type == typeof(rdtSerializerButton))
			{
				Vector2 size = GUI.skin.button.CalcSize(new GUIContent(propName));
				if (GUILayout.Button(propName, GUILayout.Width(size.x + 40f)))
				{
					rdtSerializerButton button = new rdtSerializerButton(true);
					ValueChangedEvent evt = new ValueChangedEvent(false, button, true, -1);
					onValueChanged(evt);
				}
			}
			else if (type == typeof(rdtSerializerSlider))
			{
				rdtSerializerSlider slider = (rdtSerializerSlider)propValue;
				float newValue = EditorGUILayout.Slider(propName, slider.Value, slider.LimitMin, slider.LimitMax);
				if (!slider.Value.Equals(newValue))
				{
					rdtSerializerSlider newSlider = new rdtSerializerSlider(newValue, slider.LimitMin, slider.LimitMax);
					ValueChangedEvent evt = new ValueChangedEvent(slider, newSlider, true, -1);
					onValueChanged(evt);
				}
			}
			else
			{
				string typeName = (type != null) ? type.Name : "<null>";
				rdtDebug.Debug(string.Concat(new object[]
				{
					"rdtGuiProperty: Unknown type: ",
					typeName,
					" (name=",
					propName,
					", value=",
					propValue,
					")"
				}));
			}
			if (hasSpace)
				EditorGUILayout.EndHorizontal();
			m_expandedCache.SetExpanded(foldout, m_currentComponent, foldoutKey);
		}

		private void Space()
		{
			GUILayout.Space(EditorStyles.foldout.padding.left + EditorStyles.foldout.margin.left - EditorStyles.label.padding.left);
		}

		private float DrawFloat(string label, float value)
		{
			EditorGUILayout.BeginHorizontal();
			Space();
			value = EditorGUILayout.FloatField(label, value);
			EditorGUILayout.EndHorizontal();
			return value;
		}

		private Vector4 DrawVector4(string label, Vector4 value, ref bool foldout)
		{
			EditorGUILayout.BeginVertical();
			GUILayout.Label("");
			Rect rect = GUILayoutUtility.GetLastRect();
			foldout = EditorGUI.Foldout(rect, foldout, label, true);
			if (foldout)
			{
				EditorGUI.indentLevel++;
				value.x = DrawFloat("X", value.x);
				value.y = DrawFloat("Y", value.y);
				value.z = DrawFloat("Z", value.z);
				value.w = DrawFloat("W", value.w);
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndVertical();
			return value;
		}
	}
}
