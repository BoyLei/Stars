

using System;
using System.Collections.Generic;
using SGF.Module.Framework;
using StarProject.Module;
using StarProjectDef;
using UnityEngine.Events;

namespace StarProject.Service.Business
{
    /// <summary>
    /// 格子管理器
    /// </summary>
    [XLua.LuaCallCSharp]
    public class SlotManager : ServiceModule<SlotManager>
    {
        private string LOG_TAG = "SlotManager";


        /// <summary>
        /// 所有格子  Dictionary<功能, Dictionary<组ID, Dictionary<格子唯一ID, BaseSlot>>>
        /// </summary>
        public Dictionary<E_SlotModule, Dictionary<int, Dictionary<int, BaseSlot>>> AllGroupSlotPair = new Dictionary<E_SlotModule, Dictionary<int, Dictionary<int, BaseSlot>>>();

        /// <summary>
        /// 创建的自增Idx
        /// </summary>
        public int Idx = 0;

        /// <summary>
        /// 初始化格子
        /// </summary>
        public void Init()
        {
            CheckSingleton();
            /*
            
            foreach (var item in E_SlotModule)
            {
                isdedine
            }
            */
            // 背包格子
            Dictionary<int, Dictionary<int, BaseSlot>> BagGroupSlotPair = new Dictionary<int, Dictionary<int, BaseSlot>>();
            AllGroupSlotPair.Add(E_SlotModule.BagModule, BagGroupSlotPair);

            // 装备格子
            Dictionary<int, Dictionary<int, BaseSlot>> EquipGroupSlotPair = new Dictionary<int, Dictionary<int, BaseSlot>>();
            AllGroupSlotPair.Add(E_SlotModule.EquipModule, BagGroupSlotPair);

            // 护符格子
            Dictionary<int, Dictionary<int, BaseSlot>> AmuletGroupSlotPair = new Dictionary<int, Dictionary<int, BaseSlot>>();
            AllGroupSlotPair.Add(E_SlotModule.AmuletModule, BagGroupSlotPair);

            // 药品格子
            Dictionary<int, Dictionary<int, BaseSlot>> MedicineGroupSlotPair = new Dictionary<int, Dictionary<int, BaseSlot>>();
            AllGroupSlotPair.Add(E_SlotModule.MedicineModule, BagGroupSlotPair);

        }

        /// <summary>
        /// 删除格子组
        /// </summary>
        /// <param name="e_SlotModule"></param>
        public void DeleteSlotGroup(E_SlotModule e_SlotModule)
        {
            var groupSlotPair = AllGroupSlotPair[e_SlotModule];
            groupSlotPair.Clear();


           /* //Example
            *//*var a = *//*CreatSlotToGroup(E_SlotModule.AmuletModule).RegOnUpdateCallBacke(SlotUpdateActionType.SelectAction).AddListener(foo);
            
         *//*   a 就是表现层次，或者任意三方要创建逻辑层格子的时候，  逻辑层反问（返回）当我呗通知的时候告诉你哪个方法？*/
        }

        private void foo(int arg0, object[] arg1)
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// 添加到格子组
        /// 组是一个弱概念，没有组id也不会重复，它类似一个【Tag】
        /// 组一定有概念，
        /// </summary>
        /// <param name="e_SlotModule"></param>
        /// <param name="_action"></param>
        /// <param name="groupid"></param>
        /// <param name="size"></param>
        /// <param name="state"></param>
        /// <param name="maxStack"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="slotUpdateActionType"></param>
        /// <returns>【因为已经返回你BaseSlot了，所以你可以自己注册了，创建格子和注册信息是可以一起的】</returns>
        public BaseSlot CreatSlotToGroup(E_SlotModule e_SlotModule, int groupid = 0, int size=1, int state = 1, int maxStack = 0, int posX = 0, int posY = 0/* out 约束绑定事件,创建的时候是否一定约束 绑定*/)
        {
            var groupSlotPair = AllGroupSlotPair[e_SlotModule];
            Idx++;
            switch (e_SlotModule)
            {
                case E_SlotModule.BagModule:

                    //先看缓存有没有这个重复的格子==保险(buyong)

                    //格子new返回一个
                    //组处理
                    BagSlot bagSlot = new BagSlot();
                    bagSlot.Init(Idx, size, state, maxStack, posX, posY);
                    if (!groupSlotPair.ContainsKey(groupid))
                    {
                        groupSlotPair.Add(groupid,new Dictionary<int, BaseSlot>());
                    }

                    groupSlotPair[groupid].Add(Idx, bagSlot);

                    return bagSlot;

                    break;
                case E_SlotModule.EquipModule:

                    EquipSlot equipSlot = new EquipSlot();
                    equipSlot.Init(Idx, size, state, maxStack, posX, posY);
                    if (!groupSlotPair.ContainsKey(groupid))
                    {
                        groupSlotPair.Add(groupid, new Dictionary<int, BaseSlot>());
                    }

                    groupSlotPair[groupid].Add(Idx, equipSlot);

                    return equipSlot;

                    break;
                case E_SlotModule.AmuletModule:

                    AmuletSlot amuletSlot = new AmuletSlot();
                    amuletSlot.Init(Idx, size, state, maxStack, posX, posY);
                    if (!groupSlotPair.ContainsKey(groupid))
                    {
                        groupSlotPair.Add(groupid, new Dictionary<int, BaseSlot>());
                    }

                    groupSlotPair[groupid].Add(Idx, amuletSlot);
                    return amuletSlot;

                    break;
                case E_SlotModule.MedicineModule:

                    break;
                default:
                    break;
            }

            return null;


        }


