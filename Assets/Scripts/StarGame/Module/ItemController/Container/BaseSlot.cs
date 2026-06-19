using System;
using System.Collections.Generic;
using Google.Protobuf;
using ProtoMsg;
using StarProject.Game.Data;
using StarProject.Service.LocalData;
using StarProjectDef;
using UnityEngine;
using UnityEngine.Events;
using XLua;

namespace StarProject.Module
{


    public enum TipsType
    {
        Item,//道具
        Equip,//装备
        Amulet,//护符
        PartnerEquip,//伙伴装备
        Emblem//纹章
    }


    public enum SlotUpdateActionType
    {
        SelectAction,//选中的事件
        UpdateDataAction,//更新的事件
        //红点相关的阿事件
    }

    


    [XLua.LuaCallCSharp]

    public class BaseSlot : UnityEngine.Pool.GenericPool<BaseSlot>, IDisposable
    {
        /// <summary>
        /// 格子的唯一ID
        /// </summary>
        public int ID;


        //格子静态属性

        public int Size;//格子的大小，类似生化危机4，有长方形，有正方形

        public int MaxStack;//最大堆叠数，0是无限堆叠

        public Vector2Int Pos;//格子的坐标，暂时不用【二维数组的index】

        public int State;//格子的状态，0未开启，1已开启


        //格子动态属性



        public long BaseID;//临时之后删掉（配置表id，可以通过EntityID获得）

        public ulong EntityID;//数据库id--保证装备唯一

        public int Num;//道具数量

        //Tips类型：Tip类型通常是由于格子属性而导致的，物品和数据只是填充，另外不同类型的格子确实可能有相同的tips类型、
        public TipsType TipsType;


        //事件枚举
        //选中事件
        //数据刷新事件
        //.....

        /// <summary>
        /// 更新回调事件
        /// 第三方交互的回调事件/服务器的/某些业务的事件 ----->格子系统的表现层注册的注册给我（UpdateActionDic）----->SlotMgr指定或者分发Call------>对应组/id/系统都可以进行通知
        /// 我一定返回给格子，推到的可以处理表现，其他没推到的也需要处理，所以必须要返回一个id<int>
        /// 但其他也可能是 调用者，需要分发给本组的其他的信息obj
        /// </summary>
        public Dictionary<SlotUpdateActionType, UnityEvent<int,object[]>> UpdateActionDic=new Dictionary<SlotUpdateActionType, UnityEvent<int, object[]>>(); //UnityAction<int, object[]>>();
        //事件event是封装的代理方法，他封装了一个类，他包裹了调用透传(代理)的方式。
        //事件枚举
        //选中事件
        //数据刷新事件

        /*
        表现层 找到mgr的格子 new一个格子开放给表现层（lua能拿到updateAction += fun（））
            public void regViewAction(ref updateAction) 
        {
            updateAction += updateAction;
            }
        */


        //【模拟复选框，和单选框】
        //通常如果一组用相同的约束或者控制关系时用到：例如装备，符文，同一时间只有一个格子可以被选用，选用者通过调用点击时间调用一组格子的Update刷新一组格子的选中状态
        private int groupID;//逻辑层控制显示层（辅助用处）
        // 组idlist  +  组id = 数据list ； 服务器/用户操作Evt == > 本逻辑层 ===> 其他逻辑层操作派发/数据变更/表现层基于格子的数据刷新（通常是组内只允许有一个被选中）
        //其实也在兼容控制TipsType + 被update（被动）刷新所驱动

        //放进去的东西
        private object obj;

        // 托管资源
        private bool disposed = false;


        // 类的析构函数
        ~BaseSlot()
        {
            Dispose(false);
        }

        /// <summary>
        /// 初始化格子
        /// </summary>
        public virtual void Init(int id,int size,int state =1, int maxStack=0,int posX=0,int posY=0)
        {
            ID = id;//格子的唯一ID
            Pos = new Vector2Int(posX, posY);
            Size = size;
            State = state;
            MaxStack = maxStack;
            TipsType = TipsType.Item;
            groupID = 0;
            CancelBind();

        }

        /// <summary>
        /// 把物品放进去
        /// </summary>
        public void PutIn(long baseID, ulong entityID, int num)
        {
            BaseID = baseID;
            EntityID = entityID;
            Num = num;
        }

