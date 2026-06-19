///--------------------------------------------------------------------
/// 文件名   :   GlobalFunctionManager.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/11/03 10:37:40
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using ClientNpc;
using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject.Game;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Player;
using StarProject.Module;
using StarProject.Service.Business;
using StarProject.Service.DisplayProcess;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProject.Service.SDK;
using StarProject.Service.SystemOpen;
using StarProject.Service.Timeline;
using StarProject.UI.StarWorld;
using StarProjectDef;
using System;
using System.Collections.Generic;
using SGF;
using Task;
using UnityEngine;

/// <summary>
/// 服务器同步的效果
/// </summary>
namespace StarProject.Service.Function
{
    #region 事件效果

    public delegate bool GlobalFunctionEvent(FunctionType type, int EffectID, string args,
        System.Action<object> cb = null, ulong entityID = 0);


    [XLua.CSharpCallLua]
    public class GlobalFunctionManager : ServiceModule<GlobalFunctionManager>
    {
        private Dictionary<FunctionType, GlobalFunctionEvent> FunctionMap = null;
        private Dictionary<int, bool> EffectStatus = new();

        public bool DoFunction(int id, ulong entityID = 0)
        {
            if (LocalData.LocalDataManager.Instance.M_CommonEffectData != null &&
                LocalData.LocalDataManager.Instance.M_CommonEffectData.StaticCMeffectDatas != null)
            {
                if (LocalData.LocalDataManager.Instance.M_CommonEffectData.StaticCMeffectDatas.TryGetValue(id,
                        out var data))
                {
                    System.Text.StringBuilder stringBuilder = new();
                    bool breaking = false;
                    breaking = string.IsNullOrEmpty(data.Args1);

                    if (!breaking)
                    {
                        stringBuilder.Append(data.Args1);
                        breaking = string.IsNullOrEmpty(data.Args2);
                    }

                    if (!breaking)
                    {
                        stringBuilder.Append(",");
                        stringBuilder.Append(data.Args2);
                        breaking = string.IsNullOrEmpty(data.Args3);
                    }

                    if (!breaking)
                    {
                        stringBuilder.Append(",");
                        stringBuilder.Append(data.Args3);
                        breaking = string.IsNullOrEmpty(data.Args4);
                    }

                    if (!breaking)
                    {
                        stringBuilder.Append(",");
                        stringBuilder.Append(data.Args4);
                        breaking = string.IsNullOrEmpty(data.Args5);
                    }

                    if (!breaking)
                    {
                        stringBuilder.Append(",");
                        stringBuilder.Append(data.Args5);
                        breaking = string.IsNullOrEmpty(data.Args6);
                    }

                    if (!breaking)
                    {
                        stringBuilder.Append(",");
                        stringBuilder.Append(data.Args6);
                        breaking = string.IsNullOrEmpty(data.Args7);
                    }

                    if (!breaking)
                    {
                        stringBuilder.Append(",");
                        stringBuilder.Append(data.Args7);
                        breaking = string.IsNullOrEmpty(data.Args8);
                    }

                    if (!breaking)
                    {
                        stringBuilder.Append(",");
                        stringBuilder.Append(data.Args8);
                        breaking = string.IsNullOrEmpty(data.Args9);
                    }

                    if (!breaking)
                    {
                        stringBuilder.Append(",");
                        stringBuilder.Append(data.Args9);
                        breaking = string.IsNullOrEmpty(data.Args10);
                    }

                    if (!breaking)
                    {
                        stringBuilder.Append(",");
                        stringBuilder.Append(data.Args10);
                    }


                    bool execute = DoFunction((FunctionType)data.GetEffecttype(), data.GetID(),
                        stringBuilder.ToString(), (object arg) =>
                        {
                            if (data.NextID != null && data.NextID.Count > 0)
                            {
                                for (int i = 0; i < data.NextID.Count; i++)
                                {
                                    DoFunction(data.NextID[i], entityID);
                                }
                            }
                        }, entityID);

                    return execute;
                }
            }

            return false;
        }

        /// <summary>
        /// 服务器通知执行效果
        /// </summary>
        /// <param name="id"></param>
        private void ExecuteServerCall(int id, ulong EntityID)
        {
            if (LocalData.LocalDataManager.Instance.M_CommonEffectData != null &&
                LocalData.LocalDataManager.Instance.M_CommonEffectData.StaticCMeffectDatas != null)
            {
                if (LocalData.LocalDataManager.Instance.M_CommonEffectData.StaticCMeffectDatas.TryGetValue(id,
                        out var data))
                {
                    System.Text.StringBuilder stringBuilder = new();
                    bool breaking = false;
                    breaking = string.IsNullOrEmpty(data.Args1);

                    if (!breaking)
                    {
                        stringBuilder.Append(data.Args1);
                        stringBuilder.Append(",");
                        breaking = string.IsNullOrEmpty(data.Args2);
                    }

                    if (!breaking)
                    {
                        stringBuilder.Append(data.Args2);
                        stringBuilder.Append(",");
                        breaking = string.IsNullOrEmpty(data.Args3);
                    }

                    if (!breaking)
                    {
                        stringBuilder.Append(data.Args3);
                        stringBuilder.Append(",");
                        breaking = string.IsNullOrEmpty(data.Args4);
                    }

                    if (!breaking)
                    {
                        stringBuilder.Append(data.Args4);
                        stringBuilder.Append(",");
                        breaking = string.IsNullOrEmpty(data.Args5);
                    }

                    if (!breaking)
                    {
                        stringBuilder.Append(data.Args5);
                        stringBuilder.Append(",");
                        breaking = string.IsNullOrEmpty(data.Args6);
                    }

                    if (!breaking)
                    {
                        stringBuilder.Append(data.Args6);
                        stringBuilder.Append(",");
                        breaking = string.IsNullOrEmpty(data.Args7);
                    }

                    if (!breaking)
                    {
                        stringBuilder.Append(data.Args7);
                        stringBuilder.Append(",");
                        breaking = string.IsNullOrEmpty(data.Args8);
                    }

                    if (!breaking)
                    {
                        stringBuilder.Append(data.Args8);
                        stringBuilder.Append(",");
                        breaking = string.IsNullOrEmpty(data.Args9);
                    }

                    if (!breaking)
                    {
                        stringBuilder.Append(data.Args9);
                        stringBuilder.Append(",");
                        breaking = string.IsNullOrEmpty(data.Args10);
                    }

                    if (!breaking)
                    {
                        stringBuilder.Append(data.Args10);
                    }

                    bool execute = DoFunction((FunctionType)data.GetEffecttype(), data.GetID(),
                        stringBuilder.ToString(), (object arg) =>
                        {
                            if (data.GetWaitEnd())
                            {
                                //结束
                                NoticeServerExecute(data.GetID());
                            }
                        }, EntityID);
                }
            }
        }


        private void ExecuteServerCall(ProtoMsg.EffectData data, ulong EntityID)
        {
            System.Text.StringBuilder stringBuilder = new();
            bool breaking = false;
            breaking = string.IsNullOrEmpty(data.Args1);

            if (!breaking)
            {
                stringBuilder.Append(data.Args1);
                breaking = string.IsNullOrEmpty(data.Args2);
            }

            if (!breaking)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(data.Args2);
                breaking = string.IsNullOrEmpty(data.Args3);
            }

            if (!breaking)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(data.Args3);
                breaking = string.IsNullOrEmpty(data.Args4);
            }

