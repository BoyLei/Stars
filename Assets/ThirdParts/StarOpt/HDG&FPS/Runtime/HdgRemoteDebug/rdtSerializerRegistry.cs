using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameDLL.Hdg
{
	public class rdtSerializerRegistry
	{
		private delegate object ConvertObjectDelegate(object objIn, rdtSerializerRegistry registry);

		private Dictionary<Type, ConvertObjectDelegate> m_converters = new Dictionary<Type, ConvertObjectDelegate>();

		private HashSet<Type> m_failures = new HashSet<Type>();

		//private HashSet<Type> m_referenceFailures = new HashSet<Type>();

		private HashSet<Type> m_unknownPrimitives = new HashSet<Type>();

		private HashSet<string> m_skipProperties = new HashSet<string>();

		private HashSet<string> m_skipTypes = new HashSet<string>();

		private Dictionary<string, HashSet<string>> m_skipPropertiesPerType = new Dictionary<string, HashSet<string>>();

		private Dictionary<string, HashSet<string>> m_includePropertiesPerType = new Dictionary<string, HashSet<string>>();

		private HashSet<string> m_dontReadProperties = new HashSet<string>();

		private object NotHandledConversion(object objIn, rdtSerializerRegistry r)
		{
			return null;
		}

		public rdtSerializerRegistry()
		{
			m_converters.Add(typeof(Vector2), (object objIn, rdtSerializerRegistry r) => new rdtSerializerVector2((Vector2)objIn));
			m_converters.Add(typeof(Vector2Int), (object objIn, rdtSerializerRegistry r) => new rdtSerializerVector2Int((Vector2Int)objIn));
			m_converters.Add(typeof(Vector3), (object objIn, rdtSerializerRegistry r) => new rdtSerializerVector3((Vector3)objIn));
			m_converters.Add(typeof(Vector3Int), (object objIn, rdtSerializerRegistry r) => new rdtSerializerVector3Int((Vector3Int)objIn));
			m_converters.Add(typeof(Vector4), (object objIn, rdtSerializerRegistry r) => new rdtSerializerVector4((Vector4)objIn));
			m_converters.Add(typeof(Quaternion), (object objIn, rdtSerializerRegistry r) => new rdtSerializerQuaternion((Quaternion)objIn));
			m_converters.Add(typeof(Color), (object objIn, rdtSerializerRegistry r) => new rdtSerializerColor((Color)objIn));
			m_converters.Add(typeof(Color32), (object objIn, rdtSerializerRegistry r) => new rdtSerializerColor32((Color32)objIn));
			m_converters.Add(typeof(Rect), (object objIn, rdtSerializerRegistry r) => new rdtSerializerRect((Rect)objIn));
			m_converters.Add(typeof(RectInt), (object objIn, rdtSerializerRegistry r) => new rdtSerializerRectInt((RectInt)objIn));
			m_converters.Add(typeof(Bounds), (object objIn, rdtSerializerRegistry r) => new rdtSerializerBounds((Bounds)objIn));
			m_converters.Add(typeof(BoundsInt), (object objIn, rdtSerializerRegistry r) => new rdtSerializerBoundsInt((BoundsInt)objIn));
			m_converters.Add(typeof(Matrix4x4), (object objIn, rdtSerializerRegistry r) => new rdtSerializerMatrix4x4((Matrix4x4)objIn));
			m_converters.Add(typeof(List<>), rdtSerializerContainerArray.Serialize);
			m_converters.Add(typeof(Dictionary<, >), NotHandledConversion);
			m_converters.Add(typeof(Array), rdtSerializerContainerArray.Serialize);
			InitSkipProperties();
		}

		public void AddUnknownPrimitive(Type type)
		{
			if (!m_unknownPrimitives.Contains(type))
			{
				rdtDebug.Warning("Remote Debug: Tried to serialise an unknown primitive type '{0}' ({1})", type, type.FullName);
				m_unknownPrimitives.Add(type);
			}
		}

		public object Serialize(object obj)
		{
			if (obj == null || obj.Equals(null))
				return null;
			object serializedObj = obj;
			Type typeToConvert = obj.GetType();
			if (typeof(rdtSerializerInterface).IsAssignableFrom(typeToConvert))
				return serializedObj;
			if (typeToConvert.IsArray)
				typeToConvert = typeof(Array);
			else if (typeToConvert.IsGenericType)
				typeToConvert = typeToConvert.GetGenericTypeDefinition();
			ConvertObjectDelegate converter;
			if (m_converters.TryGetValue(typeToConvert, out converter))
				serializedObj = converter(obj, this);
			else if (typeToConvert.IsUserStruct() || typeToConvert.IsReference())
				serializedObj = ReadAllFields(obj);
			if (serializedObj != null)
			{
				Type serialisedObjType = serializedObj.GetType();
				if (!serialisedObjType.IsSerializable && !typeof(rdtSerializerInterface).IsAssignableFrom(serialisedObjType))
				{
					if (!m_failures.Contains(serialisedObjType))
					{
						rdtDebug.Warning("Remote Debug: Object '{0}' (type {1}) is not serializable!", serializedObj, serialisedObjType.Name);
						m_failures.Add(serialisedObjType);
					}
					return null;
				}
			}
			return serializedObj;
		}

		public object Deserialize(object obj)
		{
			if (obj == null || obj.Equals(null))
				return null;
			object deserializedObj = obj;
			rdtSerializerInterface serializer = obj as rdtSerializerInterface;
			if (serializer != null)
				deserializedObj = serializer.Deserialize(this);
			List<rdtTcpMessageComponents.Property> subProperties = deserializedObj as List<rdtTcpMessageComponents.Property>;
			if (subProperties != null)
			{
				for (int i = 0; i < subProperties.Count; i++)
				{
					rdtTcpMessageComponents.Property p = subProperties[i];
					p.Deserialise(this);
					subProperties[i] = p;
				}
			}
			return deserializedObj;
		}

		public void AddField(List<rdtTcpMessageComponents.Property> allFields, string name, object value, rdtTcpMessageComponents.Property.Type type, RangeAttribute rangeAttribute, bool isArrayOrList)
		{
			object serializedValue;
			if (rangeAttribute != null && value is float)
				serializedValue = new rdtSerializerSlider((float)value, rangeAttribute.min, rangeAttribute.max);
			else
			{
				serializedValue = Serialize(value);
				if (value != null && (serializedValue == null || serializedValue.Equals(null)))
					return;
			}
			allFields.Add(new rdtTcpMessageComponents.Property
			{
				m_isArray = (serializedValue is rdtSerializerContainerArray || isArrayOrList),
				m_name = name,
				m_value = serializedValue,
				m_type = type
			});
		}

		private bool CanAddMember(object owner, MemberInfo memberInfo, Type memberType)
		{
			bool flag = memberInfo.IsDefined(typeof(ObsoleteAttribute), false);
			bool hide = memberInfo.IsDefined(typeof(HideInInspector), false);
			bool isMonoBehaviour = memberType.IsSubclassOf(typeof(Component));
			return !flag && !hide && !isMonoBehaviour;
		}

		public List<rdtTcpMessageComponents.Property> ReadAllFields(object owner)
		{
			Type ownerType = owner.GetType();
			string ownerTypeName = ownerType.Name;
			if (m_skipTypes.Contains(ownerTypeName))
				return null;
			List<rdtTcpMessageComponents.Property> allFields = new List<rdtTcpMessageComponents.Property>();
			PropertyInfo[] props = ownerType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			if (!(owner is MonoBehaviour) && !m_dontReadProperties.Contains(ownerTypeName))
			{
				foreach (PropertyInfo p in props)
				{
					if (p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0 /*&& !p.PropertyType.IsEnum*/)
					{
						string pName = p.Name;
						bool checkSkip = !HasIncludePerType(ownerTypeName);
						if ((checkSkip || IncludeMember(ownerTypeName, pName)) && (!checkSkip || !SkipMember(ownerTypeName, pName)))
						{
							MethodInfo getMethod = p.GetGetMethod();
							if (getMethod != null && getMethod.IsPublic)
							{
								MethodInfo setMethod = p.GetSetMethod();
								if (setMethod != null && setMethod.IsPublic && CanAddMember(owner, p, p.PropertyType))
								{
									RangeAttribute range = null;
									object value = p.GetValue(owner, null);
									Type propType = p.PropertyType;
									bool isArrayOrList = propType.IsGenericList() || propType.IsArray;
									if (isArrayOrList)
									{
										Type elementType = propType.GetListElementType();
										if (elementType.IsSubclassOf(typeof(Component)) || (!elementType.IsSerializable && !elementType.IsUserStruct()))
										{
											//goto IL_181;
											break;
										}
									}
									AddField(allFields, pName, value, rdtTcpMessageComponents.Property.Type.Property, range, isArrayOrList);
								}
							}
						}
					}
					//IL_181:;
				}
			}
			foreach (FieldInfo f in ownerType.GetAllFields().ToArray())
			{
				bool serialize = f.IsDefined(typeof(SerializeField), false);
				RangeAttribute range2 = null;
				if ((f.IsPublic || serialize) /*&& !f.FieldType.IsEnum*/)
				{
					string fName = f.Name;
					bool checkSkip = !HasIncludePerType(ownerTypeName);
					if ((checkSkip || IncludeMember(ownerTypeName, fName)) && (!checkSkip || !SkipMember(ownerTypeName, fName)) && CanAddMember(owner, f, f.FieldType))
					{
						object value2 = f.GetValue(owner);
						Type fieldType = f.FieldType;
						bool isArrayOrList2 = fieldType.IsGenericList() || fieldType.IsArray;
						if (isArrayOrList2)
						{
							Type elementType2 = fieldType.GetListElementType();
							if (elementType2.IsSubclassOf(typeof(Component)) || (!elementType2.IsSerializable && !elementType2.IsUserStruct()))
							{
								//goto IL_297;
								continue;
							}
						}
						AddField(allFields, fName, value2, rdtTcpMessageComponents.Property.Type.Field, range2, isArrayOrList2);
					}
				}
				//IL_297:;
			}
			foreach (MethodInfo l in ownerType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
			{
				if (l.IsDefined(typeof(RemoteDebugActionAttribute), false))
				{
					allFields.Add(new rdtTcpMessageComponents.Property
					{
						m_name = l.Name,
						m_value = new rdtSerializerButton(false),
						m_type = rdtTcpMessageComponents.Property.Type.Method
					});
				}
			}
			return allFields;
		}

		private object MakeNewList(IList oldValue, Type listType, int arraySize)
		{
			Type elementType = listType.GetListElementType();
			object newValue;
			if (oldValue == null)
			{
				IList list;
				if (listType.IsArray)
					list = Array.CreateInstance(elementType, arraySize);
				else
				{
					list = (IList)typeof(List<>).MakeGenericType(new Type[]
					{
						elementType
					}).GetConstructor(Type.EmptyTypes).Invoke(null);
					for (int i = 0; i < arraySize; i++)
						list.Add(null);
				}
				for (int j = 0; j < arraySize; j++)
				{
					object dummyValue = Activator.CreateInstance(elementType);
					list[j] = dummyValue;
				}
				newValue = list;
			}
			else if (listType.IsArray)
			{
				Array oldArray = oldValue as Array;
				Array newArray = Array.CreateInstance(elementType, arraySize);
				Array.Copy(oldArray, newArray, Mathf.Min(arraySize, oldArray.Length));
				for (int k = oldArray.Length; k < arraySize; k++)
				{
					object dummyValue2 = Activator.CreateInstance(elementType);
					newArray.SetValue(dummyValue2, k);
				}
				newValue = newArray;
			}
			else
			{
				if (arraySize < oldValue.Count)
				{
					int diff = oldValue.Count - arraySize;
					for (int l = 0; l < diff; l++)
						oldValue.RemoveAt(oldValue.Count - 1);
				}
				else if (arraySize > oldValue.Count)
				{
					int diff2 = arraySize - oldValue.Count;
					for (int m = 0; m < diff2; m++)
					{
						object dummyValue3 = Activator.CreateInstance(elementType);
						oldValue.Add(dummyValue3);
					}
				}
				newValue = oldValue;
			}
			return newValue;
		}

		public void SetArraySize(object owner, List<rdtTcpMessageComponents.Property> allFields, int arraySize)
		{
			if (arraySize < 0)
				return;
			List<rdtTcpMessageComponents.Property> fields = allFields;
			rdtTcpMessageComponents.Property p = fields[0];
			Type type = owner.GetType();
			if (!p.m_isArray)
			{
				fields = (p.m_value as List<rdtTcpMessageComponents.Property>);
				if (fields == null)
				{
					rdtDebug.Error(this, "Expected to find a list of properties at {0}, but found {1} while trying to set array size", p.m_name, (p.m_value != null) ? p.m_value.GetType().Name : "<null>");
					return;
				}
			}
			if (p.m_type == rdtTcpMessageComponents.Property.Type.Property)
			{
				PropertyInfo prop = type.GetProperty(p.m_name);
				if (prop == null)
					return;
				if (p.m_isArray)
				{
					Type propType = prop.PropertyType;
					IList oldValue = prop.GetValue(owner, null) as IList;
					object newValue = MakeNewList(oldValue, propType, arraySize);
					prop.SetValue(owner, newValue, null);
					return;
				}
				object child = prop.GetValue(owner, null);
				SetArraySize(child, fields, arraySize);
				prop.SetValue(owner, child, null);
				return;
			}
			else
			{
				if (p.m_type != rdtTcpMessageComponents.Property.Type.Field)
				{
					rdtDebug.Error(this, "Unexpected property type {0} when setting array size on {1}", p.m_type.ToString(), p.m_name);
					return;
				}
				FieldInfo field = type.GetField(p.m_name);
				if (field == null)
					return;
				if (p.m_isArray)
				{
					Type fieldType = field.FieldType;
					IList oldValue2 = field.GetValue(owner) as IList;
					object newValue2 = MakeNewList(oldValue2, fieldType, arraySize);
					field.SetValue(owner, newValue2);
					return;
				}
				object child2 = field.GetValue(owner);
				SetArraySize(child2, fields, arraySize);
				field.SetValue(owner, child2);
				return;
			}
		}

		public void WriteAllFields(object realOwner, List<rdtTcpMessageComponents.Property> allFields, int arrayIndex = -1)
		{
			bool isArrayOrList = false;
			object owner = realOwner;
			Type ownerType = owner.GetType();
			if ((ownerType.IsArray || ownerType.IsGenericList()) && arrayIndex != -1)
			{
				isArrayOrList = true;
				owner = ((IList)owner)[arrayIndex];
			}
			rdtDebug.Debug(this, "WriteAllFields");
			ownerType = owner.GetType();
			int i = 0;
			while (i < allFields.Count)
			{
				rdtTcpMessageComponents.Property property = allFields[i];
				object packedValue = property.m_value;
				object deserializedValue = Deserialize(packedValue);
				if (deserializedValue is rdtSerializerSlider)
					deserializedValue = ((rdtSerializerSlider)deserializedValue).Value;
				
				if (property.m_type == rdtTcpMessageComponents.Property.Type.Property)
                {
					PropertyInfo prop = ownerType.GetProperty(property.m_name);
					if (prop != null)
					{
						List<rdtTcpMessageComponents.Property> subProperties = property.m_value as List<rdtTcpMessageComponents.Property>;
						if (subProperties == null)
						{
							try
							{
								rdtDebug.Debug(this, "Setting property {0} to {1}", property.m_name, deserializedValue.ToString());
								prop.SetValue(owner, deserializedValue, null);
							}
							catch (Exception ex)
							{
								rdtDebug.Warning(this, "Property '{0}' could not be set: {1}!", property.m_name, ex.Message);
							}
						}
						else
						{
							object oldValue = prop.GetValue(owner, null);
							WriteAllFields(oldValue, subProperties, arrayIndex);
							prop.SetValue(owner, oldValue, null);
						}
					}
				}
				else if (property.m_type == rdtTcpMessageComponents.Property.Type.Field)
				{
					FieldInfo field = ownerType.GetFieldInHierarchy(property.m_name);
					if (field != null)
					{
						List<rdtTcpMessageComponents.Property> subProperties2 = property.m_value as List<rdtTcpMessageComponents.Property>;
						if (subProperties2 == null)
						{
							try
							{
								rdtDebug.Debug(this, "Setting field {0} to {1}", property.m_name, deserializedValue.ToString());
								field.SetValue(owner, deserializedValue);
							}
							catch (ArgumentException argException)
							{
								rdtDebug.Error(this, "'{0}' could not be assigned: {1}!", property.m_name, argException.Message);
							}
						}
						else
						{
							object oldValue2 = field.GetValue(owner);
							WriteAllFields(oldValue2, subProperties2, arrayIndex);
							field.SetValue(owner, oldValue2);
						}
					}
				}
				else
                {
					MethodInfo method = ownerType.GetMethod(property.m_name);
					if (method != null)
					{
						try
						{
							if (((rdtSerializerButton)property.m_value).Pressed)
								method.Invoke(owner, null);
						}
						catch (Exception ex2)
						{
							string msg = (ex2.InnerException != null) ? ex2.InnerException.Message : ex2.Message;
							string callstack = (ex2.InnerException != null) ? ex2.InnerException.StackTrace : ex2.StackTrace;
							object[] args = new object[]
							{
								property.m_name,
								msg
							};
							rdtDebug.Error("RemoteDebugServer: Method '{0}' failed: {1}", args);
							rdtDebug.Error(callstack);
						}
					}
				}
				i++;
			}
			if (isArrayOrList)
				((IList)realOwner)[arrayIndex] = owner;
		}

		private void InitSkipProperties()
		{
			m_skipTypes.Add("ParticleSystemRenderer");
			m_skipProperties.Add("hideFlags");
			m_skipProperties.Add("useGUILayout");
			m_skipProperties.Add("tag");
			m_skipProperties.Add("name");
			m_skipProperties.Add("enabled");
			m_skipProperties.Add("m_CachedPtr");
			m_skipProperties.Add("m_InstanceID");
			AddSkipForType("Rigidbody2D", new string[]
			{
				"position",
				"rotation",
				"freezeRotation"
			});
			string[] skipForRigidBody3D = new string[]
			{
				"position",
				"rotation",
				"freezeRotation",
				"useConeFriction"
			};
			AddSkipForType("Rigidbody", skipForRigidBody3D);
			string[] skipForCollider = new string[]
			{
				"material",
				"sharedMaterial",
				"density",
				"sharedMesh"
			};
			AddSkipForType("BoxCollider", skipForCollider);
			AddSkipForType("BoxCollider2D", skipForCollider);
			AddSkipForType("CircleCollider2D", skipForCollider);
			AddSkipForType("SphereCollider", skipForCollider);
			AddSkipForType("PolygonCollider2D", skipForCollider);
			AddSkipForType("MeshCollider", skipForCollider);
			AddSkipForType("CapsuleCollider", skipForCollider);
			AddSkipForType("EdgeCollider2D", skipForCollider);
			AddSkipForType("WheelCollider", skipForCollider);
			AddSkipForType("TerrainCollider", skipForCollider);
			AddSkipForType("TerrainCollider", new string[]
			{
				"isTrigger",
				"terrainData"
			});
			AddSkipForType("CharacterController", skipForCollider);
			AddSkipForType("CharacterController", new string[]
			{
				"isTrigger",
				"contactOffset"
			});
			string[] skipForCloth = new string[]
			{
				"capsuleColliders",
				"sphereColliders",
				"solverFrequency",
				"useContinuousCollision",
				"useVirtualParticles"
			};
			AddSkipForType("Cloth", skipForCloth);
			string[] skipForJoint2D = new string[]
			{
				"breakForce",
				"breakTorque",
				"connectedBody"
			};
			AddSkipForType("HingeJoint2D", skipForJoint2D);
			AddSkipForType("FixedJoint2D", skipForJoint2D);
			AddSkipForType("SpringJoint2D", skipForJoint2D);
			AddSkipForType("DistanceJoint2D", skipForJoint2D);
			AddSkipForType("FrictionJoint2D", skipForJoint2D);
			AddSkipForType("RelativeJoint2D", skipForJoint2D);
			AddSkipForType("SliderJoint2D", skipForJoint2D);
			AddSkipForType("WheelJoint2D", skipForJoint2D);
			AddSkipForType("TargetJoint2D", new string[]
			{
				"enableCollision"
			});
			AddSkipForType("TargetJoint2D", skipForJoint2D);
			string[] skipForJoint = new string[]
			{
				"connectedBody"
			};
			AddSkipForType("CharacterJoint", skipForJoint);
			AddSkipForType("ConfigurableJoint", skipForJoint);
			AddSkipForType("FixedJoint", skipForJoint);
			AddSkipForType("HingeJoint", skipForJoint);
			AddSkipForType("SpringJoint", skipForJoint);
			AddSkipForType("ReflectionProbe", new string[]
			{
				"bakedTexture",
				"customBakedTexture"
			});
			AddSkipForType("Skybox", new string[]
			{
				"material"
			});
			AddSkipForType("NavMeshAgent", new string[]
			{
				"velocity",
				"nextPosition"
			});
			AddSkipForType("AudioSource", new string[]
			{
				"clip",
				"outputAudioMixerGroup"
			});
			AddSkipForType("AudioLowPassFilter", new string[]
			{
				"customCutoffCurve"
			});
			AddSkipForType("AudioReverbZone", new string[]
			{
				"reverbDelay",
				"reflectionsDelay"
			});
			AddSkipForType("LensFlare", new string[]
			{
				"flare"
			});
			AddSkipForType("Projector", new string[]
			{
				"material"
			});
			AddSkipForType("EventSystem", new string[]
			{
				"m_FirstSelected",
				"firstSelectedGameObject"
			});
			AddSkipForType("EventTrigger", new string[]
			{
				"m_Delegates"
			});
			AddSkipForType("Canvas", new string[]
			{
				"worldCamera"
			});
			AddSkipForType("TouchInputModule", new string[]
			{
				"forceModuleActive"
			});
			AddSkipForType("Light", new string[]
			{
				"flare",
				"cookie"
			});
			string[] skipForLayoutGroups = new string[]
			{
				"m_Padding",
				"padding"
			};
			AddSkipForType("GridLayoutGroup", skipForLayoutGroups);
			AddSkipForType("HorizontalLayoutGroup", skipForLayoutGroups);
			AddSkipForType("VerticalLayoutGroup", skipForLayoutGroups);
			AddSkipForType("TextMesh", new string[]
			{
				"font"
			});
			AddSkipForType("Animation", new string[]
			{
				"clip"
			});
			AddSkipForType("Animator", new string[]
			{
				"runtimeAnimatorController",
				"avatar",
				"bodyPosition",
				"bodyRotation",
				"playbackTime"
			});
			AddSkipForType("NetworkView", new string[]
			{
				"observed",
				"viewID"
			});
			AddSkipForType("Terrain", new string[]
			{
				"terrainData",
				"materialTemplate"
			});
			AddSkipForType("NavMeshAgent", new string[]
			{
				"path"
			});
			AddSkipForType("OffMeshLink", new string[]
			{
				"startTransform",
				"endTransform"
			});
			string[] skipForNetworkManager = new string[]
			{
				"m_SpawnPrefabs",
				"m_ConnectionConfig",
				"m_GlobalConfig",
				"m_Channels",
				"m_PlayerPrefab",
				"client",
				"matchInfo",
				"matchMaker",
				"matches"
			};
			AddSkipForType("NetworkManager", skipForNetworkManager);
			AddSkipForType("NetworkLobbyManager", skipForNetworkManager);
			AddSkipForType("NetworkLobbyManager", new string[]
			{
				"m_LobbyPlayerPrefab",
				"m_GamePlayerPrefab"
			});
			AddSkipForType("NetworkTransform", new string[]
			{
				"m_ClientMoveCallback3D",
				"m_ClientMoveCallback2D"
			});
			AddSkipForType("NetworkTransformVisualizer", new string[]
			{
				"m_VisualizerPrefab"
			});
			AddSkipForType("GUIText", new string[]
			{
				"material",
				"font"
			});
			AddSkipForType("GUITexture", new string[]
			{
				"texture",
				"border"
			});
			string[] skipForUIBehaviour = new string[]
			{
				"m_OnClick",
				"m_TargetGraphic",
				"m_AnimationTriggers",
				"m_SpriteState",
				"m_OnCullStateChanged",
				"m_Template",
				"m_CaptionText",
				"m_CaptionImage",
				"m_Options",
				"m_OnValueChanged",
				"m_ItemText",
				"m_ItemImage",
				"m_Sprite",
				"m_Material",
				"m_TextComponent",
				"m_Placeholder",
				"m_OnEndEdit",
				"m_OnValidateInput",
				"m_Texture",
				"m_HandleRect",
				"m_FontData",
				"m_Group",
				"m_AsteriskChar",
				"m_FillRect",
				"onValueChanged",
				"graphic",
				"m_HorizontalScrollbar",
				"m_Content",
				"m_VerticalScrollbar",
				"m_Viewport"
			};
			AddSkipForType("Navigation", new string[]
			{
				"m_SelectOnUp",
				"m_SelectOnDown",
				"m_SelectOnLeft",
				"m_SelectOnRight"
			});
			AddSkipForType("Button", skipForUIBehaviour);
			AddSkipForType("Dropdown", skipForUIBehaviour);
			AddSkipForType("Image", skipForUIBehaviour);
			AddSkipForType("InputField", skipForUIBehaviour);
			AddSkipForType("RawImage", skipForUIBehaviour);
			AddSkipForType("Scrollbar", skipForUIBehaviour);
			AddSkipForType("ScrollRect", skipForUIBehaviour);
			AddSkipForType("Selectable", skipForUIBehaviour);
			AddSkipForType("Slider", skipForUIBehaviour);
			AddSkipForType("Text", skipForUIBehaviour);
			AddSkipForType("Toggle", skipForUIBehaviour);
			AddDontReadProperties("ColorBlock");
			AddDontReadProperties("Navigation");
			string[] includeForTransform = new string[]
			{
				"localPosition",
				"localEulerAngles",
				"localScale"
			};
			AddIncludeForType("Transform", includeForTransform);
			string[] includeForRectTransform = new string[]
			{
				"anchoredPosition",
				"anchorMax",
				"anchorMin",
				"offsetMax",
				"offsetMin",
				"pivot"
			};
			AddIncludeForType("RectTransform", includeForTransform);
			AddIncludeForType("RectTransform", includeForRectTransform);
			string[] includeForRenderer = new string[]
			{
				"shadowCastingMode",
				"receiveShadows",
				"useLightProbes",
				"reflectionProbeUsage"
			};
			AddIncludeForType("MeshRenderer", includeForRenderer);
			AddIncludeForType("SpriteRenderer", includeForRenderer);
			AddIncludeForType("SpriteRenderer", new string[]
			{
				"color",
				"flipX",
				"flipY"
			});
			string[] includeForParticleSystemRenderer = new string[]
			{
				"alignment",
				"cameraVelocityScale",
				"lengthScale",
				"maxParticleSize",
				"minParticleSize",
				"normalDirection",
				"pivot",
				"renderMode",
				"sortingFudge",
				"sortMode",
				"velocityScale"
			};
			AddIncludeForType("ParticleSystemRenderer", includeForRenderer);
			AddIncludeForType("ParticleSystemRenderer", includeForParticleSystemRenderer);
			AddIncludeForType("TrailRenderer", includeForRenderer);
			AddIncludeForType("TrailRenderer", new string[]
			{
				"autodestruct",
				"endWidth",
				"startWidth",
				"time"
			});
			AddIncludeForType("SkinnedMeshRenderer", includeForRenderer);
			AddIncludeForType("SkinnedMeshRenderer", new string[]
			{
				"quality",
				"updateWhenOffscreen",
				"localBounds"
			});
			AddIncludeForType("LineRenderer", includeForRenderer);
			AddIncludeForType("LineRenderer", new string[]
			{
				"useWorldSpace"
			});
			AddIncludeForType("BillboardRenderer", includeForRenderer);
			string[] includeForCamera = new string[]
			{
				"clearFlags",
				"backgroundColor",
				"cullingMask",
				"orthographic",
				"orthographicSize",
				"fov",
				"nearClipPlane",
				"farClipPlane",
				"rect",
				"depth",
				"renderingPath",
				"useOcclusionCulling",
				"hdr",
				"targetDisplay"
			};
			AddIncludeForType("Camera", includeForCamera);
			AddIncludeForType("MeshFilter");
			AddIncludeForType("NetworkAnimator");
			AddIncludeForType("NetworkIdentity", new string[]
			{
				"m_ServerOnly",
				"m_LocalPlayerAuthority"
			});
			AddIncludeForType("CanvasRenderer");
		}

		private void AddDontReadProperties(string typeName)
		{
			m_dontReadProperties.Add(typeName);
		}

		private void AddSkipForType(string typeName, params string[] properties)
		{
			HashSet<string> set;
			if (!m_skipPropertiesPerType.TryGetValue(typeName, out set))
			{
				set = new HashSet<string>();
				m_skipPropertiesPerType.Add(typeName, set);
			}
			set.UnionWith(properties);
		}

		private void AddIncludeForType(string typeName, params string[] properties)
		{
			HashSet<string> set;
			if (!m_includePropertiesPerType.TryGetValue(typeName, out set))
			{
				set = new HashSet<string>();
				m_includePropertiesPerType.Add(typeName, set);
			}
			set.UnionWith(properties);
		}

		private bool HasIncludePerType(string ownerTypeName)
		{
			return m_includePropertiesPerType.ContainsKey(ownerTypeName);
		}

		private bool IncludeMember(string ownerTypeName, string memberInfoName)
		{
			HashSet<string> includePerType = null;
			return !m_includePropertiesPerType.TryGetValue(ownerTypeName, out includePerType) || includePerType.Contains(memberInfoName);
		}

		private bool SkipMember(string ownerTypeName, string memberInfoName)
		{
			if (m_skipProperties.Contains(memberInfoName))
				return true;
			HashSet<string> skipPerType = null;
			return m_skipPropertiesPerType.TryGetValue(ownerTypeName, out skipPerType) && skipPerType.Contains(memberInfoName);
		}
	}
}
