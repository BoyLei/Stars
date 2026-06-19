#if UNITY_EDITOR        
using UnityEditor;
using UnityEngine;

namespace CokingNodeEditor
{
    public class Connection
    {
        public ConnectionPoint inPoint;     //进点
        public ConnectionPoint outPoint;    //出点

        public ConnectionType type;

        public CokingNodeEditor OwnerWindow;

        //构造函数
        public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint, CokingNodeEditor ownerWindow)
        {
            this.inPoint = inPoint;
            this.outPoint = outPoint;

            if (outPoint.type == ConnectionPointTypeEnum.TrueOut)
            {
                type = ConnectionType.TrueConnect;
            }
            else if (outPoint.type == ConnectionPointTypeEnum.AlwaysOut)
            {
                type = ConnectionType.AlwaysConnect;
            }
            else if (outPoint.type == ConnectionPointTypeEnum.FalseOut)
            {
                type = ConnectionType.FalseConnect;
            }

            inPoint.Connentions.Add(this);
            outPoint.Connentions.Add(this);

            OwnerWindow = ownerWindow;
        }

        public void Draw()
        {
            if (OwnerWindow.isRemoveConnectionMode)//仅在移除连线模式下才显示移除连线的按钮
            {
                Vector2 buttonSize = new(20, 20);//按钮的尺寸

                //连线的中心，即按钮的位置
                Vector2 LineCenter = (inPoint.rect.center + outPoint.rect.center) / 2;
                //绘制按钮，按下时移除自己
                if (GUI.Button(new Rect(LineCenter - (buttonSize / 2), buttonSize), "X"))
                {
                    OwnerWindow.ProcessRemoveConnect(this);//移除自己
                }
            }

            Handles.DrawBezier(     //绘制通过给定切线的起点和终点的纹理化贝塞尔曲线
            inPoint.rect.center,    //startPosition	贝塞尔曲线的起点。
            outPoint.rect.center,   //endPosition	贝塞尔曲线的终点。
            inPoint.rect.center + (Vector2.left * 50f),   //startTangent	贝塞尔曲线的起始切线。
            outPoint.rect.center - (Vector2.left * 50f),  //endTangent	贝塞尔曲线的终点切线。
            Color.white,        //color	    要用于贝塞尔曲线的颜色。
            null,               //texture	要用于绘制贝塞尔曲线的纹理。
            2f                  //width	    贝塞尔曲线的宽度。
            );
        }
    }
}

#endif