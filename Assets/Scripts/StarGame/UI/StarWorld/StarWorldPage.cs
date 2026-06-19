using DG.Tweening;
using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using SGF.Time;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject.Game;
using StarProject.Game.Entity.RemoteDynamic;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Skill.Utils;
using StarProject.Module;
using StarProject.Service.Business;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProject.Service.Sound;
using StarProject.Service.SystemOpen;
using StarProjectDef;
using System;
using System.Collections.Generic;
using StarProject.Module.StarWordGame;
using StarProject.Service.SDK;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

namespace StarProject.UI.StarWorld
{
    public class StarWorldPage : UIPage
    {
        private string LOG_TAG = "[StarWorldPage]";

        private StarWorldModule m_Mymodule;

        public Transform M_TopLeftRoot;
        public Transform M_TopCentreRoot;
        public Transform M_TopRightRoot;
        public Transform M_MiddleLeftRoot;
        public Transform M_MiddleCentreRoot;
        public Transform M_MiddleRightRoot;
        public Transform M_DownLeftRoot;
        public Transform M_DownCentreRoot;
        public Transform M_DownRightRoot;

        public Transform M_MiniMapRoot;
        private Transform M_TreasureRoot;


        // ---------------------- GM


        #region 原子锁
        public Text M_ClientForbidMoveText;
        public Text M_ServerForbidMoveText;
        public Text M_SkillForbidMoveText;
        public Text M_ClientForbidDirText;
        public Text M_ServerForbidDirText;
        public Text M_SkillForbidDirText;
        #endregion

        #region 活动倒计时预告

        //private Button GVE_Group; // GVE活动倒计时按钮
        public Button GVE_Btn; // GVE活动倒计时按钮
        public Text m_GVE_Time; //GVE活动倒计时
        public Text m_GVE_Title; //GVE活动倒计时

        //private Button GNG_Group; // GNG活动倒计时按钮
        public Button GNG_Btn; // GNG活动倒计时按钮
        public Text m_GNG_Time; //GNG活动倒计时
        public Text m_GNG_Title; //GNG活动倒计时

        public Button Party_Btn; // 篝火活动倒计时按钮
        public Text m_Party_Time; //篝火活动倒计时
        public Text m_Party_Title; //篝火活动倒计时


        //private Button Wild_Group; // 野外boss活动倒计时按钮
        public Button Wild_Btn; // 野外boss活动倒计时按钮
        public Text m_Wild_Time; //野外boss活动倒计时
        public Text m_Wild_Title; //野外boss活动倒计时

        //private Button PVP_Group; // PVP活动倒计时按钮
        public Button PVP_Btn; // PVP活动倒计时按钮
        public Text m_PVP_Time; //PVP活动倒计时
        public Text m_PVP_Title; //PVP活动倒计时
        #endregion

        #region 系统功能按钮

        public Button Ectype_Btn; // 副本退出按钮
        public Button BtnSetting; // 副本设置按钮
        public Text m_Ectype_Time; // 副本倒计时
        public Transform TaskRoot; // 任务挂件容器

        private JButton ectypeBagBtn; // 副本临时背包按钮
        private JButton gveBossRewardBtn; // gveBoss奖励
        private JButton gngBossRewardBtn; // gngBoss奖励

        #endregion

        private GameObject LeftBtnLyout;
        private GameObject EventTimeWidget;

        private GameObject RightBtnLayout;
        private GameObject RightTimeLimitBtnLayout;

        private GameObject DownCentreSonRoot;


        // ----------------------

        // 菜单面板
        private JButton m_ArrowBtn;

        private JButton mMailBtn;

        private AdvancedTreasure _treasureBtn;

        private NormalTreasure _normalTreasure;

        private GameObject go_JBtnGoldShop;
        public GameObject go_BtnShop;
        public GameObject go_BtnFirstCharge2;
        private GameObject go_BtnFirstCharge3;
        private GameObject go_BtnPreview;
        private GameObject go_BtnPreviewRedPoint;
        public GameObject go_BtnShop_shop;
        public GameObject go_BtnShop_charge;

        public GameObject go_Act_Redpoint;

        JButton m_onlineBtn;
        GameObject go_Online_Redpoint;
        Text txt_Online_Status;

        private GameObject JBtnAct;
        private DictionaryEx<ModuleDef.Name, Transform> FunctionRoot = new() { };
        public Transform GetFuncTransform(ModuleDef.Name ModuleName)
        {
            if (FunctionRoot.TryGetValue(ModuleName, out Transform t))
            {
                return t;
            }

            return null;
        }

        //public Material cardMat;
        Material orgMat;
        bool cardOpened = false;
        GetRedPoint GetRedPointEvent;

        IsAllFinish IsAllFinishEvent;
        CanGetOnlineReward CanGetOnlineRewardEvent;
        GetOnlineRewardTime GetOnlineRewardTimeEvent;
        [SerializeField]
        private JButton SkipBtn;

        protected override void Awake()
        {
            base.Awake();
            //SetCloseDestroy(true);//Close的时候销毁 防止UI状态没有初始化
            // 绑定按钮事件
            {
                // 收缩箭头
                {
                    //m_ArrowBtn = m_MenuInfoPanel.Find("BtnArrow").GetComponent<JButton>();
                    //m_ArrowBtn.OnClick = (go) =>
                    //{
                    //    OnBtnClickArrow();
                    //    //ModuleManager.Instance.SendMessage(ModuleDef.SecretAreaModule, "OpenSecretAreaWindow", null);
                    //};
                }
                // 装备
                {
                    //JButton Btn = m_MenuInfoPanel.Find("BtnEquip").GetComponent<JButton>();
                    //Btn.OnClick = (go) => { OnBtnClickEquip(); };
                }
                // 右一排按钮
                {
                    RightBtnLayout = M_TopRightRoot.Find("RightBtnLayout").gameObject;
                    // 菜单按钮
                    {
                        JButton Btn = RightBtnLayout.transform.Find("JBtnMenu").GetComponent<JButton>();
                        Btn.OnClick = (go) => { OnBtnClickMenu(); };
                        ModuleManager.Instance.SendMessage(ModuleDef.Name.SkillUnlockTipsModule, "OnJBtnMenuFind", new object[] { Btn.transform });
                    }
                    // 背包
                    {
                        JButton Btn = RightBtnLayout.transform.Find("JBtnBackpack").GetComponent<JButton>();
                        Btn.OnClick = (go) => { OnBtnClickBackPack(); };
                    }
                    // 交易
                    {
                        SystemItem systemItem = RightBtnLayout.transform.Find("JBtnTrade").GetComponent<SystemItem>();
                        systemItem.RegisterSystemClick(() =>
                        {
                            ModuleManager.Instance.SendMessage(ModuleDef.Name.RechargeModule, "OnOpen", null, null);
                        });
                    }
                    // Gold商店
                    {
                        go_JBtnGoldShop = RightBtnLayout.transform.Find("JBtnGoldShop").gameObject;
                        SystemItem systemItem = go_JBtnGoldShop.GetComponent<SystemItem>();
                        systemItem.RegisterSystemClick(() =>
                        {
                            OnBtnClickShop();
                        });
                    }

                    // 玩法
                    {
                        SystemItem systemItem = RightBtnLayout.transform.Find("JBtnGamePlay").GetComponent<SystemItem>();
                        systemItem.RegisterSystemClick(() =>
                        {
                            ModuleManager.Instance.SendMessage(ModuleDef.Name.GamePlayChoiceModule, "OnOpenGamePlayChoice", new object[] { });
                        });
                    }
                }

                // 右二排按钮
                {
                    RightTimeLimitBtnLayout = M_TopRightRoot.Find("RightTimeLimitBtnLayout").gameObject;
                    // 首充
                    {
                        go_BtnFirstCharge3 = RightTimeLimitBtnLayout.transform.Find("JBtnFirstCharge").gameObject;
                        JButton Btn2 = go_BtnFirstCharge3.GetComponent<JButton>();
                        Btn2.OnClick = (go) => { OnBtnClickFirstCharge(); };
                    }

                    // 商店
                    {
                        go_BtnShop = RightTimeLimitBtnLayout.transform.Find("JBtnShop").gameObject;
                        go_BtnShop.SetActive(false);
                        SystemItem systemItem = go_BtnShop.GetComponent<SystemItem>();
                        systemItem.RegisterSystemClick(() =>
                        {
                            if (go_BtnShop_shop.activeSelf)
                            {
                                OnBtnClickShop();
                            }
                            else
                            {
                                OnBtnClickFirstCharge();
                            }
                        });
                    }

                    // 玩法预告
                    {
                        go_BtnPreview = RightTimeLimitBtnLayout.transform.Find("JBtnPreview").gameObject;
                        go_BtnPreviewRedPoint = RightTimeLimitBtnLayout.transform.Find("JBtnPreview/RedPoint").gameObject;
                        JButton Btn2 = go_BtnPreview.GetComponent<JButton>();
                        Btn2.OnClick = (go) => { OnBtnClickPreview(); };
                    }
                    // 转职
                    {
                        SystemItem systemItem = RightTimeLimitBtnLayout.transform.Find("JBtnTransJob").GetComponent<SystemItem>();
                        systemItem.RegisterSystemClick(() =>
                        {
                            // 打开转职界面
                            //UIManager.Instance.OpenWindow(UIDef.TransJobWindow, null, MainPageCommond.HideBoth, false, true);
                            UIManager.Instance.OpenWindowAsync(UIDef.TransJobWindow, null, null, MainPageCommond.HideBoth, false, true);
                        });
                    }
                    // 在线礼包
                    {
                        SystemItem systemItem = RightTimeLimitBtnLayout.transform.Find("JBtnOnline").GetComponent<SystemItem>();
                        systemItem.RegisterSystemClick(() =>
                        {
                            ModuleManager.Instance.SendMessage(ModuleDef.Name.OnlineRewardModule, "OnOpenOnlineRewardWindow", new object[] { });
                        });
                        m_onlineBtn = RightTimeLimitBtnLayout.transform.Find("JBtnOnline").GetComponent<JButton>();
                        // m_onlineBtn.OnClick = (go) =>
                        // {
                        //     ModuleManager.Instance.SendMessage(ModuleDef.Name.OnlineRewardModule, "OnOpenOnlineRewardWindow", new object[] { });
                        // };

                        go_Online_Redpoint = RightTimeLimitBtnLayout.transform.Find("JBtnOnline/RedPoint").gameObject;
                        txt_Online_Status = RightTimeLimitBtnLayout.transform.Find("JBtnOnline/Status").GetComponent<Text>();
                    }
                }
                // 左活动福利按钮
                {
                    // 活动（福利）
                    LeftBtnLyout = M_TopLeftRoot.Find("PanelADD/LeftBtnLyout").gameObject;
                    {
                        SystemItem systemItem = M_TopLeftRoot.Find("PanelADD/LeftBtnLyout/JBtnActivity").GetComponent<SystemItem>();
                        systemItem.RegisterSystemClick(() =>
                        {
                            OnBtnClickActivity();
                        });
                    }
                    // real活动
                    {
                        JBtnAct = M_TopLeftRoot.Find("PanelADD/LeftBtnLyout/JBtnAct").gameObject;
                        SystemItem systemItem = JBtnAct.GetComponent<SystemItem>();
                        systemItem.RegisterSystemClick(() =>
                        {
                            OnBtnClickAct();
                        });
                    }
                    ////下载
                    //{
                    //    JButton downloadbtn = M_TopLeftRoot.Find("PanelADD/LeftBtnLyout/BtnActivity").GetComponent<JButton>();
                    //    downloadbtn.OnClick += OnBtnClickBtnActivity;
                    //}
                }
                // 左二限时玩法
                {
                    EventTimeWidget = M_TopLeftRoot.Find("PanelADD/EventTimeWidget").gameObject;

                    GVE_Btn.onClick.RemoveAllListeners();
                    GVE_Btn.onClick.AddListener(OnGoToGVE);
                    //GVE_Btn.gameObject.SetActive(false);
                    GVE_Btn.transform.parent.gameObject.SetActive(false);

                    GNG_Btn.onClick.RemoveAllListeners();
                    GNG_Btn.onClick.AddListener(OnGoToGNG);
                    //GNG_Btn.gameObject.SetActive(false);
                    GNG_Btn.transform.parent.gameObject.SetActive(false);


                    Party_Btn.onClick.RemoveAllListeners();
                    Party_Btn.onClick.AddListener(OnGoToParty);
                    //Party_Btn.gameObject.SetActive(false);
                    Party_Btn.transform.parent.gameObject.SetActive(false);


                    Wild_Btn.onClick.RemoveAllListeners();
                    Wild_Btn.onClick.AddListener(OnGoToWild);
                    //Wild_Btn.gameObject.SetActive(false);
                    Wild_Btn.transform.parent.gameObject.SetActive(false);

                    PVP_Btn.onClick.RemoveAllListeners();
                    PVP_Btn.onClick.AddListener(OnGoToPVP);
                    //PVP_Btn.gameObject.SetActive(false);
                    PVP_Btn.transform.parent.gameObject.SetActive(false);
                }
                // 退出按钮
                {
                    Ectype_Btn.gameObject.SetActive(false);
                    Ectype_Btn.onClick.RemoveAllListeners();
                    Ectype_Btn.onClick.AddListener(OnQuitEctype);

                    BtnSetting.onClick.AddListener(OnOpenSetting);
                }
                // GM按钮
                {
                    Button Btn = M_MiddleCentreRoot.Find("GMBtn").GetComponent<Button>();
                    Btn.gameObject.SetActive(AppConfig.IsGM());
                    Btn.onClick.RemoveAllListeners();
                    Btn.onClick.AddListener(() =>
                    {
                        //UIRoot.GmRoot.SetActive(true);
                        //UIManager.Instance.OpenWidget(UIDef.GmWidget, true, null, UIRoot.UIROOT.transform, MainPageCommond.HideNone, true, true);
                        UIManager.Instance.OpenWidgetAsync(UIDef.GmWidget, null, true, null, UIRoot.UIROOT.transform, MainPageCommond.HideNone, true, true);
                    });
                }

                // 下方按钮
                {
                    DownCentreSonRoot = M_DownCentreRoot.Find("SonRoot").gameObject;
                    // 好友
                    {
                        JButton Btn = DownCentreSonRoot.transform.Find("JBtnFriend").GetComponent<JButton>();
                        Btn.OnClick = (go) =>
                        {
                            ModuleManager.Instance.SendMessage(ModuleDef.Name.FriendModule, "OnOpenFriendWindow", new object[] { });
                        };

                    }
                    //邮件
                    {
                        mMailBtn = DownCentreSonRoot.transform.Find("JBtnMail").GetComponent<JButton>();
                        mMailBtn.OnClick = (go) =>
                        {
                            ModuleManager.Instance.SendMessage(ModuleDef.Name.MailModule, "OnOpenMailWidget", new object[] { });
                        };
                    }
                    // 临时背包
                    {
                        /*JButton ectypeEnterBtn = M_TopRightRoot.Find("EctypeEnterBtn").GetComponent<JButton>();
                        ectypeEnterBtn.OnClick = (go) =>
                        {
                            ModuleManager.Instance.SendMessage(ModuleDef.EctypeEntranceModule, "OnOpenEctypeEntrance", new object[] { });
                        };*/
                        ectypeBagBtn = DownCentreSonRoot.transform.Find("JBtnEctypeBag").GetComponent<JButton>();
                        ectypeBagBtn.OnClick = (go) =>
                        {
                            ModuleManager.Instance.SendMessage(ModuleDef.Name.EctypeBagModule, "OnOpenEctypeBag", new object[] { });
                        };
                        ectypeBagBtn.gameObject.SetActive(false);
                    }
                    //切换摄像机视角
                    {
                        JButton Btn = DownCentreSonRoot.transform.Find("JBtnCamera").GetComponent<JButton>();
                        Btn.OnClick = (go) => { GlobalEvent.OnSwitchBattleCameraDefaultParam.Invoke(null); };
                    }
                    //GVEBoss奖励
                    {
                        gveBossRewardBtn = DownCentreSonRoot.transform.Find("BtnGVEBossReward").GetComponent<JButton>();
                        gveBossRewardBtn.OnClick = (go) =>
                        {
                            //UIManager.Instance.OpenWidget(UIDef.GVEBossRewardsWidget, false, GVEBossRewards);
                            UIManager.Instance.OpenWidgetAsync(UIDef.GVEBossRewardsWidget, null, false, GVEBossRewards);
                        };
                        gveBossRewardBtn.gameObject.SetActive(false);
                    }
                    //GNG奖励
                    {
                        gngBossRewardBtn = DownCentreSonRoot.transform.Find("BtnGNGBossReward").GetComponent<JButton>();
                        gngBossRewardBtn.OnClick = (go) =>
                        {
                            //UIManager.Instance.OpenWidget(UIDef.AfternoonGveFinishWidget, false, new object[] { GNGPersonRewardNtf, GNGGuildRewardNtf });
                            UIManager.Instance.OpenWidgetAsync(UIDef.AfternoonGveFinishWidget, null, false, new object[] { GNGPersonRewardNtf, GNGGuildRewardNtf });
                        };
                        gngBossRewardBtn.gameObject.SetActive(false);
                    }
                }

                //选择器
                // {
                //     JButton Btn = M_TopRightRoot.Find("Image/JBtnSelector").GetComponent<JButton>();
                //     Btn.OnClick = (go) =>
                //     {
                //         //ModuleManager.Instance.SendMessage(ModuleDef.Name.SelectorModule, "OnOpenSelectorByItemID", new object[] {21103005 });
                //         if (!cardOpened)
                //         {
                //             orgMat = RenderSettings.skybox;
                //             StartCoroutine(LoadScene(18));
                //             RenderSettings.skybox = cardMat;
                //         }
                //         else
                //         {
                //             UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(18);
                //             RenderSettings.skybox = orgMat;
                //             GameInput.Instance.gameObject.SetActive(true);
                //             GlobalModules.Instance.gameObject.SetActive(true);
                //         }
                //         cardOpened = !cardOpened;
                //     };
                // }

                // 符文
                {
                    //JButton Btn = m_MenuInfoPanel.Find("BtnRune").GetComponent<JButton>();
                    //Btn.OnClick = (go) => { OnBtnClickRune(); };
                }
                // 角色属性
                {
                    //JButton Btn = m_MenuInfoPanel.Find("BtnRole").GetComponent<JButton>();
                    //Btn.OnClick = (go) => { OnBtnClickAttr(); };
                }
                // 伙伴界面
                {
                    //JButton Btn = m_MenuInfoPanel.Find("BtnPartner").GetComponent<JButton>();
                    //Btn.OnClick = (go) =>
                    //{
                    //    UIManager.Instance.OpenWindow("Partner/Prefab/PartnerWindow", null, MainPageCommond.HideBoth);
                    //};
                }
                // 技能界面
                {
                    //JButton Btn = m_MenuInfoPanel.Find("BtnSkill").GetComponent<JButton>();
                    //Btn.OnClick = (go) =>
                    //{
                    //    ModuleManager.Instance.SendMessage(ModuleDef.Name.SkillWindowModule, "OnOpenSkill", new object[] { });
                    //};
                }
                //副本入口
                //{
                //    ectypeBtn = m_MenuInfoPanel.Find("BtnEctype").GetComponent<JButton>();
                //    ectypeBtn.OnClick = (go) =>
                //    {
                //        ModuleManager.Instance.SendMessage(ModuleDef.Name.GamePlayChoiceModule, "OnOpenGamePlayChoice", new object[] { });
                //    };
                //    ectypeBtn.gameObject.SetActive(true);
                //}
                // 公会
                {
                    //JButton Btn = m_MenuInfoPanel.Find("BtnUnion").GetComponent<JButton>();
                    //Btn.OnClick = (go) =>
                    //{
                    //    ModuleManager.Instance.SendMessage(ModuleDef.Name.GuildModule, "OnOpenGuild", new object[] { });
                    //};
                }

            }

            M_TreasureRoot = M_MiddleRightRoot.Find("TreasureRoot").transform;
            _normalTreasure = M_TreasureRoot.Find("NormalTreasure").gameObject.GetComponent<NormalTreasure>();
            _treasureBtn = M_TreasureRoot.Find("AdvancedTreasure").gameObject.GetComponent<AdvancedTreasure>();
            SkipBtn.OnClick += OnClickSkipHandler;

        }



