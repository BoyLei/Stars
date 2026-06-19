///--------------------------------------------------------------------
/// �ļ���   :   ItemControllerModule
/// ��  ��   :   ���߹���ģ��
/// ˵  ��   :   ����������
/// �������� :   2022/10/27 18:26:38
/// ������   :   ������
/// ��Ȩ���� :   �ο�����Ƽ��������޹�˾ 
///--------------------------------------------------------------------

using Google.Protobuf.Collections;
using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using SGF.UI.Framework;
using StarProject.Service.Business;
using StarProjectDef;
using System;
using System.Collections.Generic;

namespace StarProject.Module
{
    public class RingTaskModule : BusinessModule
    {

        RepeatedField<int> taskIDs;
        bool isGetExRew;


        private Dictionary<string, System.Action<object>> InternalMessages = null;




        public override void Create(object args = null)
        {
            base.Create(args);
            BindEquipSlotControllerMsg();

            InternalMessages = new Dictionary<string, Action<object>>();
            InternalMessages.Add("OnCkeckAndOpenRingTaskWindow", OnCheckAndOpenRingTaskWindow);
            InternalMessages.Add("OnAcceptRingTask", OnAcceptRingTask);
            InternalMessages.Add("OnOpenRingTaskRandomWidget", OnOpenRingTaskRandomWidget); 

            //监听完成回调
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.RingTaskFinishedNtfID, OnFinishRingTaskHandler, null);

        }

        private void OnFinishRingTaskHandler(MessageHandleData data)
        {
            RingTaskFinishedNtf msg = data.data as RingTaskFinishedNtf;
            if (msg != null)
            {
                var allRingTasks = BusinessManager.Instance.GetAllRingTaskIDs();
                if (allRingTasks != null && allRingTasks.Count > 0)
                {
                    for (int i = 0; i < allRingTasks.Count; i++)
                    {
                        if (msg.TaskID == allRingTasks[i])
                        {
                            OnFinishRingTask(i);
                        }
                    }
                }
            }
        }

        private void OnFinishRingTask(int idx)
        {
            bool isFirstOpen = false;
            int finishIdx = idx+1;
            //完成任务并打开UI
            var args = new object[] { isFirstOpen, finishIdx };
            UIManager.Instance.OpenWindowAsync(UIDef.RingTaskWindow, null, args, StarProjectDef.MainPageCommond.HideBoth);
        }


        private void OnAcceptRingTask(object obj)
        {
            bool isFirstOpen = true;
            int finishIdx = 0;
            //完成任务并打开UI
            var args = new object[] { isFirstOpen, finishIdx };
            UIManager.Instance.OpenWindowAsync(UIDef.RingTaskWindow, null, args, StarProjectDef.MainPageCommond.HideBoth);
        }

