using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GameDLL.Hdg
{
    public class rdtMessageGameObjectsHandler
	{
		private RemoteDebugServer m_server;

		private bool m_dontDestroyOnLoadBadObject;

		private List<GameObject> m_gameObjects;

        private List<Scene> m_scenes;

        private List<rdtTcpMessageGameObjects.Gob> m_allGobs;

		private List<rdtTcpMessageComponents.Component> m_components;

		private List<Component> m_unityComponents;

		public rdtMessageGameObjectsHandler(RemoteDebugServer server)
		{
			m_scenes = new List<Scene>(8);
			m_gameObjects = new List<GameObject>(2048);
			m_allGobs = new List<rdtTcpMessageGameObjects.Gob>(2048);
			m_components = new List<rdtTcpMessageComponents.Component>(16);
			m_unityComponents = new List<Component>(16);
			m_server = server;
			m_server.AddCallback(typeof(rdtTcpMessageGetGameObjects), OnRequestGameObjects);
			m_server.AddCallback(typeof(rdtTcpMessageGetComponents), OnRequestGameObjectComponents);
			m_server.AddCallback(typeof(rdtTcpMessageUpdateComponentProperties), OnUpdateComponentProperties);
			m_server.AddCallback(typeof(rdtTcpMessageUpdateGameObjectProperties), OnUpdateGameObjectProperties);
			m_server.AddCallback(typeof(rdtTcpMessageSetArraySize), OnSetArraySize);
			m_server.AddCallback(typeof(rdtTcpMessageDeleteGameObjects), OnDeleteGameObjects);
			m_server.AddCallback(typeof(rdtTcpMessageSetParent), OnSetParent);
		}

		private void OnUpdateGameObjectProperties(rdtTcpMessage message)
		{
			rdtTcpMessageUpdateGameObjectProperties msg = (rdtTcpMessageUpdateGameObjectProperties)message;
			GameObject gob = FindGameObject(msg.m_instanceId);
			if (gob == null)
				return;
			if (msg.HasFlag(rdtTcpMessageUpdateGameObjectProperties.Flags.UpdateEnabled))
				gob.SetActive(msg.m_enabled);
			if (msg.HasFlag(rdtTcpMessageUpdateGameObjectProperties.Flags.UpdateLayer))
				gob.layer = msg.m_layer;
			if (msg.HasFlag(rdtTcpMessageUpdateGameObjectProperties.Flags.UpdateTag))
				gob.tag = msg.m_tag;
			if (msg.HasFlag(rdtTcpMessageUpdateGameObjectProperties.Flags.UpdateFlags))
				gob.hideFlags = (HideFlags)msg.m_hideFlags;
		}

		private void OnUpdateComponentProperties(rdtTcpMessage message)
		{
			rdtDebug.Debug(this, "OnUpdateComponentProperties");
			rdtTcpMessageUpdateComponentProperties msg = (rdtTcpMessageUpdateComponentProperties)message;
			GameObject gob = FindGameObject(msg.m_gameObjectInstanceId);
			if (gob == null)
				return;
			Component component = FindComponent(gob, msg.m_componentInstanceId);
			if (component == null)
			{
				rdtDebug.Error(this, "Tried to update component with id {0} (name={1}) but couldn't find it!", msg.m_componentInstanceId, msg.m_componentName);
				return;
			}
			if (component is Behaviour)
				((Behaviour)component).enabled = msg.m_enabled;
			else if (component is Renderer)
				((Renderer)component).enabled = msg.m_enabled;
			else if (component is Collider)
				((Collider)component).enabled = msg.m_enabled;
			if (msg.m_properties != null)
			{
				m_server.SerializerRegistry.WriteAllFields(component, msg.m_properties, msg.m_arrayIndex);
				Graphic g = component as Graphic;
				if (g)
					g.SetAllDirty();
			}
		}

		private void OnSetArraySize(rdtTcpMessage message)
		{
			rdtDebug.Debug(this, "rdtTcpMessageSetArraySize");
			rdtTcpMessageSetArraySize msg = (rdtTcpMessageSetArraySize)message;
			GameObject gob = FindGameObject(msg.m_gameObjectInstanceId);
			if (gob == null)
				return;
			if (msg.m_size < 0)
				return;
			Component component = FindComponent(gob, msg.m_componentInstanceId);
			if (component == null)
			{
				rdtDebug.Error(this, "Tried to set array size on component with id {0} (name={1}) but couldn't find it!", msg.m_componentInstanceId, msg.m_componentName);
				return;
			}
			m_server.SerializerRegistry.SetArraySize(component, msg.m_properties, msg.m_size);
		}

		private void OnDeleteGameObjects(rdtTcpMessage message)
		{
			rdtTcpMessageDeleteGameObjects msg = (rdtTcpMessageDeleteGameObjects)message;
			for (int j = 0; j < msg.m_instanceIds.Count; j++)
			{
				GameObject gob = FindGameObject(msg.m_instanceIds[j]);
				if (!(gob == null))
					Object.Destroy(gob);
			}
		}

		private void OnSetParent(rdtTcpMessage message)
		{
			rdtTcpMessageSetParent msg = (rdtTcpMessageSetParent)message;
			List<int> instanceIds = new List<int>();
			instanceIds.Add(msg.m_parentId);
			instanceIds.AddRange(msg.m_childrenIds);
			Dictionary<int, Transform> transDic = new Dictionary<int, Transform>();
			int index = msg.m_startIndex;
			Scene parentScene = FindTransformsAndScene(instanceIds, transDic);
			if (parentScene.handle == msg.m_parentId)
			{
				for (int j = 0; j < msg.m_childrenIds.Count; j++)
				{
					Transform child;
					if (transDic.TryGetValue(msg.m_childrenIds[j], out child))
					{
						child.SetParent(null);
						SceneManager.MoveGameObjectToScene(child.gameObject, parentScene);
                        child.SetSiblingIndex(index);
						index++;

					}
				}
			}
			else
			{
				Transform parent;
				if (transDic.TryGetValue(msg.m_parentId, out parent))
				{
					for (int j = 0; j < msg.m_childrenIds.Count; j++)
					{
						Transform child;
						if (transDic.TryGetValue(msg.m_childrenIds[j], out child))
						{
							child.SetParent(parent);
							child.SetSiblingIndex(index);
							index++;
						}
					}
				}
            }
		}

		private void OnRequestGameObjectComponents(rdtTcpMessage message)
		{
			rdtTcpMessageGetComponents m = (rdtTcpMessageGetComponents)message;
			GameObject gob = (m.m_instanceId != 0) ? FindGameObject(m.m_instanceId) : null;
			rdtTcpMessageComponents msg = default(rdtTcpMessageComponents);
			msg.m_instanceId = ((gob != null) ? m.m_instanceId : 0);
			msg.m_components = new List<rdtTcpMessageComponents.Component>();
			msg.m_layer = ((gob != null) ? gob.layer : 0);
			msg.m_tag = ((gob != null) ? gob.tag : "");
			msg.m_enabled = (gob != null && gob.activeInHierarchy);
			msg.m_hideFlags = (byte)((gob != null) ? gob.hideFlags : 0);
			if (gob)
			{
				m_components.Clear();
				gob.GetComponents(m_unityComponents);
				if (m_unityComponents.Count > m_components.Capacity)
					m_components.Capacity = m_unityComponents.Count;
				for (int j = 0; j < m_unityComponents.Count; j++)
				{
					Component c = m_unityComponents[j];
					if (c == null)
					{
						rdtDebug.Debug(this, "Component is null, skipping");
					}
					else
					{
						List<rdtTcpMessageComponents.Property> properties = m_server.SerializerRegistry.ReadAllFields(c);
						if (properties == null)
							rdtDebug.Debug(this, "Properties are null, skipping");
						else
						{
							rdtTcpMessageComponents.Component component = default(rdtTcpMessageComponents.Component);
							if (c is Behaviour)
							{
								component.m_canBeDisabled = true;
								component.m_enabled = ((Behaviour)c).enabled;
							}
							else if (c is Renderer)
							{
								component.m_canBeDisabled = true;
								component.m_enabled = ((Renderer)c).enabled;
							}
							else if (c is Collider)
							{
								component.m_canBeDisabled = true;
								component.m_enabled = ((Collider)c).enabled;
							}
							else
							{
								component.m_canBeDisabled = false;
								component.m_enabled = true;
							}
							Type type = c.GetType();
							component.m_name = type.Name;
							component.m_assemblyName = type.AssemblyQualifiedName;
							component.m_instanceId = c.GetInstanceID();
							component.m_properties = properties;
							m_components.Add(component);
						}
					}
				}
			}
			msg.m_components = m_components;
			m_unityComponents.Clear();
			m_server.EnqueueMessage(msg);
		}

		private void OnRequestGameObjects(rdtTcpMessage message)
		{
			rdtTcpMessageGameObjects msg = default(rdtTcpMessageGameObjects);
			m_scenes.Clear();
			List<Scene> scenes = m_scenes;
			m_gameObjects.Clear();
			int total = 0; 
			for (int j = 0; j < SceneManager.sceneCount; j++)
			{
				Scene scene = SceneManager.GetSceneAt(j);
				if (scene.isLoaded && scene.IsValid())
				{
					scenes.Add(scene);
					total += scene.rootCount;
					if (total > m_gameObjects.Capacity)
						m_gameObjects.Capacity = scene.rootCount;
					GameObject[] gobs = scene.GetRootGameObjects();
					m_gameObjects.AddRange(gobs);
				}
			}
			List<GameObject> gameObjects = m_gameObjects;
			List<GameObject> ddol = m_server.DontDestroyOnLoadObjects;
			if (!m_dontDestroyOnLoadBadObject)
			{
				for (int k = 0; k < ddol.Count; k++)
				{
					if (ddol[k] == null)
					{
						rdtDebug.Log(rdtDebug.LogLevel.Warning, "A null GameObject was found in the DontDestroyOnLoadObjects list! Please ensure only DontDestroyOnLoad objects are added to the server.");
						m_dontDestroyOnLoadBadObject = true;
						break;
					}
				}
			}
			for (int l = 0; l < ddol.Count; l++)
			{
				GameObject obj = ddol[l];
				if (!(obj == null) && !gameObjects.Contains(obj))
				{
					gameObjects.Add(obj);
					if (!scenes.Contains(obj.scene))
						scenes.Add(obj.scene);
				}
			}
			int count = gameObjects.Count;
			total = count + scenes.Count;
			if (total > m_allGobs.Capacity)
				m_allGobs.Capacity = total;
			m_allGobs.Clear();
			for (int s = 0; s < scenes.Count; s++)
            {
				AddScene(scenes[s], m_allGobs);
            }
			for (int m = 0; m < count; m++)
			{
				GameObject g = gameObjects[m];
				if (g != null && g.hideFlags == HideFlags.None && g.transform.hideFlags == HideFlags.None)
					AddGameObject(g, m_allGobs);
			}
			msg.m_allGobs = m_allGobs;
			m_server.EnqueueMessage(msg);
			m_scenes.Clear();
			m_gameObjects.Clear();
		}

		private void AddScene(Scene s, List<rdtTcpMessageGameObjects.Gob> list)
		{
			rdtTcpMessageGameObjects.Gob gob = default(rdtTcpMessageGameObjects.Gob);
			gob.m_name = s.name;
			gob.m_scene = s.handle;
			gob.m_instanceId = s.handle;
			gob.m_hideFlags = 255;
			gob.m_enabled = true;
			list.Add(gob);
		}

		private void AddGameObject(GameObject g, List<rdtTcpMessageGameObjects.Gob> list)
		{
			rdtTcpMessageGameObjects.Gob gob = default(rdtTcpMessageGameObjects.Gob);
			//gob.m_scene = g.scene.IsValid() ? g.scene.name : "<no scene>";
			gob.m_scene = g.scene.IsValid() ? g.scene.handle : 0;
			gob.m_name = g.name;
			gob.m_instanceId = g.GetInstanceID();
			gob.m_hideFlags = (byte)g.hideFlags;
			Transform parent = g.transform.parent;
			//gob.m_hasParent = parent != null;
			//if (gob.m_hasParent)
			if (parent != null)
				gob.m_parentInstanceId = parent.gameObject.GetInstanceID();
			gob.m_enabled = g.activeInHierarchy;
			list.Add(gob);
			for (int i = 0; i < g.transform.childCount; i++)
			{
				Transform child = g.transform.GetChild(i);
				AddGameObject(child.gameObject, list);
			}
		}

		public GameObject FindGameObject(int instanceId)
		{
			try
			{
				for (int i = 0; i < SceneManager.sceneCount; i++)
				{
					Scene scene = SceneManager.GetSceneAt(i);
					if (scene.isLoaded && scene.IsValid())
					{
						if (scene.rootCount > m_gameObjects.Capacity)
							m_gameObjects.Capacity = scene.rootCount;
						m_gameObjects.Clear();
						scene.GetRootGameObjects(m_gameObjects);
						int count = m_gameObjects.Count;
						for (int j = 0; j < count; j++)
						{
							GameObject parent = m_gameObjects[j];
							GameObject gob = FindGameObject(instanceId, parent);
							if (gob != null)
								return gob;
						}
					}
				}
				List<GameObject> ddol = m_server.DontDestroyOnLoadObjects;
				for (int k = 0; k < ddol.Count; k++)
				{
					GameObject parent2 = ddol[k];
					if (parent2 != null)
					{
						GameObject gob2 = FindGameObject(instanceId, parent2);
						if (gob2 != null)
							return gob2;
					}
				}
				return null;
			}
			finally
            {
				m_gameObjects.Clear();
			}
        }

        private GameObject FindGameObject(int instanceId, GameObject parent)
		{
			if (parent.GetInstanceID() == instanceId)
				return parent;
			for (int i = 0; i < parent.transform.childCount; i++)
			{
				GameObject child = parent.transform.GetChild(i).gameObject;
				GameObject gob = FindGameObject(instanceId, child);
				if (gob != null)
					return gob;
			}
			return null;
		}

		private Component FindComponent(GameObject gob, int instanceId)
		{
			gob.GetComponents<Component>(m_unityComponents);
			for (int i = 0; i < m_unityComponents.Count; i++)
			{
				Component component = m_unityComponents[i];
				if (component.GetInstanceID() == instanceId)
					return component;
			}
			return null;
		}

		public Scene FindTransformsAndScene(List<int> instanceIds, Dictionary<int, Transform> transDic)
		{
            try
			{
				Scene parentScene = new Scene();
				int parentId = instanceIds[0];
				for (int i = 0; i < SceneManager.sceneCount; i++)
				{
					Scene scene = SceneManager.GetSceneAt(i);
					if (scene.isLoaded && scene.IsValid())
					{
						if (parentId != 0 && parentId == scene.handle)
						{
							instanceIds.Remove(scene.handle);
							parentScene = scene;
							parentId = 0;
						}

						if (scene.rootCount > m_gameObjects.Capacity)
							m_gameObjects.Capacity = scene.rootCount;
						m_gameObjects.Clear();
						scene.GetRootGameObjects(m_gameObjects);
						int count = m_gameObjects.Count;
						for (int j = 0; j < count; j++)
						{
							FindTransforms(m_gameObjects[j].transform, instanceIds, transDic);
							if (instanceIds.Count == 0)
								return parentScene;
						}
						if (parentId != 0 && !instanceIds.Contains(parentId))
							parentId = 0;
					}
				}
				List<GameObject> ddol = m_server.DontDestroyOnLoadObjects;
				for (int k = 0; k < ddol.Count; k++)
				{
					GameObject parent = ddol[k];
					if (parent != null)
					{
						if (parentId != 0)
						{
							Scene scene = parent.scene;
							if (parentId == scene.handle)
							{
								instanceIds.Remove(scene.handle);
								parentScene = scene;
								parentId = 0;
							}
						}
						FindTransforms(parent.transform, instanceIds, transDic);
						if (instanceIds.Count == 0)
							return parentScene;
						if (parentId != 0 && !instanceIds.Contains(parentId))
							parentId = 0;
					}
				}
				return parentScene;
			}
			finally
			{
				m_gameObjects.Clear();
			}
		}

		private void FindTransforms(Transform parent, List<int> instanceIds, Dictionary<int, Transform> transDic)
		{
			int instanceId = parent.gameObject.GetInstanceID();
			int index = instanceIds.IndexOf(instanceId);
			if (index >= 0)
			{
				transDic[instanceId] = parent;
				instanceIds.Remove(instanceId);
				if (instanceIds.Count == 0)
					return;
			}
			for (int i = 0; i < parent.childCount; i++)
			{
				FindTransforms(parent.GetChild(i), instanceIds, transDic);
				if (instanceIds.Count == 0)
					return;
			}
		}
	}
}
