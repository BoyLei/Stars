///--------------------------------------------------------------------
/// 文件名   :   InterActionModule
/// 内  容   :   交互模块
/// 说  明   :  交互条件判断,交互效果实现
/// 创建日期 :   2022/07/21 14:30:38
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using SGF.Time;
using StarProject.Game;
using StarProject.Game.Player;
using StarProject.Service.Business;
using StarProject.Service.DisplayProcess;
using StarProject.Service.LocalData;
using StarProject.Service.ServerService;
using StarProject.Service.User;
using StarProjectDef;
using System;
using System.Collections.Generic;
using SGF.Unity;
using UnityEngine;
using static StarProject.Service.Function.GlobalFunctionManager;

namespace StarProject.Module
{
    public class InterActionModule : BusinessModule
    {
        /// <summary>
        /// 今日提醒
        /// </summary>
        private string NOTICE = "";

        public int last_notice = 0;

        private List<ulong> InRangeObjects = null;

        private Dictionary<string, System.Action<object[]>> InternalMessages = null;

        private ulong playerRoleId;

        public override void Create(object args = null)
        {
            InternalMessages = new Dictionary<string, Action<object[]>>();
            InRangeObjects = new List<ulong>();
            InternalMessages.Add("OnEnterRange", Enter_Range);
            InternalMessages.Add("OnLeaveRange", Leave_Range);
            InternalMessages.Add("QueryInter", Query_Inter);

            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.InterRetID, OnInterRet, this); //增减数据
            MsgRetManager.Instance.OnMessageEnum(OnInterFailRet, MsgIDEnum.LobbyServiceReqID); //增减数据
            GlobalEvent.OnRoleCreateComplete.AddListener(OnRoleCreateComplete);
        }

        private void OnRoleCreateComplete(object arg0)
        {
            playerRoleId = UserManager.Instance.MainUserData.playerRoleId;
            NOTICE = string.Format("today_notice_{0}", playerRoleId);
            if (KeyExists("cost_notice", NOTICE))
            {
                int cost_notice = Load<int>("cost_notice", NOTICE);
                last_notice = cost_notice;
            }
        }


        private void Enter_Range(object[] obj)
        {
            ulong EntityID = Convert.ToUInt64(obj[0]);
            if (!InRangeObjects.Contains(EntityID))
            {
                var entity = GameManager.Instance.GetEntityCtr(EntityID);
                if (entity != null)
                {
                    ObjectCtrlGroup ctrlGroup = entity as ObjectCtrlGroup;
                    if (ctrlGroup != null && ctrlGroup.M_Curr != null)
                    {
                        InRangeObjects.Add(EntityID);
                        
                        /*var cfg=LocalDataManager.Instance.GetInteractDataCell(ctrlGroup.M_Curr.ConfigIndex);
                        if (cfg!= null)
                        {
                            //有消耗
                            if (cfg.GetIsAutoInter()==2)
                            {
                                DelayInvoker.DelayInvoke(2, (a) =>
                                {
                                    if (InRangeObjects.Contains(EntityID))
                                    {
                                        Query_Inter(new object[]{EntityID});
                                    }
                                },null);
                                //(ctrlGroup.M_Curr as Game.Entity.VitalSigns.ObstacleBase)?.ActionModelFesnel(true);
                            }
                        }*/
                        
                        (ctrlGroup.M_Curr as Game.Entity.VitalSigns.ObstacleBase).ActionModelFesnel(true);
                        ModuleManager.Instance.SendMessage(ModuleDef.Name.ObjectInteractiveModule, "OnUpdateService");
                    }
                }

                //(ctrlGroup.M_Curr as Game.Entity.VitalSigns.ObstacleBase).ActionModelInteractiveEff(true);
            }
        }

        private void Leave_Range(object[] obj)
        {
            ulong EntityID = Convert.ToUInt64(obj[0]);
            if (InRangeObjects.Contains(EntityID))
            {
                InRangeObjects.Remove(EntityID);

                var entity = GameManager.Instance.GetEntityCtr(EntityID);
                if (entity != null)
                {
                    ObjectCtrlGroup ctrlGroup = entity as ObjectCtrlGroup;
                    if (ctrlGroup != null && ctrlGroup.M_Curr != null)
                    {
                        (ctrlGroup.M_Curr as Game.Entity.VitalSigns.ObstacleBase)?.ActionModelFesnel(false);
                    }
                    else
                    {
                        SGF.Debuger.LogWarning(
                            $"InterActionModule OnLeaveRange() EntityID={EntityID},ctrlGroup,M_Curr=null");
                    }
                }
                ModuleManager.Instance.SendMessage(ModuleDef.Name.ObjectInteractiveModule, "OnUpdateService");
                //(ctrlGroup.M_Curr as Game.Entity.VitalSigns.ObstacleBase).ActionModelInteractiveEff(false);
            }
        }