        private void OnCheckAndOpenRingTaskWindow(object obj)
        {
            var allRingTask = GetTaskIDs();

            if (allRingTask != null && allRingTask.Count > 0)
            {
                uint id = (uint)allRingTask[0];
                //打开UI
                if (TaskHelper.ContainTask(id) || TaskHelper.IsTaskFinsh(id))
                {
                    RedPointManager.Instance.TriggerConditionTypeBoolValue(RedPointConditionType.RingTaskNotReceive, false);
                    //UIManager.Instance.OpenWindow(UIDef.RingTaskWindow, null, StarProjectDef.MainPageCommond.HideBoth);

                    bool isFirstOpen = false;
                    int finishIdx = 0;
                    //打开UI
                    var args = new object[] { isFirstOpen, finishIdx };
                    UIManager.Instance.OpenWindowAsync(UIDef.RingTaskWindow, null, args, StarProjectDef.MainPageCommond.HideBoth);
                }
                else
                //接取任务并且打开UI
                {
                    UserSundryMDMgr userSundryMDMgr=(UserSundryMDMgr)FixMessageManager.Instance.GetMDMgr(FixUpdateDef.UserSundry);
                    //新手环任务完成后才显示对话
                    if (userSundryMDMgr.GetServerSign(SrvSignTypeEnum.RingTaskSetFresh))
                    {
                        //判断是否解锁
                        if (Service.Business.BusinessManager.Instance.SystemIsOpen(StarProjectDef.SystemOpenType.RingTask) && Service.Business.BusinessManager.Instance.CheckSysOpen(5))
                        {
                            RedPointManager.Instance.TriggerConditionTypeBoolValue(RedPointConditionType.RingTaskNotReceive, true);
                        }
                        else
                        {
                            RedPointManager.Instance.TriggerConditionTypeBoolValue(RedPointConditionType.RingTaskNotReceive, false);
                        }

                        ModuleManager.Instance.SendMessage(ModuleDef.Name.AvgLuaModule, "OnOpenAvgTalkBox", new object[] { 200000001 });
                        ModuleManager.Instance.Event(ModuleDef.Name.AvgLuaModule, "OnFinishAvgTalkBox").AddListener(OnAcceptTaskReq);
                    }
                    else
                    {
                        var message = new ProtoMsg.AcceptTaskReq();
                        message.ID = allRingTask[0];
                        NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeLobby, message, false);
                        //判断是否解锁
                        if (Service.Business.BusinessManager.Instance.SystemIsOpen(StarProjectDef.SystemOpenType.RingTask) && Service.Business.BusinessManager.Instance.CheckSysOpen(5))
                        {
                            RedPointManager.Instance.TriggerConditionTypeBoolValue(RedPointConditionType.RingTaskNotReceive, true);
                        }
                        else
                        {
                            RedPointManager.Instance.TriggerConditionTypeBoolValue(RedPointConditionType.RingTaskNotReceive, false);
                        }
                    }
                }
            }
        }


        private void OnOpenRingTaskRandomWidget(object obj)
        {
            UIManager.Instance.OpenWidgetAsync(UIDef.RingTaskRandomWidget,null);
        }

        private void OnAcceptTaskReq(object args)
        {
            var allRingTask = GetTaskIDs();
            var message = new ProtoMsg.AcceptTaskReq();
            message.ID = allRingTask[0];
            NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeLobby, message, false);
            ModuleManager.Instance.Event(ModuleDef.Name.AvgLuaModule, "OnFinishAvgTalkBox").RemoveListener(OnAcceptTaskReq);
        }



        protected override void OnModuleMessage(string msg, object[] args)
        {
            if (InternalMessages.TryGetValue(msg, out System.Action<object> action))
            {
                action?.Invoke(args);
            }
        }
        protected override void Show(object arg)
        {

        }

        private void BindEquipSlotControllerMsg()
        {
            //环任务推送通知(刚上线)
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.RingTaskNtfID, OnRingTaskNtf, this);


            //环任务领取道具错误码监听
            MsgRetManager.Instance.OnMessageEnum(OnRingTaskExReqRet, MsgIDEnum.RingTaskExReqID);
        }


        private void OnRingTaskNtf(MessageHandleData data)
        {
            RingTaskNtf message = (RingTaskNtf)data.data;

            taskIDs = message.TaskIDs;
            isGetExRew = message.IsGetExRew;


            CheckRingTaskGetInfo();

        }

        public void CheckRingTaskGetInfo()
        {
            var allRingTask = GetTaskIDs();
            if (allRingTask != null && allRingTask.Count > 0)
            {
                uint id = (uint)allRingTask[0];
                if (!(TaskHelper.ContainTask(id) || TaskHelper.IsTaskFinsh(id)))
                {
                    if (Service.Business.BusinessManager.Instance.SystemIsOpen(StarProjectDef.SystemOpenType.RingTask) && Service.Business.BusinessManager.Instance.CheckSysOpen(5))
                    {
                        RedPointManager.Instance.TriggerConditionTypeBoolValue(RedPointConditionType.RingTaskNotReceive, true);
                    }
                    else
                    {
                        RedPointManager.Instance.TriggerConditionTypeBoolValue(RedPointConditionType.RingTaskNotReceive, false);
                    }

                }
            }

        }

        private void OnRingTaskExReqRet(MsgRetArgs args)
        {
            if (args.MsgRet.RetCode == 0)
            {
                //领取成功
                isGetExRew = true;
            }
            else
            {
                //交给上层处理
            }
        }


        public bool CheckIsGetExRew()
        {
            return isGetExRew;
        }

        public RepeatedField<int> GetTaskIDs()
        {
            return taskIDs;
        }

        public override void Release()
        {
            //监听完成回调
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.RingTaskFinishedNtfID, OnFinishRingTaskHandler, null);
            InternalMessages.Clear();
        }


    }
}
