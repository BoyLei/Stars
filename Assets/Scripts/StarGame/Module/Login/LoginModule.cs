using ProtoMsg;
using SGF.Module.Framework;
using SGF.Time;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject.Game;
using StarProject.Service.Battle;
using StarProject.Service.Cam;
using StarProject.Service.Language;
using StarProject.Service.Resource;
using StarProject.Service.SDK;
using StarProject.Service.Sound;
using StarProject.Service.User;
using StarProject.Service.UserManager.Data;
using StarProject.UI.Login;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Module
{

    /// <summary>
    /// 实现登录相关的逻辑
    /// 比较关键的逻辑有：断线重连
    /// </summary>
    public class LoginModule : BusinessModule
    {
        #region 登录协议

        private readonly string USER_LOGIN_NEW = "userLoginNew"; //  新的登录协议，获取【时间戳、token等重要参数】   11111
        private readonly string YOUKA_LOGIN = "youkaLogin";      //  SDK登录后 给服务器 发送的 登录http 协议
        private readonly string YOUKA_LOGIN_NEW = "youkaLoginNew";      //  SDK登录后 给服务器 发送的 登录http 协议   

        private readonly string GODCHOOSEHEROREQ = "GodChooseHeroReq"; // 

        private readonly string GET_GROUP_LIST = "getGroupList"; // 获取区服和服务器列表

        private readonly string GET_GROUP_LISTNew = "loginAfter/getGroupList"; // 获取区服和服务器列表


        private readonly string GET_PICK_GROUP = "loginAfter/pickGroup"; // 选择服务器  (可能玩家没创号返回的参数会不同)111带token

        private readonly string CREATE_ROLE = "loginAfter/createRole";   // 创号(根据不同情况返回参数可能不同)111带token 111111111

        private readonly string GET_OWNER_GROUP_LIST = "loginAfter/getOwnerGroupList";   // 得到玩家有号的那些区服111带token

        private readonly string CHOOSE_HERO_HANDLER = "loginAfter/ChooseHeroHandler";    // 选择某个角色进入游戏111带token 1111111111

        private readonly string DELETE_HERO_HANDLER = "loginAfter/DeleteHeroHandler";    // 删除角色111带token 111111111111


        #endregion

        public static string DEFAULT_CHANNEL = "1";
        // public static string DEFAULT_CHANNEL = "310003";

        private string TagFlag = "[LoginModule]";
        public ModuleEvent testLoginModuleEvt { get { return Event("testLoginModuleEvt"); } }
        private UserData mainUserData;

        private string GameServerIpAddress = string.Empty;//"http://10.191.73.20:8001/";

        // 记录的登录账号
        private string m_RecordLoginID = "";
        public string M_RecordLoginID
        {
            get
            {
                return m_RecordLoginID;
            }
            set
            {
                m_RecordLoginID = value;
            }
        }

        // 记录的登录区服
        private GroupInfo m_GroupInfo;
        public GroupInfo M_GroupInfo
        {
            get
            {
                return m_GroupInfo;
            }
            set
            {
                if (value != null)
                {
                    GroupInfo groupInfo = GetGroupInfoById(value.groupID);
                    if (groupInfo != null)
                    {
                        value = groupInfo;
                    }
                }
                m_GroupInfo = value;
            }
        }

        // 记录的登录区服的角色PID
        private UInt64 m_RecordLoginPID = 0;
        public UInt64 M_RecordLoginPID
        {
            get
            {
                if (m_RecordLoginPID == 0)
                {
                    if (GameLoginInfo.M_PickGroupReq != null)
                    {
                        string key = $"{E_RecordType.RecordLoginPID}_{GameServerIpAddress}_{M_RecordLoginID}_{GameLoginInfo.M_PickGroupReq.GroupID}";
                        if (CacheKeyExists(key))
                        {
                            m_RecordLoginPID = GetLocalCache<UInt64>(key);
                        }
                    }
                }
                return m_RecordLoginPID;
            }
        }

        // 推荐的区服
        public GroupInfo RecommendGroupInfo = null;

        public UILoginPage M_UILoginPage;
        private UILoginPage m_UILoginPage
        {
            get
            {
                if (M_UILoginPage == null)
                {
                    if (UIManager.Instance.M_Current_UIPage != null && UIManager.Instance.M_Current_UIPage.name.IndexOf("UILoginPage") != -1)
                    {
                        M_UILoginPage = (UILoginPage)UIManager.Instance.M_Current_UIPage;
                    }
                }
                return M_UILoginPage;
            }
        }

        private SKDLoginResult sdkLoginResult { get => SDKManager.Instance.sdkLoginResult; }

        public AgainLoginType M_AgainLoginType = AgainLoginType.None;

        /// <summary>
        /// 是否已经自动弹出过自动公告
        /// </summary>
        private bool IsAutoTipsAnnouncement = false;

        private Dictionary<string, bool> DelayInvokerDic = new();

        public override void Create(object args = null)
        {
            base.Create(args);
        }

        protected override void Show(object arg)
        {
            //Login需要高配【开】
            {

                GameConfig.FirstMachineCheckCache = GameConfig.MachineQualityLevel;
                GameConfig.MachineQualityLevel = MachineQualityLevel.TopLevel;
                AppMain.Instance.UseHighLevelSetting();
            }

            GetRecordLoginIP();
            GetRecordData();
            DelayInvokerDic.Clear();
            m_GroupListCountdown = SystemConstConfigs.Loop_Get_GroupList_Req;

            SoundManager.Instance.PlayEventBGMName("BGM_Login", null, null);
            CameraManager.Instance.GetCamera(E_CameraType.StarWorldCam).gameObject.SetActive(true);
            DynamicUIRoot.EntityUIRoot.gameObject.SetActive(true);
            EntityRoot.Instance.gameObject.SetActive(true);
            GameManager.Instance.IsLoginInitState = true;
            Debug.Log($"[公告] LoginModule Show set IsLoginInitState : {true}");

            Action cb = () =>
            {
                m_HidePageCount = 0;
                UnityEngine.Vector3 localRotate = new(5f, 0, 0);
                UnityEngine.Vector3 localPos = new(0, 1.2f, -2.7f);
                CameraManager.Instance.SetBattlePos(localRotate, localPos);
                M_AgainLoginType = AgainLoginType.AgainLogin;
                //GetGroupList();
                if (arg != null)
                {
                    M_AgainLoginType = (AgainLoginType)arg;
                }
                if (M_AgainLoginType == AgainLoginType.AgainLogin || M_AgainLoginType == AgainLoginType.None)
                {
#if UNITY_EDITOR
                    //单机脚本段磊测试脚本，如果出现两个page说明点过离线local，要点击离线-clean
                    if (EditorModeTest.EditorMode.IsEditorMode)
                    {
                        SGF.UI.Framework.UIManager.Instance.OpenPage(UIDef.UILoginPage, null, () =>
                        {
                            m_HidePageCount = 0;
                            GameLoginInfo.M_PickSrvAck = EditorModeTest.EditorMode.Instance.localServer.pickSrvAck;
                            //UIManager.Instance.OpenWindow(UIDef.UICreateRoleWindow, GameLoginInfo.M_PickSrvAck);
                            GameManager.Instance.IsLoginInitState = false;
                            UIManager.Instance.OpenWindowAsync(UIDef.UICreateRoleWindow, null, GameLoginInfo.M_PickSrvAck);
                            UnityEngine.Vector3 localRotate = new(5f, 0, 0);
                            UnityEngine.Vector3 localPos = new(0, 1.1f, -2.5f);
                            CameraManager.Instance.SetBattlePos(localRotate, localPos);
                        });
                        return;
                    }
#endif

                    // Debug.LogError($"[公告] loginModule 设置登录状态 : {GameManager.Instance.IsLoginInitState}");
                    AutoLogin();
                    GlobalEvent.OnSDKEvent.RemoveListener(OnSDKEvent);
                    GlobalEvent.OnSDKEvent.AddListener(OnSDKEvent);
                    // GlobalEvent.TestEvent.AddListener(OnTestEvent);

                    GameManager.Instance.Loading.OnProcessBlackEnd();
                }
                else if (M_AgainLoginType == AgainLoginType.BacktrackSelectRole || M_AgainLoginType == AgainLoginType.ChangeLanguage)
                {
                    IsAutoTipsAnnouncement = true;
                    // 返回选角界面
                    SelectServiceId(GameLoginInfo.M_PickGroupReq.GroupID);
                    if (m_UILoginPage != null)
                    {
                        m_UILoginPage.ShowLoginInfoPanel(M_RecordLoginID);
                    }
                    //UIManager.Instance.OpenWindow(UIDef.UICreateRoleWindow, GameLoginInfo.M_PickSrvAck);
                }
            };
            SGF.UI.Framework.UIManager.Instance.OpenPage(UIDef.UILoginPage, null, cb);
        }

        private void OnFixUpdate()
        {
            // 轮询请求服务器列表信息
            FixUpdateGetGroupList();
        }

        public override void Release()
        {
            //GameLoginInfo.M_UserLoginNAck = null;
            GameLoginInfo.M_GetGroupListAck = null;
            GameLoginInfo.M_PickSrvAck = null;
            GameLoginInfo.M_UserCreateNPlayerAck = null;
            GameLoginInfo.M_GetOwnerGroupListAck = null;
            //GameLoginInfo.M_ChossHeroAck = null;  // 这里有Token，先不清理了
            GameLoginInfo.M_DeleteHeroAck = null;
            DelayInvokerDic.Clear();

            MonoHelper.RemoveFixedUpdateListener(OnFixUpdate);

            ResourceFormalManager.Instance.ForceClearMiddleResourceCache("Login");

            base.Release();
        }

        // 获取列表倒计时
        private float m_GroupListCountdown = 20.0f;
        // 刷新列表信息
        private Action m_RefreshGroupList = null;
        public void RegisterRefreshGroupListAction(Action cb)
        {
            m_RefreshGroupList += cb;
        }

        public void UnRegisterRefreshGroupListAction(Action cb)
        {
            m_RefreshGroupList -= cb;
        }

        private void FixUpdateGetGroupList()
        {
            if (m_GroupListCountdown > 0)
            {
                m_GroupListCountdown -= Time.fixedDeltaTime;
                if (m_GroupListCountdown <= 0)
                {
                    GetGroupListNew();
                }
            }
        }

        public void ShowAnnouncement()
        {
            if (!SDKManager.Instance.IsInit && !IsAutoTipsAnnouncement)
            {
                IsAutoTipsAnnouncement = true;
                // 普通登录协议发送后，显示公告弹窗
                ModuleManager.Instance.SendMessage(ModuleDef.Name.AnnouncementModule, "OnOpen", 2, true);
            }
        }

        private void OnSDKEvent(string sdkEvent, object v)
        {
            // 如果 收到了 登录成功的事件, 那就 直接走 sdk 登录

            if (sdkEvent == "login" && (bool)v)
            {
                OnSDKLgoinSuccessed();
            }
            else if (sdkEvent == "loginOut" && (bool)v)
            {
                OnSDKLoginOut();
            }

        }

        // 设置登录服的IP
        public void SetLoginIP(string ip)
        {
            GameServerIpAddress = $"{ip}";
            // 设置 缓存数据 登录服IP
            {
                SetLocalCache<string>($"{E_RecordType.RecordLoginIP}", GameServerIpAddress);
            }
            //GetGroupList();
        }

        public void LoginOut()
        {
            M_RecordLoginID = "";
            M_GroupInfo = null;
            M_AgainLoginType = AgainLoginType.AgainLogin;
            if (SDKManager.Instance.IsInit)
            {
                // 账号登出, 目前没有 做到清理缓存, 看 后面策划需求
                SDKManager.Instance.LoginOut();
            }
            else
            {
                m_UILoginPage.ShowLoginPanel();
            }

            //GameLoginInfo.M_UserLoginNAck = null;
            //GameLoginInfo.M_GetGroupListAck = null;
            //GameLoginInfo.M_PickSrvAck = null;
            //GameLoginInfo.M_UserCreateNPlayerAck = null;
            //GameLoginInfo.M_GetOwnerGroupListAck = null;
            ////GameLoginInfo.M_ChossHeroAck = null;  // 这里有Token，先不清理了
            //GameLoginInfo.M_DeleteHeroAck = null;
        }

        private void GetRecordData()
        {
            // 读取上次缓存的登录账号ID
            {
                string key = $"{E_RecordType.RecordLoginID}_{GameServerIpAddress}";
                if (CacheKeyExists(key))
                {
                    m_RecordLoginID = GetLocalCache<string>(key);
                }
            }

            // 读取上次缓存的登录账号ID的服务器
            GetRecordGroupInfo();
        }

        private void GetRecordGroupInfo()
        {
            M_GroupInfo = null;
            if (!string.IsNullOrEmpty(M_RecordLoginID))
            {
                string key = $"{E_RecordType.RecordGroupInfo}_{GameServerIpAddress}_{M_RecordLoginID}";
                if (CacheKeyExists(key))
                {
                    M_GroupInfo = GetLocalCache<GroupInfo>(key);
                }
            }
        }

        //正式出包流程：就配置内网加GM整包就是内网，不加就是外网
        private void RefreshGroupInfo()
        {
            if (m_GroupInfo == null)
            {
                return;
            }
            GroupInfo groupInfo = GetGroupInfoById(m_GroupInfo.groupID);
            if (groupInfo != null)
            {
                m_GroupInfo = groupInfo;
            }
        }

        // 读取上次缓存的登录服IP
        private void GetRecordLoginIP()
        {
            //【1缓存是本地=上次】
            // GlobalDataConfig，为了开发方便本地测试时，
            //首次：一定为空一定找不到
            //内部开发人员选取一个服务器：ES3本地帮Editor记录选取，帮外网包QA记录选取，帮玩家记录选取（虽然他不能选）；
            string localKey = $"{E_RecordType.RecordLoginIP}";
            if (CacheKeyExists(localKey))
            {
                GameServerIpAddress = GetLocalCache<string>(localKey);
            }



            //【2走配置xml ,本地的/远程的=列出的和active的，配置是远程就是远程的】
#if !GM && !STAR_DEV
           GameServerIpAddress = $"{RemoteConfig.LoginUrl}";//;正式包会替换外网资源，Editor去掉这两个也能测试外网下载资源
#endif


            // 3.什么地址都找不到起码给一个默认登录的地址：（也包含首次/也包含网络坏了）
            if (string.IsNullOrEmpty(GameServerIpAddress) || string.IsNullOrWhiteSpace(GameServerIpAddress))
            {
                GameServerIpAddress = GlobalDataConfig.Instance.GetUrl();
            }


            Debug.Log($"登录服ip={GameServerIpAddress}");
        }

        /// <summary>
        /// C:\Users\maliangbo\AppData\LocalLow\Yoka\Stars : 拓展力更强 ； 更偏重应用级
        /// EasySave : 拓展力稍弱 ； 更偏重系统级
        /// </summary>
        /// <param name="ud"></param>
        private void OnLoginSucSaveUsrNApp(UserData ud)
        {
            UserManager.Instance.UpdateMainUserData(ud);


            //这个是有用的存储功能不可以删除，要开启查询到底是什么问题，马良博
            //AppConfig.Init();
            /*AppConfig.Value.mainUserData = ud;
            AppConfig.Save();*/


            //将登录成功事件通知给整个游戏:Event尽量用在Module之间，或复杂系统
            //GlobalEvent.onLogin.Invoke(true);

            //将UI切换到主城
            //SGF.UI.Framework.UIManager.Instance.EnterMainPage();
        }

        public void UpdateUILoginPageAreaData()
        {
            if (m_UILoginPage != null)
            {
                m_UILoginPage.SetDefaultAreaStateIcon();
            }
        }

        #region 定时器缓存

        private void SetDelayInvoker(string key, bool state)
        {
            if (state == false)
            {
                if (DelayInvoker.ContainInvoke(key))
                {
                    DelayInvoker.CancelInvoke(key);
                }
                if (GetDelayInvokerState(key))
                {
                    UIAPI.CloseRequestLoading2();
                }
            }

            if (DelayInvokerDic.ContainsKey(key))
            {
                DelayInvokerDic[key] = state;
            }
            else
            {
                DelayInvokerDic.Add(key, state);
            }
        }

        private bool GetDelayInvokerState(string key)
        {
            bool state = false;
            if (DelayInvokerDic.TryGetValue(key, out state))
            {

            }
            return state;
        }

        #endregion

        #region 设置UI3DLight

        private int m_HidePageCount = 0;
        public int M_HidePageCount => m_HidePageCount;
        public void SetPageActive(bool isShow, bool isForce)
        {
            if (isShow)
            {
                ShowPage(isForce);
            }
            else
            {
                HidePage(isForce);
            }
        }
        private void HidePage(bool isForceShow = false)
        {
            if (isForceShow)
            {
                m_HidePageCount = 1;
            }
            else
            {
                m_HidePageCount++;
            }
            if (m_HidePageCount == 1 && m_UILoginPage != null)
            {
                //m_UILoginPage.gameObject.SetActive(false);
                m_UILoginPage.SetShow(false);
            }
        }
        private void ShowPage(bool isForceShow = false)
        {
            if (isForceShow)
            {
                m_HidePageCount = 0;
            }
            else
            {
                m_HidePageCount--;
            }
            if (m_HidePageCount <= 0 && m_UILoginPage != null)
            {
                //m_UILoginPage.gameObject.SetActive(true);
                m_UILoginPage.SetShow(true);
            }
        }

        #endregion

        #region [协议]通用解析
        public void SendHttpReq<T1, T2>(string mgsId, T1 sendJson = default, Action<T2> callBack = null) where T1 : class
        {
            string url = GameServerIpAddress + mgsId;
            // string url = $"http://10.191.73.222:8001/{mgsId}";
            // string url = $"http://10.191.73.210:8001/{mgsId}";
            // string url = "http://10.191.73.222:8001/youkaLoginNew";

            HttpUtils.SendPostHttp<T1, T2>(url, sendJson, callBack);
        }



        /// <summary>
        /// 发送包含 登录头信息的 http 请求
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="mgsId"></param>
        /// <param name="sendJson"></param>
        /// <param name="callBack"></param>
        public void SendLoginHeaderHttpReq<T1, T2>(string mgsId, T1 sendJson = default, Action<T2> callBack = null) where T1 : class
        {
            string url = GameServerIpAddress + mgsId;

            HttpUtils.SendLoginHeaderHttpReq(url, sendJson, callBack);
        }

        public void SendLoginHeaderHttpReq<T1, T2, T3>(string mgsId, T1 sendJson = default, Action<T2, T3> callBack = null, T3 resultCallBack = null) where T1 : class where T3 : class
        {
            string url = GameServerIpAddress + mgsId;

            HttpUtils.SendLoginHeaderHttpReq(url, sendJson, callBack, resultCallBack);
        }

        #endregion

        #region [LocalData]本地缓存

        public void SetLocalCache<T>(string key, T value, string fileName = "data")
        {
            Save<T>(key, value, fileName);
        }

        public T GetLocalCache<T>(string key, string fileName = "data")
        {
            return Load<T>(key, fileName);
        }

        public bool CacheKeyExists(string key, string fileName = "data")
        {
            return KeyExists(key, fileName);
        }

        #endregion

        #region [0]新的登录协议，获取【时间戳、token等重要参数】

        /// <summary>
        /// 自动登录 openID(如果有sdk，那就是渠道sdk给的openID.如果是裸包,那就是玩家自己输入的账号)
        /// </summary>
        public void AutoLogin()
        {
            Debug.LogWarning($"[SDK] loginModel 开始自动登录， sdk IsInit: {SDKManager.Instance.IsInit},  has loginResult: {!string.IsNullOrEmpty(SDKManager.Instance.LoginResult)}");

            /// 进入 loginPage 的显示流程. 
            /// 首先判断 是否是 star_dev 环境, 如果是 star_dev，需要选服, 所以啥都不用做
            /// 如果非 选服界面, 判断 sdk 是否 已经登录
            if (SDKManager.Instance.IsInit && SDKManager.Instance.IsLogined)
            {
                // 首先判断 是否已经实名认证, 如果只是登录了 sdk 但是没有实名认证
                if (!SDKManager.Instance.IsRealName && SDKManager.Instance.IsMainLand)
                {
                    Debug.LogWarning($"[sdk] 当前未实名, 所以 autoLogin 不做任何事");
                }
                else
                {
                    Debug.LogWarning($"[sdk] 已经实名认证, 所以 走 登录游戏的流程");
                    LoginGame();
                }
            }

        }

        // 点击开发的登录按钮事件
        public void ClickLogin()
        {
            if (!SDKManager.Instance.IsInit)
            {
                Debug.LogWarning($"[sdk] ClickLogin, sdk 未初始化,  ---》UserDevLogin");

                UserDevLogin();
            }
            else
            {
                if (SDKManager.Instance.IsLogined)
                {
                    {
                        // 如果未实名, 点击 进入 实名认证界面
                        // if (!SDKManager.Instance.IsRealName)
                        // {
                        //     Debug.LogWarning($"[sdk] 当前未实名, 准备执行 实名认证");

                        //     SDKManager.Instance.Login();
                        // }
                        // else
                        // {
                        //     // 如果已经实名认证了, 那就直接登录游戏
                        //     LoginGame();
                        // }
                    }

                    LoginGame();

                }
                else
                {
                    SDKManager.Instance.Login();
                }
            }
        }

        public void UserDevLogin()
        {
            Debug.Log($"[SDK] 使用了 开发登录 UserDevLogin ");

            if (string.IsNullOrEmpty(M_RecordLoginID) || string.IsNullOrWhiteSpace(M_RecordLoginID))
            {
                m_UILoginPage.ShowLoginPanel();
            }
            else
            {
                UserDevLogin(M_RecordLoginID);
            }
        }

        /// <summary>
        /// 登录 游戏 的流程. 如果是 成年人, 直接 发送给服务器发送 sdk 登录;
        /// 如果是 未成年人, 走 未成年人登录流程
        /// </summary>
        public void LoginGame()
        {
            if (SDKManager.Instance.IsMainLand)
            {
                // 如果已经实名, 并且已经成年, 那就 直接 发送sdk 登录
                if (SDKManager.Instance.IsAdult)
                {
                    Debug.Log($"[sdk] 成年人认证成功, 发送 服务器sdk 登录");
                    SendYoukaLogin();
                }
                else
                {
                    //如果 没有成年, 就需要 跟服务器 一样,判断 是否是节假日，走 未成年人 登录流程 
                    OnChildLogin();
                }
            }
            else
            {
                Debug.Log($"[sdk] 不是大陆,走 服务器sdk 登录");

                SendYoukaLogin();
            }

        }

        /// <summary>
        /// sdk login 成功的接口
        /// </summary>
        public void OnSDKLgoinSuccessed()
        {
            M_RecordLoginID = sdkLoginResult.userId;
            Debug.Log($"1111是循环的吗？, 设置 M_RecordLoginID: {M_RecordLoginID}, {SDKManager.Instance.LoginResult}");


            GetRecordGroupInfo();

            Debug.LogWarning($"[SDK] 进入sdk登录回调流程, IsAdult: {SDKManager.Instance.IsAdult} , 是否绑定IDCard: {SDKManager.Instance.IsRealName}, 是否是海外: {!SDKManager.Instance.IsMainLand} ");

            /// 2024/6/9
            /// 新版的 sdk 登录流程, 实名认证流程在sdk 内部,
            /// 所以 客户端不需要关心 是否实名
            /// 
            /// 现在流程时：
            /// 玩家进入游戏后，客户端调用login接口，弹出SDK弹窗，通行证登录，实名认证未完成，返回给游戏登录失败
            if (SDKManager.Instance.IsMainLand)
            {
                Debug.Log($"[SDK] 进入sdk登录回调流程, 绑定了idCard, 进入 成年人/未成年人 登录流程 ");

                // 绑定完账号之后,重新 login sdk , 此时判断 玩家是否 成人
                if (SDKManager.Instance.IsAdult)
                {
                    // 如果是 成人, 发送给服务器 sdk 登录，进入选服流程
                    SendYoukaLogin();
                }
                else
                {
                    // 如果是未成年人, 判断当前是否是 节假日
                    OnChildLogin();
                }
            }
            else
            {
                Debug.LogWarning($"[SDK] 海外用户,直接走 登录 ");

                // 海外用户不需要走 是否未成年人 的 登录判断流程, 所以 直接发送 登录
                SendYoukaLogin();
            }
        }

        public void OnSDKLoginOut()
        {
            M_RecordLoginID = "";
            M_GroupInfo = null;
            Debug.Log($"[sdk] 登出成功, 准备执行 登出界面逻辑");
            m_UILoginPage.ShowLoginBase("");
        }


        // 未成年人 登录
        public void OnChildLogin()
        {

            // 如果是 节假日, 那就可以进入 游戏
            if (TimeUtils.IsServerNowHoliday())
            {
                Debug.Log($"[SDK] 未成年 , 当前是 节假日, 发送 sdk 登录流程");
                // 发送 sdk 登录流程
                SendYoukaLogin();
            }
            else
            {
                Debug.Log($"[SDK] 未成年 , 弹出 防成谜弹窗");
                DelayInvoker.DelayInvoke(0, (object[] args) =>
                {
                    // 防成谜弹出, 点击确认 会关闭游戏
                    UIAPI.ShowMsgBox(44, (string v) =>
                    {
                        Debug.Log($"[SDK] 未成年 , 准备登出sdk");
                        SDKManager.Instance.LoginOut();
                        // Application.Quit();
                    }, new object[] { });
                });

            }
        }

        public void UserDevLogin(string openId)
        {
            M_RecordLoginID = openId;
            GetRecordGroupInfo();

            GameLoginInfo.M_UserLoginReq.Openid = openId;
            GameLoginInfo.M_UserLoginReq.Channel = DEFAULT_CHANNEL;         //渠道SDK

            // CommonHttpServerReq(USER_LOGIN_NEW, UserDevLoginCallBack, GameLoginInfo.M_UserLoginReq);

            SendHttpReq<StarProjectDef.UserLoginReq, UserLoginRet>(USER_LOGIN_NEW, GameLoginInfo.M_UserLoginReq, UserDevLoginCallBack);


            DelayInvoker.DelayInvoke(USER_LOGIN_NEW, 1, DelayShowLoginLoadingUI, null);


        }

        private void DelayShowLoginLoadingUI(object[] args)
        {
            SetDelayInvoker(USER_LOGIN_NEW, true);

            Action onOutAction = () =>
            {
                Frame.Util.ShowSystemMessage(LanguageManager.Instance.GetLanguageByKey("RequestTimeoutTips"));
            };
            UIAPI.ShowRequestLoading2(SystemConstConfigs.Login_ReqWaitTimeout * 1000, onOutAction, null, LanguageManager.Instance.GetLanguageByKey("RequestLoadingTips_1"), LoadingWidgetTypeEnum.AwaitResponse);
        }

        private void UserDevLoginCallBack(UserLoginRet userLoginRet)
        {
            Debug.Log($"[开发DEV] 收到登录回复: {userLoginRet}");

            SetDelayInvoker(USER_LOGIN_NEW, false);
            // note: 2023/12/4 夏哥的 意思 result 返回 0 || 1 都表示成功, 不过此处 没有使用 result,所以先增加个备注
            GameLoginInfo.M_UserLoginNAck = userLoginRet;
            if (GameLoginInfo.M_UserLoginNAck != null)
            {
                if (GameLoginInfo.M_UserLoginNAck.Result > 1)
                {
                    Frame.Util.ShowMessageByCode((int)GameLoginInfo.M_UserLoginNAck.Result);
                }
                else
                {
                    GetGroupListNew();
                    MonoHelper.RemoveFixedUpdateListener(OnFixUpdate);
                    MonoHelper.AddFixedUpdateListener(OnFixUpdate);
                    m_UILoginPage.ShowLoginInfoPanel(M_RecordLoginID);
                }
            }
            else
            {
                DelayShowLoginLoadingUI(null);
            }
        }

        YoukaLoginNewReq youkaLoginNewReq = new();
        private YoukaLoginNewReq FormateSendSDKSeverLogin()
        {
            if (sdkLoginResult != null)
            {
                youkaLoginNewReq.LoginData = sdkLoginResult.dataString;
                /// 2024/7/16 新版sdk 平台id 字段被干掉了,所以 先默认传0
                youkaLoginNewReq.PlatformID = 0;

                youkaLoginNewReq.Device = new ClientDeviceInfo();
                var dv = youkaLoginNewReq.Device.DeviceInfo;


                var sdkData = SDKManager.Instance.postServerSDKData;
                if (sdkData != null)
                {

                    if (sdkData.one_id != null)
                    {
                        dv.Add("one_id", sdkData.one_id);
                    }
                    else
                    {
                        Debug.LogWarning($"[SDK] postServerSDKData one_id 没有!!!");
                    }

                    if (sdkData.device_id != null)
                    {
                        dv.Add("device_id", sdkData.device_id);
                    }
                    else
                    {
                        Debug.LogWarning($"[SDK] postServerSDKData device_id 没有!!!");
                    }

                    if (sdkData.app_package_name != null)
                    {
                        dv.Add("app_package_name", sdkData.app_package_name);
                    }
                    else
                    {
                        Debug.LogWarning($"[SDK] postServerSDKData app_package_name 没有!!!");
                    }

                    if (sdkData.device_model != null)
                    {
                        dv.Add("device_model", sdkData.device_model);
                    }
                    else
                    {
                        Debug.LogWarning($"[SDK] postServerSDKData device_model 没有!!!");
                    }


                    if (sdkData.os_version != null)
                    {
                        dv.Add("os_version", sdkData.os_version);
                    }
                    else
                    {
                        Debug.LogWarning($"[SDK] postServerSDKData os_version 没有!!!");
                    }

                    if (sdkData.cpsid != null)
                    {
                        dv.Add("cpsid", sdkData.cpsid);
                    }
                    else
                    {
                        Debug.LogWarning($"[SDK] postServerSDKData cpsid 没有!!!");
                    }


                    if (sdkData.language != null)
                    {
                        dv.Add("language", sdkData.language);
                    }
                    else
                    {
                        Debug.LogWarning($"[SDK] postServerSDKData language 没有!!!");
                    }


                    if (sdkData.channel != null)
                    {
                        dv.Add("channel", sdkData.channel);
                    }
                    else
                    {
                        Debug.LogWarning($"[SDK] postServerSDKData channel 没有!!!");
                    }

                    if (sdkData.os_type != null)
                    {
                        dv.Add("os_type", sdkData.os_type);
                    }
                    else
                    {
                        Debug.LogWarning($"[SDK] postServerSDKData os_type 没有!!!");
                    }



                    if (sdkData.app_version != null)
                    {
                        dv.Add("app_version", sdkData.app_version);
                    }
                    else
                    {
                        Debug.LogWarning($"[SDK] postServerSDKData app_version 没有!!!");
                    }

                    if (sdkData.app_version_code != null)
                    {
                        dv.Add("app_version_code", sdkData.app_version_code);
                    }
                    else
                    {
                        Debug.LogWarning($"[SDK] postServerSDKData app_version_code 没有!!!");
                    }

                    if (sdkData.user_id != null)
                    {
                        dv.Add("user_id", sdkData.user_id);
                    }
                    else
                    {
                        Debug.LogWarning($"[SDK] postServerSDKData user_id 没有!!!");
                    }


                    if (sdkData.yoka_id != null)
                    {
                        dv.Add("yoka_id", sdkData.yoka_id);
                    }
                    else
                    {
                        Debug.LogWarning($"[SDK] postServerSDKData yoka_id 没有!!!");
                    }


                    if (sdkData.user_type != null)
                    {
                        dv.Add("user_type", sdkData.user_type);
                    }
                    else
                    {
                        Debug.LogWarning($"[SDK] postServerSDKData user_type 没有!!!");
                    }
                }
                else
                {
                    Debug.LogError("[sdk] postServerSDKData 是 null！！！");
                }


                return youkaLoginNewReq;
            }

            return null;
        }

        /// <summary>
        /// sdk 登录成功后，点击登录按钮给服务器 发送的 登录请求
        /// </summary>
        private void SendYoukaLogin()
        {
            Debug.Log($"[SDK] 准备发送登录 SendYoukaLogin : {SDKManager.Instance.LoginResult} , sdkLoginResult: {sdkLoginResult}");
            // 登录 id 先用 topUserID,后面 跟夏哥 确认一下
            if (sdkLoginResult != null)
            {
                GameLoginInfo.M_UserLoginReq.Openid = sdkLoginResult.SdkUserName;
                GameLoginInfo.M_UserLoginReq.Channel = sdkLoginResult.ChannelID;         //渠道SDK


                Debug.Log($"[SDK] 发送登录 SendYoukaLogin : YOUKA_LOGIN_NEW: {YOUKA_LOGIN_NEW}");

                // CommonHttpServerReq(YOUKA_LOGIN_NEW, UserLoginNewCallBack, null, sendStr);
                SendHttpReq<YoukaLoginNewReq, UserLoginRet>(YOUKA_LOGIN_NEW, FormateSendSDKSeverLogin(), UserLoginNewCallBack);


            }

            // 油卡登录协议发送后，显示公告弹窗
            if (!IsAutoTipsAnnouncement)
            {
                IsAutoTipsAnnouncement = true;
                // 普通登录协议发送后，显示公告弹窗
                ModuleManager.Instance.SendMessage(ModuleDef.Name.AnnouncementModule, "OnOpen", 1, true);
            }
        }

        private void UserLoginNewCallBack(UserLoginRet userLoginRet)
        {
            Debug.Log($"[SDK] UserLoginNewCallBack 收到登录回复: {userLoginRet}");
            GameLoginInfo.M_UserLoginNAck = userLoginRet;

            if (GameLoginInfo.M_UserLoginNAck != null)
            {
                if (GameLoginInfo.M_UserLoginNAck.Result > 1)
                {
                    Frame.Util.ShowMessageByCode((int)GameLoginInfo.M_UserLoginNAck.Result);
                    Debug.Log($"UserLoginNewCallBack 收到登录回复: 登录失败");
                }
                else
                {
                    GetGroupListNew();
                    MonoHelper.RemoveFixedUpdateListener(OnFixUpdate);
                    MonoHelper.AddFixedUpdateListener(OnFixUpdate);
                    if (m_UILoginPage != null)
                    {
                        Debug.Log($"UserLoginNewCallBack 收到登录回复: 刷新界面 打开登录区服按钮");
                        m_UILoginPage.ShowLoginInfoPanel(M_RecordLoginID);
                    }
                }
            }
            else
            {
                Debug.Log($"UserLoginNewCallBack 收到登录回复: 超时");
            }
        }

        #endregion

        #region [1]获取区服列表
        /// <summary>
        /// 1获取区服和服务器列表
        /// </summary>
        /// <param name="acct"></param>
        private void GetGroupList()
        {
            // CommonHttpServerReq(GET_GROUP_LIST, this.ServiceListCallBack, null);
            SendHttpReq<System.Object, GetGroupListAck>(GET_GROUP_LIST, null, ServiceListCallBack);

            m_GroupListCountdown = SystemConstConfigs.Loop_Get_GroupList_Req;
        }

        /// <summary>
        /// 1返回服务器区服列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void ServiceListCallBack(GetGroupListAck getGroupListAck)
        {
            GameLoginInfo.M_GetGroupListAck = getGroupListAck;

            SetRecommendGroupInfo();
            RefreshGroupInfo();

            m_RefreshGroupList?.Invoke();
        }

        private void GetGroupListNew(Action<bool> cb = null)
        {
            Debug.Log($" 拉取服务器列表:  [GetGroupListNew] : {GameServerIpAddress} , GET_GROUP_LISTNew: {GET_GROUP_LISTNew}");
            // HttpUtils.CommonHttpServerAfterReq2(GameServerIpAddress, GET_GROUP_LISTNew, ServiceListCallBack, null, cb);

            SendLoginHeaderHttpReq<object, GetGroupListAck, Action<bool>>(GET_GROUP_LISTNew, null, ServiceListCallBack, cb);

            if (cb != null)
            {
                DelayInvoker.DelayInvoke(GET_GROUP_LISTNew, 1, DelayShowGetGroupListNewLoadingUI, null);
            }
        }

        private void DelayShowGetGroupListNewLoadingUI(object[] args)
        {
            SetDelayInvoker(GET_GROUP_LISTNew, true);
            Action onOutAction = () =>
            {
                Frame.Util.ShowSystemMessage(LanguageManager.Instance.GetLanguageByKey("RequestTimeoutTips"));
            };
            UIAPI.ShowRequestLoading2(SystemConstConfigs.Login_ReqWaitTimeout * 1000, onOutAction, null, LanguageManager.Instance.GetLanguageByKey("RequestLoadingTips_3"), LoadingWidgetTypeEnum.AwaitResponse);
        }

        private void ServiceListCallBack(GetGroupListAck getGroupListAck, Action<bool> cb = null)
        {
            Debug.Log($"拉取服务器区服列表 ServiceListCallBack response= {getGroupListAck}");
            SetDelayInvoker(GET_GROUP_LISTNew, false);

            GameLoginInfo.M_GetGroupListAck = getGroupListAck;
            if (GameLoginInfo.M_GetGroupListAck != null)
            {
                if (GameLoginInfo.M_GetGroupListAck.Result > 1)
                {
                    //Frame.Util.ShowMessageByCode(GameLoginInfo.M_GetGroupListAck.Result);
                    Debug.LogWarning($"拉取服务器区服列表 Result={GameLoginInfo.M_GetGroupListAck.Result}");
                    cb?.Invoke(false);
                }
                else
                {
                    cb?.Invoke(true);
                    SetRecommendGroupInfo();
                    RefreshGroupInfo();
                    m_RefreshGroupList?.Invoke();
                }
            }
            m_GroupListCountdown = SystemConstConfigs.Loop_Get_GroupList_Req;
        }

        /// <summary>
        /// 根据区服ID获的区服的信息
        /// </summary>
        /// <param name="groupID"></param>
        /// <returns></returns>
        public GroupInfo GetGroupInfoById(UInt32 groupID)
        {
            if (groupID <= 0)
            {
                return null;
            }
            GetGroupListAck GroupListAck = GameLoginInfo.M_GetGroupListAck;
            if (GroupListAck != null && GroupListAck.areaList.Count > 0)
            {
                for (int i = 0; i < GroupListAck.areaList.Count; i++)
                {
                    List<GroupInfo> groupList = GroupListAck.areaList[i].GroupList;
                    for (int j = 0; j < groupList.Count; j++)
                    {
                        if (groupList[j].groupID == groupID)
                        {
                            return groupList[j];
                        }
                    }
                }
            }
            return null;
        }

        private void SetRecommendGroupInfo()
        {
            RecommendGroupInfo = null;
            GetGroupListAck GroupListAck = GameLoginInfo.M_GetGroupListAck;
            if (GroupListAck != null && GroupListAck.areaList.Count > 0)
            {
                AreaInfo areaInfo = GroupListAck.areaList[0];
                if (areaInfo != null)
                {
                    List<GroupInfo> groupList = areaInfo.GroupList;
                    groupList.Sort(CompareToRecommedTime);
                    if (groupList.Count > 0)
                    {
                        RecommendGroupInfo = groupList[0];
                    }
                }
            }
        }

        public int CompareToRecommedTime(GroupInfo x, GroupInfo y)
        {
            // 开服》推荐》开服时间
            if (x.groupLoad > 0 && y.groupLoad < 0)
            {
                return -1;
            }
            else if (x.groupLoad < 0 && y.groupLoad > 0)
            {
                return 1;
            }
            else if (x.isRecommend && !y.isRecommend)
            {
                return -1;
            }
            else if (y.isRecommend && !x.isRecommend)
            {
                return 1;
            }
            else
            {
                return -x.CreateTime.CompareTo(y.CreateTime);
            }
        }

        private void OpenAreaListWindow()
        {
            Debug.Log($"[公告] LoginModule OpenAreaListWindow set IsLoginInitState : {false}");
            GameManager.Instance.IsLoginInitState = false;
            //Action<UIWindow> action = (UIWindow ui) =>
            //{
            //    //if (ui != null)
            //    //{
            //    //    ui.onClose += (arg) =>
            //    //    {
            //    //        string str = arg as string;
            //    //        if (string.IsNullOrEmpty(str))
            //    //        {
            //    //            this.Log("[UI][WINDOW]:被关闭事件_例子");
            //    //            return;
            //    //        }
            //    //    };
            //    //}
            //};
            UIManager.Instance.OpenWindowAsync(UIDef.UIAreaListWindow, null, GameLoginInfo.M_GetGroupListAck);
        }

        #endregion

        #region [2]选择服务器

        /// <summary>
        /// 2选择服务器
        /// (可能玩家没创号返回的参数会不同)
        /// </summary>
        public void SelectServiceId(uint groupId)
        {

            // 判断区服是否可以进入
            GroupInfo groupInfo = GetGroupInfoById(groupId);
            if (groupInfo != null)
            {
                if (groupInfo.groupLoad < 0)
                {
                    // 区服正在维护中，不可进入
                    Frame.Util.ShowMessage("服务器维护中");
                    return;
                }
            }

            Debug.Log($" 拉取服务器列表:  [SelectServiceId] : {GameServerIpAddress} , GET_PICK_GROUP: {GET_PICK_GROUP} , groupId : {groupId}");
            GameLoginInfo.M_PickGroupReq.Channel = DEFAULT_CHANNEL;         //渠道SDK
            GameLoginInfo.M_PickGroupReq.GroupID = groupId;
            GameLoginInfo.M_PickGroupReq.Openid = M_RecordLoginID;

            SendLoginHeaderHttpReq<PickGroupReq, PickSrvAck>(GET_PICK_GROUP, GameLoginInfo.M_PickGroupReq, PickGroupCallBack);

            DelayInvoker.DelayInvoke(GET_PICK_GROUP, 1, DelayPickGroupLoadingUI, null);
        }

        private void DelayPickGroupLoadingUI(object[] args)
        {
            SetDelayInvoker(GET_PICK_GROUP, true);
            Action onOutAction = () =>
            {
                Frame.Util.ShowSystemMessage(LanguageManager.Instance.GetLanguageByKey("RequestTimeoutTips"));
            };
            UIAPI.ShowRequestLoading2(SystemConstConfigs.Login_ReqWaitTimeout * 1000, onOutAction, null, LanguageManager.Instance.GetLanguageByKey("RequestLoadingTips_3"), LoadingWidgetTypeEnum.AwaitResponse);
        }

        /// <summary>
        /// 2选择服务器获取数据
        /// </summary>
        /// <param name="originalRequest"></param>
        /// <param name="response"></param>
        private void PickGroupCallBack(PickSrvAck pickSrvAck)
        {
            Debug.Log($"登录流程 PickGroupCallBack 2选择服务器获取数据");
            SetDelayInvoker(GET_PICK_GROUP, false);

            //M_Layer
            GameLoginInfo.M_PickSrvAck = pickSrvAck;
            if (GameLoginInfo.M_PickSrvAck != null)
            {
                if (GameLoginInfo.M_PickSrvAck.Result > 1)
                {
                    Frame.Util.ShowMessageByCode(GameLoginInfo.M_PickSrvAck.Result);
                }
                else
                {
                    Debug.Log($"[公告] LoginModule PickGroupCallBack set IsLoginInitState : {false}");
                    GameManager.Instance.IsLoginInitState = false;
                    UIManager.Instance.OpenWindowAsync(UIDef.UICreateRoleWindow, null, GameLoginInfo.M_PickSrvAck);
                }
            }
        }

        public void LoginGroupId(uint groupId, Action<bool> cb)
        {
            Debug.Log($"登录流程 2选择服务器获取数据 [LoginGroupId] : {GameServerIpAddress} , GET_PICK_GROUP: {GET_PICK_GROUP} , groupId : {groupId}");

            GameLoginInfo.M_PickGroupReq.Channel = DEFAULT_CHANNEL;         //渠道SDK
            GameLoginInfo.M_PickGroupReq.GroupID = groupId;
            GameLoginInfo.M_PickGroupReq.Openid = M_RecordLoginID;

            // HttpUtils.CommonHttpServerAfterReq2(GameServerIpAddress, GET_PICK_GROUP, LoginGroupIdCallBack, GameLoginInfo.M_PickGroupReq, cb);
            SendLoginHeaderHttpReq<PickGroupReq, PickSrvAck, Action<bool>>(GET_PICK_GROUP, GameLoginInfo.M_PickGroupReq, LoginGroupIdCallBack, cb);

        }

        private void LoginGroupIdCallBack(PickSrvAck pickSrvAck, Action<bool> cb)
        {
            Debug.Log($"登录流程 LoginGroupIdCallBack 2选择服务器获取数据: {pickSrvAck}");

            GameLoginInfo.M_PickSrvAck = pickSrvAck;
            if (GameLoginInfo.M_PickSrvAck != null)
            {
                if (GameLoginInfo.M_PickSrvAck.Result > 1)
                {
                    Frame.Util.ShowMessageByCode(GameLoginInfo.M_PickSrvAck.Result);
                }
            }
            cb?.Invoke(true);
        }
        #endregion

        #region [3]创建账号
        /// <summary>
        /// 3创建账号
        /// (可能玩家没创号返回的参数会不同)
        /// </summary>
        public void CreateAccount(string nickName, int modelID, int jobID, Action<bool> cb)
        {
            Debug.Log($"创建账号 UID={GameLoginInfo.M_UserLoginNAck.UID},GroupID={GameLoginInfo.M_PickGroupReq.GroupID},NickName={nickName},ModelID={modelID},JobID={jobID}");
            GameLoginInfo.M_UserCreateNPlayerReq = new UserCreateNPlayerReq();
            GameLoginInfo.M_UserCreateNPlayerReq.UID = (UInt64)GameLoginInfo.M_UserLoginNAck.UID;
            GameLoginInfo.M_UserCreateNPlayerReq.GroupID = (int)GameLoginInfo.M_PickGroupReq.GroupID;
            GameLoginInfo.M_UserCreateNPlayerReq.NickName = nickName;
            GameLoginInfo.M_UserCreateNPlayerReq.ModelID = modelID;
            GameLoginInfo.M_UserCreateNPlayerReq.JobID = jobID;
            GameLoginInfo.M_UserCreateNPlayerReq.Equip1MainColor = "255;255;255";
            GameLoginInfo.M_UserCreateNPlayerReq.Equip1SubColor = "255;255;255";

            // HttpUtils.CommonHttpServerAfterReq2(GameServerIpAddress, CREATE_ROLE, CreateAccountCallBack, GameLoginInfo.M_UserCreateNPlayerReq, cb);
            SendLoginHeaderHttpReq<UserCreateNPlayerReq, UserCreateNPlayerAck, Action<bool>>(CREATE_ROLE, GameLoginInfo.M_UserCreateNPlayerReq, CreateAccountCallBack, cb);

            DelayInvoker.DelayInvoke(CREATE_ROLE, 1, DelayShowCreateRoleLoadingUI, null);
        }

        private void DelayShowCreateRoleLoadingUI(object[] args)
        {
            SetDelayInvoker(CREATE_ROLE, true);
            Action onOutAction = () =>
            {
                Frame.Util.ShowSystemMessage(LanguageManager.Instance.GetLanguageByKey("RequestTimeoutTips"));
            };
            UIAPI.ShowRequestLoading2(SystemConstConfigs.Login_ReqWaitTimeout * 1000, onOutAction, null, LanguageManager.Instance.GetLanguageByKey("RequestLoadingTips_3"), LoadingWidgetTypeEnum.AwaitResponse);
        }

        /// <summary>
        /// 3创建账号回调
        /// </summary>
        private void CreateAccountCallBack(UserCreateNPlayerAck userCreateNPlayerAck, Action<bool> cb)
        {
            Debug.Log($"登录流程 CreateAccountCallBack 3创建账号回调: {userCreateNPlayerAck}");
            SetDelayInvoker(CREATE_ROLE, false);

            GameLoginInfo.M_UserCreateNPlayerAck = userCreateNPlayerAck;
            if (GameLoginInfo.M_UserCreateNPlayerAck != null)
            {
                if (GameLoginInfo.M_UserCreateNPlayerAck.Result > 1)
                {
                    Frame.Util.ShowMessageByCode(GameLoginInfo.M_UserCreateNPlayerAck.Result);
                    // 创角失败
                    //Frame.Util.ShowSystemMessage(GameLoginInfo.M_UserCreateNPlayerAck.ResultMsg);
                    //if (GameLoginInfo.M_UserCreateNPlayerAck.Result == 209)
                    //{
                    //    // 昵称重名
                    //    cb?.Invoke(false);
                    //}
                    cb?.Invoke(false);
                }
                else
                {
                    //SDKManager.Instance.UpLoadRoleData(GameLoginInfo.M_UserCreateNPlayerAck.PlayerData[0].PID.ToString(),
                    //    GameLoginInfo.M_UserCreateNPlayerReq.NickName, "1", GSSDK.GSRole.RoleType.TYPE_CREATE_ROLE, (int)GameLoginInfo.M_PickGroupReq.GroupID);

                    var groupInfo = GetGroupInfoById(GameLoginInfo.M_PickGroupReq.GroupID);
                    string groupName = string.Empty;
                    if (groupInfo != null)
                    {
                        groupName = groupInfo.groupName;
                    }
                    SDKManager.Instance.ReportSDKCreateRole(
                           ServerId: GameLoginInfo.M_PickGroupReq.GroupID.ToString(),
                           ServerName: groupName,
                           RoleId: GameLoginInfo.M_UserCreateNPlayerAck.PlayerData[0].PID.ToString(),
                           RoleName: GameLoginInfo.M_UserCreateNPlayerReq.NickName,
                           RoleLevel: "1",
                           CreateTime: GameLoginInfo.GetCreateTime().ToString()
                           );
                    cb?.Invoke(true);
                    // 创角成功后请求进入游戏
                    ChoseHeroAck(GameLoginInfo.M_UserCreateNPlayerAck.PlayerData[0].PID, null);
                }
            }
        }

        #endregion

        #region [4] 得到玩家有号的那些区服
        /// <summary>
        /// 获得区服
        /// (可能玩家没创号返回的参数会不同)
        /// </summary>
        public void GetOwnerGroupList()
        {
            if (GameLoginInfo.M_GetGroupListAck == null || GameLoginInfo.M_GetGroupListAck.areaList == null || GameLoginInfo.M_GetGroupListAck.areaList.Count <= 0)
            {
                // 新啦一下所有的区服
                //GetGroupList();
                Action<bool> cb = (isSuccess) =>
                {
                    if (isSuccess)
                    {
                        GetOwnerGroupList();
                    }
                };
                GetGroupListNew(cb);
                return;
            }

            GameLoginInfo.M_GetSrvSingleReq = new GetSrvSingleReq();
            GameLoginInfo.M_GetSrvSingleReq.Openid = M_RecordLoginID;
            GameLoginInfo.M_GetSrvSingleReq.Channel = DEFAULT_CHANNEL;

            SendLoginHeaderHttpReq<GetSrvSingleReq, GetOwnerGroupListAck>(GET_OWNER_GROUP_LIST, GameLoginInfo.M_GetSrvSingleReq, GetOwnerGroupListCallBack);

        }

        /// <summary>
        /// 4账号区服回调
        /// </summary>
        public void GetOwnerGroupListCallBack(GetOwnerGroupListAck getOwnerGroupListAck)
        {
            Debug.Log($"登录流程 GetOwnerGroupListCallBack 4账号区服回调: {getOwnerGroupListAck}");

            GameLoginInfo.M_GetOwnerGroupListAck = getOwnerGroupListAck;
            OpenAreaListWindow();
        }

        #endregion

        #region [5]选择某个角色进入游戏

        /// <summary>
        /// 5 选择角色进入游戏
        /// </summary>
        /// <param name="pid"></param>
        public void ChoseHeroAck(UInt64 pid, Action<bool> cb)
        {
            GameLoginInfo.M_ChooseHeroReq = new ChooseHeroReq();
            GameLoginInfo.M_ChooseHeroReq.PID = pid;
            GameLoginInfo.M_ChooseHeroReq.GroupID = GameLoginInfo.M_PickGroupReq.GroupID;

            string recordLoginPIDKey = $"{E_RecordType.RecordLoginPID}_{GameServerIpAddress}_{M_RecordLoginID}_{GameLoginInfo.M_PickGroupReq.GroupID}";
#if UNITY_EDITOR
            if (EditorModeTest.EditorMode.IsEditorMode)
            {
                GameLoginInfo.M_UserLoginNAck = new UserLoginRet();
                GameLoginInfo.M_UserLoginNAck.UID = (ulong)pid;
                GameLoginInfo.M_ChooseHeroReq.UID = pid;
                // 设置 缓存数据
                SetLocalCache<UInt64>(recordLoginPIDKey, pid);
                // 如果是 离线编译器 模式, 那就 不走发送协议, 而是本地直接走 登录成功
                OnLoginSuccessed(cb);
                return;
            }
#endif
            Debug.Log($"选择那个角色进入游戏 pid={pid},GroupID={GameLoginInfo.M_PickGroupReq.GroupID},UID={GameLoginInfo.M_UserLoginNAck.UID}");

            GameLoginInfo.GameServerIpAddress = GameServerIpAddress;


            // HttpUtils.CommonHttpServerAfterReq2(GameServerIpAddress, CHOOSE_HERO_HANDLER, ChossHeroAckCallBack, GameLoginInfo.M_ChooseHeroReq, cb);

            SendLoginHeaderHttpReq<ChooseHeroReq, ChossHeroAck, Action<bool>>(CHOOSE_HERO_HANDLER, GameLoginInfo.M_ChooseHeroReq, ChossHeroAckCallBack, cb);

            GameLoginInfo.M_ChooseHeroReq.UID = (UInt64)GameLoginInfo.M_UserLoginNAck.UID;

            DelayInvoker.DelayInvoke(CHOOSE_HERO_HANDLER, 1, DelayShowChooseHeroHandlerLoadingUI, null);


            // 设置 缓存数据 登录账号
            {
                string key = $"{E_RecordType.RecordLoginID}_{GameServerIpAddress}";
                SetLocalCache<string>(key, M_RecordLoginID);
            }
            // 设置 缓存数据 登录区服
            {
                string key = $"{E_RecordType.RecordGroupInfo}_{GameServerIpAddress}_{M_RecordLoginID}";
                SetLocalCache<GroupInfo>(key, M_GroupInfo);
            }
            // 设置 缓存数据 登录区服的角色PID
            {
                SetLocalCache<UInt64>(recordLoginPIDKey, pid);
            }
        }

        private void DelayShowChooseHeroHandlerLoadingUI(object[] args)
        {
            SetDelayInvoker(CHOOSE_HERO_HANDLER, true);

            Action onOutAction = () =>
            {
                Frame.Util.ShowSystemMessage(LanguageManager.Instance.GetLanguageByKey("RequestTimeoutTips"));
            };
            UIAPI.ShowRequestLoading2(SystemConstConfigs.Login_ReqWaitTimeout * 1000, onOutAction, null, LanguageManager.Instance.GetLanguageByKey("RequestLoadingTips_5"), LoadingWidgetTypeEnum.AwaitResponse);
        }

        /// <summary>
        /// 5 选择角色进入游戏回调
        /// </summary>
        private void ChossHeroAckCallBack(ChossHeroAck chossHeroAck, Action<bool> cb)
        {
            Debug.Log($"登录流程 ChossHeroAckCallBack 5选择角色进入游戏回调: {chossHeroAck}");
            SetDelayInvoker(CHOOSE_HERO_HANDLER, false);

            //M_Layer
            GameLoginInfo.M_ChossHeroAck = chossHeroAck;
            if (GameLoginInfo.M_ChossHeroAck != null)
            {
                if (GameLoginInfo.M_ChossHeroAck.Result > 1)
                {
                    // 进入游戏失败
                    Frame.Util.ShowMessageByCode(GameLoginInfo.M_ChossHeroAck.Result);
                    cb?.Invoke(false);
                    SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.LOGIN_FAIL);
                }
                else
                {
                    OnLoginSuccessed(cb);
                }
            }
        }

        private void OnLoginSuccessed(Action<bool> cb)
        {
            //---Save Account Data-------------------
            mainUserData = new UserData();
            mainUserData.playerRoleId = GameLoginInfo.M_ChooseHeroReq.PID;
            mainUserData.accountID = (ulong)GameLoginInfo.M_UserLoginNAck.UID;
            mainUserData.groupID = GameLoginInfo.M_ChooseHeroReq.GroupID;
            mainUserData.groupName = M_GroupInfo?.groupName;

            //post role init 
            //SDKManager.Instance.UpLoadRoleData(roleID: mainUserData.playerRoleId.ToString(), roleName: mainUserData.name, roleLevel: "1", GSSDK.GSRole.RoleType.TYPE_ENTER_GAME, (int)GameLoginInfo.M_PickGroupReq.GroupID);

            string groupName = string.Empty;
            if (M_GroupInfo != null)
            {
                groupName = M_GroupInfo.groupName;
            }
            SDKManager.Instance.ReportSDKEnterGame(
                   ServerId: GameLoginInfo.M_ChooseHeroReq.GroupID.ToString(),
                   ServerName: groupName,
                   RoleId: mainUserData.playerRoleId.ToString(),
                   RoleName: mainUserData.name,
                   RoleLevel: "1",
                   CreateTime: GameLoginInfo.GetCreateTime().ToString()
                   );


            SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.LOGIN_SUC);

            OnLoginSucSaveUsrNApp(mainUserData);
            BattleManager.Instance.InitForbiddeAutoPlaySkills();
            AppConfig.IsFirstLogin = false;
            //---Star World Create-------------------
            //GameManager.Instance.Loading.OnProcessStart();

            //GameManager.Instance.Loading.OnShowText("加载场景");

            //不必等待socket，是后台必做的事情不是流程，保证界面有一个最新界面极速反馈，消息来了再更新
            //SystemConstConfigs.FirstLevel
            //int mapBaseID = LocalDataManager.Instance.GetMapBaseID(SystemConstConfigs.FirstLevel, SpaceType.SpacePlot);
            //MapBaseDataCell mapBaseDataCell = LocalDataManager.Instance.GetMapBaseDataCell(mapBaseID);
            //if (mapBaseDataCell != null)
            //{
            //    StarScenesManager.Instance.SetPreLoadScene(mapBaseDataCell.MapName, LoadSceneType.PreLoadFadeBlack);
            //}
            //else
            //{
            //    GameManager.Instance.Loading.OnPlayBlackDOFade(true, 0.2f);
            //}

            StarScenesManager.Instance.CreateRoleLoadMap = true;
            GameManager.Instance.Loading.OnPlayBlackDOFade(true, 1f, (finish) => { GameManager.Instance.Loading.OnShowSpecialLoading(); });

            //ModuleManager.Instance.CreateModule(ModuleDef.Name.StarWorldModule);         //世界战斗
            //GameManager.Instance.IsLockGameMsgHandle = true;
            //ModuleManager.Instance.ShowModule(ModuleDef.Name.StarWorldModule);           //启动界面
            StarWorldSocketMsg.Start();

            //Login需要高配【关】
            {

                GameConfig.MachineQualityLevel = GameConfig.FirstMachineCheckCache;
                AppMain.Instance.SetQualityLevel(GameConfig.MachineQualityLevel);
            }
            //确认主角信息到了以后再启动所有依赖主角的Module，性能更稳健，但是目前不需要杀掉-重启module
            //对于客户端了来讲，要么杀掉，要么重登：已经启动的没必要杀掉再起来（BusinessModule）。

            Debug.LogWarning($"[{TagFlag}] 登录流程 ChossHeroAckCallBack ShowModule=StarWorldModule");
            cb?.Invoke(true);
            ModuleManager.Instance.ReleaseModule(this);
            Debug.LogWarning($"[{TagFlag}] 登录流程 ChossHeroAckCallBack ReleaseModule=LoginModule");
            //TODO：提醒卸载模块的时候|你就可以关闭挂件，关闭window，处理Page（也可以不处理）
            //---Game Begin !!!-------------------


        }

        #endregion

        #region [6] 删除角色
        public void DeleteHeroAck(UInt64 pid, UInt32 groupId, Action<bool> cb)
        {
            GameLoginInfo.M_DeleteHeroReq = new DeleteHeroReq();
            GameLoginInfo.M_DeleteHeroReq.PID = pid;
            GameLoginInfo.M_DeleteHeroReq.GroupID = groupId;
            GameLoginInfo.M_DeleteHeroReq.UID = (UInt64)GameLoginInfo.M_UserLoginNAck.UID;

            // HttpUtils.CommonHttpServerAfterReq2(GameServerIpAddress, DELETE_HERO_HANDLER, DeleteHeroAckCallBack, GameLoginInfo.M_DeleteHeroReq, cb);

            SendLoginHeaderHttpReq<DeleteHeroReq, DeleteHeroAck, Action<bool>>(DELETE_HERO_HANDLER, GameLoginInfo.M_DeleteHeroReq, DeleteHeroAckCallBack, cb);

            DelayInvoker.DelayInvoke(DELETE_HERO_HANDLER, 1, DelayShowDeleteHeroAckLoadingUI, null);
            SetDelayInvoker(DELETE_HERO_HANDLER, true);
        }

        private void DelayShowDeleteHeroAckLoadingUI(object[] args)
        {
            Action onOutAction = () =>
            {
                Frame.Util.ShowSystemMessage(LanguageManager.Instance.GetLanguageByKey("RequestTimeoutTips"));
            };
            UIAPI.ShowRequestLoading2(SystemConstConfigs.Login_ReqWaitTimeout * 1000, onOutAction, null, LanguageManager.Instance.GetLanguageByKey("RequestLoadingTips_4"), LoadingWidgetTypeEnum.AwaitResponse);
        }

        private void DeleteHeroAckCallBack(DeleteHeroAck deleteHeroAck, Action<bool> cb)
        {
            //M_Layer
            SetDelayInvoker(DELETE_HERO_HANDLER, false);
            GameLoginInfo.M_DeleteHeroAck = deleteHeroAck;
            if (GameLoginInfo.M_DeleteHeroAck != null)
            {
                if (GameLoginInfo.M_DeleteHeroAck.Result > 1)
                {
                    // 删除失败
                    Frame.Util.ShowMessageByCode(GameLoginInfo.M_DeleteHeroAck.Result);
                    cb?.Invoke(false);
                    Debug.Log($"[{TagFlag}] ChossHeroAckCallBack err={GameLoginInfo.M_DeleteHeroAck.ResultMsg}");
                }
                else
                {
                    // 删除成功
                    cb?.Invoke(true);
                }
            }
        }
        #endregion



    }
}
