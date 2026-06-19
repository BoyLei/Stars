#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace CokingNodeEditor
{
    public class ConfigWeightChange
    {
        //从属窗口
        public CokingNodeEditor OwnerWindow;

        //节点矩形
        public Rect Rect;

        //拖拽状态标识
        private bool isDragged = false;

        public ConfigWeightChange(CokingNodeEditor ownerWindow)
        {
            Rect = new Rect(ownerWindow.ConfigWeight - 3, ownerWindow.TitleHeight, 6, ownerWindow.position.height - ownerWindow.TitleHeight);

            this.OwnerWindow = ownerWindow;
        }

        /// <summary>
        /// 绘制节点
        /// </summary>
        public void Draw()
        {
            EditorGUIUtility.AddCursorRect(Rect, MouseCursor.ResizeHorizontal);
            GUI.Box(Rect, "");
        }

        /// <summary>
        /// 修改节点偏移
        /// </summary>
        /// <param name="delta">偏移值</param>
        public void Drag(Vector2 delta)
        {
            //使自身增加偏移
            if (Rect.position.x + delta.x < 100)
            {
                Rect.position = new Vector2(100, Rect.position.y);
            }
            else if (Rect.position.x + delta.x >= OwnerWindow.position.width - 100)
            {
                Rect.position = new Vector2(OwnerWindow.position.width - 100, Rect.position.y);
            }
            else
            {
                Rect.position = new Vector2(Rect.position.x + delta.x, Rect.position.y);
            }
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
                        if (Rect.Contains(e.mousePosition))
                        {
                            isDragged = true;
                            GUI.changed = true;
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
    }
}

#endif