        protected override void OnOpen(object arg)
        {
            SGF.Debuger.LogWarning($"{LOG_TAG} 启动HUD");
            ForcePlayAnimation("StarWorldPage_Open");

            base.OnOpen(arg);

            m_Mymodule = ModuleManager.Instance.GetModule(ModuleDef.Name.StarWorldModule) as StarWorldModule;

            InitFunctionRoot();

            //打开快速穿戴面板
            ModuleManager.Instance.SendMessage(ModuleDef.Name.QuickEquipModule, "OnOpenQuickEquip", null);
            //打开跑马灯
            ModuleManager.Instance.SendMessage(ModuleDef.Name.RunHorseModule, "OnOpenRunHorse", null);
            //打开掉落面板
            ModuleManager.Instance.SendMessage(ModuleDef.Name.DropInfoListModule, "OnOpenDrop", null);
            //打开交互面板
            ModuleManager.Instance.SendMessage(ModuleDef.Name.ObjectInteractiveModule, "OnOpenObjectInteractive", null);
            // 客户端全量请求通缉信息
            ModuleManager.Instance.SendMessage(ModuleDef.Name.WantedModule, "OnSendWTaskGetAllInfoReq", null);
            // 
            ModuleManager.Instance.SendMessage(ModuleDef.Name.GuildModule, "OnGuildModuleShow", null);
            // 清空旧的聊天信息
            ModuleManager.Instance.SendMessage(ModuleDef.Name.ChatModule, "OnClearAllChatItem", null);

            // 优先创建提示界面
            {
                //UIManager.Instance.OpenWidget(UIDef.InterActionObject, false, null, M_MiddleCentreRoot, StarProjectDef.MainPageCommond.HideNone, true, false);
                UIManager.Instance.OpenWidgetAsync(UIDef.InterActionObject, null, false, null, M_MiddleCentreRoot, StarProjectDef.MainPageCommond.HideNone, true, false);
            }
            // 伙伴图标的widget 
            {
                //UIManager.Instance.OpenWidget(UIDef.PartnerWidget, false, null, M_DownRightRoot, MainPageCommond.HideNone, true, false);
                UIManager.Instance.OpenWidgetAsync(UIDef.PartnerWidget, null, false, null, M_DownRightRoot, MainPageCommond.HideNone, true, false);
            }

            BindMsg();

            GlobalEvent.OnMainPageLoadedComplete?.Invoke(true);
            GameManager.Instance.IsLoginInitState = transform;

            OnSceneMapLoadComplete(0);
            //SGF.Debuger.Log($"[公告] StarWorldPage Show set IsLoginInitState : {GameManager.Instance.IsLoginInitState}");


            LuaModule luaModule = ModuleManager.Instance.GetModule(ModuleDef.Name.ActivityModule) as LuaModule;
            LuaModule beginnerluaModule = ModuleManager.Instance.GetModule(ModuleDef.Name.BeginnerTargetModule) as LuaModule;
            if (luaModule != null && beginnerluaModule != null)
            {
                var tb = luaModule.GetLuaTable();
                var beginnertb = beginnerluaModule.GetLuaTable();
                FuncBool _GetBginnerRedPoint = beginnertb.Get<FuncBool>("GetRedPointNum");

                FuncBool _GetRedPoint = tb.Get<FuncBool>("GetRedPoint");
                FuncBool _GetRedPoint2 = tb.Get<FuncBool>("GetRedPoint2");
                FuncBool _GetGamePlayPreviewRedPoint = tb.Get<FuncBool>("GetGamePlayPreviewRedPoint");

                var checkInMDMgr = FixMessageManager.Instance.GetMDMgr(FixUpdateDef.CheckIn) as CheckInMDMgr;
                var chekinMD = checkInMDMgr.GetCheckInMD();
                if (chekinMD != null)
                {
                    bool redPoint = _GetRedPoint(tb);
                    bool redPoint2 = _GetRedPoint2(tb);
                    bool redPoint3 = _GetGamePlayPreviewRedPoint(tb);
                    bool redPointBeginner = _GetBginnerRedPoint(beginnertb);
                    SetActRedpoint(redPoint || redPoint2 || redPoint3 || redPointBeginner);
                }
            }

            LuaModule luaModule2 = ModuleManager.Instance.GetModule(ModuleDef.Name.OnlineRewardModule) as LuaModule;
            if (luaModule2 != null)
            {
                var tb = luaModule2.GetLuaTable();

                tb.Get("IsAllFinish", out IsAllFinishEvent);
                tb.Get("CanGetOnlineReward", out CanGetOnlineRewardEvent);
                tb.Get("GetOnlineRewardTime", out GetOnlineRewardTimeEvent);

                MonoHelper.RemoveUpdateListener(StartTick);
                MonoHelper.AddUpdateListener(StartTick);
                StartTick();
            }

            GlobalEvent.OnRefreshHudTaskIndex.Invoke(1);
            // TODO:曲
            // 这里红点逻辑写的不对
            //进入主界面开始下载分包内容
       //     DlcDownload.Instance.StartDlc();
        }

        void OnClickSkipHandler(GameObject go)
        {
            SDKManager.Instance.EventPreNewPlayerEvent("15_0");
            DoSkip();
        }


        private void DoSkip(System.Action onCancle = null)
        {
            UIAPI.ShowMsgBox(76, (evtName) =>
            {
                if (evtName == "SURE")
                {
                    // 上报服务器
                    ProtoMsg.JumpSpaceReq jumpSpaceReq = new();
                    jumpSpaceReq.JumpType = 1;
                    SocketBase battleSocket = NetworkManager.Instance.gameSocket;
                    battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, jumpSpaceReq, false);

                    SDKManager.Instance.EventPreNewPlayerEvent("15_1");
                    //打点日志
                }
                else
                {
                    SDKManager.Instance.EventPreNewPlayerEvent("15_2");
                    onCancle?.Invoke();
                }
            });
        }
        private void OnTaskChange(int arg0, int arg1)
        {
            RefreshPreviewShowAndRedPoint();
        }

