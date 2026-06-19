using ProtoMsg;
using SGF.Network;
using SGF.UI.Framework;
using StarProject.Service.SDK;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.UI.StarWorld
{
    [System.Serializable]
    public class SettingWindow : UIWindow
    {
        //private string LOG_TAG = "[SettingWindow]";

        public StarUITabMenu starUITabMenu;
        public List<SettingBaseParam> SettingBaseParams;
        public JButton BindIDBtn;
        public JButton LoginOutBtn;
        public JButton JBtnRestoration;

        protected override void Awake()
        {
            base.Awake();
            starUITabMenu.Init(0, OnTabCB);
            BindIDBtn.OnClick += OnBindIDClick;
            LoginOutBtn.OnClick += OnLoginOutClick;
            JBtnRestoration.OnClick += OnJBtnRestoration;
        }

        private void OnTabCB(int index)
        {
            starUITabMenu.Check(index);

            for (int i = 0; i < SettingBaseParams.Count; i++)
            {
                var item = SettingBaseParams[i];
                bool isShow = i == index;
                item.SetShow(isShow);
            }
        }

        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);
            for (int i = 0; i < SettingBaseParams.Count; i++)
            {
                SettingBaseParams[i].Init();
            }
            OnTabCB(0);
        }

        protected override void OnClose(object arg = null)
        {
            base.OnClose(arg);
            for (int i = 0; i < SettingBaseParams.Count; i++)
            {
                var itemBase = SettingBaseParams[i];
                itemBase.SaveServerData();
                itemBase.SaveLocalData();
                itemBase.Reset();
            }
        }

        private void OnBindIDClick(UnityEngine.GameObject go)
        {
            SDKManager.Instance.BindIDCard();
        }

        private void OnLoginOutClick(UnityEngine.GameObject go)
        {
            SDKManager.Instance.LoginOut();
        }

        private void OnJBtnRestoration(GameObject arg0)
        {
            UIAPI.ShowMsgBox(68, (string EventName) =>
            {
                if (EventName == "SURE")
                {
                    RoleReviveReq roleReviveReqMsg = new RoleReviveReq();
                    roleReviveReqMsg.ReviveType = ReviveTypeEnum.ReviveTypeBackSafePos;
                    NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeScene, roleReviveReqMsg, false);
                    UIManager.Instance.CloseWindow(UIDef.SettingWindow);
                }
            }, new object[] { });
        }
    }
}