        /// <summary>
        /// 当前交互物件ID
        /// </summary>
        /// <returns></returns>
        public List<ulong> InRangeInterActionObjects()
        {
            return InRangeObjects;
        }

        /// <summary>
        /// 更新每日提醒
        /// </summary>
        public void SaveCostNotice()
        {
            if (last_notice != TimeUtils.ServerNow.DayOfYear)
            {
                last_notice = TimeUtils.ServerNow.DayOfYear;
                Save("cost_notice", last_notice, NOTICE);
            }
        }

        /// <summary>
        /// 是否存在每日消耗提醒
        /// </summary>
        /// <returns></returns>
        public bool HasCostNotice()
        {
            int s_cost = TimeUtils.ServerNow.DayOfYear;
            return last_notice != s_cost;
        }

        private void Query_Inter(object[] arg)
        {
            /// long signedValue = (long)arg[0];
            ulong entityID = Convert.ToUInt64(arg[0]);
            bool isTask = false;
            if (arg.Length > 1)
            {
                isTask = Convert.ToBoolean(arg[1]);
            }

            int SourceID = 0;
            if (arg.Length > 2)
            {
                SourceID = Convert.ToInt32(arg[2]);
            }

            int SourceSubID = 0;
            if (arg.Length > 3)
            {
                SourceSubID = Convert.ToInt32(arg[3]);
            }

            //ulong entityID = (ulong)signedValue;
            int configID = (int)GameManager.Instance.GetEntityCfgID(entityID);
            InteractDataCell interactDataCell = LocalDataManager.Instance.GetInteractDataCell(configID);
            if (interactDataCell != null)
            {
                //有消耗
                if (interactDataCell.GetCostID() > 0)
                {
                    BusinessManager.Instance.OpenCostWidget(interactDataCell.GetCostID(),
                        () => { SendServerInter(entityID, configID, isTask, SourceID, SourceSubID); });
                }
                else
                {
                    SendServerInter(entityID, configID, isTask, SourceID, SourceSubID);
                }
            }
            else
            {
                SendServerInter(entityID, configID, isTask, SourceID, SourceSubID);
            }
        }

        private void SendServerInter(ulong entityID, int configID, bool isTask, int SourceID, int SourceSubID)
        {
            if (GameManager.Instance.M_MainPlayerCtrlBase != null)
            {
                PlayerCtrlGroup player = (PlayerCtrlGroup)GameManager.Instance.M_MainPlayerCtrlBase;
                if (player != null)
                {
                    if (player.IsIntering() || player.IsLockInter)
                    {
                        //玩家已经在交互
                        return;
                    }
                }
            }

            //Debug.LogError(entityID);
            var lobbyService = new LobbyServiceReq();
            lobbyService.MapID = GameManager.Instance.GetCurMapId();
            lobbyService.UID = entityID;
            lobbyService.ObjID = (uint)configID;
            if (isTask)
            {
                lobbyService.SourceType = MapServiceSourceTypeEnum.Task;
            }
            else
            {
                lobbyService.SourceType = MapServiceSourceTypeEnum.MstDefault;
            }

            lobbyService.SourceID = SourceID; //任务ID
            lobbyService.SourceSubID = SourceSubID; //任务子ID
            lobbyService.ObjType = ProtoMsg.ServiceObjEnum.ObjInteract;
            NetworkManager.Instance.gameSocket.SendRPCMsg(SGF.Network.ServerType.ServerTypeLobby, lobbyService, false);
            {
                PlayerCtrlGroup player = (PlayerCtrlGroup)GameManager.Instance.M_MainPlayerCtrlBase;
                if (player != null)
                {
                    player.PrepareInteract(entityID);
                }
            }
            /*SocketBase battleSocket = NetworkManager.Instance.gameSocket;
            InterEeq interEeq = new InterEeq();
            interEeq.InterID = entityID;
            interEeq.Baseid = configID;
            battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, interEeq);*/
        }