        private void RefreshPreviewShowAndRedPoint()
        {
            LuaModule luaModule = ModuleManager.Instance.GetModule(ModuleDef.Name.ActivityModule) as LuaModule;
            if (luaModule != null)
            {
                var tb = luaModule.GetLuaTable();
                FuncBool _GetGamePlayPreviewRedPoint = tb.Get<FuncBool>("GetGamePlayPreviewRedPoint");
                bool haveRedPoint = false;
                if (_GetGamePlayPreviewRedPoint != null)
                {
                    haveRedPoint = _GetGamePlayPreviewRedPoint(tb);
                }

                bool canShow = false;
                FuncBool _IsPreviewTabCanShow = tb.Get<FuncBool>("IsPreviewTabCanShow");
                if (_IsPreviewTabCanShow != null)
                {
                    canShow = _IsPreviewTabCanShow(tb);
                }

                go_BtnPreview?.SetActive(canShow);
                go_BtnPreviewRedPoint?.SetActive(haveRedPoint);
            }
        }

        private void InitFunctionRoot()
        {
            FunctionRoot.Clear();
            FunctionRoot.Add(ModuleDef.Name.TaskModule, TaskRoot);
            FunctionRoot.Add(ModuleDef.Name.SecretAreaModule, TaskRoot);
            FunctionRoot.Add(ModuleDef.Name.DailyTeamModule, TaskRoot);
            FunctionRoot.Add(ModuleDef.Name.WantedModule, M_TopRightRoot);
            FunctionRoot.Add(ModuleDef.Name.AfternoonGveModule, TaskRoot);
            FunctionRoot.Add(ModuleDef.Name.PartyTimeModule, TaskRoot);
            FunctionRoot.Add(ModuleDef.Name.ExchangeModule, M_TopRightRoot);
            FunctionRoot.Add(ModuleDef.Name.FriendModule, null);
            FunctionRoot.Add(ModuleDef.Name.PvpModule, TaskRoot);
            FunctionRoot.Add(ModuleDef.Name.PersonalTowerModule, TaskRoot);
            FunctionRoot.Add(ModuleDef.Name.EctypeEntranceModule, null);
            m_Mymodule.OnOpenPage(GetFuncTransform);
        }

        // IEnumerator LoadScene(int sceneId)
        // {
        //     AsyncOperation op = SceneManager.LoadSceneAsync(sceneId, LoadSceneMode.Additive);
        //     op.allowSceneActivation = true;
        //     while (!op.isDone)
        //     {
        //         yield return new WaitForEndOfFrame();
        //     }

        //     yield return new WaitForEndOfFrame();

        //     var scene = SceneManager.GetSceneByBuildIndex(18);
        //     UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);

        //     GameInput.Instance.gameObject.SetActive(false);
        //     GlobalModules.Instance.gameObject.SetActive(false);
        // }

        private void OnEndTreasure(int subType)
        {
            if (GameManager.Instance.GetCurMapType() != ProtoMsg.SpaceType.SpaceScene)
            {
                // 非场景地图，不处理
                BusinessManager.Instance.DigTreasureEndNtfCacheType = subType;
                return;
            }
            if (_normalTreasure == null)
            {
                return;
            }
            ulong entityID = 0;
            var  itemSubType = 0;
            var items = BusinessManager.Instance.GetItemsByItemType(20, subType);
            if (items != null && items.Count > 0)
            {
                foreach (var item in items)
                {
                    if (item.TData != null)
                    {
                        if (item.TData.TreasureMapID == GameManager.Instance.GetCurMapId() && subType == item.TData.SubType)
                        {
                            entityID = item.EntityID;
                            itemSubType = item.TData.SubType;
                            break;
                        }
                    }
                }

                if (entityID == 0)
                {
                    foreach (var item in items)
                    {
                        entityID = item.EntityID;
                        itemSubType = item.TData.SubType;
                        if (entityID > 0)
                        {
                            break;
                        }
                    }

                }
            }

            if (entityID > 0)
            {
                _normalTreasure.SetData(2, entityID, itemSubType == 1);
            }
            else
            {
                _normalTreasure.SetData(0, 0);
            }
        }

        private void OnInterChange(string key, object value)
        {
            ulong uid = GameManager.Instance.M_MainPlayerCtrlBase.Data.Attrs.GetAoiValue<ulong>(EnumAOIType.Ulong, key);
            if (uid > 0)
            {
                if (_normalTreasure != null)
                {
                    _normalTreasure.SetData(0, 0);
                }

            }
        }

        /// <summary>
        /// 监听冒险等级变化
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void OnRiskInfoChange(string key, object value)
        {
            RiskLevelMD RiskInfo = null;
            object info = GameManager.Instance.M_MainPlayerCtrlBase?.Data?.Attrs?.GetProtoValue(AOIAttrDefine.RiskInfo);
            if (info == null)
            {
                return;
            }
            else
            {
                RiskInfo = (ProtoMsg.RiskLevelMD)info;
            }

            var curRiskExp = RiskInfo.CurRiskExp;
            var curRiskLevel = RiskInfo.CurRiskLevel;
            var cfg = LocalDataManager.Instance.GetAdvGradeExpDataCell(curRiskLevel);

            //判断是否可以突破
            bool canRankUp = false;

            //判断是否可以升级
            bool canLvUp = false;

            if (cfg == null)
            {
                return;
            }
            if (cfg.GetIfBreak())
            {
                int GroupID = cfg.GetBreakConditionGroupID();
                canRankUp = GameManager.Instance.IsConditionMete(GroupID); //判断是否满足
            }
            else
            {
                var cfgNext = LocalDataManager.Instance.GetAdvGradeExpDataCell(curRiskLevel + 1);
                if (cfgNext != null && curRiskExp >= cfg.GetExp())
                {
                    canLvUp = true;
                }
            }

            bool isSysOpen = BusinessManager.Instance.SystemIsOpen(StarProjectDef.SystemOpenType.AdventureLV);

            //红点
            RedPointManager.Instance.TriggerConditionTypeBoolValue(RedPointConditionType.Adventure, isSysOpen && (canRankUp || canLvUp));


        }

