using GameDLL.Hdg;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameEditor.Hdg
{
    public partial class ConnectionWindow
    {
        private bool m_forceRefreshHierarchy;

        private float m_gameobjectUpdateTime = 1f;
        public float gameobjectUpdateTime
        {
            get
            {
                return m_gameobjectUpdateTime;
            }
            set
            {
                m_gameobjectUpdateTime = value;
            }
        }

        private float m_componentUpdateTime = 0.1f;
        public float componentUpdateTime
        {
            get
            {
                return m_componentUpdateTime;
            }
            set
            {
                m_componentUpdateTime = value;
            }
        }

        [NonSerialized]
        private bool m_waitingForGameObjects;

        private rdtTcpMessageComponents? m_components;


        private Vector2 m_componentsScrollPos;

        private double m_gameObjectRefreshTimer;

        private double m_componentRefreshTimer;

        private rdtGuiTree<rdtTcpMessageGameObjects.Gob> m_hierarchyTree;

        private rdtSerializerRegistry m_serializerRegistry = new rdtSerializerRegistry();



        private rdtGuiProperty m_propertyGui;

        [NonSerialized]
        private rdtTcpMessageComponents.Component? m_pendingExpandComponent;

        [NonSerialized]
        private List<rdtTcpMessageGameObjects.Gob> m_gameObjects;

        private Texture2D m_sceneIcon;


        private void ConnectHierarchy()
        {
            m_pendingExpandComponent = null;
            m_components = null;
            m_propertyGui = new rdtGuiProperty(OnComponentValueChanged);
            m_client.AddCallback(typeof(rdtTcpMessageGameObjects), OnMessageGameObjects);
            m_client.AddCallback(typeof(rdtTcpMessageComponents), OnMessageGameObjectComponents);
        }

        private void DisconnectHierarchy()
        {
            m_hierarchyTree.Clear();
            m_components = null;
            m_waitingForGameObjects = false;
        }

        private void ConnectionStatusChangedHierarchy()
        {
            m_pendingExpandComponent = null;
            m_components = null;
            m_hierarchyTree.Clear();
            if (!m_automaticRefresh && m_client != null && m_client.IsConnected)
                RefreshGameObjects();
        }

        private void OnEnableHierarchy()
        {
            m_hierarchyTree = new rdtGuiTree<rdtTcpMessageGameObjects.Gob>(PAGE_TYPE_HIERARCHY);
            m_hierarchyTree.SelectedNodesChanged += OnHierarchyTreeSelectionChanged;
            m_hierarchyTree.SelectedNodesDeleted += OnHierarchyTreeSelectionDeleted;
            m_hierarchyTree.DrapNodesEnded += OnHierarchyTreeDragEnded;
            m_hierarchyTree.CheckDrop += OnHierarchyTreeCheckDrop;
            m_hierarchyTree.CheckAndUpdateDrop += OnHierarchyTreeCheckAndUpdateDrop;
            m_gameobjectUpdateTime = EditorPrefs.GetFloat("Hdg.RemoteDebug.GameobjectUpdateTime", 1f);
            m_componentUpdateTime = EditorPrefs.GetFloat("Hdg.RemoteDebug.ComponentUpdateTime", 0.1f);
        }

        private void OnDisableHierarchy()
        {

        }

        private void InitHierarchyStylesAndContent()
        {

        }

        private void UpdateHierarchy(double delta)
        {
            m_gameObjectRefreshTimer -= delta;
            if (m_gameObjectRefreshTimer <= 0.0)
            {
                m_forceRefreshHierarchy = false;
                RefreshGameObjects();
                m_gameObjectRefreshTimer = m_gameobjectUpdateTime;
            }
            if (m_hierarchyTree.SelectedNodes.Count == 1)
            {
                m_componentRefreshTimer -= delta;
                if (m_componentRefreshTimer <= 0.0)
                {
                    RefreshComponents();
                    m_componentRefreshTimer = m_componentUpdateTime;
                }
            }
        }

        private void DrawHierarchy(bool windowHasFocus)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1f, GUIStyle.none, GUILayout.ExpandHeight(true), GUILayout.Width(m_split.SeparatorPosition));
            m_hierarchyTree.Draw(rect, windowHasFocus);
            m_split.Draw();
            m_componentsScrollPos = EditorGUILayout.BeginScrollView(m_componentsScrollPos);
            float inspectorWidth = position.width - m_split.SeparatorPosition;
            EditorGUIUtility.wideMode = (inspectorWidth >= WIDE_MODE_SIZE_THRESHOLD);
            EditorGUIUtility.labelWidth = 0f;
            EditorGUIUtility.fieldWidth = 0f;
            if (inspectorWidth > LABEL_ADJUST_SIZE_THRESHOLD)
                EditorGUIUtility.labelWidth = (inspectorWidth - LABEL_ADJUST_SIZE_THRESHOLD) * 0.5f + EditorGUIUtility.labelWidth;
            DrawComponents();
            EditorGUILayout.EndScrollView();
        }

        private void DrawComponents()
        {
            if (m_components == null)
                return;
            if (m_components.Value.m_instanceId == 0)
            {
                EditorGUILayout.LabelField("GameObject was not found.");
                return;
            }
            DrawGameObjectTitle();
            rdtGuiLine.DrawHorizontalSplitLine();
            foreach (rdtTcpMessageComponents.Component c in m_components.Value.m_components)
            {
                if (!DrawComponentTitle(c, null))
                    rdtGuiLine.DrawHorizontalSplitLine();
                else
                {
                    m_propertyGui.DrawComponent(m_components.Value.m_instanceId, c, c.m_properties);
                    EditorGUILayout.Space();
                    rdtGuiLine.DrawHorizontalSplitLine();
                }
            }
            EditorGUILayout.Space();
        }

        private bool DrawComponentTitle(rdtTcpMessageComponents.Component component, Component unityComponent = null)
        {
            bool wasExpanded = m_expandedCache.IsExpanded(component, null);
            bool isExpanded = wasExpanded;
            Rect lastRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(18f));
            GUILayout.Space(4f);
            Rect rect = GUILayoutUtility.GetRect(13f, 16f, GUILayout.ExpandWidth(false));
            if (component.m_properties != null && component.m_properties.Count > 0)
            {
                isExpanded = EditorGUI.Foldout(rect, wasExpanded, GUIContent.none, m_normalFoldoutStyle);
                if (isExpanded != wasExpanded)
                    m_expandedCache.SetExpanded(isExpanded, component, null);
            }
            bool enabled = true;
            int toggleWidth = 16;// m_toggleStyle.normal.background.width;
            if (component.m_canBeDisabled)
                enabled = EditorGUILayout.Toggle(component.m_enabled, m_toggleStyle, GUILayout.Width(toggleWidth));
            else
                EditorGUILayout.LabelField("", GUILayout.Width(toggleWidth));
            string name = ObjectNames.NicifyVariableName(component.m_name);
            if (m_debug || m_showInstanceID)
                name = name + ":" + component.m_instanceId;
            EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
            if (component.m_canBeDisabled && enabled != component.m_enabled)
            {
                component.m_enabled = enabled;
                rdtGuiProperty.ValueChangedEvent evt = new rdtGuiProperty.ValueChangedEvent
                {
                    Component = component
                };
                OnPropertyChanged(evt, null);
            }
            EditorGUILayout.EndHorizontal();
            if (component.m_properties != null && component.m_properties.Count > 0)
            {
                Event evt2 = Event.current;
                if (lastRect.Contains(evt2.mousePosition) && evt2.isMouse)
                {
                    if (evt2.type == EventType.MouseDown)
                    {
                        m_pendingExpandComponent = new rdtTcpMessageComponents.Component?(component);
                        evt2.Use();
                    }
                    else if (evt2.type == EventType.MouseUp && m_pendingExpandComponent != null && m_pendingExpandComponent.Value.m_instanceId != component.m_instanceId)
                        m_pendingExpandComponent = null;
                }
            }
            return isExpanded;
        }

        private void DrawGameObjectTitle()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            bool enabled = EditorGUILayout.ToggleLeft("Enabled", m_components.Value.m_enabled, GUILayout.Width(80f));
            if (enabled != m_components.Value.m_enabled)
            {
                rdtTcpMessageComponents components = m_components.Value;
                components.m_enabled = enabled;
                m_components = new rdtTcpMessageComponents?(components);
                m_hierarchyTree.SelectedNodes[0].Enabled = enabled;
                OnGameObjectChanged();
            }
            if (m_hierarchyTree.SelectedNodes.Count == 1)
            {
                EditorGUILayout.LabelField("HideFlags", GUILayout.Width(60f));
                HideFlags flags = (HideFlags)EditorGUILayout.EnumFlagsField((HideFlags)m_components.Value.m_hideFlags, GUILayout.ExpandWidth(true));
                if (flags != (HideFlags)m_components.Value.m_hideFlags)
                {
                    rdtTcpMessageComponents components = m_components.Value;
                    components.m_hideFlags = (byte)flags;
                    m_components = new rdtTcpMessageComponents?(components);
                    OnGameObjectChanged();
                }
            }
            EditorGUILayout.EndHorizontal();
            if (m_hierarchyTree.SelectedNodes.Count == 1)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Tag", GUILayout.Width(30f));
                string newTag = EditorGUILayout.TagField(GUIContent.none, m_components.Value.m_tag, GUILayout.MinWidth(50f));
                if (newTag != m_components.Value.m_tag)
                {
                    rdtTcpMessageComponents components = m_components.Value;
                    components.m_tag = newTag;
                    m_components = new rdtTcpMessageComponents?(components);
                    OnGameObjectChanged();
                }
                EditorGUILayout.LabelField("Layer", GUILayout.Width(40f));
                int newLayer = EditorGUILayout.LayerField(GUIContent.none, m_components.Value.m_layer, GUILayout.MinWidth(50f));
                if (newLayer != m_components.Value.m_layer)
                {
                    rdtTcpMessageComponents components = m_components.Value;
                    components.m_layer = newLayer;
                    m_components = new rdtTcpMessageComponents?(components);
                    OnGameObjectChanged();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void BuildHierarchyTree()
        {
            m_hierarchyTree.Clear();
            List<rdtTcpMessageGameObjects.Gob> list = (from x in m_gameObjects
                                                           //where !x.m_hasParent
                                                       where x.m_parentInstanceId == 0
                                                       select x).ToList();
            Dictionary<int, rdtTcpMessageGameObjects.Gob> existing = new Dictionary<int, rdtTcpMessageGameObjects.Gob>();
            List<rdtTcpMessageGameObjects.Gob> nonRoots = (from x in (from x in m_gameObjects
                                                                          //where x.m_hasParent
                                                                      where x.m_parentInstanceId != 0
                                                                      select x).Where(delegate (rdtTcpMessageGameObjects.Gob x)
                                                                      {
                                                                          if (existing.ContainsKey(x.m_instanceId))
                                                                              return false;
                                                                          existing.Add(x.m_instanceId, x);
                                                                          return true;
                                                                      })
                                                           orderby x.m_parentInstanceId
                                                           select x).ToList();
            if (m_sceneIcon == null)
                m_sceneIcon = EditorGUIUtility.FindTexture("BuildSettings.Editor.Small");
            List<rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node> sceneRoots = new List<rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node>();
            using (List<rdtTcpMessageGameObjects.Gob>.Enumerator enumerator = list.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    rdtTcpMessageGameObjects.Gob r = enumerator.Current;
                    rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node root = null;
                    if (r.m_hideFlags == 255)
                    {
                        root = sceneRoots.FirstOrDefault((rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node x) => x.ID == r.m_instanceId);
                        r.m_hideFlags = 0;
                        if (root == null)
                        {
                            root = m_hierarchyTree.AddNode(r);
                            sceneRoots.Add(root);
                            root.NormalIcon = m_sceneIcon;
                            root.IsBold = true;
                            //DontDestroyOnLoad
                        }
                        else
                        {
                            m_hierarchyTree.ResetNode(r);
                        }
                    }
                    else
                    {
                        root = sceneRoots.FirstOrDefault((rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node x) => x.ID == r.m_scene);
                        if (root == null)
                        {
                            root = m_hierarchyTree.AddNode(r.m_scene);
                            sceneRoots.Add(root);
                            root.NormalIcon = m_sceneIcon;
                            root.IsBold = true;
                        }
                        rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node node = root.AddNode(r, r.m_enabled);
                        AddHierarchyChildren(node, nonRoots);
                    }
                }
            }
        }

        private void AddHierarchyChildren(rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node parentNode, List<rdtTcpMessageGameObjects.Gob> nonRoots)
        {
            int firstChildIndex = 0;
            while (firstChildIndex < nonRoots.Count && nonRoots[firstChildIndex].m_parentInstanceId != parentNode.Data.m_instanceId)
                firstChildIndex++;
            if (firstChildIndex >= nonRoots.Count || nonRoots.Count == 0)
                return;
            List<rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node> children = new List<rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node>();
            int count = 0;
            int i = firstChildIndex;
            while (i < nonRoots.Count)
            {
                rdtTcpMessageGameObjects.Gob g = nonRoots[i];
                if (g.m_parentInstanceId != parentNode.Data.m_instanceId)
                    break;
                rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node node = parentNode.AddNode(g, g.m_enabled);
                children.Add(node);
                i++;
                count++;
            }
            nonRoots.RemoveRange(firstChildIndex, count);
            for (int j = 0; j < children.Count; j++)
            {
                rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node node2 = children[j];
                AddHierarchyChildren(node2, nonRoots);
            }
        }

        private void OnComponentValueChanged(rdtGuiProperty.ValueChangedEvent valueChangedEvent)
        {
            if (valueChangedEvent.UpdateProperty)
            {
                rdtTcpMessageComponents.Property prop = valueChangedEvent.Hierarchy.Pop();
                prop.m_value = valueChangedEvent.NewValue;
                valueChangedEvent.Hierarchy.Push(prop);
            }
            OnPropertyChanged(valueChangedEvent, null);
        }

        private void OnMessageGameObjects(rdtTcpMessage message)
        {
            m_waitingForGameObjects = false;
            rdtTcpMessageGameObjects msg = (rdtTcpMessageGameObjects)message;
            m_gameObjects = msg.m_allGobs;
            m_updatingTree = true;
            List<rdtTcpMessageGameObjects.Gob> selectionData = (from x in m_hierarchyTree.SelectedNodes
                                                                where x.HasData
                                                                select x.Data).ToList();
            List<string> selectionNoData = (from x in m_hierarchyTree.SelectedNodes
                                            where !x.HasData
                                            select x.Name).ToList();
            BuildHierarchyTree();
            if (selectionData.Count > 0 || selectionNoData.Count > 0)
            {
                List<rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node> selectionNodesData = (from x in selectionData
                                                                                          select m_hierarchyTree.FindNode(x) into x
                                                                                          where x != null
                                                                                          select x).ToList();
                List<rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node> selectionNodesNoData = (from x in selectionNoData
                                                                                            select m_hierarchyTree.FindNode(x) into x
                                                                                            where x != null
                                                                                            select x).ToList();
                m_hierarchyTree.SelectedNodes.AddRange(selectionNodesData);
                m_hierarchyTree.SelectedNodes.AddRange(selectionNodesNoData);
            }
            m_updatingTree = false;
            Repaint();
        }

        private void OnMessageGameObjectComponents(rdtTcpMessage message)
        {
            m_components = new rdtTcpMessageComponents?((rdtTcpMessageComponents)message);
            List<rdtTcpMessageComponents.Component> components = m_components.Value.m_components;
            for (int i = 0; i < components.Count; i++)
            {
                rdtTcpMessageComponents.Component c = components[i];
                if (c.m_properties == null)
                {
                    rdtDebug.Debug(this, "Component '{0}' has no properties", c.m_name);
                }
                else
                {
                    for (int j = 0; j < c.m_properties.Count; j++)
                    {
                        rdtTcpMessageComponents.Property p = c.m_properties[j];
                        p.Deserialise(m_serializerRegistry);
                        c.m_properties[j] = p;
                    }
                    components[i] = c;
                }
            }
            Repaint();
        }

        private void OnHierarchyTreeSelectionChanged()
        {
            if (m_updatingTree)
                return;
            m_clearFocus = true;
            rdtGuiTree<rdtTcpMessageGameObjects.Gob>.SelectedNodeCollection selected = m_hierarchyTree.SelectedNodes;
            if (selected.Count != 0 && !selected.Any((rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node x) => !x.HasData))
                RefreshComponents();
            else
            {
                m_pendingExpandComponent = null;
                m_components = null;
            }
            Repaint();
        }

        private void OnHierarchyTreeSelectionDeleted()
        {
            rdtTcpMessageDeleteGameObjects msg = default(rdtTcpMessageDeleteGameObjects);
            IEnumerable<int> selectedIds = from x in m_hierarchyTree.SelectedNodes
                                           select x.Data.m_instanceId;
            msg.m_instanceIds = selectedIds.ToList<int>();
            m_client.EnqueueMessage(msg);
            m_hierarchyTree.SelectedNodes.Clear();
            m_gameObjectRefreshTimer = 0.10000000149011612;//=0.1f
            m_forceRefreshHierarchy = true;
        }

        private void OnHierarchyTreeDragEnded(int parentId, int startIndex, List<int> childrenIds)
        {
            rdtTcpMessageSetParent msg = default(rdtTcpMessageSetParent);
            msg.m_parentId = parentId;
            msg.m_startIndex = startIndex;
            msg.m_childrenIds = childrenIds;
            m_client.EnqueueMessage(msg);
            m_gameObjectRefreshTimer = 0.10000000149011612;//=0.1f
            m_forceRefreshHierarchy = true;
        }

        private bool OnHierarchyTreeCheckDrop(rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node dropTarget, rdtGuiTree<rdtTcpMessageGameObjects.Gob>.HierarchyDropMode dropMode)
        {
            if (dropTarget.IsFirstNode)
            {
                if (!dropMode.HasFlag(rdtGuiTree<rdtTcpMessageGameObjects.Gob>.HierarchyDropMode.kHierarchyDropUpon))
                    return true;
            }
            return false;
        }

        private void OnHierarchyTreeCheckAndUpdateDrop(rdtGuiTree<rdtTcpMessageGameObjects.Gob>.DropResult result, List<rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node> selection)
        {
            for (int i = 0; i < selection.Count; i++)
            {
                rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node node = selection[i];
                if (node != null && !node.IsFirstNode && !result.parent.IsSelfOrChild(node))
                    result.children.Add(node);
            }
        }

        private List<rdtTcpMessageComponents.Property> CloneAndSerialize(Stack<rdtTcpMessageComponents.Property> hierarchy, bool serialiseValues = true)
        {
            List<rdtTcpMessageComponents.Property> list = new List<rdtTcpMessageComponents.Property>();
            rdtTcpMessageComponents.Property topProperty = hierarchy.Peek();
            rdtTcpMessageComponents.Property property = topProperty.Clone();
            if (serialiseValues)
                property.m_value = m_serializerRegistry.Serialize(topProperty.m_value);
            bool top = true;
            foreach (rdtTcpMessageComponents.Property p in hierarchy)
            {
                if (top)
                    top = false;
                else
                {
                    rdtTcpMessageComponents.Property parent = p.Clone();
                    parent.m_value = new List<rdtTcpMessageComponents.Property>
                    {
                        property
                    };
                    property = parent;
                }
            }
            list.Add(property);
            return list;
        }

        private void OnPropertyChanged(rdtGuiProperty.ValueChangedEvent valueChangedEvent, Component unityComponent = null)
        {
            if (m_client == null)
                return;
            rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node selected = m_hierarchyTree.SelectedNodes[0];
            rdtTcpMessage message;
            if (valueChangedEvent.NewArraySize != -1)
            {
                rdtTcpMessageSetArraySize i = default(rdtTcpMessageSetArraySize);
                i.m_gameObjectInstanceId = selected.Data.m_instanceId;
                i.m_componentName = valueChangedEvent.Component.m_name;
                i.m_componentInstanceId = valueChangedEvent.Component.m_instanceId;
                i.m_size = valueChangedEvent.NewArraySize;
                Stack<rdtTcpMessageComponents.Property> hierarchy = valueChangedEvent.Hierarchy;
                if (hierarchy != null && hierarchy.Count > 0)
                {
                    i.m_properties = CloneAndSerialize(hierarchy, false);
                }
                message = i;
            }
            else
            {
                rdtTcpMessageUpdateComponentProperties j = default(rdtTcpMessageUpdateComponentProperties);
                j.m_arrayIndex = valueChangedEvent.ArrayIndex;
                j.m_gameObjectInstanceId = selected.Data.m_instanceId;
                j.m_componentName = valueChangedEvent.Component.m_name;
                j.m_componentInstanceId = valueChangedEvent.Component.m_instanceId;
                j.m_enabled = valueChangedEvent.Component.m_enabled;
                Stack<rdtTcpMessageComponents.Property> hierarchy2 = valueChangedEvent.Hierarchy;
                if (hierarchy2 != null && hierarchy2.Count > 0)
                {
                    j.m_properties = CloneAndSerialize(hierarchy2, true);
                }
                message = j;
            }
            m_client.EnqueueMessage(message);
            RefreshComponents();
        }

        private void OnGameObjectChanged()
        {
            if (m_client == null)
                return;
            rdtGuiTree<rdtTcpMessageGameObjects.Gob>.SelectedNodeCollection selected = m_hierarchyTree.SelectedNodes;
            for (int i = 0; i < selected.Count; i++)
            {
                rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node gob = selected[i];
                rdtTcpMessageUpdateGameObjectProperties message = default(rdtTcpMessageUpdateGameObjectProperties);
                message.m_instanceId = gob.Data.m_instanceId;
                message.m_enabled = m_components.Value.m_enabled;
                message.SetFlag(rdtTcpMessageUpdateGameObjectProperties.Flags.UpdateEnabled, true);
                message.m_layer = m_components.Value.m_layer;
                message.SetFlag(rdtTcpMessageUpdateGameObjectProperties.Flags.UpdateLayer, selected.Count == 1);
                message.m_tag = m_components.Value.m_tag;
                message.SetFlag(rdtTcpMessageUpdateGameObjectProperties.Flags.UpdateTag, selected.Count == 1);
                message.m_hideFlags = m_components.Value.m_hideFlags;
                message.SetFlag(rdtTcpMessageUpdateGameObjectProperties.Flags.UpdateFlags, selected.Count == 1);
                m_client.EnqueueMessage(message);
            }
        }

        private void RefreshComponents()
        {
            if (m_hierarchyTree.SelectedNodes.Count == 0)
                return;
            rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node node = m_hierarchyTree.SelectedNodes[0];
            if (!node.HasData)
                return;
            rdtTcpMessageGetComponents message = default(rdtTcpMessageGetComponents);
            message.m_instanceId = node.Data.m_instanceId;
            m_client.EnqueueMessage(message);
        }

        private void RefreshGameObjects()
        {
            if (m_client == null || !m_client.IsConnected || m_waitingForGameObjects)
                return;
            rdtDebug.Debug(this, "Refreshing GameObject list from the server");
            rdtTcpMessageGetGameObjects msg = default(rdtTcpMessageGetGameObjects);
            m_client.EnqueueMessage(msg);
            m_waitingForGameObjects = true;
        }


    }
}