        private void OnInterFailRet(MsgRetArgs args)
        {
            PlayerCtrlGroup player = (PlayerCtrlGroup)GameManager.Instance.M_MainPlayerCtrlBase;
            if (player != null)
            {
                player.InterFail(0);
            }
        }

        private void OnInterRet(MessageHandleData data)
        {
            InterRet inter = data.data as InterRet;
            if (inter != null)
            {
                int IsSuccess = inter.IsSuccess;
                if (IsSuccess == 0) // 孔磊让我改成0是成功的
                {
                    curstate state = inter.State;
                    ulong interid = inter.Interid;
                    long endtime = inter.Endtime;
                    long delay = endtime - TimeUtils.ServerNowStampMilli;
                    SGF.Debuger.LogWarning($" 交互对象UID={interid} 交互状态 {state}  交互结束时间{endtime}  当前服务器时间{TimeUtils.ServerNowStampMilli}  剩余时间{delay} ");
                    long need = 0;
                    var mInterObjectConfigID = (int)GameManager.Instance.GetEntityCfgID(interid);
                    InteractDataCell interactDataCell = LocalDataManager.Instance.GetInteractDataCell(mInterObjectConfigID);
                    if (interactDataCell != null)
                    {
                        need = interactDataCell.GetMineID();
                    }

                    if (state == curstate.Inting)
                    {
                        if (GlobalEvent.InterEvent != null)
                        {
                            GlobalEvent.InterEvent.Invoke(interid, endtime);
                        }

                        if (need > 0 && GlobalEvent.OnStopIner != null)
                        {
                            GlobalEvent.OnStopIner.Invoke(1, need, true);
                        }
                    }
                    else if (state == curstate.Break || state == curstate.End)
                    {
                        if (GlobalEvent.InterEvent != null)
                        {
                            GlobalEvent.InterEvent.Invoke(0, 0);
                        }

                        if (need > 0 && GlobalEvent.OnStopIner != null)
                        {
                            GlobalEvent.OnStopIner.Invoke(1, need, false);
                        }

                        //if (!interactDataCell.GetCanBreak())
                        //{
                        //    OnInterFailRet(null);
                        //}
                    }

                    var entityBase = (ObjectCtrlGroup)GameManager.Instance.GetEntityCtr(interid);
                    entityBase?.OnInterRet(state);
                }
                else
                {
                    //其他是错误码
                    DisplayProcessDispenser.Instance.AddSpecialMessage($"ErrorCode:{IsSuccess}");
                }
            }
        }

        protected override void OnModuleMessage(string msg, object[] args)
        {
            if (InternalMessages.TryGetValue(msg, out System.Action<object[]> action))
            {
                action?.Invoke(args);
            }
        }


        #region 交互条件模块

        public bool InterActionCondition(int ConfigID, ulong playerUID, int index, ulong interobjectUID)
        {
            InteractDataCell interactDataCell = LocalDataManager.Instance.GetInteractDataCell(ConfigID);

            if (interactDataCell != null)
            {
                var services = ServerServiceManager.Instance.GetServices(ServerServiceType.InterAction, (uint)ConfigID);
                if (services != null && services.Count > 0)
                {
                    bool meta = false;
                    //外部注册服务
                    foreach (var item in services)
                    {
                        meta = meta ||
                               StarProject.Service.Function.GlobalFunctionManager.Instance.ConditionGroupMete(playerUID,
                                   item.ConditionGroupID, index, interobjectUID);
                    }

                    return meta;
                }
                else
                {
                    if (interactDataCell.GetConditionID() == -1)
                    {
                        return false;
                    }

                    if (interactDataCell.GetConditionID() == 0)
                    {
                        return true;
                    }

                    //配置表服务
                    return StarProject.Service.Function.GlobalFunctionManager.Instance.ConditionGroupMete(playerUID,
                        interactDataCell.GetConditionID(), index, interobjectUID);
                }
            }

            return false;
        }

        #endregion

        public override void Release()
        {
            GlobalEvent.OnRoleCreateComplete.RemoveListener(OnRoleCreateComplete);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.InterRetID, OnInterRet, this); //增减数据
            MsgRetManager.Instance.OffMessageEnum(OnInterFailRet, MsgIDEnum.LobbyServiceReqID); //增减数据
            InternalMessages.Clear();
            InRangeObjects.Clear();
        }
    }
}