        //一个表现，对应一个逻辑应该。这么设计是action表现层，对应一个逻辑层，或多个逻辑层。这样适合action由表现层得到中心
        /* public void RegSlotUpdateAction(E_SlotModule e_SlotModule, SlotUpdateActionType actionType, UnityAction<int, object[]> action, int groupid = -1, int id= -1)
         {
             //因为表现层和  我逻辑层是一一对应的，所以表现明确逻辑必须明确
             var groupSlotPair = AllGroupSlotPair[e_SlotModule];



             if (groupid == -1)
             {
                 //更新全部
                 foreach (var groupSlot in groupSlotPair)
                 {
                     foreach (var slot in groupSlot.Value)
                     {
                         slot.Value.RegUpdateAction(actionType, action);
                     }
                 }
             }
             else
             {
                 if (id == -1)
                 {
                     //更新某个组
                     foreach (var slot in groupSlotPair[groupid])
                     {
                         slot.Value.RegUpdateAction(actionType, action);
                     }

                 }
                 else
                 {
                     //更新某条数据
                     groupSlotPair[groupid][id].RegUpdateAction(actionType, action);
                 }
             }


         }*/



        /*【为特定一个格子进行事件注册/控制和清空，修正接口】
         * 【访问事件】
        可以用来！！！启动的注册,
        可以用来！！！追加注册，修改注册外面控制清空 和 +=
            创建格子未必要绑定事件
            
        / <summary>
        / 设置格子组的更新事件

            [表现层想在事件被invoke的时候得到什么样的影响  
            当事件被派发的时候
            方法（写在表现层次）]
        / </summary>
        / <param name="e_SlotModule"></param>
        / <param name="actionName"></param>
        / <param name="action"></param>
        / <param name="groupid"></param>
        / <param name="id"></param>
        */
        public /*UnityAction*/ UnityEvent<int,object[]> RegOnSlotUpdateAction(E_SlotModule e_SlotModule, int groupid /*= -1*/, int id/* = -1*/, SlotUpdateActionType actionType)
        {
            //因为表现层和  我逻辑层是一一对应的，所以表现明确逻辑必须明确
            var groupSlotPair = AllGroupSlotPair[e_SlotModule];


            //枚举中是否注册了
            


            //if (groupid == -1)
            //{
            //    更新全部
            //    foreach (var groupSlot in groupSlotPair)
            //    {
            //        foreach (var slot in groupSlot.Value)
            //        {
            //            slot.Value.RegUpdateAction(actionName, action);
            //        }
            //    }
            //}
            //else
            //{
            //    if (id == -1)
            //    {
            //        更新某个组
            //        foreach (var slot in groupSlotPair[groupid])
            //        {
            //            slot.Value.SetUpdateAction(actionName, action);
            //        }

            //    }
            //    else
            //    {
            //        更新某条数据
            //        groupSlotPair[groupid][id].SetUpdateAction(actionName, action);
            //    }
            //}



            // -1 效果是 一个格子的表现层，需要所有的格子逻辑层次对他发消息

            if(!groupSlotPair.ContainsKey(groupid))
            {
                return null;
            }
            if (!groupSlotPair[groupid].ContainsKey(id))
            {
                return null;
            }

            return groupSlotPair[groupid][id].RegOnUpdateCallBacke(actionType);
        }
        

        /// <summary>
        /// 【刷新组事件】
        /// 本质是一个按钮表现层对【一组/一个系统/某个id】，进行派发通知，并且通知中告知其他需要关注的信息【args】根据SlotUpdateActionType不同而自己设定
        /// 同一个事件，可以允许传递不同的信息机制中是允许的
        /// 比如：红点系统noitce【一组id的回调，但同时你可以传一堆别的信息】“一个action可以公用”
        /// 
        /// 
        /// </summary>
        /// <param name="e_SlotModule">对应系统</param>
        /// <param name="actionType"></param>
        /// <param name="groupid">对应组别  -1就是本系统的全部【组别】</param>
        /// <param name="id">对应id  -1就是本【组】的全部的id</param>
        /// <param name="args">默认为空，你可以拓展;对于单个action的多用这一点比较抽象需要对照使用（发送者，和监听者自己去匹配），【id是唯一不重复的，所以不支持所有组内id为1的进行更新也美这种需求】</param>
        public void CallSlotUpdateAction(E_SlotModule e_SlotModule, SlotUpdateActionType actionType, int groupid = -1,int id = -1,object[] args = null) 
        {

            var groupSlotPair = AllGroupSlotPair[e_SlotModule];

            if(groupid==-1)
            {
                //更新全部
                foreach (var groupSlot in groupSlotPair)
                {
                    foreach (var slot in groupSlot.Value)
                    {
                        slot.Value.CallSlotUpdateAction(actionType,args);
                    }
                }
            }
            else
            {
                if(id==-1)
                {
                    //更新某个组
                    foreach(var slot in groupSlotPair[groupid])
                    {
                        slot.Value.CallSlotUpdateAction(actionType,args);
                    }

                }
                else
                {
                    //更新某条数据
                    groupSlotPair[groupid][id].CallSlotUpdateAction(actionType, args);
                }
                

            }

        }


        public override void Release()
        {
            base.Release();
            Idx = 0;
            //清空数据
            foreach (var groupSlotPair in AllGroupSlotPair)
            {
                groupSlotPair.Value.Clear();
            }
        }
    }

}