            if (!breaking)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(data.Args4);
                breaking = string.IsNullOrEmpty(data.Args5);
            }

            if (!breaking)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(data.Args5);
                breaking = string.IsNullOrEmpty(data.Args6);
            }

            if (!breaking)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(data.Args6);
                breaking = string.IsNullOrEmpty(data.Args7);
            }

            if (!breaking)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(data.Args7);
                breaking = string.IsNullOrEmpty(data.Args8);
            }

            if (!breaking)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(data.Args8);
                breaking = string.IsNullOrEmpty(data.Args9);
            }

            if (!breaking)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(data.Args9);
                breaking = string.IsNullOrEmpty(data.Args10);
            }

            if (!breaking)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(data.Args10);
            }

            if (EffectStatus.ContainsKey(data.ID))
            {
                //StarDebug.LogError($"服务器效果重复{data.ID}");
            }
            else
            {
                EffectStatus.Add(data.ID, data.WaitEnd);
            }

            Debuger.Log("收到服务器效果执行通知" + data.Effecttype + "=======" + data.ToString());
            bool execute = DoFunction((FunctionType)data.Effecttype, data.ID, stringBuilder.ToString(), (object arg) =>
            {
                if (data.WaitEnd)
                {
                    //结束
                    NoticeServerExecute(data.ID);
                }
            }, EntityID);
        }


        public void ExecuteClient(EffectJsonData data, ulong entityID)
        {
            System.Text.StringBuilder stringBuilder = new();
            bool breaking = false;
            breaking = string.IsNullOrEmpty(data.Args1);

            if (!breaking)
            {
                stringBuilder.Append(data.Args1);
                breaking = string.IsNullOrEmpty(data.Args2);
            }

            if (!breaking)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(data.Args2);
                breaking = string.IsNullOrEmpty(data.Args3);
            }

            if (!breaking)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(data.Args3);
                breaking = string.IsNullOrEmpty(data.Args4);
            }

            if (!breaking)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(data.Args4);
                breaking = string.IsNullOrEmpty(data.Args5);
            }

            if (!breaking)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(data.Args5);
                breaking = string.IsNullOrEmpty(data.Args6);
            }

            if (!breaking)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(data.Args6);
                breaking = string.IsNullOrEmpty(data.Args7);
            }

            if (!breaking)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(data.Args7);
                breaking = string.IsNullOrEmpty(data.Args8);
            }

            if (!breaking)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(data.Args8);
                breaking = string.IsNullOrEmpty(data.Args9);
            }

            if (!breaking)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(data.Args9);
                breaking = string.IsNullOrEmpty(data.Args10);
            }

            if (!breaking)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(data.Args10);
            }

            //StarDebug.Log("客户端执行");
            bool execute = DoFunction((FunctionType)data.EffectType, 0, stringBuilder.ToString(), null, entityID);
        }

        /// <summary>
        /// 告诉服务器执行结果
        /// </summary>
        /// <param name="id"></param>
        /// <param name="index"></param>
        private void NoticeServerExecute(int id)
        {
            if (EffectStatus.ContainsKey(id))
            {
                if (!EffectStatus[id])
                {
                    return;
                }
            }

            SocketBase battleSocket = NetworkManager.Instance.gameSocket;
            CurEffOverReq notify = new();
            notify.EffectID = id;
            notify.OptIndex = 0;
            //StarDebug.Log($"通知服务效果选项结束{id}");
            battleSocket.SendRPCMsg(ServerType.ServerTypeSpace, notify, isEncrypt: false);
            if (EffectStatus.ContainsKey(id))
            {
                EffectStatus.Remove(id);
            }
        }

        /// <summary>
        /// 客户端通知关卡行为树启动
        /// </summary>
        private void NoticeServerBevStart()
        {
            SocketBase battleSocket = NetworkManager.Instance.gameSocket;
            ClientNoticeBevStart notice = new();
            battleSocket.SendRPCMsg(ServerType.ServerTypeSpace, notice, isEncrypt: false);
            SGF.Debuger.LogWarning($"新手关新流程 发送行为树开启");
        }

        public bool DoFunction(FunctionType type, int effectid, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            try
            {
                if (FunctionMap.TryGetValue(type, out GlobalFunctionEvent callback))
                {
                    if (callback != null)
                    {
                        return callback.Invoke(type, effectid, args, cb, entityID);
                    }
                    else
                    {
                        OnEffectEndHandler(effectid);
                    }
                }
                else
                {
                    OnEffectEndHandler(effectid);
                }
            }
            catch (Exception e)
            {
                OnEffectEndHandler(effectid);
                Debug.LogError($"执行效果异常，错误信息：{e.Message}");
                throw;
            }

            return false;
        }


        private void InitEventEffect()
        {
            FunctionMap = new Dictionary<FunctionType, GlobalFunctionEvent>();

            FunctionMap.Add(FunctionType.ReleaseSkill, ReleaseSkill); //释放技能
            //FunctionMap.Add(FunctionType.AddBuff, AddBuff);               //添加Buff           服务器实现
            //FunctionMap.Add(FunctionType.RemoveBuff, RemoveBuff);         //移除Buff            服务器实现
            FunctionMap.Add(FunctionType.ReciveTask, ReciveTask);
            FunctionMap.Add(FunctionType.FinishTask, FinishTask); //完成任务           服务器实现
            FunctionMap.Add(FunctionType.EntityLevel, EntityLevel); //进入副本
            FunctionMap.Add(FunctionType.Dialogue, Dialogue); //触发对话
            FunctionMap.Add(FunctionType.PlayBlack, PlayBlack); //播放黑幕
            FunctionMap.Add(FunctionType.PlayTimeline, PlayTimeline); //播放Timeline
            FunctionMap.Add(FunctionType.PlayPlot, PlayPlot); //播放章节
            FunctionMap.Add(FunctionType.PlayImage, PlayImage); //播放剧情图片
            FunctionMap.Add(FunctionType.OpenNpcShop, OpenNpcShop); //打开NPC商店
            FunctionMap.Add(FunctionType.ChangeNpcState, ChangeNpcState); //打开NPC商店
            FunctionMap.Add(FunctionType.ClientSendModuleMsg, ClientSendModuleMsg); //发送客户事件
            FunctionMap.Add(FunctionType.ActiveSceneCam, ActiveSceneCamMsg); //发送客户事件
            FunctionMap.Add(FunctionType.ModifyEntityTempVisibility, ModifyEntityTempVisibility); //修改对象显隐
            FunctionMap.Add(FunctionType.PlayAnimation, PlayAnimation); //播放动画
            FunctionMap.Add(FunctionType.PlayFx, PlayFx); //播放动画
            FunctionMap.Add(FunctionType.TriggerGuide, TriggerGuide); //触发引导
            FunctionMap.Add(FunctionType.ShowMessage, ShowMessage); //飘字显示
            FunctionMap.Add(FunctionType.ChangeSceneObjState, ChangeSceneObjState); //修改场景对象状态
            FunctionMap.Add(FunctionType.NpcMove, OnNpcMove); //Npc移动
            FunctionMap.Add(FunctionType.ScreenImpulse, OnScreenImpulse); //屏幕震动
            FunctionMap.Add(FunctionType.PlayPv, OnPlayPv); //播放音效
            FunctionMap.Add(FunctionType.SystemGuide, OnSystemGuide); //系统引导
            FunctionMap.Add(FunctionType.PartnerDialog, OnPartnerDialog); //伙伴喊话
            FunctionMap.Add(FunctionType.ChanageSceneTransitionData, OnChanageSceneTransitionData); //切图过渡信息配置
            FunctionMap.Add(FunctionType.FakePartnerUI, OnFakePartnerUI); //假伙伴UI
            FunctionMap.Add(FunctionType.Transition, OnTransition); //转场黑屏白屏
            FunctionMap.Add(FunctionType.PartnerJoin, OnPartnerJoin); // 伙伴加入效果
            FunctionMap.Add(FunctionType.ShowMessageBox, OnShowMessageBox);
            FunctionMap.Add(FunctionType.EndGuide, OnEndGuide); //结束引导
            FunctionMap.Add(FunctionType.PlayEffectFormPlayerToPoint, OnPlayEffectFormPlayerToPoint);
            FunctionMap.Add(FunctionType.PartnerLevel, OnPartnerLevel); // 伙伴离开效果
            FunctionMap.Add(FunctionType.ScenePlaysControl, OnScenePlaysControl); // 场景控制
            FunctionMap.Add(FunctionType.ShowEnterDungeonMenu, OnShowEnterDungeonMenu);//进入副本弹窗
            FunctionMap.Add(FunctionType.ShowMindRepair, OnShowMindRepair);//打开心灵修复
            //--服务器通知执行效果
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.UserCurEffNotifyID, OnUserCurEffNotify, this);

            GlobalEvent.OnEffectEnd.AddListener(OnEffectEndHandler);

            GlobalEvent.OnSelectEffect.AddListener(OnSelectEffectHandler);
        }


        private void OnSelectEffectHandler(int EffectID, int Index)
        {
            if (EffectStatus.ContainsKey(EffectID))
            {
                if (!EffectStatus[EffectID])
                {
                    return;
                }
            }

            SocketBase battleSocket = NetworkManager.Instance.gameSocket;
            CurEffOverReq notify = new();
            notify.EffectID = EffectID;
            notify.OptIndex = Index;
            battleSocket.SendRPCMsg(ServerType.ServerTypeSpace, notify, isEncrypt: false);
            //SGF.Debuger.LogWarning($"名字测试 主角创建 通知服务效果选项结束{EffectID} {Index}");
            if (EffectStatus.ContainsKey(EffectID))
            {
                EffectStatus.Remove(EffectID);
            }
        }

        private void OnEffectEndHandler(int EffectID)
        {
            if (EffectStatus.ContainsKey(EffectID))
            {
                if (!EffectStatus[EffectID])
                {
                    return;
                }
            }

            SocketBase battleSocket = NetworkManager.Instance.gameSocket;
            CurEffOverReq notify = new();
            notify.EffectID = EffectID;
            notify.OptIndex = 0;
            battleSocket.SendRPCMsg(ServerType.ServerTypeSpace, notify, isEncrypt: false);
            //SGF.Debuger.LogWarning($"名字测试 主角创建 通知服务效果结束{EffectID}");
            if (EffectStatus.ContainsKey(EffectID))
            {
                EffectStatus.Remove(EffectID);
            }
        }

        private void OnUserCurEffNotify(MessageHandleData data)
        {
            if (data == null)
            {
                return;
            }

            UserCurEffNotify curEffOver = data.data as UserCurEffNotify;
            if (curEffOver == null)
            {
                return;
            }

            //SGF.Debuger.LogError("顺序   当前玩家执行效果通知");

            //优先执行 服务器发过来的数据
            if (curEffOver.Data != null)
            {
                StarDebug.Log(StarDebug.Orange, "EffectID", curEffOver.Data.ID, curEffOver.ObjType, curEffOver.ObjID);
                ExecuteServerCall(curEffOver.Data, curEffOver.ObjID);
            }

            ////执行配置效果
            //if (curEffOver.EffectID != 0)
            //{
            //    //StarDebug.Log(StarDebug.Orange, "EffectID", curEffOver.EffectID);
            //    ExecuteServerCall(curEffOver.EffectID);
            //}
        }

        private bool ReleaseSkill(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.ReleaseSkill)
            {
                return false;
            }

            cb?.Invoke(0);
            return true;
        }

        private bool FinishTask(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.FinishTask)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            string[] temps = args.Split(',');
            if (temps == null || temps.Length < 2)
            {
                return false;
            }

            int eventID = 0;
            int taskID = 0;
            if (!ToInt(temps[0], out taskID))
            {
                return false;
            }

            if (!ToInt(temps[1], out eventID))
            {
                return false;
            }

            TaskHelper.ClientFinishTask(taskID, eventID);
            cb?.Invoke(0);
            return true;
        }

        private bool ReciveTask(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.ReciveTask)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            int taskID = 0;
            if (!ToInt(args, out taskID))
            {
                return false;
            }

            TaskHelper.AcceptTaskReq(taskID);
            cb?.Invoke(0);
            return true;
        }

        private bool EntityLevel(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.EntityLevel)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            int id = 0;
            if (!ToInt(args, out id))
            {
                return false;
            }

            StarWorldModule starWorld =
                (StarWorldModule)ModuleManager.Instance.GetModule(ModuleDef.Name.StarWorldModule);
            // 根据ID判断是那个副本类型
            starWorld.SendFBChangeReq(ChangeReason.Instance, id, SpaceType.SpaceMirror);
            cb?.Invoke(0);
            return true;
        }


        private bool OnFakePartnerUI(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.FakePartnerUI)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            string[] paramArr = args.Split(",");
            if (paramArr.Length != 2)
            {
                return false;
            }

            string partnerID = paramArr[0];
            int isShow = Convert.ToInt32(paramArr[1]);
            StarWorldPage starWorldPage = (StarWorldPage)UIManager.Instance.M_Current_UIPage;
            if (starWorldPage)
            {
                if (isShow == 1)
                {
                    UIManager.Instance.OpenWidgetAsync(UIDef.FakePartnerWidget, null, false, partnerID,
                        starWorldPage.M_DownRightRoot, MainPageCommond.HideNone, true);
                }
                else
                {
                    UIManager.Instance.CloseWidget(UIDef.FakePartnerWidget, starWorldPage.M_DownRightRoot, true);
                }
            }

            return true;
        }

        private bool OnTransition(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.Transition)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            int id = 0;
            if (!ToInt(args, out id))
            {
                return false;
            }

            ModuleManager.Instance.SendMessage(ModuleDef.Name.AvgLuaModule, "OnTransition",
                new object[] { id, EffectID });


            return true;
        }

        private bool OnPlayEffectFormPlayerToPoint(FunctionType type, int effectid, string args, Action<object> cb,
            ulong entityid)
        {
            if (type != FunctionType.PlayEffectFormPlayerToPoint)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            string[] temps = args.Split(',');
            if (temps == null || temps.Length < 4)
            {
                return false;
            }

            string effectPath = temps[0];

            float x = 0;
            if (!ToFloat(temps[1], out x))
            {
                return false;
            }

            float y = 0;
            if (!ToFloat(temps[2], out y))
            {
                return false;
            }

            float z = 0;
            if (!ToFloat(temps[3], out z))
            {
                return false;
            }

            //todo

            return true;
        }

        private bool OnShowEnterDungeonMenu(FunctionType type, int effectid, string args, Action<object> cb,
            ulong entityid)
        {
            if (type != FunctionType.ShowEnterDungeonMenu)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            string[] temps = args.Split(',');
            if (temps == null || temps.Length >1)
            {
                return false;
            }


            int id = 0;
            if (!ToInt(temps[0], out id))
            {
                return false;
            }


            ModuleManager.Instance.SendMessage(ModuleDef.Name.EctypeEntranceModule, "OnShowEnterDungeonMenu", new object[] { id });
            return true;

        }

        private bool OnShowMindRepair(FunctionType type, int effectid, string args, Action<object> cb,
            ulong entityid)
        {
            if (type != FunctionType.ShowMindRepair)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            string[] temps = args.Split(',');
            if (temps == null || temps.Length > 1)
            {
                return false;
            }


            int id = 0;
            if (!ToInt(temps[0], out id))
            {
                return false;
            }


            ModuleManager.Instance.SendMessage(ModuleDef.Name.MindRepairModule, "OnOpenMindRepairWindow", new object[] { id });
            return true;

        }

        private bool OnScenePlaysControl(FunctionType type, int effectid, string args, Action<object> cb,
            ulong entityid)
        {
            if (type != FunctionType.ScenePlaysControl)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            string[] temps = args.Split(',');
            if (temps == null || temps.Length < 2)
            {
                return false;
            }


            int id = 0;
            if (!ToInt(temps[0], out id))
            {
                return false;
            }

            bool open = false;
            if (!ToInt(temps[1], out var state))
            {
                return false;
            }

            open = state == 1;

            ModuleManager.Instance.SendMessage(ModuleDef.Name.ScenePlayModule, "OnSetPlayState", id, open);
            return true;
        }

        private bool OnEndGuide(FunctionType type, int effectid, string args, Action<object> cb, ulong entityid)
        {
            if (type != FunctionType.EndGuide)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            int id = 0;
            if (!ToInt(args, out id))
            {
                return false;
            }

            var module = ModuleManager.Instance.GetModule(ModuleDef.Name.TutorialModule) as TutorialModule;
            if (module != null)
            {
                module.EndGuide(id);
            }

            return true;
        }

        private bool OnShowMessageBox(FunctionType type, int effectid, string args, Action<object> cb, ulong entityid)
        {
            if (type != FunctionType.ShowMessageBox)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            int id = 0;
            if (!ToInt(args, out id))
            {
                return false;
            }

            UIAPI.ShowMsgBox(id, (eventName) =>
            {
                if (eventName == "SURE")
                {
                    OnEffectEndHandler(effectid);
                }
            });
            return true;
        }


        private bool OnPartnerJoin(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.PartnerJoin)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            //int cfgID = 0;
            //if (!ToInt(args, out cfgID))
            //{
            //    return false;
            //}

            //var comEffCfg = LocalDataManager.Instance.GetCMeffectDataCell(cfgID);
            //if (comEffCfg == null)
            //{
            //    return false;
            //}

            int partnerID = 0;
            if (!ToInt(args, out partnerID))
            {
                return false;
            }

            GameManager.Instance.EventPreNewPlayerEvent($"4_{EffectID}");

            SGF.UI.Framework.UIManager.Instance.OpenWidgetAsync(UIDef.PartnerJoinWidget, null, true, partnerID, null,
                MainPageCommond.HideNone, true, true);

            return true;
        }

        private bool OnPartnerLevel(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.PartnerLevel)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            int partnerID = 0;
            if (!ToInt(args, out partnerID))
            {
                return false;
            }

            var partnerCfg = LocalDataManager.Instance.GetPartnerDataCell(partnerID);
            if (partnerCfg == null)
            {
                return false;
            }

            var color = ColorDefine.Instance.GetColor($"Partner_Head_{partnerCfg.GetQuality()}");

            string tips =
                $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{partnerCfg.Name}</color> {LanguageManager.Instance.GetLanguageByKey("Local_Str_LeaveTeam")}";
            DisplayProcessDispenser.Instance.AddPartnerLevelMessage(tips);

            return true;
        }


        private bool Dialogue(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.Dialogue)
            {

                Debuger.LogWarning($"类型不匹配 {type}");
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                Debuger.LogWarning($" 参数异常 {args}");
                return false;
            }

            int id = 0;
            if (!ToInt(args, out id))
            {
                Debuger.LogWarning($" 参数异常 {id}");
                return false;
            }
            Debuger.LogWarning($" 开始对话 {id}");
            
            var cfg = LocalDataManager.Instance.GetCommonDialogDataCell(id);
            if (cfg != null)
            {
                GameManager.Instance.EventPreNewPlayerEvent($"1_{cfg.GetDialog_type()}_{id}_1");
            }
            ModuleManager.Instance.SendMessage(ModuleDef.Name.AvgLuaModule, "OnOpenAvg",
                new object[] { id, EffectID, entityID });
            Debuger.LogWarning($" 执行对话 {id} 又没成功我并不知道了");
            return true;
        }


        private bool OnSceneControlFlag(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.SceneControlFlag)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            string[] temps = args.Split(',');
            if (temps == null || temps.Length < 2)
            {
                return false;
            }

            int kg = 0;
            if (!ToInt(temps[0], out kg))
            {
                return false;
            }

            int effectId = 0;
            if (!ToInt(temps[1], out effectId))
            {
                return false;
            }

            GameManager.Instance.SetScreenUIEffect(effectId, kg);


            return true;
        }


        private bool OnScreenImpulse(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.ScreenImpulse)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            var path = args;
            Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(path, (go) =>
            {
                if (go != null)
                {
                    var impulse = go.GetComponent<Cinemachine.CinemachineImpulseSource>();
                    if (impulse != null)
                    {
                        impulse.GenerateImpulseWithForce(1);
                        DelayInvoker.DelayInvoke(this, impulse.m_ImpulseDefinition.m_ImpulseDuration,
                            (args) => { GameObject.Destroy(go); });
                    }
                    else
                    {
                        //StarDebug.LogError("Cinemachine.CinemachineImpulseSource is null" + path);
                    }
                }
            });

            return true;
        }


        private bool OnPlayPv(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.PlayPv)
            {
                return false;
            }

            var cfg = LocalDataManager.Instance.GetPvListDataCell(Convert.ToInt32(args));
            if (cfg == null)
            {
                return false;
            }

            PlayVideoWidgetArgs data = new();
            data.PvCfgID = cfg.GetID();
            data.EffectID = EffectID;
            switch (LanguageManager.Instance.CurLanguageType)
            {
                case LanguageType.Chinese:
                    {
                        data.Path = cfg.PvPath;
                    }
                    break;
                case LanguageType.English:
                    {
                        data.Path = cfg.PvPathEN;
                    }
                    break;
            }

            data.SoundEventName = cfg.SoundEventName;
            data.IsShowSkipBtn = cfg.IsSkip;
            data.IsInBalack = cfg.IsInBalack;
            data.IsOutBlack = cfg.IsOutBlack;


            data.CallBack = null;

            SGF.UI.Framework.UIManager.Instance.OpenWidgetAsync(UIDef.PlayVideoWidget, null, true, data, null,
                MainPageCommond.HideNone, true, true);

            return true;
        }

        private bool OnSystemGuide(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.SystemGuide)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            int id = 0;
            if (!ToInt(args, out id))
            {
                return false;
            }
            //GMCommand.Instance.DoGuide(id);

            GameManager.Instance.EventPreNewPlayerEvent($"14_{EffectID}_{id}");

            TaskHelper.DoGuide(id);

            return true;
        }

        private bool OnPartnerDialog(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.PartnerDialog)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            string[] temps = args.Split(',');
            if (temps == null || temps.Length < 2)
            {
                return false;
            }

            int index = 0;
            if (!ToInt(temps[0], out index))
            {
                return false;
            }

            int dialogID = 0;
            if (!ToInt(temps[1], out dialogID))
            {
                return false;
            }

            int partnerID = 0;
            if (temps.Length > 2 && !ToInt(temps[2], out partnerID))
            {
                return false;
            }

            GlobalEvent.OnPartnerDialog?.Invoke(index, dialogID, partnerID);
            return true;
        }

        private bool OnChanageSceneTransitionData(FunctionType type, int EffectID, string args, Action<object> cb,
            ulong entityID = 0)
        {
            if (type != FunctionType.ChanageSceneTransitionData)
            {
                return false;
            }

            SGF.Debuger.LogWarning($"新手关新流程 预加载={SGF.Time.TimeUtils.ServerNow} args={args} ");

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            string[] temps = args.Split(',');
            if (temps == null || temps.Length < 3)
            {
                return false;
            }

            int TransitionType = 0;
            if (!ToInt(temps[0], out TransitionType))
            {
                return false;
            }

            int SceneBaseDataID = 0;
            if (!ToInt(temps[1], out SceneBaseDataID))
            {
                return false;
            }

            int PVCfgID = 0;
            if (!ToInt(temps[2], out PVCfgID))
            {
                return false;
            }

            LoadSceneType loadSceneType = LoadSceneType.Default;
            if (TransitionType == 1)
            {
                var cfg = LocalDataManager.Instance.GetPvListDataCell(PVCfgID);
                if (cfg == null)
                {
                    return false;
                }

                PlayVideoWidgetArgs data = new();
                data.PvCfgID = cfg.GetID();
                data.EffectID = EffectID;
                switch (LanguageManager.Instance.CurLanguageType)
                {
                    case LanguageType.Chinese:
                        {
                            data.Path = cfg.PvPath;
                        }
                        break;
                    case LanguageType.English:
                        {
                            data.Path = cfg.PvPathEN;
                        }
                        break;
                }

                data.SoundEventName = cfg.SoundEventName;
                data.IsShowSkipBtn = cfg.IsSkip;
                data.IsInBalack = cfg.IsInBalack;
                data.IsOutBlack = cfg.IsOutBlack;
                data.CallBack = NoticeServerBevStart;
                
                SGF.UI.Framework.UIManager.Instance.OpenQueueWidget(UIDef.PlayVideoWidget, null, false, data, null,
                    MainPageCommond.HideNone, true, true);
                //GlobalEvent.onSceneLoaded.RemoveListener(OnSceneLoadSuccess);
                void OnSceneLoadSuccess(string MapSceneName, bool succeed)
                {

                    GlobalEvent.onSceneLoaded.RemoveListener(OnSceneLoadSuccess);
                }
                GlobalEvent.onSceneLoaded.AddListener(OnSceneLoadSuccess);
                loadSceneType = LoadSceneType.PreLoadPV;
                SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 播放PV={cfg.PvPath}");
            }
            else
            {
                loadSceneType = LoadSceneType.PreLoadFadeWhite;
                SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 播放闪白 ");
            }

            var baseMapCfg = LocalDataManager.Instance.GetMapBaseDataCell(SceneBaseDataID);
            if (baseMapCfg == null)
            {
                return false;
            }

            if (loadSceneType == LoadSceneType.PreLoadFadeWhite)
            {
                //return true;
            }

            StarScenesManager.Instance.SetPreLoadScene(baseMapCfg.MapName, loadSceneType);

            return true;
        }

        private bool OnNpcMove(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.NpcMove)
            {
                return false;
            }

            string[] temps = args.Split(',');
            if (temps == null || temps.Length < 4)
            {
                return false;
            }

            int mapID = 0;
            if (!ToInt(temps[0], out mapID))
            {
                return false;
            }

            int npcIndex = 0;
            if (!ToInt(temps[1], out npcIndex))
            {
                return false;
            }

            int movetype = 0;
            if (!ToInt(temps[2], out movetype))
            {
                return false;
            }

            int arg = 0;
            if (!ToInt(temps[3], out arg))
            {
                return false;
            }

            ClientNpcManager.Instance.NpcMove(mapID, npcIndex, movetype, arg, EffectID);
            return true;
        }


        private bool ChangeSceneObjState(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.ChangeSceneObjState)
            {
                return false;
            }

            string[] temps = args.Split(',');
            if (temps == null || temps.Length < 2)
            {
                return false;
            }

            int id = 0;
            if (!ToInt(temps[0], out id))
            {
                return false;
            }

            int state = 0;
            if (!ToInt(temps[1], out state))
            {
                return false;
            }

            var objects = GameObject.FindObjectsByType<SceneObjectState>(FindObjectsSortMode.None);
            if (objects != null)
            {
                foreach (var scene in objects)
                {
                    if (scene != null && scene.ID == id)
                    {
                        scene.StateEnum = (SObjStateEnum)state;
                        scene.SetAnimation();
                        break;
                    }
                }
            }

            // TaskHelper.PlayBlackMovie(id, EffectID);
            return true;
        }

        private bool PlayBlack(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.PlayBlack)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            int id = 0;
            if (!ToInt(args, out id))
            {
                return false;
            }

            TaskHelper.PlayBlackMovie(id, EffectID);
            return true;
        }

        private bool PlayTimeline(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.PlayTimeline)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            int id = 0;
            if (ToInt(args, out id))
            {
                //SGF.Debuger.LogError($"禁止 主角移动 timeline id={id}, EffectID={EffectID}");
                /*
                var cfg = LocalData.LocalDataManager.Instance.GetTimelineConfigDataCell(id);
                if (cfg != null)
                {
                    if (cfg.IsSkip || cfg.IsInBalack || cfg.IsOutBlack)
                    {
                        Action<object> widgetCloseCB = (effectid) =>
                        {
                            UIManager.Instance.ClosePlayTimelineBlackWidget?.Invoke();
                            cb?.Invoke(EffectID);
                        };

                        PlayTimelineBlackWidgetArgs playTimelineBlackWidgetArgs = new();
                        playTimelineBlackWidgetArgs.TimelineConfigData = cfg;
                        playTimelineBlackWidgetArgs.CallBack = () =>
                        {
                            TimelineManager.Instance.PlayTimeline(id, EffectID, widgetCloseCB);
                        };

                        SGF.UI.Framework.UIManager.Instance.OpenWidgetAsync(UIDef.PlayTimelineBlackWidget, null, true,
                            playTimelineBlackWidgetArgs, null, MainPageCommond.HideNone, true, true);
                    }
                    else
                    {
                        Action<object> widgetCloseCB = (effectid) => { cb?.Invoke(EffectID); };
                        TimelineManager.Instance.PlayTimeline(id, EffectID, widgetCloseCB);
                    }
                }
                else
                {
                    Action<object> widgetCloseCB = (effectid) => { cb?.Invoke(EffectID); };
                    TimelineManager.Instance.PlayTimeline(id, EffectID, widgetCloseCB);
                }
                */
                Action<object> widgetCloseCB = (effectid) => { cb?.Invoke(EffectID); };
                TimelineManager.Instance.PlayTimeline(id, EffectID, widgetCloseCB);
                return true;
            }

            //NoticeServerExecute(EffectID);
            return false;
        }

        private bool PlayPlot(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.PlayPlot)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            int id = 0;
            if (!ToInt(args, out id))
            {
                return false;
            }

            TaskHelper.PlayPlot(id, EffectID);
            return true;
        }

        private bool TriggerGuide(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.TriggerGuide)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            int id = 0;
            if (!ToInt(args, out id))
            {
                return false;
            }

            var module = ModuleManager.Instance.GetModule(ModuleDef.Name.TutorialModule) as TutorialModule;
            if (module == null)
            {
                return false;
            }

            module.TestTutorial(id);
            return true;
        }


        private bool ShowMessage(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.ShowMessage)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            int id = 0;
            if (!ToInt(args, out id))
            {
                return false;
            }

            StarProjectDef.CRetMsgDataCell retM =
                StarProject.Service.LocalData.LocalDataManager.Instance.GetCRetMsgDataCell(id);
            if (retM != null)
            {
                Frame.Util.ShowMessage(retM.Note, retM.GetMsgType());
            }

            return true;
        }

        private bool PlayImage(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.PlayImage)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            int id = 0;
            if (!ToInt(args, out id))
            {
                return false;
            }

            TaskHelper.PlayImage(id, EffectID);
            return true;
        }


        private bool OpenNpcShop(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.OpenNpcShop)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            int id = 0;
            if (!ToInt(args, out id))
            {
                return false;
            }

            ModuleManager.Instance.SendMessage(ModuleDef.Name.ShopModule, "OnOpenNpcShop", new object[] { id });
            return true;
        }

        private bool ChangeNpcState(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.ChangeNpcState)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            string[] temps = args.Split(',');
            if (temps == null || temps.Length < 2)
            {
                return false;
            }

            int index = 0;
            if (!ToInt(temps[0], out index))
            {
                return false;
            }

            int state = 0;
            if (!ToInt(temps[1], out state))
            {
                return false;
            }

            ClientNpc.ClientNpcManager.Instance.TranslateState(index, (ClientNpc.StateEnum)state);
            return true;
        }

        private bool ClientSendModuleMsg(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.ClientSendModuleMsg)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            string[] temps = args.Split(',');
            if (temps == null || temps.Length < 2)
            {
                return false;
            }

            string ModuleName = temps[0];
            string EventName = temps[1];
            if (string.IsNullOrEmpty(ModuleName))
            {
                return false;
            }

            if (string.IsNullOrEmpty(EventName))
            {
                return false;
            }

            ModuleManager.Instance.SendMessage(ModuleDef.GetModuleName(ModuleName), EventName);

            return true;
        }


        private bool ActiveSceneCamMsg(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            //StarDebug.Log("ActiveSceneCamMsg");
            if (type != FunctionType.ActiveSceneCam)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            int index = 0;
            if (ToInt(args, out index))
            {
                GlobalEvent.OnActiveSceneVirtualCamera.Invoke(index, EffectID);
            }

            GameManager.Instance.EventPreNewPlayerEvent($"7_{EffectID}");

            return true;
        }


        private bool ModifyEntityTempVisibility(FunctionType type, int EffectID, string args,
            System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.ModifyEntityTempVisibility)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            string[] temps = args.Split(',');
            if (temps == null || temps.Length < 3)
            {
                return false;
            }


            var e_type = 0;
            if (!ToInt(temps[0], out e_type))
            {
                return false;
            }

            int ConfigID = 0;
            if (!ToInt(temps[1], out ConfigID))
            {
                return false;
            }

            bool active = temps[2] == "1";
            TaskEntityType EntityType = (TaskEntityType)e_type;
            E_EntityType E_type = E_EntityType.None;

            if (EntityType == TaskEntityType.Npc)
            {
                E_type = E_EntityType.Npc;
            }
            else if (EntityType == TaskEntityType.Obj)
            {
                E_type = E_EntityType.Interact;
            }

            GlobalEvent.OnEntityTempVisibityChange?.Invoke(E_type, ConfigID, active);
            return true;
        }


        private bool PlayAnimation(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.PlayAnimation)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            string[] temps = args.Split(',');
            if (temps == null || temps.Length < 4)
            {
                return false;
            }


            var e_type = 0;
            if (!ToInt(temps[0], out e_type))
            {
                return false;
            }

            int ConfigID = 0;
            if (!ToInt(temps[1], out ConfigID))
            {
                return false;
            }

            string AnimationName = temps[2];
            if (string.IsNullOrEmpty(AnimationName))
            {
                return false;
            }

            int duration = 0;
            if (!ToInt(temps[3], out duration))
            {
                return false;
            }

            TaskEntityType EntityType = (TaskEntityType)e_type;
            //E_EntityType E_type = E_EntityType.None;

            if (EntityType == TaskEntityType.Npc)
            {
                var npcEntity = GameManager.Instance.GetNPCCtrlGroupByConfig(ConfigID);
                if (npcEntity != null && npcEntity.M_Curr != null)
                {
                    if (npcEntity.M_Curr.M_eSubState == E_ULayerSubState.Idle)
                    {
                        var animParam =
                            npcEntity.M_Curr.GetAnimParamByState(E_ULayerSubState.Performance, AnimationName);
                        npcEntity.M_Curr.ChangeState(GameKeyCommand.Performance, animParam, false);

                        DelayInvoker.DelayInvoke(duration / 1000.0f, (args) =>
                        {
                            if (npcEntity != null && npcEntity.M_Curr != null)
                            {
                                if (npcEntity.M_Curr.M_eSubState == E_ULayerSubState.Performance)
                                {
                                    var animParam = npcEntity.M_Curr.GetAnimParamByState(E_ULayerSubState.Idle);
                                    npcEntity.M_Curr.ChangeState(GameKeyCommand.Idle, animParam, false);
                                }
                            }
                        }, null);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool PlayFx(FunctionType type, int EffectID, string args, System.Action<object> cb = null,
            ulong entityID = 0)
        {
            if (type != FunctionType.PlayFx)
            {
                return false;
            }

            if (string.IsNullOrEmpty(args))
            {
                return false;
            }

            string[] temps = args.Split(',');
            if (temps == null || temps.Length < 4)
            {
                return false;
            }


            var e_type = 0;
            if (!ToInt(temps[0], out e_type))
            {
                return false;
            }

            int ConfigID = 0;
            if (!ToInt(temps[1], out ConfigID))
            {
                return false;
            }

            string FxName = temps[2];
            if (string.IsNullOrEmpty(FxName))
            {
                return false;
            }

            int duration = 0;
            if (!ToInt(temps[3], out duration))
            {
                return false;
            }

            TaskEntityType EntityType = (TaskEntityType)e_type;
            //E_EntityType E_type = E_EntityType.None;

            if (EntityType == TaskEntityType.Npc)
            {
                var npcEntity = GameManager.Instance.GetNPCCtrlGroupByConfig(ConfigID);
                if (npcEntity != null && npcEntity.M_Curr != null)
                {
                    if (npcEntity.M_Curr.M_eSubState == E_ULayerSubState.Idle)
                    {
                        //特效
                        FxParam fxParam = new();
                        fxParam.InitWithLogic(FxName);
                        npcEntity.M_Curr.PlaySpecialEffect(fxParam);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool ToFloat(string arg, out float v)
        {
            return System.Single.TryParse(arg, out v);
        }

        private bool ToInt(string arg, out int v)
        {
            return System.Int32.TryParse(arg, out v);
        }

        #endregion

        #region 条件模块

        /// <summary>
        /// 交互条件判断
        /// </summary>
        /// <param name="playerUid"></param>
        /// <param name="interobjectUid"></param>
        /// <param name="ConfigID"></param>
        /// <returns></returns>
        public delegate bool
            InterActionConditionDelegate(ulong playerUid, ConditionData dataCell, params object[] args);

        private Dictionary<ConditionType, InterActionConditionDelegate> Conditions = null;

        private int ConditionGroupID;
        private Dictionary<int, Dictionary<int, List<ConditionData>>> ConditionCfs = null;

        private void InitCondition()
        {
            ConditionGroupID = 1000000;
            //初始化交互条件
            Conditions = new Dictionary<ConditionType, InterActionConditionDelegate>();
            ConditionCfs = new Dictionary<int, Dictionary<int, List<ConditionData>>>();
            Conditions.Add(ConditionType.LevelLimit, OnLevelLmit);
            Conditions.Add(ConditionType.TaskIsFinish, OnTaskIsFinish);
            Conditions.Add(ConditionType.TaskIsRunning, OnTaskIsRuning);
            Conditions.Add(ConditionType.TaskEventIsFinish, OnTaskEventIsFinish);
            Conditions.Add(ConditionType.TaskIsCommit, OnTaskIsCommit);
            Conditions.Add(ConditionType.ItemLimit, OnItemLimt);
            Conditions.Add(ConditionType.TeamCheck, OnTeamCheck);
            Conditions.Add(ConditionType.GuildLevelLimit, OnGuildLevelLimit);
            Conditions.Add(ConditionType.TreasureActived, OnTreasureActived);
            Conditions.Add(ConditionType.ServerOpendDay, OnServerOpendDay);
            Conditions.Add(ConditionType.FinishLevel, OnFinishLevel);
            Conditions.Add(ConditionType.RiskLevelLimit, OnRiskLevelLimit);
            Conditions.Add(ConditionType.PlayerJob, OnPlayerJob);
            Conditions.Add(ConditionType.TeamLevelLimt, OnTeamLevelLimt);
            Conditions.Add(ConditionType.GuidExisted, OnGuidExisted);
            Conditions.Add(ConditionType.CheckBuff, OnCheckBuff);
            Conditions.Add(ConditionType.WantedAccepted, OnWantedAccepted);
            Conditions.Add(ConditionType.EquipIntensify, OnEquipIntensify);
            Conditions.Add(ConditionType.PartnerCount, OnPartnerCount);
            Conditions.Add(ConditionType.AmuletPolishing, OnAmuletPolishing);
            Conditions.Add(ConditionType.EquipQualityCount, OnEquipQualityCount);
            Conditions.Add(ConditionType.JobSkillLevelCount, OnJobSkillLevelCount);
            Conditions.Add(ConditionType.PersonDailyPass, OnPersonDailyPass);
            Conditions.Add(ConditionType.TeamDailyPass, OnTeamDailyPass);
            Conditions.Add(ConditionType.AppointSystemOpen, OnAppointSystemOpen);
            Conditions.Add(ConditionType.CheckBlackBoard, OnCheckBlackBoard);
            Conditions.Add(ConditionType.PowerModuleCheck, OnPowerModuleCheck);
            Conditions.Add(ConditionType.SkillDotTotal, OnSkillDotTotal);
            Conditions.Add(ConditionType.AdventureCheck, OnAdventureCheck);
            if (LocalData.LocalDataManager.Instance.M_CommonConditionData != null &&
                LocalData.LocalDataManager.Instance.M_CommonConditionData.StaticCMconditionDatas != null &&
                LocalData.LocalDataManager.Instance.M_CommonConditionData.StaticCMconditionDatas.Count > 0)
            {
                foreach (var item in LocalData.LocalDataManager.Instance.M_CommonConditionData.StaticCMconditionDatas)
                {
                    if (ConditionCfs.ContainsKey(item.Value.GetGroupID()))
                    {
                        if (ConditionCfs[item.Value.GetGroupID()].ContainsKey(item.Value.GetSubGroupID()))
                        {
                            ConditionCfs[item.Value.GetGroupID()][item.Value.GetSubGroupID()]
                                .Add(new ConditionData(item.Value));
                        }
                        else
                        {
                            var list = new List<ConditionData>();
                            list.Add(new ConditionData(item.Value));
                            ConditionCfs[item.Value.GetGroupID()].Add(item.Value.GetSubGroupID(), list);
                        }
                    }
                    else
                    {
                        var list = new List<ConditionData>();
                        list.Add(new ConditionData(item.Value));

                        var dic = new Dictionary<int, List<ConditionData>>();
                        dic.Add(item.Value.GetSubGroupID(), list);

                        ConditionCfs.Add(item.Value.GetGroupID(), dic);
                    }
                }
            }
        }

        private bool OnTaskIsCommit(ulong playerUid, ConditionData dataCell, object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            TaskCommitCondition taskCommit = dataCell.ConditionCfg as TaskCommitCondition;
            if (taskCommit == null)
            {
                return false;
            }

            bool result = TaskHelper.TaskIsCommit(taskCommit.TaskID);
            return taskCommit.Flag ? !result : result;
        }

        private bool OnTaskEventIsFinish(ulong playerUid, ConditionData dataCell, object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            TaskEventFinishCondition finishCondition = dataCell.ConditionCfg as TaskEventFinishCondition;
            if (finishCondition == null)
            {
                return false;
            }

            bool result = TaskHelper.IsFinishTaskEvent(finishCondition.TaskID, finishCondition.EventID);
            return finishCondition.Flag ? !result : result;
        }

        private bool OnTaskIsRuning(ulong playerUid, ConditionData dataCell, object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            TaskRunningCondition taskRunning = dataCell.ConditionCfg as TaskRunningCondition;
            if (taskRunning == null)
            {
                return false;
            }

            bool result = TaskHelper.ContainTask(taskRunning.TaskID);
            return taskRunning.Flag ? !result : result;
        }

        private bool OnTaskIsFinish(ulong playerUid, ConditionData dataCell, object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            TaskFinishCondition taskFinish = dataCell.ConditionCfg as TaskFinishCondition;
            if (taskFinish == null)
            {
                return false;
            }

            bool result = TaskHelper.IsTaskFinsh(taskFinish.TaskID);
            return taskFinish.Flag ? !result : result;
        }

        //服务器判断，客户端判断不了
        private bool OnTeamCheck(ulong playerUID, ConditionData dataCell, params object[] args)
        {
            return true;
        }

        private bool OnGuildLevelLimit(ulong playerUID, ConditionData dataCell, params object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            GuildLevelLimitCondition level = dataCell.ConditionCfg as GuildLevelLimitCondition;

            if (level == null)
            {
                return false;
            }

            int guildLv = BusinessManager.Instance.GetGuildLevel();
            var result = guildLv >= level.Level;
            return level.Flag ? !result : result;
        }

        private bool OnTreasureActived(ulong playerUID, ConditionData dataCell, params object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            if (args == null)
            {
                return false;
            }

            if (args.Length < 1)
            {
                return false;
            }

            int index = System.Convert.ToInt32(args[0]);
            TreasureActivedCondition treasureActivedCondition = dataCell.ConditionCfg as TreasureActivedCondition;

            if (treasureActivedCondition == null)
            {
                return false;
            }

            treasureActivedCondition.Index = index;
            if (!BusinessManager.Instance.IsTreasuring())
            {
                return false;
            }

            var data = BusinessManager.Instance.GetTreasureData();
            if (data == null)
            {
                return false;
            }

            var result = data.TreasureInterID == treasureActivedCondition.Index;
            var result2 = dataCell.ConditionCfg.Flag ? !result : result;
            return result2;
        }

        //判断开服时间
        private bool OnServerOpendDay(ulong playerUID, ConditionData dataCell, params object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            ServerOpendDayCondition day = dataCell.ConditionCfg as ServerOpendDayCondition;

            if (day == null)
            {
                return false;
            }

            if (GameManager.Instance.GloInfoSrv != null)
            {
                bool result = GameManager.Instance.GloInfoSrv.GRisk.OffDay >= day.Day;
                return day.Flag ? !result : result;
            }


            return false;
        }

        //判断个人秘境层数(需要曲子取)
        private bool OnFinishLevel(ulong playerUID, ConditionData dataCell, params object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            FinishLevelCondition level = dataCell.ConditionCfg as FinishLevelCondition;

            if (level == null)
            {
                return false;
            }

            int MyLevel = GameManager.Instance.SeasonPassFloor; //这里要用曲哥的人秘境层数

            bool result = MyLevel >= level.Index;
            return level.Flag ? !result : result;
        }

        private bool OnPlayerJob(ulong playerUID, ConditionData dataCell, params object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            PlayerJobCondition job = dataCell.ConditionCfg as PlayerJobCondition;

            if (job == null)
            {
                return false;
            }

            uint playerJob = GameManager.Instance.GetPlayerJob();
            bool result = playerJob == job.Job;
            return job.Flag ? !result : result;
        }


        private bool OnGuidExisted(ulong playerUID, ConditionData dataCell, params object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }


            return BusinessManager.Instance.GetGuildID() > 0;
        }


        private bool OnTeamLevelLimt(ulong playerUID, ConditionData dataCell, params object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            TeamLevelLimtCondition level = dataCell.ConditionCfg as TeamLevelLimtCondition;

            if (level == null)
            {
                return false;
            }

            var module = ModuleManager.Instance.GetModule(ModuleDef.Name.TeamModule) as LuaModule;
            if (module != null)
            {
                var luatable = module.GetLuaTable();
                if (luatable != null)
                {
                    var teamInfo = luatable.Get<TeamInfo>("tinfo");
                    if (teamInfo != null)
                    {
                        bool result = true;

                        foreach (var user in teamInfo.UserInfos)
                        {
                            if (user.Level < level.Level)
                            {
                                result = false;
                                break;
                            }
                        }

                        return level.Flag ? !result : result;
                    }
                }
            }

            return false;
        }

        private bool OnCheckBuff(ulong playerUID, ConditionData dataCell, params object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            CheckBuffCondition buff = dataCell.ConditionCfg as CheckBuffCondition;

            if (buff == null)
            {
                return false;
            }

            var Player = GameManager.Instance.M_MainPlayerCtrlBase as PlayerCtrlGroup;
            if (Player != null)
            {
                var entityBase = Player.M_Curr as NPCEntityBase;
                if (entityBase != null)
                {
                    bool result = entityBase.HasBuff(buff.BuffID);
                    return buff.Flag ? !result : result;
                }
            }

            return false;
        }

        private bool OnWantedAccepted(ulong playerUid, ConditionData dataCell, object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            WantedAcceptedCondition wantedAcceptedCondition = dataCell.ConditionCfg as WantedAcceptedCondition;
            if (wantedAcceptedCondition == null)
            {
                return false;
            }

            bool isReceiveTask = false;
            var info = GameManager.Instance.GetMainPlayerWantTaskMD();
            if (info != null)
            {
                isReceiveTask = info.IsAcc;
            }

            return wantedAcceptedCondition.Flag ? !isReceiveTask : isReceiveTask;
        }


        private bool OnEquipIntensify(ulong playerUid, ConditionData dataCell, object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            EquipIntensifyCondition condition = dataCell.ConditionCfg as EquipIntensifyCondition;
            if (condition == null)
            {
                return false;
            }

            int haveNum = 0;
            int maxEquipNum = 8; //装备槽位上限，之后读表
            for (int i = 1; i <= maxEquipNum; i++)
            {
                var equipSlotData = GameManager.Instance.GetEquipSlotData(i);
                if (equipSlotData != null)
                {
                    var upLv = equipSlotData.UpLv;
                    if (upLv >= condition.EquipLevel)
                    {
                        haveNum++;
                    }
                }
            }

            bool isEnough = haveNum >= condition.Count;

            return condition.Flag ? !isEnough : isEnough;
        }

        private bool OnEquipQualityCount(ulong playerUid, ConditionData dataCell, object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            EquipQualityCondition condition = dataCell.ConditionCfg as EquipQualityCondition;
            if (condition == null)
            {
                return false;
            }

            bool isEnough = BusinessManager.Instance.CheckWearEquipQualityCount(condition.Quality, condition.Count);

            return condition.Flag ? !isEnough : isEnough;
        }

        private bool OnJobSkillLevelCount(ulong playerUid, ConditionData dataCell, object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            EquipQualityCondition condition = dataCell.ConditionCfg as EquipQualityCondition;
            if (condition == null)
            {
                return false;
            }

            bool isEnough = GameManager.Instance.HeroSkillTotalLevel() >= condition.Count;

            return condition.Flag ? !isEnough : isEnough;
        }

        private bool OnPersonDailyPass(ulong playerUid, ConditionData dataCell, object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            PersonDailyPassCondition condition = dataCell.ConditionCfg as PersonDailyPassCondition;
            if (condition == null)
            {
                return false;
            }

            bool isEnough = GameManager.Instance.IsDailyCopyPassed(condition.id, condition.diff);

            return condition.Flag ? !isEnough : isEnough;
        }

        private bool OnTeamDailyPass(ulong playerUid, ConditionData dataCell, object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            TeamDailyPassCondition condition = dataCell.ConditionCfg as TeamDailyPassCondition;
            if (condition == null)
            {
                return false;
            }

            bool isEnough = GameManager.Instance.IsTeamCopyPassed(condition.id);

            return condition.Flag ? !isEnough : isEnough;
        }

        private bool OnAppointSystemOpen(ulong playerUid, ConditionData dataCell, object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            AppointSystemOpenCondition condition = dataCell.ConditionCfg as AppointSystemOpenCondition;
            if (condition == null)
            {
                return false;
            }

            bool isEnough = SystemOpenManager.Instance.SystemIsOpen(condition.systemID);

            return condition.Flag ? !isEnough : isEnough;
        }

        private bool OnCheckBlackBoard(ulong playerUid, ConditionData dataCell, object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            CheckBlackBoardCondition condition = dataCell.ConditionCfg as CheckBlackBoardCondition;
            if (condition == null)
            {
                return false;
            }

            bool isEnough = GameManager.Instance.IsBlackBoard(condition.key, condition.value);

            return condition.Flag ? !isEnough : isEnough;
        }

        private bool OnPowerModuleCheck(ulong playerUid, ConditionData dataCell, object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            PowerModuleCheckCondition condition = dataCell.ConditionCfg as PowerModuleCheckCondition;
            if (condition == null)
            {
                return false;
            }

            bool isEnough = Battle.FightPowerManager.Instance.GetServerPowerAllData()
                .IsMatchPower(condition.moduleID, condition.limit,
                    condition.type == 0); // GameManager.Instance.IsBlackBoard(condition.key, condition.value);

            return condition.Flag ? !isEnough : isEnough;
        }

        private bool OnSkillDotTotal(ulong playerUid, ConditionData dataCell, object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            SkillDotTotalCondition condition = dataCell.ConditionCfg as SkillDotTotalCondition;
            if (condition == null)
            {
                return false;
            }

            bool isEnough = condition.value >= GameManager.Instance.GetSkillPoint();

            return condition.Flag ? !isEnough : isEnough;
        }
        private bool OnAdventureCheck(ulong playerUid, ConditionData dataCell, object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            AdventureCheckCondition condition = dataCell.ConditionCfg as AdventureCheckCondition;
            if (condition == null)
            {
                return false;
            }
            LobbyGamePlayMDMgr md = (LobbyGamePlayMDMgr)FixMessageManager.Instance.GetMDMgr(FixUpdateDef.LobbyGamePlayMD);
            bool isEnough = condition.stage >= md.GetMD().NewbieTarget.StageDone;

            return condition.Flag ? !isEnough : isEnough;
        }
        private bool OnPartnerCount(ulong playerUid, ConditionData dataCell, object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            PartnerCountCondition condition = dataCell.ConditionCfg as PartnerCountCondition;
            if (condition == null)
            {
                return false;
            }

            bool isFinish = false;
            var partnerMDMgr = BusinessManager.Instance.GetPartnerMDMgr();
            if (partnerMDMgr != null)
            {
                int count = partnerMDMgr.GetLevelPartnerCount(condition.PartnerLevel);
                isFinish = count > condition.Count;
            }

            return condition.Flag ? !isFinish : isFinish;
        }

        private bool OnAmuletPolishing(ulong playerUid, ConditionData dataCell, object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            AmuletPolishingCondition condition = dataCell.ConditionCfg as AmuletPolishingCondition;
            if (condition == null)
            {
                return false;
            }

            var allitems = BusinessManager.Instance.GetItemsByItemType(5, 0);


            int haveNum = 0;
            for (int i = 0; i < allitems.Count; i++)
            {
                var it = allitems[i];
                var combatSkills = it.CombatSkills;
                int curPolish = combatSkills.CurPolish; //已经打磨的次数
                var itemCfg = LocalData.LocalDataManager.Instance.GetItemDataCell(it.BaseID);
                int maxNum = LocalData.LocalDataManager.Instance.GetMaxPolishNum(itemCfg.GetQuality()); //打磨次数的上限

                if (curPolish == maxNum)
                {
                    haveNum++;
                }
            }

            bool isEnough = haveNum >= condition.AmuletCount;

            return condition.Flag ? !isEnough : isEnough;
        }

        /// <summary>
        /// 冒险等级判断
        /// </summary>
        /// <param name="playerUID"></param>
        /// <param name="dataCell"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool OnRiskLevelLimit(ulong playerUID, ConditionData dataCell, params object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            RiskLevelLimitCondition level = dataCell.ConditionCfg as RiskLevelLimitCondition;

            if (level == null)
            {
                return false;
            }

            if (GameManager.Instance.M_MainPlayerCtrlBase == null ||
                GameManager.Instance.M_MainPlayerCtrlBase.Data == null ||
                GameManager.Instance.M_MainPlayerCtrlBase.Data.Attrs == null)
            {
                return false;
            }

            var RiskInfo =
                GameManager.Instance.M_MainPlayerCtrlBase.Data.Attrs.GetProtoValue(
                    StarProjectDef.AOIAttrDefine.RiskInfo) as RiskLevelMD;

            if (RiskInfo != null)
            {
                bool result = RiskInfo.CurRiskLevel >= level.Level;
                return level.Flag ? !result : result;
            }

            return false;
        }

        private bool OnLevelLmit(ulong playerUID, ConditionData dataCell, params object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            LevelCondition level = dataCell.ConditionCfg as LevelCondition;

            if (level == null)
            {
                return false;
            }


            var EntityBase = GameManager.Instance.GetEntityCtr(playerUID);
            if (EntityBase != null)
            {
                if (EntityBase.entityBaseData != null && EntityBase.entityBaseData.Attrs != null)
                {
                    int PlayerLevel =
                        EntityBase.entityBaseData.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.PlayerLevel);

                    bool result = PlayerLevel >= level.Level;
                    return level.Flag ? !result : result;
                }
            }

            return false;
        }

        private bool OnPreTask(ulong playerUID, ConditionData dataCell, params object[] args)
        {
            return false;
        }

        private bool OnTask(ulong playerUID, ConditionData dataCell, params object[] args)
        {
            return false;
        }

        private bool OnPeopleNum(ulong playerUID, ConditionData dataCell, params object[] args)
        {
            return false;
        }

        private bool OnItemLimt(ulong playerUID, ConditionData dataCell, params object[] args)
        {
            if (dataCell == null)
            {
                return false;
            }

            ItemLimitCondition itemLimit = dataCell.ConditionCfg as ItemLimitCondition;
            if (itemLimit == null)
            {
                return false;
            }

            bool result = BusinessManager.Instance.GetCurrencyNum(itemLimit.ItemID) >= itemLimit.ItemCount;
            return itemLimit.Flag ? !result : result;
        }

        private bool OnMoneyLimit(ulong playerUID, ConditionData dataCell, params object[] args)
        {
            return false;
        }


        /// <summary>
        /// 条件注册
        /// </summary>
        /// <param name="key"></param>
        /// <param name="conditions"></param>
        public void RegisterConditions(int groupID, List<ConditionData> conditions, int subGroupID = 0)
        {
            if (conditions != null && conditions.Count < 1)
            {
                return;
            }

            if (ConditionCfs.ContainsKey(groupID))
            {
                if (ConditionCfs[groupID].ContainsKey(subGroupID))
                {
                    ConditionCfs[groupID][subGroupID].AddRange(conditions);
                }
                else
                {
                    ConditionCfs[groupID].Add(subGroupID, conditions);
                }
            }
            else
            {
                var dic = new Dictionary<int, List<ConditionData>>();
                dic.Add(subGroupID, conditions);

                ConditionCfs.Add(groupID, dic);
            }
        }

        public int GetConditionGroupID()
        {
            return ConditionGroupID++;
        }

        /// <summary>
        /// 单个条件是否满足
        /// </summary>
        /// <param name="playerUID"></param>
        /// <param name="dataCell"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool ConditionMete(ulong playerUID, ConditionData dataCell, params object[] args)
        {
            if (Conditions.TryGetValue(dataCell.ConditionType, out var action))
            {
                if (action != null)
                {
                    return action.Invoke(playerUID, dataCell, args);
                }
            }

            return true;
        }


        public Dictionary<int, bool> ConditionGroupMeteDic(ulong playerUID, int groupID, params object[] args)
        {
            Dictionary<int, bool> dicOut = new();
            if (ConditionCfs.ContainsKey(groupID))
            {
                foreach (var dic in ConditionCfs[groupID])
                {
                    var list = dic.Value;
                    if (list != null && list.Count > 0)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            bool met = ConditionMete(playerUID, list[i], args);
                            dicOut[(int)list[i].ConditionType] = met;
                        }
                    }
                }
            }

            return dicOut;
        }


        /// <summary>
        /// 判断条件组是否满足
        /// </summary>
        /// <param name="playerUID"></param>
        /// <param name="groupID"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool ConditionGroupMete(ulong playerUID, int groupID, params object[] args)
        {
            if (ConditionCfs.ContainsKey(groupID))
            {
                bool metadic = false;

                foreach (var dic in ConditionCfs[groupID])
                {
                    var list = dic.Value;
                    if (list != null && list.Count > 0)
                    {
                        bool meta = true;
                        for (int i = 0; i < list.Count; i++)
                        {
                            meta &= ConditionMete(playerUID, list[i], args);
                            if (!meta)
                            {
                                break;
                            }
                        }

                        metadic |= meta;
                        if (metadic)
                        {
                            break;
                        }
                    }
                }

                return metadic;
            }

            return true;
        }

        public class ConditionData
        {
            public int GroupID;
            public int SubGroupID; //子组ID
            public ConditionType ConditionType;
            public CMCondition ConditionCfg;

            public ConditionData(int groupID, ConditionType conditionType, CMCondition mCondition, int subGroupID = 0)
            {
                this.GroupID = groupID;
                this.SubGroupID = subGroupID;
                this.ConditionType = conditionType;
                this.ConditionCfg = mCondition;
                this.ConditionCfg.SubID = subGroupID;
            }

            private int ToInt(string arg)
            {
                if (!string.IsNullOrEmpty(arg))
                {
                    if (System.Int32.TryParse(arg, out int result))
                    {
                        return result;
                    }
                }

                return 0;
            }

            private uint ToUInt(string arg)
            {
                if (!string.IsNullOrEmpty(arg))
                {
                    if (System.UInt32.TryParse(arg, out uint result))
                    {
                        return result;
                    }
                }

                return 0;
            }

            public ConditionData(int groupID, JsonConditon data)
            {
                this.GroupID = groupID;
                this.ConditionType = data.ConditionType;
                switch (this.ConditionType)
                {
                    case ConditionType.LevelLimit:
                        LevelCondition level = new();
                        level.Level = ToInt(data.Args1);
                        this.ConditionCfg = level;
                        break;

                    case ConditionType.TaskIsFinish:
                        TaskFinishCondition taskfinish = new();
                        taskfinish.TaskID = ToUInt(data.Args1);
                        this.ConditionCfg = taskfinish;
                        break;
                    case ConditionType.TaskIsRunning:
                        TaskRunningCondition taskRunning = new();
                        taskRunning.TaskID = ToUInt(data.Args1);
                        this.ConditionCfg = taskRunning;
                        break;
                    case ConditionType.TaskEventIsFinish:
                        TaskEventFinishCondition TaskEvent = new();
                        TaskEvent.TaskID = ToUInt(data.Args1);
                        TaskEvent.EventID = ToInt(data.Args2);
                        this.ConditionCfg = TaskEvent;
                        break;
                    case ConditionType.TaskIsCommit:
                        TaskCommitCondition taskCommit = new();
                        taskCommit.TaskID = ToUInt(data.Args1);
                        this.ConditionCfg = taskCommit;
                        break;
                    case ConditionType.ItemLimit:
                        ItemLimitCondition itemLimit = new();
                        itemLimit.ItemID = ToInt(data.Args1);
                        itemLimit.ItemCount = ToInt(data.Args2);
                        this.ConditionCfg = itemLimit;
                        break;
                    case ConditionType.TeamCheck:
                        TeamCheckCondition teamCheck = new();
                        this.ConditionCfg = teamCheck;
                        break;

                    case ConditionType.GuildLevelLimit:
                        GuildLevelLimitCondition guildLevelLimit = new();
                        guildLevelLimit.Level = ToInt(data.Args1);
                        this.ConditionCfg = guildLevelLimit;
                        break;
                    case ConditionType.TreasureActived:
                        TreasureActivedCondition activedCondition = new();
                        if (!string.IsNullOrEmpty(data.Args1))
                        {
                            activedCondition.Index = ToInt(data.Args1);
                        }

                        this.ConditionCfg = activedCondition;
                        break;
                    case ConditionType.ServerOpendDay:
                        ServerOpendDayCondition serverOpend = new();
                        serverOpend.Day = ToInt(data.Args1);
                        this.ConditionCfg = serverOpend;
                        break;
                    case ConditionType.FinishLevel:
                        FinishLevelCondition finishLevel = new();
                        finishLevel.Index = ToInt(data.Args1);
                        this.ConditionCfg = finishLevel;
                        break;
                    case ConditionType.RiskLevelLimit:
                        RiskLevelLimitCondition RiskLevelLimit = new();
                        RiskLevelLimit.Level = ToInt(data.Args1);
                        this.ConditionCfg = RiskLevelLimit;
                        break;

                    case ConditionType.PlayerJob:
                        PlayerJobCondition PlayerJob = new();
                        PlayerJob.Job = ToInt(data.Args1);
                        this.ConditionCfg = PlayerJob;
                        break;

                    case ConditionType.TeamLevelLimt:
                        TeamLevelLimtCondition PassLevel = new();
                        PassLevel.Level = ToInt(data.Args1);
                        this.ConditionCfg = PassLevel;
                        break;
                    case ConditionType.CheckBuff:
                        CheckBuffCondition CheckBuff = new();
                        CheckBuff.BuffID = ToInt(data.Args1);
                        this.ConditionCfg = CheckBuff;
                        break;
                    case ConditionType.WantedAccepted:
                        WantedAcceptedCondition WantedAccepted = new();
                        this.ConditionCfg = WantedAccepted;
                        break;
                    case ConditionType.EquipIntensify:
                        EquipIntensifyCondition EquipIntensify = new();
                        EquipIntensify.EquipLevel = ToInt(data.Args1);
                        EquipIntensify.Count = ToInt(data.Args2);
                        this.ConditionCfg = EquipIntensify;
                        break;
                    case ConditionType.PartnerCount:
                        PartnerCountCondition PartnerCount = new();
                        PartnerCount.PartnerLevel = ToInt(data.Args1);
                        PartnerCount.Count = ToInt(data.Args2);
                        this.ConditionCfg = PartnerCount;
                        break;
                    case ConditionType.AmuletPolishing:
                        AmuletPolishingCondition AmuletPolishing = new();
                        AmuletPolishing.AmuletCount = ToInt(data.Args1);
                        this.ConditionCfg = AmuletPolishing;
                        break;
                    case ConditionType.EquipQualityCount:
                        EquipQualityCondition EquipQualityCount = new();
                        EquipQualityCount.Quality = ToInt(data.Args1);
                        EquipQualityCount.Count = ToInt(data.Args2);
                        this.ConditionCfg = EquipQualityCount;
                        break;
                    case ConditionType.JobSkillLevelCount:
                        JobSkillLevelCountCondition JobSkillLevelCount = new();
                        JobSkillLevelCount.LevelCount = ToInt(data.Args1);
                        this.ConditionCfg = JobSkillLevelCount;
                        break;
                    case ConditionType.PersonDailyPass:
                        PersonDailyPassCondition PersonDailyPass = new();
                        PersonDailyPass.id = Convert.ToUInt64(data.Args1);
                        PersonDailyPass.diff = Convert.ToUInt64(data.Args2);
                        this.ConditionCfg = PersonDailyPass;
                        break;
                    case ConditionType.TeamDailyPass:
                        TeamDailyPassCondition TeamDailyPass = new();
                        TeamDailyPass.id = ToInt(data.Args1);
                        this.ConditionCfg = TeamDailyPass;
                        break;
                    case ConditionType.AppointSystemOpen:
                        AppointSystemOpenCondition AppointSystemOpen = new();
                        AppointSystemOpen.systemID = (SystemOpenType)Enum.Parse(typeof(SystemOpenType), data.Args1);
                        this.ConditionCfg = AppointSystemOpen;
                        break;
                    case ConditionType.CheckBlackBoard:
                        CheckBlackBoardCondition CheckBlackBoard = new();
                        CheckBlackBoard.key = data.Args1;
                        CheckBlackBoard.value = ToInt(data.Args2);
                        this.ConditionCfg = CheckBlackBoard;
                        break;
                    case ConditionType.PowerModuleCheck:
                        PowerModuleCheckCondition PowerModuleCheck = new();
                        PowerModuleCheck.moduleID = ToInt(data.Args1);
                        PowerModuleCheck.limit = ToInt(data.Args2);
                        PowerModuleCheck.type = ToInt(data.Args3);
                        this.ConditionCfg = PowerModuleCheck;
                        break;
                    case ConditionType.SkillDotTotal:
                        SkillDotTotalCondition SkillDotTotal = new();
                        SkillDotTotal.value = ToInt(data.Args1);
                        this.ConditionCfg = SkillDotTotal;
                        break;
                    case ConditionType.AdventureCheck:
                        AdventureCheckCondition AdventureCheck = new();
                        AdventureCheck.stage = ToInt(data.Args1);
                        this.ConditionCfg = AdventureCheck;
                        break;
                }

                this.SubGroupID = data.SubGroupID;
                this.ConditionCfg.SubID = data.SubGroupID;
                this.ConditionCfg.Flag = data.Flag;
                this.ConditionCfg.IsShow = data.IsShow;
            }

            public ConditionData(CMconditionDataCell data)
            {
                this.GroupID = data.GetGroupID();
                this.SubGroupID = data.GetSubGroupID(); //需要改动
                int cType = data.GetConditionType();
                if (cType == 0)
                {
                    return;
                }

                this.ConditionType = (ConditionType)cType;
                switch (this.ConditionType)
                {
                    case ConditionType.LevelLimit:
                        LevelCondition level = new();
                        level.Level = ToInt(data.Args1);
                        this.ConditionCfg = level;
                        break;

                    case ConditionType.TaskIsFinish:
                        TaskFinishCondition taskfinish = new();
                        taskfinish.TaskID = ToUInt(data.Args1);
                        this.ConditionCfg = taskfinish;
                        break;
                    case ConditionType.TaskIsRunning:
                        TaskRunningCondition taskRunning = new();
                        taskRunning.TaskID = ToUInt(data.Args1);
                        this.ConditionCfg = taskRunning;
                        break;
                    case ConditionType.TaskEventIsFinish:
                        TaskEventFinishCondition TaskEvent = new();
                        TaskEvent.TaskID = ToUInt(data.Args1);
                        TaskEvent.EventID = ToInt(data.Args2);
                        this.ConditionCfg = TaskEvent;
                        break;
                    case ConditionType.TaskIsCommit:
                        TaskCommitCondition taskCommit = new();
                        taskCommit.TaskID = System.UInt32.Parse(data.Args1);
                        this.ConditionCfg = taskCommit;
                        break;
                    case ConditionType.ItemLimit:
                        ItemLimitCondition itemLimit = new();
                        itemLimit.ItemID = ToInt(data.Args1);
                        itemLimit.ItemCount = ToInt(data.Args2);
                        this.ConditionCfg = itemLimit;
                        break;
                    case ConditionType.TeamCheck:
                        TeamCheckCondition teamCheck = new();
                        this.ConditionCfg = teamCheck;
                        break;
                    case ConditionType.GuildLevelLimit:
                        GuildLevelLimitCondition guildLevelLimit = new();
                        guildLevelLimit.Level = ToInt(data.Args1);
                        this.ConditionCfg = guildLevelLimit;
                        break;
                    case ConditionType.TreasureActived:
                        TreasureActivedCondition activedCondition = new();
                        if (!string.IsNullOrEmpty(data.Args1))
                        {
                            activedCondition.Index = ToInt(data.Args1);
                        }

                        this.ConditionCfg = activedCondition;
                        break;
                    case ConditionType.ServerOpendDay:
                        ServerOpendDayCondition serverOpend = new();
                        serverOpend.Day = ToInt(data.Args1);
                        this.ConditionCfg = serverOpend;
                        break;
                    case ConditionType.FinishLevel:
                        FinishLevelCondition finishLevel = new();
                        finishLevel.Index = ToInt(data.Args1);
                        this.ConditionCfg = finishLevel;
                        break;
                    case ConditionType.RiskLevelLimit:
                        RiskLevelLimitCondition RiskLevelLimit = new();
                        RiskLevelLimit.Level = ToInt(data.Args1);
                        this.ConditionCfg = RiskLevelLimit;
                        break;
                    case ConditionType.PlayerJob:
                        PlayerJobCondition PlayerJob = new();
                        PlayerJob.Job = ToInt(data.Args1);
                        this.ConditionCfg = PlayerJob;
                        break;

                    case ConditionType.TeamLevelLimt:
                        TeamLevelLimtCondition PassLevel = new();
                        PassLevel.Level = ToInt(data.Args1);
                        this.ConditionCfg = PassLevel;
                        break;

                    case ConditionType.CheckBuff:
                        CheckBuffCondition CheckBuff = new();
                        CheckBuff.BuffID = ToInt(data.Args1);
                        this.ConditionCfg = CheckBuff;
                        break;
                    case ConditionType.WantedAccepted:
                        WantedAcceptedCondition WantedAccepted = new();
                        this.ConditionCfg = WantedAccepted;
                        break;
                    case ConditionType.EquipIntensify:
                        EquipIntensifyCondition EquipIntensify = new();
                        EquipIntensify.EquipLevel = ToInt(data.Args1);
                        EquipIntensify.Count = ToInt(data.Args2);
                        this.ConditionCfg = EquipIntensify;
                        break;
                    case ConditionType.PartnerCount:
                        PartnerCountCondition PartnerCount = new();
                        PartnerCount.PartnerLevel = ToInt(data.Args1);
                        PartnerCount.Count = ToInt(data.Args2);
                        this.ConditionCfg = PartnerCount;
                        break;
                    case ConditionType.AmuletPolishing:
                        AmuletPolishingCondition AmuletPolishing = new();
                        AmuletPolishing.AmuletCount = ToInt(data.Args1);
                        this.ConditionCfg = AmuletPolishing;
                        break;
                    case ConditionType.EquipQualityCount:
                        EquipQualityCondition EquipQualityCount = new();
                        EquipQualityCount.Quality = ToInt(data.Args1);
                        EquipQualityCount.Count = ToInt(data.Args2);
                        this.ConditionCfg = EquipQualityCount;
                        break;
                    case ConditionType.JobSkillLevelCount:
                        JobSkillLevelCountCondition JobSkillLevelCount = new();
                        JobSkillLevelCount.LevelCount = ToInt(data.Args1);
                        this.ConditionCfg = JobSkillLevelCount;
                        break;
                    case ConditionType.PersonDailyPass:
                        PersonDailyPassCondition PersonDailyPass = new();
                        PersonDailyPass.id = Convert.ToUInt64(data.Args1);
                        PersonDailyPass.diff = Convert.ToUInt64(data.Args2);
                        this.ConditionCfg = PersonDailyPass;
                        break;
                    case ConditionType.TeamDailyPass:
                        TeamDailyPassCondition TeamDailyPass = new();
                        TeamDailyPass.id = ToInt(data.Args1);
                        this.ConditionCfg = TeamDailyPass;
                        break;
                    case ConditionType.AppointSystemOpen:
                        AppointSystemOpenCondition AppointSystemOpen = new();
                        AppointSystemOpen.systemID = (SystemOpenType)Enum.Parse(typeof(SystemOpenType), data.Args1);
                        this.ConditionCfg = AppointSystemOpen;
                        break;
                    case ConditionType.CheckBlackBoard:
                        CheckBlackBoardCondition CheckBlackBoard = new();
                        CheckBlackBoard.key = data.Args1;
                        CheckBlackBoard.value = ToInt(data.Args2);
                        this.ConditionCfg = CheckBlackBoard;
                        break;
                    case ConditionType.PowerModuleCheck:
                        PowerModuleCheckCondition PowerModuleCheck = new();
                        PowerModuleCheck.moduleID = ToInt(data.Args1);
                        PowerModuleCheck.limit = ToInt(data.Args2);
                        PowerModuleCheck.type = ToInt(data.Args3);
                        this.ConditionCfg = PowerModuleCheck;
                        break;
                    case ConditionType.SkillDotTotal:
                        SkillDotTotalCondition SkillDotTotal = new();
                        SkillDotTotal.value = ToInt(data.Args1);
                        this.ConditionCfg = SkillDotTotal;
                        break;
                    case ConditionType.AdventureCheck:
                        AdventureCheckCondition AdventureCheck = new();
                        AdventureCheck.stage = ToInt(data.Args1);
                        this.ConditionCfg = AdventureCheck;
                        break;
                }

                if (this.ConditionCfg == null)
                {
                    SGF.Debuger.LogWarning(
                        $"GlobalFunctionManager ConditionData() ConditionType={this.ConditionType} 客户端没定义 err!!!");
                    return;
                }

                if (ConditionCfg != null)
                {
                    this.ConditionCfg.SubID = data.GetSubGroupID();
                    this.ConditionCfg.Flag = data.GetFlag();
                    this.ConditionCfg.IsShow = data.GetIsShow();
                }
            }
        }

        #endregion

        public void Init()
        {
            InitEventEffect();
            InitCondition();
        }


        public override void Release()
        {
            base.Release();
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.UserCurEffNotifyID, OnUserCurEffNotify, this);
            GlobalEvent.OnEffectEnd.RemoveListener(OnEffectEndHandler);
            GlobalEvent.OnSelectEffect.RemoveListener(OnSelectEffectHandler);
            FunctionMap?.Clear();
            Conditions?.Clear();
            ConditionCfs?.Clear();
        }
    }
}