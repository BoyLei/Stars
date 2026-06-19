using ProtoMsg;
using SGF.Network;
using Sirenix.OdinInspector;
using StarProject.Service.Business;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace StarProject.UI.StarWorld
{
    public class SettingGameParam : SettingBaseParam
    {
        [LabelText("复选拒绝陌生人聊天")]
        public Toggle ToggleForbiddeMsg;
        [LabelText("复选拒绝陌生人组队")]
        public Toggle ToggleForbiddeTeam;
        [LabelText("复选好友申请")]
        public Toggle ToggleForbiddeFriend;
        [LabelText("复选拒绝梦境入侵推送")]
        public Toggle ToggleForbiddeDreamInvade;
        [LabelText("复选体力回满")]
        public Toggle ToggleForbiddeStamina;

        private BaseBinary baseBinary;
        // 通知服务器
        public Dictionary<ProtoMsg.UserSettingEnum, bool> ChanageServerSettingDic = new();


        protected override void Awake()
        {
            base.Awake();
            ToggleForbiddeMsg.onValueChanged.AddListener(OnToggleForbiddeMsg);
            ToggleForbiddeTeam.onValueChanged.AddListener(OnToggleForbiddeTeam);
            ToggleForbiddeFriend.onValueChanged.AddListener(OnToggleForbiddeFriend);
            ToggleForbiddeDreamInvade.onValueChanged.AddListener(OnToggleForbiddeDreamInvade);
            ToggleForbiddeStamina.onValueChanged.AddListener(OnToggleForbiddeStamina);

            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.UserSettingBatchRetID, OnUserSettingBatchRet, this);

        }

        private void OnDestroy()
        {
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.UserSettingBatchRetID, OnUserSettingBatchRet, this);
        }

        private void OnUserSettingBatchRet(MessageHandleData data)
        {
            UserSettingBatchRet dialogNotify = (UserSettingBatchRet)data.data;

            foreach (var item in dialogNotify.BatchSet)
            {
                int val = item.SetTy == UserSettingReq.Types.SetType.Open ? 1 : 0;
                bool res = ProtoUtils.BaseBinaryUpData(baseBinary, (int)item.SetID, val);
                SGF.Debuger.Log($"设置 返回 type={item.SetID},val={val},res={res}");
            }
        }

        public override void Reset()
        {
            base.Reset();
            ChanageServerSettingDic.Clear();
        }

        public override void Init()
        {
            base.Init();
            baseBinary = BusinessManager.Instance.M_UserSundryMD.UserSetting;

            foreach (UserSettingEnum type in Enum.GetValues(typeof(UserSettingEnum)))
            {
                int val = ProtoUtils.BaseBinaryContainKey(baseBinary, (int)type);
                SGF.Debuger.Log($"设置 初始化 type={type},val={val}");
                switch (type)
                {
                    case UserSettingEnum.SettingDefault:
                        break;
                    case UserSettingEnum.SettingReplyInNormal:
                        break;
                    case UserSettingEnum.SettingReplyInBattle:
                        break;
                    case UserSettingEnum.SettingRefusePrivateChat:
                        ToggleForbiddeMsg.SetIsOnWithoutNotify(val == 1);
                        break;
                    case UserSettingEnum.SettingRefuseFriendInvite:
                        ToggleForbiddeFriend.SetIsOnWithoutNotify(val == 1);
                        break;
                    case UserSettingEnum.SettingRefuseTeamInvite:
                        ToggleForbiddeTeam.SetIsOnWithoutNotify(val == 1);
                        break;
                    default:
                        break;
                }
            }

            ToggleForbiddeDreamInvade.SetIsOnWithoutNotify(false);
            ToggleForbiddeStamina.SetIsOnWithoutNotify(false);
        }

        // 复选拒绝陌生人聊天
        // 客户端自己拦截
        private void OnToggleForbiddeMsg(bool arg0)
        {
            AddChanageServerSettingDic(ProtoMsg.UserSettingEnum.SettingRefusePrivateChat, arg0);
        }

        // 复选拒绝陌生人组队
        private void OnToggleForbiddeTeam(bool arg0)
        {
            AddChanageServerSettingDic(ProtoMsg.UserSettingEnum.SettingRefuseTeamInvite, arg0);
        }

        // 复选好友申请
        private void OnToggleForbiddeFriend(bool arg0)
        {
            AddChanageServerSettingDic(ProtoMsg.UserSettingEnum.SettingRefuseFriendInvite, arg0);
        }

        // 复选拒绝梦境入侵推送
        private void OnToggleForbiddeDreamInvade(bool arg0)
        {
            // 先不做
        }

        // 复选体力回满
        private void OnToggleForbiddeStamina(bool arg0)
        {
            // 先不做
        }


        #region 本地缓存

        public override void SaveLocalData()
        {
            base.SaveLocalData();
        }

        #endregion

        #region 服务器缓存

        protected void AddChanageServerSettingDic(ProtoMsg.UserSettingEnum type, bool isClose)
        {
            if (ChanageServerSettingDic.ContainsKey(type))
            {
                ChanageServerSettingDic[type] = isClose;
            }
            else
            {
                ChanageServerSettingDic.Add(type, isClose);
            }
        }

        public override void SaveServerData()
        {
            base.SaveServerData();

            UserSettingBatchReq userSettingBatchReq = new();
            foreach (var item in ChanageServerSettingDic)
            {
                UserSettingReq userSettingReq = new();
                userSettingReq.SetTy = item.Value == true ? UserSettingReq.Types.SetType.Open : UserSettingReq.Types.SetType.Close;
                userSettingReq.SetID = item.Key;
                SGF.Debuger.Log($"设置 请求 type={item.Key},val={item.Value}");
                userSettingBatchReq.BatchSet.Add(userSettingReq);
            }
            if (userSettingBatchReq.BatchSet.Count > 0)
            {
                NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeLobby, userSettingBatchReq, false);
            }
        }

        #endregion


    }
}
