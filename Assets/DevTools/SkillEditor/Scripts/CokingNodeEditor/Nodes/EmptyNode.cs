#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CokingNodeEditor
{
    public class EmptyNode
    {
        //标识是否是头效果
        public bool IsHead = false;

        //基础大小
        public Vector2 BaseRectSize;
        //节点矩形
        public Rect CurRect;
        //拖拽状态标识
        private bool isDragged = false;
        //从属窗口
        public CokingNodeEditor OwnerWindow;
        //进入点
        public ConnectionPoint inPoint;
        //输出点
        public ConnectionPoint outPoint;


        //节点配置
        public string NodeName;
        //节点类型
        public NodeTypeEnum NodeType;
        //节点ID
        public int NodeID;

        //节点高度
        public int Height = 100;
        //节点宽度
        public int Width = 160;

        public EmptyNode(Vector2 position, CokingNodeEditor onwerWindow, bool isHead)
        {
            BaseRectSize = new Vector2(GetWidth(), GetHeight());
            CurRect = new Rect(position.x, position.y, GetWidth(), GetHeight());
            ChangeRectScale(onwerWindow.Config.totalScale, 1, position);

            this.OwnerWindow = onwerWindow;
            this.IsHead = isHead;

            //创建进和出的连接点
            if (!isHead)
            {
                inPoint = new ConnectionPoint(this, onwerWindow, ConnectionPointTypeEnum.In);
                this.OwnerWindow.connectionPoints.Add(inPoint);
            }

            outPoint = new ConnectionPoint(this, onwerWindow, ConnectionPointTypeEnum.AlwaysOut);
            this.OwnerWindow.connectionPoints.Add(outPoint);
        }

        /// <summary>
        /// 绘制节点
        /// </summary>
        public void Draw()
        {
            if (IsHead)
            {
                GUI.Box(CurRect, NodeID.ToString(), OwnerWindow.style_NodeGround);
            }
            else
            {
                GUI.Box(CurRect, NodeID.ToString(), OwnerWindow.style_NodeGround);
            }

            //头效果没有进点
            if (!IsHead)
            {
                inPoint.Draw();
            }
            outPoint.Draw();
        }

        /// <summary>
        /// 修改节点偏移
        /// </summary>
        /// <param name="delta">偏移值</param>
        public void Drag(Vector2 delta)
        {
            //使自身增加偏移
            CurRect.position += delta;
        }

        //此节点处理事件，返回是否发生拖拽
        public bool ProcessEvents(Event e)
        {
            switch (e.type)
            {
                //鼠标按下：
                case EventType.MouseDown:
                    if (e.button == 0)//按下左键
                    {
                        if (CurRect.Contains(e.mousePosition))
                        {
                            isDragged = true;
                        }

                        GUI.changed = true;
                    }
                    //被选择且鼠标在节点范围内
                    if (e.button == 1)
                    {
                        if (CurRect.Contains(e.mousePosition))
                        {
                            if (!IsHead)
                            {
                                RightMouseMenu();
                            }
                            e.Use();
                        }
                    }
                    break;
                //鼠标松开：
                case EventType.MouseUp:
                    isDragged = false;
                    break;
                //鼠标拖拽：
                case EventType.MouseDrag:
                    if (e.button == 0 && isDragged)
                    {
                        Drag(e.delta);
                        //标识e被处理
                        e.Use();
                        //拖拽了就返回true
                        return true;
                    }
                    break;
            }
            //如果最后没有任何拖拽发生，则返回false
            return false;
        }

        public int GetHeight()
        {
            return Height;
        }
        public int GetWidth()
        {
            return Width;
        }

        private void RightMouseMenu()
        {
            //创建菜单对象
            GenericMenu genericMenu = new();
            //菜单加一项 Remove node，它将调用节点窗口的ProcessRemoveNode函数
            genericMenu.AddItem(new GUIContent("删除节点"), false, () => OwnerWindow.ProcessRemoveNode(this));
            //显示菜单
            genericMenu.ShowAsContext();
        }

        public void ChangeRectScale(float scale, float oldScale, Vector2 pivot)
        {
            // 以鼠标位置为中心进行缩放
            float newWidth = BaseRectSize.x * scale;
            float newHeight = BaseRectSize.y * scale;
            float newX = pivot.x - ((pivot.x - CurRect.x) * scale / oldScale);
            float newY = pivot.y - ((pivot.y - CurRect.y) * scale / oldScale);

            CurRect = new Rect(newX, newY, newWidth, newHeight);
        }
    }
}

#endif