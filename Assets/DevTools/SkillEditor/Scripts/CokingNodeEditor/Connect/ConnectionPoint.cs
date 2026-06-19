#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace CokingNodeEditor
{
    //连接点，是节点两侧用于连线的接点
    public class ConnectionPoint
    {
        //矩形范围
        public Rect rect;
        //种类：是进还是出
        public ConnectionPointTypeEnum type;
        //所归属的节点
        public EmptyNode OwnerNode;
        //所归属的窗口
        public CokingNodeEditor OwnerWindow;
        //相连的节点列表
        public List<Connection> Connentions = new();

        /// <summary>
        /// 初始化函数
        /// </summary>
        /// <param name="ownerNode">所属节点</param>
        /// <param name="ownerWidow">所属窗口</param>
        /// <param name="type">进出类型</param>
        public ConnectionPoint(EmptyNode ownerNode, CokingNodeEditor ownerWidow, ConnectionPointTypeEnum type)
        {
            this.OwnerNode = ownerNode;
            this.type = type;
            this.OwnerWindow = ownerWidow;

            //矩形范围：
            rect = new Rect
                (0,     //X：之后会根据归属节点重新计算
                0,      //Y：之后会根据归属节点重新计算
                40f,    //宽度
                40f);   //高度
        }

        //绘制图形
        public void Draw()
        {
            //连接点的Y值应在所归属的节点的中间：
            rect.y = OwnerNode.CurRect.y + (OwnerNode.CurRect.height * 0.5f) - (rect.height * 0.5f);

            //连接点的X值根据种类区分：
            switch (type)
            {
                case ConnectionPointTypeEnum.In:    //对于进的连接点，绘制在节点的左侧：
                    rect.x = OwnerNode.CurRect.x - rect.width + 20;
                    break;

                case ConnectionPointTypeEnum.AlwaysOut:   //对于出的连接点，绘制在节点的右侧：
                    rect.x = OwnerNode.CurRect.x + OwnerNode.CurRect.width - 20;
                    break;
            }

            rect.width = 40 * OwnerWindow.Config.totalScale;
            rect.height= 40 * OwnerWindow.Config.totalScale;

            if (Connentions.Count > 0)
            {
                GUI.Box(rect, "", OwnerWindow.style_Point_Open);
            }
            else
            {
                GUI.Box(rect, "", OwnerWindow.style_Point_Close);
            }
        }
    }
}
#endif