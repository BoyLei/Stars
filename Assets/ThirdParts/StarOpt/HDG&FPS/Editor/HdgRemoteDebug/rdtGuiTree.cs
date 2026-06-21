using GameDLL;
using GameDLL.Hdg;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameEditor.Hdg
{
	public class rdtGuiTree<T> where T : IModifiable
	{
		public class Node
		{
			private rdtExpandedCache m_expandedCache;

			public int ID { get; private set; }

			public T Data { get; private set; }

			public string Name { get; private set; }

			public List<Node> Children { get; private set; }

			public bool HasChildren
			{
				get
				{
					return Children.Count > 0;
				}
			}

			public Node Parent { get; private set; }

			public bool IsRoot
            {
				get
                {
					return Parent == null;
                }
            }

			public Node FirstNode { get; private set; }

			public bool IsFirstNode
            {
				get
                {
					return FirstNode != null && FirstNode == this;

				}
            }

			public bool CanBeParent
            {
				get
                {
					return Data != null && Data.canBeParent;
                }
            }

			public bool Enabled { get; set; }

			public bool HasData { get; private set; }

			public Texture2D NormalIcon { get; set; }

			public Texture2D ExpandedIcon { get; set; }

			public Texture2D EmptyIcon { get; set; }

			public bool IsBold { get; set; }

			public int Depth { get; set; }

			public bool Expanded
			{
				get
				{
					//int hashCode;
					//if (!HasData)
					//	hashCode = Name.GetHashCode();
					//else
					//{
					//	T data = Data;
					//	hashCode = data.GetHashCode();
					//}
					//int hash = hashCode;
					return m_expandedCache.IsExpanded(ID, null);
				}
				set
				{
					//int hashCode;
					//if (!HasData)
					//	hashCode = Name.GetHashCode();
					//else
					//{
					//	T data = Data;
					//	hashCode = data.GetHashCode();
					//}
					//int hash = hashCode;
					m_expandedCache.SetExpanded(value, ID, null);
				}
			}

			public Node(string name, Node parent, rdtExpandedCache expandedCache)
			{
				Depth = ((parent == null) ? 0 : (parent.Depth + 1));
				Parent = parent;
				HasData = false;
				Name = name;
				m_expandedCache = expandedCache;
				Enabled = true;
				Children = new List<Node>();
				if (parent != null)
					FirstNode = this;
				UpdateID();
			}

			public Node(int id, Node parent, rdtExpandedCache expandedCache)
			{
				Depth = ((parent == null) ? 0 : (parent.Depth + 1));
				Parent = parent;
				HasData = false;
				Name = "Untitled";
				m_expandedCache = expandedCache;
				Enabled = true;
				Children = new List<Node>();
				FirstNode = this;
				ID = id;
			}

			public Node(T data, Node parent, rdtExpandedCache expandedCache)
			{
				Depth = ((parent == null) ? 0 : (parent.Depth + 1));
				HasData = parent.FirstNode != null;
				m_expandedCache = expandedCache;
				Enabled = true;
				Parent = parent;
				Data = data;
				Children = new List<Node>();
				FirstNode = parent.FirstNode;
				Name = data.ToString();
				if (FirstNode == null)
					FirstNode = this;
				UpdateID();
			}

			internal void ResetData(T data)
            {
				Data = data;
				UpdateID();
			}

			void UpdateID()
            {
				if (Parent == null)
					ID = 0;
                //if (!HasData)
                //{
                //	//if (Parent == null)
                //	//	ID = 0;
                //	//else
                //	ID = Name.GetHashCode();
                //}
                else
                {
					T data = Data;
					ID = data.GetHashCode();
				}
			}

			public Node AddNode(T data, bool enabled = true)
			{
				Node node = new Node(data, this, m_expandedCache);
				node.Enabled = enabled;
				Children.Add(node);
				return node;
			}

			public Texture2D GetIcon(bool wasExpanded)
            {
				Texture2D icon = null;
				if (wasExpanded)
					icon = ExpandedIcon;
				else if (!HasChildren)
					icon = EmptyIcon;
				if (icon == null)
					icon = NormalIcon;
				return icon;
			}

			/// <summary>
			/// 判断node节点是否是当前节点的子节点
			/// </summary>
			public bool IsSelfOrChild(Node node)
            {
				if (node == this)
					return true;
				Node child = this;
				while(child.Parent != null)
                {
					if (child.Parent == node)
						return true;
					child = child.Parent;
				}
				return false;
			}

			public int GetItemControlID()
            {
				return ID + 10000000;
			}
		}

		public class SelectedNodeCollection : ObservableList<Node>
		{
		}

		public rdtExpandedCache ExpandedCache
		{
			get
			{
				return m_expandedCache;
			}
		}

		public SelectedNodeCollection SelectedNodes
		{
			get
			{
				return m_selectedNodes;
			}
		}

		public string Filter
		{
			get
			{
				return m_filter;
			}
			set
			{
				m_visibleNodesDirty |= !string.Equals(m_filter, value);
				string prevFilter = m_filter;
				m_filter = value;
				if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(prevFilter))
					EnsureVisible(SelectedNodes);
			}
		}

		public event Action SelectedNodesChanged;

		public event Action SelectedNodesDeleted;

		public event Action<int, int, List<int>> DrapNodesEnded;

		public event Func<Node, HierarchyDropMode, bool> CheckDrop;

		public event Action<DropResult, List<Node>> CheckAndUpdateDrop;

		private const float ROW_HEIGHT = 16f;

		private const float INDENT_WIDTH = 13f;

		private const float FOLDOUT_WIDTH = 12f;

		private const float BASE_INDENT = 2f;

		private const float ICON_WIDTH = 16f;

		private const float SCROLLBAR_WIDTH = 16f;

		private const float TREE_BOTTOM_MARGIN = 2f;

		private const float SPACE_BETWEEN_ICON_AND_TEXT = 2f;

		private GUIContent m_tempContent = new GUIContent();

		private Vector2 m_scrollPosition;

		private Node m_root;

		private SelectedNodeCollection m_selectedNodes;

		private GUIStyle m_foldoutStyle;

		protected float foldoutStyleWidth
		{
			get { return m_foldoutStyle.fixedWidth; }
		}

		private GUIStyle m_lineStyle;

		private GUIStyle m_disabledLineStyle;

		private GUIStyle m_boldLineStyle;

		private GUIStyle m_selectionStyle;

		private GUIStyle m_noDataStyle;

		private GUIStyle m_insertionStyle;

		GUIStyle lineStyle;

		private Rect m_scrollViewRect;

		private GUIContent m_content = new GUIContent();

		private bool m_hasFocus;

		private rdtExpandedCache m_expandedCache;

		private string m_filter;

		private List<Node> m_visibleNodes;

		private bool m_visibleNodesDirty;

		protected Rect m_draggingInsertionMarkerRect;

		private int m_treeType;

		public rdtGuiTree(int type)
		{
			m_treeType = type;
			m_expandedCache = new rdtExpandedCache();
			m_root = new Node("root", null, m_expandedCache);
			m_root.Depth = -1;
			m_selectedNodes = new SelectedNodeCollection();
			m_selectedNodes.ListChanged += OnSelectedNodesChanged;
			m_visibleNodesDirty = true;
		}

		private void OnSelectedNodesChanged(ObservableList<Node> obj)
		{
			if (SelectedNodesChanged != null)
				SelectedNodesChanged();
		}

		public Node FindNode(T data)
		{
			return BuildFlatList(m_root.Children, true).FirstOrDefault(delegate(Node x)
			{
				T data2 = x.Data;
				return data2.Equals(data);
			});
		}

		public Node FindNode(string name)
		{
			return BuildFlatList(m_root.Children, true).FirstOrDefault((Node x) => x.Name.Equals(name));
		}

        public Node AddNode(T data, bool enabled = true)
        {
            Node node = new Node(data, m_root, m_expandedCache);
            node.Enabled = enabled;
            m_root.Children.Add(node);
            m_visibleNodesDirty = true;
            return node;
        }

        public Node AddNode(string name)
		{
			Node node = new Node(name, m_root, m_expandedCache);
			node.Enabled = true;
			m_root.Children.Add(node);
			m_visibleNodesDirty = true;
			return node;
		}

		public Node AddNode(int id)
		{
			Node node = new Node(id, m_root, m_expandedCache);
			node.Enabled = true;
			m_root.Children.Add(node);
			m_visibleNodesDirty = true;
			return node;
		}

		public void ResetNode(T data)
        {
			int id = data.GetHashCode();
			Node node = m_root.Children.Find((n) => n.ID == id);
			if (node != null)
				node.ResetData(data);
		}

		public void Clear()
		{
			m_root.Children.Clear();
			SelectedNodes.Clear();
			m_visibleNodesDirty = true;
		}

		public void Draw(Rect rect, bool windowHasFocus)
		{
			RefreshVisibleNodes();

			if (Event.current.type == EventType.Repaint)
				m_scrollViewRect = rect;
			m_hasFocus = m_hasFocus && windowHasFocus;
			InitStyles();
			ProcessKeyboardInput();
			Vector2 visibleSize = GetTotalSize();
			Rect viewRect = new Rect(0f, 0f, rect.width, visibleSize.y);
			if (viewRect.height > rect.height)
			{
				viewRect.width -= SCROLLBAR_WIDTH;
			}
			m_scrollPosition = GUI.BeginScrollView(rect, m_scrollPosition, viewRect);
			m_draggingInsertionMarkerRect.x = -1;
			int first;
			int last;
			GetFirstLastRowVisible(out first, out last);
			for (int i = first; i <= last; i++)
				DrawNode(m_visibleNodes[i], i, viewRect.width);
			if (m_draggingInsertionMarkerRect.x >= 0 && Event.current.type == EventType.Repaint)
				m_insertionStyle.Draw(m_draggingInsertionMarkerRect, false, false, false, false);
			GUI.EndScrollView();

            HandleUnusedEvents();
        }

		private void EnsureVisible(List<Node> nodes)
		{
			if (nodes.Count == 0)
				return;
			for (int i = 0; i < nodes.Count; i++)
			{
				for (Node parent = nodes[i].Parent; parent != m_root; parent = parent.Parent)
					SetNodeExpanded(parent, true);
			}
			RefreshVisibleNodes();
			int index = m_visibleNodes.IndexOf(nodes[0]);
			float top = index * ROW_HEIGHT;
			float bottom = top + ROW_HEIGHT;
			if (bottom >= m_scrollPosition.y + m_scrollViewRect.height)
			{
				m_scrollPosition.y = bottom - m_scrollViewRect.height;
				return;
			}
			if (top <= m_scrollPosition.y)
				m_scrollPosition.y = index * ROW_HEIGHT;
		}

		private void DrawNode(Node node, int rowIndex, float maxWidth)
		{
			bool selected = m_selectedNodes.Contains(node);
			bool wasExpanded = node.Expanded;
			bool isExpanded = wasExpanded;
			m_content.text = node.Name;
			Rect rowRect = new Rect(0f, GetTopPixelForRow(rowIndex), maxWidth, ROW_HEIGHT);
			if (!node.HasData)
			{
				Color color = GUI.color;
				GUI.color *= new Color(1f, 1f, 1f, 0.9f);
				GUI.Label(rowRect, GUIContent.none, m_noDataStyle);
				GUI.color = color;
			}

			int nodeControlID = node.GetItemControlID();

			float foldoutIndent = GetFoldoutIndent(node);
			bool isDropTarget = m_dropData != null && m_dropData.dropTargetControlID == nodeControlID;

			Event evt = Event.current;
			if (evt.type == EventType.Repaint)
			{
				if (selected)
					m_selectionStyle.Draw(rowRect, false, false, true, m_hasFocus);
				float contentIndent = GetContentIndent(node);
				rowRect.x += contentIndent;
				rowRect.width -= contentIndent;
				GUIStyle style = node.IsBold ? m_boldLineStyle : (node.Enabled ? m_lineStyle : m_disabledLineStyle);
				style.padding.left = 2;
				Texture2D icon = node.GetIcon(wasExpanded);
				if (icon)
					style.padding.left += 16;
				style.Draw(rowRect, node.Name, false, false, selected, m_hasFocus);
				if (icon)
				{
					Rect pos = rowRect;
					pos.width = ICON_WIDTH;
					pos.height = ROW_HEIGHT;
					GUI.DrawTexture(pos, icon);
				}

				if (isDropTarget)
					lineStyle.Draw(rowRect, GUIContent.none, true, true, false, false);

				if (m_dropData != null && m_dropData.rowMarkerControlID == node.GetItemControlID() && node.CanBeParent)
                {
					float yPos = (drawRowMarkerAbove ? rowRect.y : rowRect.yMax) - m_insertionStyle.fixedHeight * 0.5f;
					m_draggingInsertionMarkerRect = new Rect(rowRect.x, yPos, rowRect.width, rowRect.height);
				}

			}
			if (node.Children.Count > 0 && string.IsNullOrEmpty(Filter))
			{
				bool expanded = GUI.Toggle(new Rect(foldoutIndent, rowRect.y, FOLDOUT_WIDTH, rowRect.height), node.Expanded, GUIContent.none, m_foldoutStyle);
				if (expanded != node.Expanded)
					SetNodeExpanded(node, expanded);
			}

            switch (evt.type)
            {
                case EventType.MouseDown:
                    if (evt.button == 0)
                    {
                        if (rowRect.Contains(evt.mousePosition) && isExpanded == wasExpanded)
                        {
                            if (evt.modifiers.HasFlag(EventModifiers.Control))
                            {
                                if (SelectedNodes.Contains(node))
                                    SelectedNodes.Remove(node);
                                else
                                    SelectedNodes.Add(node);
                            }
                            else if ((evt.modifiers & EventModifiers.Shift) == EventModifiers.Shift)
                            {
                                int nodeIndex = m_visibleNodes.IndexOf(node);
                                int maxIndex = -1;
                                int maxDelta = -1;
                                for (int i = 0; i < SelectedNodes.Count; i++)
                                {
                                    Node n = SelectedNodes[i];
                                    int index = m_visibleNodes.IndexOf(n);
                                    int delta = Mathf.Abs(nodeIndex - index);
                                    if (delta > maxDelta)
                                    {
                                        maxDelta = delta;
                                        maxIndex = index;
                                    }
                                }
                                if (maxIndex != -1)
                                {
                                    int start = maxIndex;
                                    int end = nodeIndex;
                                    if (maxIndex > nodeIndex)
                                    {
                                        start = nodeIndex;
                                        end = maxIndex;
                                    }
                                    List<Node> newSelection = m_visibleNodes.GetRange(start, end - start + 1);
                                    SelectedNodes.ReplaceAll(newSelection);
                                }
                            }
                            else
							{
								if (!SelectedNodes.Contains(node))
									SelectedNodes.ReplaceAll(node);

								m_dragSelection.Clear();
								for (int i = 0; i < SelectedNodes.Count; i++)
								{
									if (SelectedNodes[i].HasData)
										m_dragSelection.Add(SelectedNodes[i]);
								}

								if (node.HasData && m_dragSelection.Count > 0)
								{
									DragAndDropDelay delay = GetStateObject(nodeControlID);
                                    delay.mouseDownPosition = evt.mousePosition;
								}

                                GUIUtility.hotControl = nodeControlID;
                            }
                            evt.Use();
                        }
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == nodeControlID && m_dragSelection.Count > 0)
                    {
                        DragAndDropDelay delay = GetStateObject(nodeControlID);
                        if (delay.CanStartDrag())
                        {
                            StartDrag(node, m_dragSelection);
                            GUIUtility.hotControl = 0;
                        }

                        evt.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == nodeControlID)
                    {
                        GUIUtility.hotControl = 0;
                        m_dragSelection.Clear();
						m_dropResult = null;


						if (rowRect.Contains(evt.mousePosition))
                            SelectedNodes.ReplaceAll(node);
                        else
                            SelectedNodes.Clear();

                        evt.Use();
                    }
                    break;
                case EventType.DragUpdated:
                case EventType.DragPerform:
					if (DragElement(node, rowRect, rowIndex))
                        GUIUtility.hotControl = 0;
                    break;
            }
        }

		private void InitStyles()
		{
			if (m_foldoutStyle != null && m_lineStyle != null && m_boldLineStyle != null && m_selectionStyle != null)
				return;
			m_selectionStyle = new GUIStyle("PR Label");
			m_lineStyle = new GUIStyle("PR Label");
			Texture2D background = m_lineStyle.hover.background;
			m_lineStyle.onNormal.background = background;
			m_lineStyle.onActive.background = background;
			m_lineStyle.onFocused.background = background;
			m_lineStyle.alignment = TextAnchor.MiddleLeft;
			m_boldLineStyle = new GUIStyle(m_lineStyle);
			m_boldLineStyle.font = EditorStyles.boldLabel.font;
			m_boldLineStyle.fontStyle = EditorStyles.boldLabel.fontStyle;
			m_disabledLineStyle = new GUIStyle("PR DisabledLabel");
			m_disabledLineStyle.alignment = TextAnchor.MiddleLeft;
			m_foldoutStyle = new GUIStyle("IN Foldout");
			m_noDataStyle = new GUIStyle("ProjectBrowserTopBarBg");
			m_insertionStyle = new GUIStyle("TV Insertion");
			lineStyle = "SelectionRect";
		}

		private void ProcessKeyboardInput()
		{
			Event evt = Event.current;
			if (!evt.isKey || m_selectedNodes.Count == 0 || evt.type != EventType.KeyDown || GUIUtility.keyboardControl != 0)
				return;
			int selectedIndex = m_visibleNodes.IndexOf(m_selectedNodes[m_selectedNodes.Count - 1]);
			if (selectedIndex == -1 || m_visibleNodes.Count == 0)
			{
				Node node = (m_visibleNodes.Count > 0) ? m_visibleNodes[0] : null;
				SelectedNodes.ReplaceAll(node);
				EnsureVisible(SelectedNodes);
				return;
			}
			else
			{
				bool ensureVisible = true;
				switch (evt.keyCode)
				{
					case KeyCode.UpArrow:
						if (selectedIndex > 0)
							SelectedNodes.ReplaceAll(m_visibleNodes[selectedIndex - 1]);
						evt.Use();
						break;
					case KeyCode.DownArrow:
						if (selectedIndex < m_visibleNodes.Count - 1)
							SelectedNodes.ReplaceAll(m_visibleNodes[selectedIndex + 1]);
						evt.Use();
						break;
					case KeyCode.RightArrow:
						evt.Use();
						ProcessRightArrow(m_visibleNodes);
						break;
					case KeyCode.LeftArrow:
						evt.Use();
						ProcessLeftArrow(m_visibleNodes);
						break;
					case KeyCode.Home:
						SelectedNodes.ReplaceAll(m_visibleNodes[0]);
						evt.Use();
						break;
					case KeyCode.End:
						SelectedNodes.ReplaceAll(m_visibleNodes[m_visibleNodes.Count - 1]);
						evt.Use();
						break;
					case KeyCode.PageUp:
						{
							int numRows = (int)(m_scrollViewRect.height / ROW_HEIGHT);
							int index = Mathf.Max(selectedIndex - numRows, 0);
							SelectedNodes.ReplaceAll(m_visibleNodes[index]);
							evt.Use();
							break;
						}
					case KeyCode.PageDown:
						{
							int numRows2 = (int)(m_scrollViewRect.height / ROW_HEIGHT);
							int index2 = Mathf.Min(selectedIndex + numRows2, m_visibleNodes.Count - 1);
							SelectedNodes.ReplaceAll(m_visibleNodes[index2]);
							evt.Use();
							break;
						}
					case KeyCode.Delete:
						if (SelectedNodesDeleted != null)
							SelectedNodesDeleted();
						evt.Use();
						break;
					default:
						ensureVisible = false;
						break;
				}
				if (ensureVisible)
					EnsureVisible(SelectedNodes);
			}
		}

		private void ProcessLeftArrow(List<Node> flatList)
		{
			if (!string.IsNullOrEmpty(Filter))
				return;
			if (m_selectedNodes.Count != 1)
			{
				for (int i = 0; i < m_selectedNodes.Count; i++)
					SetNodeExpanded(m_selectedNodes[i], false);
				return;
			}
			Node node = m_selectedNodes[0];
			if (node.Expanded)
			{
				SetNodeExpanded(node, false);
				return;
			}
			if (node.Parent != m_root)
			{
				SelectedNodes.ReplaceAll(node.Parent);
				return;
			}
			for (int j = flatList.IndexOf(node) - 1; j >= 0; j--)
			{
				Node k = flatList[j];
				if (k.Children.Count > 0)
				{
					SelectedNodes.ReplaceAll(k);
					return;
				}
			}
		}

		private void ProcessRightArrow(List<Node> flatList)
		{
			if (!string.IsNullOrEmpty(Filter))
				return;
			if (m_selectedNodes.Count == 1)
			{
				Node node = m_selectedNodes[0];
				if (node.Children.Count == 0 || node.Expanded)
				{
					for (int i = flatList.IndexOf(node) + 1; i < flatList.Count; i++)
					{
						Node j = flatList[i];
						if (j.Children.Count > 0)
						{
							SelectedNodes.ReplaceAll(j);
							return;
						}
					}
					return;
				}
				if (node.Children.Count > 0)
				{
					SetNodeExpanded(node, true);
					return;
				}
			}
			else
			{
				for (int k = 0; k < m_selectedNodes.Count; k++)
					SetNodeExpanded(m_selectedNodes[k], true);
			}
		}

		private List<Node> BuildFlatList(List<Node> nodes, bool allNodes = false)
		{
			List<Node> flatList = new List<Node>();
			for (int i = 0; i < nodes.Count; i++)
			{
				Node n = nodes[i];
				bool hasFilter = !string.IsNullOrEmpty(Filter);
				if (allNodes || !hasFilter || n.Name.IndexOf(Filter, StringComparison.OrdinalIgnoreCase) >= 0)
					flatList.Add(n);
				if (allNodes || n.Expanded || hasFilter)
					flatList.AddRange(BuildFlatList(n.Children, false));
			}
			return flatList;
		}

		private void RefreshVisibleNodes()
		{
			if (!m_visibleNodesDirty)
			{
				if (m_visibleNodes == null)
					m_visibleNodes = new List<Node>();
				return;
			}
			m_visibleNodesDirty = false;
			m_visibleNodes = BuildFlatList(m_root.Children, false);
		}

		internal int rowCount
		{
			get
			{
				if (m_visibleNodes == null)
					return 0;
				return m_visibleNodes.Count;
			}
		}

		internal IList<Node> GetRows()
		{
			RefreshVisibleNodes();
			return m_visibleNodes;
		}

		internal Node GetItem(int row)
		{
			return m_visibleNodes[row];
		}

		private float GetFoldoutIndent(Node node)
		{
			if (!string.IsNullOrEmpty(Filter))
				return BASE_INDENT;
			return SPACE_BETWEEN_ICON_AND_TEXT + node.Depth * INDENT_WIDTH;
		}

		internal float GetContentIndent(Node node)
		{
			return GetFoldoutIndent(node) + FOLDOUT_WIDTH;
		}

		private Vector2 GetTotalSize()
		{
			return new Vector2(1f, m_visibleNodes.Count * ROW_HEIGHT + TREE_BOTTOM_MARGIN);
		}

		private void GetFirstLastRowVisible(out int first, out int last)
		{
			first = Mathf.Max(Mathf.FloorToInt(m_scrollPosition.y / ROW_HEIGHT), 0);
			last = first + Mathf.CeilToInt(m_scrollViewRect.height / ROW_HEIGHT);
			last = Mathf.Min(last, m_visibleNodes.Count - 1);
		}

		private float GetTopPixelForRow(int row)
		{
			return row * ROW_HEIGHT;
		}

		public Rect GetRowRect(int row, float rowWidth)
		{
			return new Rect(0, GetTopPixelForRow(row), rowWidth, ROW_HEIGHT);
		}

		private GUIContent GetContent(string text)
		{
			m_tempContent.text = text;
			m_tempContent.tooltip = string.Empty;
			return m_tempContent;
		}

		internal void SetNodeExpanded(Node node, bool expanded)
		{
			m_visibleNodesDirty |= (node.Expanded != expanded);
			node.Expanded = expanded;
		}

		private float m_halfDropBetweenHeight = 4f;

		public float halfDropBetweenHeight
		{
			get
			{
				return m_halfDropBetweenHeight;

			}
		}

		//public class DropChild
  //      {
		//	public Node child;
		//	public short index;
  //      }

		public class DropResult
        {
			public Node parent;
			public List<Node> children;
			public int startIndex;
        }

		protected class DropData
		{
			public int[] expandedArrayBeforeDrag;
			public int lastControlID = -1;
			public int dropTargetControlID = -1;
			public int rowMarkerControlID = -1;
			public double expandItemBeginTimer;
			public Vector2 expandItemBeginPosition;
		}

		public enum DropPosition
		{
			Upon = 0,
			Below = 1,
			Above = 2
		}

		protected DropData m_dropData = new DropData();
		protected DropResult m_dropResult;
		const double k_DropExpandTimeout = 0.7;

		List<Node> m_dragSelection = new List<Node>();
		internal class DragAndDropDelay
		{
			public Vector2 mouseDownPosition;

			public bool CanStartDrag()
			{
				return Vector2.Distance(mouseDownPosition, Event.current.mousePosition) > 6;
			}
		}

		Dictionary<int, DragAndDropDelay> m_StateCache = new Dictionary<int, DragAndDropDelay>();
		DragAndDropDelay GetStateObject(int controlID)
		{
			DragAndDropDelay o;
			if (!m_StateCache.TryGetValue(controlID, out o))
			{
				o = new DragAndDropDelay();
				m_StateCache[controlID] = o;
			}
			return o;

		}

		internal bool CanBeParent(Node node)
		{
			if (node.Parent != null)
				return true;
			return false;
		}

		internal void NotifyListenersThatDragEnded(DropResult result, bool draggedItemsFromOwnTreeView)
		{
			if (DrapNodesEnded != null && draggedItemsFromOwnTreeView && result != null && result.children.Count > 0)
			{
				List<int> children = new List<int>();
				for (int i = 0; i < result.children.Count; i++)
					children.Add(result.children[i].ID);
                DrapNodesEnded(result.parent.ID, result.startIndex, children);
            }
		}

		public bool drawRowMarkerAbove { get; set; }

		protected float GetDropBetweenHalfHeight(Node item, Rect itemRect)
		{
			return CanBeParent(item) ? halfDropBetweenHeight : itemRect.height * 0.5f;
		}

		protected bool TryGetDropPosition(Node item, Rect itemRect, int row, out DropPosition dropPosition)
		{
			Vector2 currentMousePos = Event.current.mousePosition;

			if (itemRect.Contains(currentMousePos))
			{
				float dropBetweenHalfHeight = GetDropBetweenHalfHeight(item, itemRect);
				if (currentMousePos.y >= itemRect.yMax - dropBetweenHalfHeight)
					dropPosition = DropPosition.Below;
				else if (currentMousePos.y <= itemRect.yMin + dropBetweenHalfHeight)
					dropPosition = DropPosition.Above;
				else
					dropPosition = DropPosition.Upon;
				return true;
			}
			else
			{
				// Check overlap with next item (if any)
				float nextOverlap = halfDropBetweenHeight;
				int nextRow = row + 1;
				if (nextRow < rowCount)
				{
					Rect nextRect = GetRowRect(nextRow, itemRect.width);
					bool nextCanBeParent = CanBeParent(GetItem(nextRow));
					if (nextCanBeParent)
						nextOverlap = halfDropBetweenHeight;
					else
						nextOverlap = nextRect.height * 0.5f;
				}
				Rect nextOverlapRect = itemRect;
				nextOverlapRect.y = itemRect.yMax;
				nextOverlapRect.height = nextOverlap;
				if (nextOverlapRect.Contains(currentMousePos))
				{
					dropPosition = DropPosition.Below;
					return true;
				}

				// Check overlap above first item
				if (row == 0)
				{
					Rect overlapUpwards = itemRect;
					overlapUpwards.yMin -= halfDropBetweenHeight;
					overlapUpwards.height = halfDropBetweenHeight;
					if (overlapUpwards.Contains(currentMousePos))
					{
						dropPosition = DropPosition.Above;
						return true;
					}
				}
			}

			dropPosition = DropPosition.Below;
			return false;
		}

		bool DragElement(Node targetItem, Rect targetItemRect, int row)
		{
			bool perform = Event.current.type == EventType.DragPerform;
			// Are we dragging outside any items
			if (targetItem == null)
			{
				// If so clear any drop markers
				if (m_dropData != null)
				{
					m_dropData.dropTargetControlID = 0;
					m_dropData.rowMarkerControlID = 0;
				}

				// And let client decide what happens when dragging outside items

				DragAndDrop.visualMode = DoDrag(null, null, perform, DropPosition.Below);
				if (DragAndDrop.visualMode != DragAndDropVisualMode.None && perform)
					FinalizeDragPerformed(true);

				return false;
			}

			DropPosition dropPosition;
			if (!TryGetDropPosition(targetItem, targetItemRect, row, out dropPosition))
				return false;

			Node parentItem = null;
			switch (dropPosition)
			{
				case DropPosition.Upon:
					{
						// Client must decide what happens when dropping upon: e.g: insert last or first in child list
						parentItem = targetItem;
					}
					break;
				case DropPosition.Below:
					{
						// When hovering between an expanded parent and its first child then make sure we change state to match that
						if (targetItem.Expanded && targetItem.HasChildren)
						{
							parentItem = targetItem;
							targetItem = targetItem.Children[0];
							dropPosition = DropPosition.Above;
						}
						else
						{
							// Drop as next sibling to target
							parentItem = targetItem.Parent;
						}
					}
					break;
				case DropPosition.Above:
					{
						parentItem = targetItem.Parent;
					}
					break;
				default:
					//Assert.IsTrue(false, Constants.UnhandledEnum);
					//GLog.IsTrue(false, "Unhandled enum");
					break;
			}

			DragAndDropVisualMode mode = DragAndDropVisualMode.None;
			if (perform)
			{
				// Try Drop on top of element
				if (dropPosition == DropPosition.Upon)
					mode = DoDrag(targetItem, targetItem, true, dropPosition);

				// Fall back to dropping on parent  (drop between elements)
				if (mode == DragAndDropVisualMode.None && parentItem != null)
				{
					mode = DoDrag(parentItem, targetItem, true, dropPosition);
				}

				// Finalize drop
				if (mode != DragAndDropVisualMode.None)
				{
					FinalizeDragPerformed(false);
				}
				else
				{
					DragCleanup(true);
					
					NotifyListenersThatDragEnded(null, false);
				}
			}
			else // DragUpdate
			{
				if (m_dropData == null)
					m_dropData = new DropData();
				m_dropData.dropTargetControlID = 0;
				m_dropData.rowMarkerControlID = 0;

				int itemControlID = targetItem.GetItemControlID();// TreeViewController.GetItemControlID(targetItem);
				HandleAutoExpansion(itemControlID, targetItem, targetItemRect);

				// Try drop on top of element
				if (dropPosition == DropPosition.Upon)
					mode = DoDrag(targetItem, targetItem, false, dropPosition);

				if (mode != DragAndDropVisualMode.None)
				{
					m_dropData.dropTargetControlID = itemControlID;
					DragAndDrop.visualMode = mode;
				}
				// Fall back to dropping on parent (drop between elements)
				else if (targetItem != null && parentItem != null)
				{
					mode = DoDrag(parentItem, targetItem, false, dropPosition);

					if (mode != DragAndDropVisualMode.None)
					{
						drawRowMarkerAbove = dropPosition == DropPosition.Above;
						m_dropData.rowMarkerControlID = itemControlID;
						DragAndDrop.visualMode = mode;
					}
				}
			}

			Event.current.Use();
			return true;
		}

		void FinalizeDragPerformed(bool revertExpanded)
		{
			DragCleanup(revertExpanded);
            DragAndDrop.AcceptDrag();

            bool draggedItemsFromOwnTreeView = true;
			if (m_dragSelection.Count > 0 && m_dragSelection[0] != null && GetRows().FirstOrDefault(t => t.ID == m_dragSelection[0].ID) == null)
				draggedItemsFromOwnTreeView = false;

			//int[] newSelection = new int[objs.Count];
			//for (int i = 0; i < objs.Count; ++i)
			//{
			//	if (objs[i] == null)
			//		continue;

			//	newSelection[i] = (objs[i].GetInstanceID());
			//}
            NotifyListenersThatDragEnded(m_dropResult, draggedItemsFromOwnTreeView);
            //Undo.SetCurrentGroupName(Constants.UndoActionName);
        }



		/// <summary>
		/// 拖拽停留展开
		/// </summary>
		void HandleAutoExpansion(int itemControlID, Node targetItem, Rect targetItemRect)
		{
			Vector2 currentMousePos = Event.current.mousePosition;

			// Handle auto expansion
			float targetItemIndent = GetContentIndent(targetItem);
			float betweenHalfHeight = GetDropBetweenHalfHeight(targetItem, targetItemRect);
			Rect indentedContentRect = new Rect(targetItemRect.x + targetItemIndent, targetItemRect.y + betweenHalfHeight, targetItemRect.width - targetItemIndent, targetItemRect.height - betweenHalfHeight * 2);
			bool hoveringOverIndentedContent = indentedContentRect.Contains(currentMousePos);

			if (itemControlID != m_dropData.lastControlID || !hoveringOverIndentedContent || m_dropData.expandItemBeginPosition != currentMousePos)
			{
				m_dropData.lastControlID = itemControlID;
				m_dropData.expandItemBeginTimer = Time.realtimeSinceStartup;
				m_dropData.expandItemBeginPosition = currentMousePos;
			}

			bool expandTimerExpired = Time.realtimeSinceStartup - m_dropData.expandItemBeginTimer > k_DropExpandTimeout;
			bool mayExpand = hoveringOverIndentedContent && expandTimerExpired;

			// Auto open folders we are about to drag into
			if (targetItem != null && mayExpand && targetItem.HasChildren && !targetItem.Expanded)
			{
				// Store the expanded array prior to drag so we can revert it with a delay later
				if (m_dropData.expandedArrayBeforeDrag == null)
				{
					List<int> expandedIDs = GetCurrentExpanded();
					m_dropData.expandedArrayBeforeDrag = expandedIDs.ToArray();
				}
				SetNodeExpanded(targetItem, true);
				m_dropData.expandItemBeginTimer = Time.realtimeSinceStartup;
				m_dropData.lastControlID = 0;
			}
		}

		List<int> GetCurrentExpanded()
		{
			var visibleItems = GetRows();
			List<int> expandedIDs = (from item in visibleItems where item.Expanded select item.ID).ToList();
			return expandedIDs;
		}


		void DragCleanup(bool revertExpanded)
		{
			if (m_dropData != null)
			{
				if (m_dropData.expandedArrayBeforeDrag != null && revertExpanded)
				{
					RestoreExpanded(new List<int>(m_dropData.expandedArrayBeforeDrag));
				}
				m_dropData = new DropData();
			}
		}

		// We assume that we can only have expanded items during dragging
		public void RestoreExpanded(List<int> ids)
		{
			var visibleItems = GetRows();
			foreach (Node item in visibleItems)
				SetNodeExpanded(item, ids.Contains(item.ID));
		}

		void StartDrag(Node draggedItem, List<Node> draggedItemIDs)
		{
            DragAndDrop.PrepareStartDrag();

            if ((Event.current.control || Event.current.command) && !draggedItemIDs.Contains(draggedItem))
			{
				draggedItemIDs.Add(draggedItem);
			}

            // Ensure correct order for hierarchy items (to preserve visible order when dropping at new location)
            draggedItemIDs = SortIDsInVisiblityOrder(draggedItemIDs);

			if (!draggedItemIDs.Contains(draggedItem))
				draggedItemIDs = new List<Node> { draggedItem };

			DragAndDrop.objectReferences = null;
			DragAndDrop.paths = new string[0];
            string title;
            if (draggedItemIDs.Count > 1)
                title = "<Multiple>";
            else
				title = draggedItemIDs[0].Name;
            DragAndDrop.StartDrag(title);

            //dataSource.SetupChildParentReferencesIfNeeded();
        }

		public List<Node> SortIDsInVisiblityOrder(IList<Node> nodes)
		{
			if (nodes.Count <= 1)
				return nodes.ToList(); // no sorting needed

			var visibleRows = GetRows();
			List<Node> sorted = new List<Node>();
			for (int i = 0; i < visibleRows.Count; ++i)
			{
				Node node = visibleRows[i];
				for (int j = 0; j < nodes.Count; ++j)
				{
					if (nodes[j] == node)
					{
						sorted.Add(node);
						break;
					}
				}
			}

			// Some rows with selection are collapsed (not visible) so add those to the end
			if (nodes.Count != sorted.Count)
			{
				sorted.AddRange(nodes.Except(sorted));
				if (nodes.Count != sorted.Count)
					Debug.LogError("SortIDsInVisiblityOrder failed: " + nodes.Count + " != " + sorted.Count);
			}

			return sorted;
		}

		DragAndDropVisualMode DoDrag(Node parentItem, Node targetItem, bool perform, DropPosition dropPos)
		{
            // Scene dragging logic
            //DragAndDropVisualMode dragSceneResult = DoDragScenes(parentItem, targetItem, perform, dropPos);
            //if (dragSceneResult != DragAndDropVisualMode.None)
            //{
            //    return dragSceneResult;
            //}

            if (targetItem != null && !IsDropTargetUserModifiable(targetItem, dropPos))
			{
				return DragAndDropVisualMode.Rejected;
			}

			HierarchyDropMode option = HierarchyDropMode.kHierarchyDragNormal;
            var searchActive = !string.IsNullOrEmpty(m_filter); //!string.IsNullOrEmpty(dataSource.searchString);
            if (searchActive)
                option |= HierarchyDropMode.kHierarchySearchActive;

            if (parentItem == null || targetItem == null)
			{
				if (m_root.Children.Count == 0)
					return DragAndDropVisualMode.Rejected;
				Node lastScene = m_root.Children[m_root.Children.Count -1];
				if (lastScene.Name == "DontDestroyOnLoad")
                {
					if (m_root.Children.Count == 1)
						return DragAndDropVisualMode.Rejected;
					lastScene = m_root.Children[m_root.Children.Count - 2];
				}

                option |= HierarchyDropMode.kHierarchyDropUpon;
                return Drop(lastScene, option, perform);
            }

            // Here we are hovering over items

            bool draggingUpon = dropPos == DropPosition.Upon;

            if (searchActive && !draggingUpon)
            {
                return DragAndDropVisualMode.None;
            }

            if (draggingUpon)
            {
                option |= HierarchyDropMode.kHierarchyDropUpon;
            }
            else
            {
                if (dropPos == DropPosition.Above)
                {
                    option |= HierarchyDropMode.kHierarchyDropAbove;
                }
                else
                {
                    option |= HierarchyDropMode.kHierarchyDropBetween;
                }
            }

            bool isDroppingBetweenParentAndFirstChild = parentItem != null && targetItem != parentItem && dropPos == DropPosition.Above && parentItem.Children[0] == targetItem;
            if (isDroppingBetweenParentAndFirstChild)
            {
                option |= HierarchyDropMode.kHierarchyDropAfterParent;
            }

			int gameObjectOrSceneInstanceID = targetItem.ID;// GetDropTargetInstanceID(hierarchyTargetItem, dropPos);
            if (gameObjectOrSceneInstanceID == 0)
                return DragAndDropVisualMode.Rejected;

			//if (perform && SubSceneGUI.IsUsingSubScenes() && !IsValidSubSceneDropTarget(gameObjectOrSceneInstanceID, dropPos, DragAndDrop.objectReferences))
			//    return DragAndDropVisualMode.Rejected;
            return Drop(targetItem, option, perform);
		}
		/*
		private DragAndDropVisualMode DoDragScenes(Node parentItem, Node targetItem, bool perform, DropPosition dropPos)
		{
			// We allow dragging SceneAssets on any game object in the Hierarchy to make it easy to drag in a Scene from
			// the project browser. If dragging on a game object (not a sceneheader) we place the dropped scene
			// below the game object's scene

			// Case: 1
			//List<Scene> scenes = DragAndDrop.GetGenericData(kSceneHeaderDragString) as List<Scene>;
			bool reorderExistingScenes = false;// (scenes != null);

			// Case: 2
			bool insertNewScenes = false;
			//if (!reorderExistingScenes && DragAndDrop.objectReferences.Length > 0)
			//{
			//	int sceneAssetCount = 0;
			//	foreach (var dragged in DragAndDrop.objectReferences)
			//	{
			//		if (dragged is SceneAsset)
			//			sceneAssetCount++;
			//	}
			//	insertNewScenes = (sceneAssetCount == DragAndDrop.objectReferences.Length);
			//}

			// Early out if not case 1 or 2
			if (!reorderExistingScenes && !insertNewScenes)
				return DragAndDropVisualMode.None;

			if (perform)
			{
                List<Scene> scenesToBeMoved = null;
                if (insertNewScenes)
                {
                    List<Scene> insertedScenes = new List<Scene>();
                    foreach (var sceneAsset in DragAndDrop.objectReferences)
                    {
                        string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                        Scene scene = SceneManager.GetSceneByPath(scenePath);
                        if (SceneHierarchy.IsSceneHeaderInHierarchyWindow(scene))
                            m_TreeView.Frame(scene.handle, true, true);
                        else
                        {
                            bool unloaded = Event.current.alt;
                            if (unloaded)
                                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.AdditiveWithoutLoading);
                            else
                                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                            if (SceneHierarchy.IsSceneHeaderInHierarchyWindow(scene))
                                insertedScenes.Add(scene);
                        }
                    }
                    if (targetItem != null)
                        scenesToBeMoved = insertedScenes;

                    // Select added scenes and frame last scene
                    if (insertedScenes.Count > 0)
                    {
                        Selection.instanceIDs = insertedScenes.Select(x => x.handle).ToArray();
                        m_TreeView.Frame(insertedScenes.Last().handle, true, false);
                    }
                }
                else // reorderExistingScenes
                    scenesToBeMoved = scenes;

                if (scenesToBeMoved != null)
                {
                    if (targetItem != null)
					{
						Node dstScene = targetItem.Scene;
						if (dstScene != null)
						{
							if (targetItem != dstScene || dropPos == DropPosition.Upon)
								dropPos = DropPosition.Below;

							if (dropPos == DropPosition.Above)
							{
								//for (int i = 0; i < scenesToBeMoved.Count; i++)
								//    EditorSceneManager.MoveSceneBefore(scenesToBeMoved[i], dstScene);
							}
							else if (dropPos == DropPosition.Below)
							{
								//for (int i = scenesToBeMoved.Count - 1; i >= 0; i--)
								//    EditorSceneManager.MoveSceneAfter(scenesToBeMoved[i], dstScene);
							}
						}
					}
					else
					{
						Scene dstScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
						for (int i = scenesToBeMoved.Count - 1; i >= 0; i--)
							EditorSceneManager.MoveSceneAfter(scenesToBeMoved[i], dstScene);
					}
				}
			}

			return DragAndDropVisualMode.Move;
		}
		*/
		bool IsDropTargetUserModifiable(Node targetItem, DropPosition dropPos)
        {
            switch (dropPos)
            {
                case DropPosition.Upon:
                    if (targetItem.Data != null)
                        return IsUserModifiable(targetItem.Data);
                    break;
                case DropPosition.Below:
                case DropPosition.Above:
					Node targetParent = targetItem.Parent;
                    if (targetParent != null && targetItem.Data != null)
                        return IsUserModifiable(targetItem.Data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dropPos), dropPos, null);
            }

            return true;
        }

        bool IsUserModifiable(IModifiable iModifiable)
        {
			return iModifiable.editable;
        }

		public enum HierarchyDropMode
		{
			kHierarchyDragNormal = 0,
			kHierarchyDropUpon = 1 << 0,
			kHierarchyDropBetween = 1 << 1,
			kHierarchyDropAfterParent = 1 << 2,
			kHierarchySearchActive = 1 << 3,
			kHierarchyDropAbove = 1 << 4
		}

		DragAndDropVisualMode Drop(Node dropTarget, HierarchyDropMode dropMode, bool perform)
		{
			if(CheckDrop != null)
			{
				if (CheckDrop(dropTarget, dropMode))
					return DragAndDropVisualMode.Rejected;
			}
			//if (m_treeType == ConnectionWindow.PAGE_TYPE_HIERARCHY)
			//{
			//	if (dropTarget.IsFirstNode)
			//	{
			//		if (!dropMode.HasFlag(HierarchyDropMode.kHierarchyDropUpon))
			//			return DragAndDropVisualMode.Rejected;
			//	}
			//}
			//else if (m_treeType == ConnectionWindow.PAGE_TYPE_FILES)
			//{
			//	if (dropTarget.CanBeParent)
			//	{
			//		if (!dropMode.HasFlag(HierarchyDropMode.kHierarchyDropUpon))
			//			return DragAndDropVisualMode.Rejected;
			//	}
			//	else
			//		return DragAndDropVisualMode.Rejected;
			//}
			if (perform)
			{
				//GLog.Error(dropTarget.Name + " : " + dropMode);
				if (dropMode.HasFlag(HierarchyDropMode.kHierarchyDropUpon))
				{
					//设置对象在当前对象队尾
					m_dropResult = new DropResult();
					m_dropResult.parent = dropTarget;
					CheckAndUpdateDropResult(dropTarget.Children.Count);
				}
				else if (dropMode.HasFlag(HierarchyDropMode.kHierarchyDropAbove))
				{
					m_dropResult = new DropResult();
					m_dropResult.parent = dropTarget.Parent;
					if (dropMode.HasFlag(HierarchyDropMode.kHierarchyDropAfterParent))
					{
						//设置对象在当前对象和父节点之间
						CheckAndUpdateDropResult(0);
					}
					else
					{
						//设置对象在当前对象前
						int index = m_dropResult.parent.Children.IndexOf(dropTarget);
						CheckAndUpdateDropResult(index);
					}
				}
				else if (dropMode.HasFlag(HierarchyDropMode.kHierarchyDropBetween))
				{
					//设置对象在当前对象下
					m_dropResult = new DropResult();
					m_dropResult.parent = dropTarget.Parent;
					int index = m_dropResult.parent.Children.IndexOf(dropTarget);
					CheckAndUpdateDropResult(index + 1);
				}
				else
                {
					Debug.LogError("Unset DropMode======>" + dropMode);
                }
			}
			return DragAndDropVisualMode.Move;
		}

		void CheckAndUpdateDropResult(int startIndex)
        {
			Node parent = m_dropResult.parent;
			m_dropResult.startIndex = startIndex;
			m_dropResult.children = new List<Node>();
			if (CheckAndUpdateDrop != null)
				CheckAndUpdateDrop(m_dropResult, m_dragSelection);
			//if (m_treeType == ConnectionWindow.PAGE_TYPE_HIERARCHY)
			//{
			//	for (int i = 0; i < m_dragSelection.Count; i++)
			//	{
			//		Node node = m_dragSelection[i];
			//		if (node != null && !node.IsFirstNode && !parent.IsSelfOrChild(node))
			//		{
			//			m_dropResult.children.Add(new DropChild()
			//			{
			//				child = node,
			//				index = (short)startIndex,
			//			});
			//			startIndex++;
			//		}
			//	}
			//}
			//else if (m_treeType == ConnectionWindow.PAGE_TYPE_FILES)
			//{
			//	List<Node> selected = m_dragSelection.FindAll((node) => node != null && node.Parent != parent && !parent.IsSelfOrChild(node));
			//	for (int i = 0; i < selected.Count; i++)
			//	{
			//		Node node = selected[i];
			//		int idx = 0;
			//		for (; idx < selected.Count; idx++)
			//		{
			//			Node n = selected[idx];
			//			if (n != node)
			//			{
			//				if (node.IsSelfOrChild(n))
			//					break;
			//			}
			//		}
			//		if (idx == selected.Count)
			//		{
			//			m_dropResult.children.Add(new DropChild()
			//			{
			//				child = node,
			//			});
			//		}
			//	}
			//}
		}

        void HandleUnusedEvents()
        {
			Event evt = Event.current;
			switch (evt.type)
            {
				case EventType.MouseDown:
					m_hasFocus = m_scrollViewRect.Contains(evt.mousePosition);
					if (m_hasFocus)
					{
						SelectedNodes.Clear();
						m_dragSelection.Clear();
						evt.Use();
					}
					break;
				case EventType.Used:
					m_hasFocus = true;
					break;

				case EventType.DragUpdated:
                    if (m_scrollViewRect.Contains(evt.mousePosition))
                    {
						m_hasFocus = true;
						DragElement(null, new Rect(), -1);
                        //Repaint();
                        evt.Use();
                    }
                    break;

                case EventType.DragPerform:
                    if (m_scrollViewRect.Contains(evt.mousePosition))
                    {
						m_hasFocus = true;
                        DragElement(null, new Rect(), -1);
						m_dragSelection.Clear();
						//Repaint();
						evt.Use();
                    }
                    break;

                case EventType.DragExited:
                    //if (dragging != null)
                    {
                        m_dragSelection.Clear();
                        DragCleanup(true);
                        //Repaint();
                    }
                    break;
            }
        }
    }
}