        /// <summary>
        /// 把物体放进格子
        /// </summary>
        /// <param name="obj"></param>
        public void SetObjectToSlot(object obj)
        {
            this.obj = obj;
        }

        /// <summary>
        /// 获取格子里的物体
        /// </summary>
        public object GetObjectInSlot()
        {
            return obj;
        }
        
 
        /// <summary>
        /// 把物品拿出来(离开格子脱离关联关系)
        /// 去掉绑定
        /// </summary>
        public void CancelBind()
        {
            BaseID = 0;
            EntityID = 0;
            Num = 0;
            groupID = 0;
            UpdateActionDic.Clear();
        }


        /// <summary>
        /// 交换格子内容
        /// </summary>
        public void Swap(BaseSlot other)
        {
            long tempBaseID = this.BaseID;
            this.BaseID = other.BaseID;
            other.BaseID= tempBaseID;

            ulong tempEntityID = this.EntityID;
            this.EntityID = other.EntityID;
            other.EntityID = tempEntityID;

            int tempNum = this.Num;
            this.Num = other.Num;
            other.Num = tempNum;

        }



        /// <summary>
        /// 设置更新回调
        /// </summary>
        /// <param name="updateAction"></param>
        public UnityEvent <int, object[]> RegOnUpdateCallBacke(SlotUpdateActionType actionType)
        {
            if (!UpdateActionDic.ContainsKey(actionType))
            {
                // 外层不决定内层的（给与），创建是有就是有没有就是没有，是根据需求来的，节省空间，根据需求可能有有就有没有要创建
                //return null;
                //是不能决定有很多一致给我，我默认一个，但是我确定一个就好
                //决定创建没问题，决定创建是节省，来源与需求
                //确实没权力驱动我创建吗，有权力开放根据提供的接口来看，自己创建是约束，别人创建是动态
                //第一action他不用new他存储的是地址不用声明直接给他函数就好不传他也有不给也行，你不用new你add的时候这个value就已经有指针容器了，你可以传递给他一个基础闭包，也可以一个都不给交给外界给
                //换成event就要new了
                //如果用action 引用者必须要有左值存储他，引用者已经自己缓存了一个action，已经不用我的了，通过event的封装其实可以让外部直接addlistener进来通过链式调用
                UpdateActionDic.Add(actionType, new UnityEvent<int, object[]>());
            }
            
            return UpdateActionDic[actionType];
            //都创建好了直接返回也行（荣誉），被动需要返回也行（比较好），就是不能给我我是自己持有
        }



        /// <summary>
        /// 设置更新回调
        /// 这个思路是表现层的所有方法给逻辑层list批量执行
        /// 错误1，把一个表现层事件注册给所有格子（表现层还需要分发器）
        /// 误解2，unityaction直接返回去节省很多层中间包装
        /// 错误3，同样的事件其实加不进来
        /// 4遍历也不对，因为只有两个，不遍历也注册不进来
        /// 5，就应该利用dic的方便访问的结构，利用action的list特性
        /// 最终进行派发
        /// </summary>
        /// <param name="updateAction"></param>
    /*    public void RegUpdateAction(SlotUpdateActionType actionType,UnityAction<int, object[]> action)
        {
            if(!UpdateActionDic.ContainsKey(actionType))
            {
                UpdateActionDic.Add(actionType, action);
            }
        }*/

        /// <summary>
        /// 调用更新回调
        /// </summary>
        /// <param name="actionName"></param>
        public void CallSlotUpdateAction(SlotUpdateActionType actionType, object[] args)
        {
            if (UpdateActionDic.ContainsKey(actionType))
            {
                UpdateActionDic[actionType].Invoke(ID, args);
            }
        }



        /// <summary>
        /// 设置组ID
        /// </summary>
        /// <param name="id"></param>
        public void SetGroupID(int id)
        {
            this.groupID = id;
        }



        /// <summary>
        /// 手动调用的Release方法
        /// </summary>

        public void Release()
        {
            Dispose();
        }



        /// <summary>
        /// 只是释放格子
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        // 释放资源的实际方法
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // 释放托管资源
                    obj = null;
                    UpdateActionDic.Clear();
                }

                // 释放非托管资源
                // 使用API或其他方式释放unmanagedResource

                disposed = true;
            }
        }


    }
}
