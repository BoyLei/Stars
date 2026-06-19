using SGF.Module.Framework;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject.Game;
using StarProject.Module;
using StarProject.Service.AtlasManager;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProject.Service.SDK;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.Login
{
    //要不就改成window，要不然就所有window都是挂件，切换其状态
    public class UILoginPage : UIPage
    {
        //private string TagFlag = "[UILoginPage]";

        private LoginModule m_Mymodule;
        private CanvasGroup M_MainCanvasGroup;
        // 登录账号面板
        public GameObject M_LoginPanel;
        public InputField M_InputId;
        public InputField M_InputName;
        // 登录信息面板
        public GameObject M_LoginInfoPanel;
        public Text M_CurrentID;
        // -- 默认登录的区服 
        public Image M_LastGroupLoadState;          // 服务器负载状态标识（火爆：红/拥挤：黄/正常：绿/维护中：灰） 
        public Text M_LastGroupName;                // 服务器名

        private JButton m_LoginBtn;
        private string recordLoginID = "";
        private GroupInfo groupInfo;

        public Text m_VersionTex;

        private Action<Sprite> loadGroupIcon;


        #region 登录IP

        public GameObject LoginIPPanel;
        public Text LoginIP;
        public RectTransform LoginIPList;
        public GameObject ItemIP;
        public GameObject LoginArrowDown;
        public GameObject LoginArrowUp;
        public InputField CustomInputField;

        private Toggle privacyCheck;

        private TextMeshProLink privacyTextLink;


        private bool m_IsLoginIPPanelShow = false;

        #endregion

        private bool m_IsSDKLogin = true;

        //private CustomRenderTexture customRenderTexture = null;

        protected override void Awake()
        {
            base.Awake();
            onCloseDestroy = true;
            M_MainCanvasGroup = gameObject.GetComponent<CanvasGroup>();

            InitLoginIPList();
            // 绑定点击事件
            {
                {
                    // （有账号直接进）进入游戏
                    m_LoginBtn = transform.Find("Cut4Cam90/AssemblyRatio2/BtnLogin").GetComponent<JButton>();
                    m_LoginBtn.OnClick = (go) =>
                    {
                        SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.CLICK_ENTER_GAME);

                        if (!CheckPrivacy())
                        {
                            TipsPrivacyCheck(() =>
                            {
                                OnBtnClickLoginIPArrowJBtn(false);
                                OnBtnClickLogin();
                            });
                            return;
                        }
                        OnBtnClickLoginIPArrowJBtn(false);
                        OnBtnClickLogin();
                    };
                }
                {
                    // （没账号[输入账号]直接进）进入游戏
                    JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/LoginPanel/BtnLoginID").GetComponent<JButton>();
                    Btn.OnClick = (go) =>
                    {
                        SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.CLICK_ENTER_GAME);
                        if (!CheckPrivacy())
                        {
                            TipsPrivacyCheck(() =>
                            {
                                OnBtnClickLoginIPArrowJBtn(false);
                                OnBtnClickLoginID();
                            });
                            return;
                        }
                        OnBtnClickLoginIPArrowJBtn(false);
                        OnBtnClickLoginID();
                    };
                }
                {
                    // （有账号，登录区服）进入游戏
                    JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/LoginInfoPanel/BtnLoginArea").GetComponent<JButton>();
                    Btn.OnClick = (GameObject go) =>
                    {
                        SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.CLICK_ENTER_GAME);
                        if (!CheckPrivacy())
                        {
                            TipsPrivacyCheck(() =>
                            {
                                OnBtnClickLoginIPArrowJBtn(false);
                                OnBtnClickLoginArea();
                            });
                            return;
                        }
                        OnBtnClickLoginIPArrowJBtn(false);
                        OnBtnClickLoginArea();
                    };
                }
                {
                    // 选择区服
                    JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/LoginInfoPanel/DefaultArea").GetComponent<JButton>();
                    Btn.OnClick = (go) =>
                    {
                        if (!CheckPrivacy())
                        {
                            TipsPrivacyCheck(() =>
                            {
                                OnBtnClickCheckArea();
                                SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.CLICK_SERVER_AREA);
                            });
                            return;
                        }
                        OnBtnClickCheckArea();
                        SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.CLICK_SERVER_AREA);
                    };
                }
                {
                    // 注销
                    JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/LoginInfoPanel/BtnLogout").GetComponent<JButton>();
                    Btn.OnClick = (go) =>
                    {
                        OnBtnClickLoginIPArrowJBtn(false);
                        OnBtnClickLoginOut();
                    };
                }
                {
                    // 切换账号
                    JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/LoginInfoPanel/BtnSwitch").GetComponent<JButton>();
                    Btn.OnClick = (go) =>
                    {
                        SDKManager.Instance.SwitchAccount();
                    };
                }
                {
                    // 公告
                    JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/BtnNotice").GetComponent<JButton>();
                    Btn.OnClick = (go) =>
                    {
                        OnBtnClickLoginIPArrowJBtn(false);
                        OnBtnClickNotice();
                    };
                }
                {
                    // 客服
                    JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/BtnService").GetComponent<JButton>();
                    Btn.OnClick = (go) =>
                    {
                        OnBtnClickLoginIPArrowJBtn(false);
                        OnBtnClickService();
                    };
                }
                {
                    // 登录IP箭头
                    JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/LoginIPPanel/JBtnArrow").GetComponent<JButton>();
                    Btn.OnClick = (go) =>
                    {
                        OnBtnClickLoginIPArrowJBtn(!m_IsLoginIPPanelShow);
                        //var cfg = LocalDataManager.Instance.GetPvListDataCell(2);
                        //PlayVideoWidgetArgs data = new();
                        //data.EffectID = 1000704;
                        //data.Path = cfg.PvPath;
                        //data.IsShowSkipBtn = true;
                        //data.IsInBalack = true;
                        //data.IsOutBlack = true;
                        //data.CallBack = null;
                        //SGF.UI.Framework.UIManager.Instance.OpenWidgetAsync(UIDef.PlayVideoWidget, null, false, data, null, MainPageCommond.HideNone, true, true);
                    };
                }
                {
                    // 自定义登录IP按钮
                    JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/LoginIPPanel/JBtnCustomIP").GetComponent<JButton>();
                    Btn.OnClick = (go) =>
                    {
                        OnBtnClickCustomIPJBtn();
                    };
                }
                {
                    // 登出SDK
                    JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/LoginIPPanel/JBtnLoginOutSDK").GetComponent<JButton>();
                    Btn.OnClick = (go) =>
                    {
                        OnBtnClickLoginOutSDKJBtn();
                    };
                }
                {
                    // 适龄提示按钮
                    JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/JBtnAgeTips").GetComponent<JButton>();
                    Btn.OnClick = (go) =>
                    {
                        OnBtnClickAgeTipsJBtn();
                    };
                }
                {

                    // 使用SDK登录
                    JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/BtnLoginSDK").GetComponent<JButton>();
                    Text text = transform.Find("Cut4Cam90/AssemblyRatio2/BtnLoginSDK/Text").GetComponent<Text>();
                    Btn.OnClick = (go) =>
                    {
                        OnBtnClickLoginIPArrowJBtn(false);
                        m_IsSDKLogin = !m_IsSDKLogin;
                        text.text = m_IsSDKLogin ? "开发登录" : "SDK登录";
                        //var cfg = LocalDataManager.Instance.GetPvListDataCell(1);
                        //PlayVideoWidgetArgs data = new();
                        //data.EffectID = 1000704;
                        //data.Path = cfg.PvPath;
                        //data.IsShowSkipBtn = true;
                        //data.CallBack = null;
                        //SGF.UI.Framework.UIManager.Instance.OpenWidgetAsync(UIDef.PlayVideoWidget, null, false, data, null, MainPageCommond.HideNone, true, true);
                    };
                    bool isShowBtn = false;
#if UNITY_EDITOR
                    m_IsSDKLogin = false;
#elif STAR_DEV
                    isShowBtn = true;
                    m_IsSDKLogin = false;
#endif
                    //isShowBtn = false;
                    Btn.gameObject.SetActive(isShowBtn);
                }

                {
                    privacyCheck = transform.Find("Cut4Cam90/AssemblyRatio2/Content/PrivacyCheck").GetComponent<Toggle>();
                    privacyCheck.onValueChanged.AddListener(OnPrivacyCheckClick);

                    privacyTextLink = privacyCheck.transform.parent.Find("PrivacyText").GetComponent<TextMeshProLink>();
                    privacyTextLink.SetUrl(new List<string>() { GameConfig.URL_THE_STAR_PRIVACY, GameConfig.URL_THE_STAR_LICENSE });

                    RefreshPrivacyCheck();
                }
            }

            // sdk 登陆有延迟， 所以sdk 登录刷新后,  重新刷一遍隐私协议
            GlobalEvent.OnSDKEvent.AddListener(OnSDKEvent);

            var parent = transform.Find("ImgBg");
            if (parent != null)
            {
                StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(UIDef.FX_UI_UILoginPage,
                (GameObject go) =>
                {
                    if (go == null)
                    {
                        return;
                    }
                    var gob = GameObject.Instantiate<GameObject>(go, parent);
                    if (gob != null)
                    {
                        //gob.transform.SetParent(parent);
                        //gob.transform.SetLocalScale(Vector3.one);
                        gob.transform.localPosition = Vector3.zero;
                        gob.transform.localRotation = Quaternion.identity;

                        UIFXBGAdaptive uIFXBGAdaptive = gob.GetComponent<UIFXBGAdaptive>();
                        if (uIFXBGAdaptive != null)
                        {
                            uIFXBGAdaptive.Init();
                        }
                    }
                });
            }
        }

        private void TipsPrivacyCheck(Action clickCb)
        {
            Frame.Util.ShowMessage(LanguageManager.Instance.GetLanguageByKey("CheckPrivacyTips"));
            UIAPI.ShowMsgBox(73, (eventName) =>
            {
                if (eventName == "SURE")
                {
                    privacyCheck.isOn = true;
                    clickCb?.Invoke();
                }
            }, new object[] { "LinkUrl", GameConfig.URL_THE_STAR_PRIVACY, GameConfig.URL_THE_STAR_LICENSE });

        }
        private bool CheckPrivacy()
        {
            return privacyCheck.isOn;
        }

        private void OnSDKEvent(string sdkEvent, object v)
        {
            if (sdkEvent == "login" && (bool)v)
            {
                // 登录成功 如果已经勾选了，那就 默认设置 勾选的缓存
                if (privacyCheck.isOn)
                {
                    CachePrivacy();
                }
            }
        }

        private void RefreshPrivacyCheck()
        {
            string key = "PrivacyCheck";
            bool has = LocalCacheManager.Instance.Has(key);
            if (!has)
            {
                privacyCheck.isOn = false;
            }
            else
            {
                // 拿到本地 缓存的 sdkUserID
                string sdkUserID = (string)LocalCacheManager.Instance.Get(key);

                privacyCheck.isOn = sdkUserID == SDKManager.Instance.SdkUserID;
            }
        }

        private void OnPrivacyCheckClick(bool isOn)
        {
            if (isOn)
            {
                CachePrivacy();
                SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.CHK_PRIVACY_1);
                SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.CHK_PRIVACY_2);
            }
        }

        private void CachePrivacy()
        {
            string key = "PrivacyCheck";

            LocalCacheManager.Instance.SetValueType(key, SDKManager.Instance.SdkUserID);
        }

        private void RefreshPrivacyCheck(bool isOpen)
        {
            privacyCheck.transform.parent.gameObject.SetActive(isOpen);
        }

        protected override void OnOpen(object arg)
        {
            base.OnOpen(arg);

            m_Mymodule = ModuleManager.Instance.GetModule(ModuleDef.Name.LoginModule) as LoginModule;
            // 渠道 sdk 登录 弹窗, 由界面 自己决定 什么时候弹

            OnBtnClickLoginIPArrowJBtn(false);
            m_VersionTex.text = GameApp.Instance.LocalVersion.ToString();

            if (m_Mymodule != null)
            {
                SetShow(m_Mymodule.M_HidePageCount <= 0);
                recordLoginID = m_Mymodule.M_RecordLoginID;
                m_Mymodule.RegisterRefreshGroupListAction(RefreshGroupList);
            }
            else
            {
                SetShow(true);
            }
            SetDefaultAreaStateIcon();
            M_LoginPanel.SetActive(false);
            M_LoginInfoPanel.SetActive(false);
            m_LoginBtn.gameObject.SetActive(true);
            RefreshPrivacyCheck(true);
            //RectTransform rect = M_LoginInfoPanel.GetComponent<RectTransform>();
            //customRenderTexture = new CustomRenderTexture((int)rect.rect.width, (int)rect.rect.height, GraphicsFormat.R8G8B8A8_UNorm);
            //customRenderTexture.antiAliasing = 0;
            //customRenderTexture.wrapMode = TextureWrapMode.Clamp;
            //customRenderTexture.filterMode = FilterMode.Point;
            //customRenderTexture.initializationMode = CustomRenderTextureUpdateMode.OnDemand;
            //customRenderTexture.initializationSource = CustomRenderTextureInitializationSource.TextureAndColor;
            //customRenderTexture.initializationColor = Color.black;
            //PVRawImage.texture = customRenderTexture;
            //PVMediaPlayer.targetTexture = customRenderTexture;

            GameManager.Instance.IsLoginInitState = true;

            SGF.Debuger.Log($"[公告] UILoginPage  OnOpen set IsLoginInitState : {true}");
            // 下一帧update 执行
            DelayInvoker.DelayInvoke(2f, (args) =>
            {
                if (SDKManager.Instance.IsInit)
                {
                    SGF.Debuger.Log($"[公告] UILoginPage 2s 后开启 sdk 登录");
                    if (m_Mymodule != null && m_Mymodule.M_AgainLoginType != AgainLoginType.BacktrackSelectRole)
                    {
                        SDKManager.Instance.Login();
                    }
                }
            }, null);
        }

        protected override void OnClose(object arg)
        {
            //if (customRenderTexture != null)
            //{
            //    customRenderTexture.Release();
            //}
            loadGroupIcon = null;
            m_Mymodule.UnRegisterRefreshGroupListAction(RefreshGroupList);
            base.OnClose(arg);
        }

        public void SetDefaultAreaStateIcon()
        {
            //string groupName = GameConfig.LocalStr["DefaultRecommondGroup"];
            string groupName = LanguageManager.Instance.GetLanguageByKey("DefaultRecommondGroup");
            groupInfo = m_Mymodule.M_GroupInfo;
            if (groupInfo == null)
            {
                groupInfo = m_Mymodule.RecommendGroupInfo;
            }
            if (groupInfo != null)
            {
                string stateIconName = "Signin_Explosivedegree_fluent";
                if (groupInfo.groupLoad < 0)
                {
                    stateIconName = "Signin_Explosivedegree_Undermaintenance";
                }
                loadGroupIcon = (Sprite sp) =>
                {
                    if (M_LastGroupLoadState != null && sp != null)
                    {
                        M_LastGroupLoadState.sprite = sp;
                    }
                    loadGroupIcon = null;
                };
                AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathLogin, stateIconName, loadGroupIcon);

                groupName = groupInfo.groupName;
            }
            M_LastGroupName.text = groupName;
        }

        private void RefreshGroupList()
        {
            SetDefaultAreaStateIcon();
        }

        // 显示登录开发账号面板
        public void ShowLoginPanel()
        {
            groupInfo = null;
            recordLoginID = "";
            //M_CurrentID.text = "当前账号：" + recordLoginID;
            //M_CurrentID.text = string.Format(GameConfig.LocalStr["CurLoginID"], recordLoginID);
            M_CurrentID.text = string.Format(LanguageManager.Instance.GetLanguageByKey("CurLoginID"), recordLoginID);

            M_LoginPanel.SetActive(true);
            M_LoginInfoPanel.SetActive(false);
            m_LoginBtn.gameObject.SetActive(false);
            // RefreshPrivacyCheck(false);

            M_InputId.text = "";
#if STAR_DEV
            LoginIPPanel.SetActive(true);
#else
            LoginIPPanel.SetActive(false);
#endif
            GameManager.Instance.IsLoginInitState = true;
            SGF.Debuger.Log($"[公告] UILoginPage  ShowLoginPanel set IsLoginInitState : {true}");
        }

        // 显示登录选择区服面板
        public void ShowLoginInfoPanel(string loginID)
        {
            groupInfo = null;
            recordLoginID = loginID;
            //M_CurrentID.text = "当前账号：" + recordLoginID;
            //M_CurrentID.text = string.Format(GameConfig.LocalStr["CurLoginID"], recordLoginID);
            M_CurrentID.text = string.Format(LanguageManager.Instance.GetLanguageByKey("CurLoginID"), recordLoginID);

            M_LoginPanel.SetActive(false);
            M_LoginInfoPanel.SetActive(true);
            m_LoginBtn.gameObject.SetActive(false);
            // RefreshPrivacyCheck(false);

            LoginIPPanel.SetActive(false);
            M_InputId.text = "";
            //M_GroupInfo
            SetDefaultAreaStateIcon();

            GameManager.Instance.IsLoginInitState = true;
            SGF.Debuger.Log($"[公告] UILoginPage ShowLoginInfoPanel set IsLoginInitState : {true}");
        }

        // 只显示登录按钮
        public void ShowLoginBase(string loginID)
        {
            recordLoginID = loginID;
            m_LoginBtn.gameObject.SetActive(true);
            RefreshPrivacyCheck(true);

            //M_CurrentID.text = "当前账号：" + recordLoginID;
            //M_CurrentID.text = string.Format(GameConfig.LocalStr["CurLoginID"], recordLoginID);
            M_CurrentID.text = string.Format(LanguageManager.Instance.GetLanguageByKey("CurLoginID"), recordLoginID);

            M_LoginPanel.SetActive(false);
            M_LoginInfoPanel.SetActive(false);
            M_InputId.text = "";
#if STAR_DEV
            LoginIPPanel.SetActive(true);
#else
            LoginIPPanel.SetActive(false);
#endif
        }

        private void InitLoginIPList()
        {
#if STAR_DEV
            InitLocalIp();
            //開的話 + stardev = 手動選擇服務器
            //#if UNITY_EDITOR
            //            InitLocalIp();
            //#else
            //            Dictionary<int, IpConfig> ipsDic = RemoteConfig.LoginUrls;
            //            if (ipsDic != null && ipsDic.Count > 0)
            //            {
            //                InitRemoteIp();
            //            }
            //            else
            //            {
            //                InitLocalIp();
            //            }
            //#endif
#else
            LoginIPPanel.SetActive(false);
#endif
        }

        private void InitLocalIp()
        {
            List<IpConfig> ips = GlobalDataConfig.Instance.Ips;
            for (int i = 0; i < ips.Count; i++)
            {
                var ipItem = ips[i];
                var gob = GameObject.Instantiate(ItemIP);
                gob.transform.SetParent(LoginIPList);
                gob.transform.localPosition = Vector3.zero;
                gob.transform.localScale = Vector3.one;

                var Btn = gob.GetComponent<JButton>();
                Btn.OnClick = (go) =>
                {
                    m_Mymodule?.SetLoginIP(ipItem.ip);
                    LoginIP.text = ipItem.E_IP;
                    OnBtnClickLoginIPArrowJBtn(false);
                };
                gob.transform.Find("ip").GetComponent<Text>().text = ipItem.E_IP;
            }
            LoginIP.text = GlobalDataConfig.Instance.ipType;
            LoginIPPanel.SetActive(true);
        }

        private void InitRemoteIp()
        {
            Dictionary<int, IpConfig> ipsDic = RemoteConfig.LoginUrls;
            if (ipsDic != null && ipsDic.Count > 0)
            {
                foreach (var item in ipsDic)
                {
                    var ipItem = item.Value;
                    var gob = GameObject.Instantiate(ItemIP);
                    gob.transform.SetParent(LoginIPList);
                    gob.transform.localPosition = Vector3.zero;
                    gob.transform.localScale = Vector3.one;

                    var Btn = gob.GetComponent<JButton>();
                    Btn.OnClick = (go) =>
                    {
                        m_Mymodule?.SetLoginIP(ipItem.ip);
                        LoginIP.text = ipItem.E_IP;
                        OnBtnClickLoginIPArrowJBtn(false);
                    };
                    gob.transform.Find("ip").GetComponent<Text>().text = ipItem.E_IP;
                }
                LoginIP.text = RemoteConfig.LoginUrlDesc;
                LoginIPPanel.SetActive(true);
            }
        }

        private void OnBtnClickLogin()
        {
            SGF.Debuger.LogWarning($"[sdk] 点击按钮登录: OnBtnClickLogin  ");

            //m_LoginBtn.gameObject.SetActive(false);
            //m_Mymodule.ClickLogin();
            //m_Mymodule.ClickLogin();

#if UNITY_EDITOR
            m_Mymodule.ClickLogin();

#else
                        if (m_IsSDKLogin)
                        {
                            m_Mymodule.ClickLogin();
                        }
                        else
                        {
                            ShowLoginPanel();
                        }
#endif
        }

        // 点击登录ID
        private void OnBtnClickLoginID()
        {
            string userName = M_InputId.text.Trim();
            if (string.IsNullOrEmpty(userName) || string.IsNullOrWhiteSpace(userName))
            {
                userName = UnityEngine.Random.Range(1, 999999).ToString();
            }

            recordLoginID = userName;
            //m_Mymodule.SetLocalCache<string>(E_RecordType.RecordLoginID.ToString(), recordLoginID);
            //SetLoginInfoPanel(true);
            SGF.Debuger.LogWarning($"[SDK] 点击输入账号 按钮 进入开发登录: OnBtnClickLoginID");

            m_Mymodule.UserDevLogin(recordLoginID);
            //m_Mymodule.GetOwnerGroupList();
        }

        // 点击登录区
        private void OnBtnClickLoginArea()
        {
            //SetLoginInfoPanel(!M_LoginInfoPanel.activeSelf);
            if (groupInfo != null)
            {
                m_Mymodule.SelectServiceId(groupInfo.groupID);
            }
            else
            {
                OnBtnClickCheckArea();
            }
        }

        // 点击选择区
        private void OnBtnClickCheckArea()
        {
            //m_Mymodule.SetPlayerId(recordLoginID);
            m_Mymodule.GetOwnerGroupList();
        }

        // 点击注销
        private void OnBtnClickLoginOut()
        {
            recordLoginID = "";
            m_Mymodule.LoginOut();
            SetDefaultAreaStateIcon();
        }

        // 点击客服
        private void OnBtnClickService()
        {
            // Frame.Util.ShowSystemMessage(GameConfig.LocalStr["ComingSoon"]);
            SDKManager.Instance.OpenCustomeService();
        }

        // 点击公告
        private void OnBtnClickNotice()
        {
            // UIAPI.ShowMsgBox("游戏近期公告", "十一国庆将近，各位玩家将会获得国庆大礼包,确认测试龙珠系统", "取消|确认|不感兴趣", OnBtnClick);
            //ModuleManager.Instance.CreateModule(ModuleDef.DragonBallModule, "StarPara");
            ModuleManager.Instance.SendMessage(ModuleDef.Name.AnnouncementModule, "OnOpen", 1, false);
        }

        // 点击登录IP箭头
        private void OnBtnClickLoginIPArrowJBtn(bool isShow)
        {
            m_IsLoginIPPanelShow = isShow;
            LoginIPList.gameObject.SetActive(isShow);
            LoginArrowDown.SetActive(!isShow);
            LoginArrowUp.SetActive(isShow);
        }

        // 点击自定义登录IP按钮
        private void OnBtnClickCustomIPJBtn()
        {
            if (string.IsNullOrEmpty(CustomInputField.text) || string.IsNullOrWhiteSpace(CustomInputField.text))
            {
                //StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.AddSystemMessage(GameConfig.LocalStr["InputNoEmpty"]);
                StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.AddSystemMessage(LanguageManager.Instance.GetLanguageByKey("InputNoEmpty"));
                return;
            }
            m_Mymodule?.SetLoginIP(CustomInputField.text);
        }

        // 点击登出SDK按钮
        private void OnBtnClickLoginOutSDKJBtn()
        {
            m_Mymodule?.LoginOut();
        }

        // 适龄提示按钮
        private void OnBtnClickAgeTipsJBtn()
        {
            UIAPI.ShowGameplayInfo(100);
        }

        //------------

        public void SetShow(bool isShow)
        {
            M_MainCanvasGroup.alpha = isShow ? 1 : 0;
            M_MainCanvasGroup.interactable = isShow;
            M_MainCanvasGroup.blocksRaycasts = isShow;
        }

        //------------

        public void OnLogoShowEnd()
        {
            SGF.Debuger.Log("开场动画完成");
            m_Mymodule?.ShowAnnouncement();
        }

    }
}
