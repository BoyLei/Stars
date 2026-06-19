#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CokingNodeEditor
{
#if UNITY_EDITOR
    public class CokingNodeEditor : UnityEditor.EditorWindow
    {
        //节点列表
        private Dictionary<int, EmptyNode> nodes = new();
        //头节点
        private EmptyNode headNode = null;
        //当前选择的节点
        public ConnectionPoint SelectingPoint;

        //链接列表
        public List<Connection> Connections = new();

        //节点菜单
        private UnityEditor.GenericMenu _menu;

        //鼠标位置
        private Vector2 mousePosition = Vector2.zero;

        //画布偏移
        private Vector2 gridOffset = Vector2.zero;

        //所有链接点
        public List<ConnectionPoint> connectionPoints = new();

        //一些控件风格
        private GUIStyle style_BackGround;//背景板
        public GUIStyle style_NodeGround;//节点底板
        public GUIStyle style_Point_Open;//打开的连接点
        public GUIStyle style_Point_Close;//关闭的连接点
        private GUIStyle style_Gray;//灰色底板
        private GUIStyle style_Line;//分割线

        //界面数据记录
        public float TitleHeight = 20;
        public float ConfigWeight = 240;

        //界面修改控件
        private ConfigWeightChange configWeightChange;

        //节点树节点列表
        public NodeTreeConfigJson Config = new();

        //页面状态数据
        public bool isRemoveConnectionMode;

        //节点菜单是否展示
        public bool IsOpenMenu = false;

        public UnityEditor.GenericMenu menu
        {
            get
            {
                if (_menu == null)
                {
                    _menu = new UnityEditor.GenericMenu();
                    _menu.AddItem(new GUIContent("添加节点"), false, () =>
                    {
                        AddNode(new EmptyNode(mousePosition, this, false));
                        IsOpenMenu = false;
                    });
                }
                return _menu;
            }
        }

        [UnityEditor.MenuItem("Window/Coking Node Editor")]
        public static void OpenWindow()
        {
            //旧的拿不到，就创建新的
            CokingNodeEditor cokingNodeEditor = GetWindow<CokingNodeEditor>();

            cokingNodeEditor.titleContent = new GUIContent("Coking Node Editor");

            //初始化编辑器风格
            cokingNodeEditor.InitGUIStyle();

            //如果没有头效果就要创建一个
            if (cokingNodeEditor.headNode == null)
            {
                cokingNodeEditor.headNode = new EmptyNode(cokingNodeEditor.position.center, cokingNodeEditor, true);
            }

            //初始化配置宽度修改矩形
            cokingNodeEditor.configWeightChange = new ConfigWeightChange(cokingNodeEditor);

            // 使用SerializedObject和SerializedProperty来显示和编辑自定义类的实例
            cokingNodeEditor.serializedObject = new SerializedObject(cokingNodeEditor);
            cokingNodeEditor.serializedProperty = cokingNodeEditor.serializedObject.FindProperty("Config");
        }

        public void OnGUI()
        {
            //先绘制背景板
            DrawBackGround();

            //绘制网格
            DrawGrid(10, 0.3f, Color.black);
            DrawGrid(100, 0.5f, Color.black);

            //刷新节点和连线
            RefreshConnections();
            RefreshNodes();

            //根据配置绘制标题栏
            DrawTitle();

            //根据配置绘制配置界面
            //先绘制个矩形，后面再加按钮吧
            DrawConfig();

            //监听鼠标事件
            //如果发生了拖拽，需要刷新节点
            ProcessEvents(Event.current);

            //如果有选择的点，需要绘制曲线
            DrawPendingConnection(Event.current);

            if (GUI.changed)
            {
                Repaint();
            }
        }

        #region 触发类的事件

        private void ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    {
                        if (e.button == 1)
                        {
                            if (SelectingPoint != null)
                            {
                                SelectingPoint = null;
                            }
                            else
                            {
                                mousePosition = e.mousePosition;
                                ShowNodeMenu();
                            }
                        }
                        if (e.button == 0)
                        {
                            if (SelectingPoint != null && IsOpenMenu)
                            {
                                SelectingPoint = null;
                            }
                            //左键按下处理顺序：
                            foreach (var connectPoint in connectionPoints)
                            {
                                if (connectPoint.rect.Contains(e.mousePosition))
                                {
                                    if (SelectingPoint == null)
                                    {
                                        SelectingPoint = connectPoint;
                                        return;
                                    }
                                }
                            }
                        }
                        break;
                    }
                //鼠标松开：
                case EventType.MouseUp:
                    {
                        if (e.button == 0)
                        {
                            if (SelectingPoint != null)
                            {
                                ConnectionPoint cp = CokingNodeEditorHelper.CheckInConnectionPoint(connectionPoints, e);
                                if (cp != null)
                                {
                                    //若先前所选的连接点的方向和现在的点不一样，并且不是已存在的线，则可以创建连接
                                    if (SelectingPoint.type != cp.type)
                                    {
                                        //根据自己的类型来决定创建连接时参数的顺序
                                        if (cp.type == ConnectionPointTypeEnum.In)
                                        {
                                            //检查是否存在
                                            foreach (var connection in cp.Connentions)
                                            {
                                                if (connection.outPoint == SelectingPoint)
                                                {
                                                    return;
                                                }
                                            }
                                            AddConnection(cp, SelectingPoint);
                                        }
                                        else
                                        {
                                            //检查是否存在
                                            foreach (var connection in cp.Connentions)
                                            {
                                                if (connection.inPoint == SelectingPoint)
                                                {
                                                    return;
                                                }
                                            }
                                            AddConnection(SelectingPoint, cp);
                                        }

                                        //连接创建结束后将SelectingPoint置为空
                                        SelectingPoint = null;
                                    }
                                }
                                else
                                {
                                    //展示节点菜单
                                    ShowNodeMenu(SelectingPoint, e.mousePosition);
                                }
                            }
                        }
                        break;
                    }
                case EventType.KeyDown:
                    {
                        //按下D进入连线删除模式
                        if (e.keyCode == KeyCode.D)
                        {
                            isRemoveConnectionMode = true;
                            GUI.changed = true;
                        }
                        break;
                    }
                case EventType.KeyUp:
                    {
                        //抬起D退出连线删除模式
                        if (e.keyCode == KeyCode.D)
                        {
                            isRemoveConnectionMode = false;
                            GUI.changed = true;
                        }
                        break;
                    }
                // 处理鼠标滚轮事件
                case EventType.ScrollWheel:
                    {


                        //计算缩放因子
                        float scrollDelta = e.delta.y;
                        float scaleAmount = 1.0f + (scrollDelta * -0.005f);

                        //记录当前缩放因子
                        float oldScale = Config.totalScale;

                        // 更新总的缩放因子
                        Config.totalScale *= scaleAmount;

                        //如果缩放过头了就不缩了，放大也是一样
                        if (Config.totalScale <= 0.4f)
                        {
                            Config.totalScale = 0.4f;
                            break;
                        }
                        if (Config.totalScale >= 8f)
                        {
                            Config.totalScale = 8f;
                            break;
                        }

                        //对头矩形进行缩放
                        headNode.ChangeRectScale(Config.totalScale, oldScale, e.mousePosition);
                        //对每个矩形进行缩放
                        foreach (var node in nodes.Values)
                        {
                            node.ChangeRectScale(Config.totalScale, oldScale, e.mousePosition);
                        }
                        //重绘界面以反映缩放
                        Repaint();
                        break;
                    }
            }
            bool DragHappend = false;

            //处理configWeightChange
            if (configWeightChange != null)
            {
                if (configWeightChange.ProcessEvents(e))
                {
                    DragHappend = true;
                    ConfigWeight += e.delta.x;
                    if (ConfigWeight >= position.width - 100)
                    {
                        ConfigWeight = position.width - 100;
                    }
                    if (ConfigWeight <= 100)
                    {
                        ConfigWeight = 100;
                    }
                }
            }



            //降序处理所有节点
            foreach (var node in nodes.Values)
            {
                //处理每个节点的事件并看是否发生了拖拽
                DragHappend = node.ProcessEvents(e);
                //只能同时拖拽一个节点
                if (DragHappend)
                {
                    break;
                }
            }
            //最后处理头效果，保证头效果正确
            if (headNode != null)
            {
                DragHappend = headNode.ProcessEvents(e) || DragHappend;
            }

            //拖拽画布是最后一层
            switch (e.type)
            {
                //鼠标拖拽：
                case EventType.MouseDrag:
                    {
                        if (e.button == 0 && SelectingPoint == null)
                        {
                            DragAllNodes(e.delta);
                            GUI.changed = true;
                        }
                        break;
                    }
            }
            //只要有拖拽产生，就要刷新GUI
            if (DragHappend)
            {
                GUI.changed = true;
            }
        }

        #endregion

        #region 展示类的事件

        /// <summary>
        /// 展示节点菜单
        /// </summary>
        private void ShowNodeMenu()
        {
            //显示菜单
            menu.ShowAsContext();
            IsOpenMenu = true;
        }

        /// <summary>
        /// 由一个连线展示节点菜单
        /// </summary>
        private void ShowNodeMenu(ConnectionPoint cp, Vector2 pos)
        {
            UnityEditor.GenericMenu createMenu = new();

            createMenu.AddItem(new GUIContent("添加节点"), false, () =>
            {
                AddNode(new EmptyNode(pos, this, false), cp);
                IsOpenMenu = false;
            });

            //显示菜单
            createMenu.ShowAsContext();
            IsOpenMenu = true;
        }

        private void DrawBackGround()
        {
            //矩形范围：
            Rect rect = new(ConfigWeight, TitleHeight, position.width - ConfigWeight, position.height - TitleHeight);
            GUI.Box(rect, "", style_BackGround);
        }


        private SerializedObject serializedObject;
        private SerializedProperty serializedProperty;
        private void DrawConfig()
        {
            Rect configRect = new(0, TitleHeight, ConfigWeight, position.height - TitleHeight);
            GUI.Box(configRect, "", style_Gray);
            //绘制配置分割线
            Rect configLine = new(ConfigWeight - 1, TitleHeight, 1, position.height - TitleHeight);
            GUI.Box(configLine, "", style_Line);
            //绘制配置大小判定
            configWeightChange.Draw();

            //在矩形范围内绘制config
            Rect showConfigRect = new(20, TitleHeight, ConfigWeight - 40, position.height - TitleHeight);
            GUILayout.BeginArea(showConfigRect);
            serializedObject.Update();
            EditorGUILayout.LabelField("节点树配置", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedProperty, true);

            // 应用更改
            serializedObject.ApplyModifiedProperties();
            // 结束布局区域
            GUILayout.EndArea();
        }

        /// <summary>
        /// 刷新节点
        /// </summary>
        public void RefreshNodes()
        {
            if (headNode != null)
            {
                headNode.Draw();
            }
            foreach (var node in nodes.Values)
            {
                node.Draw();
            }
        }

        /// <summary>
        /// 刷新所有连线
        /// </summary>
        private void RefreshConnections()
        {
            for (int i = 0; i < Connections.Count; i++)
            {
                Connections[i].Draw();
            }
        }

        /// <summary>
        /// 绘制待连接线
        /// </summary>
        /// <param name="e"></param>
        private void DrawPendingConnection(Event e)
        {
            if (SelectingPoint != null)//如果已经选择了一个连接点，则画出待连接的线
            {
                //贝塞尔曲线的起点，根据已选则点的方向做判断：
                Vector3 startPosition = (SelectingPoint.type == ConnectionPointTypeEnum.In) ? SelectingPoint.rect.center : e.mousePosition;
                Vector3 endPosition = (SelectingPoint.type == ConnectionPointTypeEnum.In) ? e.mousePosition : SelectingPoint.rect.center;

                UnityEditor.Handles.DrawBezier(     //绘制通过给定切线的起点和终点的纹理化贝塞尔曲线
                startPosition,
                endPosition,
                startPosition + (Vector3.left * 50f), //startTangent	贝塞尔曲线的起始切线。
                endPosition - (Vector3.left * 50f),   //endTangent	贝塞尔曲线的终点切线。
                Color.white,        //color	    要用于贝塞尔曲线的颜色。
                null,               //texture	要用于绘制贝塞尔曲线的纹理。
                2f                  //width	    贝塞尔曲线的宽度。
                );

                GUI.changed = true;
            }
        }

        /// <summary>
        /// 绘制标题
        /// </summary>
        private void DrawTitle()
        {
            Rect titleRect = new(0, 0, position.width, TitleHeight);
            GUI.Box(titleRect, "", style_Gray);
            //绘制标题分割线
            Rect titleLine = new(0, TitleHeight - 1, position.width, 1);
            GUI.Box(titleLine, "", style_Line);
            //绘制按钮
            //计算窗口长度，防止按钮超出窗口
            float buttonWide = 80;
            int buttonNum = 2;
            if (position.width < (buttonWide * buttonNum) + 20)
            {
                buttonWide = (position.width - 20) / buttonNum;
            }
            //读取Json
            //保存Json
        }

        /// <summary>
        /// 拖拽所有节点（拖拽画布）
        /// </summary>
        /// <param name="delta"></param>
        private void DragAllNodes(Vector2 delta)
        {
            headNode.Drag(delta);
            foreach (var node in nodes.Values)
            {
                node.Drag(delta);
            }

        }

        private void InitGUIStyle()
        {
            //底板的风格
            style_BackGround = new GUIStyle();
            style_BackGround.normal.background = UnityEditor.EditorGUIUtility.Load("Assets/DevTools/SkillEditor/Res/BackGround.tga") as Texture2D;
            style_BackGround.border.top = 100;
            style_BackGround.border.left = 100;
            style_BackGround.border.bottom = 100;
            style_BackGround.border.right = 100;

            //节点的风格：
            style_NodeGround = new GUIStyle();
            style_NodeGround.normal.background = UnityEditor.EditorGUIUtility.Load("Assets/DevTools/SkillEditor/Res/NodeGround.tga") as Texture2D;
            style_NodeGround.border.top = 20;
            style_NodeGround.border.bottom = 20;
            style_NodeGround.border.left = 20;
            style_NodeGround.border.right = 20;
            style_NodeGround.alignment = TextAnchor.MiddleCenter;
            style_NodeGround.normal.textColor = Color.white;

            //关闭的连接点的风格：
            style_Point_Close = new GUIStyle();
            style_Point_Close.normal.background = UnityEditor.EditorGUIUtility.Load("Assets/DevTools/SkillEditor/Res/ConnectButtonClose.tga") as Texture2D;
            style_Point_Close.active.background = UnityEditor.EditorGUIUtility.Load("Assets/DevTools/SkillEditor/Res/ConnectButtonClose.tga") as Texture2D;
            style_Point_Close.hover.background = UnityEditor.EditorGUIUtility.Load("Assets/DevTools/SkillEditor/Res/ConnectButtonClose.tga") as Texture2D;

            //开启的连接点的风格：
            style_Point_Open = new GUIStyle();
            style_Point_Open.normal.background = UnityEditor.EditorGUIUtility.Load("Assets/DevTools/SkillEditor/Res/ConnectButtonOpen.tga") as Texture2D;
            style_Point_Open.active.background = UnityEditor.EditorGUIUtility.Load("Assets/DevTools/SkillEditor/Res/ConnectButtonOpen.tga") as Texture2D;
            style_Point_Open.hover.background = UnityEditor.EditorGUIUtility.Load("Assets/DevTools/SkillEditor/Res/ConnectButtonOpen.tga") as Texture2D;

            //其他栏的风格：
            style_Gray = new GUIStyle();
            style_Gray.normal.background = UnityEditor.EditorGUIUtility.Load("Assets/DevTools/SkillEditor/Res/Gray.tga") as Texture2D;

            //分割线的风格：
            style_Line = new GUIStyle();
            style_Line.normal.background = UnityEditor.EditorGUIUtility.Load("Assets/DevTools/SkillEditor/Res/Black.tga") as Texture2D;
        }

        #endregion

        #region 管理类的事件

        /// <summary>
        /// 新增一个节点
        /// </summary>
        /// <param name="emptyNode"></param>
        public void AddNode(EmptyNode emptyNode)
        {
            int id = GetLastNodeID();
            emptyNode.NodeID = id;
            nodes.Add(id, emptyNode);
        }

        /// <summary>
        /// 通过链接点新增一个节点
        /// </summary>
        /// <param name="emptyNode"></param>
        public void AddNode(EmptyNode emptyNode, ConnectionPoint connectionPoint)
        {
            int id = GetLastNodeID();
            emptyNode.NodeID = id;
            nodes.Add(id, emptyNode);
            if (connectionPoint.type == ConnectionPointTypeEnum.In)
            {
                AddConnection(connectionPoint, emptyNode.outPoint);
            }
            else
            {
                AddConnection(emptyNode.inPoint, connectionPoint);
            }
            SelectingPoint = null;
        }

        public void AddConnection(ConnectionPoint inPoint, ConnectionPoint outPoint)
        {
            Connections.Add(new Connection(inPoint, outPoint, this));
        }

        /// <summary>
        /// 绘制画布网格
        /// </summary>
        /// <param name="gridSpacing">格子间距</param>
        /// <param name="gridOpacity">网格线不透明度</param>
        /// <param name="gridColor">网格线颜色</param>
        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            //宽度分段
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            //高度分段
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            UnityEditor.Handles.BeginGUI();//在 3D Handle GUI 内开始一个 2D GUI 块。
            {
                //设置颜色：
                UnityEditor.Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

                //单格的偏移，算是GridOffset的除余
                Vector3 curGridOffset = new(gridOffset.x % gridSpacing, gridOffset.y % gridSpacing, 0);

                //绘制所有的竖线
                for (int i = 0; i < widthDivs; i++)
                {
                    UnityEditor.Handles.DrawLine(
                        new Vector3(gridSpacing * i, 0 - gridSpacing, 0) + curGridOffset,                  //起点
                        new Vector3(gridSpacing * i, position.height + gridSpacing, 0f) + curGridOffset);  //终点
                }
                //绘制所有的横线
                for (int j = 0; j < heightDivs; j++)
                {
                    UnityEditor.Handles.DrawLine(
                        new Vector3(0 - gridSpacing, gridSpacing * j, 0) + curGridOffset,                  //起点
                        new Vector3(position.width + gridSpacing, gridSpacing * j, 0f) + curGridOffset);   //终点
                }

                //重设颜色
                UnityEditor.Handles.color = Color.white;
            }
            UnityEditor.Handles.EndGUI(); //结束一个 2D GUI 块并返回到 3D Handle GUI。
        }

        public int GetLastNodeID()
        {
            int i = 0;
            foreach (var node in nodes.Values)
            {
                if (node.NodeID > i)
                {
                    i = node.NodeID;
                }
            }
            return i + 1;
        }

        public void ProcessRemoveNode(EmptyNode node)
        {
            //收集“待删除连接列表”
            List<Connection> connectionsToRemove = new();

            //遍历所有的连接，若连接的入点或出点是属于要删除的节点的，则将其添加到“待删除连接列表”中
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].inPoint == node.inPoint || Connections[i].outPoint == node.outPoint)
                {
                    connectionsToRemove.Add(Connections[i]);
                }
            }

            //删除“待删除连接列表”中所有连接
            for (int i = 0; i < connectionsToRemove.Count; i++)
            {
                Connections.Remove(connectionsToRemove[i]);

                connectionsToRemove[i].inPoint.Connentions.Remove(connectionsToRemove[i]);
                connectionsToRemove[i].outPoint.Connentions.Remove(connectionsToRemove[i]);
            }

            //移除节点
            nodes.Remove(node.NodeID);
            //移除连接点
            connectionPoints.Remove(node.outPoint);
            connectionPoints.Remove(node.inPoint);
        }

        public void ProcessRemoveConnect(Connection connection)
        {
            Connections.Remove(connection);

            connection.inPoint.Connentions.Remove(connection);
            connection.outPoint.Connentions.Remove(connection);
        }

        #endregion
    }
#endif
}
#endif