        /// <summary>
        /// 当玩家 等级 发生变化的时候的 通知
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void OnPlayerLevelChange(string key, object value)
        {
            var level = GameManager.Instance.M_MainPlayerCtrlBase.Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, key);
            if (level > 0)
            {
                GlobalEvent.OnPlayerDataChange?.Invoke(AOIAttrDefine.PlayerLevel, level);
                RefreshPreviewShowAndRedPoint();
            }
        }

        int GveTreasureInterID = 0;
        private void OnTreasureMonidChange(string key, object value)
        {
            TreMonID treMonID = GameManager.Instance.M_MainPlayerCtrlBase.Data.Attrs.GetProtoValue(AOIAttrDefine.TreasureMonid) as TreMonID;


            if (treMonID != null && treMonID.ETreID > 0)
            {
                //全部杀光
                if (treMonID.DeadNum == treMonID.MonIDS.Count)
                {



                    //藏宝图数据
                    var treasureData = BusinessManager.Instance.GetTreasureData();
                    if (treasureData != null)
                    {
                        if (treasureData.TreasureEID != 0 && treasureData.TreasureMapID != 0)
                        {
                            if (GveTreasureInterID > 0)
                            {
                                treasureData.TreasureInterID = GveTreasureInterID;
                            }

                            /*
                            var entityid = GameManager.Instance.GetInterEntityID(treasureData.TreasureInterID);//交互物id
                            
                            if (entityid > 0)
                            {
                                ModuleManager.Instance.SendMessage(ModuleDef.Name.InterActionModule, "QueryInter", entityid);
                            }
                            */

                            GameManager.Instance.UpdateAllNpcServiceState();

                        }
                        else
                        {
                            Debug.Log("藏宝图属性为空");
                        }
                    }



                }
                else
                {

                    //藏宝图数据
                    var treasureData = BusinessManager.Instance.GetTreasureData();
                    if (treasureData != null)
                    {
                        if (treasureData.TreasureEID != 0 && treasureData.TreasureMapID != 0 &&
                            treasureData.TreasureInterID != 0)
                        {
                            GveTreasureInterID = treasureData.TreasureInterID;

                            treasureData.TreasureInterID = 0;

                            GameManager.Instance.UpdateAllNpcServiceState();

                        }
                    }
                }

            }




        }

        private void OnCurTreasureChange(string key, object value)
        {
            var _isTreasuring = false;
            var _isNormalTreasure = false;
            var treasureData = BusinessManager.Instance.GetTreasureData();
            if (treasureData != null)
            {
                if (treasureData.TreasureEID != 0 && treasureData.TreasureMapID != 0 &&
                    treasureData.TreasureInterID != 0)
                {

                    // var entityid = GameManager.Instance.GetInterEntityID(treasureData.TreasureInterID);//交互物id
                    // Debug.LogError("藏宝图交互物件 实体ID:" + entityid);

                    //_isNormalTreasure
                    var item = BusinessManager.Instance.GetItemByItemEntityId(treasureData.TreasureEID);
                    if (item != null)
                    {
                        var config = LocalDataManager.Instance.GetItemDataCell(item.BaseID);
                        if (config != null)
                        {
                            if (config.GetItemType() == 20)
                            {
                                if (config.GetSubType() == 1)
                                {
                                    if (GameManager.Instance.GetCurMapId() == treasureData.TreasureMapID)
                                    {
                                        _isTreasuring = true;
                                    }
                                }
                                else
                                {
                                    if (config.GetSubType() == 0 || config.GetSubType() == 2)
                                    {
                                        if (GameManager.Instance.GetCurMapType() == ProtoMsg.SpaceType.SpaceScene)
                                        {
                                            _isNormalTreasure = true;
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("藏宝图属性为空");
                }
            }

            if (_treasureBtn != null)
            {
                if (_isTreasuring)
                {
                    _treasureBtn.SetTarget(new Vector3(treasureData.TreasurePos.X, treasureData.TreasurePos.Y,
                        treasureData.TreasurePos.Z));
                }

                _treasureBtn.gameObject.SetActive(_isTreasuring);
            }

            if (_normalTreasure != null)
            {
                if (_isNormalTreasure)
                {
                    _normalTreasure.SetData(1, treasureData.TreasureEID);
                    _normalTreasure.gameObject.SetActive(true);
                }
                else
                {
                    if (!_normalTreasure.IsUsing())
                    {
                        _normalTreasure.gameObject.SetActive(false);
                    }
                }


            }
            GameManager.Instance.UpdateAllNpcServiceState();
        }

        protected override void OnClose(object arg)
        {
            ForcePlayAnimation("StarWorldPage_Close");

            m_Mymodule.OnClosePage();
            OpenEnemyInfoWidget(null);
            FunctionRoot.Clear();
            UIManager.Instance.CloseWidget(UIDef.PlayerInfoWidget, M_DownRightRoot, false);
            //ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, "CloseTaskWindow", new object[] { });
            m_LuaFunSetEnemyInfo = null;
            M_EnemyInfoWidget = null;
            mainPlayerEntity = null;

            if (m_MenuPanelTweener != null)
            {
                m_MenuPanelTweener.onComplete = null;
                m_MenuPanelTweener.Kill(true);
            }

            m_IsOpen = true;
            m_MenuPanelTweener = null;
            OffMsg();
            m_LastAnimName = string.Empty;
            lastFinalEveryPartNeedHide = MainPageCommond.HideNone;

            ModuleManager.Instance.SendMessage(ModuleDef.Name.GuildModule, "OnGuildModuleHide", new object[] { });
            ModuleManager.Instance.SendMessage(ModuleDef.Name.ChatModule, "OnOpenChatWindowShow", new object[] { false });


            ModuleManager.Instance.SendMessage(ModuleDef.Name.PartyTimeModule, "OnDeleteHud", new object[] { });
            ModuleManager.Instance.SendMessage(ModuleDef.Name.AfternoonGveModule, "OnDeleteHud", new object[] { });


            if (GameManager.Instance.M_MainPlayerCtrlBase != null && GameManager.Instance.M_MainPlayerCtrlBase.Data != null)
            {
                GameManager.Instance.M_MainPlayerCtrlBase.Data.UnRegisterAttribute(AOIAttrDefine.CurTreasure, OnCurTreasureChange);
                GameManager.Instance.M_MainPlayerCtrlBase.Data.UnRegisterAttribute(AOIAttrDefine.TreasureMonid, OnTreasureMonidChange);
                GameManager.Instance.M_MainPlayerCtrlBase.Data.UnRegisterAttribute(AOIAttrDefine.InteractID, OnInterChange);
                GameManager.Instance.M_MainPlayerCtrlBase.Data.UnRegisterAttribute(AOIAttrDefine.PlayerLevel, OnPlayerLevelChange);
                GameManager.Instance.M_MainPlayerCtrlBase.Data.UnRegisterAttribute(AOIAttrDefine.RiskInfo, OnRiskInfoChange);
            }

            // -----------------------GM
            //if (GameManager.Instance.M_MainPlayerCtrlBase != null)
            //{
            //    GameManager.Instance.M_MainPlayerCtrlBase.entityBaseData.ActionStateChange -= OnStateChange;
            //}
            // -----------------------
        }

        private void BindMsg()
        {
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.GVEBossRewardNtfID, OnGVEBossRewardNtf, this);

            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.GNGPersonRewardNtfID, OnGNGPersonRewardNtf, this);
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.GNGGuildRewardNtfID, OnGNGGuildRewardNtf, this);

            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.RoleReviveRetID, OnRoleReviveRetNtf, this);
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.WorldLevelRetID, OnWorldLevelRet, this);

            GlobalEvent.onEnemyInfo.AddListener(OpenEnemyInfoWidget);
            GlobalEvent.onMainPlayerDie.AddListener(OpenWaitToBornWindow);
            GlobalEvent.onMainPlayerDie.AddListener(SoundManager.Instance.OnPlayerDie);
            GlobalEvent.OnSetTaskWidgetShow.AddListener(OnEntryEctype);
            GlobalEvent.OnSetEventTime.AddListener(OnSetEventTime);
            GlobalEvent.OnRefeshShopBtn.AddListener(OnRefeshShopBtn);


            GlobalEvent.OnClearGVEBossReward.AddListener(OnClearGVEBossReward);
            GlobalEvent.OnClearGNGBossReward.AddListener(OnClearGNGBossReward);

            GlobalEvent.OnFreshExitTime.AddListener(SetEctypeLimtTime);

            GlobalEvent.OnChatHudShowEvent.AddListener(OnChatHudShowEvent);
            GlobalEvent.OnEndTreasure.AddListener(OnEndTreasure);

            GlobalEvent.ShowTaskInfoPanel.AddListener(SetTaskInfoPanelShow);

            GlobalEvent.OnSceneMapConfigLoad.AddListener(OnSceneMapLoadComplete);
            GlobalEvent.TaskStageChange.AddListener(OnTaskChange);

            GlobalEvent.OnXinSGUIShow.AddListener(OnXinSGUIShowCB);

            FixMessageManager.Instance.OnMessage(FixUpdateDef.UserSundry, OnUserSundryRefesh);
        }

        public void OffMsg()
        {
            FixMessageManager.Instance.OffMessage(FixUpdateDef.UserSundry, OnUserSundryRefesh);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.GVEBossRewardNtfID, OnGVEBossRewardNtf, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.GNGPersonRewardNtfID, OnGNGPersonRewardNtf, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.GNGGuildRewardNtfID, OnGNGGuildRewardNtf, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.RoleReviveRetID, OnRoleReviveRetNtf, this);
            NetworkManager.Instance.OffMessageEnum(MsgIDEnum.WorldLevelRetID, OnWorldLevelRet, this);

            GlobalEvent.onEnemyInfo.RemoveListener(OpenEnemyInfoWidget);
            GlobalEvent.onMainPlayerDie.RemoveListener(OpenWaitToBornWindow);
            GlobalEvent.onMainPlayerDie.RemoveListener(SoundManager.Instance.OnPlayerDie);
            GlobalEvent.OnSetTaskWidgetShow.RemoveListener(OnEntryEctype);
            GlobalEvent.OnSetEventTime.RemoveListener(OnSetEventTime);

            GlobalEvent.OnRefeshShopBtn.RemoveListener(OnRefeshShopBtn);

            GlobalEvent.OnClearGVEBossReward.RemoveListener(OnClearGVEBossReward);
            GlobalEvent.OnClearGNGBossReward.RemoveListener(OnClearGNGBossReward);

            GlobalEvent.OnFreshExitTime.RemoveListener(SetEctypeLimtTime);
            GlobalEvent.OnChatHudShowEvent.RemoveListener(OnChatHudShowEvent);
            GlobalEvent.OnEndTreasure.RemoveListener(OnEndTreasure);

            GlobalEvent.ShowTaskInfoPanel.RemoveListener(SetTaskInfoPanelShow);

            GlobalEvent.OnSceneMapConfigLoad.RemoveListener(OnSceneMapLoadComplete);
            GlobalEvent.TaskStageChange.RemoveListener(OnTaskChange);

            GlobalEvent.OnXinSGUIShow.RemoveListener(OnXinSGUIShowCB);

            MonoHelper.RemoveUpdateListener(StartTick);
            MonoHelper.RemoveSecTimeUpdateListener(OnSecTimeUpdate, MonoHelper.E_ModuleType.CommonBusiness);
            MonoHelper.RemoveSecTimeUpdateListener(OnEventTimeUpdate, MonoHelper.E_ModuleType.CommonBusiness);
        }

        private void OnUserSundryRefesh(FixMessageManager.FixMessageNotifyData data)
        {
            RefreshPreviewShowAndRedPoint();
        }

        private string m_LastAnimName = string.Empty;
        private MainPageCommond lastFinalEveryPartNeedHide = MainPageCommond.HideNone;
        public void PlayAnimByState(bool isOpen, MainPageCommond finalEveryPartNeedHide /*, string triggerUIName*/)
        {
            string animName = string.Empty;
            if ((finalEveryPartNeedHide & MainPageCommond.PageHide) == MainPageCommond.PageHide)
            {
                animName = "StarWorldPage_Close";
            }
            else if ((finalEveryPartNeedHide & MainPageCommond.HudHideRight) == MainPageCommond.HudHideRight)
            {
                animName = "StarWorldPage_RightOut";
            }
            else if (finalEveryPartNeedHide == MainPageCommond.HideNone && lastFinalEveryPartNeedHide == MainPageCommond.HudHideRight)
            {
                animName = "StarWorldPage_RightGo";
            }
            else if (finalEveryPartNeedHide == MainPageCommond.HideNone)
            {
                animName = "StarWorldPage_Open";
            }
            //SGF.Debuger.LogWarning($"主界面动画 isOpen={isOpen},finalEveryPartNeedHide={finalEveryPartNeedHide},animName={animName},m_LastAnimName={m_LastAnimName}");

            lastFinalEveryPartNeedHide = finalEveryPartNeedHide;
            System.Action action = () =>
            {
                if (!isOpen)
                {
                    //GlobalEvent.OnMainPageAnimCallBack?.Invoke(triggerUIName, !isOpen);
                }
                SetAreaActive(finalEveryPartNeedHide);
            };

            if (animName == string.Empty)
            {
                action?.Invoke();
                return;
            }

            if (m_LastAnimName != string.Empty && animName == m_LastAnimName)
            {
                action?.Invoke();
                return;
            }
            m_LastAnimName = animName;
            //SGF.Debuger.LogError($"主界面动画 isOpen={isOpen},finalEveryPartNeedHide={finalEveryPartNeedHide},animName={animName},m_LastAnimName={m_LastAnimName}");
            if (m_LastAnimName == "StarWorldPage_Open" || m_LastAnimName == "StarWorldPage_RightGo")
            {
                // 回到主界面的时候刷新转职红点
                RedPointManager.Instance.TriggerConditionType(RedPointConditionType.TransJob);


            }

            PlayAnimation(animName, action);
        }

        private void SetAreaActive(MainPageCommond finalEveryPartNeedHide)
        {
            //SGF.Debuger.LogError($"主界面动画 finalEveryPartNeedHide={finalEveryPartNeedHide}");
            return;
            //M_TopLeftRoot.gameObject.SetActive(!((finalEveryPartNeedHide & MainPageCommond.HidePageTopLeft) == MainPageCommond.HidePageTopLeft));

            //M_TopCentreRoot.gameObject.SetActive(!((finalEveryPartNeedHide & MainPageCommond.HidePageTopCentre) == MainPageCommond.HidePageTopCentre));

            //M_TopRightRoot.gameObject.SetActive(!((finalEveryPartNeedHide & MainPageCommond.HidePageTopRight) == MainPageCommond.HidePageTopRight));

            //M_MiddleLeftRoot.gameObject.SetActive(!((finalEveryPartNeedHide & MainPageCommond.HidePageModdleLeft) == MainPageCommond.HidePageModdleLeft));

            //M_MiddleCentreRoot.gameObject.SetActive(!((finalEveryPartNeedHide & MainPageCommond.HidePageModdleCentre) == MainPageCommond.HidePageModdleCentre));

            //M_MiddleRightRoot.gameObject.SetActive(!((finalEveryPartNeedHide & MainPageCommond.HidePageModdleRight) == MainPageCommond.HidePageModdleRight));

            //M_DownLeftRoot.gameObject.SetActive(!((finalEveryPartNeedHide & MainPageCommond.HidePageDownLeft) == MainPageCommond.HidePageDownLeft));

            //M_DownCentreRoot.gameObject.SetActive(!((finalEveryPartNeedHide & MainPageCommond.HidePageDownCentre) == MainPageCommond.HidePageDownCentre));

            //M_DownRightRoot.gameObject.SetActive(!((finalEveryPartNeedHide & MainPageCommond.HidePageDownRight) == MainPageCommond.HidePageDownRight));
        }

        private void OnOpenSetting()
        {
            UIManager.Instance.OpenWindowAsync(UIDef.SettingWindow, null, null, MainPageCommond.HideBoth);
        }

        private void OnQuitEctype()
        {
            //EctypeModule module = ModuleManager.Instance.GetModule(ModuleDef.EctypeModule) as EctypeModule;
            //if (module.MapSpaceType == SpaceType.SpaceDaily)
            //{
            //    UIAPI.ShowMsgBox(17, (EventName) =>
            //    {
            //        if (EventName == "SURE")
            //        {
            //            StarWorldModule starWorld = (StarWorldModule)ModuleManager.Instance.GetModule(ModuleDef.StarWorldModule);
            //            if (starWorld != null)
            //            {
            //                starWorld.BackToWorld();
            //            }
            //        }
            //    });
            //    return;
            //}
            string mapName = GameManager.Instance.GetCurSceneLevelName();
            UIAPI.ShowMsgBox(EctypeQuitID, (EventName) =>
            {
                if (EventName == "SURE")
                {
                    StarWorldModule starWorld = (StarWorldModule)ModuleManager.Instance.GetModule(ModuleDef.Name.StarWorldModule);
                    if (starWorld != null)
                    {
                        starWorld.BackToWorld();
                    }

                    StarProject.GlobalEvent.OnMiniMapShowEctypeTask.Invoke(-1);
                }
            }, new object[] { mapName });
        }

        //
        private const int EctypeQuitID = 1;
        private int EctypeLimtTime;

        /// <summary>
        /// 进入副本前状态 0:未设置过， 1 张开 ；2 合着
        /// </summary>
        //private int PreEnterEctypeState = 0;


        private void SetEctypeLimtTime(long endTime)
        {
            EctypeLimtTime = (int)((endTime - TimeUtils.ServerNowStampMilli) / 1000); // data.GetLimitTime();
            if (EctypeLimtTime > 0)
            {
                if (m_Ectype_Time != null)
                {
                    m_Ectype_Time.text = TimeUtils.GetTimeStringV2("%mm:%ss", EctypeLimtTime);
                }

                MonoHelper.RemoveSecTimeUpdateListener(OnSecTimeUpdate, MonoHelper.E_ModuleType.CommonBusiness);
                MonoHelper.AddSecTimeUpdateListener(OnSecTimeUpdate, MonoHelper.E_ModuleType.CommonBusiness);
            }
        }

        private void OnEntryEctype(bool entriy)
        {
            entriy = !entriy;
            SpaceType spaceType = GameManager.Instance.GetCurMapType();
            MonoHelper.RemoveSecTimeUpdateListener(OnSecTimeUpdate, MonoHelper.E_ModuleType.CommonBusiness);
            EctypeLimtTime = 0;
            if (entriy)
            {
                EctypeModule module = ModuleManager.Instance.GetModule(ModuleDef.Name.EctypeModule) as EctypeModule;
                //进入副本默认关闭箭头
                //if (m_IsOpen)
                //{
                //    PreEnterEctypeState = 1;
                //    OnBtnClickArrow();
                //}
                //else
                //{
                //    PreEnterEctypeState = 2;
                //}

                // 开始倒计时
                SGF.Debuger.LogWarning($"副本时间  结束时间{module.EctypeEndTime}  当前服务器时间 ={TimeUtils.ServerNowStampMilli}  剩余时间{EctypeLimtTime}");
                EctypeLimtTime = 0;
                m_Ectype_Time.text = "";
                //module.MapID
                switch (spaceType)
                {
                    case SpaceType.SpaceDefault:
                        break;
                    case SpaceType.SpaceScene:
                        break;
                    case SpaceType.SpaceMirror:
                    case SpaceType.SpacePlot:
                    case SpaceType.SpaceDaily:
                    case SpaceType.SpaceTeamDaily:
                    case SpaceType.SpaceMirrorTreasure:
                    case SpaceType.SpacePlotTreasure:
                    case SpaceType.SpaceGng:
                    case SpaceType.Space10V10:
                    case SpaceType.SpaceArena:
                    case SpaceType.SpacePersonTower:
                        {
                            // 副本持续时间
                            //EctypeQuitID = 1;
                            EctypeLimtTime = (int)((module.EctypeEndTime - TimeUtils.ServerNowStampMilli) / 1000); // data.GetLimitTime();
                            if (EctypeLimtTime > 0)
                            {
                                m_Ectype_Time.text = TimeUtils.GetTimeStringV2("%mm:%ss", EctypeLimtTime);
                                MonoHelper.AddSecTimeUpdateListener(OnSecTimeUpdate, MonoHelper.E_ModuleType.CommonBusiness);
                            }
                        }
                        break;
                    case SpaceType.SpaceSercet:
                        // 秘境不需要副本倒计时
                        break;
                    case SpaceType.SpaceWtask:
                        {
                            EctypeLimtTime = SystemConstConfigs.TeamWantedTimeLimits * 60;
                            if (EctypeLimtTime > 0)
                            {
                                m_Ectype_Time.text = TimeUtils.GetTimeStringV2("%mm:%ss", EctypeLimtTime);
                                MonoHelper.AddSecTimeUpdateListener(OnSecTimeUpdate, MonoHelper.E_ModuleType.CommonBusiness);
                            }
                        }
                        break;
                    default:
                        break;
                }
                //GlobalEvent.OnOpenEctypeBag.Invoke(module.MapSpaceType == SpaceType.SpaceDaily);
            }
            else
            {
                //退出副本 还原箭头
                //if (PreEnterEctypeState > 0)
                //{
                //    m_IsOpen = PreEnterEctypeState == 2;
                //    OnBtnClickArrow();
                //    PreEnterEctypeState = 0;
                //}

                //EctypeQuitID = 0;
                EctypeLimtTime = 0;
                ectypeBagBtn.gameObject.SetActive(false);
                GlobalEvent.OnOpenEctypeBag.Invoke(false);
            }
        }

        // 副本倒计时
        private void OnSecTimeUpdate()
        {
            if (EctypeLimtTime > 0)
            {
                EctypeLimtTime--;
                if (m_Ectype_Time != null)
                {
                    m_Ectype_Time.text = TimeUtils.GetTimeStringV2("%mm:%ss", EctypeLimtTime);
                }
            }
        }

        // 测试返回login场景
        private void OnTestBackStarWorld()
        {
            m_Mymodule.TestBackStarWorld();
        }

        public void CreateMainPlayerPanel()
        {
            if (GameManager.Instance.mainPlayerId != 0)
            {
                //NPCEntityBase entityBase = (NPCEntityBase)GameManager.Instance.M_MainPlayerCtrlBase.M_Curr;
                //UIManager.Instance.OpenWidget(UIDef.PlayerInfoWidget, false, GameManager.Instance.M_MainPlayerCtrlBase.M_Curr, M_DownRightRoot, MainPageCommond.HideNone, true, false);
                UIManager.Instance.OpenWidgetAsync(UIDef.PlayerInfoWidget, null, false, GameManager.Instance.M_MainPlayerCtrlBase.M_Curr, M_DownRightRoot, MainPageCommond.HideNone, true, false);
                //UIManager.Instance.OpenWidget(UIDef.PlayerInfoWidget2, false, GameManager.Instance.M_MainPlayerCtrlBase.M_Curr, M_DownCentreRoot, MainPageCommond.HideNone, true, false);
                UIManager.Instance.OpenWidgetAsync(UIDef.PlayerInfoWidget2, null, false, GameManager.Instance.M_MainPlayerCtrlBase.M_Curr, M_DownCentreRoot, MainPageCommond.HideNone, true, false);
                //打开聊天窗口
                ModuleManager.Instance.SendMessage(ModuleDef.Name.ChatModule, "OnOpenChat", null);
                //打开弹幕面板
                ModuleManager.Instance.SendMessage(ModuleDef.Name.ChatModule, "OnOpenChatBarrage", new object[] { M_TopCentreRoot });
                //打开HUD聊天挂件
                ModuleManager.Instance.SendMessage(ModuleDef.Name.ChatModule, "OnOpenChatHud", new object[] { DownCentreSonRoot.transform });
                // 通缉任务
                ModuleManager.Instance.SendMessage(ModuleDef.Name.WantedModule, "InformWantedWindow", new object[] { 0 });
                ModuleManager.Instance.SendMessage(ModuleDef.Name.TaskModule, "InformWantedWindow", new object[] { 0 });

                //开始同步活动时间
                ModuleManager.Instance.SendMessage(ModuleDef.Name.EventModule, "OnCalEventOpenTime", new object[] { });
            }

            // 创建小地图挂件
            //UIManager.Instance.OpenWidget(UIDef.MiniMapWidget, false, null, M_MiniMapRoot, MainPageCommond.HideNone, true, false);
            UIManager.Instance.OpenWidgetAsync(UIDef.MiniMapWidget, null, false, null, M_MiniMapRoot, MainPageCommond.HideNone, true, false);

#if STAR_DEV || GM || UNITY_EDITOR
            // 原子锁action显示，临时代码
            GameManager.Instance.M_MainPlayerCtrlBase.entityBaseData.ActionStateChange += OnStateChange;
#endif

            GameManager.Instance.M_MainPlayerCtrlBase.Data.RegisterAttribute(AOIAttrDefine.CurTreasure, OnCurTreasureChange);
            OnCurTreasureChange(AOIAttrDefine.CurTreasure, null);

            //GVE藏宝图玩家属性监听
            GameManager.Instance.M_MainPlayerCtrlBase.Data.RegisterAttribute(AOIAttrDefine.TreasureMonid, OnTreasureMonidChange);
            OnTreasureMonidChange(AOIAttrDefine.TreasureMonid, null);

            GameManager.Instance.M_MainPlayerCtrlBase.Data.RegisterAttribute(AOIAttrDefine.InteractID, OnInterChange);

            GameManager.Instance.M_MainPlayerCtrlBase.Data.RegisterAttribute(AOIAttrDefine.PlayerLevel, OnPlayerLevelChange);

            //冒险等级变化监听
            GameManager.Instance.M_MainPlayerCtrlBase.Data.RegisterAttribute(AOIAttrDefine.RiskInfo, OnRiskInfoChange);
            OnRiskInfoChange("", null);//手动调用一次

            //初始化道具
            GlobalEvent.OnItemFirstInit.Invoke(null);

            // 主角初始化的时候，刷新 系统开放数据的 信息
            SystemOpenManager.Instance.RefreshOnPlayerDataInit();


            GlobalEvent.OnRefeshShopBtn.Invoke(false);

            //刷新环任务红点
            GameManager.Instance.CheckRingTaskGetInfo();

            RefreshPreviewShowAndRedPoint();

            SendWorldLevelReq();


            // 本地服测试界面
#if UNITY_EDITOR
            if (EditorModeTest.EditorMode.IsEditorMode)
            {
                //UIManager.Instance.OpenWidget(UIDef.LocalServerTestWidget, true, null, UIRoot.UIROOT.transform, MainPageCommond.HideNone, true, true);
                UIManager.Instance.OpenWidgetAsync(UIDef.LocalServerTestWidget, null, true, null, UIRoot.UIROOT.transform, MainPageCommond.HideNone, true, true);
            }
#endif
        }

        #region 世界等级开放

        private void SendWorldLevelReq()
        {
            ProtoMsg.WorldLevelReq worldLevelReq = new();
            SocketBase battleSocket = NetworkManager.Instance.gameSocket;
            battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, worldLevelReq, false);
        }

        private void OnWorldLevelRet(MessageHandleData data)
        {
            WorldLevelRet ret = (WorldLevelRet)data.data;

            if (ret == null)
            {
                return;
            }

            if (ret.IsConfirm)
            {
                SGF.Debuger.LogWarning($"OnWorldLevelRet() 世界等级 确认过了 SrvOpenDay={ret.SrvOpenDay},WorldMaxLevel={ret.WorldMaxLevel}");
                return;
            }

            UIManager.Instance.OpenWidgetAsync(UIDef.WorldLvUpWidget, null, false, ret);
        }

        #endregion

        private LuaUIWidget M_EnemyInfoWidget;
        private XLua.LuaFunction m_LuaFunSetEnemyInfo;
        private Action<UIWidget> m_LocalEnemyInfoWidgetAction;
        private NPCEntityBase m_EnemyNpcEntityBase;

        public void OpenEnemyInfoWidget(NPCEntityBase entityBase)
        {
            bool isShow = false;
            if (entityBase != null)
            {
                E_EntityType entityType = entityBase.Data.EntityType;
                switch (entityType)
                {
                    case E_EntityType.None:
                    case E_EntityType.RoomSpace:
                    case E_EntityType.Npc:
                    case E_EntityType.BulletEntity:
                    case E_EntityType.Interact:
                    case E_EntityType.Summon:
                    case E_EntityType.Partner:
                    case E_EntityType.ClientSummon:
                    case E_EntityType.Player:
                        {
                            bool isTargetNtt = SkillUtils.GetEntityIsMainPlayerEnemy(entityBase);
                            if (isTargetNtt)
                            {
                                isShow = true;
                            }
                        }
                        break;
                    case E_EntityType.Monster:
                    case E_EntityType.GVEBoss:
                    case E_EntityType.Robot:
                        {
                            // 如果是怪物，那也有可能会是主角的友方
                            bool isTargetNtt = SkillUtils.GetEntityIsMainPlayerEnemy(entityBase);
                            if (isTargetNtt && !entityBase.Data.IsRobot)
                            {
                                MonsterDataCell monsterAttrDataCell = LocalDataManager.Instance.GetMonsterDataCell((int)entityBase.ConfigIndex);
                                if (monsterAttrDataCell != null)
                                {
                                    int monsterType = monsterAttrDataCell.GetMonType();
                                    var isWildBoss = (entityBase.Data.ActMark & (1 << (int)GamePlay.GpWildBoss)) > 0;
                                    isShow = monsterType > 1 || entityBase.Data.IsGVEBoss || isWildBoss;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                if (!isShow)
                {
                    entityBase = null;
                }
            }
            m_EnemyNpcEntityBase = entityBase;
            // 通知HUD敌方面板
            {
                if (m_LuaFunSetEnemyInfo == null)
                {
                    if (entityBase != null)
                    {
                        if (m_LocalEnemyInfoWidgetAction == null)
                        {
                            m_LocalEnemyInfoWidgetAction = (UIWidget ui) =>
                            {
                                if (ui != null)
                                {
                                    M_EnemyInfoWidget = ui as LuaUIWidget;
                                    m_LuaFunSetEnemyInfo = M_EnemyInfoWidget.GetLuaPanel().ScriptTable.Get<XLua.LuaFunction>("SetEnemyInfo");
                                    m_LuaFunSetEnemyInfo?.Call(M_EnemyInfoWidget.GetLuaPanel().ScriptTable, m_EnemyNpcEntityBase);
                                }
                                m_LocalEnemyInfoWidgetAction = null;
                            };
                            UIManager.Instance.OpenWidgetAsync(UIDef.EnemyInfoWidget, m_LocalEnemyInfoWidgetAction, false, entityBase, M_TopCentreRoot, MainPageCommond.HideNone, true, false);
                        }
                    }
                }
                else
                {
                    m_LuaFunSetEnemyInfo?.Call(M_EnemyInfoWidget.GetLuaPanel().ScriptTable, entityBase);
                }
            }
            // 设置GVEBoss信息
            {
                if (entityBase != null)
                {
                    if (entityBase.Data.IsGVEBoss)
                    {
                        ModuleManager.Instance.SendMessage(ModuleDef.Name.EventBossRankModule, "OnOpenBossRank", new object[] { entityBase.EntityId });
                    }
                }
                else
                {
                    ModuleManager.Instance.SendMessage(ModuleDef.Name.EventBossRankModule, "OnCloseBossRank", new object[] { });
                }
            }

            //野外boss
            {
                var mapID = GameManager.Instance.GetCurMapId();
                var ServerID = GameManager.Instance.GetCurServerID();
                if (entityBase != null && entityBase.Data != null)
                {
                    var isWildBoss = (entityBase.Data.ActMark & (1 << (int)GamePlay.GpWildBoss)) > 0;
                    if (isWildBoss)
                    {
                        ModuleManager.Instance.SendMessage(ModuleDef.Name.WildBossModule, "OnOpenWildBossDamageWidget", new object[] { entityBase.EntityId, M_TopCentreRoot, entityBase.ConfigIndex, mapID, ServerID });
                    }
                    else
                    {
                        ModuleManager.Instance.SendMessage(ModuleDef.Name.WildBossModule, "OnCloseWildBossDamageWidget", new object[] { });
                    }
                }
                else
                {
                    ModuleManager.Instance.SendMessage(ModuleDef.Name.WildBossModule, "OnCloseWildBossDamageWidget", new object[] { });
                }
            }

            // 设置目标箭头指示器面板
            OpenEnemyArrowLoockAtWidget(entityBase);
        }

        private void OpenEnemyArrowLoockAtWidget(NPCEntityBase entityBase)
        {
            //UIManager.Instance.OpenWidget(UIDef.ArrowLockAtWidget, false, entityBase, null, MainPageCommond.HideNone, false, false);
            //UIManager.Instance.OpenWidget(UIDef.ArrowLockAtWidget, false, entityBase, M_MiddleCentreRoot, MainPageCommond.HideNone, false, false);
            UIManager.Instance.OpenWidgetAsync(UIDef.ArrowLockAtWidget, null, false, entityBase, M_MiddleCentreRoot, MainPageCommond.HideNone, false, false);
        }

        //挂了传递true，------>挂了，------->显示界面
        //活了传递false,------->活了，------->隐藏界面
        public void OpenWaitToBornWindow(bool dieeee)
        {
            //SGF.Debuger.LogError("OpenWaitToBornWindow" + dieeee);
            if (dieeee)
            {
                if (GameManager.Instance.M_Map != null)
                {
                    var MapSpaceType = GameManager.Instance.M_Map.GetMapType();
                    switch (MapSpaceType)
                    {
                        case SpaceType.SpaceScene:
                        case SpaceType.SpaceMirror:
                        case SpaceType.SpaceDefault:
                        case SpaceType.SpacePlot:
                        case SpaceType.SpaceGng:
                            {
                                UIManager.Instance.CloseAllLoadedWindowNWidget();
                                //UIManager.Instance.OpenWindow(UIDef.UIWaitToRebornWindow, dieeee, MainPageCommond.HideBoth);
                                UIManager.Instance.OpenWindowAsync(UIDef.UIWaitToRebornWindow, null, dieeee, MainPageCommond.HideBoth);
                            }
                            break;
                        case SpaceType.SpaceSercet:
                            {
                                // 如果是秘境副本，发送副本中死亡事件
                                var M_EntityBase = GameManager.Instance.M_MainPlayerCtrlBase;
                                int num = 0;
                                if (M_EntityBase != null)
                                {
                                    num = M_EntityBase.Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.ReviveCount);
                                }

                                ModuleManager.Instance.SendMessage(ModuleDef.Name.SecretAreaModule, "OpenSecretAreaReviveWidget", new object[] { num });
                            }
                            break;
                        case SpaceType.SpaceWtask:
                            {
                                ModuleManager.Instance.SendMessage(ModuleDef.Name.WantedModule, "OpenReviveWidget");
                            }
                            break;
                        case SpaceType.SpaceDaily:
                        case SpaceType.SpaceTeamDaily:
                        case SpaceType.SpaceArena:
                            {
                                // 不处理
                            }
                            break;
                        case SpaceType.SpacePersonTower:
                            break;
                        default:
                            {
                                UIManager.Instance.CloseAllLoadedWindowNWidget();
                                //UIManager.Instance.OpenWindow(UIDef.UIWaitToRebornWindow, dieeee, MainPageCommond.HideBoth);
                                UIManager.Instance.OpenWindowAsync(UIDef.UIWaitToRebornWindow, null, dieeee, MainPageCommond.HideBoth);
                            }
                            break;
                    }
                }
                else
                {
                    UIManager.Instance.CloseAllLoadedWindowNWidget();
                    //UIManager.Instance.OpenWindow(UIDef.UIWaitToRebornWindow, dieeee, MainPageCommond.HideBoth);
                    UIManager.Instance.OpenWindowAsync(UIDef.UIWaitToRebornWindow, null, dieeee, MainPageCommond.HideBoth);
                }
            }
        }

        private void OnRoleReviveRetNtf(MessageHandleData data)
        {
            RoleReviveRet ret = (RoleReviveRet)data.data;
            if (GameManager.Instance.M_Map != null)
            {
                var MapSpaceType = GameManager.Instance.M_Map.GetMapType();
                switch (MapSpaceType)
                {
                    case SpaceType.SpaceDefault:
                    case SpaceType.SpaceScene:
                    case SpaceType.SpaceMirror:
                    case SpaceType.SpacePlot:
                    case SpaceType.SpaceDaily:
                    case SpaceType.SpaceSercet:
                    case SpaceType.SpaceTeamDaily:
                    case SpaceType.SpaceMirrorTreasure:
                    case SpaceType.SpacePlotTreasure:
                    case SpaceType.SpaceGng:
                    case SpaceType.SpacePersonTower:
                        break;
                    case SpaceType.SpaceWtask:
                        {
                            ModuleManager.Instance.SendMessage(ModuleDef.Name.WantedModule, "OnRoleReviveRet", new object[] { ret.RetValue == 0 });
                        }
                        break;
                    case SpaceType.Space10V10:
                        {
                            ModuleManager.Instance.SendMessage(ModuleDef.Name.PvpModule, "OnRoleReviveRet", new object[] { });
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        // 主角遥感控制HUD聊天显示时事件
        private void OnChatHudShowEvent(bool isShow)
        {
            //ModuleManager.Instance.SendMessage(ModuleDef.Name.ChatModule, "OnChatHudShow", new object[] { isShow });
        }

        #region GM

        #region 修改客户端战斗状态时间

        private AOIEntityObject mainPlayerEntity;

        public void onEndEdit(string text)
        {
            float _num = GameConfig.System_BattleStateCountDown;
            if (float.TryParse(text, out _num))
            {
                SGF.Debuger.Log($"输入框==={_num}");
            }
            else
            {
                _num = GameConfig.System_BattleStateCountDown;
            }
            //GameManager.Instance.M_GM_System_BattleStateCountDown = _num;


            if (mainPlayerEntity == null)
            {
                mainPlayerEntity = GameManager.Instance.GetEntityCtr(GameManager.Instance.mainPlayerId).M_Curr;
            }

            mainPlayerEntity.Speed = _num;
        }

        #endregion

        #region 原子锁显示

        private void OnStateChange(E_BattleStateType e_BattleStateType, bool isLock, int tag)
        {

            if (!AppConfig.IsDev())
            {
                return;
            }
            M_ClientForbidMoveText.transform.parent.gameObject.SetActive(true);

            if (e_BattleStateType == E_BattleStateType.BattleState_ForbidMove)
            {
                string text = isLock ? "禁移动" : "可移动";
                if (tag == 0)
                {
                    M_ClientForbidMoveText.text = $"C: {text}";
                    //SGF.Debuger.LogError($"主角属性同步 原子锁 客户端线 移动 {text}");
                }
                else if (tag == 1)
                {
                    M_ServerForbidMoveText.text = $"S: {text}";
                    //SGF.Debuger.LogError($"主角属性同步 原子锁 服务器线 移动 {text}");
                }
                else
                {
                    M_SkillForbidMoveText.text = $"A: {text}";
                    //SGF.Debuger.LogError($"主角属性同步 原子锁 Aoi线 移动 {text}");
                }
            }
            else
            {
                string text = isLock ? "禁转向" : "可转向";
                if (tag == 0)
                {
                    M_ClientForbidDirText.text = $"C: {text}";
                    //SGF.Debuger.LogError($"主角属性同步 原子锁 客户端线 转向 {text}");
                }
                else if (tag == 1)
                {
                    M_ServerForbidDirText.text = $"S: {text}";
                    //SGF.Debuger.LogError($"主角属性同步 原子锁 服务器线 转向 {text}");
                }
                else
                {
                    M_SkillForbidDirText.text = $"A: {text}";
                    //SGF.Debuger.LogError($"主角属性同步 原子锁 Aoi线 转向 {text}");
                }
            }
        }

        #endregion

        #region 点击输入指定指令

        public void OnClickSendGM()
        {
            NPCEntityBase main = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr as NPCEntityBase;
            int level = main.Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.PlayerLevel);
            SocketBase battleSocket = NetworkManager.Instance.gameSocket;
            GmCmdReq gmCmdReq = new();
            string msg = $"setlevel {level + 1}";
            gmCmdReq.Cmd = msg;
            battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, gmCmdReq, isAutoChangeMsgTarget: false);

            //UnityEngine.Vector3 pos = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();
            //for (int i = 0; i < 4; i++)
            //{
            //    GlobalEvent.onCreateLocalEntity.Invoke((int)E_LocalEntityType.TreasureBox, 9999991, E_LocalEntitySource.SecretArea.ToString(), pos, null, true);
            //}
        }

        #endregion

        #endregion

        #region 按钮点击事件

        private void OnBtnClickMenu()
        {
            //UIManager.Instance.OpenWidget(UIDef.HudMenuWidget, true, null, null, MainPageCommond.HudHideRight, false, false);
            UIManager.Instance.OpenWidgetAsync(UIDef.HudMenuWidget, null, true, null, null, MainPageCommond.HudHideRight, false, false);
        }

        // 点击背包界面
        private void OnBtnClickBackPack()
        {
            //
            ModuleManager.Instance.SendMessage(ModuleDef.Name.BagModule, "OnOpenBag", new object[] { 1 });
        }

        // 点击符文界面
        private void OnBtnClickRune()
        {
            //
            ModuleManager.Instance.SendMessage(ModuleDef.Name.AmuletModule, "OnOpenAmulet", new object[] { 0, 0 });
        }


        //点击首充
        private void OnBtnClickFirstCharge()
        {
            //UIManager.Instance.OpenWidget(UIDef.FirstPayWidget, false, 0, null, MainPageCommond.HideNone, false, false);
            UIManager.Instance.OpenWidgetAsync(UIDef.FirstPayWidget, null, false, 0, null, MainPageCommond.HideNone, false, false);
        }

        //点击玩法预告
        private void OnBtnClickPreview()
        {
            ModuleManager.Instance.SendMessage(ModuleDef.Name.ActivityModule, "OnOpenPreviewWindow", new object[] { 0 });
        }

        //点击活动
        private void OnBtnClickActivity()
        {

            if (!GameManager.Instance.IsUnLockPayFunction())
            {
                //没有开启支付直接打开通行证
                UIManager.Instance.OpenWidgetAsync(UIDef.BattlePassWidget, null, false, new object[] { 1, 1 }, null, MainPageCommond.HideNone, false, false);
                return;
            }

            if (GameManager.Instance.FirstPaySignInfo != null)
            {
                bool isGetAllFirstChargeItem = GameManager.Instance.FirstPaySignInfo.GetFirstPayAwardNum >= 3;


                if (isGetAllFirstChargeItem)
                {
                    //UIManager.Instance.OpenWidget("Shop/MoonCardWidget", false, 0, null, MainPageCommond.HideNone, false, false);
                    UIManager.Instance.OpenWidgetAsync(UIDef.MoonCardWidget, null, false, 0, null, MainPageCommond.HideNone, false, false);
                }
                else
                {
                    UIManager.Instance.OpenWidgetAsync(UIDef.FirstPayWidget, null, false, 0, null, MainPageCommond.HideNone, false, false);
                }
            }
            else
            {
                //UIManager.Instance.OpenWidget("Shop/MoonCardWidget", false, 0, null, MainPageCommond.HideNone, false, false);
                UIManager.Instance.OpenWidgetAsync(UIDef.MoonCardWidget, null, false, 0, null, MainPageCommond.HideNone, false, false);
            }
        }

        void OnBtnClickAct()
        {
            ModuleManager.Instance.SendMessage(ModuleDef.Name.ActivityModule, "OnShowSignInWindow", new object[] { });
        }

        void OnBtnClickBtnActivity(GameObject go)
        {
            //打开下载大窗口
            //if (!DlcDownload.Instance.priorityscenes.IsNullOrEmpty())
            //{
            //    ModuleManager.Instance.SendMessage(ModuleDef.Name.DownLoadModule, "OnOpenDownload", new object[] { 2, DlcDownload.Instance.priorityscenes });
            //}
            //else
            //{
            //    ModuleManager.Instance.SendMessage(ModuleDef.Name.DownLoadModule, "OnOpenDownload", new object[] { 1 });
            //}
            //测试打开下载小窗口
      //      ModuleManager.Instance.SendMessage(ModuleDef.Name.DownLoadModule, "OnOpenDownload", new object[] { 2, "map/map_chenxgd" });
        }

        // 点击商店界面
        private void OnBtnClickShop()
        {
            //
            ModuleManager.Instance.SendMessage(ModuleDef.Name.ShopModule, "OnOpenShop", new object[] { 0 });
        }

        // 点击装备界面
        private void OnBtnClickEquip()
        {
            ModuleManager.Instance.SendMessage(ModuleDef.Name.EquipUpgradeModule, "OnOpenEquipUpgrade",
                new object[] { 0, 0 });
        }

        //GNG奖励回复
        GNGPersonRewardNtf GNGPersonRewardNtf = null;
        GNGGuildRewardNtf GNGGuildRewardNtf = null;
        private void OnGNGPersonRewardNtf(MessageHandleData data)
        {
            GNGPersonRewardNtf = (GNGPersonRewardNtf)data.data;
            gngBossRewardBtn.gameObject.SetActive(true);
        }
        private void OnGNGGuildRewardNtf(MessageHandleData data)
        {
            GNGGuildRewardNtf = (GNGGuildRewardNtf)data.data;
            gngBossRewardBtn.gameObject.SetActive(true);
        }

        //GVEBoss奖励回复
        List<GVEBossRewardNtf> GVEBossRewards = new();
        private void OnGVEBossRewardNtf(MessageHandleData data)
        {
            GVEBossRewardNtf propPanelRet = (GVEBossRewardNtf)data.data;


            if (propPanelRet.State == BossRewardState.BrsInRank)
            {
                //有奖励
                if (propPanelRet.Reward.Data.Count > 0)
                {
                    GVEBossRewards.Add(propPanelRet);
                    gveBossRewardBtn.gameObject.SetActive(true);
                }
                int mapID = GameManager.Instance.M_Map.GetMapId();
                //产生提示
                for (int i = 0; i < propPanelRet.Maps.Count; i++)
                {
                    if (propPanelRet.Maps[i] == mapID)
                    {
                        //提示语句
                        long bossID = propPanelRet.Index;
                        string bossName = LocalDataManager.Instance.GetMonsterDataCell(bossID).Name;
                        int rank = propPanelRet.Rank;
                        //Frame.Util.ShowMessage(string.Format("您的公会在抵御{0}时排名第{1},获得了击杀奖励！", bossName, rank));
                        Frame.Util.ShowMessageByCode(CRetMsgEnum.Tips_GVE_01, bossName, rank);

                    }
                }
            }
            else if (propPanelRet.State == BossRewardState.BrsOutRank)
            {
                //提示语句
                long bossID = propPanelRet.Index;
                string bossName = LocalDataManager.Instance.GetMonsterDataCell(bossID).Name;
                int rank = propPanelRet.Rank;
                //Frame.Util.ShowMessage(string.Format("您的公会在抵御{0}时排名第{1}，请再接再厉！", bossName, rank));
                Frame.Util.ShowMessageByCode(CRetMsgEnum.Tips_GVE_02, bossName, rank);
            }
            else if (propPanelRet.State == BossRewardState.BrsOutHurt)
            {
                //提示语句
                long bossID = propPanelRet.Index;
                string bossName = LocalDataManager.Instance.GetMonsterDataCell(bossID).Name;
                //Frame.Util.ShowMessage(string.Format("您的公会在抵御{0}时的付出，再多召集些小伙伴吧！", bossName));
                Frame.Util.ShowMessageByCode(CRetMsgEnum.Tips_GVE_03, bossName);
            }
            else if (propPanelRet.State == BossRewardState.BrsNoGuild)
            {
                //提示语句
                long bossID = propPanelRet.Index;
                string bossName = LocalDataManager.Instance.GetMonsterDataCell(bossID).Name;
                //Frame.Util.ShowMessage(string.Format("您的公会在抵御{0}时的付出，请寻找公会大显身手吧！"));
                Frame.Util.ShowMessageByCode(CRetMsgEnum.Tips_GVE_04, bossName);
            }

        }

        void OnClearGVEBossReward(bool b)
        {
            GVEBossRewards.Clear();
            gveBossRewardBtn.gameObject.SetActive(false);
        }


        void OnClearGNGBossReward(bool b)
        {
            GNGPersonRewardNtf = null;
            GNGGuildRewardNtf = null;
            gngBossRewardBtn.gameObject.SetActive(false);
        }

        #region 点击箭头按钮动画

        private bool m_IsOpen = true;
        private Tweener m_MenuPanelTweener;
        //private float m_MenuClosePosX = 0; // 菜单按钮栏关闭收回去的X的坐标

        private void OnBtnClickArrow()
        {
            //if (m_MenuPanelTweener != null && m_MenuPanelTweener.IsPlaying())
            //{
            //    m_MenuPanelTweener.Kill();
            //}

            //// 当前0，那就关闭回去
            //// 否则，就展开
            //if (m_MenuClosePosX == 0)
            //{
            //    float widthCount = m_MenuInfoPanel.GetComponent<RectTransform>().rect.width;
            //    float menuIntervalWidth = widthCount / m_MenuInfoPanel.childCount;
            //    m_MenuClosePosX = widthCount - menuIntervalWidth;
            //}

            //float _PosX = !m_IsOpen ? 0 : m_MenuClosePosX;
            ////参数一:要移动到的位置
            ////参数二:动画完成要多少秒
            //m_MenuPanelTweener = m_MenuInfoPanel.DOLocalMoveX(_PosX, 0.5f);
            //m_MenuPanelTweener.onComplete = () =>
            //{
            //    m_MenuPanelTweener.onComplete = null;
            //    m_MenuPanelTweener.Kill();
            //    m_IsOpen = !m_IsOpen;
            //    m_ArrowBtn.transform.GetChild(0).gameObject.SetActive(!m_IsOpen);
            //    m_ArrowBtn.transform.GetChild(1).gameObject.SetActive(m_IsOpen);
            //};
            //string soundName = m_IsOpen ? "UI_Accordion_On_Sound" : "UI_Accordion_Off_Sound";
            //m_ArrowBtn.PlaySoundName = soundName;
        }

        #endregion

        #endregion



        #region 场景配置文件加载完毕

        private void OnSceneMapLoadComplete(int arg0)
        {
            ProtoMsg.SpaceType curMapType = GameManager.Instance.GetCurMapType();
            string mapSubType = GameManager.Instance.GetMapSubType();

            bool isHide = curMapType != SpaceType.SpaceDefault && curMapType != SpaceType.SpaceScene && curMapType != SpaceType.SpaceGuildTerritory;
            LeftBtnLyout.SetActive(!isHide);
            EventTimeWidget.SetActive(!isHide);
            RightBtnLayout.SetActive(!isHide);
            RightTimeLimitBtnLayout.SetActive(!isHide);
            M_TreasureRoot.gameObject.SetActive(!isHide);
            // 特殊需求
            bool isHide3 = false;
            if (curMapType == SpaceType.SpacePlot)
            {
                isHide3 = mapSubType == GameConfig.INSTANCE_PLOT_FIRST || mapSubType == GameConfig.INSTANCE_PLOT_SECOND || mapSubType == GameConfig.INSTANCE_PLOT_THIRD;
            }
            SkipBtn.gameObject.SetActive(isHide3);

            /* SkipBtn.gameObject.SetActive(false);
             if ( mapSubType == GameConfig.INSTANCE_PLOT_FIRST)
             {
                 DoSkip(() =>
                 {
                     SkipBtn.gameObject.SetActive(true);

                 });
             }*/

            DownCentreSonRoot.SetActive(!isHide3);

            bool isHide2 = mapSubType == GameConfig.INSTANCE_PLOT_FIRST || mapSubType == GameConfig.INSTANCE_PLOT_THIRD;
            DynamicUIRoot.EntityUIRoot.GetComponent<CanvasGroup>().alpha = isHide2 ? 0f : 1f;
            DynamicUIRoot.DamageUIRoot.GetComponent<CanvasGroup>().alpha = isHide2 ? 0f : 1f;

            bool isShow = true;
            if (mapSubType == GameConfig.INSTANCE_PLOT_SECOND)
            {
                isShow = BusinessManager.Instance.GetXinSGStateDic("-1");
            }

            GameManager.Instance.IsShowPlayerWidget = !isHide2 && isShow;
            bool isInEctype = curMapType != SpaceType.SpaceDefault && curMapType != SpaceType.SpaceScene && curMapType != SpaceType.SpaceGuildTerritory;
            BtnSetting.gameObject.SetActive(GameManager.Instance.IsShowPlayerWidget && isInEctype);
            Ectype_Btn.gameObject.SetActive(!isHide3 && isHide);
            GlobalEvent.onPlayerInfoWidgetShow?.Invoke(GameManager.Instance.IsShowPlayerWidget);
            OnCurTreasureChange(AOIAttrDefine.CurTreasure, null);
        }

        #endregion

        #region 新手关
        private void OnXinSGUIShowCB(object arg0)
        {
            string mapSubType = GameManager.Instance.GetMapSubType();
            bool isHide2 = mapSubType == GameConfig.INSTANCE_PLOT_FIRST || mapSubType == GameConfig.INSTANCE_PLOT_THIRD;
            bool isShow = BusinessManager.Instance.GetXinSGStateDic("-1");
            GameManager.Instance.IsShowPlayerWidget = !isHide2 && isShow;
            BtnSetting.gameObject.SetActive(GameManager.Instance.IsShowPlayerWidget);
            GlobalEvent.onPlayerInfoWidgetShow?.Invoke(GameManager.Instance.IsShowPlayerWidget);
        }

        #endregion

        #region HUD活动倒计时相关


        int jumpGVEIdx = 1;
        int jumpGNGIdx = 1;
        int jumpPVPIdx = 1;
        int jumpWildIdx = 1;
        int jumpPartyIdx = 1;

        /// <summary>
        /// 点击前往GVE
        /// </summary>
        void OnGoToGVE()
        {
            ModuleManager.Instance.SendMessage(ModuleDef.Name.EventModule, "OnOpenEventPopWindow", new object[] { jumpGVEIdx });
        }

        /// <summary>
        /// 点击前往GNG
        /// </summary>
        void OnGoToGNG()
        {
            ModuleManager.Instance.SendMessage(ModuleDef.Name.EventModule, "OnOpenEventPopWindow", new object[] { jumpGNGIdx });
        }

        /// <summary>
        /// 点击前往篝火
        /// </summary>
        void OnGoToParty()
        {
            ModuleManager.Instance.SendMessage(ModuleDef.Name.EventModule, "OnOpenEventPopWindow", new object[] { jumpPartyIdx });
        }

        /// <summary>
        /// 点击前往PVP
        /// </summary>
        void OnGoToPVP()
        {
            ModuleManager.Instance.SendMessage(ModuleDef.Name.EventModule, "OnOpenEventPopWindow", new object[] { jumpPVPIdx });
        }

        /// <summary>
        /// 点击前往PVP
        /// </summary>
        void OnGoToWild()
        {
            ModuleManager.Instance.SendMessage(ModuleDef.Name.EventModule, "OnOpenWildBoss", new object[] { });
        }

        bool isAddUpdate = false;



        private long serverOpenTimeGVE;
        private long serverCloseTimeGVE;

        private long serverOpenTimeGNG;
        private long serverCloseTimeGNG;

        private long serverOpenTimeWild;
        private long serverCloseTimeWild;

        private long serverOpenTimePVP;
        private long serverCloseTimePVP;

        private long serverOpenTimeParty;
        private long serverCloseTimeParty;




        private int PVPSignUpStatus;

        //[ContextMenu("刷新")]
        private void OnEventTimeUpdate()
        {
            var serverTimeNow = (int)(GameManager.Instance.GetServerTimeStamp() / 1000);

            long GVELimtTimePre = serverOpenTimeGVE - serverTimeNow;
            long GVELimtTime = serverCloseTimeGVE - serverTimeNow;

            if (GVELimtTime > 0)
            {
                if (m_GVE_Time != null)
                {
                    if (BusinessManager.Instance.SystemIsOpen(StarProjectDef.SystemOpenType.PlayModeGVE))
                    {
                        //GVE_Btn.gameObject.SetActive(true);
                        GVE_Btn.transform.parent.gameObject.SetActive(true);
                        GVE_Btn.transform.parent.GetComponent<LayoutElement>().preferredHeight += 0.0001f;
                        if (GVELimtTimePre > 0)
                        {
                            //m_GVE_Time.text = $"<color=#CFBCA6>{GameConfig.LocalStr["OpenSoon"]}</color>";
                            m_GVE_Time.text = $"<color=#CFBCA6>{LanguageManager.Instance.GetLanguageByKey("OpenSoon")}</color>";
                        }
                        else
                        {
                            //m_GVE_Time.text = TimeUtils.GetTimeStringV2("%mm:%ss ", GVELimtTime) + GameConfig.LocalStr["BeOpenTo"];
                            m_GVE_Time.text = TimeUtils.GetTimeStringV2("%mm:%ss ", GVELimtTime) + LanguageManager.Instance.GetLanguageByKey("BeOpenTo");
                        }
                    }
                    else
                    {
                        //GVE_Btn.gameObject.SetActive(false);
                        GVE_Btn.transform.parent.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                //GVE_Btn.gameObject.SetActive(false);
                GVE_Btn.transform.parent.gameObject.SetActive(false);
            }

            long GNGLimtTimePre = serverOpenTimeGNG - serverTimeNow;
            long GNGLimtTime = serverCloseTimeGNG - serverTimeNow;

            if (GNGLimtTime > 0)
            {

                GNGLimtTime--;
                GNGLimtTimePre--;
                if (m_GNG_Time != null)
                {
                    if (BusinessManager.Instance.SystemIsOpen(StarProjectDef.SystemOpenType.GuildNoonActiv))
                    {
                        //GNG_Btn.gameObject.SetActive(true);
                        GNG_Btn.transform.parent.gameObject.SetActive(true);
                        GNG_Btn.transform.parent.GetComponent<LayoutElement>().preferredHeight += 0.0001f;
                        if (GNGLimtTimePre > 0)
                        {
                            //m_GNG_Time.text = "<color=#CFBCA6>即将开放</color>";
                            m_GNG_Time.text = $"<color=#CFBCA6>{LanguageManager.Instance.GetLanguageByKey("OpenSoon")}</color>";
                        }
                        else
                        {
                            //m_GNG_Time.text = TimeUtils.GetTimeStringV2("%mm:%ss ", GNGLimtTime) + "开放中";
                            m_GNG_Time.text = TimeUtils.GetTimeStringV2("%mm:%ss ", GNGLimtTime) + LanguageManager.Instance.GetLanguageByKey("BeOpenTo");
                        }
                    }
                    else
                    {
                        //GNG_Btn.gameObject.SetActive(false);
                        GNG_Btn.transform.parent.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                //GNG_Btn.gameObject.SetActive(false);
                GNG_Btn.transform.parent.gameObject.SetActive(false);
            }


            long PartyLimtTimePre = serverOpenTimeParty - serverTimeNow;
            long PartyLimtTime = serverCloseTimeParty - serverTimeNow;

            if (PartyLimtTime > 0)
            {

                PartyLimtTime--;
                PartyLimtTimePre--;
                if (m_Party_Time != null)
                {
                    if (BusinessManager.Instance.SystemIsOpen(StarProjectDef.SystemOpenType.Guild))
                    {
                        //Party_Btn.gameObject.SetActive(true);
                        Party_Btn.transform.parent.gameObject.SetActive(true);
                        Party_Btn.transform.parent.GetComponent<LayoutElement>().preferredHeight += 0.0001f;
                        if (PartyLimtTimePre > 0)
                        {
                            //m_Party_Time.text = "<color=#CFBCA6>即将开放</color>";
                            m_Party_Time.text = $"<color=#CFBCA6>{LanguageManager.Instance.GetLanguageByKey("OpenSoon")}</color>";
                        }
                        else
                        {
                            //m_Party_Time.text = TimeUtils.GetTimeStringV2("%mm:%ss ", PartyLimtTime) + "开放中";
                            m_Party_Time.text = TimeUtils.GetTimeStringV2("%mm:%ss ", PartyLimtTime) + LanguageManager.Instance.GetLanguageByKey("BeOpenTo");
                        }
                    }
                    else
                    {
                        //Party_Btn.gameObject.SetActive(false);
                        Party_Btn.transform.parent.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                //Party_Btn.gameObject.SetActive(false);
                Party_Btn.transform.parent.gameObject.SetActive(false);
            }



            long WildLimtTimePre = serverOpenTimeWild - serverTimeNow;
            long WildLimtTime = serverCloseTimeWild - serverTimeNow;

            if (WildLimtTime > 0)
            {

                WildLimtTime--;
                WildLimtTimePre--;
                if (m_Wild_Time != null)
                {
                    if (BusinessManager.Instance.SystemIsOpen(StarProjectDef.SystemOpenType.FieldBoss))
                    {
                        //Wild_Btn.gameObject.SetActive(true);
                        Wild_Btn.transform.parent.gameObject.SetActive(true);
                        Wild_Btn.transform.parent.GetComponent<LayoutElement>().preferredHeight += 0.0001f;
                        if (WildLimtTimePre > 0)
                        {
                            //m_Wild_Time.text = "<color=#CFBCA6>即将开放</color>";
                            m_Wild_Time.text = $"<color=#CFBCA6>{LanguageManager.Instance.GetLanguageByKey("OpenSoon")}</color>";
                        }
                        else
                        {
                            //m_Wild_Time.text = TimeUtils.GetTimeStringV2("%mm:%ss ", WildLimtTime) + "开放中";
                            m_Wild_Time.text = TimeUtils.GetTimeStringV2("%mm:%ss ", WildLimtTime) + LanguageManager.Instance.GetLanguageByKey("BeOpenTo");
                        }
                    }
                    else
                    {
                        //Wild_Btn.gameObject.SetActive(false);
                        Wild_Btn.transform.parent.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                //Wild_Btn.gameObject.SetActive(false);
                Wild_Btn.transform.parent.gameObject.SetActive(false);
            }

            long PVPLimtTimePre = serverOpenTimePVP - serverTimeNow;
            long PVPLimtTime = serverCloseTimePVP - serverTimeNow;

            if (PVPLimtTime > 0)
            {
                PVPLimtTime--;
                PVPLimtTimePre--;
                if (m_PVP_Time != null)
                {
                    if (BusinessManager.Instance.SystemIsOpen(StarProjectDef.SystemOpenType.BattleField))
                    {
                        //PVP_Btn.gameObject.SetActive(true);
                        PVP_Btn.transform.parent.gameObject.SetActive(true);
                        PVP_Btn.transform.parent.GetComponent<LayoutElement>().preferredHeight += 0.0001f;
                        if (PVPLimtTimePre > 0)
                        {
                            //m_PVP_Time.text = "<color=#FF6960>" + TimeUtils.GetTimeStringV2("%mm:%ss", PVPLimtTimePre) + "</color> <color=#CFBCA6>未报名</color>";
                            m_PVP_Time.text = "<color=#CFBCA6>" + TimeUtils.GetTimeStringV2("%mm:%ss", PVPLimtTimePre) + $" {LanguageManager.Instance.GetLanguageByKey("NotSignedUp")}</color>";
                        }
                        else
                        {
                            if (PVPSignUpStatus >= 1)
                            {
                                m_PVP_Time.text = TimeUtils.GetTimeStringV2("%mm:%ss", PVPLimtTime) + $" {LanguageManager.Instance.GetLanguageByKey("BeOpenTo")}";
                            }
                            else
                            {
                                //m_PVP_Time.text = "<color=#FF6960>" + TimeUtils.GetTimeStringV2("%mm:%ss", PVPLimtTime) + "</color> <color=#CFBCA6>未报名</color>";
                                m_PVP_Time.text = "<color=#CFBCA6>" + TimeUtils.GetTimeStringV2("%mm:%ss", PVPLimtTime) + $" {LanguageManager.Instance.GetLanguageByKey("NotSignedUp")}</color>";
                            }
                        }
                    }
                    else
                    {
                        //PVP_Btn.gameObject.SetActive(false);
                        PVP_Btn.transform.parent.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                //PVP_Btn.gameObject.SetActive(false);
                PVP_Btn.transform.parent.gameObject.SetActive(false);
            }
        }


        void OnSetEventTime(params object[] args)
        {
            int gameID = (int)args[0];
            int idx = (int)args[1];
            long serverOpenTime = (long)args[2];
            long serverCloseTime = (long)args[3];
            long serverPrepareTime = (long)args[4];

            if (gameID == 1)
            {
                jumpGVEIdx = idx;
                serverCloseTimeGVE = serverCloseTime;
                serverOpenTimeGVE = serverOpenTime;
                m_GVE_Title.text = LocalDataManager.Instance.GetRecyclePlayDataCell((long)gameID).PlayName;

            }

            else if (gameID == 2)
            {
                jumpGNGIdx = idx;
                serverCloseTimeGNG = serverCloseTime;
                serverOpenTimeGNG = serverOpenTime;
                m_GNG_Title.text = LocalDataManager.Instance.GetRecyclePlayDataCell((long)gameID).PlayName;

            }

            else if (gameID == 3)
            {
                jumpPVPIdx = idx;
                serverCloseTimePVP = serverCloseTime;
                serverOpenTimePVP = serverOpenTime;
                m_PVP_Title.text = LocalDataManager.Instance.GetRecyclePlayDataCell((long)gameID).PlayName;
            }

            else if (gameID == 4)
            {
                jumpWildIdx = idx;
                serverCloseTimeWild = serverCloseTime;
                serverOpenTimeWild = serverOpenTime;
                m_Wild_Title.text = LocalDataManager.Instance.GetRecyclePlayDataCell((long)gameID).PlayName;
            }

            else if (gameID == 5)
            {
                jumpPartyIdx = idx;
                serverCloseTimeParty = serverCloseTime;
                serverOpenTimeParty = serverOpenTime;
                m_Party_Title.text = LocalDataManager.Instance.GetRecyclePlayDataCell((long)gameID).PlayName;
            }

            if (!isAddUpdate)
            {
                isAddUpdate = true;
                MonoHelper.AddSecTimeUpdateListener(OnEventTimeUpdate, MonoHelper.E_ModuleType.CommonBusiness);
            }

        }


        #endregion

        #region 首充按钮刷新

        private void OnRefeshShopBtn(bool arg)
        {

            if (!GameManager.Instance.IsUnLockPayFunction())
            {
                //没有开启支付系统不显示首充
                go_BtnShop.SetActive(false);
                go_BtnFirstCharge3.SetActive(false);
                go_BtnFirstCharge2.SetActive(false);
                return;
            }

            if (GameManager.Instance.FirstPaySignInfo == null)
                return;

            bool isFinishFirstCharge = GameManager.Instance.FirstPaySignInfo.HaveFirstPayAct;
            bool isGetAllFirstChargeItem = GameManager.Instance.FirstPaySignInfo.GetFirstPayAwardNum >= 3;

            int LastGetFirstPayAwardTime = GameManager.Instance.FirstPaySignInfo.LastGetFirstPayAwardTime;

            int serverNowTime = (int)Mathf.Floor(GameManager.Instance.GetServerTimeStamp() / 1000) - (5 * 3600); //凌晨五点刷新（扣除5小时）
            int day1 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(serverNowTime).ToLocalTime().Day;
            int day2 = 0;
            if (LastGetFirstPayAwardTime != 0)
            {
                DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(LastGetFirstPayAwardTime - (5 * 3600)).ToLocalTime();
                day2 = dt.Day; //凌晨五点刷新（扣除5小时）
            }

            if (isGetAllFirstChargeItem)
            {
                go_BtnFirstCharge2.SetActive(false);

                //临时关闭钻石商店
                //go_BtnShop_shop.SetActive(true);
                //go_BtnShop_charge.SetActive(false);
                go_BtnShop.SetActive(false);

                go_BtnFirstCharge3.SetActive(false);
            }
            else
            {
                if (isFinishFirstCharge)
                {
                    //临时关闭钻石商店
                    //go_BtnShop_shop.SetActive(true);
                    //go_BtnShop_charge.SetActive(false);
                    go_BtnShop.SetActive(false);


                    go_BtnFirstCharge3.SetActive(true);
                    //如果没有领取就显示
                    go_BtnFirstCharge2.SetActive(day1 != day2);
                }
                else
                {
                    //临时关闭钻石商店
                    go_BtnShop_shop.SetActive(false);
                    go_BtnShop_charge.SetActive(true);
                    go_BtnShop.SetActive(true);

                    go_BtnFirstCharge3.SetActive(false);
                    go_BtnFirstCharge2.SetActive(false);
                }
            }
        }

        #endregion

        #region 修改任务节点的显隐

        public void SetTaskInfoPanelShow(bool show)
        {
            TaskRoot.gameObject.SetActive(show);
        }

        #endregion

        #region PVP
        public void UpdatePvpSignStatus(int status)
        {
            PVPSignUpStatus = status;
        }
        #endregion

        #region ACT
        public void SetActRedpoint(bool show)
        {
            go_Act_Redpoint.SetActive(show);
            //Debug.LogError($"SetActRedpoint:{show}");
        }
        #endregion

        #region online reward
        public void StartTick()
        {
            go_Online_Redpoint.SetActive(false);
            LuaModule luaModule2 = ModuleManager.Instance.GetModule(ModuleDef.Name.OnlineRewardModule) as LuaModule;
            if (luaModule2 != null)
            {
                var tb = luaModule2.GetLuaTable();

                if (IsAllFinishEvent(tb))
                {
                    if (m_onlineBtn.gameObject.activeSelf)
                    {
                        m_onlineBtn.gameObject.SetActive(false);
                    }
                }
                else if (CanGetOnlineRewardEvent(tb))
                {
                    if(!m_onlineBtn.gameObject.activeSelf)
                        m_onlineBtn.gameObject.SetActive(true);
                    go_Online_Redpoint.SetActive(true);
                    //txt_Online_Status.text = GameConfig.LocalStr["CanGet"];
                    txt_Online_Status.text = LanguageManager.Instance.GetLanguageByKey("CanGet");
                }
                else
                {
                    if (!m_onlineBtn.gameObject.activeSelf)
                        m_onlineBtn.gameObject.SetActive(true);
                    txt_Online_Status.text = GetOnlineRewardTimeEvent(tb);
                }
            }
            var isactivityopen = SystemOpenManager.Instance.SystemIsOpen(SystemOpenType.Activity);
            if (isactivityopen != JBtnAct.activeSelf)
            {
                JBtnAct.SetActive(isactivityopen);
            } 
        }
        #endregion
    }

    [XLua.CSharpCallLua]
    public delegate bool GetRedPoint(object self);

    [XLua.CSharpCallLua]
    public delegate bool IsAllFinish(object self);

    [XLua.CSharpCallLua]
    public delegate bool CanGetOnlineReward(object self);

    [XLua.CSharpCallLua]
    public delegate string GetOnlineRewardTime(object self);

    [XLua.CSharpCallLua]
    public delegate bool FuncBool(object self);

    [XLua.CSharpCallLua]
    public delegate bool GetMsgData(object self = null);
}