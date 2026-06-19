///--------------------------------------------------------------------
/// 文件名   :   PositionTrigger.cs
/// 内  容   :   
/// 说  明   :  位置触发器
/// 创建日期 :   2023/05/04 18:34:27
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using ClientNpc;
using StarProject.Game;
using StarProject.Game.Player;
using System.Collections;
using System.Collections.Generic;
using Task;
using UnityEngine;
namespace Trigger
{
    public class PositionTrigger : BaseTrigger
    {

        /// <summary>
        /// 触发器类型
        /// </summary>
        public override TrrigerType M_TriggerType => TrrigerType.PositionTrriger;

        /// <summary>
        /// 自身位置
        /// </summary>
        public Vector3 Position
        {
            get
            {
                if (IsMainPlayer && Player!=null)
                {
                    return Player.M_Curr.Position();
                }
                return ClientNpcManager.Instance.GetPosition(Index);
            }
        }

        public Vector3 TriggerPositon { get; private set; }

        /// <summary>
        /// 转向
        /// </summary>
        public float Rotation { get; private set; }

        /// <summary>
        /// 触发形状
        /// </summary>
        public AreaShape areaShape { get; private set; }

        /// <summary>
        /// 多边形点
        /// </summary>
        public List<Vector3> Polygons;

        /// <summary>
        /// 矩形长
        /// </summary>
        public float Length;

        /// <summary>
        /// 矩形宽
        /// </summary>
        public float Width;

        /// <summary>
        /// true 进入触发/ false 退出触发
        /// </summary>
        public bool IsEnterTrigger { get; private set; }

        
        /// <summary>
        /// 触发半径
        /// </summary>
        public float Range { get; private set; }




        /// <summary>
        /// 标记 0 未设置， 1 已设置
        /// </summary>
        private int Flag = 0;

        /// <summary>
        /// 是否已经触发
        /// </summary>
        public bool IsTrigger
        {
            get
            {
                return Flag == 1;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configID">配置ID</param>
        /// <param name="areaShape">触发器形状</param>
        /// <param name="pos">触发位置</param>
        /// <param name="rot">触发朝向</param>
        /// <param name="isenter">进入触发、退出触发</param>
        /// <param name="range">半径</param>
        /// <param name="len">长度</param>
        /// <param name="width">宽度</param>
        /// <param name="polygons">多边形点集合</param>
        /// <param name="count">触发次数</param>
        /// <param name="action">触发事件</param>
        public PositionTrigger(int index,long configID,int areaShape ,Vector3 pos,float rot, bool isenter, float range,float len,float width,List<CustomVector3> polygons, int count,bool isMainPlayer, System.Action<ulong> action) : base(count,isMainPlayer, action)
        {
            this.Index = index;
            this.ConfigID = configID;
            this.areaShape = (AreaShape)areaShape;
            this.TriggerPositon = pos;
            this.Rotation = rot;
            this.Range = range;
            this.Length = len;
            this.Width = width;
            this.Polygons = new List<Vector3>();
            this.Polygons.Clear();
            if(polygons!=null && polygons.Count>0)
            {
                foreach (var item in polygons)
                {
                    this.Polygons.Add(item.Convert());
                }
            }
            this.IsEnterTrigger = isenter;
            OnCreate();
        }



        protected override void OnCreate()
        {
            base.OnCreate();
            Flag = 0;
        }

        private bool InCircle()
        {
            float dis = Vector3.Distance(TriggerPositon, Position);
           return dis <= Range;
        }

        private bool InRectangle()
        {
            float MiniX = TriggerPositon.x - Length / 2;
            float MaxX = TriggerPositon.x + Length / 2;
            float MiniZ = TriggerPositon.z + Width / 2;
            float MaxZ = TriggerPositon.z - Width / 2;
            return Position.x >= MiniX && Position.x <= MaxX && Position.z >= MiniZ && Position.z <= MaxZ;
        }

        private bool InPolygon()
        {           
            if(Polygons==null)
            {
                return false;
            }
            int crossNum = 0;
            int vertexCount = Polygons.Count;

            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 v1 = Polygons[i];
                Vector3 v2 = Polygons[(i + 1) % vertexCount];

                if (((v1.z <= Position.z) && (v2.z > Position.z))
                    || ((v1.z > Position.z) && (v2.z <= Position.z)))
                {
                    if (Position.x < v1.x + (Position.z - v1.z) / (v2.z - v1.z) * (v2.x - v1.x))
                    {
                        crossNum += 1;
                    }
                }
            }

            if (crossNum % 2 == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public override void EnterFrame(int frameIndex)
        {
            if (Player == null)
            {
                return;
            }
            if (!IsValid())
            {
                return;
            }
            bool inner = false;

            if (areaShape == AreaShape.Circle)
            {
                //是否在圆内
                inner = InCircle();
            }
            else if (areaShape == AreaShape.Rectangle)
            {
                //是否在矩形内
                inner = InRectangle();
            }
            else if (areaShape == AreaShape.Polygon)
            {
                //是否在多边形内
                inner = InPolygon();
            }
            if (IsEnterTrigger)
            {
                //进入触发
                if (Flag == 0)
                {
                    //尚未触发
                    if (inner)
                    {
                        Flag = 1;
                        OnTrigger();
                    }
                }
                else
                {
                    //尚未触发
                    if (!inner)
                    {
                        Flag = 0;
                    }
                }
            }
            else
            {
                if (Flag == 0)
                {
                    //尚未触发
                    if (!inner)
                    {
                        Flag = 1;

                        //invoke
                        OnTrigger();
                    }
                }
                else
                {
                    //尚未触发
                    if (inner)
                    {
                        Flag = 0;
                    }
                }
            }
        }
    }
}
